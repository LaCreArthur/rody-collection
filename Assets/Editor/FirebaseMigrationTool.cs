using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;

/// <summary>
/// Editor tool to migrate stories from StreamingAssets to Firebase.
/// Access via menu: Tools > Firebase > Migration Tool
/// </summary>
public class FirebaseMigrationTool : EditorWindow
{
    private Vector2 _scrollPos;
    private List<string> _stories = new List<string>();
    private Dictionary<string, bool> _storySelection = new Dictionary<string, bool>();
    private bool _isUploading = false;
    private string _statusMessage = "";
    private float _progress = 0f;
    private int _currentStoryIndex = 0;
    private int _currentSceneIndex = 0;
    private bool _cleanBeforeUpload = true;

    [MenuItem("Tools/Firebase/Migration Tool")]
    public static void ShowWindow()
    {
        var window = GetWindow<FirebaseMigrationTool>("Firebase Migration");
        window.minSize = new Vector2(400, 500);
        window.RefreshStoryList();
    }

    private void OnGUI()
    {
        GUILayout.Label("Firebase Migration Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Firebase config info
        EditorGUILayout.HelpBox($"Project: {FirebaseConfig.ProjectId}\nStorage: {FirebaseConfig.StorageBucket}", MessageType.Info);
        EditorGUILayout.Space();

        // Refresh button
        if (GUILayout.Button("Refresh Story List"))
        {
            RefreshStoryList();
        }

        EditorGUILayout.Space();
        GUILayout.Label($"Found {_stories.Count} stories in StreamingAssets:", EditorStyles.label);

        // Story list with checkboxes
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(250));
        foreach (var story in _stories)
        {
            if (!_storySelection.ContainsKey(story))
                _storySelection[story] = false;

            _storySelection[story] = EditorGUILayout.ToggleLeft(story, _storySelection[story]);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // Select all / none buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select All"))
        {
            foreach (var story in _stories)
                _storySelection[story] = true;
        }
        if (GUILayout.Button("Select None"))
        {
            foreach (var story in _stories)
                _storySelection[story] = false;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Clean before upload option
        _cleanBeforeUpload = EditorGUILayout.ToggleLeft("Clean Storage before upload (recommended)", _cleanBeforeUpload);

        EditorGUILayout.Space();

        // Upload button
        EditorGUI.BeginDisabledGroup(_isUploading);
        if (GUILayout.Button("Upload Selected to Firebase", GUILayout.Height(40)))
        {
            StartUpload();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        // Clean all storage button
        EditorGUI.BeginDisabledGroup(_isUploading);
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("Clean ALL Storage (Delete all files)"))
        {
            if (EditorUtility.DisplayDialog("Clean All Storage",
                "This will DELETE ALL files in Firebase Storage.\n\nAre you sure?", "Yes, Delete All", "Cancel"))
            {
                StartCleanAllStorage();
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUI.EndDisabledGroup();

        // Progress bar
        if (_isUploading)
        {
            EditorGUILayout.Space();
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(GUILayout.Height(20)), _progress, _statusMessage);
        }
        else if (!string.IsNullOrEmpty(_statusMessage))
        {
            EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
        }

        EditorGUILayout.Space();

        // Test connection button
        if (GUILayout.Button("Test Firebase Connection"))
        {
            TestConnection();
        }
    }

    private void RefreshStoryList()
    {
        _stories.Clear();
        string streamingPath = Application.streamingAssetsPath;

        if (!Directory.Exists(streamingPath))
        {
            _statusMessage = "StreamingAssets folder not found!";
            return;
        }

        var directories = Directory.GetDirectories(streamingPath);
        foreach (var dir in directories)
        {
            var storyId = Path.GetFileName(dir);
            if (storyId.StartsWith(".")) continue;

            var levelsFile = Path.Combine(dir, "levels.rody");
            if (File.Exists(levelsFile))
            {
                _stories.Add(storyId);
            }
        }

        _statusMessage = $"Found {_stories.Count} stories";
    }

    private void StartUpload()
    {
        var selected = new List<string>();
        foreach (var kv in _storySelection)
        {
            if (kv.Value) selected.Add(kv.Key);
        }

        if (selected.Count == 0)
        {
            _statusMessage = "No stories selected!";
            return;
        }

        _isUploading = true;
        _progress = 0f;
        _currentStoryIndex = 0;

        EditorCoroutineRunner.StartCoroutine(UploadStoriesCoroutine(selected));
    }

    private IEnumerator UploadStoriesCoroutine(List<string> stories)
    {
        int totalStories = stories.Count;

        for (int i = 0; i < stories.Count; i++)
        {
            string storyId = stories[i];
            _currentStoryIndex = i;
            _statusMessage = $"Uploading {storyId}... ({i + 1}/{totalStories})";
            _progress = (float)i / totalStories;

            yield return UploadStoryCoroutine(storyId);
        }

        _isUploading = false;
        _progress = 1f;
        _statusMessage = $"Upload complete! {totalStories} stories uploaded.";
        Repaint();
    }

    private IEnumerator UploadStoryCoroutine(string storyId)
    {
        string storyPath = Path.Combine(Application.streamingAssetsPath, storyId);
        string levelsFile = Path.Combine(storyPath, "levels.rody");
        string creditsFile = Path.Combine(storyPath, "credits.txt");
        string spritesPath = Path.Combine(storyPath, "Sprites");

        // Clean old files first if option is enabled
        if (_cleanBeforeUpload)
        {
            _statusMessage = $"Cleaning old files for {storyId}...";
            Repaint();
            yield return CleanStoryStorageCoroutine(storyId);
        }

        // Count scenes
        int sceneCount = CountScenes(levelsFile);
        Debug.Log($"[Migration] {storyId}: {sceneCount} scenes");

        // Load credits
        string credits = "";
        string title = storyId;
        if (File.Exists(creditsFile))
        {
            credits = File.ReadAllText(creditsFile);
            using (var sr = new StreamReader(creditsFile))
            {
                title = sr.ReadLine() ?? storyId;
            }
        }

        // Upload story metadata
        yield return UploadStoryMetadata(storyId, title, sceneCount, credits);

        // Upload each scene
        for (int scene = 1; scene <= sceneCount; scene++)
        {
            _currentSceneIndex = scene;
            _statusMessage = $"Uploading {storyId} scene {scene}/{sceneCount}...";
            Repaint();

            var sceneData = LoadSceneFromFile(levelsFile, scene);
            yield return UploadScene(storyId, scene, sceneData);

            // Upload sprites for this scene
            yield return UploadSceneSprites(storyId, scene, spritesPath);
        }

        // Upload scene 0 sprite (title screen) if exists
        string titleSprite = Path.Combine(spritesPath, "0.png");
        if (File.Exists(titleSprite))
        {
            yield return UploadSprite(storyId, "0.png", titleSprite);
        }

        // Upload cover.png from Sprites folder
        string coverPath = Path.Combine(spritesPath, "cover.png");
        if (File.Exists(coverPath))
        {
            _statusMessage = $"Uploading {storyId} cover...";
            Repaint();
            yield return UploadSprite(storyId, "cover.png", coverPath);
        }

        Debug.Log($"[Migration] {storyId} complete!");
    }

    private IEnumerator UploadStoryMetadata(string storyId, string title, int sceneCount, string credits)
    {
        string url = FirebaseConfig.GetDocumentUrl($"stories/{storyId}");

        string json = $@"{{
            ""fields"": {{
                ""title"": {{ ""stringValue"": ""{EscapeJson(title)}"" }},
                ""sceneCount"": {{ ""integerValue"": ""{sceneCount}"" }},
                ""credits"": {{ ""stringValue"": ""{EscapeJson(credits)}"" }}
            }}
        }}";

        using (var request = new UnityWebRequest(url, "PATCH"))
        {
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Migration] Failed to upload metadata for {storyId}: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
            }
        }
    }

    private IEnumerator UploadScene(string storyId, int sceneIndex, SceneData scene)
    {
        string url = FirebaseConfig.GetDocumentUrl($"stories/{storyId}/scenes/{sceneIndex}");
        string json = BuildSceneDataJson(scene);

        using (var request = new UnityWebRequest(url, "PATCH"))
        {
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Migration] Failed to upload scene {storyId}/{sceneIndex}: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
            }
        }
    }

    private IEnumerator UploadSceneSprites(string storyId, int sceneIndex, string spritesPath)
    {
        if (!Directory.Exists(spritesPath)) yield break;

        // Upload all frames for this scene (1.1.png, 1.2.png, etc.)
        for (int frame = 1; frame <= 10; frame++)
        {
            string fileName = $"{sceneIndex}.{frame}.png";
            string filePath = Path.Combine(spritesPath, fileName);

            if (File.Exists(filePath))
            {
                yield return UploadSprite(storyId, fileName, filePath);
            }
            else
            {
                break; // No more frames
            }
        }
    }

    private IEnumerator UploadSprite(string storyId, string spriteName, string localPath)
    {
        byte[] pngData = File.ReadAllBytes(localPath);
        string storagePath = $"stories/{storyId}/sprites/{spriteName}";
        string url = FirebaseConfig.GetStorageUploadUrl(storagePath);

        using (var request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(pngData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "image/png");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Migration] Failed to upload sprite {spriteName}: {request.error}");
            }
        }
    }

    private void StartCleanAllStorage()
    {
        _isUploading = true;
        _progress = 0f;
        _statusMessage = "Cleaning storage...";
        EditorCoroutineRunner.StartCoroutine(CleanAllStorageCoroutine());
    }

    private IEnumerator CleanAllStorageCoroutine()
    {
        _statusMessage = "Listing files in storage...";
        Repaint();

        // List all files in storage
        string listUrl = $"{FirebaseConfig.StorageBaseUrl}?prefix=stories/";

        using (var request = UnityWebRequest.Get(listUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Migration] Failed to list storage files: {request.error}");
                _statusMessage = $"Failed to list files: {request.error}";
                _isUploading = false;
                Repaint();
                yield break;
            }

            var json = request.downloadHandler.text;
            var files = ParseStorageFileList(json);

            if (files.Count == 0)
            {
                _statusMessage = "Storage is already empty!";
                _isUploading = false;
                Repaint();
                yield break;
            }

            Debug.Log($"[Migration] Found {files.Count} files to delete");

            // Delete each file
            for (int i = 0; i < files.Count; i++)
            {
                string fileName = files[i];
                _statusMessage = $"Deleting {i + 1}/{files.Count}: {fileName}";
                _progress = (float)i / files.Count;
                Repaint();

                yield return DeleteStorageFile(fileName);
            }

            _statusMessage = $"Deleted {files.Count} files from storage!";
            _isUploading = false;
            _progress = 1f;
            Repaint();
        }
    }

    private IEnumerator CleanStoryStorageCoroutine(string storyId)
    {
        // List files for this story
        string prefix = $"stories/{storyId}/";
        string listUrl = $"{FirebaseConfig.StorageBaseUrl}?prefix={UnityWebRequest.EscapeURL(prefix)}";

        using (var request = UnityWebRequest.Get(listUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[Migration] Could not list files for {storyId}: {request.error}");
                yield break;
            }

            var json = request.downloadHandler.text;
            var files = ParseStorageFileList(json);

            Debug.Log($"[Migration] Deleting {files.Count} old files for {storyId}");

            foreach (var fileName in files)
            {
                yield return DeleteStorageFile(fileName);
            }
        }
    }

    private List<string> ParseStorageFileList(string json)
    {
        var files = new List<string>();

        // Simple JSON parsing for {"items": [{"name": "..."}, ...]}
        int itemsStart = json.IndexOf("\"items\"");
        if (itemsStart < 0) return files;

        int pos = itemsStart;
        while (true)
        {
            int nameStart = json.IndexOf("\"name\"", pos);
            if (nameStart < 0) break;

            int valueStart = json.IndexOf("\"", nameStart + 7);
            if (valueStart < 0) break;

            int valueEnd = json.IndexOf("\"", valueStart + 1);
            if (valueEnd < 0) break;

            string fileName = json.Substring(valueStart + 1, valueEnd - valueStart - 1);
            files.Add(fileName);

            pos = valueEnd + 1;
        }

        return files;
    }

    private IEnumerator DeleteStorageFile(string filePath)
    {
        string encodedPath = UnityWebRequest.EscapeURL(filePath);
        string url = $"{FirebaseConfig.StorageBaseUrl}/{encodedPath}";

        using (var request = UnityWebRequest.Delete(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[Migration] Failed to delete {filePath}: {request.error}");
            }
        }
    }

    private void TestConnection()
    {
        EditorCoroutineRunner.StartCoroutine(TestConnectionCoroutine());
    }

    private IEnumerator TestConnectionCoroutine()
    {
        _statusMessage = "Testing connection...";
        Repaint();

        string url = FirebaseConfig.GetCollectionUrl("stories");
        Debug.Log($"[Migration] Testing URL: {url}");

        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                _statusMessage = "Connection successful! Firebase is accessible.";
                Debug.Log($"[Migration] Connection test response: {request.downloadHandler.text}");
            }
            else
            {
                _statusMessage = $"Connection failed: {request.error}";
                Debug.LogError($"[Migration] Connection test failed: {request.error}");
                Debug.LogError($"[Migration] Response code: {request.responseCode}");
                Debug.LogError($"[Migration] URL: {url}");
                if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    Debug.LogError($"[Migration] Response body: {request.downloadHandler.text}");
                }
            }

            Repaint();
        }
    }

    #region Helpers

    private int CountScenes(string levelsFile)
    {
        int count = 0;
        using (var sr = new StreamReader(levelsFile))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line == "~") count++;
            }
        }
        return count;
    }

    private SceneData LoadSceneFromFile(string filePath, int sceneIndex)
    {
        var raw = new string[26];

        using (var sr = new StreamReader(filePath))
        {
            string line;

            // Skip to the requested scene
            for (int j = 1; j < sceneIndex; j++)
            {
                while ((line = sr.ReadLine()) != "~") ;
            }

            int i = 0;
            while ((line = sr.ReadLine()) != "~" && i < 26)
            {
                if (line == "")
                {
                    raw[i] = "";
                    i++;
                }
                else if (line[0] == '#')
                {
                    continue;
                }
                else
                {
                    if (line[0] == '.')
                        raw[i] = "";
                    else
                        raw[i] = ParseLine(line);
                    i++;
                }
            }
        }

        return SceneDataParser.Parse(raw);
    }

    private string ParseLine(string line)
    {
        string newLine = (line[0] == '\\') ? "" : line[0].ToString();
        for (int i = 1; i < line.Length; ++i)
        {
            if (line[i] == 'n' && line[i - 1] == '\\')
                newLine = string.Concat(newLine, "\n");
            else if (line[i] != '\\')
                newLine = string.Concat(newLine, line[i]);
        }
        return newLine;
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
}

