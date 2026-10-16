using System;

namespace YtdOptimizer.Core
{
    public class OptimizerConfig
    {
        public string Gta5Folder { get; set; } = string.Empty;
        public string CodeWalkerFolder { get; set; } = string.Empty;
        public string TexconvPath { get; set; } = "texconv.exe";

        public int MaxSquareDimension { get; set; } = 1024;
        public int MaxWideWidth { get; set; } = 1024;
        public int MaxWideHeight { get; set; } = 512;
        public int MaxTallWidth { get; set; } = 512;
        public int MaxTallHeight { get; set; } = 1024;
        public int MaxGenericLongEdge { get; set; } = 1024;

        public bool BackupOriginal { get; set; } = true;
        public bool SkipNormalMaps { get; set; } = true;
        public bool SkipSpecularMaps { get; set; } = true;
        public bool ConvertFormatIfNotDxt5 { get; set; } = true;
        public int ParallelDegree { get; set; } = 4;

        public double AlphaDiffThresholdPercent { get; set; } = 2.0;
        public double TransparentRatioThresholdPercent { get; set; } = 3.0;

        public string Language { get; set; } = "tr"; // "tr", "en", "de"

        public static OptimizerConfig Default()
        {
            return new OptimizerConfig();
        }
    }
}
