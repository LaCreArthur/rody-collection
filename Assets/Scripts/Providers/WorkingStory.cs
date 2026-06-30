using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Transitional static facade over the single StorySession. Preserves the
/// long-standing WorkingStory.* call surface so consumers migrate one file at
/// a time; it is removed once everything reads the session via StoryRoot.
/// </summary>
public static class WorkingStory
{
    static readonly StorySession _session = new StorySession();

    /// <summary>The one live session. Subscribe to its DirtyChanged here.</summary>
    public static StorySession Session => _session;

    public static Story Current => _session.Current;
    public static bool IsOfficial => _session.IsOfficial;
    public static bool IsDirty => _session.IsDirty;
    public static string LastSavePath => _session.LastSavePath;
    public static bool IsLoaded => _session.IsLoaded;
    public static bool IsUserStory => _session.IsUserStory;
    public static string Title => _session.Title;
    public static string Id => _session.Id;
    public static int SceneCount => _session.SceneCount;

    public static int CurrentSceneIndex
    {
        get => _session.CurrentSceneIndex;
        set => _session.CurrentSceneIndex = value;
    }

    /// <summary>Loads an official story from Resources into the session (read-only until forked).</summary>
    public static void LoadOfficial(string storyId)
    {
        var provider = StoryProviderManager.Provider as ResourcesStoryProvider;
        if (provider == null)
        {
            Debug.LogError("WorkingStory: ResourcesStoryProvider not available");
            return;
        }

        var story = provider.GetExportedStory(storyId);
        if (story == null)
        {
            Debug.LogError($"WorkingStory: Story not found: {storyId}");
            return;
        }

        _session.Load(story, StorySource.Builtin);
    }

    public static void LoadFromJson(string json, string savePath = null) => _session.LoadFromJson(json, savePath);
    public static void CreateNew(string title) => _session.CreateNew(title);
    public static void ForkForEditing() => _session.ForkForEditing();
    public static void SaveScene(int sceneIndex, SceneData data) => _session.SaveScene(sceneIndex, data);
    public static void SaveSprite(string spriteName, Texture2D texture) => _session.SaveSprite(spriteName, texture);
    public static void CreateNewScene(int sceneIndex) => _session.CreateNewScene(sceneIndex);
    public static void DeleteScene(int sceneIndex) => _session.DeleteScene(sceneIndex);
    public static void SetTitle(string title) => _session.SetTitle(title);
    public static void SetSceneCount(int count) => _session.SetSceneCount(count);
    public static void SetCredits(string credits) => _session.SetCredits(credits);
    public static SceneData LoadScene(int sceneIndex) => _session.LoadScene(sceneIndex);
    public static Sprite LoadSprite(string spriteName, int width = 320, int height = 130) => _session.LoadSprite(spriteName, width, height);
    public static List<Sprite> LoadSceneSprites(int sceneIndex) => _session.LoadSceneSprites(sceneIndex);
    public static string GetCredits() => _session.GetCredits();
    public static string ExportToJson() => _session.ExportToJson();
    public static void MarkSaved(string path) => _session.MarkSaved(path);
    public static void Clear() => _session.Clear();
    public static void ClearSpriteCache() => _session.ClearSpriteCache();
}
