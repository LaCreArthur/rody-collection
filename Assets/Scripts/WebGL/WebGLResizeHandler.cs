#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///     Fixes WebGL canvas resolution issues when loading new scenes.
///     Triggers a browser resize event which forces Unity to recalculate canvas size.
/// </summary>
public class WebGLResizeHandler : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void TriggerResize();
#endif

    static WebGLResizeHandler _instance;

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

    void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Multiple resize attempts at different timings to ensure canvas is properly sized
        Invoke(nameof(DoResize), 0.1f);
        Invoke(nameof(DoResize), 2f);
        Invoke(nameof(DoResize), 5f);
#endif
    }

    void DoResize()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Force Unity to recalculate resolution (960x600 is the target)
        Screen.SetResolution(960, 600, false);
        Debug.Log($"[WebGLResizeHandler] Reset resolution for scene: {SceneManager.GetActiveScene().name}");
#endif
    }
}
