using System;

namespace YtdOptimizer.Core
{
    public struct TargetSize
    {
        public int Width;
        public int Height;
        public bool NeedsResize;

        public TargetSize(int width, int height, bool needsResize)
        {
            Width = width;
            Height = height;
            NeedsResize = needsResize;
        }

        public override string ToString()
        {
            return $"{Width}x{Height}";
        }
    }

    public static class SizeRules
    {
        public static TargetSize CalculateTargetSize(int srcWidth, int srcHeight, OptimizerConfig config)
        {
            if (srcWidth <= 0 || srcHeight <= 0)
                return new TargetSize(srcWidth, srcHeight, false);

            // 1. Zaten hedef boyutlardan biriyse dokunma:
            // 1024x1024, 1024x512, 512x1024
            if ((srcWidth == 1024 && srcHeight == 1024) ||
                (srcWidth == 1024 && srcHeight == 512) ||
                (srcWidth == 512 && srcHeight == 1024))
            {
                return new TargetSize(srcWidth, srcHeight, false);
            }

            // 2. Kare olup 1024'ten büyükse 1024x1024 yap (ör. 2048x2048, 4096x4096)
            if (srcWidth == srcHeight)
            {
                if (srcWidth > config.MaxSquareDimension)
                {
                    return new TargetSize(config.MaxSquareDimension, config.MaxSquareDimension, true);
                }
                // Hedeften küçükse büyütme
                return new TargetSize(srcWidth, srcHeight, false);
            }

            // 3. 2:1 yatay oranlı olup 1024x512'den büyükse 1024x512 yap (ör. 2048x1024, 4096x2048)
            if (srcWidth == srcHeight * 2)
            {
                if (srcWidth > config.MaxWideWidth || srcHeight > config.MaxWideHeight)
                {
                    return new TargetSize(config.MaxWideWidth, config.MaxWideHeight, true);
                }
                return new TargetSize(srcWidth, srcHeight, false);
            }

            // 4. 1:2 dikey oranlı olup 512x1024'ten büyükse 512x1024 yap (ör. 1024x2048, 2048x4096)
            if (srcHeight == srcWidth * 2)
            {
                if (srcWidth > config.MaxTallWidth || srcHeight > config.MaxTallHeight)
                {
                    return new TargetSize(config.MaxTallWidth, config.MaxTallHeight, true);
                }
                return new TargetSize(srcWidth, srcHeight, false);
            }

            // 5. Diğer oranlarda en uzun kenarı 1024 yap, oranı koru, 2'nin kuvvetine yuvarla
            int maxDim = Math.Max(srcWidth, srcHeight);
            if (maxDim > config.MaxGenericLongEdge)
            {
                double scale = (double)config.MaxGenericLongEdge / maxDim;
                int targetW = RoundToPowerOfTwo((int)Math.Round(srcWidth * scale));
                int targetH = RoundToPowerOfTwo((int)Math.Round(srcHeight * scale));

                // Hedeften küçük olan boyutu orijinalinden büyük yapma
                if (targetW > srcWidth) targetW = srcWidth;
                if (targetH > srcHeight) targetH = srcHeight;

                targetW = Math.Max(4, targetW);
                targetH = Math.Max(4, targetH);

                bool needsChange = (targetW != srcWidth) || (targetH != srcHeight);
                return new TargetSize(targetW, targetH, needsChange);
            }

            // Hedeften küçük dokuları büyütme
            return new TargetSize(srcWidth, srcHeight, false);
        }

        public static int RoundToPowerOfTwo(int value)
        {
            if (value <= 4) return 4;
            // Nearest power of two
            int p = 1;
            while (p < value)
            {
                p <<= 1;
            }
            int prev = p >> 1;
            if (prev >= 4 && (value - prev) < (p - value))
            {
                return prev;
            }
            return p;
        }

        public static bool IsPowerOfTwo(int x)
        {
            return (x > 0) && ((x & (x - 1)) == 0);
        }
    }
}
