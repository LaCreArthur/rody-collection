using UnityEngine;

namespace ReferenceScanner
{
    /// <summary>
    /// Represents a single reference found during scanning.
    /// </summary>
    public class ScanResult
    {
        public string AssetPath { get; }
        public string AssetType { get; }  // "Scene", "Prefab", "Script", etc.
        public string ObjectPath { get; } // Hierarchy path within scene/prefab
        public string ComponentType { get; }
        public string FieldName { get; }

        public ScanResult(string assetPath, string assetType, string objectPath = null,
            string componentType = null, string fieldName = null)
        {
            AssetPath = assetPath;
            AssetType = assetType;
            ObjectPath = objectPath;
            ComponentType = componentType;
            FieldName = fieldName;
        }

        public string DisplayName => System.IO.Path.GetFileNameWithoutExtension(AssetPath);

        public override string ToString()
        {
            var result = $"[{AssetType}] {DisplayName}";
            if (!string.IsNullOrEmpty(ObjectPath))
                result += $" > {ObjectPath}";
            if (!string.IsNullOrEmpty(ComponentType))
                result += $" ({ComponentType})";
            if (!string.IsNullOrEmpty(FieldName))
                result += $".{FieldName}";
            return result;
        }
    }
}
