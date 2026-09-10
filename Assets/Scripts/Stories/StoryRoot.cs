using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Cross-scene composition, workspace transitions and the one recovery writer.</summary>
public class StoryRoot : MonoBehaviour
{
    static StoryRoot _instance;
    static StoryRoot I
    {
        get
        {
            if (_instance == null) new GameObject(WebFs.ReceiverObject).AddComponent<StoryRoot>();
            return _instance;
        }
    }

    StorySession _session;
    StoryStore _store;
    StoryCatalog _catalog;
    RA_FeedbackPanel _dialog;
    bool _ready, _initializing, _hydrated, _cacheAvailable, _skipRecovery;
    bool _saving, _writeRequested, _writeInFlight, _cacheErrorShown;
    float _writeAt;
    string _recoveryError;
    Story _replacement;
    int _replacementScene;

    public static StorySession Session => I._session;
    public static StoryStore Store => I._store;
    public static StoryCatalog Catalog => I._catalog;
    public static bool IsReady => I._ready;
    public static bool IsBusy => I._saving || WebGLFileBrowser.IsBusy || (I._dialog != null && I._dialog.IsVisible);
    public static string RecoveryError => I._recoveryError;
    public static event Action StateChanged;
    public static void Ensure() { _ = I; }

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        gameObject.name = WebFs.ReceiverObject;
        DontDestroyOnLoad(gameObject);
        _session = new StorySession();
        _store = new StoryStore();
        _catalog = new StoryCatalog(_store);
        _session.WorkspaceChanged += OnWorkspaceChanged;
    }

    void Start() => Initialize();

    void Initialize()
    {
        if (_initializing || _writeInFlight) return;
        _initializing = true;
        _store.Hydrate(error =>
        {
            _initializing = false;
            _hydrated = error == null;
            if (error == null && !_skipRecovery)
            {
                try { _session.RecoverWorkspace(_store.ReadWorkspace()); }
                catch (Exception e) { error = e.Message; }
            }
            if (error != null)
            {
                _recoveryError = error;
                Dialog.ShowChoices(
                    "Impossible de récupérer ton histoire dans ce navigateur.\n" + error +
                    (_session.HasWorkspace
                        ? "\nTon histoire reste ouverte. Enregistrer permet de télécharger un fichier."
                        : "\nContinuer ouvre un nouvel espace de travail sans récupérer l’ancien brouillon."),
                    "Réessayer", Initialize, "Continuer", () =>
                    {
                        _skipRecovery = true;
                        _ready = true;
                        // Never flush an unhydrated filesystem over existing IndexedDB data.
                        _cacheAvailable = _hydrated;
                        _cacheErrorShown = true;
                        StateChanged?.Invoke();
                    });
                StateChanged?.Invoke();
                return;
            }
            _cacheAvailable = true;
            _ready = true;
            _recoveryError = null;
            _cacheErrorShown = false;
            StateChanged?.Invoke();
            if (_skipRecovery && _session.HasWorkspace) ScheduleWrite(0);
        });
    }

    RA_FeedbackPanel Dialog
    {
        get
        {
            if (_dialog == null)
            {
                var canvas = Instantiate(Resources.Load<GameObject>("StoryFeedback"), transform);
                _dialog = canvas.GetComponentInChildren<RA_FeedbackPanel>(true);
            }
            return _dialog;
        }
    }

    public static void ShowMessage(string message) => I.Dialog.ShowMessage(message);
    public static void Confirm(string message, Action onConfirm) => I.Dialog.ShowConfirm(message, "Confirmer", onConfirm);

    void OnWorkspaceChanged()
    {
        if (_ready) ScheduleWrite(0.35f);
        StateChanged?.Invoke();
    }

    void ScheduleWrite(float delay)
    {
        if (!_session.HasWorkspace) return;
        _writeRequested = true;
        _writeAt = Time.unscaledTime + delay;
    }

    public static void FlushWorkspace() => I.ScheduleWrite(0);

    void Update()
    {
        if (_ready && _cacheAvailable && _writeRequested && !_writeInFlight && Time.unscaledTime >= _writeAt)
        {
            string json;
            try { json = StoryJson.SerializeWorkspace(_session.Workspace); }
            catch (Exception e) { _writeRequested = false; CacheFailed(e.Message); return; }
            _writeRequested = false;
            _writeInFlight = true;
            _store.WriteWorkspace(json, error =>
            {
                _writeInFlight = false;
                if (error != null) CacheFailed(error);
                else { _recoveryError = null; _cacheErrorShown = false; StateChanged?.Invoke(); }
            });
        }
        if (_ready && _recoveryError != null && !_cacheErrorShown && !IsBusy)
        {
            _cacheErrorShown = true;
            Dialog.ShowConfirm("La récupération automatique a échoué. Ton travail reste ouvert.\n" +
                "Enregistrer permet toujours de télécharger ton histoire.\n" + _recoveryError,
                "Réessayer", () =>
                    RetryRecovery());
        }
    }

    public static void RetryRecovery()
    {
        var root = I;
        if (IsBusy || root._initializing || root._writeInFlight) return;
        if (root._cacheAvailable) root.ScheduleWrite(0);
        else { root._skipRecovery = true; root.Initialize(); }
    }

    void CacheFailed(string error)
    {
        _recoveryError = error;
        StateChanged?.Invoke();
    }

    public static void SaveWorkspace(Action<string> completion = null)
    {
        var root = I;
        if (!root._session.HasWorkspace) { completion?.Invoke("Aucune histoire personnelle ouverte."); return; }
        if (IsBusy) { completion?.Invoke("Une opération est déjà en cours."); return; }
        root._saving = true;
        StateChanged?.Invoke();
        Story snapshot;
        string json;
        try
        {
            snapshot = root._session.Draft.Clone();
            snapshot.exportedAt = DateTime.UtcNow.ToString("o");
            json = StoryJson.Serialize(snapshot);
        }
        catch (Exception e) { root.FinishSave(null, e.Message, completion); return; }
        string filename = StorySession.FileStem(snapshot.story.title) + ".rody.json";
        WebGLFileBrowser.Instance.DownloadTextFile(filename, json,
            error => root.FinishSave(snapshot, error, completion));
    }

    void FinishSave(Story snapshot, string error, Action<string> completion)
    {
        if (error == null)
        {
            _session.AcceptSavedSnapshot(snapshot);
            ScheduleWrite(0);
        }
        try { completion?.Invoke(error); }
        finally { _saving = false; StateChanged?.Invoke(); }
        if (completion == null) ShowMessage(error == null
            ? "Téléchargement lancé.\nEnregistrer télécharge ton histoire.\n" +
                (_recoveryError == null ? "Ce navigateur garde automatiquement ton travail."
                    : "La récupération automatique est indisponible. Garde le fichier téléchargé.")
            : "Le téléchargement n’a pas pu démarrer.\n" + error);
    }

    public static void DiscardWorkspace(Action onDiscarded)
    {
        if (!IsReady || IsBusy || !Session.IsDirty) return;
        Confirm("Annuler toutes les modifications de «" + Session.Draft.story.title +
            "» ?\nToute l’histoire reviendra à sa version initiale ou au dernier Enregistrer.", () =>
        {
            Session.DiscardChanges();
            FlushWorkspace();
            onDiscarded();
        });
    }

    public static void RequestWorkspace(Story candidate, int editorScene = 0)
    {
        if (!IsReady || IsBusy) return;
        I._replacement = candidate;
        I._replacementScene = editorScene;
        if (Session.IsDirty) I.ShowReplacement();
        else I.InstallReplacement();
    }

    void ShowReplacement(string error = null)
    {
        string message = "Remplacer «" + _session.Draft.story.title + "» par «" + _replacement.story.title +
            "» ?\nEnregistrer télécharge l’histoire actuelle avant de la remplacer.";
        if (error != null) message += "\nLe téléchargement n’a pas démarré : " + error;
        Dialog.ShowChoices(message, "Enregistrer", () => SaveWorkspace(saveError =>
        {
            if (saveError == null) InstallReplacement();
            else ShowReplacement(saveError);
        }), "Ne pas enregistrer", InstallReplacement, () => _replacement = null);
    }

    void InstallReplacement()
    {
        var story = _replacement;
        _replacement = null;
        _session.InstallWorkspace(story, _replacementScene);
        ScheduleWrite(0);
        SceneManager.LoadScene(AppScenes.Editor);
    }

    public static void EditWorkspace()
    {
        if (!IsReady || IsBusy || !Session.HasWorkspace) return;
        Session.ActivateWorkspace();
        FlushWorkspace();
        SceneManager.LoadScene(AppScenes.Editor);
    }

    public static void EditCurrentStory()
    {
        if (!IsReady || IsBusy || !Session.IsLoaded) return;
        if (Session.IsOfficial) RequestWorkspace(StorySession.Duplicate(Session.Current), Session.CurrentSceneIndex);
        else EditWorkspace();
    }

    public static void ImportWorkspace()
    {
        if (!IsReady || IsBusy) return;
        WebGLFileBrowser.Instance.OpenFileAsText(".rody.json,.json,application/json", (content, error) =>
        {
            if (error != null) { ShowMessage("Impossible de lire le fichier.\n" + error); return; }
            if (content == null) return;
            Story candidate;
            try
            {
                candidate = StoryJson.Deserialize(content);
                if (candidate == null) throw new InvalidOperationException("Le fichier est vide.");
                var images = new SpriteCache();
                try
                {
                    foreach (var image in candidate.sprites)
                        if (images.Get(image.Key, image.Value) == null)
                            throw new InvalidOperationException($"L’image «{image.Key}» ne peut pas être lue.");
                }
                finally { images.Clear(); }
            }
            catch (Exception e) { ShowMessage("Cette histoire ne peut pas être ouverte.\n" + e.Message); return; }
            RequestWorkspace(candidate);
        });
    }

    public void OnSyncFsComplete(string error) => WebFs.HandleFlushComplete(error);
    public void OnSyncFsHydrated(string error) => WebFs.HandleHydrateComplete(error);

    void OnDestroy()
    {
        if (_instance != this) return;
        _session.WorkspaceChanged -= OnWorkspaceChanged;
        _session.ClearSpriteCache();
        StateChanged = null;
        _instance = null;
    }
}
