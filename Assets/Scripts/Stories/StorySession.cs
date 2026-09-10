using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Owns the one editable workspace independently from the story being played.</summary>
public class StorySession
{
    public StoryWorkspace Workspace { get; private set; }
    public Story Draft => Workspace?.draft;
    public bool HasWorkspace => Draft != null;
    public bool IsDirty => Workspace?.isDirty ?? false;
    public event Action WorkspaceChanged;

    Story _builtin;
    bool _playingWorkspace;
    readonly SpriteCache _sprites = new SpriteCache();

    public Story Current => _playingWorkspace ? Draft : _builtin;
    public StorySource Source => _playingWorkspace ? StorySource.User : StorySource.Builtin;
    public bool IsLoaded => Current != null;
    public bool IsOfficial => IsLoaded && !_playingWorkspace;
    public string Title => Current?.story.title ?? "Sans titre";
    public string Id => Current?.story.id ?? "";
    public int SceneCount => Current?.scenes.Count ?? 0;
    public int CurrentSceneIndex { get; set; } = 1;

    public int EditorSceneIndex
    {
        get => Workspace?.editorSceneIndex ?? 0;
        set
        {
            if (!HasWorkspace) return;
            int index = Mathf.Clamp(value, 0, Draft.scenes.Count);
            if (Workspace.editorSceneIndex == index) return;
            Workspace.editorSceneIndex = index;
            WorkspaceChanged?.Invoke();
        }
    }

    public void LoadBuiltin(Story story)
    {
        _sprites.Clear();
        _builtin = story;
        _playingWorkspace = false;
        CurrentSceneIndex = 1;
    }

    public void ActivateWorkspace()
    {
        if (!HasWorkspace) throw new InvalidOperationException("Aucune histoire personnelle ouverte.");
        if (!_playingWorkspace) _sprites.Clear();
        _playingWorkspace = true;
        _builtin = null;
    }

    public void InstallWorkspace(Story story, int editorScene = 0)
    {
        Workspace = new StoryWorkspace
        {
            draft = story,
            restorePoint = story.Clone(),
            editorSceneIndex = Mathf.Clamp(editorScene, 0, story.scenes.Count)
        };
        _sprites.Clear();
        ActivateWorkspace();
        CurrentSceneIndex = 1;
        WorkspaceChanged?.Invoke();
    }

    public void RecoverWorkspace(StoryWorkspace workspace)
    {
        Workspace = workspace;
        if (workspace != null)
            workspace.editorSceneIndex = Mathf.Clamp(workspace.editorSceneIndex, 0, workspace.draft.scenes.Count);
        if (_playingWorkspace) _sprites.Clear();
        WorkspaceChanged?.Invoke();
    }

    public SceneData LoadDraftScene(int sceneIndex) => Draft.scenes.Find(s => s.index == sceneIndex).data;

    /// <summary>Called at the accepted typed edit, never by UI redraw or file/cache reads.</summary>
    public void NotifyEdited()
    {
        Workspace.isDirty = true;
        WorkspaceChanged?.Invoke();
    }

    public void AcceptSavedSnapshot(Story saved)
    {
        Workspace.restorePoint = saved;
        Workspace.isDirty = false;
        WorkspaceChanged?.Invoke();
    }

    public void DiscardChanges()
    {
        if (!IsDirty) return;
        Workspace.draft = Workspace.restorePoint.Clone();
        Workspace.isDirty = false;
        Workspace.editorSceneIndex = Mathf.Clamp(Workspace.editorSceneIndex, 0, Draft.scenes.Count);
        if (_playingWorkspace) _sprites.Clear();
        WorkspaceChanged?.Invoke();
    }

    public string SaveSprite(string spriteName, Texture2D texture)
    {
        Texture2D readable = null;
        try
        {
            readable = TextureUtils.MakeReadable(texture);
            AtariPalette.ApplyPalette(readable);
            string encoded = Convert.ToBase64String(readable.EncodeToPNG());
            if (Draft.sprites.TryGetValue(spriteName, out var previous) && previous == encoded) return null;
            Draft.sprites[spriteName] = encoded;
            if (_playingWorkspace) _sprites.Evict(spriteName);
            NotifyEdited();
            return null;
        }
        catch (Exception e) { return e.Message; }
        finally { if (readable != null) UnityEngine.Object.Destroy(readable); }
    }

    public int DraftFrameCount(int sceneIndex)
    {
        int count = 0;
        while (Draft.sprites.ContainsKey(SpriteCache.SceneFrameName(sceneIndex, count + 1))) count++;
        return count;
    }

