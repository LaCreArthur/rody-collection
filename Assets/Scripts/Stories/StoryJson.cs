using System;
using Newtonsoft.Json;

/// <summary>
/// The single serialize / deserialize / deep-copy path for Story. Reused by
/// export, persist, import, and fork. Replaces the scattered JsonConvert sites.
/// </summary>
public static class StoryJson
{
    static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        Converters = { new SpeechDocumentReader() }
    };

    /// <summary>Serialize a story to indented JSON (for files and export).</summary>
    public static string Serialize(Story story) => JsonConvert.SerializeObject(story, Formatting.Indented, Settings);

    /// <summary>Deserialize a story from JSON. Returns null on empty input.</summary>
    public static Story Deserialize(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        var story = JsonConvert.DeserializeObject<Story>(json, Settings);
        if (story != null && story.formatVersion < Story.CurrentFormatVersion)
            story.formatVersion = Story.CurrentFormatVersion;
        return story;
    }

    /// <summary>Deep copy via a compact JSON round-trip (matches the former fork-on-edit copy).</summary>
    public static Story Clone(Story story) =>
        story == null ? null : Deserialize(JsonConvert.SerializeObject(story, Formatting.None, Settings));

    // Existing portable stories contain notation strings. Read each once as a
    // source-less document; every subsequent save writes the document object.
    sealed class SpeechDocumentReader : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanConvert(Type type) => type == typeof(SpeechDocument);

        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            if (reader.TokenType == JsonToken.String)
                return SpeechDocument.FromNotation((string)reader.Value);
            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException("A dialogue must be a speech document or an original notation string.");
            var document = new SpeechDocument();
            serializer.Populate(reader, document);
            return document;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotSupportedException();
    }
}
