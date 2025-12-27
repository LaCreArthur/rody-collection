using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Story provider that loads directly from .rody.json files.
/// No intermediate folder structure needed - reads JSON and decodes sprites on demand.
/// </summary>
public class JsonStoryProvider : IStoryProvider
{
    private string jsonFilePath;
    private StoryExporter.ExportedStory cachedStory;
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    public JsonStoryProvider(string jsonPath)
    {
        jsonFilePath = jsonPath;
        LoadJson();
    }

    private void LoadJson()
    {
        if (!File.Exists(jsonFilePath))
        {
            Debug.LogError($"JsonStoryProvider: File not found: {jsonFilePath}");
            return;
        }

        try
        {
            string json = File.ReadAllText(jsonFilePath);
            cachedStory = JsonConvert.DeserializeObject<StoryExporter.ExportedStory>(json);
            Debug.Log($"JsonStoryProvider: Loaded '{cachedStory?.story?.title}' with {cachedStory?.scenes?.Count} scenes");
        }
        catch (Exception e)
        {
            Debug.LogError($"JsonStoryProvider: Failed to parse JSON: {e.Message}");
        }
    }

    public List<StoryMetadata> GetStories()
    {
        var list = new List<StoryMetadata>();
        if (cachedStory?.story != null)
        {
            var meta = new StoryMetadata(cachedStory.story.id)
            {
                title = cachedStory.story.title,
                sceneCount = cachedStory.story.sceneCount
            };
            list.Add(meta);
        }
        return list;
    }

    public int GetSceneCount(string storyId)
    {
        return cachedStory?.story?.sceneCount ?? 0;
    }

    public SceneData LoadScene(string storyId, int sceneIndex)
    {
        if (cachedStory?.scenes == null)
        {
            Debug.LogError("JsonStoryProvider: No scenes loaded");
            return SceneDataParser.CreateGlitchScene();
        }

        // Find scene by index (scenes are 1-indexed in the game)
        var scene = cachedStory.scenes.Find(s => s.index == sceneIndex);
        if (scene == null)
        {
            Debug.LogError($"JsonStoryProvider: Scene {sceneIndex} not found");
            return SceneDataParser.CreateGlitchScene();
        }

        return scene.data ?? SceneDataParser.CreateGlitchScene();
    }

