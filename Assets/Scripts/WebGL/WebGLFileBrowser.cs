using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// WebGL file browser helper using browser's native file picker.
/// Uses SendMessage callbacks from jslib for async operations.
/// </summary>
public class WebGLFileBrowser : MonoBehaviour
{
    private static WebGLFileBrowser _instance;
    private Action<string> _onFileContentLoaded;
    private Action _onDownloadComplete;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void UploadFileContent(string gameObjectName, string methodName, string filter);

    [DllImport("__Internal")]
    private static extern void UploadFileAsBase64(string gameObjectName, string methodName, string filter);

    [DllImport("__Internal")]
    private static extern void DownloadFile(string gameObjectName, string methodName, string filename, byte[] byteArray, int byteArraySize);
#endif

    private Action<string> _onImageLoaded;

    /// <summary>
    /// Gets or creates the singleton instance.
    /// </summary>
    public static WebGLFileBrowser Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("WebGLFileBrowser");
                _instance = go.AddComponent<WebGLFileBrowser>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    /// <summary>
    /// Opens a file picker and reads the selected file as text.
    /// </summary>
    /// <param name="filter">File filter (e.g., ".json" or "application/json")</param>
    /// <param name="onComplete">Callback with file content (empty string if cancelled/error)</param>
    public void OpenFileAsText(string filter, Action<string> onComplete)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        _onFileContentLoaded = onComplete;
        UploadFileContent(gameObject.name, "OnFileContentLoaded", filter);
#else
        Debug.LogWarning("[WebGLFileBrowser] OpenFileAsText only works in WebGL builds");
        onComplete?.Invoke("");
#endif
    }

    /// <summary>
    /// Downloads data as a file to the user's device.
    /// </summary>
    /// <param name="filename">Suggested filename with extension</param>
    /// <param name="data">File content as bytes</param>
    /// <param name="onComplete">Callback when download starts (optional)</param>
    public void DownloadFileAsBytes(string filename, byte[] data, Action onComplete = null)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        _onDownloadComplete = onComplete;
        DownloadFile(gameObject.name, "OnDownloadComplete", filename, data, data.Length);
#else
        Debug.LogWarning("[WebGLFileBrowser] DownloadFileAsBytes only works in WebGL builds");
        onComplete?.Invoke();
#endif
    }

    /// <summary>
    /// Downloads a string as a text file.
    /// </summary>
    public void DownloadTextFile(string filename, string content, Action onComplete = null)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(content);
        DownloadFileAsBytes(filename, data, onComplete);
    }

    // Called from jslib via SendMessage
    public void OnFileContentLoaded(string content)
    {
        Debug.Log($"[WebGLFileBrowser] File content received: {content.Length} chars");
        _onFileContentLoaded?.Invoke(content);
        _onFileContentLoaded = null;
    }

    // Called from jslib via SendMessage
    public void OnDownloadComplete()
    {
        Debug.Log("[WebGLFileBrowser] Download complete");
        _onDownloadComplete?.Invoke();
        _onDownloadComplete = null;
    }

    /// <summary>
    /// Opens image picker and returns base64 data URL.
    /// </summary>
    /// <param name="filter">Image filter (e.g., "image/png,image/jpeg")</param>
    /// <param name="onComplete">Callback with data URL (empty string if cancelled)</param>
    public void OpenImageAsBase64(string filter, Action<string> onComplete)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        _onImageLoaded = onComplete;
        UploadFileAsBase64(gameObject.name, "OnImageLoaded", filter);
#else
        Debug.LogWarning("[WebGLFileBrowser] OpenImageAsBase64 only works in WebGL builds");
        onComplete?.Invoke("");
#endif
    }

    // Called from jslib via SendMessage
    public void OnImageLoaded(string dataUrl)
    {
        Debug.Log($"[WebGLFileBrowser] Image received: {(string.IsNullOrEmpty(dataUrl) ? "cancelled" : dataUrl.Length + " chars")}");
        _onImageLoaded?.Invoke(dataUrl);
        _onImageLoaded = null;
    }

    /// <summary>
    /// Converts base64 data URL to Texture2D.
    /// </summary>
    public static Texture2D DataUrlToTexture(string dataUrl)
    {
        if (string.IsNullOrEmpty(dataUrl)) return null;

        // Strip "data:image/png;base64," prefix
        int commaIndex = dataUrl.IndexOf(',');
        if (commaIndex < 0) return null;

        string base64 = dataUrl.Substring(commaIndex + 1);
        byte[] bytes = Convert.FromBase64String(base64);

        var tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        return tex;
    }
}
