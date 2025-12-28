using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to batch export stories from StreamingAssets to .rody.json format.
/// </summary>
public class StoryExportTool : EditorWindow
{
    private string outputPath;
    private Vector2 scrollPosition;
    private string[] storyFolders;

    [MenuItem("Tools/Rody/Export Stories to JSON")]
    public static void ShowWindow()
    {
        GetWindow<StoryExportTool>("Story Export Tool");
    }

    [MenuItem("Tools/Rody/Export All Stories Now")]
    public static void ExportAllStoriesNow()
    {
        string outputPath = Path.Combine(Application.dataPath, "..", "static", "Stories");

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        string[] storyFolders = Directory.GetDirectories(Application.streamingAssetsPath);
        int successCount = 0;

        foreach (var folder in storyFolders)
        {
            string folderName = Path.GetFileName(folder);

            // Skip invalid or template folders
            if (folderName == "Rody0") continue;
            if (!File.Exists(Path.Combine(folder, "levels.rody"))) continue;

            string outputFile = Path.Combine(outputPath, StoryExporter.GetExportFileName(folder));

            if (StoryExporter.ExportToFile(folder, outputFile))
            {
                Debug.Log($"[StoryExportTool] Exported: {folderName} -> {outputFile}");
                successCount++;
            }
            else
            {
                Debug.LogError($"[StoryExportTool] Failed: {folderName}");
            }
        }

        Debug.Log($"[StoryExportTool] Export complete! {successCount} stories exported to {outputPath}");
    }

    void OnEnable()
    {
        outputPath = Path.Combine(Application.dataPath, "ExportedStories");
        RefreshStoryList();
    }

    void RefreshStoryList()
    {
        if (Directory.Exists(Application.streamingAssetsPath))
        {
            storyFolders = Directory.GetDirectories(Application.streamingAssetsPath);
        }
        else
        {
            storyFolders = new string[0];
        }
    }

    void OnGUI()
    {
        GUILayout.Label("Export Stories to JSON", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Output path
        GUILayout.Label("Output Directory:");
        EditorGUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField(outputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selected = EditorUtility.OpenFolderPanel("Select Output Folder", outputPath, "");
            if (!string.IsNullOrEmpty(selected))
            {
                outputPath = selected;
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Refresh button
        if (GUILayout.Button("Refresh Story List"))
        {
            RefreshStoryList();
        }

        GUILayout.Space(10);

        // Story list
        GUILayout.Label($"Stories in StreamingAssets ({storyFolders?.Length ?? 0}):", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

        if (storyFolders != null)
        {
            foreach (var folder in storyFolders)
            {
                string folderName = Path.GetFileName(folder);

                // Skip non-story folders
                if (!IsValidStoryFolder(folder))
                {
                    GUI.color = Color.gray;
                    GUILayout.Label($"  {folderName} (not a valid story)");
                    GUI.color = Color.white;
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"  {folderName}");
                if (GUILayout.Button("Export", GUILayout.Width(60)))
                {
                    ExportSingleStory(folder);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        // Export all button
        GUI.color = Color.green;
        if (GUILayout.Button("Export All Stories", GUILayout.Height(30)))
        {
            ExportAllStories();
        }
        GUI.color = Color.white;

        GUILayout.Space(10);

        // Info
        EditorGUILayout.HelpBox(
            "This tool exports story folders to portable .rody.json files.\n" +
            "Each JSON file contains all scene data and sprites (as base64).",
            MessageType.Info);
    }

    bool IsValidStoryFolder(string folderPath)
    {
        return File.Exists(Path.Combine(folderPath, "levels.rody"))
            && File.Exists(Path.Combine(folderPath, "credits.txt"))
            && Directory.Exists(Path.Combine(folderPath, "Sprites"));
    }

    void ExportSingleStory(string storyPath)
    {
        string folderName = Path.GetFileName(storyPath);

        // Skip Rody0 (base template)
        if (folderName == "Rody0")
        {
            Debug.Log($"[StoryExportTool] Skipping {folderName} (base template)");
            return;
        }

        // Ensure output directory exists
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        string outputFile = Path.Combine(outputPath, StoryExporter.GetExportFileName(storyPath));

        bool success = StoryExporter.ExportToFile(storyPath, outputFile);

        if (success)
        {
            Debug.Log($"[StoryExportTool] Exported: {folderName} -> {outputFile}");
            EditorUtility.DisplayDialog("Export Complete", $"Exported {folderName} to:\n{outputFile}", "OK");
        }
        else
        {
            Debug.LogError($"[StoryExportTool] Failed to export: {folderName}");
            EditorUtility.DisplayDialog("Export Failed", $"Failed to export {folderName}. Check console for details.", "OK");
        }
    }

    void ExportAllStories()
    {
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        int successCount = 0;
        int skipCount = 0;
        int failCount = 0;

        foreach (var folder in storyFolders)
        {
            string folderName = Path.GetFileName(folder);

            // Skip invalid folders
            if (!IsValidStoryFolder(folder))
            {
                skipCount++;
                continue;
            }

            // Skip Rody0 (base template)
            if (folderName == "Rody0")
            {
                skipCount++;
                continue;
            }

            string outputFile = Path.Combine(outputPath, StoryExporter.GetExportFileName(folder));

            if (StoryExporter.ExportToFile(folder, outputFile))
            {
                Debug.Log($"[StoryExportTool] Exported: {folderName}");
                successCount++;
            }
            else
            {
                Debug.LogError($"[StoryExportTool] Failed: {folderName}");
                failCount++;
            }
        }

        string message = $"Export complete!\n\nSuccess: {successCount}\nSkipped: {skipCount}\nFailed: {failCount}\n\nOutput: {outputPath}";
        EditorUtility.DisplayDialog("Batch Export Complete", message, "OK");

        // Open the output folder
        EditorUtility.RevealInFinder(outputPath);
    }
}
