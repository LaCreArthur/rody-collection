using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton manager for accessing the current story provider.
/// Allows easy switching between local and remote providers.
/// Handles platform-specific initialization (WebGL uses Firebase).
/// </summary>
public class StoryProviderManager : MonoBehaviour
{
    private static StoryProviderManager _instance;
    private static IStoryProvider _provider;
    private static FirebaseStoryProvider _firebaseProvider;
    private static bool _initialized = false;
    private static bool _isInitializing = false;

    public static StoryProviderManager Instance => _instance;

    /// <summary>
    /// Event fired when the provider is ready to use.
    /// </summary>
    public static event Action OnProviderReady;

    /// <summary>
    /// Whether the provider is initialized and ready.
    /// </summary>
    public static bool IsReady => _initialized;

    /// <summary>
    /// Whether we're using Firebase (WebGL) or local storage.
    /// </summary>
    public static bool IsUsingFirebase
    {
        get
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return true;
#else
            return _firebaseProvider != null && _provider == _firebaseProvider;
#endif
        }
    }

    /// <summary>
    /// Gets the current story provider instance.
    /// Creates a LocalStoryProvider by default if none is set.
    /// </summary>
    public static IStoryProvider Provider
    {
        get
        {
            if (_provider == null)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                Debug.LogWarning("StoryProviderManager: Provider accessed before initialization on WebGL. Call Initialize() first.");
                return null;
#else
                _provider = new LocalStoryProvider();
                _initialized = true;
#endif
            }
            return _provider;
        }
    }

    /// <summary>
    /// Gets the Firebase provider for async operations.
    /// Returns null if not using Firebase.
    /// </summary>
    public static FirebaseStoryProvider FirebaseProvider => _firebaseProvider;

    /// <summary>
    /// Initializes the provider based on platform.
    /// Must be called on WebGL before using the provider.
    /// </summary>
    public static void Initialize(Action onReady = null, Action<string> onError = null)
    {
        if (_initialized)
        {
            onReady?.Invoke();
            return;
        }

        if (_isInitializing)
        {
            Debug.LogWarning("StoryProviderManager: Already initializing");
            return;
        }

        if (_instance == null)
        {
            Debug.LogError("StoryProviderManager: No instance found. Add StoryProviderManager to a GameObject in your scene.");
            onError?.Invoke("StoryProviderManager instance not found");
            return;
        }

        _isInitializing = true;

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: Use Firebase
        _instance.InitializeFirebase(onReady, onError);
#else
        // Desktop/Editor: Use local storage
        _provider = new LocalStoryProvider();
        _initialized = true;
        _isInitializing = false;
        Debug.Log("StoryProviderManager: Initialized with LocalStoryProvider");
        onReady?.Invoke();
        OnProviderReady?.Invoke();
#endif
    }

    /// <summary>
    /// Forces Firebase provider even on desktop (for testing).
    /// </summary>
    public static void InitializeWithFirebase(Action onReady = null, Action<string> onError = null)
    {
        if (_instance == null)
        {
            Debug.LogError("StoryProviderManager: No instance found");
            onError?.Invoke("StoryProviderManager instance not found");
            return;
        }

        _isInitializing = true;
        _instance.InitializeFirebase(onReady, onError);
    }

    private void InitializeFirebase(Action onReady, Action<string> onError)
    {
        Debug.Log("[StoryProviderManager] InitializeFirebase() called");
        Debug.Log($"[StoryProviderManager] Instance is: {(_instance != null ? "valid" : "NULL")}");
        Debug.Log($"[StoryProviderManager] Coroutine runner (this): {(this != null ? this.name : "NULL")}");

        _firebaseProvider = new FirebaseStoryProvider(this);
        _provider = _firebaseProvider;

        Debug.Log("[StoryProviderManager] FirebaseStoryProvider created, calling LoadStoriesAsync...");

        // Load stories list to verify connection works
        _firebaseProvider.LoadStoriesAsync(
            stories =>
            {
                _initialized = true;
                _isInitializing = false;
                Debug.Log($"[StoryProviderManager] SUCCESS - Firebase initialized, found {stories.Count} stories");
                foreach (var story in stories)
                {
                    Debug.Log($"[StoryProviderManager]   - Story: {story.id} = {story.title} ({story.sceneCount} scenes)");
                }
                onReady?.Invoke();
                OnProviderReady?.Invoke();
            },
            error =>
            {
                _isInitializing = false;
                Debug.LogError($"[StoryProviderManager] FAILED - Firebase initialization error: {error}");
                onError?.Invoke(error);
            }
        );
    }

    /// <summary>
    /// Sets a custom story provider.
    /// </summary>
    public static void SetProvider(IStoryProvider provider)
    {
        _provider = provider;
        _initialized = true;
        Debug.Log($"StoryProviderManager: Provider set to {provider.GetType().Name}");
    }

    /// <summary>
    /// Resets to the default local provider.
    /// </summary>
    public static void ResetToLocal()
    {
        _provider = new LocalStoryProvider();
        _firebaseProvider = null;
        _initialized = true;
        Debug.Log("StoryProviderManager: Reset to LocalStoryProvider");
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Auto-initialize on Awake (can be disabled if you want manual control)
        // Initialize();
    }
}
