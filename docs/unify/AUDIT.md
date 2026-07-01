# Rody Collection - Story / Storage System Audit

Audit for the unification + WebGL-only migration. Six parallel auditors covered: storage core, selection UI, the Rody Maker editor, runtime gameplay consumers, platform/persistence, and the data format + cross-scene glue. Findings below are reconciled against direct file inspection (file:line cites are load-bearing).

Verified environment facts:

- Active build target is WebGL; `webGLTemplate` is `PROJECT:RodyTemplate`.
- `ProjectVersion.txt` is `6000.3.18f1`. `CLAUDE.md` documents `6000.3.2f1`. The doc is stale (code-irrelevant, but flagged).
- `grep` across `Assets/` for `syncfs` / `IDBFS` / `indexedDB` returns zero hits (excluding Sirenix doc text). There is no runtime WebGL persistence flush anywhere.

---

## 1. Executive Summary

### Current data flow

There are two parallel storage engines holding the same data type, plus a third "catalog" role nobody fully uses:

1. **The on-disk + in-memory model** is `StoryExporter.ExportedStory` (`Assets/Scripts/Providers/StoryExporter.cs:17-40`): `formatVersion`, `exportedAt`, `ExportedStoryMetadata{id,title,sceneCount}`, `credits`, `List<ExportedScene>{index, SceneData}`, and `Dictionary<string,string> sprites` (filename -> base64 PNG). Every `.rody.json` file (official in `Assets/Resources/Stories/`, or exported/imported user files) is exactly this serialized via Newtonsoft.
2. **`ResourcesStoryProvider`** (`Assets/Scripts/Providers/ResourcesStoryProvider.cs`, the only `IStoryProvider` impl, reached via the `StoryProviderManager` singleton) eagerly deserializes **every** official `ExportedStory` (all scenes + all base64 sprites) from `Resources/Stories/index.json` into a dictionary at construction (`LoadAllStories`, line 23-62), and exposes its own read + sprite-cache API.
3. **`WorkingStory`** (`Assets/Scripts/Providers/WorkingStory.cs`) is a `static` class holding exactly one live `ExportedStory` (`Current`) for both play and edit. It has its own duplicate read/sprite-cache API plus all mutation, fork, export, blank-texture generation, and dirty/official flags.

Runtime path: `Bootstrap.Awake` spawns the `StoryProviderManager` (`Bootstrap.cs:48-53`); `Bootstrap.Start` calls `StoryProviderManager.Initialize()` which constructs the provider. Scene 0 (`RA_ScrollView`) calls `Provider.GetStories()` for the catalog and `Provider.LoadSprite(id,"cover.png")` for covers. Selecting an official story calls `WorkingStory.LoadOfficial(id)`, which downcasts the provider to `ResourcesStoryProvider` and calls `GetExportedStory(id)` (`WorkingStory.cs:127-141`). **Every gameplay (`GameManager`) and editor (`RM_GameManager`) scene then reads ONLY from `WorkingStory`** (`LoadScene` / `LoadSprite` / `LoadSceneSprites` / `GetCredits` / `CurrentSceneIndex`), never from the provider. The provider's per-scene read methods (`LoadScene`, `LoadSceneSprites`, `LoadCredits`, `GetSceneCount`, `StoryExists`, `SaveStory`) are therefore **dead at runtime**: the provider is used only as a story catalog + cover-sprite loader + the `GetExportedStory` handoff.

Cross-scene state travels through three channels at once: the `WorkingStory` static (the real channel, survives `LoadScene` because it is static), a partly-dead PlayerPrefs layer (`gamePath`, `scenesCount`, `customGame`, `gameToDelete`, `gameToDeleteType`, `rodyMakerFirstTime`), and magic strings stuffed into scene-0 slot `GameObject.name` (`"workingStory"`, `"json:<path>"`, or the raw official `story.id`).

### The official-vs-user duality

`WorkingStory.IsOfficial` (`WorkingStory.cs:20`) is a single bool that fans out into branching across the whole system: `IsUserStory` derived (line 52), fork-only-if-official in `RA_ScrollView.HandleEditClicked`, Export-enabled-only-for-UserStory in `RA_ActionPanel.Show`, slot-kind recovered by sniffing `GameObject.name` prefixes (`GetSlotKind` and ~4 re-decodes), `MenuManager.ForkAndEdit` branching on `IsOfficial`, `RM_GameManager.Update` suppressing the exit warning for official stories, and `ExportReminder` gating on `!IsOfficial && IsDirty`. Reads against an in-memory `ExportedStory` are identical regardless of provenance; the distinction only matters at edit time. Fork's real job (`ForkForEditing`, `WorkingStory.cs:244-275`) is breaking a shared object reference + relabeling, not a different data path.

### The single-WorkingStory constraint

`WorkingStory` is `static`: there is exactly one story in memory, no lifecycle, no injection, no way to hold two. On WebGL (the target) this means **exactly one non-official story can exist at a time** (the in-memory "workingStory" slot). The multi-user-story library (`RA_ScrollView.LoadUserStories`, `json:` slots, delete flow) is gated behind `Bootstrap.HasFileSystem` and **does not function on WebGL at all**. Persistence on WebGL today is zero: `WorkingStory.Current` is volatile in-memory, and the only durability is a browser file download (export). `Application.persistentDataPath` works on WebGL (IndexedDB-backed) but is never written to in a WebGL build.

