using System;
using System.IO;
using Microsoft.Win32;

namespace YtdOptimizer.Core
{
    public class EnvironmentCheckResult
    {
        public bool Gta5Installed { get; set; }
        public string Gta5Path { get; set; } = string.Empty;

        public bool CodeWalkerValid { get; set; }
        public string CodeWalkerPath { get; set; } = string.Empty;

        public bool TexconvValid { get; set; }
        public string TexconvPath { get; set; } = string.Empty;

        public bool AllValid => Gta5Installed && CodeWalkerValid && TexconvValid;
    }

    public static class EnvironmentChecker
    {
        public static EnvironmentCheckResult Check(OptimizerConfig config)
        {
            var result = new EnvironmentCheckResult();

            // 1. GTA5 check
            result.Gta5Path = FindGta5Folder(config.Gta5Folder);
            result.Gta5Installed = !string.IsNullOrEmpty(result.Gta5Path) && Directory.Exists(result.Gta5Path);
            if (result.Gta5Installed && string.IsNullOrEmpty(config.Gta5Folder))
            {
                config.Gta5Folder = result.Gta5Path;
            }

            // 2. CodeWalker check
            result.CodeWalkerPath = FindCodeWalkerFolder(config.CodeWalkerFolder);
            result.CodeWalkerValid = ValidateCodeWalkerFolder(result.CodeWalkerPath);
            if (result.CodeWalkerValid && string.IsNullOrEmpty(config.CodeWalkerFolder))
            {
                config.CodeWalkerFolder = result.CodeWalkerPath;
            }

            // 3. texconv.exe check
            result.TexconvPath = FindTexconv(config.TexconvPath);
            result.TexconvValid = !string.IsNullOrEmpty(result.TexconvPath) && File.Exists(result.TexconvPath);
            if (result.TexconvValid)
            {
                config.TexconvPath = result.TexconvPath;
            }

            return result;
        }

        public static string FindGta5Folder(string userConfigured)
        {
            if (!string.IsNullOrWhiteSpace(userConfigured) && Directory.Exists(userConfigured))
            {
                if (File.Exists(Path.Combine(userConfigured, "GTA5.exe")) || File.Exists(Path.Combine(userConfigured, "PlayGTAV.exe")))
                    return userConfigured;
            }

            // Registry Rockstar Games
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Rockstar Games\Grand Theft Auto V"))
                {
                    if (key != null)
                    {
                        var val = key.GetValue("InstallFolder") as string ?? key.GetValue("InstallFolderSteam") as string;
                        if (!string.IsNullOrEmpty(val) && Directory.Exists(val))
                            return val.TrimEnd('\\', '/');
                    }
                }
            }
            catch { }

            // Registry WOW6432Node Rockstar Games
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Rockstar Games\Grand Theft Auto V"))
                {
                    if (key != null)
                    {
                        var val = key.GetValue("InstallFolder") as string ?? key.GetValue("InstallFolderSteam") as string;
                        if (!string.IsNullOrEmpty(val) && Directory.Exists(val))
                            return val.TrimEnd('\\', '/');
                    }
                }
            }
            catch { }

            // Common Steam path
            string[] commonPaths = new[]
            {
                @"C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V",
                @"C:\Program Files\Steam\steamapps\common\Grand Theft Auto V",
                @"D:\SteamLibrary\steamapps\common\Grand Theft Auto V",
                @"E:\SteamLibrary\steamapps\common\Grand Theft Auto V",
                @"C:\Program Files\Epic Games\GTAV",
                @"D:\Epic Games\GTAV",
                @"E:\Epic Games\GTAV"
            };

            foreach (var p in commonPaths)
            {
                if (Directory.Exists(p) && (File.Exists(Path.Combine(p, "GTA5.exe")) || File.Exists(Path.Combine(p, "PlayGTAV.exe"))))
                    return p;
            }

            return string.Empty;
        }

        public static string FindCodeWalkerFolder(string userConfigured)
        {
            if (!string.IsNullOrWhiteSpace(userConfigured) && ValidateCodeWalkerFolder(userConfigured))
                return userConfigured;

            // Check current directory or program directory
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (ValidateCodeWalkerFolder(baseDir))
                return baseDir;

            // Check desktop shortcuts or known paths
            string[] probe = new[]
            {
                @"E:\CodeWalker30_dev48",
                @"C:\CodeWalker",
                @"D:\CodeWalker"
            };

            foreach (var p in probe)
            {
                if (ValidateCodeWalkerFolder(p))
                    return p;
            }

            return string.Empty;
        }

        public static bool ValidateCodeWalkerFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
                return false;

            bool hasDll = File.Exists(Path.Combine(folder, "CodeWalker.Core.dll"));
            bool hasExe = File.Exists(Path.Combine(folder, "CodeWalker.exe"));
            return hasDll || hasExe;
        }

        public static string FindTexconv(string userConfigured)
        {
            if (!string.IsNullOrWhiteSpace(userConfigured) && File.Exists(userConfigured))
                return Path.GetFullPath(userConfigured);

            string inBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "texconv.exe");
            if (File.Exists(inBase))
                return inBase;

            string inCurrent = Path.Combine(Directory.GetCurrentDirectory(), "texconv.exe");
            if (File.Exists(inCurrent))
                return inCurrent;

            return string.Empty;
        }
    }
}
