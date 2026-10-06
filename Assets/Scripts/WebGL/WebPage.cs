using UnityEngine;

/// <summary>Receives calls from the web page around the WebGL player (the TV sound button).</summary>
public class WebPage : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        var page = new GameObject(nameof(WebPage));
        page.AddComponent<WebPage>();
        DontDestroyOnLoad(page);
    }
#endif

    // Page numbers arrive as float: SendMessage("WebPage", "SetVolume", 0 or 1).
    public void SetVolume(float volume) => AudioListener.volume = volume;
}