---

## 2. Subsystem Map

### 2.1 Storage Core (providers + model)

| File | Role | LOC |
|------|------|-----|
| `Assets/Scripts/Providers/WorkingStory.cs` | Static single-story holder. De-facto source of truth at play/edit. load+edit+save+export+sprite-cache+blank-texture-gen+dirty/official flags. God class. | 727 |
| `Assets/Scripts/Providers/IStoryProvider.cs` | Read+save abstraction. Most methods unused at runtime. Also declares `StoryMetadata` + `StoryData`. | 88 |
| `Assets/Scripts/Providers/ResourcesStoryProvider.cs` | Only `IStoryProvider` impl. Eager-loads all official stories + own sprite cache. Runtime use: `GetStories`, cover `LoadSprite`, `GetExportedStory`. `SaveStory` is a logged no-op. | 234 |
| `Assets/Scripts/Providers/StoryProviderManager.cs` | MonoBehaviour singleton (`Instance` + static `_provider` + `_initialized`). Only ever yields one concrete provider. | 91 |
| `Assets/Scripts/Providers/StoryExporter.cs` | Defines `ExportedStory`/`ExportedStoryMetadata`/`ExportedScene` (the real model). Plus editor-only folder->JSON export from `levels.rody`. | 264 |
| `Assets/Scripts/Models/SceneData.cs` | Typed per-scene POCO (dialogues, texts, music, voice, objectZones). The one clean data abstraction. Reused by both engines. | 172 |
| `Assets/Scripts/Models/SceneDataParser.cs` | Parses 26-line `levels.rody` into `SceneData` + `CreateGlitchScene` fallback. Only invoked by `StoryExporter` and as glitch fallback. | 210 |

### 2.2 Selection UI (scene 0_MenuCollection)

| File | Role | LOC |
|------|------|-----|
| `Assets/Scripts/RodyAnthology/RA_ScrollView.cs` | The selection brain (god-object). Builds/orders/selects slots, owns `userStorySlotIndices`, sniffs slot KIND by name, subscribes to `RA_ActionPanel` static events, loads into `WorkingStory`, writes PlayerPrefs, launches scenes. All official-vs-user + desktop-vs-WebGL branching for selection. | ~719 |
| `Assets/Scripts/RodyAnthology/RA_NewGame.cs` | New-story create + import/export/delete file IO. Every method `#if UNITY_WEBGL`/`#else` split. Unconditional `using SFB;`. | 251 |
| `Assets/Scripts/RodyAnthology/RA_ActionPanel.cs` | Blind action bar: 4 buttons fire static events; `Show(SlotKind)` enables Edit/Export/Import/New + swaps Edit label. The desired UI pattern. | 70 |
| `Assets/Scripts/RodyAnthology/RA_FeedbackPanel.cs` | Reusable modal (`ShowMessage`/`ShowConfirm`/`Hide`). Single `pendingAction`. | 69 |
| `Assets/Scripts/RodyAnthology/RA_Menu.cs` | Pixelation transition controller. Not story-data related. | 100 |
| `Assets/Scripts/RodyAnthology/RA_SoundManager.cs` | Menu SFX + easter eggs. `isRollPlaying` read by `RA_ScrollView.Update`. | 75 |
| `Assets/Scripts/RodyAnthology/RA_Triggers.cs`, `RA_MusicScript.cs`, `RA_BtnMastico.cs` | Decorative chrome. No story logic. | 54/30/18 |
| `Assets/Scenes/0_MenuCollection.unity` | Assigns 4 slot-prefab fields (3 unused). Action-bar buttons NOT scene-wired (static events). | - |

### 2.3 Rody Maker Editor (scene RM_Main, build index 6)

| File | Role | LOC |
|------|------|-----|
| `Assets/Scripts/RodyMaker/RM_GameManager.cs` | Editor brain + state holder. ~25 flattened public scene fields + 6 `List<GameObject>` zone lists. `ReadSceneStr()` flattens `SceneData`->fields. | 279 |
| `Assets/Scripts/RodyMaker/RM_SaveLoad.cs` | Static facade over `WorkingStory` + scene<->`SceneData` conversion + ~70% dead legacy string-array conversion. Duplicate `MakeTextureReadable` + desktop-only `LoadSprite`. | 456 |
| `Assets/Scripts/RodyMaker/RM_MainLayout.cs` | Save=Export flow + thumbnail grid + scene-lifecycle decision logic (magic 18/29/30). | 337 |
| `Assets/Scripts/RodyMaker/RM_ImagesLayout.cs` | Main scene-image import. WebGL/desktop branch. `ProcessImportedTexture` (resize+palette+thumbnail+persist). | 88 |
| `Assets/Scripts/RodyMaker/RM_ImgAnimLayout.cs` | Animation-frame import. Static `List<Sprite> frames` is a parallel store. WebGL branch DIVERGES (no `AtariPalette`, pixelsPerUnit 100 vs 0.5). | 87 |
| `Assets/Scripts/RodyMaker/RM_WarningLayout.cs` | Confirmation dialog that ALSO performs business logic (delete/create/switch/test/exit) via four mutually-exclusive bool flags. | 111 |
| `Assets/Scripts/RodyMaker/RM_ObjLayout.cs` | Object-zone drawing (7-state machine). Hardcodes 320x200 / Screen.width. Only zone[0] is persisted. | 287 |
| `RM_ObjectsLayout.cs`, `RM_IntroLayout.cs`, `RM_DialoguesLayout.cs`, `RM_DialLayout.cs`, `RM_MusicLayout.cs`, `RM_TextInput.cs` | Sub-panels: copy gm fields in on open, write back on return (switch-on-index everywhere). `RM_MusicLayout` has two inverse 21-case switch tables. | 46/53/54/137/110/69 |
| `RM_Layout.cs` | Abstract base. `Awake` does `GameObject.Find("GameManager")` string lookup. | 25 |
| `RM_TextureScale.cs` | Static multi-threaded resize with static mutable buffers (not reentrant). | 155 |
| `RM_TooltipDisplay.cs`, `RM_ButtonTooltip.cs` | Pure UI tooltips. | 147/32 |

