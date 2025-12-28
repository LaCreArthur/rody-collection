using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstraction layer for story data access.
/// Used by ResourcesStoryProvider for loading official stories from embedded Resources.
/// Runtime story state is managed by WorkingStory.
/// </summary>
public interface IStoryProvider
{
    /// <summary>
    /// Gets metadata for all available stories.
    /// </summary>
    List<StoryMetadata> GetStories();
    
    /// <summary>
    /// Gets the number of scenes in a story.
    /// </summary>
    int GetSceneCount(string storyId);
    
    /// <summary>
    /// Loads scene data for a specific scene.
    /// </summary>
    SceneData LoadScene(string storyId, int sceneIndex);
    
    /// <summary>
    /// Loads a sprite from a story.
    /// </summary>
    Sprite LoadSprite(string storyId, string spriteName, int width, int height);
    
    /// <summary>
    /// Loads all sprites for a scene (base + animation frames).
    /// </summary>
    List<Sprite> LoadSceneSprites(string storyId, int sceneIndex);
    
    /// <summary>
    /// Loads the credits text for a story.
    /// </summary>
    string LoadCredits(string storyId);
    
    /// <summary>
    /// Saves a story (for editor/creator mode).
    /// </summary>
    void SaveStory(string storyId, StoryData story);
    
    /// <summary>
    /// Checks if a story exists.
    /// </summary>
    bool StoryExists(string storyId);
}

/// <summary>
/// Metadata about a story for display in selection UI.
/// </summary>
[System.Serializable]
public class StoryMetadata
{
    public string id;           // Folder name / unique identifier
    public string title;        // Display title
    public int sceneCount;      // Number of scenes
    public Sprite thumbnail;    // Optional thumbnail image
    
    public StoryMetadata(string id)
    {
        this.id = id;
        this.title = id;  // Default to folder name
        this.sceneCount = 0;
    }
}

/// <summary>
/// Complete story data for saving/loading.
/// </summary>
[System.Serializable]
public class StoryData
{
    public StoryMetadata metadata;
    public List<SceneData> scenes;
    public string credits;
    
    public StoryData(string storyId)
    {
        metadata = new StoryMetadata(storyId);
        scenes = new List<SceneData>();
        credits = "";
    }
}
