# Story architecture

Implementation reference for the single workspace, updated **2026-09-10**.
Code and serialized UI are integrated locally; focused Editor checks passed.
Browser acceptance remains pending. [ROADMAP.md](../ROADMAP.md) owns release status,
[AUDIT.md](AUDIT.md) owns remaining checks and limitations, and
[GAME_DESIGN.md](../GAME_DESIGN.md) owns product behavior.

## Runtime ownership

| Producer / owner | Responsibility | Consumers |
|---|---|---|
| `Assets/Scripts/Stories/Story.cs` | Portable payload: metadata, scenes, credits, story-wide pronunciation respellings, encoded sprites | Session, file Save/import, resource export tools |
| `StoryJson.cs` | One serialization/deep-copy path; structural validation and old dialogue-string reading | Store, catalog, session, exporter |
| `StorySession.cs` | One `StoryWorkspace`: draft, independent restore snapshot, dirty flag and editor cursor; separate active play target and play cursor | Maker, gameplay, collection and recovery writer |
| `StoryStore.cs` | One `workspace.json` recovery envelope; read-only built-in Resources | Root and catalog |
| `StoryCatalog.cs` | Original manifest order/membership and fresh original materialization | Collection and original duplication |
| `SpriteCache.cs` | Decode, cache and teardown; canonical image keys | Active play/editor image cache and independent carousel cover cache |
| `StoryRoot.cs` | Cross-scene lifecycle, recovery readiness, replacement decision, file Save lock and one recovery writer | All entry points and shared dialog |
| `Assets/Scripts/WebGL/WebFs.cs` | IndexedDB hydrate/flush callback boundary | Store |
| `Assets/Scripts/WebGL/WebGLFileBrowser.cs` | One active picker/download operation; explicit cancellation/error/handoff result | Story/image import and file Save |

Paths without a directory above are in `Assets/Scripts/Stories/`.
`Current` projects the active gameplay story: an original or the editable draft.
Loading an original does not replace the workspace. `EditorSceneIndex` and
`CurrentSceneIndex` have different lifetimes; testing and gameplay progression
never move the remembered editor scene.

## Editing and file Save

Maker binds ordinary accepted values directly to typed draft data. Text enters
the draft as typed, music when selected, target geometry on a completed near/target pair, and
images after successful conversion. The Maker preview is a native aspect-fit UI
image; main and detailed views retain the complete 320×130 coordinate area.
The unapproved status strip and its preview resizing were removed on September 14.
Programmatic rebinds do not mark dirty.
Each accepted dialogue edit also regenerates that line's speech document from its
French text and the story's respellings; a result for superseded text or another
draft is dropped. The pronunciation-fix mode edits the respellings with explicit
Validate/Cancel; Validate resyncs every affected line. The speaker button writes
speaker and pitch directly. No mirrored scene model or Save-time scene
reconstruction remains.

A workspace is installed with an independent deep restore snapshot and no edits
since that snapshot. Save is always available when a workspace exists, including
an unchanged import or duplicate. Save locks conflicting actions, captures the
whole draft, stamps the outgoing file, and starts one `.rody.json` download.
Successful browser handoff advances the restore snapshot to exactly that capture
and clears dirty; an error changes neither. The lock releases after handoff and
any accepted replacement, not after a disk write the browser API cannot observe.

Whole-story Discard clones the restore snapshot into the draft, clears dirty,
clamps the editor cursor and rebuilds images/views. It restores added/deleted
scenes and every encoded sprite as well as dialogue and text.

New, Duplicate and Import prepare a candidate before requesting replacement.
Import parses required structure and decodes its images before showing the prompt.
Changed work offers Save / Discard / Cancel; observed Save failure keeps the old
workspace and replacement choice. The root owns the pending candidate so the
originating scene cannot destroy the decision. Collection, story menu and gameplay
paintbrush use this same entry boundary.

The collection projects seven original cards from the manifest and one personal
card from the live draft. Personal play/edit/Save never resolve a title/id to an old
file. The former user-library membership, sorting, id precedence and deletion paths
are removed; original ids cannot redirect to imported content.

The shared feedback prefab reuses the collection's art and opens over any scene.
Its callbacks hide the modal before acting. Maker controls and raw gameplay
navigation respect pending file operations and modal decisions.

## Browser recovery

The root hydrates once before exposing personal-workspace actions, then reads the
fixed recovery record. Originals may be browsed independently. The envelope stores
`draft`, `restorePoint`, `isDirty` and `editorSceneIndex` together, using the same
speech serializer as portable files. The old `Stories/` directory is untouched;
there is no legacy library or automatic choice of an old personal file.

Every accepted edit, cursor change, replacement, Save checkpoint and Discard
requests a write. A short delay coalesces typing; completed edits/navigation request
an immediate write. The root serializes the entire latest record, writes a temporary
file and replaces `workspace.json`, then flushes IDBFS. Only one flush is active;
changes during it leave a latest-record write pending. A cache callback never
changes authored content, dirty state or the restore point.

Hydration/read failure offers Retry or explicit Continue. An unhydrated filesystem
cannot be flushed over unread IndexedDB content. Read failure after hydration can
be replaced only after the explicit Continue choice. Write failure leaves the draft
open and file Save usable; the error dialog offers retry. The removed status strip
no longer supplies a separate retry action after dismissal. Save feedback reflects unavailable
recovery. No page-close asynchronous
flush is promised, and no background cache success is presented as file Save.

The JavaScript boundary is `Assets/Plugins/WebGL/RodyWeb.jslib`: native file input,
Blob download and `FS.syncfs`. Picker results distinguish cancellation, read failure
and content. Download success means browser handoff, not proof of a file on disk.
Outside WebGL, picker/download operations report unavailable instead of fake
success. Desktop parity and multiple-tab conflict resolution are outside this leg.

## Content and file format

The runtime catalog is
[`Assets/Resources/Stories/catalog.json`](../../Assets/Resources/Stories/catalog.json).
It contains seven built-ins: six Atari stories (Noël is the fourth), then Ibiza.
Each card contains `id`, `title`, `sceneCount`, base64 `cover`, and `source`.
Runtime order comes from this manifest. The export tool owns the generation order.

The story envelope is `formatVersion`, `exportedAt`, `story` (`id`, `title`,
`sceneCount`), `credits`, `scenes`, `respellings` and `sprites`. `respellings` maps a
lower-case written word to its one-word French respelling; a file without it has none. Each scene entry is
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
images. Existing originals include 640×400 titles and 640×260 scene frames; these
are 2× source images with the same logical dimensions. Sprite pixels-per-unit
normalizes their display width without modifying encoded content. Save serializes these bytes without resizing or synthesizing frames. Image
import owns palette/size conversion; removing an animation frame compacts its sequence.

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
