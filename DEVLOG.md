# Development Log

> Agentic hindsight - reverse chronological

---

## 2026-07-22: Original 1988 Speech Engine — Phase 2 (Authenticity) Complete

**Changes**:
- Preprocessor (`tools/original-extraction/preprocess.py`) made BIT-EXACT vs emulator
  ground truth: 472/472 commands (records 0,1,3). Fixes: consonant onset tail
  (b4514-b45b0) with voicing table 0x4b2a and 0x11/0x12->0x10 clamp; vowel dispatch
  fall-throughs (a 5,6 -> b4298; a>7 -> b42da).
- Bank-4 diphone matrix solved and rendered: clip index 0x45c/4 + P + 14*X, 151 real
  transition grains (the missing coarticulation).
- 0x61 decoded = 5-level AMPLITUDE envelope (x0.5..x1.5), not frequency. 0x66 = speed
  via inter-sample delay (period ~372+10*delay cycles), rendered as relative resampling.
- Silences cycle-counted: word gap 11.6ms, sentence pause 323ms (was 3x too short —
  explained the entire 13% duration gap vs original).
- Rody1 AAA.PRG extracted from rody1.st (FAT12); engine disassembled offline with capstone.
- Original speech located in existing capture (r1_full.wav @121s) + fresh Hatari capture
  of scene 2 (3 reps). All irreplaceable /tmp assets rescued into `captures/`.

**Validation**:
- Arthur blind QA (5 sentences): ~96% word accuracy by ear vs whisper 0.57 mean.
- Instant-switch A/B player vs real captures: intro "sounds perfect", scene 2 "perfection".
- Whisper transcribes our render == original, word for word, on scene 2.

**What worked**:
- Command-stream diffing (gtdiff.py) vs live-captured GT made every preprocessor bug
  visible as a clean insert/replace pattern; disasm confirmed each fix.
- Whisper as cut-verifier and duration matching as speed-calibration (no waveform xcorr needed).

**Mistakes (do not repeat)**:
- Shipped 3 music cuts to Arthur's ears without whisper-verifying them first (tool was
  already in session). Rule now in project memory: verify artifacts with the direct tool.
- Nearly fetched a YouTube longplay while the clean capture sat on disk, because a proxy
  statistic wrongly said "no speech exists". Verify absence with the mechanism, not a heuristic.
- STT mean is a LOWER BOUND on intelligibility: adding real diphones dropped whisper 0.57->0.47
  while humans heard clear improvement. Never optimize the render against whisper.

**Next**:
- Corpus alignment: 94 known texts <-> bit-exact command streams => label every grain +
  diphone cell => French-phoneme->grain table => speak NEW sentences in the authentic voice.

## 2026-03-12: Phoneme Conversion Investigation

**Changes**:
- Studied the runtime phoneme parser and token inventory in `SoundManager` / `P.cs`.
- Audited the bundled story JSONs as a text-to-phoneme corpus instead of treating them as opaque content.
- Logged the conclusion that assisted conversion is the right first implementation, not naive full automation.

**What worked**:
- The shipped stories already provide hundreds of aligned French text and phoneme examples, which is enough to bootstrap a dictionary and rule set.
- The current runtime contract is simple and stable: underscore-separated tokens, spaces between words, automatic pause per word.
- The roadmap already points in the right direction with a phoneme dictionary and learn feature.

**Hindsight**:
- The system is a custom retro spelling language, not generic French phonetics. Treating it like IPA would be wrong from the start.
- The corpus has dirty tokens (`!`, `.p`, `M`, `ca`, `il`, `w`) that the runtime does not recognize cleanly, so normalization has to happen before any converter work.
- The first useful version should generate suggestions, expose uncertainty, and let authors correct them in-editor. Pretending it can be perfect on day one would be bullshit.

## 2026-03-12: RM_Main Tooltip Prototype Stabilized

**Changes**:
- Added a reusable hover tooltip component for editor UI controls.
- Replaced the bad save-status-panel tooltip hack with a dedicated tooltip panel authored in `6_RM_Main` and wired into `RM_MainLayout`.
- Verified the pattern on two buttons first: save and intro.