/// <summary>
/// Coroutine runner for Editor scripts that properly handles AsyncOperation.
/// </summary>
public static class EditorCoroutineRunner
{
    private class EditorCoroutine
    {
        public IEnumerator Routine;
        public Stack<IEnumerator> Stack = new Stack<IEnumerator>();
        public AsyncOperation WaitingFor;
    }

    private static List<EditorCoroutine> _coroutines = new List<EditorCoroutine>();

    public static void StartCoroutine(IEnumerator routine)
    {
        var coroutine = new EditorCoroutine { Routine = routine };
        coroutine.Stack.Push(routine);
        _coroutines.Add(coroutine);
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        for (int i = _coroutines.Count - 1; i >= 0; i--)
        {
            var coroutine = _coroutines[i];

            if (coroutine.Stack.Count == 0)
            {
                _coroutines.RemoveAt(i);
                continue;
            }

            // If waiting for an async operation, check if it's done
            if (coroutine.WaitingFor != null)
            {
                if (!coroutine.WaitingFor.isDone)
                {
                    continue; // Still waiting
                }
                coroutine.WaitingFor = null; // Done waiting
            }

            var current = coroutine.Stack.Peek();

            bool moveNext;
            try
            {
                moveNext = current.MoveNext();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _coroutines.RemoveAt(i);
                continue;
            }

            if (!moveNext)
            {
                coroutine.Stack.Pop();
            }
            else if (current.Current is IEnumerator nested)
            {
                coroutine.Stack.Push(nested);
            }
            else if (current.Current is AsyncOperation asyncOp)
            {
                coroutine.WaitingFor = asyncOp;
            }
            // else: yield return null or other - just continue next frame
        }

        if (_coroutines.Count == 0)
        {
            EditorApplication.update -= Update;
        }
    }
}
