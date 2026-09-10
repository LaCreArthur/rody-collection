using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Built-in membership, order and materialization from the export-time manifest.
/// The personal workspace is owned by StorySession, never resolved through an id.
/// </summary>
public class StoryCatalog
{
    const string ManifestResource = "Stories/catalog"; // Resources/Stories/catalog.json

    readonly StoryStore _store;
    List<StoryCard> _builtin; // cached manifest cards

    public StoryCatalog(StoryStore store) => _store = store;

    /// <summary>Only the immutable original collection; the workspace is not an id-addressed card.</summary>
    public List<StoryCard> Cards() => BuiltinCards();

    public Story Resolve(string id)
    {
        if (!BuiltinCards().Exists(card => card.id == id))
            throw new System.ArgumentException("Cette histoire ne figure pas dans la collection.");
        var json = _store.ReadBuiltinJson(id);
        if (json == null) throw new System.IO.FileNotFoundException("L’histoire originale est introuvable.");
        return StoryJson.Deserialize(json);
    }

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

}

/// <summary>Serialized shape of Resources/Stories/catalog.json (built-in membership + order + covers).</summary>
[System.Serializable]
public class StoryCatalogManifest
{
    public List<StoryCard> stories;
}
