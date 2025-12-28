using System;
using UnityEngine;

/// <summary>
/// Singleton manager for accessing the current story provider.
/// Handles platform-specific initialization:
/// - WebGL: Uses ResourcesStoryProvider (embedded JSON in build)
/// - Desktop: Uses LocalStoryProvider (StreamingAssets folder)
/// </summary>
public class StoryProviderManager : MonoBehaviour
{
    private static StoryProviderManager _instance;
    private static IStoryProvider _provider;
    private static bool _initialized = false;

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
    /// Whether we're using WebGL (Resources) or local storage (StreamingAssets).
    /// </summary>
    public static bool IsUsingWebGL
    {
        get
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return true;
#else
            return _provider is ResourcesStoryProvider;
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
                // Auto-initialize with ResourcesStoryProvider for WebGL
                _provider = new ResourcesStoryProvider("Stories");
                _initialized = true;
                Debug.Log("StoryProviderManager: Auto-initialized with ResourcesStoryProvider");
#else
                _provider = new LocalStoryProvider();
                _initialized = true;
#endif
            }
            return _provider;
        }
    }

    /// <summary>
    /// Initializes the provider based on platform.
    /// </summary>
    public static void Initialize(Action onReady = null, Action<string> onError = null)
    {
        if (_initialized)
        {
            onReady?.Invoke();
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: Use Resources (embedded JSON)
        try
        {
            _provider = new ResourcesStoryProvider("Stories");
            _initialized = true;
            Debug.Log("StoryProviderManager: Initialized with ResourcesStoryProvider");
            onReady?.Invoke();
            OnProviderReady?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"StoryProviderManager: Failed to initialize: {e.Message}");
            onError?.Invoke(e.Message);
        }
#else
        // Desktop/Editor: Use local storage
        _provider = new LocalStoryProvider();
        _initialized = true;
        Debug.Log("StoryProviderManager: Initialized with LocalStoryProvider");
        onReady?.Invoke();
        OnProviderReady?.Invoke();
#endif
    }

    /// <summary>
    /// Forces Resources provider even on desktop (for testing).
    /// </summary>
    public static void InitializeWithResources(Action onReady = null, Action<string> onError = null)
    {
        try
        {
            _provider = new ResourcesStoryProvider("Stories");
            _initialized = true;
            Debug.Log("StoryProviderManager: Initialized with ResourcesStoryProvider (forced)");
            onReady?.Invoke();
            OnProviderReady?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"StoryProviderManager: Failed to initialize: {e.Message}");
            onError?.Invoke(e.Message);
        }
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
    }
}
