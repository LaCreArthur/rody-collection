# Roadmap

> Single source of truth for project progress and remaining work.
> **Updated:** 2026-01-21 (Save Awareness plan added)

---

## Current State

### Architecture (Working) ✅

```
STORY SELECTION (RA_ScrollView)
    │
    ├─► Official story → WorkingStory.LoadOfficial(storyId)
    ├─► Import button → File picker → WorkingStory.LoadFromJson()
    └─► New story → WorkingStory.CreateNew(title)
            │
            ▼
RUNTIME (Title.cs, GameManager.cs, MenuManager.cs)
    │
    └─► WorkingStory.LoadScene() / LoadSprite()  ← Single path, no branching!
            │
            ▼
EDITOR (RM_SaveLoad)
    │
    └─► WorkingStory.SaveScene() / SaveSprite()
            │
            ▼
EXPORT → WorkingStory.ExportToJson() → File save/download
```

**Unified!** All runtime story operations go through WorkingStory. No more dual code paths.

### What's Complete

| Component | Status | Notes |
|-----------|--------|-------|
| **ResourcesStoryProvider** | ✅ Done | Loads official stories from `Resources/Stories/*.rody.json` |
| **WorkingStory.cs** | ✅ Done | Full in-memory story management (load, edit, save, export) |
| **Runtime story detection** | ✅ Done | `PathManager.IsJsonStory` checks WorkingStory + path prefixes |
| **Fork-on-edit (official)** | ✅ Done | `WorkingStory.ForkForEditing()` deep copies for editing |
| **ObjectZone typed format** | ✅ Done | All 7 stories re-exported with typed floats |
| **Firebase removal** | ✅ Done | WebGL uses embedded Resources, no HTTP |

### What Was Cleaned Up (Dec 2025)

| File | Before | After | Removed |
|------|--------|-------|---------|
| `RM_SaveLoad.cs` | ~1073 lines | ~450 lines | Folder-based code paths |
| `GameManager.cs` | ~451 lines | ~290 lines | Dual init methods |
| `MenuManager.cs` | ~217 lines | ~139 lines | Dual init methods |
| `PathManager.cs` | 228 lines | 29 lines | Dead path properties |
| `RA_NewGame.cs` | ~350 lines | ~300 lines | Folder copy methods |
| `Title.cs` | ~150 lines | ~135 lines | Dual init methods |

**Deleted in Audit (Dec 2025):**
| File | Lines | Reason |
|------|-------|--------|
| `JsonStoryProvider.cs` | 483 | ✅ DELETED - Dead code, never instantiated |

**Consider Moving:**
| File | Lines | Reason |
|------|-------|--------|
| `StoryExporter.cs` | ~150 | Only `ExportedStory` model needed (move to Models/) |

### Platform Checks Remaining

`#if UNITY_WEBGL` guards in 5 files (all legitimate):

| File | Purpose | Phase 2 Work |
|------|---------|--------------|
| `RA_NewGame.cs` | import/export buttons | Needs jslib for file content |
| `RM_ImagesLayout.cs` | image import | Needs jslib for file content |
| `RM_ImgAnimLayout.cs` | animation import | Needs jslib for file content |
| `Bootstrap.cs` | WebGLResizeHandler + IsWebGL property | Keep as-is |
| `WebGLResizeHandler.cs` | Browser canvas resize via jslib | Keep as-is |

---

## Goal: Single Runtime Format

**Vision:** ALL stories load into `WorkingStory` at selection time. No runtime branching.

### Target Architecture

```
STORY SELECTION (RA_ScrollView)
    │
    ├─► Official story → WorkingStory.LoadOfficial(storyId)
    ├─► Import button → File picker → WorkingStory.LoadFromJson()
    └─► New story → WorkingStory.CreateNew(title)
            │
            ▼
RUNTIME (Title.cs, GameManager.cs, MenuManager.cs)
    │
    └─► WorkingStory.LoadScene() / LoadSprite()  ← Single path, no branching
            │
            ▼
EDITOR (RM_SaveLoad)
    │
    └─► WorkingStory.SaveScene() / SaveSprite()  ← Already working
            │
            ▼
EXPORT → WorkingStory.ExportToJson() → File save/download
```

### Benefits

| Metric | Current | Target |
|--------|---------|--------|
| Code paths | 2 (official vs user) | 1 |
| `isUserStory` checks | ~20 | 0 |
| `PathManager.IsJsonStory` uses | 7 | 0 |
| Provider files | 3 | 1 (ResourcesStoryProvider for initial load) |
| Lines of code | ~2500 storage-related | ~1500 |

---

## Remaining Work

### Phase 1: Unify Runtime to WorkingStory ✅ COMPLETE

All runtime story operations now go through WorkingStory exclusively.

| Task | Status | Notes |
|------|--------|-------|
| 1.1 `RA_ScrollView.cs` | ✅ | Loads into WorkingStory before scene transition |
| 1.2 `Title.cs` | ✅ | Single `InitFromWorkingStory()` path |
| 1.3 `GameManager.cs` | ✅ | Single `InitFromWorkingStory()` path |
| 1.4 `MenuManager.cs` | ✅ | Simplified `ForkAndEdit()` |