**What worked**:
- A dedicated tooltip panel avoids hover flicker and keeps the user visually anchored on the hovered button.
- Auto-sizing from text plus padding produces a cleaner retro UI fit than a fixed banner.
- Edge handling matters immediately: the tooltip needs horizontal clamping and vertical flipping to stay inside the canvas.

**Hindsight**:
- Prototype one tooltip, then two, before touching the rest of a complex scene. That catches architectural mistakes early.
- Inspector-wired scene references are better than runtime-generated tooltip UI for a Unity editor screen like this; the object is visible, styleable, and debuggable in-scene.
- Animation polish should be treated as a later pass. Behavior stability matters first.

## 2026-03-12: 2_Menu Fixed Grid Placeholder Rules

**Changes**:
- Documented the 16-slot preview grid in `2_Menu` as an intentional Atari-style limitation, not a pagination bug.
- Recorded the safe runtime rule for short stories: unused slots keep placeholder art, tint their inner image dark grey, and are non-interactable.

**Root cause**:
- `MenuManager` originally only replaced preview art when a scene thumbnail existed.
- For stories with fewer than 16 scenes, unused slots could keep stale or misleading visuals and still behave like selectable scenes unless disabled explicitly.

**Hindsight**:
- Fixed-layout nostalgia UIs still need explicit empty-state rules.
- Placeholder behavior should be designed, not left as whatever the serialized scene happened to contain.
- Future refactor should move selection ownership out of `Clickable.Update()` polling and into explicit menu-state events.

## 2026-03-12: Final Scene Intro Skip Bug

**Changes**:
- Fixed `ClickHandler.NextClick()` so pressing `Next` during intro on the final scene no longer jumps straight to credits when that scene still has an object phase.
- Added a safe fallback: only skip directly to credits from intro if the current scene has no primary `obj` target phase at all.

**Root cause**:
- `ClickHandler.NextClick()` treated `CurrentSceneIndex + 1 > SceneCount` as proof that the current scene had no gameplay left.
- That assumption is wrong: the object-search phase belongs to the current scene, including the last one.
- This was easy to misdiagnose because `2_Menu.unity` also serializes `MenuManager.sceneToLoad`; manually selecting scene 2 in-editor made the bug look like malformed UGC was skipping object gameplay.

**Observed failure mode**:
- Start directly on the final scene of a 2-scene story.
- Click `Next` in intro.
- Log shows `next clicked in intro`, then credits load immediately.
- Object handlers (`Founded`, `Near`, `Miss`) never become relevant because gameplay never enters the object phase.

**Hindsight**:
- Do not decide credits routing from scene count inside intro UI handlers.
- Let scene progression own the transition: last-scene object completion can still advance to credits naturally on the next gameplay transition.
- When testing from `2_Menu`, treat serialized scene-selection state as part of the repro. The scene asset can preserve a non-default starting scene across runs.

---

## 2026-01-09: UI/UX Enhancement Planning

**Changes**: Created implementation plan for menu story slot floating action buttons

**Plan Summary** (`plan.md`):
- Fork button on official stories (top-right)
- Edit + Export buttons on user stories
- Buttons show only on selected slot
- Reuse create popup for edit mode (with delete option)
- API readiness: 95% (WorkingStory already has ForkForEditing, SetTitle, etc.)

**Key Pattern**: Use serialized fields (`RA_SlotItem` component) instead of `Transform.Find()` for button references

**Files to create/modify**:
- `RA_SlotItem.cs` (NEW) - Component with serialized button refs
- `Slot.prefab` - Add ActionButtons container
- `RA_ScrollView.cs` - Wire buttons, selection visibility
- `RA_NewGame.cs` - Edit mode, delete functionality

**Estimated effort**: 7-11 hours

---

## 2026-01-09: WebGL Image Upload Fixes (Round 2)

