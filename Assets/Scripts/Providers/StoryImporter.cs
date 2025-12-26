using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Imports stories from portable .rody.json format.
/// Creates story folder in UserStoriesPath with all files.
/// </summary>
public static class StoryImporter
{
    /// <summary>
    /// Imports a story from JSON string.
    /// </summary>
    /// <param name="json">JSON content of .rody.json file</param>
    /// <returns>Path to the imported story folder, or null on failure</returns>
    public static string ImportFromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("StoryImporter: JSON content is empty");
            return null;
        }

        StoryExporter.ExportedStory exported;
        try
        {
            exported = JsonConvert.DeserializeObject<StoryExporter.ExportedStory>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"StoryImporter: Failed to parse JSON: {e.Message}");
            return null;
        }

        if (exported == null || exported.story == null)
        {
            Debug.LogError("StoryImporter: Invalid story data");
            return null;
        }

        // Create unique folder name
        string storyId = SanitizeFileName(exported.story.id);
        if (string.IsNullOrEmpty(storyId))
        {
            storyId = "imported_story";
        }

        string storyPath = GetUniqueImportPath(storyId);

        try
        {
            PathManager.EnsureUserStoriesDirectory();
            Directory.CreateDirectory(storyPath);

            // Write levels.rody
            WriteLevelsFile(storyPath, exported);

            // Write credits.txt
            WriteCreditsFile(storyPath, exported);

            // Write sprites
            WriteSprites(storyPath, exported);

            Debug.Log($"StoryImporter: Successfully imported '{exported.story.title}' to {storyPath}");
            return storyPath;
        }
        catch (Exception e)
        {
            Debug.LogError($"StoryImporter: Failed to import story: {e.Message}");

            // Cleanup on failure
            try
            {
                if (Directory.Exists(storyPath))
                {
                    Directory.Delete(storyPath, true);
                }
            }
            catch { }

            return null;
        }
    }

    /// <summary>
    /// Imports a story from a .rody.json file.
    /// </summary>
    /// <param name="filePath">Path to the .rody.json file</param>
    /// <returns>Path to the imported story folder, or null on failure</returns>
    public static string ImportFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"StoryImporter: File not found: {filePath}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            return ImportFromJson(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"StoryImporter: Failed to read file: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Validates a .rody.json file without importing.
    /// </summary>
    /// <param name="json">JSON content to validate</param>
    /// <returns>Validation result with story info or error message</returns>
    public static (bool isValid, string title, int sceneCount, string error) ValidateJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return (false, null, 0, "JSON content is empty");
        }

        try
        {
            var exported = JsonConvert.DeserializeObject<StoryExporter.ExportedStory>(json);

            if (exported == null)
            {
                return (false, null, 0, "Failed to parse JSON");
            }

            if (exported.story == null)
            {
                return (false, null, 0, "Missing story metadata");
            }

            if (exported.scenes == null || exported.scenes.Count == 0)
            {
                return (false, null, 0, "No scenes in story");
            }

            return (true, exported.story.title, exported.scenes.Count, null);
        }
        catch (Exception e)
        {
            return (false, null, 0, e.Message);
        }
    }

    #region Private Helpers

    private static string GetUniqueImportPath(string baseId)
    {
        string basePath = PathManager.GetUserStoryPath(baseId);

        if (!Directory.Exists(basePath))
        {
            return basePath;
        }

        // Find unique suffix
        int counter = 2;
        while (Directory.Exists(PathManager.GetUserStoryPath(baseId + "_" + counter)))
        {
            counter++;
        }

        return PathManager.GetUserStoryPath(baseId + "_" + counter);
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "";
        }

        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        return name.Trim();
    }

    private static void WriteLevelsFile(string storyPath, StoryExporter.ExportedStory exported)
    {
        string levelsPath = Path.Combine(storyPath, "levels.rody");

        using (var sw = new StreamWriter(levelsPath))
        {
            for (int i = 0; i < exported.scenes.Count; i++)
            {
                var scene = exported.scenes[i];
                int sceneNum = scene.index;
                var data = scene.data;

                // Scene header comments
                sw.WriteLine("#######################");
                sw.WriteLine($"###### scene {sceneNum} ########");
                sw.WriteLine("#######################");

                // Phonemes
                sw.WriteLine("## phonems");
                sw.WriteLine(EscapeLine(data.dialogues?.intro1));
                sw.WriteLine(EscapeLine(data.dialogues?.intro2));
                sw.WriteLine(EscapeLine(data.dialogues?.intro3));
                sw.WriteLine(EscapeLine(data.dialogues?.obj));
                sw.WriteLine(EscapeLine(data.dialogues?.ngp));
                sw.WriteLine(EscapeLine(data.dialogues?.fsw));

                // Texts
                sw.WriteLine("## texts [string]");
                sw.WriteLine(EscapeLine(data.texts?.title));
                sw.WriteLine(EscapeLine(data.texts?.intro));
                sw.WriteLine(EscapeLine(data.texts?.obj));
                sw.WriteLine(EscapeLine(data.texts?.ngp));
                sw.WriteLine(EscapeLine(data.texts?.fsw));

                // Music
                sw.WriteLine("## musics [i1..i3 | l1..l15]");
                string introMusic = data.music?.introMusic ?? "i1";
                string sceneMusic = data.music?.sceneMusic ?? "l1";
                sw.WriteLine($"{introMusic},{sceneMusic}");

                // Pitch
                sw.WriteLine("## pitch [float]");
                float pitch1 = data.voice?.pitch1 ?? 1f;
                float pitch2 = data.voice?.pitch2 ?? 1f;
                float pitch3 = data.voice?.pitch3 ?? 1f;
                sw.WriteLine($"{pitch1},{pitch2},{pitch3}");

                // IsMastico booleans
                sw.WriteLine("## is mastico speaking [booleans] ?");
                string m1 = (data.voice?.isMastico1 == true) ? "1" : "0";
                string m2 = (data.voice?.isMastico2 == true) ? "1" : "0";
                string m3 = (data.voice?.isMastico3 == true) ? "1" : "0";
                string z = (data.voice?.isZambla == true) ? "1" : "0";
                sw.WriteLine($"{m1},{m2},{m3},{z}");

                // Object zones
                sw.WriteLine("## obj position");
                sw.WriteLine(EscapeLine(data.objects?.obj?.positionRaw));
                sw.WriteLine("## obj size");
                sw.WriteLine(EscapeLine(data.objects?.obj?.sizeRaw));
                sw.WriteLine("## objNear position");
                sw.WriteLine(EscapeLine(data.objects?.obj?.nearPositionRaw));
                sw.WriteLine("## objNear size");
                sw.WriteLine(EscapeLine(data.objects?.obj?.nearSizeRaw));

                sw.WriteLine("## NGP position");
                sw.WriteLine(EscapeLine(data.objects?.ngp?.positionRaw));
                sw.WriteLine("## NGP size");
                sw.WriteLine(EscapeLine(data.objects?.ngp?.sizeRaw));
                sw.WriteLine("## NgpNear position");
                sw.WriteLine(EscapeLine(data.objects?.ngp?.nearPositionRaw));
                sw.WriteLine("## NgpNear size");
                sw.WriteLine(EscapeLine(data.objects?.ngp?.nearSizeRaw));

                sw.WriteLine("## FSW position");
                sw.WriteLine(EscapeLine(data.objects?.fsw?.positionRaw));
                sw.WriteLine("## FSW size");
                sw.WriteLine(EscapeLine(data.objects?.fsw?.sizeRaw));
                sw.WriteLine("## fswNear position");
                sw.WriteLine(EscapeLine(data.objects?.fsw?.nearPositionRaw));
                sw.WriteLine("## fswNear size");
                sw.WriteLine(EscapeLine(data.objects?.fsw?.nearSizeRaw));

                // Scene delimiter
                sw.WriteLine("~");
            }
        }

        Debug.Log($"StoryImporter: Written levels.rody with {exported.scenes.Count} scenes");
    }

    private static string EscapeLine(string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return ".";
        }

        // Escape newlines as \n
        line = line.Replace("\n", "\\n");

        return line;
    }

    private static void WriteCreditsFile(string storyPath, StoryExporter.ExportedStory exported)
    {
        string creditsPath = Path.Combine(storyPath, "credits.txt");

        using (var sw = new StreamWriter(creditsPath))
        {
            // First line is the title
            sw.WriteLine(exported.story.title ?? exported.story.id);

            // Remaining lines are credits
            if (!string.IsNullOrEmpty(exported.credits))
            {
                sw.Write(exported.credits);
            }
        }

        Debug.Log("StoryImporter: Written credits.txt");
    }

    private static void WriteSprites(string storyPath, StoryExporter.ExportedStory exported)
    {
        if (exported.sprites == null || exported.sprites.Count == 0)
        {
            Debug.LogWarning("StoryImporter: No sprites to import");
            return;
        }

        string spritesPath = Path.Combine(storyPath, "Sprites");
        Directory.CreateDirectory(spritesPath);

        int successCount = 0;
        int failCount = 0;

        foreach (var kvp in exported.sprites)
        {
            string fileName = kvp.Key;
            string base64Data = kvp.Value;

            try
            {
                // Handle data URL prefix if present
                if (base64Data.StartsWith("data:"))
                {
                    int commaIndex = base64Data.IndexOf(',');
                    if (commaIndex > 0)
                    {
                        base64Data = base64Data.Substring(commaIndex + 1);
                    }
                }

                byte[] imageBytes = Convert.FromBase64String(base64Data);
                string filePath = Path.Combine(spritesPath, fileName);
                File.WriteAllBytes(filePath, imageBytes);
                successCount++;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"StoryImporter: Failed to decode sprite {fileName}: {e.Message}");
                failCount++;
            }
        }

        Debug.Log($"StoryImporter: Written {successCount} sprites, {failCount} failed");
    }

    #endregion
}