**Validation:**
- [x] Official story plays correctly
- [x] Scene transitions work
- [x] Thumbnails display in menu
- [x] Fork-and-edit works

### Phase 2: WebGL File Picker ✅ IMPLEMENTED

| Task | Status | Changes |
|------|--------|---------|
| 2.1 | ✅ | Added `UploadFileContent()` to jslib using `FileReader.readAsText()` |
| 2.2 | ✅ | Created `WebGLFileBrowser.cs` helper with DllImport + callbacks |
| 2.3 | ✅ | Wired `RA_NewGame.cs` import → `WorkingStory.LoadFromJson()` |
| 2.4 | ✅ | Wired `RA_NewGame.cs` export → `DownloadFile()` via helper |

**Files created:**
- `Assets/Scripts/WebGL/WebGLFileBrowser.cs` — Singleton helper with async callbacks

**Validation (requires WebGL build):**
- [ ] Import `.rody.json` on WebGL → loads into WorkingStory → can play
- [ ] Export/download `.rody.json` on WebGL → file downloads with correct content

### Phase 3: Cleanup ✅ COMPLETE

All dead code removed.

| Task | Status | Notes |
|------|--------|-------|
| 3.1 Delete `JsonStoryProvider.cs` | ✅ | Deleted Dec 2025 - 483 lines of dead code |
| 3.2 Move `ExportedStory` | ⏳ | Low priority |
| 3.3 Clean `RM_SaveLoad.cs` | ✅ | -620 lines removed |
| 3.4 Clean `PathManager.cs` | ✅ | -199 lines removed |
| 3.5 Remove `isUserStory` detection | ✅ | No more dual init methods |

**Validation:**
- [x] Clean compile
- [x] No `streamingAssetsPath` in codebase
- [x] No dual init methods in Title/GameManager/MenuManager

---

## Future (Not Blocking)

### Save Awareness & Rody Maker UX (Phase 4) - PLANNED

Improve UX to prevent users losing their custom stories.

**Plan:** [SAVE_AWARENESS_PLAN.md](SAVE_AWARENESS_PLAN.md)

**Key Changes:**
- Save button → Export directly to JSON file (clearer mental model)
- Tooltips on all Rody Maker buttons
- Exit warnings when leaving editor with unsaved work
- Browser `beforeunload` warning (WebGL)
- "Non exporté" visual indicator on user story slots
- First-time export guidance tooltip

**Decision:** IndexedDB persistence rejected (Safari eviction issues). Focus on export awareness instead.

**Estimated effort:** ~400 lines across 10 files

---

### Menu Story Slot Buttons (Phase 5) - PLANNED

Add floating action buttons to story slots in the menu.

**Plan:** [plan.md](../plan.md)

**Summary:**
- Fork button on official stories (top-right)
- Edit + Export buttons on user stories
- Buttons show only on selected slot
- Reuse create popup for edit mode

**Estimated effort:** 7-11 hours

---

### Multiple User Stories (Phase 6) - PLANNED

Support multiple user stories in-memory with per-slot action buttons.

**Plan:** `~/.claude/plans/atomic-wondering-wren.md`

**Summary:**
- `UserStoryCollection` class to hold multiple imported stories in-memory
- Each user story slot has Edit + Export buttons (top corners)
- Official story slots have Fork button
- No auto-persistence (keeps it simple)
- Import adds to collection, Export downloads specific story

**Estimated effort:** 4-5 hours

---

### Phoneme Dictionary

Natural French text → phoneme conversion for easier dialogue editing.

- Dictionary of ~500 common French words
- "Learn" feature for unknown words
- Local JSON storage

### Other

- **Export UI integration** - `OnExportClick()` in `RA_NewGame.cs` exists but no button is wired. Need to decide location: Scene 0 (alongside Import) or Scene 6 (Rody Maker save menu)
- **PlayerPrefs cleanup** - ✅ DONE: `currentScene`/`scenesCount` replaced with `WorkingStory` properties. Remaining: `gamePath`, `gameToDelete`, other legacy keys
- **SoundManager refactor** - Break up 360-line monolith

---

## Files Reference

| Path | Purpose |
|------|---------|
| `Assets/Scripts/Providers/WorkingStory.cs` | In-memory story state (THE source of truth) |
| `Assets/Scripts/Providers/ResourcesStoryProvider.cs` | Loads official stories from Resources |
| `Assets/Scripts/Providers/StoryProviderManager.cs` | Singleton access |
| `Assets/Scripts/Models/SceneData.cs` | Typed scene data model |
| `Assets/Resources/Stories/*.rody.json` | 7 official stories |

---

## Deleted/Archived

| File | Fate | Reason |
|------|------|--------|
| `LocalStoryProvider.cs` | Deleted | Replaced by ResourcesStoryProvider |
| `UserStoryProvider.cs` | Deleted | Replaced by WorkingStory |
| `StoryImporter.cs` | Deleted | No folder conversion needed |
| `FirebaseStoryProvider.cs` | Deleted | Firebase removed |
| `StreamingAssets/` stories | Archived to `original-stories/` | Now in Resources |
