using System;
using System.IO;
using UnityEngine;

/// <summary>One browser recovery file, plus read-only built-in resources.</summary>
public class StoryStore
{
    string WorkspacePath => Path.Combine(Application.persistentDataPath, "workspace.json");

    public void Hydrate(Action<string> onComplete) => WebFs.Hydrate(onComplete);

    public StoryWorkspace ReadWorkspace() => File.Exists(WorkspacePath)
        ? StoryJson.DeserializeWorkspace(File.ReadAllText(WorkspacePath)) : null;

    public string ReadBuiltinJson(string id)
    {
        var asset = Resources.Load<TextAsset>($"Stories/{id}.rody");
        return asset != null ? asset.text : null;
    }

    /// <summary>The root serializes/coalesces requests and allows only one write/flush at a time.</summary>
    public void WriteWorkspace(string json, Action<string> onComplete)
    {
        string temporary = WorkspacePath + ".tmp";
        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(temporary, json);
            if (File.Exists(WorkspacePath)) File.Replace(temporary, WorkspacePath, null);
            else File.Move(temporary, WorkspacePath);
        }
        catch (Exception e)
        {
            onComplete(e.Message);
            return;
        }
        WebFs.Flush(onComplete);
    }
}
