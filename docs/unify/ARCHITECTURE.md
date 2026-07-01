# Rody Collection - Unified Story Architecture (Recommended)

Status: recommended design, pre-implementation. Synthesized from three proposal angles and three judge scorecards, reconciled against `docs/unify/AUDIT.md` (file:line facts are load-bearing). No em dashes used anywhere by intent.

---

## 1. Thesis

The system already converged on one real model and one real cross-scene channel. The `.rody.json` payload (`StoryExporter.ExportedStory`: formatVersion / exportedAt / story{id,title,sceneCount} / credits / scenes / sprites) is the single on-disk AND in-memory schema for official and user stories alike (AUDIT 7.1). The `WorkingStory` static is the only thing that actually carries state across `SceneManager.LoadScene`, because it is static (AUDIT 4.1). Everything else is ceremony around those two facts: a provider interface whose only live methods are "list" and "fetch one", a double singleton wrapping it, a second copy-pasted decode/sprite-cache engine, a stringly-typed official-vs-user kind system inferred from `GameObject.name`, and six PlayerPrefs keys (three never read).

The recommended architecture does not invent a new model or a new channel. It collapses onto the two that already carry the load, splits the one God-class into single-responsibility plain-C# collaborators, deletes the desktop half outright, and adds the one genuinely missing capability: real WebGL persistence via `persistentDataPath` plus an explicit IndexedDB flush.

Provenance (official vs user) stops being a runtime branch. It becomes one metadata enum read in exactly one place: the edit affordance. Gameplay reads never see it.

---

## 2. Design Principles

- **KISS first.** Fewer types than today. One live story, one session, one persistence gateway, one sprite cache, one serializer. No abstraction earns its place unless a verified cost demands it.
- **DRY.** One base64-to-Sprite decoder (today there are two identical ones: `WorkingStory.cs:563-597` vs `ResourcesStoryProvider.cs:115-159`). One serializer. One sprite-naming helper. One catalog as the single source of membership AND order (kills the `index.json` vs `RA_ScrollView.OrderStories` duplication).
- **Clean-code-unity.**
  - MonoBehaviour is a bridge, not a brain. All story logic lives in plain C# classes.
  - Composition over inheritance. The session owns collaborators; it does not subclass them.
  - Blind UI. A button fires an event, a plain model updates, the UI redraws. No business logic in panels.
  - Architect for deletion. Each collaborator can be removed or swapped without touching the others.
  - Burn the singletons. `StoryProviderManager` (a double singleton) and the `WorkingStory` static both go. One composition root does all the `new`-ing.
  - Invert the model-to-UI dependency. The model raises `DirtyChanged`; the platform layer subscribes. Today the reverse is wired: `WorkingStory.IsDirty`'s setter calls `ExportReminder` (AUDIT S4).
- **WebGL-only.** WebGL is the only first-class platform. Other platforms get best-effort for free because `System.IO` + `persistentDataPath` work everywhere and the IndexedDB flush degrades to a no-op. Every desktop file picker, `#else` body, `HasFileSystem` branch, and the entire StandaloneFileBrowser native tree are deleted.

---

## 3. Unified Data Model and Single Source of Truth

One payload type, two granularities, both plain C# (Newtonsoft-serializable, not MonoBehaviour, not ScriptableObject).

- **Story** is the existing `ExportedStory` shape, promoted to a top-level runtime POCO. The JSON field names are unchanged, so every existing `.rody.json` (the 7 official files plus any exported user file) deserializes with zero migration. The contract is the JSON schema, not the C# type name.
- **StoryCard** is a lightweight catalog row: id, title, sceneCount, cover bytes, source. It never carries scenes or scene sprites. It exists so the selection carousel can paint without deserializing 7 multi-MB bodies (AUDIT 8). Critically, official cards are produced by an **export-time catalog manifest with covers** (see section 6), not by deserializing each full official story to extract its cover. This closes the gap the judges flagged in the lightweight-catalog claim: there is no cheap way to read "just the cover" out of a single full `.rody.json` TextAsset, so the cover is published separately at export time.

The single source of truth at runtime is **StorySession**: exactly one owned `Story`, one scene cursor, one dirty flag, and one source tag. It replaces `WorkingStory.Current` + `CurrentSceneIndex` + `IsDirty` + `IsOfficial`. Story data and navigation state live on the session; provenance is metadata only.

`SceneData` and `SceneDataParser` are kept exactly as-is. They are the one clean abstraction and the only consumer-facing per-scene shape (AUDIT 7.2). The `levels.rody` 26-line magic-index parsing path is editor/export tooling and is quarantined to an Editor-only assembly so it cannot influence the runtime contract.

