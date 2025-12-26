using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Exports stories to portable .rody.json format.
/// Includes all scene data and sprites as base64.
/// </summary>
public static class StoryExporter
{
    /// <summary>
    /// Data structure for exported story JSON.
    /// </summary>
    [Serializable]
    public class ExportedStory
    {
        public int formatVersion = 1;
        public string exportedAt;
        public ExportedStoryMetadata story;
        public string credits;
        public List<ExportedScene> scenes;
        public Dictionary<string, string> sprites; // filename -> base64 data
    }

    [Serializable]
    public class ExportedStoryMetadata
    {
        public string id;
        public string title;
        public int sceneCount;
    }

    [Serializable]
    public class ExportedScene
    {
        public int index;
        public SceneData data;
    }

    /// <summary>
    /// Exports a story to JSON string.
    /// </summary>
    /// <param name="storyPath">Full path to the story folder</param>
    /// <returns>JSON string of the exported story</returns>
    public static string ExportToJson(string storyPath)
    {
        if (!Directory.Exists(storyPath))
        {
            Debug.LogError($"StoryExporter: Story path not found: {storyPath}");
            return null;
        }

        string storyId = Path.GetFileName(storyPath);
        string levelsFile = Path.Combine(storyPath, "levels.rody");
        string creditsFile = Path.Combine(storyPath, "credits.txt");
        string spritesDir = Path.Combine(storyPath, "Sprites");

        if (!File.Exists(levelsFile))
        {
            Debug.LogError($"StoryExporter: levels.rody not found in {storyPath}");
            return null;
        }

        var exported = new ExportedStory
        {
            formatVersion = 1,
            exportedAt = DateTime.UtcNow.ToString("o"),
            story = new ExportedStoryMetadata { id = storyId },
            credits = "",
            scenes = new List<ExportedScene>(),
            sprites = new Dictionary<string, string>()
        };

        // Load credits
        if (File.Exists(creditsFile))
        {
            try
            {
                string[] creditLines = File.ReadAllLines(creditsFile);
                if (creditLines.Length > 0)
                {
                    exported.story.title = creditLines[0];
                    exported.credits = string.Join("\n", creditLines, 1, creditLines.Length - 1);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"StoryExporter: Error reading credits: {e.Message}");
                exported.story.title = storyId;
            }
        }
        else
        {
            exported.story.title = storyId;
        }

        // Count and load scenes
        int sceneCount = CountScenes(levelsFile);
        exported.story.sceneCount = sceneCount;

        for (int i = 1; i <= sceneCount; i++)
        {
            var sceneData = LoadSceneFromFile(levelsFile, i);
            exported.scenes.Add(new ExportedScene
            {
                index = i,
                data = sceneData
            });
        }

        // Load all sprites as base64
        if (Directory.Exists(spritesDir))
        {
            foreach (var file in Directory.GetFiles(spritesDir, "*.png"))
            {
                string fileName = Path.GetFileName(file);
                try
                {
                    byte[] bytes = File.ReadAllBytes(file);
                    string base64 = Convert.ToBase64String(bytes);
                    exported.sprites[fileName] = base64;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"StoryExporter: Error encoding sprite {fileName}: {e.Message}");
                }
            }
        }

        // Serialize to JSON using Newtonsoft.Json for better formatting
        string json = JsonConvert.SerializeObject(exported, Formatting.Indented);

        Debug.Log($"StoryExporter: Exported {storyId} with {sceneCount} scenes and {exported.sprites.Count} sprites");
        return json;
    }

    /// <summary>
    /// Exports a story directly to a file.
    /// </summary>
    /// <param name="storyPath">Full path to the story folder</param>
    /// <param name="outputPath">Path to write the .rody.json file</param>
    /// <returns>True if successful</returns>
    public static bool ExportToFile(string storyPath, string outputPath)
    {
        string json = ExportToJson(storyPath);

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            File.WriteAllText(outputPath, json);
            Debug.Log($"StoryExporter: Written to {outputPath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"StoryExporter: Failed to write file: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the suggested export filename for a story.
    /// </summary>
    public static string GetExportFileName(string storyPath)
    {
        string storyId = Path.GetFileName(storyPath);
        // Sanitize filename
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            storyId = storyId.Replace(c, '_');
        }
        return storyId + ".rody.json";
    }

    #region Private Helpers

    private static int CountScenes(string levelsFile)
    {
        int count = 0;
        try
        {
            using (var sr = new StreamReader(levelsFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line == "~") count++;
                }
            }
        }
        catch { }
        return count;
    }

    private static SceneData LoadSceneFromFile(string filePath, int sceneIndex)
    {
        var sceneStr = new string[26];

        try
        {
            using (var sr = new StreamReader(filePath))
            {
                string line;

                // Skip to the requested scene
                for (int j = 1; j < sceneIndex; j++)
                {
                    while ((line = sr.ReadLine()) != "~") ;
                }

                int i = 0;
                while ((line = sr.ReadLine()) != "~" && i < 26)
                {
                    if (line == "")
                    {
                        sceneStr[i] = "";
                        i++;
                    }
                    else if (line[0] == '#')
                    {
                        continue; // Comment line
                    }
                    else
                    {
                        if (line[0] == '.')
                            sceneStr[i] = "";
                        else
                            sceneStr[i] = ParseLine(line);
                        i++;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"StoryExporter: Error reading scene {sceneIndex}: {e.Message}");
        }

        return SceneDataParser.Parse(sceneStr);
    }

    private static string ParseLine(string line)
    {
        if (string.IsNullOrEmpty(line)) return "";

        string newLine = (line[0] == '\\') ? "" : line[0].ToString();
        for (int i = 1; i < line.Length; ++i)
        {
            if (line[i] == 'n' && line[i - 1] == '\\')
                newLine = string.Concat(newLine, "\n");
            else if (line[i] != '\\')
                newLine = string.Concat(newLine, line[i]);
        }
        return newLine;
    }

    #endregion
}
