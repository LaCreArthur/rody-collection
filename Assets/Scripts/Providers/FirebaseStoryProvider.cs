using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Firebase story provider using REST API.
/// Works on all platforms including WebGL.
/// Uses coroutines for async operations with callback pattern.
/// </summary>
public class FirebaseStoryProvider : IStoryProvider
{
    // Cache for loaded data
    private Dictionary<string, StoryMetadata> _storiesCache = new Dictionary<string, StoryMetadata>();
    private Dictionary<string, Dictionary<int, SceneData>> _scenesCache = new Dictionary<string, Dictionary<int, SceneData>>();
    private Dictionary<string, Sprite> _spritesCache = new Dictionary<string, Sprite>();
    private Dictionary<string, string> _creditsCache = new Dictionary<string, string>();

    private MonoBehaviour _coroutineRunner;

    public FirebaseStoryProvider(MonoBehaviour coroutineRunner)
    {
        _coroutineRunner = coroutineRunner;
    }

    #region IStoryProvider Implementation (Synchronous - uses cache)

    public List<StoryMetadata> GetStories()
    {
        return new List<StoryMetadata>(_storiesCache.Values);
    }

    public int GetSceneCount(string storyId)
    {
        if (_storiesCache.TryGetValue(storyId, out var metadata))
        {
            return metadata.sceneCount;
        }
        return 0;
    }

    public SceneData LoadScene(string storyId, int sceneIndex)
    {
        if (_scenesCache.TryGetValue(storyId, out var scenes))
        {
            if (scenes.TryGetValue(sceneIndex, out var scene))
            {
                return scene;
            }
        }
        Debug.LogWarning($"FirebaseStoryProvider: Scene {storyId}/{sceneIndex} not in cache");
        return SceneDataParser.CreateGlitchScene();
    }

    public Sprite LoadSprite(string storyId, string spriteName, int width, int height)
    {
        string key = $"{storyId}/sprites/{spriteName}";
        if (_spritesCache.TryGetValue(key, out var sprite))
        {
            return sprite;
        }
        Debug.LogWarning($"FirebaseStoryProvider: Sprite {key} not in cache");
        return null;
    }

    public List<Sprite> LoadSceneSprites(string storyId, int sceneIndex)
    {
        var sprites = new List<Sprite>();
        // Try to find all frames for this scene
        for (int i = 1; i <= 10; i++) // Max 10 frames per scene
        {
            string key = $"{storyId}/sprites/{sceneIndex}.{i}.png";
            if (_spritesCache.TryGetValue(key, out var sprite))
            {
                sprites.Add(sprite);
            }
            else
            {
                break; // No more frames
            }
        }
        return sprites;
    }

    public string LoadCredits(string storyId)
    {
        if (_creditsCache.TryGetValue(storyId, out var credits))
        {
            return credits;
        }
        return "";
    }

    public void SaveStory(string storyId, StoryData story)
    {
        // Delegate to async version (SaveStoryAsync handles coroutine internally)
        SaveStoryAsync(storyId, story, null);
    }

    public bool StoryExists(string storyId)
    {
        return _storiesCache.ContainsKey(storyId);
    }

    #endregion

    #region Async Methods (Callback-based)

    /// <summary>
    /// Loads all stories metadata from Firebase.
    /// Must be called before using synchronous methods.
    /// </summary>
    public void LoadStoriesAsync(Action<List<StoryMetadata>> onComplete, Action<string> onError = null)
    {
        _coroutineRunner.StartCoroutine(LoadStoriesCoroutine(onComplete, onError));
    }

    /// <summary>
    /// Loads a specific scene from Firebase and caches it.
    /// </summary>
    public void LoadSceneAsync(string storyId, int sceneIndex, Action<SceneData> onComplete, Action<string> onError = null)
    {
        _coroutineRunner.StartCoroutine(LoadSceneCoroutine(storyId, sceneIndex, onComplete, onError));
    }

    /// <summary>
    /// Loads a sprite from Firebase Storage and caches it.
    /// </summary>
    public void LoadSpriteAsync(string storyId, string spriteName, Action<Sprite> onComplete, Action<string> onError = null)
    {
        _coroutineRunner.StartCoroutine(LoadSpriteCoroutine(storyId, spriteName, onComplete, onError));
    }

