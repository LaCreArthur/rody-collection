using Newtonsoft.Json;

/// <summary>
/// The single serialize / deserialize / deep-copy path for Story. Reused by
/// export, persist, import, and fork. Replaces the scattered JsonConvert sites.
/// </summary>
public static class StoryJson
{
    /// <summary>Serialize a story to indented JSON (for files and export).</summary>
    public static string Serialize(Story story) => JsonConvert.SerializeObject(story, Formatting.Indented);

    /// <summary>Deserialize a story from JSON. Returns null on empty input.</summary>
    public static Story Deserialize(string json) =>
        string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<Story>(json);

    /// <summary>Deep copy via a compact JSON round-trip (matches the former fork-on-edit copy).</summary>
    public static Story Clone(Story story) =>
        story == null ? null : JsonConvert.DeserializeObject<Story>(JsonConvert.SerializeObject(story, Formatting.None));
}
