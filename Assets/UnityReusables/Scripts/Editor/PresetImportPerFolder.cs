using System.IO;
using UnityEditor;
using UnityEditor.Presets;

/// <summary>
/// The following script applies a Preset based on the folder that you add an asset to.
/// This script chooses the Preset that is in the same folder as the asset.
/// If there is no Preset in the folder, this script searches parent folders.
/// If there are no Presets in parent folders, Unity uses the default Preset that the Preset window specifies.
/// </summary>
public class PresetImportPerFolder : AssetPostprocessor
{
    void OnPreprocessAsset()
    {
        // Make sure we are applying presets the first time an asset is imported.
        if (assetImporter.importSettingsMissing)
        {
            // Get the current imported asset folder.
            var path = Path.GetDirectoryName(assetPath);
            while (!string.IsNullOrEmpty(path))
            {
                // break the while loop if at root folder
                if (path == "Assets" || path == "ProjectSettings" || path == "Library") break;
                // Find all Preset assets in this folder.
                var presetGuids = AssetDatabase.FindAssets("t:Preset", new[] { path });
                foreach (var presetGuid in presetGuids)
                {
                    // Make sure we are not testing Presets in a subfolder.
                    string presetPath = AssetDatabase.GUIDToAssetPath(presetGuid);
                    if (Path.GetDirectoryName(presetPath) == path)
                    {
                        // Load the Preset and try to apply it to the importer.
                        var preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
                        if (preset.ApplyTo(assetImporter))
                            return;
                    }
                }

                // Try again in the parent folder.
                path = Path.GetDirectoryName(path);
            }
        }
    }
}
