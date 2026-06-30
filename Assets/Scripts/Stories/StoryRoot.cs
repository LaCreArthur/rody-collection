using System;
using UnityEngine;

/// <summary>
/// Composition root and the only DontDestroyOnLoad object in the storage layer.
/// Constructs and owns the one StorySession, StoryStore, and StoryCatalog, and is
/// the SendMessage receiver for the WebFs IndexedDB callbacks (a static class
/// cannot receive SendMessage). Spawned by Bootstrap; lazily self-creates if a
/// scene is entered without it (editor direct-play), so the session always exists.
///
/// Consumers reach the model through the static accessors (StoryRoot.Session /
/// .Catalog / .Store): the one well-known access path that replaces the old
/// double-singleton provider and the old static facade.
/// </summary>
public class StoryRoot : MonoBehaviour
{
    static StoryRoot _instance;

    static StoryRoot I
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject(WebFs.ReceiverObject);
                go.AddComponent<StoryRoot>(); // Awake assigns _instance + DontDestroyOnLoad
            }
            return _instance;
        }
    }

    StorySession _session;
    StoryStore _store;
    StoryCatalog _catalog;

    /// <summary>The single live story session.</summary>
    public static StorySession Session => I._session;

    /// <summary>The single story catalog (membership + order + resolve).</summary>
    public static StoryCatalog Catalog => I._catalog;

    /// <summary>The single persistence gateway.</summary>
    public static StoryStore Store => I._store;

    /// <summary>Ensures the root exists. Call from Bootstrap in the entry scene.</summary>
    public static void Ensure() { _ = I; }

    /// <summary>Hydrate persisted user stories from IndexedDB. Call once at startup.</summary>
    public static void InitStore(Action onReady = null) => I._store.Init(onReady);

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        gameObject.name = WebFs.ReceiverObject; // must match WebFs SendMessage target
        DontDestroyOnLoad(gameObject);

        _session = new StorySession();
        _store = new StoryStore();
        _catalog = new StoryCatalog(_store);
    }

    // ---- WebFs SendMessage callback surface (jslib invokes these by name) ----

    public void OnSyncFsComplete(string error) => WebFs.HandleFlushComplete(error);
    public void OnSyncFsHydrated(string error) => WebFs.HandleHydrateComplete(error);
}
