using System;

/// <summary>
/// Lightweight catalog row for the selection carousel. Carries no scenes and no
/// scene sprites, only what a slot needs to paint. Built-in cards are produced
/// from the export-time catalog manifest; user cards from persisted file headers.
/// </summary>
[Serializable]
public class StoryCard
{
    public string id;
    public string title;
    public int sceneCount;
    public string cover;        // base64 PNG (no "data:" prefix), or null
    public StorySource source;
}
