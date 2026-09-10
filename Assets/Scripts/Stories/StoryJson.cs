using System;
using System.Collections.Generic;
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
        Validate(story);
        if (story != null && story.formatVersion < Story.CurrentFormatVersion)
            story.formatVersion = Story.CurrentFormatVersion;
        return story;
    }

    public static string SerializeWorkspace(StoryWorkspace workspace) =>
        JsonConvert.SerializeObject(workspace, Formatting.None, Settings);

    public static StoryWorkspace DeserializeWorkspace(string json)
    {
        var workspace = JsonConvert.DeserializeObject<StoryWorkspace>(json, Settings);
        if (workspace == null) throw new JsonSerializationException("Le brouillon est vide.");
        Validate(workspace.draft);
        Validate(workspace.restorePoint);
        return workspace;
    }

    // Validate the data required by the existing editor/game readers before replacing work.
    static void Validate(Story story)
    {
        if (story?.story == null || string.IsNullOrWhiteSpace(story.story.id) ||
            string.IsNullOrWhiteSpace(story.story.title) || story.scenes == null ||
            story.scenes.Count == 0 || story.story.sceneCount != story.scenes.Count || story.sprites == null)
            throw new JsonSerializationException("Ce fichier ne contient pas une histoire complète.");
        if (story.formatVersion > Story.CurrentFormatVersion)
            throw new JsonSerializationException("Cette histoire utilise un format plus récent.");

        var indices = new HashSet<int>();
        foreach (var scene in story.scenes)
        {
            if (scene == null || scene.index < 1 || scene.index > story.scenes.Count || !indices.Add(scene.index) ||
                scene.data?.texts == null || scene.data.dialogues == null || scene.data.voice == null ||
                scene.data.music == null || scene.data.objects == null)
                throw new JsonSerializationException("Les tableaux de cette histoire sont incomplets.");
            if (!story.sprites.ContainsKey(SpriteCache.SceneFrameName(scene.index, 1)))
                throw new JsonSerializationException($"L’image du tableau {scene.index} manque.");
            var dialogue = scene.data.dialogues;
            ValidateDialogue(dialogue.intro1);
            ValidateDialogue(dialogue.intro2);
            ValidateDialogue(dialogue.intro3);
            ValidateDialogue(dialogue.obj);
            ValidateDialogue(dialogue.ngp);
            ValidateDialogue(dialogue.fsw);
            if (scene.data.objects.obj == null || scene.data.objects.ngp == null || scene.data.objects.fsw == null)
                throw new JsonSerializationException("Les zones d’un tableau sont incomplètes.");
        }
        if (!story.sprites.ContainsKey(SpriteCache.TitleName))
            throw new JsonSerializationException("L’image du titre manque.");
        foreach (var pair in story.sprites)
        {
            if (string.IsNullOrEmpty(pair.Value))
                throw new JsonSerializationException($"L’image «{pair.Key}» est vide.");
        }
    }

    static void ValidateDialogue(SpeechDocument document)
    {
        if (document?.sourceText == null || document.words == null || document.words.Exists(word =>
            word == null || word.score == null || word.start < 0 || word.length < 0 ||
            word.start > document.sourceText.Length || word.length > document.sourceText.Length - word.start))
            throw new JsonSerializationException("Un dialogue est incomplet.");
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