### 2.4 Runtime Gameplay Consumers (build indices 1-5)

| File | Role | LOC |
|------|------|-----|
| `Assets/Scripts/GameManager.cs` | Scene 3 brain. `InitFromWorkingStory()` + `ApplySceneDataFromModel()` fan `SceneData` into ~15 cached fields + sibling MonoBehaviours. Also a GameObject factory (`CreateZoneList`). | 307 |
| `Assets/Scripts/Title.cs` | Scenes 1 (title) + 4 (credits), branched on `buildIndex`. Reads `WorkingStory`; writes dead `scenesCount` PlayerPref (line 41). | 132 |
| `Assets/Scripts/MenuManager.cs` | Scene 2 menu. Sets `CurrentSceneIndex`; builds scene-picker from `LoadSprite("{i}.1.png")`. `ForkAndEdit` branches on `IsOfficial`. | 179 |
| `Assets/Scripts/SoundManager.cs` | Phoneme TTS + audio. No `WorkingStory` access; receives voice state from `GameManager`. | 417 |
| `Assets/Scripts/Scene.cs` | Object-finding phase controller. No `WorkingStory`; reads off `gm`. Calls `RM_SaveLoad.SetActiveZones()` at runtime (editor-namespaced static). | 235 |
| `Assets/Scripts/SceneLoading.cs` | Intro fade-in. **The only runtime `gamePath` reader** (line 48): `Contains("Ibiza")` to toggle a Zambla object. | 58 |
| `Assets/Scripts/SceneAnimator.cs` | Sprite-frame animator. No `WorkingStory`; frames pushed in by `GameManager`. | 103 |
| `Assets/Scripts/Intro.cs` | Intro sequence. One `WorkingStory.SceneCount` read (line 91). | 106 |
| `Assets/Scripts/ClickHandler.cs` | Scene-flow buttons. Owns scene advancement via `WorkingStory.CurrentSceneIndex` + hardcoded `LoadScene(3/5/6)`. | 71 |

### 2.5 Platform / Persistence

| File | Role | LOC |
|------|------|-----|
| `Assets/Scripts/Bootstrap.cs` | `IsWebGL` (compile-guard mirror) + `HasFileSystem => !IsWebGL` (line 43). Spawns provider singleton; calls `ExportReminder.Initialize()`. | 101 |
| `Assets/Scripts/Utils/PathManager.cs` | `UserStoriesPath = persistentDataPath/UserStories`. Doc says "Desktop only". Only consumer: `RA_ScrollView.LoadUserStories`. | 29 |
| `Assets/Scripts/Utils/ExportReminder.cs` | WebGL beforeunload + confirm-dialog. `[DllImport]` `ShowConfirmDialog`/`SetUnsavedWorkFlag`/`InitBeforeUnloadHandler`. The ONLY "persistence safety" on WebGL: a nag, not storage. | 83 |
| `Assets/Scripts/WebGL/WebGLFileBrowser.cs` | WebGL file IO bridge (lazy singleton). `[DllImport]` `UploadFileContent`/`UploadFileAsBase64`/`DownloadFile`. `SendMessage` string callbacks. `DataUrlToTexture`. | 155 |
| `Assets/StandaloneFileBrowser/Plugins/StandaloneFileBrowser.jslib` | THE ONLY jslib. Serves BOTH `WebGLFileBrowser` AND `ExportReminder` DllImports. Co-located with desktop-only DLLs. | 245 |
| `Assets/StandaloneFileBrowser/StandaloneFileBrowser.cs` (+ tree) | Desktop/editor native dialog facade. No WebGL wrapper. Pulls in `Ookii.Dialogs.dll`, `System.Windows.Forms.dll`, `.so`/`.bundle`. | 153 |

### 2.6 Data Format + Cross-Scene Glue

| File | Role |
|------|------|
| `Assets/Resources/Stories/index.json` | Manifest `{"stories":[7 filenames]}`. The only thing distinguishing official content. Ordering ALSO hardcoded in `RA_ScrollView.OrderStories`. |
| `Assets/Scripts/Providers/WorkingStory.cs` | The actual cross-scene data channel (static `Current` + `CurrentSceneIndex`). |
| `Assets/Scripts/SceneLoading.cs` | The only `gamePath` consumer (Ibiza substring hack). |

---

## 3. Desktop-Only Inventory (the WebGL-only kill-list)

