using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///     Bootstraps the game: spawns the composition root and hydrates persisted
///     stories from IndexedDB. Attach to a GameObject in the first scene (Scene 0).
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

        if (nextSceneIndex >= 0)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}