There is exactly ONE owned copy of any story. Official stories are materialized into the session by deep copy on selection (copy-on-load), which kills the shared-reference hazard where two owners point at one mutable cached `ExportedStory` (AUDIT S3, verified `ResourcesStoryProvider.cs:213-219` returns the cached object by reference and `WorkingStory.cs:134-141` assigns it directly).

---

## 4. Core Types

| Type | Kind | Responsibility |
|------|------|----------------|
| `Story` | plain C# POCO | The `.rody.json` payload promoted to runtime (renamed `ExportedStory`): metadata, credits, scenes, sprites dict. Trivial accessors and `Clone()`. No load/save/export/UI logic. |
| `StorySource` | enum | `{ Builtin, User }`. Provenance only. Read in exactly one place (the edit affordance label/flow). Never branches a gameplay read path. |
| `StoryCard` | plain C# POCO | Lightweight catalog row: id, title, sceneCount, cover bytes, source. Paints the carousel without loading full bodies. |
| `StorySession` | plain C# class | THE single runtime source of truth. Owns the current `Story`, scene cursor, dirty flag, source. Exposes the preserved gameplay read contract (`IsLoaded`, `Title`, `SceneCount`, `CurrentSceneIndex`, `LoadScene`, `LoadSprite`, `LoadSceneSprites`, `GetCredits`) and all mutation (`SaveScene`, `CreateScene`, `DeleteScene`, `SetTitle`, `SetCredits`, `SaveSprite`). Raises `DirtyChanged`. Edit always operates on the one owned instance; "edit a builtin" is just "the session already holds an owned copy". |
| `StoryCatalog` | plain C# class | Lists all `StoryCard`s (builtin from the manifest + user from `persistentDataPath`) merged and ordered by the manifest order then user mtime. Resolves a card id to a full owned `Story` on demand. The single source of membership AND order. Replaces `IStoryProvider` + `ResourcesStoryProvider` (read side) + `OrderStories` + `index.json` ordering. |
| `StoryStore` | plain C# class | The only thing that touches a filesystem at runtime. Reads builtin JSON from Resources; reads/writes/lists/deletes user `.rody.json` under `persistentDataPath` via `System.IO`; calls the IndexedDB flush after writes; hydrates from IndexedDB on init. Reuses the one serializer. Replaces `PathManager` and the persistence half of the file IO. |
| `SpriteCache` | plain C# class | The one base64-to-Sprite decoder + cache + teardown (`data:` strip, `FilterMode.Point`, `Sprite.Create`, frame loop, glitch fallback). Owns the `cover.png` / `0.png` / `{scene}.{frame}.png` naming convention. The session delegates `LoadSprite` / `LoadSceneSprites` here. Replaces the two copy-pasted decoders and two duplicate teardowns. |
| `StoryJson` | static | The one serialize/deserialize. Reused for export, persist, deep-copy, and import. Replaces four scattered serialize sites. |
| `WebShare` | plain C# class + thin bridge | Browser import/export for SHARING only (upload `.rody.json`, upload image as base64, download `.rody.json`). Slimmed from `WebGLFileBrowser`, desktop branch deleted. Not a persistence role; persistence is `StoryStore`. |
| `StoryRoot` | MonoBehaviour | The composition root and the only `DontDestroyOnLoad` object. Spawned by Bootstrap. Constructs `StorySession`, `StoryCatalog`, `StoryStore`, `SpriteCache`, and owns the one jslib `SendMessage` callback surface (static classes cannot be `SendMessage` targets). The only MonoBehaviour in the storage layer; everything else is plain C#. Burns `StoryProviderManager` and the `WorkingStory` static. |
| `AppScenes` | static const | Named build indices (Selection=0, Title=1, Menu=2, Game=3, Credits=4, Win=5, Editor=6, Phonemes=7) replacing bare integer literals scattered across 8 files. |

### Why this type set