Order matters: extract the jslib BEFORE deleting the StandaloneFileBrowser tree.

### 3.0 PRE-STEP (do first, do not skip)

- **`Assets/StandaloneFileBrowser/Plugins/StandaloneFileBrowser.jslib`** is the live WebGL native layer for the WHOLE subsystem (it backs `WebGLFileBrowser`'s `UploadFileContent`/`UploadFileAsBase64`/`DownloadFile` AND `ExportReminder`'s `ShowConfirmDialog`/`SetUnsavedWorkFlag`/`InitBeforeUnloadHandler`, plus `beforeunload` reading `window._rodyHasUnsavedWork`). **Move it to `Assets/Plugins/WebGL/` (or `Assets/Scripts/WebGL/`) before deleting the desktop tree.** Deleting the folder wholesale breaks all WebGL file IO. (Serialized-asset/native-plugin risk worth an explicit verification step.)

### 3.1 Delete outright

| Target | Location | Notes |
|--------|----------|-------|
| `Bootstrap.IsWebGL` / `Bootstrap.HasFileSystem` | `Bootstrap.cs:28-43` | Sole runtime branch flag; one consumer (`RA_ScrollView:143`). |
| `PathManager` class | `Utils/PathManager.cs:1-29` | Desktop-only by its own doc. Delete OR repurpose as the single WebGL `persistentDataPath` store root. |
| `StandaloneFileBrowser` tree (after jslib extract) | `Assets/StandaloneFileBrowser/` | `.cs` facade + `IStandaloneFileBrowser.cs` + Mac/Windows/Linux/Editor + `Ookii.Dialogs.dll`, `System.Windows.Forms.dll`, `Mono.Posix.dll`, `Mono.WebBrowser.dll`, `libStandaloneFileBrowser.so`, `StandaloneFileBrowser.bundle`. |
| `using SFB;` | `RA_NewGame.cs:5`, `RM_ImagesLayout.cs:1`, `RM_ImgAnimLayout.cs:1`, `RM_MainLayout.cs:3` | Drop after the `#else` bodies are gone. |
| `RM_SaveLoad.LoadSprite(string filePath, int ignored, ...)` | `RM_SaveLoad.cs:428-455` | Filesystem `File.Exists/ReadAllBytes`. Dead on WebGL. The `int ignored` "legacy compatibility" param is a stale-API smell. |

### 3.2 Collapse `#if UNITY_WEBGL`/`#else` (delete the `#else` body, unwrap the WebGL body)

| Method | File:lines | Desktop body to delete |
|--------|-----------|------------------------|
| `RA_ScrollView` user-story gate + `LoadUserStories` + `json:` load | `RA_ScrollView.cs:142-147, 266-333, 507-542` | `Directory.GetFiles`/`File.ReadAllText`; the entire multi-user-story filesystem read path + `json:` slot concept. |
| `RA_NewGame.NG_ImgClick` | `RA_NewGame.cs:54-64` | `StandaloneFileBrowser.OpenFilePanel` cover pick. |
| `RA_NewGame.DoImport` | `RA_NewGame.cs:95-134` | `OpenFilePanel` + `File.ReadAllText`. |
| `RA_NewGame.OnExportClick` | `RA_NewGame.cs:188-214` | `SaveFilePanel` + `File.WriteAllText`. |
| `RA_NewGame.DeleteStory` | `RA_NewGame.cs:232-250` | `File.Exists/File.Delete` (desktop `gameToDelete` path). |
| `RM_MainLayout.SaveAndExport` | `RM_MainLayout.cs:177-203` | `SaveFilePanel` + `File.WriteAllText` + `MarkSaved(savePath)`. |
| `RM_ImagesLayout.ImportClick` | `RM_ImagesLayout.cs:39-49` | `OpenFilePanel` + `File.ReadAllBytes`. |
| `RM_ImgAnimLayout.ImportClick` | `RM_ImgAnimLayout.cs:34-57` | `OpenFilePanel` + `RM_SaveLoad.LoadSprite(path)`. Once gone, the `_pendingFrameIndex` guard (`:12-14`) becomes unconditional. |

### 3.3 Desktop-but-keep (editor-only, NOT a runtime path)

- `StoryExporter.ExportToJson`/`ExportToFile`/`CountScenes`/`LoadSceneFromFile`/`ParseLine` (`StoryExporter.cs:47-199`): `Directory`/`File` reads of the `original-stories/` folder format. Sole caller is `Assets/Editor/StoryExportTool.cs`. **Keep the `ExportedStory`/`ExportedScene`/`ExportedStoryMetadata` model classes (load-bearing at runtime).** Quarantine the File-IO export methods to an Editor-only assembly. Do not mistake their File usage for runtime persistence.

### 3.4 Desktop affordances with no WebGL meaning

- `RA_ScrollView.HandleEscape -> Application.Quit()` (`:585`): no-op on WebGL. Escape should return to parent menu / no-op.
- `RA_ScrollView.Update` Delete-key delete + Return=LoadFolder (`:424-432`): desktop keyboard semantics; delete only ever targets `json:` (desktop file) slots.

---

## 4. Cross-Scene Contract

The real channel is the `WorkingStory` static; the PlayerPrefs + magic-string layers are mostly vestigial duplicates. Grep-verified reader counts below.

### 4.1 The real channel (static, survives `SceneManager.LoadScene`)

