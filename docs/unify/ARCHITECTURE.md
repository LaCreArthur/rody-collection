# Story architecture

Source review: 2026-09-09, local commit `2d7555e`. This describes the implementation;
release status belongs to [ROADMAP.md](../ROADMAP.md), unresolved defects to
[AUDIT.md](AUDIT.md), and product decisions to [DECISIONS.md](DECISIONS.md).
The June migration is implemented, with integration gaps; it is not a future build plan.
The accepted replacement is specified in the [workspace plan](../EDITOR_WORKSPACE_PLAN.md).
Until it is implemented, the ownership and Save/Export paths below remain current facts.

## Runtime ownership

| Producer / owner | Responsibility | Consumers |
|---|---|---|
| `Assets/Scripts/Stories/Story.cs` | Portable story payload: metadata, scenes, credits, sprites | Session, store, export tooling |
| `StoryJson.cs` | Story read/write/deep-copy boundary; upgrades old dialogue strings on input | Store, catalog, session, exporter |
| `StorySession.cs` | One selected story, scene cursor, provenance, dirty state; scene and sprite edits | Gameplay and Rody Maker |
| `StoryStore.cs` | User files under `Application.persistentDataPath/Stories`, built-in Resources reads, save/delete plus flush | Catalog, editor, collection actions |
| `StoryCatalog.cs` | Catalog cards and fresh story materialization on selection | Collection carousel |
| `SpriteCache.cs` | Base64 decoding, sprite naming, cache teardown | Session sprites and carousel covers use separate cache instances |
| `StoryRoot.cs` | Cross-scene owner and browser sync callback receiver; lazy creation through its accessors | Runtime entry points |
| `Assets/Scripts/WebGL/WebFs.cs` | IndexedDB flush/hydrate interop | Store |
| `Assets/Scripts/WebGL/WebGLFileBrowser.cs` | Browser file upload/download | Story and image import, export |

Paths without a directory above are in `Assets/Scripts/Stories/`.
`StorySession` owns the selected runtime story; editor panels also hold unsaved
scene drafts. Do not confuse saving a panel, committing a scene to the session,
writing the local story file, and downloading an export.

## Selection, editing, save and export

1. The carousel reads cards from `StoryCatalog.Cards()`. Built-ins come from the
   generated manifest; saved user stories follow in last-write-time order, newest last.
2. Selection resolves JSON into a fresh `Story`, then loads the session with its source.
   Gameplay reads from that session. Built-in scene bodies are loaded on selection;
   user cards currently deserialize each saved user file to extract metadata and cover.
3. Collection **Dupliquer** and the story-menu editor action call `ForkForEditing`:
   deep copy, ` (copie)` title suffix, a title-derived id, user provenance. The direct
   gameplay paintbrush follows a different entry path; see the audit before relying
   on every editor entry to fork.
4. Editor **Save** commits the current scene through `RM_SaveLoad.SaveGame`, writes
   the story via `StoryStore.SaveUser`, and clears dirty state only after the flush
   callback succeeds. It displays an error when that callback reports failure.
5. New and imported stories also call `SaveUser`. Their UI callbacks currently ignore
   the error argument. Import retains the incoming story id.
6. Collection **Exporter** reloads the selected saved user story, serializes it and
   requests a `.rody.json` browser download. It does not save current editor drafts
   or record a persistent "exported" state. The download callback signals that the
   browser download was triggered, not that a file reached durable storage.

Browser file pickers are the current player path. Outside WebGL, the file-browser
wrapper logs that the operation is unavailable; local filesystem storage still works
in the Editor. Do not promise a complete current desktop import/export product.

## Persistence boundary

`StoryStore.SaveUser` writes the JSON file then calls `WebFs.Flush`.
`Assets/Plugins/WebGL/RodyWeb.jslib` invokes `FS.syncfs(false, callback)` and sends
success/error back to `StoryRoot`. `StoryStore.Init` requests the reverse sync before
its ready callback. These are implemented mechanisms, not evidence that a browser
reload has been tested successfully. Startup wiring and overlapping callbacks are
open items in the audit.

There is an explicit Save button; the current jslib does not implement the proposed
page-hide backstop. A filesystem flush alone cannot commit unsaved editor fields.
Browser-local storage is a convenience copy. Export is the portable backup/sharing
path; the user guide explains the distinction without promising cloud storage.

## Content and file format

The runtime catalog is
[`Assets/Resources/Stories/catalog.json`](../../Assets/Resources/Stories/catalog.json).
It contains seven built-ins: six Atari stories (Noël is the fourth), then Ibiza.
Each card contains `id`, `title`, `sceneCount`, base64 `cover`, and `source`.
Runtime order comes from this manifest. The export tool owns the generation order.

The story envelope is `formatVersion`, `exportedAt`, `story` (`id`, `title`,
`sceneCount`), `credits`, `scenes` and `sprites`. Each scene entry is
`{ "index": 1, "data": { ... } }`; data is described by
[`SceneData.cs`](../../Assets/Scripts/Models/SceneData.cs).
Do not use the former flat `intro1` / `objectZones` sample as a schema.

The current writer uses format **2**. The seven embedded stories remain format **1**
and are upgraded when read; subsequent saves write speech-document objects.
The speech document, notation and French authoring contract belong to
[SPEECH_ENGINE.md](../SPEECH_ENGINE.md).

| Sprite key | Purpose | Intended dimensions |
|---|---|---|
| `cover.png` | Collection card | 320 × 200 |
| `0.png` | Story title image, separate from its cover | 320 × 200 |
| `{scene}.1.png` | Main scene image; scenes start at 1 | 320 × 130 |
| `{scene}.2.png` onward | Contiguous animation frames | 320 × 130 |

These are stored keys, not filenames creators must manually assign when importing
images. Current save-path deviations are recorded in the audit.

## Authoring the built-in catalog

`original-stories/<story>/levels.rody`, `credits.txt` and `Sprites/` are sources for
re-export. Players import self-contained `.rody.json` files, not those folders.
The 26-line legacy parser is in `Assets/Scripts/Models/SceneDataParser.cs`;
folder export lives in `Assets/Editor/StoryExporter.cs`.

- **Tools > Rody > Export All Stories Now** writes to `Assets/Resources/Stories`
  and regenerates the runtime catalog.
- **Tools > Rody > Export Stories to JSON** opens the exporter window, whose default
  output is `Assets/ExportedStories`. Choose the output deliberately.
- Re-export regenerates data from the legacy source folders. Inspect the resulting
  diff before replacing hand-edited embedded stories; runtime French edits are not
  automatically copied back into `levels.rody`.

## Scenes and project configuration

[`ProjectSettings/EditorBuildSettings.asset`](../../ProjectSettings/EditorBuildSettings.asset)
owns the enabled scene paths and order. `AppScenes.cs` names indices 0–7; its
`Credits` / `Win` identifiers do not match the filenames literally. Use the configured
paths rather than the old invented `1_TitleScene` / `RM_Main` paths.
The configured scenes include the collection, story flow, Maker, standalone voice
workbench, RollToInfinity and DOOMastico.

Unity version and packages are owned by
[`ProjectVersion.txt`](../../ProjectSettings/ProjectVersion.txt) and
[`Packages/manifest.json`](../../Packages/manifest.json).
CI behavior is owned by
[deploy-pages.yml](../../.github/workflows/deploy-pages.yml): pushes to `master` and
manual dispatch build WebGL and deploy GitHub Pages. Its artifact path is
`build/WebGL/WebGL`. Do not push merely to check a documentation change.
