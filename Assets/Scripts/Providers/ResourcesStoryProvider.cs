using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Story provider that loads from Unity Resources folder.
/// Stories are embedded in the build - no HTTP requests needed.
/// Used for WebGL where StreamingAssets requires HTTP.
/// </summary>
public class ResourcesStoryProvider : IStoryProvider
{
    private string resourcesPath;
    private Dictionary<string, StoryExporter.ExportedStory> storiesCache = new Dictionary<string, StoryExporter.ExportedStory>();
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    public ResourcesStoryProvider(string path = "Stories")
    {
        resourcesPath = path;
        LoadAllStories();
    }

    private void LoadAllStories()
    {
        // Load index.json to get list of story files
        TextAsset indexAsset = Resources.Load<TextAsset>($"{resourcesPath}/index");
        if (indexAsset == null)
        {
            Debug.LogError($"ResourcesStoryProvider: Could not load {resourcesPath}/index.json");
            return;
        }

        try
        {
            var index = JsonConvert.DeserializeObject<StoryIndex>(indexAsset.text);
            Debug.Log($"ResourcesStoryProvider: Found {index.stories.Length} stories in index");

            foreach (string storyFile in index.stories)
            {
                // Remove .rody.json extension for Resources.Load
                string resourceName = storyFile.Replace(".rody.json", ".rody");
                TextAsset storyAsset = Resources.Load<TextAsset>($"{resourcesPath}/{resourceName}");

                if (storyAsset == null)
                {
                    Debug.LogWarning($"ResourcesStoryProvider: Could not load {resourcesPath}/{resourceName}");
                    continue;
                }

                var story = JsonConvert.DeserializeObject<StoryExporter.ExportedStory>(storyAsset.text);
                if (story?.story != null)
                {
                    storiesCache[story.story.id] = story;
                    Debug.Log($"ResourcesStoryProvider: Loaded '{story.story.title}' ({story.story.sceneCount} scenes)");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"ResourcesStoryProvider: Failed to parse index: {e.Message}");
        }
    }

    [Serializable]
    private class StoryIndex
    {
        public string[] stories;
    }

    public List<StoryMetadata> GetStories()
    {
        var list = new List<StoryMetadata>();
        foreach (var story in storiesCache.Values)
        {
            if (story?.story != null)
            {
                var meta = new StoryMetadata(story.story.id)
                {
                    title = story.story.title,
                    sceneCount = story.story.sceneCount
                };
                list.Add(meta);
            }
        }
        return list;
    }

    public int GetSceneCount(string storyId)
    {
        if (storiesCache.TryGetValue(storyId, out var story))
        {
            return story.story?.sceneCount ?? 0;
        }
        return 0;
    }

    public SceneData LoadScene(string storyId, int sceneIndex)
    {
        if (!storiesCache.TryGetValue(storyId, out var story) || story.scenes == null)
        {
            Debug.LogError($"ResourcesStoryProvider: Story not found: {storyId}");
            return SceneDataParser.CreateGlitchScene();
        }

        var scene = story.scenes.Find(s => s.index == sceneIndex);
        if (scene == null)
        {
            Debug.LogError($"ResourcesStoryProvider: Scene {sceneIndex} not found in {storyId}");
            return SceneDataParser.CreateGlitchScene();
        }

        return scene.data ?? SceneDataParser.CreateGlitchScene();
    }

    public Sprite LoadSprite(string storyId, string spriteName, int width, int height)
    {
        string cacheKey = $"{storyId}/{spriteName}";
        if (spriteCache.TryGetValue(cacheKey, out Sprite cached))
        {
            return cached;
        }

        if (!storiesCache.TryGetValue(storyId, out var story) || story.sprites == null)
        {
            return null;
        }

        if (!story.sprites.TryGetValue(spriteName, out string base64))
        {
            return null;
        }

        try
        {
            // Handle data URL prefix if present
            if (base64.StartsWith("data:"))
            {
                int commaIndex = base64.IndexOf(',');
                if (commaIndex > 0)
                {
                    base64 = base64.Substring(commaIndex + 1);
                }
            }

            byte[] imageBytes = Convert.FromBase64String(base64);
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.LoadImage(imageBytes);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
            spriteCache[cacheKey] = sprite;
            return sprite;
        }
        catch (Exception e)
        {
            Debug.LogError($"ResourcesStoryProvider: Failed to decode sprite {spriteName}: {e.Message}");
            return null;
        }
    }

    public List<Sprite> LoadSceneSprites(string storyId, int sceneIndex)
    {
        var sprites = new List<Sprite>();
        int frame = 1;

        while (true)
        {
            string spriteName = $"{sceneIndex}.{frame}.png";

            if (!storiesCache.TryGetValue(storyId, out var story) ||
                story.sprites == null ||
                !story.sprites.ContainsKey(spriteName))
            {
                break;
            }

            var sprite = LoadSprite(storyId, spriteName, 320, 130);
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
            frame++;
        }

        return sprites;
    }

    public string LoadCredits(string storyId)
    {
        if (!storiesCache.TryGetValue(storyId, out var story))
        {
            return "";
        }

        string title = story.story?.title ?? "";
        string credits = story.credits ?? "";
        return title + "\n" + credits;
    }

    public void SaveStory(string storyId, StoryData data)
    {
        Debug.LogWarning("ResourcesStoryProvider: SaveStory not supported - Resources are read-only");
    }

    public bool StoryExists(string storyId)
    {
        return storiesCache.ContainsKey(storyId);
    }

    public void ClearSpriteCache()
    {
        foreach (var sprite in spriteCache.Values)
        {
            if (sprite != null && sprite.texture != null)
            {
                UnityEngine.Object.Destroy(sprite.texture);
                UnityEngine.Object.Destroy(sprite);
            }
        }
        spriteCache.Clear();
    }
}
