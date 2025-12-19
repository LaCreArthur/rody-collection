using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

/// <summary>
/// Fixes WebGL canvas resolution issues when loading new scenes.
/// Triggers a browser resize event which forces Unity to recalculate canvas size.
/// </summary>
public class WebGLResizeHandler : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void TriggerResize();
#endif

    private static WebGLResizeHandler _instance;

    void Awake()
    {
        // Singleton pattern - persist across scenes
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Small delay to ensure scene is fully loaded before resize
        Invoke(nameof(DoResize), 0.1f);
#endif
    }

    private void DoResize()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        TriggerResize();
        Debug.Log($"[WebGLResizeHandler] Triggered resize after scene load");
#endif
    }
}
