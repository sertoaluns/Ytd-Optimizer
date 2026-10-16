using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace YtdOptimizer.Core
{
    public class TextureItem
    {
        public string Name { get; set; } = string.Empty;
        public string FormatName { get; set; } = string.Empty;
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }
        public int TargetWidth { get; set; }
        public int TargetHeight { get; set; }
        public long EstimatedOriginalBytes { get; set; }
        public long EstimatedTargetBytes { get; set; }
        public bool HasAlpha { get; set; }
        public bool NeedsResize { get; set; }
        public bool NeedsFormatConversion { get; set; }
        public bool NeedsOptimization => NeedsResize || NeedsFormatConversion;
        public string Status { get; set; } = "Bekliyor"; // Bekliyor, Optimize Edildi, Atlandı, Hata
        public string DetailMessage { get; set; } = string.Empty;

        public bool IsNormalMap => Name.EndsWith("_n", StringComparison.OrdinalIgnoreCase) || Name.Contains("_normal");
        public bool IsSpecularMap => Name.EndsWith("_s", StringComparison.OrdinalIgnoreCase) || Name.Contains("_spec");

        public string SizeDisplay => $"{OriginalWidth}x{OriginalHeight}";
        public string TargetSizeDisplay => $"{TargetWidth}x{TargetHeight}";
        public string FileSizeMbDisplay => $"{(EstimatedOriginalBytes / 1024.0 / 1024.0):F2} MB";
        public string AlphaTag => HasAlpha ? Loc.T("TagAlpha") : Loc.T("TagOpaque");
        public string StatusDisplay
        {
            get
            {
                if (string.Equals(Status, "Optimize Edildi", StringComparison.OrdinalIgnoreCase) || string.Equals(Status, "Optimized", StringComparison.OrdinalIgnoreCase) || string.Equals(Status, "Optimiert", StringComparison.OrdinalIgnoreCase))
                    return Loc.T("StatusOptimized");
                if (string.Equals(Status, "Atlandı", StringComparison.OrdinalIgnoreCase) || string.Equals(Status, "Skipped", StringComparison.OrdinalIgnoreCase) || string.Equals(Status, "Übersprungen", StringComparison.OrdinalIgnoreCase))
                    return Loc.T("StatusSkipped");
                if (string.Equals(Status, "Hata", StringComparison.OrdinalIgnoreCase) || string.Equals(Status, "Error", StringComparison.OrdinalIgnoreCase) || string.Equals(Status, "Fehler", StringComparison.OrdinalIgnoreCase))
                    return Loc.T("StatusError");
                return Loc.T("StatusPending");
            }
        }
    }

    public class DrawableItem
    {
        public string Name { get; set; } = string.Empty;
        public string YddFilePath { get; set; } = string.Empty;
        public string FolderPath => !string.IsNullOrEmpty(YddFilePath) ? Path.GetDirectoryName(YddFilePath) : string.Empty;
        public string FolderName => !string.IsNullOrEmpty(FolderPath) ? Path.GetFileName(FolderPath) : string.Empty;
        public bool HasYdd => !string.IsNullOrEmpty(YddFilePath) && File.Exists(YddFilePath);

        public List<YtdFileItem> MatchedYtds { get; set; } = new List<YtdFileItem>();
    }

    public class YtdFileItem
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName => Path.GetFileName(FilePath);
        public string FolderPath => !string.IsNullOrEmpty(FilePath) ? Path.GetDirectoryName(FilePath) : string.Empty;
        public string FolderName => !string.IsNullOrEmpty(FolderPath) ? Path.GetFileName(FolderPath) : string.Empty;
        public long FileSizeBytes { get; set; }
        public string MatchedDrawableName { get; set; } = string.Empty;
        public bool IsMatched => !string.IsNullOrEmpty(MatchedDrawableName) && MatchedDrawableName != "(Eşleşmedi)" && MatchedDrawableName != "(Unmatched)" && MatchedDrawableName != "(Nicht zugeordnet)";

        public List<TextureItem> Textures { get; set; } = new List<TextureItem>();

        public int TextureCount => Textures.Count;
        public int TexturesToOptimizeCount
        {
            get
            {
                int count = 0;
                foreach (var t in Textures)
                {
                    if (t.NeedsOptimization) count++;
                }
                return count;
            }
        }

        public long TotalTextureBytes
        {
            get
            {
                long total = 0;
                foreach (var t in Textures) total += t.EstimatedOriginalBytes;
                return total > 0 ? total : FileSizeBytes;
            }
        }
    }

    public static class FileMatcher
    {
        // GTA V clothing naming conventions:
        // accs_001_u -> accs_diff_001_a_uni.ytd, accs_diff_001_b_uni.ytd
        // jbib_005_u -> jbib_diff_005_a_uni.ytd
        // lowr_012_u -> lowr_diff_012_a_uni.ytd
        // feet_001_u -> feet_diff_001_a_uni.ytd
        // uppr_002_u -> uppr_diff_002_a_uni.ytd
        // task_003_u -> task_diff_003_a_uni.ytd
        // berd_001_u -> berd_diff_001_a_uni.ytd
        // hair_004_u -> hair_diff_004_a_uni.ytd
        // head_001_r -> head_diff_001_a_whi.ytd
        public static string ExtractKey(string fileNameWithoutExt)
        {
            if (string.IsNullOrEmpty(fileNameWithoutExt))
                return string.Empty;

            string s = fileNameWithoutExt.ToLowerInvariant();

            // Match pattern like (category)_(optional: diff/uni/)_(number)
            // Example: accs_001_u -> category: accs, num: 001
            // Example: accs_diff_001_a_uni -> category: accs, num: 001
            var match = Regex.Match(s, @"^([a-z]+)_(?:(?:diff|uni|normal|spec)_)?([0-9]+)");
            if (match.Success)
            {
                string category = match.Groups[1].Value;
                string num = match.Groups[2].Value;
                return $"{category}_{num}";
            }

            // Fallback: match any letters followed by underscore and numbers
            var match2 = Regex.Match(s, @"([a-z]+)_([0-9]+)");
            if (match2.Success)
            {
                return $"{match2.Groups[1].Value}_{match2.Groups[2].Value}";
            }

            // If no match, return original normalized name
            return s;
        }

        public static bool IsMatch(string drawableName, string ytdName)
        {
            string key1 = ExtractKey(drawableName);
            string key2 = ExtractKey(ytdName);

            if (!string.IsNullOrEmpty(key1) && !string.IsNullOrEmpty(key2) && key1 == key2)
            {
                return true;
            }

            return false;
        }
    }
}
