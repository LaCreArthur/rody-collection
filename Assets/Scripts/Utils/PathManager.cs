using System.IO;
using UnityEngine;

/// <summary>
/// Centralized path management for game data.
/// All paths are constructed using cross-platform Path.Combine.
/// </summary>
public static class PathManager
{
    /// <summary>
    /// Root path of the currently loaded story (set via PlayerPrefs "gamePath").
    /// For JSON stories, this is the path to the .rody.json file.
    /// For folder stories, this is the path to the story folder.
    /// </summary>
    public static string GamePath => PlayerPrefs.GetString("gamePath");

    /// <summary>
    /// Returns true if the current GamePath points to a .rody.json file.
    /// </summary>
    public static bool IsJsonStory => GamePath.EndsWith(".rody.json") || GamePath.EndsWith(".json");

    /// <summary>
    /// Root path for user-created stories (in persistent data).
    /// </summary>
    public static string UserStoriesPath =>
        Path.Combine(Application.persistentDataPath, "UserStories");

    /// <summary>
    /// Returns true if the current GamePath is a user story (in UserStoriesPath).
    /// Works for both folder and JSON stories.
    /// </summary>
    public static bool IsUserStory
    {
        get
        {
            if (string.IsNullOrEmpty(GamePath)) return false;
            return GamePath.StartsWith(UserStoriesPath);
        }
    }

    /// <summary>
    /// Returns true if the current GamePath is an official story (in StreamingAssets).
    /// Official stories require forking before editing.
    /// </summary>
    public static bool IsOfficialStory
    {
        get
        {
            if (string.IsNullOrEmpty(GamePath)) return false;
            return GamePath.StartsWith(Application.streamingAssetsPath);
        }
    }

    /// <summary>
    /// Forks a JSON story by copying it to UserStories with a unique name.
    /// Returns the path to the forked copy, or null on failure.
    /// </summary>
    public static string ForkJsonStory(string sourcePath)
    {
        if (!File.Exists(sourcePath))
        {
            Debug.LogError($"[PathManager] Cannot fork - source not found: {sourcePath}");
            return null;
        }

        // Read the JSON to get the title for naming
        try
        {
            string json = File.ReadAllText(sourcePath);
            var story = Newtonsoft.Json.JsonConvert.DeserializeObject<StoryExporter.ExportedStory>(json);
            string title = story?.story?.title ?? Path.GetFileNameWithoutExtension(sourcePath);

            // Get unique path with _edit suffix
            string destPath = GetUniqueJsonForkPath(title);

            // Copy the file
            File.Copy(sourcePath, destPath);
            Debug.Log($"[PathManager] Forked JSON story to: {destPath}");

            return destPath;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PathManager] Failed to fork JSON story: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets a unique path for a forked JSON story (adds _edit suffix).
    /// </summary>
    public static string GetUniqueJsonForkPath(string storyTitle)
    {
        EnsureUserStoriesDirectory();

        string baseName = storyTitle + "_edit.rody.json";
        string targetPath = Path.Combine(UserStoriesPath, baseName);

        if (!File.Exists(targetPath))
            return targetPath;

        // Find a unique number suffix
        int counter = 2;
        while (File.Exists(Path.Combine(UserStoriesPath, $"{storyTitle}_edit{counter}.rody.json")))
        {
            counter++;
        }

        return Path.Combine(UserStoriesPath, $"{storyTitle}_edit{counter}.rody.json");
    }

    /// <summary>
    /// Gets the path to a specific user story.
    /// </summary>
    public static string GetUserStoryPath(string storyId)
    {
        return Path.Combine(UserStoriesPath, storyId);
    }

    /// <summary>
    /// Ensures the UserStories directory exists.
    /// </summary>
    public static void EnsureUserStoriesDirectory()
    {
        if (!Directory.Exists(UserStoriesPath))
        {
            Directory.CreateDirectory(UserStoriesPath);
        }
    }

    /// <summary>
    /// Generates a unique fork name for a story by appending _edit suffix.
    /// If _edit exists, tries _edit2, _edit3, etc.
    /// </summary>
    public static string GetUniqueForkName(string originalName)
    {
        string baseName = originalName + "_edit";
        string targetPath = GetUserStoryPath(baseName);

        if (!Directory.Exists(targetPath))
            return baseName;

        // Find a unique number suffix
        int counter = 2;
        while (Directory.Exists(GetUserStoryPath(baseName + counter)))
        {
            counter++;
        }

        return baseName + counter;
    }

    /// <summary>
    /// Gets a unique path for a JSON story file in the UserStories directory.
    /// </summary>
    public static string GetUniqueJsonStoryPath(string storyTitle)
    {
        EnsureUserStoriesDirectory();

        string baseName = storyTitle + ".rody.json";
        string targetPath = Path.Combine(UserStoriesPath, baseName);

        if (!File.Exists(targetPath))
            return targetPath;

        // Find a unique number suffix
        int counter = 2;
        while (File.Exists(Path.Combine(UserStoriesPath, $"{storyTitle}_{counter}.rody.json")))
        {
            counter++;
        }

        return Path.Combine(UserStoriesPath, $"{storyTitle}_{counter}.rody.json");
    }
    
    /// <summary>
    /// Path to the Sprites folder within the current story.
    /// </summary>
    public static string SpritesPath => Path.Combine(GamePath, "Sprites");
    
    /// <summary>
    /// Path to the levels.rody file containing scene data.
    /// </summary>
    public static string LevelsFile => Path.Combine(GamePath, "levels.rody");
    
    /// <summary>
    /// Path to the credits.txt file.
    /// </summary>
    public static string CreditsFile => Path.Combine(GamePath, "credits.txt");
    
    /// <summary>
    /// Gets the path to a specific sprite file.
    /// </summary>
    /// <param name="scene">Scene number</param>
    /// <param name="frame">Frame number (1-based)</param>
    /// <returns>Full path to the sprite PNG file</returns>
    public static string GetSpritePath(int scene, int frame)
    {
        return Path.Combine(SpritesPath, $"{scene}.{frame}.png");
    }
    
    /// <summary>
    /// Gets the path to a sprite by full filename.
    /// </summary>
    /// <param name="filename">Sprite filename (e.g., "0.png", "1.1.png")</param>
    /// <returns>Full path to the sprite file</returns>
    public static string GetSpritePath(string filename)
    {
        return Path.Combine(SpritesPath, filename);
    }
    
    /// <summary>
    /// Gets the base sprite path for a scene (without frame number).
    /// Used for pattern matching when loading all frames.
    /// </summary>
    /// <param name="scene">Scene number</param>
    /// <returns>Base path like "/path/to/Sprites/1"</returns>
    public static string GetSceneSpritesBasePath(int scene)
    {
        return Path.Combine(SpritesPath, scene.ToString());
    }
}
