using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>Native browser file input and download handoff.</summary>
public class WebGLFileBrowser : MonoBehaviour
{
    static WebGLFileBrowser _instance;
    Action<string, string> _onFileLoaded;
    Action<string> _onDownloadComplete;

    public static bool IsBusy => _instance != null &&
        (_instance._onFileLoaded != null || _instance._onDownloadComplete != null);

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void UploadFileContent(string gameObjectName, string methodName, string filter, int asDataUrl);

    [DllImport("__Internal")]
    static extern void DownloadFile(string gameObjectName, string methodName, string filename, byte[] byteArray, int byteArraySize);
#endif

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

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    /// <summary>Returns content, or null on cancellation; the second argument is an error or null.</summary>
    public void OpenFileAsText(string filter, Action<string, string> onComplete) =>
        OpenFile(filter, false, onComplete);

    /// <summary>Returns a data URL, or null on cancellation; the second argument is an error or null.</summary>
    public void OpenImageAsBase64(string filter, Action<string, string> onComplete) =>
        OpenFile(filter, true, onComplete);

    void OpenFile(string filter, bool asDataUrl, Action<string, string> onComplete)
    {
        if (onComplete == null) throw new ArgumentNullException(nameof(onComplete));
#if UNITY_WEBGL && !UNITY_EDITOR
        if (IsBusy)
        {
            onComplete(null, "Une opération de fichier est déjà en cours.");
            return;
        }
        _onFileLoaded = onComplete;
        UploadFileContent(gameObject.name, nameof(OnFileLoaded), filter, asDataUrl ? 1 : 0);
#else
        onComplete(null, "L'import de fichiers est disponible uniquement dans le navigateur.");
#endif
    }

    // Called once by the browser reader, including cancellation and errors.
    public void OnFileLoaded(string result)
    {
        var callback = _onFileLoaded;
        _onFileLoaded = null;
        if (callback == null) return;

        string content;
        string error;
        try
        {
            var parsed = JObject.Parse(result);
            content = (string)parsed["content"];
            error = (string)parsed["error"];
        }
        catch (Exception ex)
        {
            callback(null, "Impossible de lire le résultat du navigateur : " + ex.Message);
            return;
        }
        callback(content, error);
    }

    /// <summary>Reports an error, or null after browser handoff. This does not confirm a disk write.</summary>
    public void DownloadFileAsBytes(string filename, byte[] data, Action<string> onComplete = null)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (IsBusy)
        {
            onComplete?.Invoke("Une opération de fichier est déjà en cours.");
            return;
        }
        if (data == null)
        {
            onComplete?.Invoke("Le contenu à télécharger est manquant.");
            return;
        }
        _onDownloadComplete = onComplete ?? (_ => { });
        DownloadFile(gameObject.name, nameof(OnDownloadComplete), filename, data, data.Length);
#else
        onComplete?.Invoke("Le téléchargement est disponible uniquement dans le navigateur.");
#endif
    }

    public void DownloadTextFile(string filename, string content, Action<string> onComplete = null)
    {
        if (content == null)
        {
            onComplete?.Invoke("Le contenu à télécharger est manquant.");
            return;
        }
        DownloadFileAsBytes(filename, System.Text.Encoding.UTF8.GetBytes(content), onComplete);
    }

    // Called after the direct download request, never after an unrelated later click.
    public void OnDownloadComplete(string error)
    {
        var callback = _onDownloadComplete;
        _onDownloadComplete = null;
        callback?.Invoke(string.IsNullOrEmpty(error) ? null : error);
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
        if (!tex.LoadImage(bytes))
        {
            Destroy(tex);
            throw new FormatException("Cette image ne peut pas être décodée.");
        }
        return tex;
    }
}
