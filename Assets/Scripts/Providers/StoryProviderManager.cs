using UnityEngine;

/// <summary>
/// Singleton manager for accessing the current story provider.
/// Allows easy switching between local and remote providers.
/// </summary>
public class StoryProviderManager : MonoBehaviour
{
    private static StoryProviderManager _instance;
    private static IStoryProvider _provider;
    
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
                _provider = new LocalStoryProvider();
            }
            return _provider;
        }
    }
    
    /// <summary>
    /// Sets a custom story provider (e.g., for Firebase).
    /// </summary>
    public static void SetProvider(IStoryProvider provider)
    {
        _provider = provider;
        Debug.Log($"StoryProviderManager: Provider set to {provider.GetType().Name}");
    }
    
    /// <summary>
    /// Resets to the default local provider.
    /// </summary>
    public static void ResetToLocal()
    {
        _provider = new LocalStoryProvider();
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
