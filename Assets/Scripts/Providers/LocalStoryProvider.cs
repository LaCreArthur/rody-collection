using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Local story provider that reads from StreamingAssets.
/// This is the default implementation for local/offline play.
/// </summary>
public class LocalStoryProvider : IStoryProvider
{
    private string _streamingAssetsPath;
    
    public LocalStoryProvider()
    {
        _streamingAssetsPath = Application.streamingAssetsPath;
    }
    
    /// <summary>
    /// Gets metadata for all stories in StreamingAssets.
    /// </summary>
    public List<StoryMetadata> GetStories()
    {
        var stories = new List<StoryMetadata>();
        
        if (!Directory.Exists(_streamingAssetsPath))
        {
            Debug.LogError("LocalStoryProvider: StreamingAssets not found");
            return stories;
        }
        
        var directories = Directory.GetDirectories(_streamingAssetsPath);
        foreach (var dir in directories)
        {
            var storyId = Path.GetFileName(dir);
            
            // Skip hidden directories and meta files
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
            Debug.LogError($"LocalStoryProvider: levels.rody not found for {storyId}");
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
            Debug.LogError($"LocalStoryProvider: Sprites folder not found for {storyId}");
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
    /// Currently delegates to RM_SaveLoad for compatibility.
    /// </summary>
    public void SaveStory(string storyId, StoryData story)
    {
        // TODO: Implement full save when migrating RM_SaveLoad
        Debug.LogWarning("LocalStoryProvider.SaveStory: Not yet implemented - use RM_SaveLoad");
    }
    
    /// <summary>
    /// Checks if a story exists.
    /// </summary>
    public bool StoryExists(string storyId)
    {
        string storyPath = Path.Combine(_streamingAssetsPath, storyId);
        string levelsFile = Path.Combine(storyPath, "levels.rody");
        return Directory.Exists(storyPath) && File.Exists(levelsFile);
    }
    
    #region Private Helpers
    
    private string GetStoryPath(string storyId, params string[] subPaths)
    {
        var fullPath = Path.Combine(_streamingAssetsPath, storyId);
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
            Debug.LogError($"LocalStoryProvider: Error reading scene {sceneIndex}: {e.Message}");
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
            Debug.LogWarning($"LocalStoryProvider: Sprite not found: {path}");
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
            Debug.LogError($"LocalStoryProvider: Error loading sprite {path}: {e.Message}");
            return null;
        }
    }
    
    #endregion
}
