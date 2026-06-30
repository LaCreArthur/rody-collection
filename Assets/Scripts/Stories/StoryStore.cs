using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// The only runtime filesystem gateway. Reads built-in stories from Resources;
/// reads / writes / lists / deletes user .rody.json under persistentDataPath via
/// System.IO; flushes to IndexedDB after writes; hydrates from IndexedDB on init.
/// Uses the one serializer (StoryJson). System.IO + persistentDataPath work on
/// every platform; the IndexedDB flush degrades to a no-op off WebGL.
/// </summary>
public class StoryStore
{
    const string ResourcesFolder = "Stories";
    const string Extension = ".rody.json";

    string UserDir => Path.Combine(Application.persistentDataPath, "Stories");

    /// <summary>
    /// Hydrates the IDBFS-backed persistentDataPath from IndexedDB, then ensures
    /// the user directory exists. Call once at startup before the first user read.
    /// </summary>
    public void Init(Action onReady = null)
    {
        WebFs.Hydrate(err =>
        {
            if (err != null) Debug.LogError($"StoryStore: hydrate failed: {err}");
            EnsureUserDir();
            onReady?.Invoke();
        });
    }

    // ---- Built-in (Resources, read-only) ----

    /// <summary>Reads a built-in story's JSON from Resources (null if missing).</summary>
    public string ReadBuiltinJson(string id)
    {
        // Files are "<id>.rody.json"; Resources.Load strips the trailing ".json".
        var asset = Resources.Load<TextAsset>($"{ResourcesFolder}/{id}.rody");
        return asset != null ? asset.text : null;
    }

    // ---- User (persistentDataPath, read/write) ----

    /// <summary>Ids of all persisted user stories.</summary>
    public List<string> ListUserIds()
    {
        var ids = new List<string>();
        if (!Directory.Exists(UserDir)) return ids;
        foreach (var path in Directory.GetFiles(UserDir, "*" + Extension))
            ids.Add(Path.GetFileName(path).Replace(Extension, ""));
        return ids;
    }

    /// <summary>Reads a persisted user story's JSON (null if missing).</summary>
    public string ReadUserJson(string id)
    {
        var path = UserPath(id);
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }

    public bool UserExists(string id) => File.Exists(UserPath(id));

    /// <summary>
    /// Writes a user story to persistentDataPath, then flushes to IndexedDB.
    /// onComplete(error) fires after the flush (error == null on success). Fails
    /// gracefully: a write/storage error is reported through onComplete, not thrown.
    /// </summary>
    public void SaveUser(Story story, Action<string> onComplete = null)
    {
        try
        {
            EnsureUserDir();
            File.WriteAllText(UserPath(story.story.id), StoryJson.Serialize(story));
        }
        catch (Exception e)
        {
            Debug.LogError($"StoryStore: write failed: {e.Message}");
            onComplete?.Invoke(e.Message);
            return;
        }
        WebFs.Flush(onComplete);
    }

    /// <summary>Deletes a persisted user story, then flushes.</summary>
    public void DeleteUser(string id, Action<string> onComplete = null)
    {
        try
        {
            var path = UserPath(id);
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception e)
        {
            Debug.LogError($"StoryStore: delete failed: {e.Message}");
            onComplete?.Invoke(e.Message);
            return;
        }
        WebFs.Flush(onComplete);
    }

    string UserPath(string id) => Path.Combine(UserDir, id + Extension);

    void EnsureUserDir()
    {
        if (!Directory.Exists(UserDir))
            Directory.CreateDirectory(UserDir);
    }
}
