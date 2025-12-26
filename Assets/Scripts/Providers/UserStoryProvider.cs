using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Story provider for user-created stories.
/// Stores stories in Application.persistentDataPath/UserStories/.
/// Uses the same file format as LocalStoryProvider (levels.rody, Sprites/, credits.txt).
/// </summary>
public class UserStoryProvider : IStoryProvider
{
    private string _userStoriesPath;

    public UserStoryProvider()
    {
        _userStoriesPath = PathManager.UserStoriesPath;
        PathManager.EnsureUserStoriesDirectory();
    }

    /// <summary>
    /// Gets metadata for all user stories.
    /// </summary>
    public List<StoryMetadata> GetStories()
    {
        var stories = new List<StoryMetadata>();

        if (!Directory.Exists(_userStoriesPath))
        {
            return stories;
        }

        var directories = Directory.GetDirectories(_userStoriesPath);
        foreach (var dir in directories)
        {
            var storyId = Path.GetFileName(dir);

            // Skip hidden directories
            if (storyId.StartsWith(".")) continue;

            // Check if it's a valid story folder (has levels.rody)
            var levelsFile = Path.Combine(dir, "levels.rody");
            if (!File.Exists(levelsFile)) continue;

            var metadata = new StoryMetadata(storyId);
            metadata.sceneCount = CountScenes(storyId);

            // Try to load title from first line of credits if available
            var creditsFile = Path.Combine(dir, "credits.txt");
            if (File.Exists(creditsFile))
            {
                try
                {
                    using (var sr = new StreamReader(creditsFile))
                    {
                        metadata.title = sr.ReadLine() ?? storyId;
                    }
                }
                catch { metadata.title = storyId; }
            }

            stories.Add(metadata);
        }

        return stories;
    }

    /// <summary>
    /// Gets the number of scenes in a story.
    /// </summary>
    public int GetSceneCount(string storyId)
    {
        return CountScenes(storyId);
    }

    /// <summary>
    /// Loads scene data for a specific scene.
    /// </summary>
    public SceneData LoadScene(string storyId, int sceneIndex)
    {
        string levelsFile = GetStoryPath(storyId, "levels.rody");

        if (!File.Exists(levelsFile))
        {
            Debug.LogError($"UserStoryProvider: levels.rody not found for {storyId}");
            return SceneDataParser.CreateGlitchScene();
        }

        var raw = ReadSceneFromFile(levelsFile, sceneIndex);
        return SceneDataParser.Parse(raw);
    }

    /// <summary>
    /// Loads a sprite from a story.
    /// </summary>
    public Sprite LoadSprite(string storyId, string spriteName, int width, int height)
    {
        string spritePath = GetStoryPath(storyId, "Sprites", spriteName);
        return LoadSpriteFromPath(spritePath, width, height);
    }

    /// <summary>
    /// Loads all sprites for a scene (base + animation frames).
    /// </summary>
    public List<Sprite> LoadSceneSprites(string storyId, int sceneIndex)
    {
        var sprites = new List<Sprite>();
        string spritesDir = GetStoryPath(storyId, "Sprites");

        if (!Directory.Exists(spritesDir))
        {
            Debug.LogError($"UserStoryProvider: Sprites folder not found for {storyId}");
            return sprites;
        }

        var dir = new DirectoryInfo(spritesDir);
        var files = dir.GetFiles($"{sceneIndex}.*.png");

        // Load frames in order (1, 2, 3, ...)
        for (int i = 1; i <= files.Length; i++)
        {
            string framePath = Path.Combine(spritesDir, $"{sceneIndex}.{i}.png");
            if (File.Exists(framePath))
            {
                var sprite = LoadSpriteFromPath(framePath, 320, 130);
                if (sprite != null) sprites.Add(sprite);
            }
        }

        return sprites;
    }

