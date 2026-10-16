using System;
using System.Collections.Generic;

namespace YtdOptimizer.Core
{
    public class OptimizationStats
    {
        public int TotalYtdCount { get; set; }
        public int ProcessedYtdCount { get; set; }
        public int FailedYtdCount { get; set; }

        public int TotalTextureCount { get; set; }
        public int OptimizedTextureCount { get; set; }
        public int SkippedTextureCount { get; set; }
        public int FailedTextureCount { get; set; }

        public int AlphaTextureCount { get; set; }
        public int OpaqueTextureCount { get; set; }

        public int AlphaValidationPassedCount { get; set; }
        public int AlphaValidationFailedCount { get; set; }

        public long InitialSizeBytes { get; set; }
        public long FinalSizeBytes { get; set; }
        public long SavedBytes => Math.Max(0, InitialSizeBytes - FinalSizeBytes);

        public double SavedMb => SavedBytes / 1024.0 / 1024.0;
        public double InitialMb => InitialSizeBytes / 1024.0 / 1024.0;
        public double FinalMb => FinalSizeBytes / 1024.0 / 1024.0;

        public List<string> ErrorLogs { get; set; } = new List<string>();
        public List<string> WarningLogs { get; set; } = new List<string>();
    }

    public class ProgressUpdateArgs
    {
        public int CurrentItem { get; set; }
        public int TotalItems { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public string LogMessage { get; set; } = string.Empty;
        public bool IsError { get; set; }
        public bool IsWarning { get; set; }
    }
}
