using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bootstraps the game by initializing the story provider.
/// On WebGL, waits for Firebase to be ready before proceeding.
/// Attach this to a GameObject in the first scene (Scene 0).
/// </summary>
public class Bootstrap : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Scene to load after initialization")]
    public int nextSceneIndex = -1; // -1 means stay in current scene

    [Tooltip("Show loading UI while initializing")]
    public GameObject loadingUI;

    [Tooltip("Show error UI if initialization fails")]
    public GameObject errorUI;

    public static bool IsInitialized { get; private set; }
    public static event Action OnInitialized;

    private void Awake()
    {
        // Ensure StoryProviderManager exists
        if (StoryProviderManager.Instance == null)
        {
            var go = new GameObject("StoryProviderManager");
            go.AddComponent<StoryProviderManager>();
            DontDestroyOnLoad(go);
        }
    }

    private void Start()
    {
        if (IsInitialized)
        {
            OnReady();
            return;
        }

        if (loadingUI != null)
            loadingUI.SetActive(true);

        Debug.Log("[Bootstrap] Initializing story provider...");

        StoryProviderManager.Initialize(
            onReady: () =>
            {
                Debug.Log("[Bootstrap] Provider ready!");
                IsInitialized = true;
                OnInitialized?.Invoke();
                OnReady();
            },
            onError: (error) =>
            {
                Debug.LogError($"[Bootstrap] Initialization failed: {error}");
                if (loadingUI != null)
                    loadingUI.SetActive(false);
                if (errorUI != null)
                    errorUI.SetActive(true);
            }
        );
    }

    private void OnReady()
    {
        if (loadingUI != null)
            loadingUI.SetActive(false);

        if (nextSceneIndex >= 0)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }

    /// <summary>
    /// Check if we're running on WebGL.
    /// </summary>
    public static bool IsWebGL
    {
        get
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }
    }

    /// <summary>
    /// Check if file system operations are available.
    /// Returns false on WebGL.
    /// </summary>
    public static bool HasFileSystem => !IsWebGL;
}