**Changes**:
- `StandaloneFileBrowser.jslib`: Fixed 7 `removeChild` errors (safe DOM cleanup via `parentNode`), changed `onmouseup` → `onmousedown` (4 locations) to fix double-click + ghost dialog issues
- `RM_ImagesLayout.cs`: Fixed PPU from 100 to 1f for WebGL sprite creation, added sprite persistence to WorkingStory
- `RA_NewGame.cs`: Fixed PPU, added replace confirmation dialog in `OnImportClick()`
- `RA_ScrollView.cs`: Added `slotExportPrefab` field for distinct Import/Export buttons
- Created `Assets/Prefabs/Slot Export.prefab` with "Exporter" label
- Updated `Slot LoadGame.prefab` label to "Importer"

**Learnings**:
- **Sprite keys are NOT interchangeable**: `cover.png` = menu thumbnail (set in story creation popup), `0.png` = title scene image (set in RodyMaker scene 0 editing). Both stored in JSON but saved at different times.
- **WebGL jslib timing**: `document.onmouseup` fires AFTER the handler is registered, causing double-click requirement. `document.onmousedown` triggers immediately on user gesture.
- **DOM cleanup in jslib**: Always check `element.parentNode` before `removeChild()` - elements can become detached but still found by `getElementById`.
- **Unity Sprite PPU**: PPU=100 (Unity default) makes 320x130 sprites display as 3.2x1.3 units. Use PPU=1f for pixel-perfect display.

**Hindsight**:
- When debugging WebGL file dialogs, check BOTH the jslib event handlers AND the C# callback flow
- Sprite key naming convention: `cover.png` (menu), `0.png` (title), `{scene}.{frame}.png` (gameplay)
- Import/Export buttons need separate prefabs to show distinct labels - same prefab = identical appearance

**Context**: `Assets/Scripts/RodyMaker/RM_ImagesLayout.cs`, `Assets/Scripts/RodyAnthology/RA_NewGame.cs`, `Assets/Scripts/RodyAnthology/RA_ScrollView.cs`, `Assets/StandaloneFileBrowser/Plugins/StandaloneFileBrowser.jslib`

---

## 2026-01-08: DOOM FPS Enemy Navigation System

**Changes**: Research/documentation only - no code changes

**Learnings**:
- Enemy movement: `EnemyMobile.cs` (AI state machine) + `EnemyController.cs` (NavMeshAgent wrapper)
- AI states: Patrol → Follow → Attack, transitions in `UpdateAIStateTransitions()`
- Speed configured via `NavigationModule`, applied in `EnemyController.Start():149-155`

**Hindsight**:
- Unity 6 deprecated built-in Navigation window - use **AI Navigation package** instead
- NavMeshSurface component replaces global bake settings
- To mark floors walkable: Layer → NavMeshSurface "Include Layers" → Bake

---

## 2026-01-02: URP Migration Complete

**Changes**: Migrated from Built-in Render Pipeline to URP.

**Key files**: `URPAsset.asset`, `DefaultVolumeProfile.asset`, `RA_Menu.cs`, `CameraController.cs`, `RollGameManager.cs`

**Deleted**: `Assets/Pixelation/` (legacy shaders), `Assets/URPDefaultResources/`

**Hindsight**:
- **URP upscaling filter must be Point (2)** for retro pixel effects - default Auto blurs pixels
- **QualitySettings overrides GraphicsSettings** - both must reference same URP Asset
- **Post-Processing Stack V2 → URP Volume API**: `PostProcessVolume` → `Volume`, `ColorGrading` → `ColorAdjustments`
- **Render scale trick** for pixelation: animate `URPAsset.renderScale` 0.1→1.0

---

## 2025-12-30: Reference Scanner Tool

Created `Tools > Reference Scanner` - EditorWindow for finding ScriptableObject references and unused assets.

**Features**: GUID-based project search, Build Report analysis for unused assets, bulk deletion.

**Hindsight**: BuildReport requires temp copy to Assets folder to load (API quirk).

---

## 2025-12-28: JSON-Only Migration (Complete)

