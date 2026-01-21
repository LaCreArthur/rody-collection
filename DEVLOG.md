# Development Log

> Agentic hindsight - reverse chronological

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
