using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ReferenceScanner
{
    /// <summary>
    /// Finds all references to ScriptableObjects across scenes and prefabs.
    /// Uses GUID-based text search for performance on large projects.
    /// </summary>
    public static class SOReferenceFinder
    {
        public static List<ScanResult> FindReferences(ScriptableObject target, Action<float, string> onProgress = null)
        {
            if (target == null) return new List<ScanResult>();

            string assetPath = AssetDatabase.GetAssetPath(target);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError($"Could not get GUID for {target.name}");
                return new List<ScanResult>();
            }

            return FindReferencesByGuid(guid, onProgress);
        }

        public static List<ScanResult> FindReferencesByGuid(string guid, Action<float, string> onProgress = null)
        {
            var results = new List<ScanResult>();

            // Get all scenes and prefabs
            var scenePaths = AssetDatabase.FindAssets("t:Scene")
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();

            var prefabPaths = AssetDatabase.FindAssets("t:Prefab")
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();

            var allPaths = scenePaths.Concat(prefabPaths).ToArray();
            int total = allPaths.Length;
            int current = 0;

            foreach (var path in allPaths)
            {
                current++;
                onProgress?.Invoke((float)current / total, path);

                if (!File.Exists(path)) continue;

                try
                {
                    string content = File.ReadAllText(path);
                    if (content.Contains(guid))
                    {
                        string assetType = path.EndsWith(".unity") ? "Scene" : "Prefab";
                        results.Add(new ScanResult(path, assetType));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Could not read {path}: {e.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Finds ScriptableObjects with no references in any scene or prefab.
        /// </summary>
        public static List<string> FindOrphanedScriptableObjects(string searchFolder = "Assets",
            Action<float, string> onProgress = null)
        {
            var orphaned = new List<string>();

            // Get all ScriptableObjects
            var soPaths = AssetDatabase.FindAssets("t:ScriptableObject", new[] { searchFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !p.StartsWith("Packages/"))
                .ToArray();

            int total = soPaths.Length;
            int current = 0;

            // Cache all scene and prefab content
            onProgress?.Invoke(0, "Caching scene and prefab content...");
            var allContent = CacheAllContent();

            foreach (var soPath in soPaths)
            {
                current++;
                onProgress?.Invoke((float)current / total, soPath);

                string guid = AssetDatabase.AssetPathToGUID(soPath);
                bool hasReference = allContent.Any(c => c.Contains(guid));

                if (!hasReference)
                {
                    orphaned.Add(soPath);
                }
            }

            return orphaned;
        }

        static List<string> CacheAllContent()
        {
            var contents = new List<string>();

            var scenePaths = AssetDatabase.FindAssets("t:Scene")
                .Select(AssetDatabase.GUIDToAssetPath);

            var prefabPaths = AssetDatabase.FindAssets("t:Prefab")
                .Select(AssetDatabase.GUIDToAssetPath);

            foreach (var path in scenePaths.Concat(prefabPaths))
            {
                if (!File.Exists(path)) continue;
                try
                {
                    contents.Add(File.ReadAllText(path));
                }
                catch { /* skip unreadable files */ }
            }

            return contents;
        }

        /// <summary>
        /// Gets all ScriptableObjects in the project grouped by type.
        /// </summary>
        public static Dictionary<string, List<string>> GetAllScriptableObjectsByType(string searchFolder = "Assets")
        {
            var byType = new Dictionary<string, List<string>>();

            var soPaths = AssetDatabase.FindAssets("t:ScriptableObject", new[] { searchFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !p.StartsWith("Packages/"));

            foreach (var path in soPaths)
            {
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so == null) continue;

                string typeName = so.GetType().Name;
                if (!byType.ContainsKey(typeName))
                    byType[typeName] = new List<string>();

                byType[typeName].Add(path);
            }

            return byType;
        }
    }
}
