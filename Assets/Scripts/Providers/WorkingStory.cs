using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Static class holding the currently loaded story in memory.
/// Only one story is active at a time. Used for play and edit.
/// </summary>
public static class WorkingStory
{
    /// <summary>
    /// The currently loaded story data.
    /// </summary>
    public static StoryExporter.ExportedStory Current { get; private set; }

    /// <summary>
    /// Is this an official story from Resources? (read-only until forked)
    /// </summary>
    public static bool IsOfficial { get; private set; }

    /// <summary>
    /// Has the story been modified since load/last save?
    /// </summary>
    public static bool IsDirty { get; private set; }

    /// <summary>
    /// Last file path where this story was saved.
    /// Null = never saved (show "Save As" dialog).
    /// </summary>
    public static string LastSavePath { get; private set; }

    /// <summary>
    /// Whether a story is currently loaded.
    /// </summary>
    public static bool IsLoaded => Current != null;

    /// <summary>
    /// Story title for display.
    /// </summary>
    public static string Title => Current?.story?.title ?? "Sans titre";

    /// <summary>
    /// Story ID.
    /// </summary>
    public static string Id => Current?.story?.id ?? "";

    /// <summary>
    /// Number of scenes in the story.
    /// </summary>
    public static int SceneCount => Current?.story?.sceneCount ?? 0;

    /// <summary>
    /// Current scene index being played/edited (1-based).
    /// Replaces PlayerPrefs "currentScene" usage.
    /// </summary>
    public static int CurrentSceneIndex { get; set; } = 1;

    // Sprite cache for loaded sprites
    private static Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

    // Pre-computed blank sprite base64
    private static string _blankSpriteBase64;
    private static string BlankSpriteBase64
    {
        get
        {
            if (_blankSpriteBase64 == null)
            {
                var tex = new Texture2D(320, 130, TextureFormat.RGBA32, false);
                var pixels = new Color[320 * 130];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
                tex.SetPixels(pixels);
                tex.Apply();
                _blankSpriteBase64 = Convert.ToBase64String(tex.EncodeToPNG());
                UnityEngine.Object.Destroy(tex);
            }
            return _blankSpriteBase64;
        }
    }

    #region Loading

    /// <summary>
    /// Loads an official story from Resources. Read-only until forked.
    /// </summary>
    public static void LoadOfficial(string storyId)
    {
        Clear();

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

        Current = story;
        IsOfficial = true;
        IsDirty = false;
        LastSavePath = null;

        Debug.Log($"WorkingStory: Loaded official story '{Title}'");
    }

