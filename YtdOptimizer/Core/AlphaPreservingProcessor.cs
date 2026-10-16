using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace YtdOptimizer.Core
{
    public class AlphaCheckResult
    {
        public bool HasAlpha { get; set; }
        public int TotalPixels { get; set; }
        public int FullyTransparentCount { get; set; }
        public int SemiTransparentCount { get; set; }
        public double TransparentRatio { get; set; } // 0.0 - 1.0
        public byte[] AlphaChannel { get; set; }
    }

    public class AlphaValidationResult
    {
        public bool Passed { get; set; }
        public double AverageAlphaDifferencePercent { get; set; }
        public double TransparentRatioDeltaPercent { get; set; }
        public bool SourceHadTransparency { get; set; }
        public bool TargetHasTransparency { get; set; }
        public string FailureReason { get; set; } = string.Empty;
    }

    public static class AlphaPreservingProcessor
    {
        /// <summary>
        /// Analyzes pixel alpha content of an ImageSharp Rgba32 image.
        /// </summary>
        public static AlphaCheckResult CheckAlpha(Image<Rgba32> image)
        {
            var res = new AlphaCheckResult();
            res.TotalPixels = image.Width * image.Height;
            res.AlphaChannel = new byte[res.TotalPixels];

            int idx = 0;
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        byte a = row[x].A;
                        res.AlphaChannel[idx++] = a;
                        if (a == 0)
                        {
                            res.FullyTransparentCount++;
                        }
                        else if (a < 255)
                        {
                            res.SemiTransparentCount++;
                        }
                    }
                }
            });

            // 255'ten farklı değer içeriyorsa ALFA
            res.HasAlpha = (res.FullyTransparentCount > 0 || res.SemiTransparentCount > 0);
            res.TransparentRatio = (double)res.FullyTransparentCount / res.TotalPixels;
            return res;
        }

        /// <summary>
        /// Resizes an image strictly adhering to the transparency preservation steps:
        /// 1. Premultiply RGB by Alpha
        /// 2. Lanczos3 / Bicubic High Quality Resampling (RGB & Alpha together)
        /// 3. Un-premultiply RGB by Alpha
        /// 4. Alpha Dilation (fill RGB of alpha=0 pixels with nearest visible color to prevent color fringes)
        /// 5. Ensure valid Alpha channel throughout
        /// </summary>
        public static Image<Rgba32> ResizeAlphaPreserving(Image<Rgba32> sourceImage, int targetWidth, int targetHeight)
        {
            // Clone to avoid mutating original
            var img = sourceImage.Clone();

            // Step 2a: Premultiply RGB with Alpha
            img.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        byte a = row[x].A;
                        if (a < 255)
                        {
                            float alphaNorm = a / 255.0f;
                            row[x].R = (byte)Math.Round(row[x].R * alphaNorm);
                            row[x].G = (byte)Math.Round(row[x].G * alphaNorm);
                            row[x].B = (byte)Math.Round(row[x].B * alphaNorm);
                        }
                    }
                }
            });

            // Step 2b: High Quality Resize with Lanczos3 Resampler
            img.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(targetWidth, targetHeight),
                Sampler = KnownResamplers.Lanczos3,
                Compand = true
            }));

            // Step 2c: Un-premultiply RGB with Alpha
            img.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        byte a = row[x].A;
                        if (a > 0 && a < 255)
                        {
                            float alphaNorm = a / 255.0f;
                            int r = (int)Math.Round(row[x].R / alphaNorm);
                            int g = (int)Math.Round(row[x].G / alphaNorm);
                            int b = (int)Math.Round(row[x].B / alphaNorm);

                            row[x].R = (byte)Math.Min(255, Math.Max(0, r));
                            row[x].G = (byte)Math.Min(255, Math.Max(0, g));
                            row[x].B = (byte)Math.Min(255, Math.Max(0, b));
                        }
                    }
                }
            });

            // Step 3: Dilation for alpha == 0 pixels (Edge overflow protection)
            DilateTransparentEdges(img);

            return img;
        }

        /// <summary>
        /// Alpha Dilation (Kenar taşması önlemi):
        /// Fills RGB values of transparent pixels (A == 0) with colors from adjacent visible pixels,
        /// preventing black/white fringing around alpha borders when sampled or mipmapped.
        /// </summary>
        public static void DilateTransparentEdges(Image<Rgba32> image, int maxPasses = 3)
        {
            int w = image.Width;
            int h = image.Height;

            // We do a fast neighborhood propagation for transparent boundary pixels
            for (int pass = 0; pass < maxPasses; pass++)
            {
                bool anyChanged = false;

                // Read current state
                var currentPixels = new Rgba32[w * h];
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < h; y++)
                    {
                        var row = accessor.GetRowSpan(y);
                        for (int x = 0; x < w; x++)
                        {
                            currentPixels[y * w + x] = row[x];
                        }
                    }
                });

                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < h; y++)
                    {
                        var row = accessor.GetRowSpan(y);
                        for (int x = 0; x < w; x++)
                        {
                            if (currentPixels[y * w + x].A == 0)
                            {
                                int rSum = 0, gSum = 0, bSum = 0, count = 0;

                                // Check 8 neighbors
                                for (int dy = -1; dy <= 1; dy++)
                                {
                                    int ny = y + dy;
                                    if (ny < 0 || ny >= h) continue;

                                    for (int dx = -1; dx <= 1; dx++)
                                    {
                                        if (dx == 0 && dy == 0) continue;
                                        int nx = x + dx;
                                        if (nx < 0 || nx >= w) continue;

                                        var neighbor = currentPixels[ny * w + nx];
                                        if (neighbor.A > 0)
                                        {
                                            rSum += neighbor.R;
                                            gSum += neighbor.G;
                                            bSum += neighbor.B;
                                            count++;
                                        }
                                    }
                                }

                                if (count > 0)
                                {
                                    row[x].R = (byte)(rSum / count);
                                    row[x].G = (byte)(gSum / count);
                                    row[x].B = (byte)(bSum / count);
                                    // A remains 0
                                    anyChanged = true;
                                }
                            }
                        }
                    }
                });

                if (!anyChanged) break;
            }
        }

        /// <summary>
        /// Validates alpha preservation between source image and resulting image:
        /// - Resizes source alpha down to target dimensions and compares average difference (max 2% threshold)
        /// - Checks transparent pixel ratio delta (max 3% threshold)
        /// - If source had transparent pixels, target MUST also have transparent pixels.
        /// </summary>
        public static AlphaValidationResult ValidateAlpha(
            Image<Rgba32> sourceImage,
            Image<Rgba32> targetImage,
            double maxAlphaDiffPercent = 2.0,
            double maxRatioDeltaPercent = 3.0)
        {
            var result = new AlphaValidationResult();

            var srcAlphaCheck = CheckAlpha(sourceImage);
            var tgtAlphaCheck = CheckAlpha(targetImage);

            result.SourceHadTransparency = srcAlphaCheck.HasAlpha;
            result.TargetHasTransparency = tgtAlphaCheck.HasAlpha;

            // 1. Source had transparency but target lost it entirely
            if (srcAlphaCheck.FullyTransparentCount > 0 && tgtAlphaCheck.FullyTransparentCount == 0 && tgtAlphaCheck.SemiTransparentCount == 0)
            {
                result.Passed = false;
                result.FailureReason = "Kaynakta şeffaf piksel vardı ancak hedefte tamamen kayboldu.";
                return result;
            }

            // 2. Compare transparent area ratio
            double srcRatio = srcAlphaCheck.TransparentRatio;
            double tgtRatio = tgtAlphaCheck.TransparentRatio;
            double ratioDeltaPercent = Math.Abs(srcRatio - tgtRatio) * 100.0;
            result.TransparentRatioDeltaPercent = ratioDeltaPercent;

            if (ratioDeltaPercent > maxRatioDeltaPercent && (srcAlphaCheck.FullyTransparentCount > 10 || tgtAlphaCheck.FullyTransparentCount > 10))
            {
                result.Passed = false;
                result.FailureReason = $"Tam şeffaf alan oranı %{ratioDeltaPercent:F2} değişti (Eşik: %{maxRatioDeltaPercent:F1}).";
                return result;
            }

            // 3. Create resized source alpha to target size for direct pixel comparison
            using (var srcAlphaImg = new Image<L8>(sourceImage.Width, sourceImage.Height))
            {
                int idx = 0;
                srcAlphaImg.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        var row = accessor.GetRowSpan(y);
                        for (int x = 0; x < row.Length; x++)
                        {
                            row[x] = new L8(srcAlphaCheck.AlphaChannel[idx++]);
                        }
                    }
                });

                // Resize source alpha to target dimensions
                srcAlphaImg.Mutate(ctx => ctx.Resize(new ResizeOptions
                {
                    Size = new Size(targetImage.Width, targetImage.Height),
                    Sampler = KnownResamplers.Bicubic
                }));

                long totalDiff = 0;
                int totalTargetPixels = targetImage.Width * targetImage.Height;

                int tgtIdx = 0;
                srcAlphaImg.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        var row = accessor.GetRowSpan(y);
                        for (int x = 0; x < row.Length; x++)
                        {
                            byte expectedA = row[x].PackedValue;
                            byte actualA = tgtAlphaCheck.AlphaChannel[tgtIdx++];
                            totalDiff += Math.Abs(expectedA - actualA);
                        }
                    }
                });

                double avgDiffPercent = ((double)totalDiff / (totalTargetPixels * 255.0)) * 100.0;
                result.AverageAlphaDifferencePercent = avgDiffPercent;

                if (avgDiffPercent > maxAlphaDiffPercent)
                {
                    result.Passed = false;
                    result.FailureReason = $"Ortalama alfa farkı %{avgDiffPercent:F2} (Eşik: %{maxAlphaDiffPercent:F1}). Şeffaflık bozulmuş olabilir.";
                    return result;
                }
            }

            result.Passed = true;
            return result;
        }
    }
}
