using System;
using System.Collections.Generic;

/// <summary>
/// The portable story payload: the single on-disk AND in-memory schema for
/// official and user stories alike (.rody.json). Promoted from the former
/// StoryExporter.ExportedStory; JSON field names are unchanged so every
/// existing .rody.json deserializes without migration.
/// Pure data: no load/save/export/UI logic lives here.
/// </summary>
[Serializable]
public class Story
{
    public int formatVersion = 1;
    public string exportedAt;
    public StoryMeta story;                       // metadata (id, title, sceneCount)
    public string credits;
    public List<StoryScene> scenes;
    public Dictionary<string, string> sprites;    // filename -> base64 data

    /// <summary>Deep copy via the one serializer.</summary>
    public Story Clone() => StoryJson.Clone(this);
}

/// <summary>Story metadata. JSON property name is "story" on the parent.</summary>
[Serializable]
public class StoryMeta
{
    public string id;
    public string title;
    public int sceneCount;
}

/// <summary>One scene entry: its 1-based index plus the typed scene data.</summary>
[Serializable]
public class StoryScene
{
    public int index;
    public SceneData data;
}