| Member | Type | Carries | Set by | Read by |
|--------|------|---------|--------|---------|
| `WorkingStory.Current` | `ExportedStory` | The full story (id/title/sceneCount/credits/scenes/sprites) | `LoadOfficial`/`LoadFromJson`/`CreateNew`/`ForkForEditing` (RA_*, MenuManager) | GameManager, Title, MenuManager, RM_*, Intro, ClickHandler |
| `WorkingStory.CurrentSceneIndex` | `int` (default 1) | The play/edit scene cursor (replaces an old `currentScene` PlayerPref) | MenuManager (1 on enter, sceneToLoad on play, 0 on edit/back), `ClickHandler.NextClick` | `GameManager.Start`, `Intro`, `ClickHandler` |
| `WorkingStory.IsLoaded` | `bool` (`Current != null`) | "Has a story been selected?" | implicitly via Current | every runtime `Start()` bounces to scene 0 if false |
| `WorkingStory.IsOfficial` / `IsDirty` / `LastSavePath` | flags | Provenance + dirty + (vestigial) save path | load/edit/`MarkSaved` | edit-time branching + ExportReminder |

### 4.2 PlayerPrefs keys (verified by grep)

| Key | Written at | Read at | Status |
|-----|-----------|---------|--------|
| `gamePath` | `RA_NewGame.cs:41/127/154`, `RA_ScrollView.cs:539/560/643/668` (mostly `memory:{Id}`; line 668 overwrites with raw slot name) | `SceneLoading.cs:48` ONLY | Nearly dead. One reader, for an Ibiza substring hack. Inconsistent encoding. |
| `scenesCount` | `RA_NewGame.cs:42/128/155`, `RA_ScrollView.cs:540/561/644`, `Title.cs:41` | **none** (`grep GetInt("scenesCount")` = 0 hits) | **Dead write-only.** Pure duplicate of `WorkingStory.SceneCount`. |
| `customGame` | `RA_ScrollView.cs:635` (=1) | **none** | **Dead write-only.** |
| `gameToDelete` | `RA_ScrollView.cs:684` | `RA_NewGame.cs:220` (cleared at `:241`) | Desktop-only delete handshake. Dies with desktop. |
| `gameToDeleteType` | `RA_ScrollView.cs:685` (`"json"`), cleared `RA_NewGame.cs:242` | **none** | **Dead discriminator.** |
| `rodyMakerFirstTime` | `RA_ScrollView.cs:113` (=1, every menu init) | `RM_GameManager.cs:57`, reset to 0 at `:61` | The one live read/write loop. Note: re-set to 1 on every menu visit, so the hint fires every editor entry. |

`ClickHandler.cs:59` has a commented-out `PlayerPrefs.SetInt("scene", ...)` (already removed in favor of `CurrentSceneIndex`).

### 4.3 Magic strings

| String | Where encoded | Decoded by | Meaning |
|--------|--------------|-----------|---------|
| slot `GameObject.name = "workingStory"` | `RA_ScrollView` (~:153) | `GetSlotKind` + `LoadSelectedStoryForAction`/`LoadFolder`/`OnSuppr` (StartsWith/==) | the single in-memory user story |
| slot `GameObject.name = "json:<fullpath>"` | `RA_ScrollView` (~:318) | same | a desktop user `.rody.json` file (desktop-only) |
| slot `GameObject.name = <story.id>` | `RA_ScrollView` (official slots) | else branch -> `LoadOfficial` | official story |
| `gamePath = "memory:<id>"` | RA_* writers | nothing parses it (only `Contains("Ibiza")`) | provenance marker, effectively unused |
| `LastSavePath = "download:<filename>"` | `RA_NewGame.cs:184` (WebGL export) | nothing reads it | sentinel for "persisted via browser download" |
| `window._rodyHasUnsavedWork` (JS) | jslib `SetUnsavedWorkFlag` | jslib `beforeunload` | C# dirty-state -> tab-close warning |

### 4.4 Scene build-index glue (bare integers, no enum)

`0` selection, `1` title test, `2` in-game menu / escape, `3` gameplay reload per scene, `5` credits/win, `6` editor entry, `7` phoneme editor (additive; NOT in the documented build order 0-6 in CLAUDE.md). Scattered as literals across `GameManager`, `Title`, `MenuManager`, `ClickHandler`, `RM_WarningLayout`, `RM_ObjLayout`, `RM_DialLayout`, `ExportReminder`.

---

## 5. DRY / Coupling / SRP Violations (ranked by severity)

### S1 - Two full storage engines for one model (highest impact)

`ResourcesStoryProvider.LoadScene/LoadSprite/LoadSceneSprites/LoadCredits` (`ResourcesStoryProvider.cs:97-198`) and `WorkingStory.LoadScene/LoadSprite/LoadSceneSprites/GetCredits` (`WorkingStory.cs:551-630`) are copy-pasted twice: same base64 decode, identical `data:` prefix stripping, `FilterMode.Point`, `Sprite.Create`, frame loop, glitch fallback. Two independent sprite caches with identical destroy-texture teardown (`ResourcesStoryProvider.cs:222-233` vs `WorkingStory.cs:681-692`). Credits assembled by the same `title + "\n" + credits` formula in both (`WorkingStory.cs:629`, `ResourcesStoryProvider.cs:195-197`); the provider's `LoadCredits` is dead. **The provider's only non-dead runtime jobs are: list the catalog and hand off one full story.** Everything else duplicates `WorkingStory`.