It is the lean spine of Angle A (Story + session-as-store + storage + sharing + bridge + scene consts) merged with the clean-code rigor of Angle B (plain-C# session, composition root, `DirtyChanged` inversion, one SpriteCache, one serializer) and the two correct keepers from Angle C (an export-time cover manifest so the lightweight catalog is real, and hydrate-from-IndexedDB on init).

It deliberately rejects Angle B's `IStoryCatalogSource` interface with two implementations and Angle C's matching `IStorySource` pair. With exactly one live story and two fixed sources (Resources, persistentDataPath), a two-implementation interface plus a merging abstraction is more seam than the requirement proves. `StoryCatalog` folds builtin and user reads inside itself directly. This is the KISS deduction both interfaces cost in judging.

---

## 5. How Official and User Unify With Zero Runtime Branching

There is one list, one type, one read path.

- `StoryCatalog.Cards()` returns builtin and user rows indistinguishably. The carousel instantiates identical slots from that list. No `IsUserStorySlot`, no `userStorySlotIndices`, no `json:` / `workingStory` magic names.
- Selecting any card calls `StoryCatalog.Resolve(id)`, which returns an owned `Story`, and `StorySession.Load(story, source)`. From that instant every gameplay scene reads the session through the preserved contract with zero provenance check. The 8 of 9 gameplay files that already read only `WorkingStory.Current` change only their receiver token (AUDIT 5).
- Edit is unified. `StorySession` already holds an owned copy, so editing a builtin needs no fork data path. The session relabels to a `User` source on first edit (copy-on-load made the fork free). "Duplicate" becomes an explicit `Story.Clone()` + new id, not an implicit fork-on-edit.
- The only place `StorySource` is consulted is the edit affordance: `RA_ActionPanel.Show(bool isUser)` enables Export for all and labels Edit appropriately; the editor entry treats a builtin edit as "produce a user copy". That is the entire surviving branch, one boolean decided once.

This is honest about the residual: provenance is consolidated, not literally eliminated. `StorySource` still decides where `StoryStore` reads bytes and whether Delete is offered. The judges flagged Angle A's "zero isOfficial branching" as slightly oversold; the accurate claim is **zero provenance branching in any gameplay read path, and exactly one provenance branch total, in the edit affordance**. The four verified `IsOfficial` sites (`MenuManager.cs:166`, `ExportReminder.cs:45/57`, `RA_ScrollView.cs:553`, `RM_GameManager.cs:238`) all collapse: `ExportReminder` is deleted, the rest read the one source flag.

The stringly-typed kind system (`GameObject.name` = `workingStory` / `json:<path>` / id, `GetSlotKind`, the 5+ re-decodes) is replaced by binding each slot to a `StoryCard.id`.

---

## 6. WebGL Persistence Design

Runtime persistence becomes real. Today it is zero: `Current` is volatile and the only durability is a browser download (AUDIT 4, grep-confirmed zero `syncfs`/`IDBFS`/`indexedDB` hits anywhere).

- **Write:** `StoryStore.Save(story)` serializes via `StoryJson` and writes `System.IO.File.WriteAllText` to `Application.persistentDataPath/Stories/<id>.rody.json`, then calls the IndexedDB flush. The same `WriteAllText` runs unmodified in the Editor and on any non-WebGL platform (native FS), where the flush is a no-op. No `#else` desktop body exists in the persistence path.
- **The flush** is a new jslib function `RodySyncFs()` wrapping `FS.syncfs(false, cb)`, because Unity mounts `persistentDataPath` on an IDBFS-backed virtual FS and does NOT auto-flush to IndexedDB on a file write. This is the single missing capability the audit flags.
- **Honest platform guard.** The flush is reached through `[DllImport("__Internal")]`, and the `__Internal` intrinsic does not link in the Editor. So the import declaration and its call site retain one `#if UNITY_WEBGL && !UNITY_EDITOR` guard, matching every existing extern in the repo (`ExportReminder.cs:11-20`, `WebGLFileBrowser.cs:15-24`). The accurate claim is "no `#else` desktop body", not "no `#if` survives". This corrects the overstatement the judges caught in two proposals.
- **Async timing contract (load-bearing).** `FS.syncfs` is asynchronous and Unity does not await it. The dirty flag is cleared ONLY in the syncfs success callback, never synchronously after `Save`. The editor shows a brief "saving" state until the callback returns. A `visibilitychange`/page-hide flush is registered as a backstop for the small window where a tab closes mid-flush. This is the concrete contract the judges asked both A and B to nail down.
- **Hydrate on init.** On first load IDBFS may not be populated yet. `StoryStore.Init()` runs one `FS.syncfs(true, cb)` to hydrate from IndexedDB BEFORE the first catalog read, so a user's saved stories reappear on reload. This is Angle C's correct catch.
- **Reads.** `StoryCatalog` enumerates `persistentDataPath/Stories` for user `.rody.json` (header-only for cards) and merges them with builtin cards from the manifest. User stories survive reload, which they cannot today.
- **Builtin covers without eager full deserialization.** An export-time manifest (`Resources/Stories/catalog.json`, generated by the existing export tool, replacing `index.json`) lists each builtin's id, title, sceneCount, and cover bytes. The carousel paints from this manifest. A full builtin body is deserialized only when its card is selected. This is the mechanism that makes the lightweight-catalog claim real instead of asserted.
- **Import/export is SHARING only.** `WebShare.Export` downloads `Current` as a `.rody.json` for the user to share; `WebShare.Import` reads a picked `.rody.json` and hands the JSON to the catalog, which materializes it as a `User` story and persists it so it survives reload. Because edits and imports auto-persist, the entire `ExportReminder` + beforeunload + `SetUnsavedWorkFlag` + `window._rodyHasUnsavedWork` + `download:` `LastSavePath` sentinel nag chain is deleted (AUDIT 12).

Stated limitation: IndexedDB can be evicted by the browser under storage pressure or in private browsing, and base64 sprites make stories 1.4-2.6 MB each. `persistentDataPath` is the convenience store; export-as-file remains the only hard backup. Writes/flush must fail gracefully with a "storage full" surface.

---

## 7. What Replaces the Cross-Scene Handshake

The cross-scene channel becomes exactly the `StorySession` instance owned by the `DontDestroyOnLoad` `StoryRoot`, reached through one well-known accessor. It carries precisely what the audit proves is needed: the one selected `Story` + the scene cursor + dirty + source.

Deleted:

- PlayerPrefs `scenesCount`, `customGame`, `gameToDeleteType`: write-only, grep-confirmed zero readers (AUDIT 4.2).
- PlayerPrefs `gameToDelete`: dies with the desktop delete handshake; replaced by `StoryCatalog.Delete(id)` called directly from the blind action bar with the selected card id.
- PlayerPrefs `gamePath`: its only live read is `SceneLoading.cs:48`'s `Contains("Ibiza")` Zambla hack; replaced by reading `SceneData.VoiceSettings.isZambla`, which already exists in the model and which `SoundManager` already honors. This deletes the last `gamePath` reader and makes the behavior data-driven.
- Magic-string slot names (`workingStory`, `json:<path>`, raw id, `memory:<id>`, `download:<filename>`) and the `window._rodyHasUnsavedWork` JS flag.

Kept (out of scope to migrate): `rodyMakerFirstTime` stays a single PlayerPref. It is genuinely a persisted first-run editor hint, not story state. Note it is currently re-set to 1 on every menu visit (`RA_ScrollView.cs:113`), so the hint fires on every editor entry; whether that is intended is an open decision, but the storage migration does not need to touch it.

Bare scene-index literals across 8 files are replaced by `AppScenes` consts.

---

## 8. Desktop-Code Kill-List

Order matters: relocate the jslib FIRST, delete the StandaloneFileBrowser tree LAST.

**Pre-step (blocking, serialized-native-plugin risk):** move `Assets/StandaloneFileBrowser/Plugins/StandaloneFileBrowser.jslib` to `Assets/Plugins/WebGL/RodyWeb.jslib` and add `RodySyncFs`. This jslib backs all WebGL file IO; deleting its host folder before extraction kills every browser op. Verify in an actual WebGL build that DllImports still bind.

Delete outright:

- `Bootstrap.IsWebGL` + `Bootstrap.HasFileSystem` (`Bootstrap.cs:28-43`) and the one consumer branch (`RA_ScrollView.cs:142-147`).
- `PathManager` (`Utils/PathManager.cs`); its `persistentDataPath` root idea folds into `StoryStore`.
- The entire `Assets/StandaloneFileBrowser/` tree after jslib extraction: the `.cs` facade, `IStandaloneFileBrowser.cs`, Mac/Windows/Linux/Editor backends, and the native binaries `Ookii.Dialogs.dll`, `System.Windows.Forms.dll`, `Mono.Posix.dll`, `Mono.WebBrowser.dll`, `libStandaloneFileBrowser.so`, `StandaloneFileBrowser.bundle`.
- Every `using SFB;` (`RA_NewGame.cs:5`, `RM_ImagesLayout.cs:1`, `RM_ImgAnimLayout.cs:1`, `RM_MainLayout.cs:3`).
- `RM_SaveLoad.LoadSprite(string filePath, int ignored, ...)` (`RM_SaveLoad.cs:428-455`): filesystem read, dead on WebGL, plus the stale `int ignored` param.
- `ExportReminder` entirely + its jslib `ShowConfirmDialog` / `SetUnsavedWorkFlag` / `InitBeforeUnloadHandler` and the `window._rodyHasUnsavedWork` flag (replaced by automatic persistence).
- The desktop keyboard/quit affordances in `RA_ScrollView`: `HandleEscape -> Application.Quit` (`:585`), Delete-key delete and Return=LoadFolder (`:424-432`).

Collapse `#if UNITY_WEBGL`/`#else` to the WebGL body (delete the `#else`):

- `RA_NewGame` `NG_ImgClick` / `DoImport` / `OnExportClick` / `DeleteStory`.
- `RM_MainLayout.SaveAndExport`, `RM_ImagesLayout.ImportClick`, `RM_ImgAnimLayout.ImportClick`.
- `RA_ScrollView.LoadUserStories` + `json:` slots (the whole desktop multi-user-story filesystem path).

Keep but quarantine to an Editor-only assembly (NOT a runtime path): `StoryExporter`'s `levels.rody` folder-to-JSON export methods. Keep the `ExportedStory`/`ExportedScene`/`ExportedStoryMetadata` model classes (renamed under `Story`, load-bearing at runtime). The export tool also gains the job of emitting `catalog.json` with covers.

---

## 9. Proposal Comparison and Why This Hybrid

Judge scores (1-5):

| Dimension | A: One Store, Min Types | B: Repository + Plain Model | C: Data-Driven (hybrid) |
|-----------|:-----------------------:|:---------------------------:|:-----------------------:|
| KISS | 4 | 3 | 3 |
| DRY | 5 | 5 | 5 |
| Deletability | 4 | 5 | 4 |
| WebGL fit | 4 | 4 | 4 |
| Migration safety | 4 | 4 | 4 |
| Clean-code-unity | 4 | 5 | 5 |
| Fatal flaws | none | none | none |

All three agree on the same correct core and none has a fatal flaw. They differ in breadth and in two honesty gaps.

- **Angle A** is the leanest and safest spine: it identifies that the system already has one model and one channel and deletes the ceremony around them. Its one real gap is that the lightweight-catalog claim was asserted without a mechanism to read a builtin cover cheaply, and "zero isOfficial branching" was slightly oversold.
- **Angle B** is the strongest clean-code shape: plain-C# session, single composition root, `DirtyChanged` inversion, one SpriteCache, one serializer. It lost KISS on a speculative `IStoryCatalogSource` interface and day-one lazy-materialization the single-story reality does not demand, and it falsely claimed "no `#if UNITY_WEBGL` survives" against its own required DllImport.
- **Angle C** is intellectually honest: it self-rejects the ScriptableObject runtime model with a correct structural argument (a SO is a read-only build artifact on WebGL, so it would force a parallel JSON model and re-create the duality). What remains is a near-duplicate of B at greater breadth. Its two unique keepers are the build-time cover catalog and the hydrate-from-IndexedDB on init.

The recommended hybrid takes **A's lean type count and incremental, low-risk migration spine**, **B's plain-C# session + composition root + `DirtyChanged` inversion + single SpriteCache/serializer** for clean-code, and **C's export-time cover manifest + hydrate-on-init flush** to make the lightweight catalog real and the persistence correct on first load. It drops the two interface abstractions (B's `IStoryCatalogSource`, C's `IStorySource`) as unearned seams for a single-live-story app, and it states the persistence platform guard and the async flush timing contract honestly rather than overclaiming uniformity.

---

## 10. Rejected Alternatives

- **ScriptableObject runtime story model (Angle C's headline).** Rejected. On WebGL a SO is a compiled read-only build artifact, so an editable story would need a parallel JSON-backed model: two engines again, exactly the duality this migration deletes. The only defensible SO use here is the optional build-time builtin catalog, and even that is simpler as generated `catalog.json`.
- **Keep the `IStoryProvider` abstraction / a catalog-source interface.** Rejected. The interface is leaky today (`GetExportedStory`, the only method actually used, is not on it, forcing a downcast, AUDIT 10). With one live story and two fixed sources, two implementations behind an interface are more machinery than the requirement proves. `StoryCatalog` folds both reads inside itself.
- **Day-one lazy-materialization split with a two-phase resolve as an architectural requirement.** Partially rejected. Lazy body load IS adopted (selection resolves the full body), because the eager full-catalog deserialization is a verified startup cost (AUDIT 8). But it is implemented as a plain cover-manifest + resolve-on-select, not as a speculative interface layer.
- **Retain official-vs-user as a runtime branch consolidated into one enum read in five places.** Rejected in favor of one read in the edit affordance only. Provenance must not touch any gameplay read path.
- **Make "Save" continue to mean "export a download".** Rejected. Save means persist locally; Export becomes a separate explicit sharing action. This is a deliberate UX change, flagged as an open decision for the user, not a silent refactor.
- **Big-bang rewrite of `WorkingStory` to instance/injected in one pass.** Rejected. The migration keeps a thin `WorkingStory`-shim-delegates-to-session intermediate so every one of the ~17 touched files keeps compiling and each step is independently revertible.
