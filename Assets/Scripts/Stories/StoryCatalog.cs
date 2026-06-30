using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// The single source of story membership AND order. Lists StoryCards (built-in
/// from the export-time catalog.json manifest, user from persistentDataPath)
/// and resolves a card id to a full owned Story on demand. Replaces the
/// old provider read side, the index.json ordering, and the hardcoded order list.
/// </summary>
public class StoryCatalog
{
    const string ManifestResource = "Stories/catalog"; // Resources/Stories/catalog.json

    readonly StoryStore _store;
    List<StoryCard> _builtin; // cached manifest cards

    public StoryCatalog(StoryStore store) => _store = store;

    /// <summary>All cards: built-in in manifest order, then user stories newest-last.</summary>
    public List<StoryCard> Cards()
    {
        var cards = new List<StoryCard>(BuiltinCards());
        cards.AddRange(UserCards());
        return cards;
    }

    /// <summary>
    /// Materializes a full owned Story for an id. A freshly deserialized object,
    /// so callers get their own copy (copy-on-load). User ids take precedence;
    /// duplicates always get a fresh id, so built-in and user ids never collide.
    /// </summary>
    public Story Resolve(string id)
    {
        if (_store.UserExists(id))
            return StoryJson.Deserialize(_store.ReadUserJson(id));

        var json = _store.ReadBuiltinJson(id);
        if (json == null)
        {
            Debug.LogError($"StoryCatalog: cannot resolve '{id}'");
            return null;
        }
        return StoryJson.Deserialize(json);
    }

    /// <summary>Provenance of an id (Builtin unless a persisted user story owns it).</summary>
    public StorySource SourceOf(string id) => _store.UserExists(id) ? StorySource.User : StorySource.Builtin;

    List<StoryCard> BuiltinCards()
    {
        if (_builtin != null) return _builtin;
        _builtin = new List<StoryCard>();

        var asset = Resources.Load<TextAsset>(ManifestResource);
        if (asset == null)
        {
            Debug.LogError("StoryCatalog: catalog.json manifest missing from Resources/Stories");
            return _builtin;
        }

        var manifest = JsonConvert.DeserializeObject<StoryCatalogManifest>(asset.text);
        if (manifest?.stories != null)
        {
            foreach (var card in manifest.stories)
            {
                card.source = StorySource.Builtin;
                _builtin.Add(card);
            }
        }
        return _builtin;
    }

    List<StoryCard> UserCards()
    {
        var list = new List<StoryCard>();
        // ponytail: reads the full body per user story to build a header card.
        // Fine while user stories are few; revisit if a header-only read is needed.
        foreach (var id in _store.ListUserIds()) // store returns them newest-last
        {
            var story = StoryJson.Deserialize(_store.ReadUserJson(id));
            if (story?.story == null) continue;

            string cover = null;
            story.sprites?.TryGetValue(SpriteCache.CoverName, out cover);

            list.Add(new StoryCard
            {
                id = story.story.id,
                title = story.story.title,
                sceneCount = story.story.sceneCount,
                cover = cover,
                source = StorySource.User,
            });
        }
        return list;
    }
}

/// <summary>Serialized shape of Resources/Stories/catalog.json (built-in membership + order + covers).</summary>
[System.Serializable]
public class StoryCatalogManifest
{
    public List<StoryCard> stories;
}