    /// <summary>
    /// Loads the credits text for a story.
    /// </summary>
    public string LoadCredits(string storyId)
    {
        string creditsFile = GetStoryPath(storyId, "credits.txt");

        if (!File.Exists(creditsFile))
        {
            return "";
        }

        try
        {
            return File.ReadAllText(creditsFile);
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Saves a story (for editor/creator mode).
    /// Note: Actual scene saving is done by RM_SaveLoad writing to GamePath.
    /// This method is for metadata updates.
    /// </summary>
    public void SaveStory(string storyId, StoryData story)
    {
        string storyPath = GetStoryPath(storyId);

        // Ensure directories exist
        if (!Directory.Exists(storyPath))
        {
            Directory.CreateDirectory(storyPath);
        }

        string spritesPath = Path.Combine(storyPath, "Sprites");
        if (!Directory.Exists(spritesPath))
        {
            Directory.CreateDirectory(spritesPath);
        }

        // Save credits.txt with title on first line
        string creditsFile = Path.Combine(storyPath, "credits.txt");
        string creditsContent = (story.metadata?.title ?? storyId) + "\n" + (story.credits ?? "");
        File.WriteAllText(creditsFile, creditsContent);

        Debug.Log($"UserStoryProvider: Saved story metadata for {storyId}");
    }

    /// <summary>
    /// Checks if a story exists.
    /// </summary>
    public bool StoryExists(string storyId)
    {
        string storyPath = Path.Combine(_userStoriesPath, storyId);
        string levelsFile = Path.Combine(storyPath, "levels.rody");
        return Directory.Exists(storyPath) && File.Exists(levelsFile);
    }

    /// <summary>
    /// Deletes a user story completely.
    /// </summary>
    public bool DeleteStory(string storyId)
    {
        string storyPath = GetStoryPath(storyId);

        if (!Directory.Exists(storyPath))
        {
            Debug.LogWarning($"UserStoryProvider: Cannot delete - story not found: {storyId}");
            return false;
        }

        try
        {
            Directory.Delete(storyPath, true);
            Debug.Log($"UserStoryProvider: Deleted story {storyId}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UserStoryProvider: Failed to delete {storyId}: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Forks an official story to user space.
    /// Copies the entire story folder to UserStories with a unique name.
    /// Returns the new story path (gamePath to use).
    /// </summary>
    public static string ForkStory(string sourcePath)
    {
        if (!Directory.Exists(sourcePath))
        {
            Debug.LogError($"UserStoryProvider: Cannot fork - source not found: {sourcePath}");
            return null;
        }

        string originalName = Path.GetFileName(sourcePath);
        string forkName = PathManager.GetUniqueForkName(originalName);
        string targetPath = PathManager.GetUserStoryPath(forkName);

        PathManager.EnsureUserStoriesDirectory();

        try
        {
            CopyDirectory(sourcePath, targetPath);

            // Update credits.txt with new title
            string creditsFile = Path.Combine(targetPath, "credits.txt");
            if (File.Exists(creditsFile))
            {
                string[] lines = File.ReadAllLines(creditsFile);
                if (lines.Length > 0)
                {
                    lines[0] = forkName; // Use fork name as title
                    File.WriteAllLines(creditsFile, lines);
                }
            }
            else
            {
                File.WriteAllText(creditsFile, forkName + "\n");
            }

            Debug.Log($"UserStoryProvider: Forked '{originalName}' to '{forkName}'");
            return targetPath;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UserStoryProvider: Fork failed: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if a path is within user stories space.
    /// </summary>
    public static bool IsUserStoryPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        return path.StartsWith(PathManager.UserStoriesPath);
    }

    #region Private Helpers

    private string GetStoryPath(string storyId, params string[] subPaths)
    {
        var fullPath = Path.Combine(_userStoriesPath, storyId);
        foreach (var sub in subPaths)
        {
            fullPath = Path.Combine(fullPath, sub);
        }
        return fullPath;
    }

    private int CountScenes(string storyId)
    {
        string levelsFile = GetStoryPath(storyId, "levels.rody");

        if (!File.Exists(levelsFile))
            return 0;

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

    private string[] ReadSceneFromFile(string filePath, int sceneIndex)
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
        catch (System.Exception e)
        {
            Debug.LogError($"UserStoryProvider: Error reading scene {sceneIndex}: {e.Message}");
        }

        return sceneStr;
    }

    private string ParseLine(string line)
    {
        // Handle escape sequences like \n
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

    private Sprite LoadSpriteFromPath(string path, int width, int height)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning($"UserStoryProvider: Sprite not found: {path}");
            return null;
        }

        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.filterMode = FilterMode.Point;
            tex.LoadImage(bytes);

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UserStoryProvider: Error loading sprite {path}: {e.Message}");
            return null;
        }
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        // Copy files
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string targetFile = Path.Combine(targetDir, fileName);
            File.Copy(file, targetFile, true);
        }

        // Copy subdirectories recursively
        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(subDir);
            string targetSubDir = Path.Combine(targetDir, dirName);
            CopyDirectory(subDir, targetSubDir);
        }
    }

    #endregion
}
