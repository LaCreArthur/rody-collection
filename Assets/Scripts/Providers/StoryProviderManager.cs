using System;
using UnityEngine;

/// <summary>
/// Singleton manager for accessing the story provider.
/// Always uses ResourcesStoryProvider (embedded JSON in Resources/Stories/).
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
    /// Gets the current story provider instance.
    /// Always ResourcesStoryProvider.
    /// </summary>
    public static IStoryProvider Provider
    {
        get
        {
            if (_provider == null)
            {
                _provider = new ResourcesStoryProvider("Stories");
                _initialized = true;
                Debug.Log("StoryProviderManager: Auto-initialized with ResourcesStoryProvider");
            }
            return _provider;
        }
    }

    /// <summary>
    /// Initializes the provider.
    /// </summary>
    public static void Initialize(Action onReady = null, Action<string> onError = null)
    {
        if (_initialized)
        {
            onReady?.Invoke();
            return;
        }

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