### S2 - WorkingStory is a static God-class violating SRP

One static type does: load + edit + save + export + sprite-cache + blank-texture-gen + id-sanitize + texture-readable + dirty/official flags. No injection, no lifetime, cannot hold two stories, cannot be tested or mocked. It is simultaneously the play model, the edit model, and the export source. Natural split: a plain `Story` data object, a session holder (current story + scene index + dirty), a sprite decoder/cache, an exporter.

### S3 - Shared-reference hazard on official stories (data-corruption risk)

`WorkingStory.LoadOfficial` (`WorkingStory.cs:127-141`) assigns the very `ExportedStory` instance the provider still caches (`GetExportedStory` returns the cached object, `ResourcesStoryProvider.cs:213-219`). Two owners of one mutable object. Any edit before `ForkForEditing` would mutate the provider's cached official copy. A unified store must materialize an owned copy on load (or make the catalog immutable).

### S4 - Model reaches into platform UI (inverted dependency)

`WorkingStory.IsDirty`'s setter calls `ExportReminder.UpdateUnsavedFlag()` on every mutation (`WorkingStory.cs:32`). The data model drives a WebGL/JS-interop concern. `ExportReminder` is also coupled back to `WorkingStory`'s exact dirty semantics (`IsLoaded && !IsOfficial && IsDirty`). Circular UI-safety <-> model dependency. Should invert: model raises a `DirtyChanged` event, the platform layer subscribes (matches the project's stated static-event preference).

### S5 - official-vs-user branching scattered as a stringly-typed type system

The "kind" of a story is inferred from a `GameObject.name` prefix and re-decoded in 5+ places (`GetSlotKind`, `LoadSelectedStoryForAction`, `LoadFolder`, `OnSuppr`, `GetSelectedUserStoryPath`), compounded by `IsOfficial`/`IsUserStory` checks in `HandleEditClicked`, `RA_ActionPanel.Show`, `MenuManager.ForkAndEdit`, `RM_GameManager.Update`, `ExportReminder`. Same distinction decided independently everywhere. Should be one typed metadata property (provenance enum) on a unified `Story` model.

### S6 - UI holds storage/business logic (blind-UI violated)

`RA_ScrollView` is a god-object (builds slots, orders stories, drives carousel lerp, sniffs kind, loads stories, writes PlayerPrefs, runs scene transitions, enumerates user-story files). `RA_NewGame`, `RM_MainLayout`, `RM_ImagesLayout`, `RM_ImgAnimLayout` each call file pickers + `File.*` IO + base64 decode + mutate `WorkingStory` + PlayerPrefs inline. `RM_WarningLayout` is a confirm dialog that deletes/creates/reindexes scenes and calls `SceneManager.LoadScene` (it even logs a "BUG" warning at the fragility). `GameManager` is model-unpacker + state container + coroutine sequencer + GameObject factory. There is no persistence service layer and no plain-C# model layer. `RA_ActionPanel` (button -> static event) is the only file already following the target pattern.

### S7 - Singleton + leaky abstraction sprawl

`StoryProviderManager` is a redundant double singleton (MonoBehaviour `Instance` AND static `_provider`/`_initialized`) that only ever constructs one `ResourcesStoryProvider`, with a lazily self-initializing `Provider` getter that makes `Bootstrap.Initialize()` mostly ceremony. `WorkingStory.LoadOfficial` hard-casts `Provider as ResourcesStoryProvider` to reach `GetExportedStory` (not on the `IStoryProvider` interface), so the abstraction earns nothing. Plus: `WebGLFileBrowser` lazy singleton, `ExportReminder` static latch, `WorkingStory` static. Persistence/IO state is global and untestable.

### S8 - Editor flatten/unflatten round-trip duplicates the whole scene model

Scene state lives three ways: typed `SceneData` in `WorkingStory`, ~25 flattened scalar fields + 6 zone-`GameObject` lists on `RM_GameManager`, and per-panel copies (`RM_DialLayout`, `RM_ObjLayout`, `RM_MusicLayout`). `RM_GameManager.ReadSceneStr` (`:122-193`) and `RM_SaveLoad.GameManagerToSceneData` (`:43-88`) are inverse hand-written serializers that must be kept in sync field-by-field. Every panel manually copies gm->panel on open and panel->gm on close (switch-on-`activeDial`/`activeObj` 1/2/3). The runtime player duplicates the same field cluster in `GameManager` (some of it dead).

### S9 - Per-platform IO duplication

Every file-IO feature carries a WebGL branch + a desktop branch doing the same logical op (5 import/export sites). Two file-picker abstractions (`SFB.StandaloneFileBrowser` sync vs `WebGLFileBrowser` async `SendMessage`) never unified. Image-import-then-process copy-pasted across `RM_ImagesLayout.OnWebGLImageImported`, `RM_ImgAnimLayout.OnWebGLFrameImported`, `RA_NewGame.OnWebGLCoverImported` (and the anim path DIVERGES: skips `AtariPalette`, different pixelsPerUnit - a latent visual bug). Cover base64 decode duplicated between `RA_ScrollView.LoadUserStories` (~:295-316) and `WorkingStory.LoadSprite` (`:575-587`) / `WebGLFileBrowser.DataUrlToTexture`.

### S10 - Redundant cross-scene state + duplicated identity

`gamePath`/`scenesCount`/`customGame` PlayerPrefs duplicate data already in `WorkingStory` and are written in 6+ places; `scenesCount`/`customGame` are never read. Story identity lives in three shapes: `StoryMetadata`, `ExportedStoryMetadata`, and ad-hoc PlayerPrefs. `MakeTextureReadable` implemented twice (`RM_SaveLoad.cs:170-191` + `WorkingStory.cs:706-724`), both in the save path back-to-back. `SanitizeId` duplicated (`WorkingStory.cs:698-704` vs `StoryExporter.GetExportFileName:170-179`). `RM_MusicLayout` music<->index mapping is two inverse 21-case switches. Story ordering duplicated between `index.json` and `RA_ScrollView.OrderStories`. Save writes the same single image into frames 1-4 (4x identical base64 in JSON, `RM_SaveLoad.cs:273-276`).

---

## 6. Dead-or-Suspect Code

### Confirmed dead (grep-verified zero readers/callers)

- PlayerPrefs `scenesCount`, `customGame`, `gameToDeleteType`: write-only, never read.
- `IStoryProvider.SaveStory` + `ResourcesStoryProvider.SaveStory` (`:200-203`): no callers, body is a LogWarning. Whole write-side of the provider abstraction is dead.
- `IStoryProvider.LoadScene/LoadSceneSprites/LoadCredits/GetSceneCount/StoryExists`: no runtime callers outside the Providers folder.
- `StoryData` class (`IStoryProvider.cs:74-87`): only referenced by the dead `SaveStory` signature.
- `StoryMetadata.thumbnail` (`IStoryProvider.cs:61`): never assigned or read.
- `RA_ScrollView` fields `slotNewGamePrefab`/`slotLoadGamePrefab`/`slotExportPrefab` (`:18-20`): assigned in the scene but never instantiated. Leftover slot-button design.
- `RA_ScrollView.IsUserStorySlot` + `userStorySlotIndices` HashSet: maintained but the only consumer is never called.
- `RA_ScrollView.GetSelectedUserStoryPath` (`:697-709`), `GetSelectedIndex` (`:714-717`): no callers.
- `RA_ScrollView.LoadFolder` (`:653-664`): the `if(json:)`/`else` branches have identical bodies (both call `LoadSelectedStoryForAction(index)`). Pointless branch.
- `RM_SaveLoad.LoadSceneData` (`:332-340`), `LoadSceneTxt` (`:346-355`), `CountScenesTxt` (`:360-368`), `LoadCredits` (`:413-426`): zero callers. The entire legacy string-array machinery (`SceneDataToStringArray`, `FormatIntroTexts`, `ObjectZoneTo*String`) exists only to feed `LoadSceneTxt`.
- `RM_GameManager` fields `currentDial`, `currentText`, `modified`, `framesCount`: declared, never meaningfully read.
- `GameManager` fields `currentDial`, `currentText` (`:34`), `currentFx` (`:30`): declared `[HideInInspector]`, never assigned/read at runtime.
- jslib `UploadFile` (`:152-202`): legacy blob-URL picker, no C# DllImport references it.

### Suspect (likely broken / editor-unreachable / fragile)

- `SceneLoading.cs:49` Ibiza check: `currentGame == "Rody Et Mastico A Ibiza"` never matches because `LoadFolder` writes `gamePath` as `memory:{id}`; only the substring `Contains("Ibiza")` clause can fire, and only via legacy raw-name slots. Likely-broken special case. Note: `SceneData.VoiceSettings` already has an `isZambla` flag that `SoundManager` reads - this gamePath check duplicates that intent and should be data-driven.
- `RM_GameManager.isZambla` round-trips from data but has NO editor UI to set it (only `isMastico1-3` toggles exist). Editor-unreachable; decide expose-or-stop-persisting.
- `RM_SaveLoad.LoadSprite` `int ignored` param: documented "legacy compatibility", threaded through `RM_ImgAnimLayout` + `RA_NewGame`. Stale API.
- `RM_ObjLayout` multi-zone: UI supports up to 5 zones but `GameObjectsToObjectZone` only persists index 0; TODO comments acknowledge incomplete handling. Drawn-but-not-saved.
- `RM_ObjLayout.RM_PhonemesClick` and `RM_DialLayout.RM_PhonemesClick` both `LoadScene(7, Additive)` - duplicated; scene 7 is undocumented in CLAUDE.md's build order.
- `RA_ScrollView.Reset()` (`:344`) shadows `MonoBehaviour.Reset` (editor reset callback) - name-collision hazard.
- `GameManager` failure destinations are ad-hoc: `LoadScene(2)` (`:73`), `LoadScene(0)`/`LoadScene(5)` (Start) - three different destinations, no shared policy.
- `Scene.cs:114/119/123` calls `RM_SaveLoad.SetActiveZones` at runtime - editor-namespaced static consumed by the player.
- `RM_MusicScript` magic numbers (`currentMusicIndex=6`, modulo 7) - fragile coupling to a 7-clip array.
- `RA_NewGame.MarkSaved("download:"+filename)`: writes a `LastSavePath` sentinel nothing reads.

---

## 7. Open Facts the Architecture Must Respect

1. **`ExportedStory` is already THE single model.** `.rody.json` (formatVersion/exportedAt/story/credits/scenes/sprites) is the on-disk AND in-memory schema for both official and user stories. The only true difference is provenance (came from `Resources/index.json` vs imported/created). `StoryData`/`StoryMetadata` are redundant alternate models that can be dropped. Track origin as one enum/source field; delete `IsOfficial`/`IsUserStory`/`SlotKind`/`userStorySlotIndices`.

2. **`SceneData` + `SceneDataParser` are clean - keep them as-is.** Only the `levels.rody` 26-line magic-index parsing path is editor/export tooling; quarantine it to an Editor-only assembly. It must NOT influence the runtime data contract.

3. **Reads are provenance-independent.** Once a chosen story is materialized into one owned in-memory `ExportedStory`, official-vs-user collapses to metadata. `ForkForEditing`'s real job is breaking the shared reference + relabeling - in a unified model "edit" always operates on a user-owned instance and "duplicate" is just a copy.

4. **WebGL persistence is viable and currently MISSING.** `Application.persistentDataPath` works on WebGL (Unity mounts an IDBFS-backed virtual FS). Today there is ZERO runtime persistence: `Current` is volatile, durability is download-only. Real persistence needs BOTH (a) `System.IO.File` write of the story JSON to `persistentDataPath`, AND (b) an explicit IDBFS->IndexedDB flush. **That flush does not exist anywhere** (no `FS.syncfs`/`IDBFS`/`indexedDB` in any Assets file or the WebGL template). A new jslib function (e.g. `SyncFs` calling `FS.syncfs(false, cb)`) must be added; Unity does not auto-sync on demand. Reuse `WorkingStory.ExportToJson()` as the serializer - do not introduce a parallel one.

5. **The runtime consumer side is already 90% unified.** 8 of 9 gameplay files read only through `WorkingStory.Current` with no provenance branching. The dual-path debt at runtime is exactly two spots: `MenuManager.ForkAndEdit`'s `IsOfficial` check and `ExportReminder`'s `IsOfficial`/`IsDirty` checks. Preserve this read contract for the gameplay layer: `IsLoaded`, `Title`, `SceneCount`, `CurrentSceneIndex`, `LoadScene(index)`, `LoadSprite(name)`, `LoadSceneSprites(index)`, `GetCredits()`. Load/edit/export/sprite-cache can be split out without touching consumers.

6. **Cross-scene state reduces to one selected story + one scene cursor.** PlayerPrefs `gamePath`/`scenesCount`/`customGame` can all be deleted: 3 are write-only/dead, and `gamePath`'s only live read is the Ibiza hack (model it via the existing `isZambla` flag). `gameToDelete`/`gameToDeleteType` die with desktop. `CurrentSceneIndex` is arguably session/navigation state, not story state.

7. **Sprite naming is load-bearing and undocumented.** Convention: `cover.png` (thumbnail), `0.png` (title @320x200), `{scene}.{frame}.png` (scene @320x130, scenes 1-based, frames 1-based). Re-encoded independently in `StoryExporter`, `WorkingStory.CreateNew/CreateNewScene/LoadSceneSprites`, `Title` (0.png), `MenuManager` ({i}.1.png), `RM_ImagesLayout`, `RA_ScrollView` (cover.png). Centralize in one helper/constant. Note one asymmetry: `WorkingStory.SaveSprite` applies `AtariPalette.ApplyPalette` on write (`:326`) but neither load path applies it - palette is a write-time-only transform.

8. **Eager full-catalog deserialization is a real cost.** `ResourcesStoryProvider` deserializes all 7 stories (all scenes + base64 sprites, 1.4-2.6 MB JSON each) at startup, and `WorkingStory` keeps a full copy of the selected one (held twice). A unified loader should keep a lightweight catalog (id/title/sceneCount/cover) and lazy-materialize a full owned story body on selection.

9. **`index.json` is the only official-content marker AND its order is duplicated** in `RA_ScrollView.OrderStories` (a hardcoded French-title whitelist keyed on `story.id`). Make one catalog the single source for both membership and display order.

10. **The IStoryProvider abstraction is leaky.** `GetExportedStory` (the only method `WorkingStory` actually needs) is not on the interface, forcing a concrete downcast. Either put full story-fetch on the interface or inject a loader. This is the seam to burn the `StoryProviderManager` singleton.

11. **Editor coordinate mapping is desktop-resolution-baked.** `RM_ObjLayout` reads `Screen.width/height` and hardcodes 320x200 / 160/65 offsets. Works on the fixed 960x600 (3x) target but is fragile; map via the scene panel's `RectTransform`, not `Screen`. `RM_TextureScale` uses static shared buffers + a Mutex and spawns threads (single-threaded on WebGL, falls back) - verify it is never called concurrently (save + import could overlap via coroutine).

12. **Export/import stays as a SHARING feature even after persistence.** `DownloadFile` + `UploadFileContent`/`UploadFileAsBase64` (jslib) remain. But once edits auto-persist, the entire `ExportReminder` + confirm-dialog + `SetUnsavedWorkFlag` + `window._rodyHasUnsavedWork` nag machinery, plus the `download:` `LastSavePath` sentinel, become unnecessary and can be deleted.
