using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///     Bootstraps the game by initializing the story provider.
///     On WebGL, loads stories from Resources folder.
///     Attach this to a GameObject in the first scene (Scene 0).
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
    public static event Action OnInitialized;

    public static bool IsInitialized { get; private set; }

    /// <summary>
    ///     Check if we're running on WebGL.
    /// </summary>
    public static bool IsWebGL
    {
        get {
#if UNITY_WEBGL && !UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }
    }

    /// <summary>
    ///     Check if file system operations are available.
    ///     Returns false on WebGL.
    /// </summary>
    public static bool HasFileSystem => !IsWebGL;

    void Awake()
    {
        // Ensure the composition root exists (spawns it in the entry scene).
        StoryRoot.Ensure();
    }

    void Start()
    {
        if (IsInitialized)
        {
            OnReady();
            return;
        }

        if (loadingUI != null)
            loadingUI.SetActive(true);

        Debug.Log("[Bootstrap] Hydrating story store...");

        // Hydrate persisted stories from IndexedDB before the first catalog read.
        StoryRoot.InitStore(() =>
        {
            Debug.Log("[Bootstrap] Store ready!");
            IsInitialized = true;
            OnInitialized?.Invoke();
            OnReady();
        });
    }

    void OnReady()
    {
        if (loadingUI != null)
            loadingUI.SetActive(false);

        // Initialize export reminder (beforeunload handler for WebGL)
        ExportReminder.Initialize();

        if (nextSceneIndex >= 0)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}