    /// <summary>
    /// Loads from JSON string (imported file).
    /// </summary>
    public static void LoadFromJson(string json, string savePath = null)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("WorkingStory: Empty JSON");
            return;
        }

        Clear();

        try
        {
            Current = JsonConvert.DeserializeObject<StoryExporter.ExportedStory>(json);
            IsOfficial = false;
            IsDirty = false;
            LastSavePath = savePath;

            Debug.Log($"WorkingStory: Loaded from JSON '{Title}'");
        }
        catch (Exception e)
        {
            Debug.LogError($"WorkingStory: Failed to parse JSON: {e.Message}");
        }
    }

    /// <summary>
    /// Creates a new blank story for editing.
    /// </summary>
    public static void CreateNew(string title)
    {
        Clear();

        Current = new StoryExporter.ExportedStory
        {
            formatVersion = 1,
            exportedAt = DateTime.UtcNow.ToString("o"),
            story = new StoryExporter.ExportedStoryMetadata
            {
                id = SanitizeId(title),
                title = title,
                sceneCount = 1
            },
            credits = "",
            scenes = new List<StoryExporter.ExportedScene>(),
            sprites = new Dictionary<string, string>()
        };

        // Add default first scene
        var defaultScene = SceneDataParser.CreateGlitchScene();
        defaultScene.texts.title = "Premier tableau";
        defaultScene.texts.intro = "Texte d'introduction";

        Current.scenes.Add(new StoryExporter.ExportedScene
        {
            index = 1,
            data = defaultScene
        });

        // Add blank sprite for first scene
        Current.sprites["1.1.png"] = BlankSpriteBase64;

        IsOfficial = false;
        IsDirty = true;
        LastSavePath = null;

        Debug.Log($"WorkingStory: Created new story '{title}'");
    }

    #endregion

    #region Editing

    /// <summary>
    /// Forks the current story for editing.
    /// Creates a deep copy and marks as non-official.
    /// </summary>
    public static void ForkForEditing()
    {
        if (Current == null)
        {
            Debug.LogError("WorkingStory: No story loaded");
            return;
        }

        if (!IsOfficial)
        {
            Debug.Log("WorkingStory: Already a user story, no fork needed");
            return;
        }

        // Deep copy via JSON serialization
        string json = JsonConvert.SerializeObject(Current, Formatting.None);
        Current = JsonConvert.DeserializeObject<StoryExporter.ExportedStory>(json);

        // Update metadata
        Current.story.title = Current.story.title + " (copie)";
        Current.story.id = SanitizeId(Current.story.title);
        Current.exportedAt = DateTime.UtcNow.ToString("o");

        IsOfficial = false;
        IsDirty = true;
        LastSavePath = null;

        // Clear sprite cache since we now have our own copy
        ClearSpriteCache();

        Debug.Log($"WorkingStory: Forked to '{Title}'");
    }

    /// <summary>
    /// Updates scene data in the working story.
    /// </summary>
    public static void SaveScene(int sceneIndex, SceneData data)
    {
        if (Current == null)
        {
            Debug.LogError("WorkingStory: No story loaded");
            return;
        }

        if (Current.scenes == null)
            Current.scenes = new List<StoryExporter.ExportedScene>();

        var existing = Current.scenes.Find(s => s.index == sceneIndex);
        if (existing != null)
        {
            existing.data = data;
        }
        else
        {
            Current.scenes.Add(new StoryExporter.ExportedScene
            {
                index = sceneIndex,
                data = data
            });
        }

        IsDirty = true;
        Debug.Log($"WorkingStory: Updated scene {sceneIndex}");
    }

    /// <summary>
    /// Saves a sprite to the working story as base64.
    /// </summary>
    public static void SaveSprite(string spriteName, Texture2D texture)
    {
        if (Current == null)
        {
            Debug.LogError("WorkingStory: No story loaded");
            return;
        }

        if (Current.sprites == null)
            Current.sprites = new Dictionary<string, string>();

        try
        {
            Texture2D readableTex = MakeTextureReadable(texture);
            byte[] pngData = readableTex.EncodeToPNG();

            if (readableTex != texture)
                UnityEngine.Object.Destroy(readableTex);

            Current.sprites[spriteName] = Convert.ToBase64String(pngData);

            // Clear cached sprite
            if (_spriteCache.ContainsKey(spriteName))
            {
                var old = _spriteCache[spriteName];
                if (old != null && old.texture != null)
                {
                    UnityEngine.Object.Destroy(old.texture);
                    UnityEngine.Object.Destroy(old);
                }
                _spriteCache.Remove(spriteName);
            }

            IsDirty = true;
            Debug.Log($"WorkingStory: Updated sprite {spriteName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"WorkingStory: Failed to save sprite: {e.Message}");
        }
    }

    /// <summary>
    /// Creates a new scene in the story.
    /// </summary>
    public static void CreateNewScene(int sceneIndex)
    {
        if (Current == null)
        {
            Debug.LogError("WorkingStory: No story loaded");
            return;
        }

        if (Current.scenes == null)
            Current.scenes = new List<StoryExporter.ExportedScene>();

        // Check if scene exists
        if (Current.scenes.Exists(s => s.index == sceneIndex))
        {
            Debug.Log($"WorkingStory: Scene {sceneIndex} already exists");
            return;
        }

        // Create new scene data
        var newScene = new SceneData();
        newScene.texts.title = "Nouveau titre";
        newScene.texts.intro = "Nouveau texte d'introduction";
        newScene.texts.obj = ".";
        newScene.texts.ngp = ".";
        newScene.texts.fsw = ".";
        newScene.dialogues.intro1 = ".";
        newScene.dialogues.intro2 = ".";
        newScene.dialogues.intro3 = ".";
        newScene.dialogues.obj = ".";
        newScene.dialogues.ngp = ".";
        newScene.dialogues.fsw = ".";

        Current.scenes.Add(new StoryExporter.ExportedScene
        {
            index = sceneIndex,
            data = newScene
        });

        // Add blank sprite
        if (Current.sprites == null)
            Current.sprites = new Dictionary<string, string>();
        Current.sprites[$"{sceneIndex}.1.png"] = BlankSpriteBase64;

        // Update scene count
        if (Current.story != null)
            Current.story.sceneCount = sceneIndex;

        IsDirty = true;
        Debug.Log($"WorkingStory: Created scene {sceneIndex}");
    }

    /// <summary>
    /// Deletes a scene from the story and reindexes subsequent scenes.
    /// </summary>
    public static void DeleteScene(int sceneIndex)
    {
        if (Current == null)
        {
            Debug.LogError("WorkingStory: No story loaded");
            return;
        }

        if (Current.scenes == null || Current.scenes.Count == 0)
        {
            Debug.LogError("WorkingStory: No scenes to delete");
            return;
        }

        // Remove the scene data
        Current.scenes.RemoveAll(s => s.index == sceneIndex);

        // Remove associated sprites
        if (Current.sprites != null)
        {
            var spritesToRemove = new List<string>();
            foreach (var key in Current.sprites.Keys)
            {
                if (key.StartsWith($"{sceneIndex}.") && key.EndsWith(".png"))
                {
                    spritesToRemove.Add(key);
                }
            }
            foreach (var key in spritesToRemove)
            {
                // Clear from cache
                if (_spriteCache.ContainsKey(key))
                {
                    var sprite = _spriteCache[key];
                    if (sprite != null && sprite.texture != null)
                    {
                        UnityEngine.Object.Destroy(sprite.texture);
                        UnityEngine.Object.Destroy(sprite);
                    }
                    _spriteCache.Remove(key);
                }
                Current.sprites.Remove(key);
            }
        }

        // Reindex subsequent scenes
        foreach (var scene in Current.scenes)
        {
            if (scene.index > sceneIndex)
            {
                int oldIndex = scene.index;
                int newIndex = oldIndex - 1;

                // Rename sprites for this scene
                if (Current.sprites != null)
                {
                    var spritesToRename = new List<string>();
                    foreach (var key in Current.sprites.Keys)
                    {
                        if (key.StartsWith($"{oldIndex}.") && key.EndsWith(".png"))
                        {
                            spritesToRename.Add(key);
                        }
                    }
                    foreach (var oldKey in spritesToRename)
                    {
                        string newKey = oldKey.Replace($"{oldIndex}.", $"{newIndex}.");
                        Current.sprites[newKey] = Current.sprites[oldKey];
                        Current.sprites.Remove(oldKey);

                        // Update cache key if cached
                        if (_spriteCache.ContainsKey(oldKey))
                        {
                            _spriteCache[newKey] = _spriteCache[oldKey];
                            _spriteCache.Remove(oldKey);
                        }
                    }
                }

                scene.index = newIndex;
            }
        }

        // Update scene count
        if (Current.story != null)
        {
            Current.story.sceneCount = Mathf.Max(0, Current.story.sceneCount - 1);
        }

        IsDirty = true;
        Debug.Log($"WorkingStory: Deleted scene {sceneIndex}");
    }

    /// <summary>
    /// Updates the story title.
    /// </summary>
    public static void SetTitle(string title)
    {
        if (Current?.story != null)
        {
            Current.story.title = title;
            Current.story.id = SanitizeId(title);
            IsDirty = true;
        }
    }

    /// <summary>
    /// Updates the scene count.
    /// </summary>
    public static void SetSceneCount(int count)
    {
        if (Current?.story != null)
        {
            Current.story.sceneCount = count;
            IsDirty = true;
        }
    }

    /// <summary>
    /// Updates the credits text.
    /// </summary>
    public static void SetCredits(string credits)
    {
        if (Current != null)
        {
            Current.credits = credits;
            IsDirty = true;
        }
    }

    #endregion

    #region Reading

    /// <summary>
    /// Loads scene data from the working story.
    /// </summary>
    public static SceneData LoadScene(int sceneIndex)
    {
        if (Current?.scenes == null)
            return SceneDataParser.CreateGlitchScene();

        var scene = Current.scenes.Find(s => s.index == sceneIndex);
        return scene?.data ?? SceneDataParser.CreateGlitchScene();
    }

    /// <summary>
    /// Loads a sprite from the working story.
    /// </summary>
    public static Sprite LoadSprite(string spriteName, int width = 320, int height = 130)
    {
        if (_spriteCache.TryGetValue(spriteName, out Sprite cached))
            return cached;

        if (Current?.sprites == null || !Current.sprites.ContainsKey(spriteName))
            return null;

        try
        {
            string base64 = Current.sprites[spriteName];

            // Handle data URL prefix
            if (base64.StartsWith("data:"))
            {
                int comma = base64.IndexOf(',');
                if (comma > 0) base64 = base64.Substring(comma + 1);
            }

            byte[] bytes = Convert.FromBase64String(base64);
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.LoadImage(bytes);

            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
            _spriteCache[spriteName] = sprite;

            return sprite;
        }
        catch (Exception e)
        {
            Debug.LogError($"WorkingStory: Failed to load sprite {spriteName}: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Loads all sprites for a scene (animation frames).
    /// </summary>
    public static List<Sprite> LoadSceneSprites(int sceneIndex)
    {
        var sprites = new List<Sprite>();

        int frame = 1;
        while (true)
        {
            string name = $"{sceneIndex}.{frame}.png";
            if (Current?.sprites == null || !Current.sprites.ContainsKey(name))
                break;

            var sprite = LoadSprite(name);
            if (sprite != null)
                sprites.Add(sprite);

            frame++;
        }

        return sprites;
    }

    /// <summary>
    /// Gets the credits text.
    /// </summary>
    public static string GetCredits()
    {
        if (Current == null) return "";
        return (Current.story?.title ?? "") + "\n" + (Current.credits ?? "");
    }

    #endregion

    #region Export/Save

    /// <summary>
    /// Exports the current story to JSON string.
    /// </summary>
    public static string ExportToJson()
    {
        if (Current == null)
        {
            Debug.LogError("WorkingStory: No story loaded");
            return null;
        }

        Current.exportedAt = DateTime.UtcNow.ToString("o");
        return JsonConvert.SerializeObject(Current, Formatting.Indented);
    }

    /// <summary>
    /// Marks the story as saved to a file path.
    /// </summary>
    public static void MarkSaved(string path)
    {
        LastSavePath = path;
        IsDirty = false;
        Debug.Log($"WorkingStory: Marked as saved to {path}");
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Clears the current story and all caches.
    /// </summary>
    public static void Clear()
    {
        ClearSpriteCache();
        Current = null;
        IsOfficial = false;
        IsDirty = false;
        LastSavePath = null;
        CurrentSceneIndex = 1;
    }

    /// <summary>
    /// Clears the sprite cache to free memory.
    /// </summary>
    public static void ClearSpriteCache()
    {
        foreach (var sprite in _spriteCache.Values)
        {
            if (sprite != null && sprite.texture != null)
            {
                UnityEngine.Object.Destroy(sprite.texture);
                UnityEngine.Object.Destroy(sprite);
            }
        }
        _spriteCache.Clear();
    }

    #endregion

    #region Helpers

    private static string SanitizeId(string title)
    {
        if (string.IsNullOrEmpty(title)) return "story";
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            title = title.Replace(c, '_');
        return title.Replace(' ', '_').ToLowerInvariant();
    }

    private static Texture2D MakeTextureReadable(Texture2D source)
    {
        if (source.isReadable) return source;

        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height);
        Graphics.Blit(source, rt);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readable.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        return readable;
    }

    #endregion
}
