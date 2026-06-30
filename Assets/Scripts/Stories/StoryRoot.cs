using System;
using UnityEngine;

/// <summary>
/// Composition root and the only DontDestroyOnLoad object in the storage layer.
/// Constructs and owns the one StorySession, StoryStore, and StoryCatalog, and is
/// the SendMessage receiver for the WebFs IndexedDB callbacks (a static class
/// cannot receive SendMessage). Spawned by Bootstrap; lazily self-creates if a
/// scene is entered without it (editor direct-play), so the session always exists.
/// </summary>
public class StoryRoot : MonoBehaviour
{
    static StoryRoot _instance;

    /// <summary>The single root, created on first access if not already spawned.</summary>
    public static StoryRoot Instance
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

    public StorySession Session { get; private set; }
    public StoryStore Store { get; private set; }
    public StoryCatalog Catalog { get; private set; }

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

        Session = new StorySession();
        Store = new StoryStore();
        Catalog = new StoryCatalog(Store);
    }

    /// <summary>Hydrate persisted user stories from IndexedDB. Call once at startup.</summary>
    public void InitStore(Action onReady = null) => Store.Init(onReady);

    // ---- WebFs SendMessage callback surface (jslib invokes these by name) ----

    public void OnSyncFsComplete(string error) => WebFs.HandleFlushComplete(error);
    public void OnSyncFsHydrated(string error) => WebFs.HandleHydrateComplete(error);
}