    /// <summary>
    /// Loads all sprites for a scene.
    /// </summary>
    public void LoadSceneSpritesAsync(string storyId, int sceneIndex, Action<List<Sprite>> onComplete, Action<string> onError = null)
    {
        _coroutineRunner.StartCoroutine(LoadSceneSpritesCoroutine(storyId, sceneIndex, onComplete, onError));
    }

    /// <summary>
    /// Saves a story to Firebase.
    /// </summary>
    public void SaveStoryAsync(string storyId, StoryData story, Action onComplete, Action<string> onError = null)
    {
        _coroutineRunner.StartCoroutine(SaveStoryCoroutine(storyId, story, onComplete, onError));
    }

    /// <summary>
    /// Saves a single scene to Firebase.
    /// </summary>
    public void SaveSceneAsync(string storyId, int sceneIndex, SceneData scene, Action onComplete, Action<string> onError = null)
    {
        _coroutineRunner.StartCoroutine(SaveSceneCoroutine(storyId, sceneIndex, scene, onComplete, onError));
    }

    /// <summary>
    /// Uploads a sprite to Firebase Storage.
    /// </summary>
    public void UploadSpriteAsync(string storyId, string spriteName, byte[] pngData, Action<string> onComplete, Action<string> onError = null)
    {
        _coroutineRunner.StartCoroutine(UploadSpriteCoroutine(storyId, spriteName, pngData, onComplete, onError));
    }

    #endregion

    #region Coroutines

    private IEnumerator LoadStoriesCoroutine(Action<List<StoryMetadata>> onComplete, Action<string> onError)
    {
        string url = FirebaseConfig.GetCollectionUrl("stories");

        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Failed to load stories: {request.error}");
                yield break;
            }

