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
    /// </summary>
    public static string GamePath => PlayerPrefs.GetString("gamePath");
    
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
