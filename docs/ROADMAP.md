# Roadmap

> Single source of truth for project progress and remaining work.
> **Updated:** 2026-09-06 (Arthur: audience, release sequencing, A/B status corrected)

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

### Save Awareness & Rody Maker UX (Phase 4) - ✅ DONE

Improve UX to prevent users losing their custom stories.

**Plan:** [SAVE_AWARENESS_PLAN.md](SAVE_AWARENESS_PLAN.md)

**Implemented:**
- [x] Save button → Export directly to JSON file
- [x] Exit warning when leaving editor with unsaved work
- [x] Menu return reminder (WebGL: browser confirm)
- [x] Browser `beforeunload` warning (WebGL)
- [x] Visual indicator (orange tint + asterisk) on unexported stories

**Deferred:**
- [ ] Tooltips on all Rody Maker buttons
- [ ] First-time export guidance tooltip

**Files changed:** 11 files, +231/-26 lines
- `RM_MainLayout.cs` - Save = Export
- `RM_GameManager.cs`, `RM_WarningLayout.cs` - Exit warning
- `Title.cs`, `MenuManager.cs` - Menu return reminder
- `ExportReminder.cs` (NEW) - Utility for reminders
- `StandaloneFileBrowser.jslib` - beforeunload handler
- `WorkingStory.cs` - IsDirty triggers unsaved flag update
- `RA_ScrollView.cs` - Visual indicator
- `Bootstrap.cs` - Initialize ExportReminder

---

### Menu Story Slot Buttons (Phase 5) - ✅ DONE (superseded by shared action panel)

The original plan ([plan.md](../plan.md)) was per-slot *floating* buttons. Replaced by a
single shared bottom action bar (`RA_ActionPanel`, "Selected Panel" in scene 0) that adapts
to the currently selected slot. Cleaner than floating buttons per slot.

**Implemented in `RA_ActionPanel.cs` + `RA_ScrollView.cs`:**
- [x] Edit button → "Dupliquer" (fork) on official stories, "Éditer" on user stories
- [x] Export button → enabled only for user stories (`SlotKind.UserStory`)
- [x] Import + New buttons → always available
- [x] Panel reflects the selected slot via `UpdateActionPanel()` → `Show(GetSelectedSlotKind())`
- [x] Actions fire as static events consumed by `RA_ScrollView` handlers

`plan.md` is now stale (describes the abandoned floating-button approach).

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

### Project intent & audience (Arthur, 2026-09-06)

- Origin: French streamer Benzaie played the original Rody & Mastico games live,
  adding an adult/psychedelic comedic frame over the kid stories. Arthur made a new
  episode (Rody à Ibiza) in that spirit, sent it, Benzaie streamed it and loved it.
  First big personal project, still on Arthur's resume. This leg = polish it and
  publish it properly.
- Audience: a handful of retro fans, not kids/teachers. Creators are assumed to
  have an AI agent for French→phoneme conversion (see Pillar C).

### Release sequencing (Arthur, 2026-09-06)

1. 1988 speech engine port (Pillar A below).
2. Rody Maker UX improvements.
3. Audio creator workbench (Pillar B), integrated in Rody Maker AND standalone
   (retro-voice toy for fun).
4. WebGL verification of everything, then publish. That is the finish line.

Unity upgrade: completed to 6000.5.10f1 with package/plugin updates (Arthur,
2026-09-06). Establish implementation status from the codebase; roadmap labels
can lag behind completed work.

### End Goal: Authentic 1988 Speech (defined 2026-07-24)

**One-liner:** every voice in the game is the authentic 1988 engine, and writing
French dialogue for it is easy: in-game, a phoneme workbench with instant playback;
outside, your AI agent converts French → phonemes via the shipped skill.

**Decisions (Arthur, 2026-07-24):**
- The 1988 engine **replaces** the clip-concatenation voice entirely. One
  speech system, no per-story voice flag.
- The standalone phoneme scene is a **creator tool** (authoring workbench), not a
  kids toy.
- French→phoneme conversion is the **AI-agent skill**, not an in-game
  dict/rules converter (LLM wins on typos/invented names; gap widens with time).

**Foundation (done, in `tools/original-extraction/`):** bit-exact Python replica of
the 1988 engine (`preprocess.py` + `render_all.py`), full descriptor↔phoneme table
(`catalog/phoneme_table.tsv`), and `speak.py` proving French → tokens → authentic
audio end-to-end (whisper-verified word-for-word, ear-verified "perfect" on 2 scenes; Arthur 2026-09-06: "perfect" may be overstated, more A/B tests needed before shipping the port).

**Pillar A — Engine port (prerequisite).** C# port of the two routines
(preprocessor + interpreter, ~250 lines total, pure logic, WebGL-safe), shipping
the PA.ROD banks. The existing phoneme notation keeps working: remake tokens map
onto engine descriptors via the phoneme table (exactly what `speak.py` does), so
stories re-voice without data migration. Replaces `SoundManager` concatenation.
- Validation: byte-compare C# PCM vs Python renders across all 102 dialogues, then
  ear A/B in-game.
- Voice distinction (Rody/Mastico, and the extra characters in Rody à Ibiza made
  by pitch-shifting): a pitch control on the engine is acceptable (Arthur
  2026-09-06). Must be A/B tested on the Ibiza episode too.

**Pillar B — Creator workbench scene.** Upgrade/replace the synth scene, slimmed
to what must live in-game: type/edit phonemes, instant authentic playback, copy
into story dialogue. The ear-loop is the validator (the 1988 Mastico spirit).
The saved artifact is always the phoneme string the author heard and approved —
French text never becomes a trusted intermediate anywhere.

*No in-game French→phoneme converter* (decision Arthur 2026-07-24): an LLM agent
with the conversion skill outperforms any dict+rules system on exactly the hard
cases (typos, invented names like Gobino/Badedon, prosody/liaison choices), and
that gap only widens. Building a baked dictionary + grapheme rules in Unity would
be rebuilding a worse LLM. Deferred-not-deleted: a dict-assist could return later
IF evidence shows agent-less creators need more than the ear-loop.

**Pillar C — Exportable conversion skill.** The French→phoneme path for creators
is the AI-agent skill (`.claude/skills/french-to-rody-phonemes/`), already in the
public repo. Work needed:
- Make it PORTABLE: conversion knowledge (token table, rules, corpus examples)
  self-contained; machine-local tooling (render script, whisper model paths)
  moved to a clearly separated optional section.
- Make it DISCOVERABLE: linked from README/site — "write your story dialogue with
  your AI agent, import the .rody.json".
- One source of truth: the skill itself is the shipped artifact; no duplicate
  exported copy to drift.
- Normalization guidance stays in the skill: shipped story strings contain dirty
  tokens (`!`, `.p`, `M`, `ca`, `il`) that are not canonical phonemes; alphabet
  contract is the engine descriptor set / `phoneme_table.tsv`, not the corpus.

**Sequencing:** A → B → C; each independently shippable. A is the foundation:
without it the workbench would author for the old voice. C is cheap (doc work on
an existing skill) and can land anytime.

### Other

- **Export UI integration** - `OnExportClick()` in `RA_NewGame.cs` exists but no button is wired. Need to decide location: Scene 0 (alongside Import) or Scene 6 (Rody Maker save menu)
- **PlayerPrefs cleanup** - ✅ DONE: `currentScene`/`scenesCount` replaced with `WorkingStory` properties. Remaining: `gamePath`, `gameToDelete`, other legacy keys
- **SoundManager refactor** - superseded by the 1988 engine port (Pillar A above): the phoneme-concatenation half of the monolith gets replaced, not refactored

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
