using System;
using System.Diagnostics;
using System.IO;

namespace YtdOptimizer.Core
{
    public class TexconvResult
    {
        public bool Success { get; set; }
        public string OutputFilePath { get; set; } = string.Empty;
        public string OutputLog { get; set; } = string.Empty;
        public string ErrorLog { get; set; } = string.Empty;
    }

    public static class TexconvRunner
    {
        /// <summary>
        /// Converts an input PNG/DDS file to DDS BC3_UNORM (DXT5) with full mipmaps (-m 0).
        /// Note: No resizing or alpha conversions are done by texconv, as instructed.
        /// </summary>
        public static TexconvResult ConvertToDxt5(string texconvPath, string inputImagePath, string outputDir)
        {
            var res = new TexconvResult();

            if (!File.Exists(texconvPath))
            {
                res.Success = false;
                res.ErrorLog = $"texconv.exe bulunamadı: {texconvPath}";
                return res;
            }

            if (!File.Exists(inputImagePath))
            {
                res.Success = false;
                res.ErrorLog = $"Girdi dosyası bulunamadı: {inputImagePath}";
                return res;
            }

            Directory.CreateDirectory(outputDir);

            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(inputImagePath);
            string expectedOutputFile = Path.Combine(outputDir, fileNameWithoutExt + ".dds");

            // Arguments: -f BC3_UNORM -m 0 -y -o "<outputDir>" "<inputImagePath>"
            string args = $"-f BC3_UNORM -m 0 -y -o \"{outputDir}\" \"{inputImagePath}\"";

            var psi = new ProcessStartInfo
            {
                FileName = texconvPath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                using (var proc = Process.Start(psi))
                {
                    if (proc == null)
                    {
                        res.Success = false;
                        res.ErrorLog = "texconv işlemi başlatılamadı.";
                        return res;
                    }

                    string stdout = proc.StandardOutput.ReadToEnd();
                    string stderr = proc.StandardError.ReadToEnd();
                    proc.WaitForExit(30000); // 30 sec timeout

                    res.OutputLog = stdout;
                    res.ErrorLog = stderr;

                    if (proc.ExitCode == 0 && File.Exists(expectedOutputFile))
                    {
                        res.Success = true;
                        res.OutputFilePath = expectedOutputFile;
                    }
                    else
                    {
                        res.Success = false;
                        res.ErrorLog = $"texconv çıkış kodu {proc.ExitCode}. {stderr}\n{stdout}";
                    }
                }
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.ErrorLog = $"texconv çalıştırma hatası: {ex.Message}";
            }

            return res;
        }

        /// <summary>
        /// Converts an input DDS to 32-bit uncompressed PNG or RGBA DDS for raw analysis if needed.
        /// texconv -ft png -f R8G8B8A8_UNORM -y -o "<outputDir>" "<inputDdsPath>"
        /// </summary>
        public static TexconvResult DecompressToPng(string texconvPath, string inputDdsPath, string outputDir)
        {
            var res = new TexconvResult();

            if (!File.Exists(texconvPath) || !File.Exists(inputDdsPath))
            {
                res.Success = false;
                res.ErrorLog = "texconv veya girdi dosyası yok.";
                return res;
            }

            Directory.CreateDirectory(outputDir);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(inputDdsPath);
            string expectedOutputFile = Path.Combine(outputDir, fileNameWithoutExt + ".png");

            string args = $"-ft png -f R8G8B8A8_UNORM -y -o \"{outputDir}\" \"{inputDdsPath}\"";

            var psi = new ProcessStartInfo
            {
                FileName = texconvPath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                using (var proc = Process.Start(psi))
                {
                    if (proc == null) return res;
                    string stdout = proc.StandardOutput.ReadToEnd();
                    string stderr = proc.StandardError.ReadToEnd();
                    proc.WaitForExit(30000);

                    if (proc.ExitCode == 0 && File.Exists(expectedOutputFile))
                    {
                        res.Success = true;
                        res.OutputFilePath = expectedOutputFile;
                    }
                    else
                    {
                        res.Success = false;
                        res.ErrorLog = $"texconv çıkış kodu {proc.ExitCode}. {stderr}\n{stdout}";
                    }
                }
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.ErrorLog = ex.Message;
            }

            return res;
        }
    }
}
