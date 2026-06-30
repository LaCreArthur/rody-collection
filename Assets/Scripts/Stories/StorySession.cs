using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The single runtime source of truth for the loaded story: one owned Story,
/// one scene cursor, one dirty flag, one provenance tag. Plain C# (no
/// MonoBehaviour). Exposes the gameplay read contract and all mutation, and
/// raises DirtyChanged so the platform layer can react without the model
/// calling into it.
/// </summary>
public class StorySession
{
    /// <summary>The currently loaded story, or null.</summary>
    public Story Current { get; private set; }

    /// <summary>Provenance of the loaded story. Read only by the edit affordance.</summary>
    public StorySource Source { get; private set; }

    bool _isDirty;
    /// <summary>True when the story changed since load/last save.</summary>
    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (_isDirty == value) return;
            _isDirty = value;
            DirtyChanged?.Invoke(_isDirty);
        }
    }

    /// <summary>Raised whenever the dirty flag flips. The platform layer subscribes;
    /// the model never calls the platform layer directly.</summary>
    public event Action<bool> DirtyChanged;

    /// <summary>Last file path this story was exported/saved to (null = never).</summary>
    public string LastSavePath { get; private set; }

    public bool IsLoaded => Current != null;
    public bool IsOfficial => IsLoaded && Source == StorySource.Builtin;
    public bool IsUserStory => IsLoaded && Source == StorySource.User;
    public string Title => Current?.story?.title ?? "Sans titre";
    public string Id => Current?.story?.id ?? "";
    public int SceneCount => Current?.story?.sceneCount ?? 0;

    /// <summary>Current scene index being played/edited (1-based).</summary>
    public int CurrentSceneIndex { get; set; } = 1;

    // The one sprite decoder/cache for this session's loaded story.
    readonly SpriteCache _sprites = new SpriteCache();

    // Pre-computed blank sprite base64 (320x130 for scenes).
    static string _blankSpriteBase64;
    static string BlankSpriteBase64
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

    // Pre-computed blank title sprite base64 (320x200 for title screen).
    static string _blankTitleSpriteBase64;
    static string BlankTitleSpriteBase64
    {
        get
        {
            if (_blankTitleSpriteBase64 == null)
            {
                var tex = new Texture2D(320, 200, TextureFormat.RGBA32, false);
                var pixels = new Color[320 * 200];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
                tex.SetPixels(pixels);
                tex.Apply();
                _blankTitleSpriteBase64 = Convert.ToBase64String(tex.EncodeToPNG());
                UnityEngine.Object.Destroy(tex);
            }
            return _blankTitleSpriteBase64;
        }
    }

    #region Loading

    /// <summary>
    /// Loads a fully materialized story into the session. The caller (catalog /
    /// shim) owns where the story came from and hands it in with its source tag.
    /// </summary>
    public void Load(Story story, StorySource source)
    {
        Clear();
        Current = story;
        Source = source;
        IsDirty = false;
        LastSavePath = null;
        Debug.Log($"StorySession: Loaded '{Title}' ({source})");
    }

    /// <summary>
    /// Loads from a JSON string (imported file). Loads as a User story.
    /// </summary>
    public void LoadFromJson(string json, string savePath = null)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("StorySession: Empty JSON");
            return;
        }

        try
        {
            var parsed = StoryJson.Deserialize(json);

            if (parsed == null ||
                parsed.story == null ||
                string.IsNullOrEmpty(parsed.story.id) ||
                string.IsNullOrEmpty(parsed.story.title) ||
                parsed.scenes == null)
            {
                Debug.LogError("StorySession: Invalid story format - missing required fields");
                return;
            }

            Load(parsed, StorySource.User);
            LastSavePath = savePath;
            Debug.Log($"StorySession: Loaded from JSON '{Title}'");
        }
        catch (Exception e)
        {
            Debug.LogError($"StorySession: Failed to parse JSON: {e.Message}");
        }
    }

    /// <summary>
    /// Creates a new blank story for editing (a User story).
    /// </summary>
    public void CreateNew(string title)
    {
        var story = new Story
        {
            formatVersion = 1,
            exportedAt = DateTime.UtcNow.ToString("o"),
            story = new StoryMeta
            {
                id = SanitizeId(title),
                title = title,
                sceneCount = 1
            },
            credits = "",
            scenes = new List<StoryScene>(),
            sprites = new Dictionary<string, string>()
        };

        var defaultScene = SceneDataParser.CreateGlitchScene();
        defaultScene.texts.title = "Premier tableau";
        defaultScene.texts.intro1 = "Texte d'introduction";
        defaultScene.texts.intro2 = "";
        defaultScene.texts.intro3 = "";

        story.scenes.Add(new StoryScene { index = 1, data = defaultScene });
        story.sprites["0.png"] = BlankTitleSpriteBase64;
        story.sprites["1.1.png"] = BlankSpriteBase64;

        Load(story, StorySource.User);
        IsDirty = true;
        Debug.Log($"StorySession: Created new story '{title}'");
    }

    #endregion

    #region Editing

    /// <summary>
    /// Forks the current story into an editable User copy (deep copy).
    /// No-op if it is already a User story.
    /// </summary>
    public void ForkForEditing()
    {
        if (Current == null)
        {
            Debug.LogError("StorySession: No story loaded");
            return;
        }

        if (Source != StorySource.Builtin)
        {
            Debug.Log("StorySession: Already a user story, no fork needed");
            return;
        }

        Current = Current.Clone();
        Current.story.title = Current.story.title + " (copie)";
        Current.story.id = SanitizeId(Current.story.title);
        Current.exportedAt = DateTime.UtcNow.ToString("o");

        Source = StorySource.User;
        IsDirty = true;
        ClearSpriteCache();
        Debug.Log($"StorySession: Forked to '{Title}'");
    }

    /// <summary>Updates scene data in the working story.</summary>
    public void SaveScene(int sceneIndex, SceneData data)
    {
        if (Current == null)
        {
            Debug.LogError("StorySession: No story loaded");
            return;
        }

        Current.scenes ??= new List<StoryScene>();

        var existing = Current.scenes.Find(s => s.index == sceneIndex);
        if (existing != null)
            existing.data = data;
        else
            Current.scenes.Add(new StoryScene { index = sceneIndex, data = data });

        IsDirty = true;
        Debug.Log($"StorySession: Updated scene {sceneIndex}");
    }

    /// <summary>Saves a sprite to the working story as base64.</summary>
    public void SaveSprite(string spriteName, Texture2D texture)
    {
        if (Current == null)
        {
            Debug.LogError("StorySession: No story loaded");
            return;
        }

        Current.sprites ??= new Dictionary<string, string>();

        try
        {
            Texture2D readableTex = TextureUtils.MakeReadable(texture);
            AtariPalette.ApplyPalette(readableTex);
            byte[] pngData = readableTex.EncodeToPNG();

            if (readableTex != texture)
                UnityEngine.Object.Destroy(readableTex);

            Current.sprites[spriteName] = Convert.ToBase64String(pngData);

            _sprites.Evict(spriteName);

            IsDirty = true;
            Debug.Log($"StorySession: Updated sprite {spriteName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"StorySession: Failed to save sprite: {e.Message}");
        }
    }

    /// <summary>Creates a new scene in the story.</summary>
    public void CreateNewScene(int sceneIndex)
    {
        if (Current == null)
        {
            Debug.LogError("StorySession: No story loaded");
            return;
        }

        Current.scenes ??= new List<StoryScene>();

        if (Current.scenes.Exists(s => s.index == sceneIndex))
        {
            Debug.Log($"StorySession: Scene {sceneIndex} already exists");
            return;
        }

        var newScene = new SceneData();
        newScene.texts.title = "Nouveau titre";
        newScene.texts.intro1 = "Nouveau texte d'introduction";
        newScene.texts.intro2 = "";
        newScene.texts.intro3 = "";
        newScene.texts.obj = ".";
        newScene.texts.ngp = ".";
        newScene.texts.fsw = ".";
        newScene.dialogues.intro1 = ".";
        newScene.dialogues.intro2 = ".";
        newScene.dialogues.intro3 = ".";
        newScene.dialogues.obj = ".";
        newScene.dialogues.ngp = ".";
        newScene.dialogues.fsw = ".";

        Current.scenes.Add(new StoryScene { index = sceneIndex, data = newScene });

        Current.sprites ??= new Dictionary<string, string>();
        Current.sprites[$"{sceneIndex}.1.png"] = BlankSpriteBase64;

        if (Current.story != null)
            Current.story.sceneCount = sceneIndex;

        IsDirty = true;
        Debug.Log($"StorySession: Created scene {sceneIndex}");
    }

    /// <summary>Deletes a scene and reindexes subsequent scenes.</summary>
    public void DeleteScene(int sceneIndex)
    {
        if (Current == null)
        {
            Debug.LogError("StorySession: No story loaded");
            return;
        }

        if (Current.scenes == null || Current.scenes.Count == 0)
        {
            Debug.LogError("StorySession: No scenes to delete");
            return;
        }

        Current.scenes.RemoveAll(s => s.index == sceneIndex);

        if (Current.sprites != null)
        {
            var spritesToRemove = new List<string>();
            foreach (var key in Current.sprites.Keys)
            {
                if (key.StartsWith($"{sceneIndex}.") && key.EndsWith(".png"))
                    spritesToRemove.Add(key);
            }
            foreach (var key in spritesToRemove)
            {
                _sprites.Evict(key);
                Current.sprites.Remove(key);
            }
        }

        foreach (var scene in Current.scenes)
        {
            if (scene.index > sceneIndex)
            {
                int oldIndex = scene.index;
                int newIndex = oldIndex - 1;

                if (Current.sprites != null)
                {
                    var spritesToRename = new List<string>();
                    foreach (var key in Current.sprites.Keys)
                    {
                        if (key.StartsWith($"{oldIndex}.") && key.EndsWith(".png"))
                            spritesToRename.Add(key);
                    }
                    foreach (var oldKey in spritesToRename)
                    {
                        string newKey = oldKey.Replace($"{oldIndex}.", $"{newIndex}.");
                        Current.sprites[newKey] = Current.sprites[oldKey];
                        Current.sprites.Remove(oldKey);

                        _sprites.Rename(oldKey, newKey);
                    }
                }

                scene.index = newIndex;
            }
        }

        if (Current.story != null)
            Current.story.sceneCount = Mathf.Max(0, Current.story.sceneCount - 1);

        IsDirty = true;
        Debug.Log($"StorySession: Deleted scene {sceneIndex}");
    }

    /// <summary>Updates the story title (and derived id).</summary>
    public void SetTitle(string title)
    {
        if (Current?.story != null)
        {
            Current.story.title = title;
            Current.story.id = SanitizeId(title);
            IsDirty = true;
        }
    }

    /// <summary>Updates the scene count.</summary>
    public void SetSceneCount(int count)
    {
        if (Current?.story != null)
        {
            Current.story.sceneCount = count;
            IsDirty = true;
        }
    }

    /// <summary>Updates the credits text.</summary>
    public void SetCredits(string credits)
    {
        if (Current != null)
        {
            Current.credits = credits;
            IsDirty = true;
        }
    }

    #endregion

    #region Reading

    /// <summary>Loads scene data from the working story.</summary>
    public SceneData LoadScene(int sceneIndex)
    {
        if (Current?.scenes == null)
            return SceneDataParser.CreateGlitchScene();

        var scene = Current.scenes.Find(s => s.index == sceneIndex);
        return scene?.data ?? SceneDataParser.CreateGlitchScene();
    }

    /// <summary>Loads a sprite from the working story (decoded + cached).</summary>
    public Sprite LoadSprite(string spriteName, int width = 320, int height = 130)
    {
        string base64 = null;
        Current?.sprites?.TryGetValue(spriteName, out base64);
        return _sprites.Get(spriteName, base64, width, height);
    }

    /// <summary>Loads all animation frames for a scene.</summary>
    public List<Sprite> LoadSceneSprites(int sceneIndex)
    {
        var sprites = new List<Sprite>();
        int frame = 1;
        while (true)
        {
            string name = SpriteCache.SceneFrameName(sceneIndex, frame);
            if (Current?.sprites == null || !Current.sprites.ContainsKey(name))
                break;

            var sprite = LoadSprite(name);
            if (sprite != null)
                sprites.Add(sprite);

            frame++;
        }
        return sprites;
    }

    /// <summary>Gets the credits text (title + credits).</summary>
    public string GetCredits()
    {
        if (Current == null) return "";
        return (Current.story?.title ?? "") + "\n" + (Current.credits ?? "");
    }

    #endregion

    #region Export/Save

    /// <summary>Exports the current story to JSON string.</summary>
    public string ExportToJson()
    {
        if (Current == null)
        {
            Debug.LogError("StorySession: No story loaded");
            return null;
        }

        Current.exportedAt = DateTime.UtcNow.ToString("o");
        return StoryJson.Serialize(Current);
    }

    /// <summary>Marks the story as saved to a file path (clears dirty).</summary>
    public void MarkSaved(string path)
    {
        LastSavePath = path;
        IsDirty = false;
        Debug.Log($"StorySession: Marked as saved to {path}");
    }

    #endregion

    #region Cleanup

    /// <summary>Clears the current story and all caches.</summary>
    public void Clear()
    {
        ClearSpriteCache();
        Current = null;
        Source = StorySource.Builtin;
        IsDirty = false;
        LastSavePath = null;
        CurrentSceneIndex = 1;
    }

    /// <summary>Clears the sprite cache to free memory.</summary>
    public void ClearSpriteCache() => _sprites.Clear();

    #endregion

    #region Helpers

    static string SanitizeId(string title)
    {
        if (string.IsNullOrEmpty(title)) return "story";
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            title = title.Replace(c, '_');
        return title.Replace(' ', '_').ToLowerInvariant();
    }

    #endregion
}