**Goal**: Unified all story storage to `.rody.json`. Removed Firebase, folder-based loading, and platform-specific code.

### What Changed
- **Firebase removed** - Static JSON in Resources folder instead of HTTP requests
- **WorkingStory.cs** - Single in-memory story state, all runtime ops go through it
- **LocalStoryProvider deleted** - Both desktop and WebGL use `ResourcesStoryProvider`
- **Folder structure eliminated** - Stories are self-contained JSON with base64 sprites
- **WebGL file picker** - jslib `UploadFileContent()` + SendMessage callback pattern

### WorkingStory API (key methods)
```csharp
WorkingStory.LoadOfficial(storyId)     // From Resources
WorkingStory.LoadFromJson(json, path)  // Import user story
WorkingStory.ForkForEditing()          // Copy official for editing
WorkingStory.ExportToJson()            // Get JSON string
```

### Major Deletions
- `FirebaseStoryProvider.cs`, `LocalStoryProvider.cs`, `UserStoryProvider.cs`, `StoryImporter.cs`
- `StreamingAssets/` story folders (~1,200 files)
- Most of `PathManager.cs` (228 → 29 lines)

### Hindsight
- **`#if UNITY_WEBGL && !UNITY_EDITOR` is problematic** - evaluates FALSE in Editor regardless of build target. Use runtime detection instead.
- **Fix for simplicity, never add complexity** - When export returned 0 stories, the fix was "stories moved to `./original-stories/`" not "add backward compatibility parser"
- **Always grep ALL files** when fixing a pattern - we missed `MenuManager.cs` initially

---

## 2025-12-28: Intro Text Format Refactor

**Problem**: Intro text used embedded quotes format (`"Dialog1" "Dialog2"`) which broke on parse.

**Solution**: Separate `intro1`, `intro2`, `intro3` fields matching PhonemeDialogues pattern.

**Action**: Run `Tools > Rody > Export All Stories Now` to regenerate JSON.

---

## 2025-12-27: Editor UX Fixes

**Git workflow**: Don't push to `master` on every commit - CI builds WebGL on each push.

**Editor buttons**: Save (flash feedback), Revert (reload from disk), Test (warns unsaved), Thumbnail click (navigate or delete ≥18 scenes).

**Bugs fixed**: New scene not appearing (scenesCount timing), JSON new scene creation, object zone format.

**Key pattern**: Cache static data (blank sprite base64) instead of recreating identical textures.

---

## 2024-12-19: Firebase Trial & Removal

**Tried**: Firebase Storage + Firestore for WebGL story loading (avoid embedding 14MB in build).

**Issues**: CORS complexity, async callbacks everywhere, billing account closure (412 errors).

**Decision**: Removed Firebase entirely. Static JSON in Resources folder is simpler - official stories are read-only anyway, no need for cloud storage. Trade-off: +14MB build size, but zero runtime dependencies.

---

## 2024-12-16: Project Resurrection

**Migration**: Unity 2019 → 2022.3 LTS → Unity 6

**Cross-platform fix**: Hardcoded `\\` paths → `Path.Combine()` throughout.

**Refactoring**: Created `PathManager`, `SceneData` (typed model), `IStoryProvider` abstraction.

**Git cleanup**: Removed paid plugins from history using `git-filter-repo`.

---

## Key Discoveries

**Architecture**:
- Heavy Inspector wiring (BetterEvents, SO Events)
- `UnityReusables` is a git submodule - commit separately

**Preferences**:
- Observable pattern over SO variables for new code
- Explicit code over Inspector wiring
- Conventional commits: `fix:`, `feat:`, `docs:`

---

## Quick Reference

| Key             | Value                                           |
|-----------------|-------------------------------------------------|
| Unity Version   | 6000.3.2f1 (Unity 6 LTS)                        |
| Render Pipeline | Universal Render Pipeline (URP)                 |
| Input System    | Both legacy and new                             |
| Key Plugins     | Odin Inspector (local), DOTween, NiceVibrations |