    public void RemoveFrame(int sceneIndex, int frameIndex)
    {
        int count = DraftFrameCount(sceneIndex);
        if (frameIndex <= 1 || frameIndex > count) return;
        for (int i = frameIndex; i < count; i++)
            Draft.sprites[SpriteCache.SceneFrameName(sceneIndex, i)] = Draft.sprites[SpriteCache.SceneFrameName(sceneIndex, i + 1)];
        Draft.sprites.Remove(SpriteCache.SceneFrameName(sceneIndex, count));
        if (_playingWorkspace) _sprites.Clear();
        NotifyEdited();
    }

    public void CreateNewScene(int sceneIndex)
    {
        if (sceneIndex != Draft.scenes.Count + 1) return;
        var scene = new SceneData();
        scene.texts.title = "Nouveau tableau";
        Draft.scenes.Add(new StoryScene { index = sceneIndex, data = scene });
        Draft.story.sceneCount = Draft.scenes.Count;
        Draft.sprites[SpriteCache.SceneFrameName(sceneIndex, 1)] = BlankScene;
        NotifyEdited();
    }

    public void DeleteScene(int sceneIndex)
    {
        if (Draft.scenes.Count <= 1 || sceneIndex < 1 || sceneIndex > Draft.scenes.Count) return;
        Draft.scenes.RemoveAll(s => s.index == sceneIndex);
        foreach (string key in Draft.sprites.Keys.Where(k => k.StartsWith(sceneIndex + ".", StringComparison.Ordinal)).ToArray())
            Draft.sprites.Remove(key);
        foreach (var scene in Draft.scenes.Where(s => s.index > sceneIndex).OrderBy(s => s.index))
        {
            string prefix = scene.index + ".";
            foreach (string key in Draft.sprites.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToArray())
            {
                Draft.sprites[(scene.index - 1) + key.Substring(prefix.Length - 1)] = Draft.sprites[key];
                Draft.sprites.Remove(key);
            }
            scene.index--;
        }
        Draft.story.sceneCount = Draft.scenes.Count;
        Workspace.editorSceneIndex = Mathf.Clamp(Workspace.editorSceneIndex, 0, Draft.scenes.Count);
        if (_playingWorkspace) _sprites.Clear();
        NotifyEdited();
    }

    public SceneData LoadScene(int sceneIndex) => Current.scenes.Find(s => s.index == sceneIndex).data;

    public Sprite LoadSprite(string spriteName, int width = 320, int height = 130)
    {
        Current.sprites.TryGetValue(spriteName, out var base64);
        return _sprites.Get(spriteName, base64, width, height);
    }

    public List<Sprite> LoadSceneSprites(int sceneIndex)
    {
        var result = new List<Sprite>();
        for (int i = 1; Current.sprites.ContainsKey(SpriteCache.SceneFrameName(sceneIndex, i)); i++)
            result.Add(LoadSprite(SpriteCache.SceneFrameName(sceneIndex, i)));
        return result;
    }

    public string GetCredits() => (Current?.story.title ?? "") + "\n" + (Current?.credits ?? "");
    public void ClearSpriteCache() => _sprites.Clear();

    static string _blankScene;
    static string _blankTitle;
    static string BlankScene => _blankScene ??= BlankImage(320, 130);
    static string BlankTitle => _blankTitle ??= BlankImage(320, 200);

    static string BlankImage(int width, int height)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var pixels = new Color32[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
        texture.SetPixels32(pixels);
        texture.Apply();
        string encoded = Convert.ToBase64String(texture.EncodeToPNG());
        UnityEngine.Object.Destroy(texture);
        return encoded;
    }

    public static Story CreateNewStory(string title)
    {
        var scene = new SceneData();
        scene.texts.title = "Premier tableau";
        scene.texts.intro1 = "Texte d’introduction";
        return new Story
        {
            story = new StoryMeta { id = FileStem(title), title = title, sceneCount = 1 },
            credits = "",
            scenes = new List<StoryScene> { new StoryScene { index = 1, data = scene } },
            sprites = new Dictionary<string, string>
            {
                [SpriteCache.TitleName] = BlankTitle,
                [SpriteCache.SceneFrameName(1, 1)] = BlankScene
            }
        };
    }

    public static Story Duplicate(Story original)
    {
        var copy = original.Clone();
        copy.story.title += " (copie)";
        copy.story.id = FileStem(copy.story.title);
        return copy;
    }

    public static string FileStem(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return "histoire";
        foreach (char c in System.IO.Path.GetInvalidFileNameChars()) title = title.Replace(c, '_');
        return title.Replace('/', '_').Replace('\\', '_').Replace(' ', '_');
    }
}

/// <summary>One browser recovery record. This envelope is not the portable story format.</summary>
public class StoryWorkspace
{
    public Story draft;
    public Story restorePoint;
    public bool isDirty;
    public int editorSceneIndex;
}