    public Sprite LoadSprite(string storyId, string spriteName, int width, int height)
    {
        // Check cache first
        if (spriteCache.TryGetValue(spriteName, out Sprite cached))
        {
            return cached;
        }

        if (cachedStory?.sprites == null || !cachedStory.sprites.ContainsKey(spriteName))
        {
            Debug.LogWarning($"JsonStoryProvider: Sprite not found: {spriteName}");
            return null;
        }

        try
        {
            string base64 = cachedStory.sprites[spriteName];

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
            tex.filterMode = FilterMode.Point; // Pixel art
            tex.LoadImage(imageBytes);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f); // 1 pixel per unit for pixel art

            // Cache it
            spriteCache[spriteName] = sprite;

            return sprite;
        }
        catch (Exception e)
        {
            Debug.LogError($"JsonStoryProvider: Failed to decode sprite {spriteName}: {e.Message}");
            return null;
        }
    }

    public List<Sprite> LoadSceneSprites(string storyId, int sceneIndex)
    {
        var sprites = new List<Sprite>();

        // Scene sprites are named: {sceneIndex}.1.png, {sceneIndex}.2.png, etc.
        int frame = 1;
        while (true)
        {
            string spriteName = $"{sceneIndex}.{frame}.png";

            if (cachedStory?.sprites == null || !cachedStory.sprites.ContainsKey(spriteName))
            {
                break; // No more frames
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
        if (cachedStory == null)
            return "";

        // Credits format: first line is title, rest is credits
        string title = cachedStory.story?.title ?? "";
        string credits = cachedStory.credits ?? "";

        return title + "\n" + credits;
    }

    public void SaveStory(string storyId, StoryData story)
    {
        // For now, saving to JSON is handled by StoryExporter
        // This could be implemented later for in-place editing
        Debug.LogWarning("JsonStoryProvider: SaveStory not implemented - use StoryExporter");
    }

    #region Save Methods

    /// <summary>
    /// Saves scene data to the cached story.
    /// Call WriteToFile() after all changes to persist.
    /// </summary>
    public void SaveScene(int sceneIndex, SceneData data)
    {
        if (cachedStory == null)
        {
            Debug.LogError("JsonStoryProvider: No story loaded");
            return;
        }

        if (cachedStory.scenes == null)
            cachedStory.scenes = new List<StoryExporter.ExportedScene>();

        // Find existing scene or create new one
        var existingScene = cachedStory.scenes.Find(s => s.index == sceneIndex);
        if (existingScene != null)
        {
            existingScene.data = data;
        }
        else
        {
            cachedStory.scenes.Add(new StoryExporter.ExportedScene
            {
                index = sceneIndex,
                data = data
            });
        }

        Debug.Log($"JsonStoryProvider: Updated scene {sceneIndex} in cache");
    }

    /// <summary>
    /// Saves a sprite to the cached story as base64.
    /// Call WriteToFile() after all changes to persist.
    /// </summary>
    public void SaveSprite(string spriteName, Texture2D texture)
    {
        if (cachedStory == null)
        {
            Debug.LogError("JsonStoryProvider: No story loaded");
            return;
        }

        if (cachedStory.sprites == null)
            cachedStory.sprites = new Dictionary<string, string>();

        try
        {
            // Make texture readable if needed
            Texture2D readableTex = MakeTextureReadable(texture);
            byte[] pngData = readableTex.EncodeToPNG();

            if (readableTex != texture)
                UnityEngine.Object.Destroy(readableTex);

            string base64 = Convert.ToBase64String(pngData);
            cachedStory.sprites[spriteName] = base64;

            // Update sprite cache
            if (spriteCache.ContainsKey(spriteName))
            {
                var oldSprite = spriteCache[spriteName];
                if (oldSprite != null && oldSprite.texture != null)
                {
                    UnityEngine.Object.Destroy(oldSprite.texture);
                    UnityEngine.Object.Destroy(oldSprite);
                }
                spriteCache.Remove(spriteName);
            }

            Debug.Log($"JsonStoryProvider: Updated sprite {spriteName} in cache");
        }
        catch (Exception e)
        {
            Debug.LogError($"JsonStoryProvider: Failed to encode sprite {spriteName}: {e.Message}");
        }
    }

    /// <summary>
    /// Updates the scene count in the story metadata.
    /// </summary>
    public void UpdateSceneCount(int count)
    {
        if (cachedStory?.story != null)
        {
            cachedStory.story.sceneCount = count;
        }
    }

    /// <summary>
    /// Creates a new scene by copying data from the previous scene.
    /// Used when adding a new scene in the editor.
    /// </summary>
    public void CreateNewScene(int sceneIndex)
    {
        if (cachedStory == null)
        {
            Debug.LogError("JsonStoryProvider: No story loaded");
            return;
        }

        // Check if scene already exists
        var existingScene = cachedStory.scenes?.Find(s => s.index == sceneIndex);
        if (existingScene != null)
        {
            Debug.Log($"JsonStoryProvider: Scene {sceneIndex} already exists");
            return;
        }

        // Get the previous scene as template
        int templateIndex = sceneIndex - 1;
        var templateScene = cachedStory.scenes?.Find(s => s.index == templateIndex);

        SceneData newSceneData;
        if (templateScene?.data != null)
        {
            // Copy from template (deep copy by creating new instance)
            newSceneData = new SceneData
            {
                titleText = "Nouveau titre",
                introText = "Nouveau texte d'introduction",
                introDial1 = templateScene.data.introDial1 ?? ".",
                introDial2 = templateScene.data.introDial2 ?? ".",
                introDial3 = templateScene.data.introDial3 ?? ".",
                objDial = ".",
                ngpDial = ".",
                fswDial = ".",
                objText = ".",
                ngpText = ".",
                fswText = ".",
                musicIntro = templateScene.data.musicIntro ?? "",
                musicLoop = templateScene.data.musicLoop ?? "",
                pitch1 = 1f,
                pitch2 = 1f,
                pitch3 = 1f,
                isMastico1 = false,
                isMastico2 = false,
                isMastico3 = false,
                isZambla = false,
                objPos = "0",
                objSize = "0,0",
                objNearPos = "0",
                objNearSize = "0,0",
                ngpPos = "0",
                ngpSize = "0,0",
                ngpNearPos = "0",
                ngpNearSize = "0,0",
                fswPos = "0",
                fswSize = "0,0",
                fswNearPos = "0",
                fswNearSize = "0,0"
            };
        }
        else
        {
            // Create default scene data
            newSceneData = SceneDataParser.CreateGlitchScene();
            newSceneData.titleText = "Nouveau titre";
            newSceneData.introText = "Nouveau texte d'introduction";
        }

        // Add to scenes list
        if (cachedStory.scenes == null)
            cachedStory.scenes = new List<StoryExporter.ExportedScene>();

        cachedStory.scenes.Add(new StoryExporter.ExportedScene
        {
            index = sceneIndex,
            data = newSceneData
        });

        // Update scene count
        UpdateSceneCount(sceneIndex);

        Debug.Log($"JsonStoryProvider: Created new scene {sceneIndex} from template");
    }

    /// <summary>
    /// Writes the cached story back to the JSON file.
    /// </summary>
    public bool WriteToFile()
    {
        if (cachedStory == null)
        {
            Debug.LogError("JsonStoryProvider: No story to save");
            return false;
        }

        try
        {
            string json = JsonConvert.SerializeObject(cachedStory, Formatting.Indented);
            File.WriteAllText(jsonFilePath, json);
            Debug.Log($"JsonStoryProvider: Saved to {jsonFilePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"JsonStoryProvider: Failed to write file: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Makes a texture readable for encoding.
    /// </summary>
    private static Texture2D MakeTextureReadable(Texture2D source)
    {
        if (source.isReadable)
            return source;

        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height);
        Graphics.Blit(source, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readable.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return readable;
    }

    #endregion

    public bool StoryExists(string storyId)
    {
        return cachedStory != null && cachedStory.story?.id == storyId;
    }

    /// <summary>
    /// Gets the cached story data for direct access.
    /// </summary>
    public StoryExporter.ExportedStory GetCachedStory()
    {
        return cachedStory;
    }

    /// <summary>
    /// Clears the sprite cache to free memory.
    /// </summary>
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