            try
            {
                var response = JsonUtility.FromJson<FirestoreCollectionResponse>(request.downloadHandler.text);
                _storiesCache.Clear();

                if (response.documents != null)
                {
                    foreach (var doc in response.documents)
                    {
                        var metadata = ParseStoryMetadata(doc);
                        if (metadata != null)
                        {
                            _storiesCache[metadata.id] = metadata;
                        }
                    }
                }

                onComplete?.Invoke(new List<StoryMetadata>(_storiesCache.Values));
            }
            catch (Exception e)
            {
                onError?.Invoke($"Failed to parse stories: {e.Message}");
            }
        }
    }

    private IEnumerator LoadSceneCoroutine(string storyId, int sceneIndex, Action<SceneData> onComplete, Action<string> onError)
    {
        string url = FirebaseConfig.GetDocumentUrl($"stories/{storyId}/scenes/{sceneIndex}");

        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Failed to load scene: {request.error}");
                yield break;
            }

            try
            {
                var doc = JsonUtility.FromJson<FirestoreDocument>(request.downloadHandler.text);
                var scene = ParseSceneData(doc);

                // Cache it
                if (!_scenesCache.ContainsKey(storyId))
                {
                    _scenesCache[storyId] = new Dictionary<int, SceneData>();
                }
                _scenesCache[storyId][sceneIndex] = scene;

                onComplete?.Invoke(scene);
            }
            catch (Exception e)
            {
                onError?.Invoke($"Failed to parse scene: {e.Message}");
            }
        }
    }

    private IEnumerator LoadSpriteCoroutine(string storyId, string spriteName, Action<Sprite> onComplete, Action<string> onError)
    {
        string path = $"stories/{storyId}/sprites/{spriteName}";
        string url = FirebaseConfig.GetStorageDownloadUrl(path);
        string cacheKey = $"{storyId}/sprites/{spriteName}";

        // Check cache first
        if (_spritesCache.TryGetValue(cacheKey, out var cached))
        {
            onComplete?.Invoke(cached);
            yield break;
        }

        using (var request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Failed to load sprite {spriteName}: {request.error}");
                yield break;
            }

            try
            {
                var texture = DownloadHandlerTexture.GetContent(request);
                texture.filterMode = FilterMode.Point;
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                _spritesCache[cacheKey] = sprite;
                onComplete?.Invoke(sprite);
            }
            catch (Exception e)
            {
                onError?.Invoke($"Failed to create sprite: {e.Message}");
            }
        }
    }

    private IEnumerator LoadSceneSpritesCoroutine(string storyId, int sceneIndex, Action<List<Sprite>> onComplete, Action<string> onError)
    {
        var sprites = new List<Sprite>();
        bool hasMoreFrames = true;
        int frameIndex = 1;

        while (hasMoreFrames && frameIndex <= 10)
        {
            string spriteName = $"{sceneIndex}.{frameIndex}.png";
            bool frameLoaded = false;
            bool frameError = false;

            yield return LoadSpriteCoroutine(storyId, spriteName,
                sprite =>
                {
                    sprites.Add(sprite);
                    frameLoaded = true;
                },
                error =>
                {
                    frameError = true;
                    hasMoreFrames = false;
                });

            if (!frameLoaded && !frameError)
            {
                hasMoreFrames = false;
            }

            frameIndex++;
        }

        onComplete?.Invoke(sprites);
    }

    private IEnumerator SaveStoryCoroutine(string storyId, StoryData story, Action onComplete, Action<string> onError)
    {
        // Save metadata
        string url = FirebaseConfig.GetDocumentUrl($"stories/{storyId}");
        var metadataJson = BuildStoryMetadataJson(story.metadata, story.credits);

        using (var request = UnityWebRequest.Put(url, metadataJson))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Failed to save story metadata: {request.error}");
                yield break;
            }
        }

        // Update cache
        _storiesCache[storyId] = story.metadata;
        _creditsCache[storyId] = story.credits;

        onComplete?.Invoke();
    }

    private IEnumerator SaveSceneCoroutine(string storyId, int sceneIndex, SceneData scene, Action onComplete, Action<string> onError)
    {
        string url = FirebaseConfig.GetDocumentUrl($"stories/{storyId}/scenes/{sceneIndex}");
        var sceneJson = BuildSceneDataJson(scene);

        // Use PATCH to create or update
        using (var request = new UnityWebRequest(url, "PATCH"))
        {
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(sceneJson);
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Failed to save scene: {request.error}");
                yield break;
            }
        }

        // Update cache
        if (!_scenesCache.ContainsKey(storyId))
        {
            _scenesCache[storyId] = new Dictionary<int, SceneData>();
        }
        _scenesCache[storyId][sceneIndex] = scene;

        onComplete?.Invoke();
    }

    private IEnumerator UploadSpriteCoroutine(string storyId, string spriteName, byte[] pngData, Action<string> onComplete, Action<string> onError)
    {
        string path = $"stories/{storyId}/sprites/{spriteName}";
        string url = FirebaseConfig.GetStorageUploadUrl(path);

        using (var request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(pngData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "image/png");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Failed to upload sprite: {request.error}");
                yield break;
            }

            // Return the download URL
            string downloadUrl = FirebaseConfig.GetStorageDownloadUrl(path);
            onComplete?.Invoke(downloadUrl);
        }
    }

    #endregion

    #region JSON Parsing & Building

    private StoryMetadata ParseStoryMetadata(FirestoreDocument doc)
    {
        if (doc == null || doc.fields == null) return null;

        // Extract story ID from document name (format: projects/.../documents/stories/{id})
        string[] parts = doc.name.Split('/');
        string storyId = parts[parts.Length - 1];

        var metadata = new StoryMetadata(storyId);

        if (doc.fields.title != null)
            metadata.title = doc.fields.title.stringValue ?? storyId;

        if (doc.fields.sceneCount != null)
            int.TryParse(doc.fields.sceneCount.integerValue, out metadata.sceneCount);

        return metadata;
    }

    private SceneData ParseSceneData(FirestoreDocument doc)
    {
        if (doc == null || doc.fields == null)
            return SceneDataParser.CreateGlitchScene();

        var scene = new SceneData();

        // Parse dialogues
        if (doc.fields.dialogues?.mapValue?.fields != null)
        {
            var d = doc.fields.dialogues.mapValue.fields;
            scene.dialogues.intro1 = d.intro1?.stringValue ?? "";
            scene.dialogues.intro2 = d.intro2?.stringValue ?? "";
            scene.dialogues.intro3 = d.intro3?.stringValue ?? "";
            scene.dialogues.obj = d.obj?.stringValue ?? "";
            scene.dialogues.ngp = d.ngp?.stringValue ?? "";
            scene.dialogues.fsw = d.fsw?.stringValue ?? "";
        }

        // Parse texts
        if (doc.fields.texts?.mapValue?.fields != null)
        {
            var t = doc.fields.texts.mapValue.fields;
            scene.texts.title = t.title?.stringValue ?? "";
            scene.texts.intro = t.intro?.stringValue ?? "";
            scene.texts.obj = t.obj?.stringValue ?? "";
            scene.texts.ngp = t.ngp?.stringValue ?? "";
            scene.texts.fsw = t.fsw?.stringValue ?? "";
        }

        // Parse music
        if (doc.fields.music?.mapValue?.fields != null)
        {
            var m = doc.fields.music.mapValue.fields;
            scene.music.introMusic = m.introMusic?.stringValue ?? "i1";
            scene.music.sceneMusic = m.sceneMusic?.stringValue ?? "l1";
        }

        // Parse voice
        if (doc.fields.voice?.mapValue?.fields != null)
        {
            var v = doc.fields.voice.mapValue.fields;
            if (v.pitch1?.doubleValue != null) float.TryParse(v.pitch1.doubleValue, out scene.voice.pitch1);
            if (v.pitch2?.doubleValue != null) float.TryParse(v.pitch2.doubleValue, out scene.voice.pitch2);
            if (v.pitch3?.doubleValue != null) float.TryParse(v.pitch3.doubleValue, out scene.voice.pitch3);
            scene.voice.isMastico1 = v.isMastico1?.booleanValue ?? false;
            scene.voice.isMastico2 = v.isMastico2?.booleanValue ?? false;
            scene.voice.isMastico3 = v.isMastico3?.booleanValue ?? false;
            scene.voice.isZambla = v.isZambla?.booleanValue ?? false;
        }

        // Parse objects
        if (doc.fields.objects?.mapValue?.fields != null)
        {
            var o = doc.fields.objects.mapValue.fields;
            ParseObjectZone(o.obj?.mapValue?.fields, scene.objects.obj);
            ParseObjectZone(o.ngp?.mapValue?.fields, scene.objects.ngp);
            ParseObjectZone(o.fsw?.mapValue?.fields, scene.objects.fsw);
        }

        return scene;
    }

    private void ParseObjectZone(FirestoreFields fields, ObjectZone zone)
    {
        if (fields == null) return;
        zone.positionRaw = fields.positionRaw?.stringValue ?? "";
        zone.sizeRaw = fields.sizeRaw?.stringValue ?? "";
        zone.nearPositionRaw = fields.nearPositionRaw?.stringValue ?? "";
        zone.nearSizeRaw = fields.nearSizeRaw?.stringValue ?? "";
    }

    private string BuildStoryMetadataJson(StoryMetadata metadata, string credits)
    {
        return $@"{{
            ""fields"": {{
                ""title"": {{ ""stringValue"": ""{EscapeJson(metadata.title)}"" }},
                ""sceneCount"": {{ ""integerValue"": ""{metadata.sceneCount}"" }},
                ""credits"": {{ ""stringValue"": ""{EscapeJson(credits)}"" }}
            }}
        }}";
    }

    private string BuildSceneDataJson(SceneData scene)
    {
        return $@"{{
            ""fields"": {{
                ""dialogues"": {{
                    ""mapValue"": {{
                        ""fields"": {{
                            ""intro1"": {{ ""stringValue"": ""{EscapeJson(scene.dialogues.intro1)}"" }},
                            ""intro2"": {{ ""stringValue"": ""{EscapeJson(scene.dialogues.intro2)}"" }},
                            ""intro3"": {{ ""stringValue"": ""{EscapeJson(scene.dialogues.intro3)}"" }},
                            ""obj"": {{ ""stringValue"": ""{EscapeJson(scene.dialogues.obj)}"" }},
                            ""ngp"": {{ ""stringValue"": ""{EscapeJson(scene.dialogues.ngp)}"" }},
                            ""fsw"": {{ ""stringValue"": ""{EscapeJson(scene.dialogues.fsw)}"" }}
                        }}
                    }}
                }},
                ""texts"": {{
                    ""mapValue"": {{
                        ""fields"": {{
                            ""title"": {{ ""stringValue"": ""{EscapeJson(scene.texts.title)}"" }},
                            ""intro"": {{ ""stringValue"": ""{EscapeJson(scene.texts.intro)}"" }},
                            ""obj"": {{ ""stringValue"": ""{EscapeJson(scene.texts.obj)}"" }},
                            ""ngp"": {{ ""stringValue"": ""{EscapeJson(scene.texts.ngp)}"" }},
                            ""fsw"": {{ ""stringValue"": ""{EscapeJson(scene.texts.fsw)}"" }}
                        }}
                    }}
                }},
                ""music"": {{
                    ""mapValue"": {{
                        ""fields"": {{
                            ""introMusic"": {{ ""stringValue"": ""{EscapeJson(scene.music.introMusic)}"" }},
                            ""sceneMusic"": {{ ""stringValue"": ""{EscapeJson(scene.music.sceneMusic)}"" }}
                        }}
                    }}
                }},
                ""voice"": {{
                    ""mapValue"": {{
                        ""fields"": {{
                            ""pitch1"": {{ ""doubleValue"": {scene.voice.pitch1} }},
                            ""pitch2"": {{ ""doubleValue"": {scene.voice.pitch2} }},
                            ""pitch3"": {{ ""doubleValue"": {scene.voice.pitch3} }},
                            ""isMastico1"": {{ ""booleanValue"": {scene.voice.isMastico1.ToString().ToLower()} }},
                            ""isMastico2"": {{ ""booleanValue"": {scene.voice.isMastico2.ToString().ToLower()} }},
                            ""isMastico3"": {{ ""booleanValue"": {scene.voice.isMastico3.ToString().ToLower()} }},
                            ""isZambla"": {{ ""booleanValue"": {scene.voice.isZambla.ToString().ToLower()} }}
                        }}
                    }}
                }},
                ""objects"": {{
                    ""mapValue"": {{
                        ""fields"": {{
                            ""obj"": {BuildObjectZoneJson(scene.objects.obj)},
                            ""ngp"": {BuildObjectZoneJson(scene.objects.ngp)},
                            ""fsw"": {BuildObjectZoneJson(scene.objects.fsw)}
                        }}
                    }}
                }}
            }}
        }}";
    }

    private string BuildObjectZoneJson(ObjectZone zone)
    {
        return $@"{{
            ""mapValue"": {{
                ""fields"": {{
                    ""positionRaw"": {{ ""stringValue"": ""{EscapeJson(zone.positionRaw)}"" }},
                    ""sizeRaw"": {{ ""stringValue"": ""{EscapeJson(zone.sizeRaw)}"" }},
                    ""nearPositionRaw"": {{ ""stringValue"": ""{EscapeJson(zone.nearPositionRaw)}"" }},
                    ""nearSizeRaw"": {{ ""stringValue"": ""{EscapeJson(zone.nearSizeRaw)}"" }}
                }}
            }}
        }}";
    }

    private string EscapeJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
    }

    #endregion

    #region Firestore JSON Models

    [Serializable]
    private class FirestoreCollectionResponse
    {
        public FirestoreDocument[] documents;
    }

    [Serializable]
    private class FirestoreDocument
    {
        public string name;
        public FirestoreFields fields;
    }

    [Serializable]
    private class FirestoreFields
    {
        // Story metadata fields
        public FirestoreValue title;
        public FirestoreValue sceneCount;
        public FirestoreValue credits;

        // Scene fields (nested maps)
        public FirestoreMapValue dialogues;
        public FirestoreMapValue texts;
        public FirestoreMapValue music;
        public FirestoreMapValue voice;
        public FirestoreMapValue objects;

        // ObjectZone fields
        public FirestoreValue positionRaw;
        public FirestoreValue sizeRaw;
        public FirestoreValue nearPositionRaw;
        public FirestoreValue nearSizeRaw;

        // Voice fields
        public FirestoreValue pitch1;
        public FirestoreValue pitch2;
        public FirestoreValue pitch3;
        public FirestoreValue isMastico1;
        public FirestoreValue isMastico2;
        public FirestoreValue isMastico3;
        public FirestoreValue isZambla;

        // Dialogue/text fields (string values)
        public FirestoreValue intro1;
        public FirestoreValue intro2;
        public FirestoreValue intro3;
        public FirestoreValue obj;
        public FirestoreValue ngp;
        public FirestoreValue fsw;
        public FirestoreValue intro;
        public FirestoreValue introMusic;
        public FirestoreValue sceneMusic;
    }

    [Serializable]
    private class FirestoreValue
    {
        public string stringValue;
        public string integerValue;
        public string doubleValue;
        public bool booleanValue;
        public FirestoreMap mapValue;  // For nested map values
    }

    [Serializable]
    private class FirestoreMapValue
    {
        public FirestoreMap mapValue;
    }

    [Serializable]
    private class FirestoreMap
    {
        public FirestoreFields fields;
    }

    #endregion
}
