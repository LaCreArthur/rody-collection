using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ReferenceScanner
{
    /// <summary>
    /// Analyzes Unity build reports to find assets included/excluded from builds.
    /// Uses Library/LastBuild.buildreport for accurate "in build" detection.
    /// </summary>
    public static class BuildReportAnalyzer
    {
        const string BuildReportPath = "Library/LastBuild.buildreport";

        public class BuildAssetInfo
        {
            public string Path { get; set; }
            public string Type { get; set; }
            public long Size { get; set; }
            public string SizeFormatted => FormatSize(Size);

            static string FormatSize(long bytes)
            {
                if (bytes < 1024) return $"{bytes} B";
                if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
                return $"{bytes / (1024.0 * 1024.0):F1} MB";
            }
        }

        public class BuildReportData
        {
            public bool IsValid { get; set; }
            public string Error { get; set; }
            public DateTime BuildTime { get; set; }
            public string Platform { get; set; }
            public long TotalSize { get; set; }
            public List<BuildAssetInfo> IncludedAssets { get; set; } = new();
            public HashSet<string> IncludedAssetPaths { get; set; } = new();
        }

        /// <summary>
        /// Loads and parses the last build report.
        /// </summary>
        public static BuildReportData LoadLastBuildReport()
        {
            var data = new BuildReportData();

            if (!File.Exists(BuildReportPath))
            {
                data.Error = "No build report found. Build your project first with File > Build Settings > Build.";
                return data;
            }

            try
            {
                // Copy to Assets temporarily to load it
                string tempPath = "Assets/Editor/ReferenceScanner/temp_buildreport.buildreport";
                File.Copy(BuildReportPath, tempPath, true);
                AssetDatabase.Refresh();

                var report = AssetDatabase.LoadAssetAtPath<BuildReport>(tempPath);
                if (report == null)
                {
                    data.Error = "Failed to load build report. Try building again.";
                    CleanupTempReport(tempPath);
                    return data;
                }

                data.IsValid = true;
                data.BuildTime = File.GetLastWriteTime(BuildReportPath);
                data.Platform = report.summary.platform.ToString();
                data.TotalSize = (long)report.summary.totalSize;

                // Extract packed assets
                foreach (var packedAsset in report.packedAssets)
                {
                    foreach (var content in packedAsset.contents)
                    {
                        var assetPath = content.sourceAssetPath;
                        if (string.IsNullOrEmpty(assetPath)) continue;
                        if (assetPath.StartsWith("Packages/")) continue; // Skip package assets

                        if (!data.IncludedAssetPaths.Contains(assetPath))
                        {
                            data.IncludedAssetPaths.Add(assetPath);
                            data.IncludedAssets.Add(new BuildAssetInfo
                            {
                                Path = assetPath,
                                Type = content.type.ToString(),
                                Size = (long)content.packedSize
                            });
                        }
                    }
                }

                CleanupTempReport(tempPath);
            }
            catch (Exception e)
            {
                data.Error = $"Error reading build report: {e.Message}";
            }

            return data;
        }

        static void CleanupTempReport(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }
                string metaPath = path + ".meta";
                if (File.Exists(metaPath))
                {
                    File.Delete(metaPath);
                }
            }
            catch { /* ignore cleanup errors */ }
        }

        /// <summary>
        /// Finds ScriptableObjects that exist in project but were NOT included in the last build.
        /// </summary>
        public static List<string> FindUnusedScriptableObjects(BuildReportData buildData,
            Action<float, string> onProgress = null)
        {
            var unused = new List<string>();

            if (!buildData.IsValid)
                return unused;

            var allSOs = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !p.StartsWith("Packages/"))
                .ToArray();

            int total = allSOs.Length;
            int current = 0;

            foreach (var soPath in allSOs)
            {
                current++;
                onProgress?.Invoke((float)current / total, soPath);

                if (!buildData.IncludedAssetPaths.Contains(soPath))
                {
                    unused.Add(soPath);
                }
            }

            return unused;
        }

        /// <summary>
        /// Finds all assets (not just SOs) that exist in project but were NOT included in the last build.
        /// </summary>
        public static List<string> FindAllUnusedAssets(BuildReportData buildData,
            string[] extensions = null, Action<float, string> onProgress = null)
        {
            var unused = new List<string>();

            if (!buildData.IsValid)
                return unused;

            extensions ??= new[] { ".asset", ".prefab", ".mat", ".png", ".jpg", ".wav", ".mp3" };

            var allAssets = AssetDatabase.FindAssets("", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !p.StartsWith("Packages/") && extensions.Any(ext => p.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            int total = allAssets.Length;
            int current = 0;

            foreach (var assetPath in allAssets)
            {
                current++;
                onProgress?.Invoke((float)current / total, assetPath);

                if (!buildData.IncludedAssetPaths.Contains(assetPath))
                {
                    unused.Add(assetPath);
                }
            }

            return unused;
        }

        /// <summary>
        /// Checks if an asset was included in the last build.
        /// </summary>
        public static bool WasIncludedInBuild(string assetPath, BuildReportData buildData)
        {
            if (!buildData.IsValid) return false;
            return buildData.IncludedAssetPaths.Contains(assetPath);
        }
    }
}
