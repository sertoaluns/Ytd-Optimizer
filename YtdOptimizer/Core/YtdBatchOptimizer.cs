using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CodeWalker.GameFiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace YtdOptimizer.Core
{
    public class YtdBatchOptimizer
    {
        private readonly OptimizerConfig _config;

        public event Action<ProgressUpdateArgs> OnProgress;

        static YtdBatchOptimizer()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                try
                {
                    var reqName = new System.Reflection.AssemblyName(args.Name).Name;
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string dllPath = Path.Combine(baseDir, reqName + ".dll");
                    if (File.Exists(dllPath))
                    {
                        return System.Reflection.Assembly.LoadFrom(dllPath);
                    }

                    // Special case: if 4.0.4.1 is requested for Unsafe, return loaded Unsafe 5.0.0.0
                    if (reqName.Equals("System.Runtime.CompilerServices.Unsafe", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (asm.GetName().Name.Equals("System.Runtime.CompilerServices.Unsafe", StringComparison.OrdinalIgnoreCase))
                            {
                                return asm;
                            }
                        }
                    }
                }
                catch { }
                return null;
            };
        }

        public YtdBatchOptimizer(OptimizerConfig config)
        {
            _config = config ?? OptimizerConfig.Default();
        }

        private void ReportProgress(int current, int total, string status, string log = "", bool isError = false, bool isWarning = false)
        {
            OnProgress?.Invoke(new ProgressUpdateArgs
            {
                CurrentItem = current,
                TotalItems = total,
                CurrentStatus = status,
                LogMessage = log,
                IsError = isError,
                IsWarning = isWarning
            });
        }

        public async Task<OptimizationStats> OptimizeYtdFilesAsync(IList<YtdFileItem> ytdItems, CancellationToken cancellationToken)
        {
            var stats = new OptimizationStats
            {
                TotalYtdCount = ytdItems.Count
            };

            foreach (var item in ytdItems)
            {
                stats.InitialSizeBytes += item.FileSizeBytes;
            }

            int processedCount = 0;
            var syncLock = new object();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(1, _config.ParallelDegree),
                CancellationToken = cancellationToken
            };

            await Task.Run(() =>
            {
                Parallel.ForEach(ytdItems, parallelOptions, ytdItem =>
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    string ytdName = ytdItem.FileName;
                    string tempBaseDir = Path.Combine(Path.GetTempPath(), "YtdOptimizer", Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(tempBaseDir);

                    try
                    {
                        var fileStats = OptimizeSingleYtd(ytdItem.FilePath, tempBaseDir);

                        lock (syncLock)
                        {
                            processedCount++;
                            stats.ProcessedYtdCount++;
                            stats.TotalTextureCount += fileStats.TotalTextureCount;
                            stats.OptimizedTextureCount += fileStats.OptimizedTextureCount;
                            stats.SkippedTextureCount += fileStats.SkippedTextureCount;
                            stats.FailedTextureCount += fileStats.FailedTextureCount;
                            stats.AlphaTextureCount += fileStats.AlphaTextureCount;
                            stats.OpaqueTextureCount += fileStats.OpaqueTextureCount;
                            stats.AlphaValidationPassedCount += fileStats.AlphaValidationPassedCount;
                            stats.AlphaValidationFailedCount += fileStats.AlphaValidationFailedCount;

                            if (File.Exists(ytdItem.FilePath))
                            {
                                stats.FinalSizeBytes += new FileInfo(ytdItem.FilePath).Length;
                            }

                            ReportProgress(
                                processedCount,
                                stats.TotalYtdCount,
                                $"İşleniyor ({processedCount}/{stats.TotalYtdCount}): {ytdName}",
                                $"[BAŞARILI] {ytdName} optimize edildi. (Değişen: {fileStats.OptimizedTextureCount}, Atlanan: {fileStats.SkippedTextureCount})");
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (syncLock)
                        {
                            processedCount++;
                            stats.FailedYtdCount++;
                            stats.ErrorLogs.Add($"{ytdName}: {ex.Message}");

                            if (File.Exists(ytdItem.FilePath))
                            {
                                stats.FinalSizeBytes += new FileInfo(ytdItem.FilePath).Length;
                            }

                            ReportProgress(
                                processedCount,
                                stats.TotalYtdCount,
                                $"Hata: {ytdName}",
                                $"[HATA] {ytdName} işlenirken hata oluştu: {ex.Message}",
                                isError: true);
                        }
                    }
                    finally
                    {
                        try
                        {
                            if (Directory.Exists(tempBaseDir))
                            {
                                Directory.Delete(tempBaseDir, true);
                            }
                        }
                        catch { }
                    }
                });
            }, cancellationToken);

            return stats;
        }

        public OptimizationStats OptimizeSingleYtd(string ytdFilePath, string tempDir)
        {
            var stats = new OptimizationStats();

            if (!File.Exists(ytdFilePath))
                throw new FileNotFoundException("YTD dosyası bulunamadı: " + ytdFilePath);

            // 1. YtdFile ile YTD'yi yükle
            YtdFile ytdFile = CodeWalkerInterop.LoadYtd(ytdFilePath);
            if (ytdFile?.TextureDict?.Textures == null)
                throw new InvalidOperationException("YTD içindeki doku sözlüğü okunamadı.");

            var textures = ytdFile.TextureDict.Textures;
            stats.TotalTextureCount = textures.Count;

            var newTextureList = new List<Texture>();
            bool anyModified = false;

            for (int i = 0; i < textures.Count; i++)
            {
                var tex = textures[i];
                string texName = tex.Name;
                int srcW = tex.Width;
                int srcH = tex.Height;
                var currentFormat = tex.Format;

                // Check normal / specular skip
                bool isNormal = texName.EndsWith("_n", StringComparison.OrdinalIgnoreCase) || texName.Contains("_normal");
                bool isSpec = texName.EndsWith("_s", StringComparison.OrdinalIgnoreCase) || texName.Contains("_spec");

                if ((_config.SkipNormalMaps && isNormal) || (_config.SkipSpecularMaps && isSpec))
                {
                    stats.SkippedTextureCount++;
                    newTextureList.Add(tex);
                    continue;
                }

                // Check size rule
                var targetSize = SizeRules.CalculateTargetSize(srcW, srcH, _config);
                bool formatIsDxt5 = (currentFormat == TextureFormat.D3DFMT_DXT5);
                bool needsFormatChange = !formatIsDxt5 && _config.ConvertFormatIfNotDxt5;

                // Boyutu zaten uygun ve formatı zaten DXT5 olan doku "atlandı" olarak işaretlenir ve hiç değiştirilmez.
                if (!targetSize.NeedsResize && !needsFormatChange)
                {
                    stats.SkippedTextureCount++;
                    newTextureList.Add(tex);
                    continue;
                }

                // Sub-directory for this texture
                string texSubDir = Path.Combine(tempDir, "tex_" + i);
                Directory.CreateDirectory(texSubDir);

                try
                {
                    // 2. Dokuyu DDS olarak geçici klasöre çıkar ve piksel verisine (RGBA) çevir
                    using (Image<Rgba32> srcImage = CodeWalkerInterop.ExtractTextureToImage(tex, _config.TexconvPath, texSubDir))
                    {
                        var alphaInfo = AlphaPreservingProcessor.CheckAlpha(srcImage);
                        if (alphaInfo.HasAlpha)
                        {
                            stats.AlphaTextureCount++;
                        }
                        else
                        {
                            stats.OpaqueTextureCount++;
                        }

                        // 3. Boyut kuralına göre gerekiyorsa alfa korumalı küçültmeyi uygula
                        Image<Rgba32> processedImage;
                        if (targetSize.NeedsResize)
                        {
                            processedImage = AlphaPreservingProcessor.ResizeAlphaPreserving(srcImage, targetSize.Width, targetSize.Height);
                        }
                        else
                        {
                            // Sadece format değişecekse pikseller aynı kalır, şeffaflık/dilation korunur
                            processedImage = srcImage.Clone();
                            AlphaPreservingProcessor.DilateTransparentEdges(processedImage, 2);
                        }

                        using (processedImage)
                        {
                            // PNG olarak geçici diske kaydet (texconv'un okuması için)
                            string intermediatePng = Path.Combine(texSubDir, "processed.png");
                            processedImage.SaveAsPng(intermediatePng);

                            // 4. texconv ile DXT5'e sıkıştır (-f BC3_UNORM -m 0 -y)
                            var texconvRes = TexconvRunner.ConvertToDxt5(_config.TexconvPath, intermediatePng, texSubDir);
                            if (!texconvRes.Success || !File.Exists(texconvRes.OutputFilePath))
                            {
                                throw new InvalidOperationException($"texconv sıkıştırma başarısız: {texconvRes.ErrorLog}");
                            }

                            // 5. Alfa doğrulamasını yap (ŞEFFAFLIK KORUMA 6)
                            // Çıkan DDS'i kontrol için aç
                            string ddsResultPath = texconvRes.OutputFilePath;
                            var decompRes = TexconvRunner.DecompressToPng(_config.TexconvPath, ddsResultPath, texSubDir);
                            if (!decompRes.Success || !File.Exists(decompRes.OutputFilePath))
                            {
                                throw new InvalidOperationException("Sonuç DDS doğrulaması için decompress edilemedi.");
                            }

                            using (var resultImg = Image.Load<Rgba32>(decompRes.OutputFilePath))
                            {
                                var valResult = AlphaPreservingProcessor.ValidateAlpha(
                                    srcImage,
                                    resultImg,
                                    _config.AlphaDiffThresholdPercent,
                                    _config.TransparentRatioThresholdPercent);

                                if (!valResult.Passed)
                                {
                                    stats.AlphaValidationFailedCount++;
                                    stats.SkippedTextureCount++;
                                    stats.WarningLogs.Add($"{Path.GetFileName(ytdFilePath)} -> {texName}: {valResult.FailureReason} (Orijinal doku korundu)");
                                    // Değiştirme, orijinal dokuyu koru!
                                    newTextureList.Add(tex);
                                    continue;
                                }

                                stats.AlphaValidationPassedCount++;
                            }

                            // 6. Çıkan DDS'i DDSIO ile Texture nesnesine çevirip YTD'deki dokunun yerine koy
                            byte[] newDdsBytes = File.ReadAllBytes(ddsResultPath);
                            Texture newTex = CodeWalkerInterop.CreateTextureFromDds(newDdsBytes, texName);
                            if (newTex == null)
                            {
                                throw new InvalidOperationException("Yeni Texture nesnesi oluşturulamadı.");
                            }

                            newTextureList.Add(newTex);
                            anyModified = true;
                            stats.OptimizedTextureCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    stats.FailedTextureCount++;
                    stats.ErrorLogs.Add($"{Path.GetFileName(ytdFilePath)} -> {texName}: {ex.Message}");
                    // Hata durumunda orijinal dokuyu koru!
                    newTextureList.Add(tex);
                }
            }

            if (anyModified)
            {
                // YTD sözlüğünü güncelle
                ytdFile.TextureDict.BuildFromTextureList(newTextureList);

                // 7. YTD'yi kaydet (backup oluşturarak)
                CodeWalkerInterop.SaveYtd(ytdFile, ytdFilePath, _config.BackupOriginal);

                // 8. Kayıt sonrası YTD'yi tekrar yükleyip açılabildiğini ve dokuların okunabildiğini doğrula
                try
                {
                    YtdFile verifyYtd = CodeWalkerInterop.LoadYtd(ytdFilePath);
                    if (verifyYtd?.TextureDict?.Textures == null || verifyYtd.TextureDict.Textures.Count != newTextureList.Count)
                    {
                        throw new InvalidOperationException("Doğrulama başarısız: Doku sayısı veya sözlük yapısı tutarsız.");
                    }
                }
                catch (Exception verifyEx)
                {
                    // Doğrulanamazsa .bak varsa geri yükle
                    string bakPath = ytdFilePath + ".bak";
                    if (File.Exists(bakPath))
                    {
                        File.Copy(bakPath, ytdFilePath, true);
                    }
                    throw new InvalidOperationException($"Kaydedilen YTD doğrulanamadı ve .bak yedeğinden geri yüklendi: {verifyEx.Message}");
                }
            }
            else
            {
                stats.SkippedTextureCount = stats.TotalTextureCount;
            }

            return stats;
        }

        /// <summary>
        /// Analyzes a single YTD file to populate texture information without modifying it.
        /// </summary>
        public static YtdFileItem AnalyzeYtd(string ytdFilePath, OptimizerConfig config)
        {
            var item = new YtdFileItem
            {
                FilePath = ytdFilePath,
                FileSizeBytes = File.Exists(ytdFilePath) ? new FileInfo(ytdFilePath).Length : 0
            };

            try
            {
                var ytd = CodeWalkerInterop.LoadYtd(ytdFilePath);
                if (ytd?.TextureDict?.Textures != null)
                {
                    int count = ytd.TextureDict.Textures.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var tex = ytd.TextureDict.Textures[i];
                        if (tex == null) continue;

                        var tItem = new TextureItem
                        {
                            Name = tex.Name,
                            FormatName = tex.Format.ToString(),
                            OriginalWidth = tex.Width,
                            OriginalHeight = tex.Height,
                            EstimatedOriginalBytes = tex.MemoryUsage > 0 ? tex.MemoryUsage : (tex.Width * tex.Height)
                        };

                        var targetSize = SizeRules.CalculateTargetSize(tex.Width, tex.Height, config);
                        tItem.TargetWidth = targetSize.Width;
                        tItem.TargetHeight = targetSize.Height;
                        tItem.NeedsResize = targetSize.NeedsResize;

                        bool isDxt5 = (tex.Format == TextureFormat.D3DFMT_DXT5);
                        tItem.NeedsFormatConversion = !isDxt5 && config.ConvertFormatIfNotDxt5;

                        // Check alpha support in format or name
                        bool fmtHasAlpha = (tex.Format == TextureFormat.D3DFMT_DXT5 ||
                                            tex.Format == TextureFormat.D3DFMT_DXT3 ||
                                            tex.Format == TextureFormat.D3DFMT_A8R8G8B8 ||
                                            tex.Format == TextureFormat.D3DFMT_BC7);

                        tItem.HasAlpha = fmtHasAlpha; // Will be precisely scanned during processing/preview

                        // Target estimate (BC3 DXT5 is 1 byte per pixel: 16 bytes per 4x4 block)
                        tItem.EstimatedTargetBytes = (long)(tItem.TargetWidth * tItem.TargetHeight * 1.33); // with mips

                        if (!tItem.NeedsResize && !tItem.NeedsFormatConversion)
                        {
                            tItem.Status = "Uygun";
                        }
                        else
                        {
                            tItem.Status = "Küçültülecek";
                        }

                        item.Textures.Add(tItem);
                    }
                }
            }
            catch (Exception ex)
            {
                // Unreadable or corrupted
                item.MatchedDrawableName = "(Hata: " + ex.Message + ")";
            }

            return item;
        }
    }
}
