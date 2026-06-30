using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Transitional static facade over the StoryRoot-owned StorySession. Preserves
/// the long-standing WorkingStory.* call surface so consumers migrate one file
/// at a time; removed once everything reads the session via StoryRoot directly.
/// </summary>
public static class WorkingStory
{
    static StorySession S => StoryRoot.Session;

    /// <summary>The one live session. Subscribe to its DirtyChanged here.</summary>
    public static StorySession Session => S;

    public static Story Current => S.Current;
    public static bool IsOfficial => S.IsOfficial;
    public static bool IsDirty => S.IsDirty;
    public static string LastSavePath => S.LastSavePath;
    public static bool IsLoaded => S.IsLoaded;
    public static bool IsUserStory => S.IsUserStory;
    public static string Title => S.Title;
    public static string Id => S.Id;
    public static int SceneCount => S.SceneCount;

    public static int CurrentSceneIndex
    {
        get => S.CurrentSceneIndex;
        set => S.CurrentSceneIndex = value;
    }

    /// <summary>Loads an official story into the session via the catalog (copy-on-load).</summary>
    public static void LoadOfficial(string storyId)
    {
        var story = StoryRoot.Catalog.Resolve(storyId);
        if (story == null)
        {
            Debug.LogError($"WorkingStory: Story not found: {storyId}");
            return;
        }

        S.Load(story, StorySource.Builtin);
    }

    public static void LoadFromJson(string json, string savePath = null) => S.LoadFromJson(json, savePath);
    public static void CreateNew(string title) => S.CreateNew(title);
    public static void ForkForEditing() => S.ForkForEditing();
    public static void SaveScene(int sceneIndex, SceneData data) => S.SaveScene(sceneIndex, data);
    public static void SaveSprite(string spriteName, Texture2D texture) => S.SaveSprite(spriteName, texture);
    public static void CreateNewScene(int sceneIndex) => S.CreateNewScene(sceneIndex);
    public static void DeleteScene(int sceneIndex) => S.DeleteScene(sceneIndex);
    public static void SetTitle(string title) => S.SetTitle(title);
    public static void SetSceneCount(int count) => S.SetSceneCount(count);
    public static void SetCredits(string credits) => S.SetCredits(credits);
    public static SceneData LoadScene(int sceneIndex) => S.LoadScene(sceneIndex);
    public static Sprite LoadSprite(string spriteName, int width = 320, int height = 130) => S.LoadSprite(spriteName, width, height);
    public static List<Sprite> LoadSceneSprites(int sceneIndex) => S.LoadSceneSprites(sceneIndex);
    public static string GetCredits() => S.GetCredits();
    public static string ExportToJson() => S.ExportToJson();
    public static void MarkSaved(string path) => S.MarkSaved(path);
    public static void Clear() => S.Clear();
    public static void ClearSpriteCache() => S.ClearSpriteCache();
}
