# Development Log

## Purpose

This document serves as a persistent development journal for tracking progress, decisions, and discoveries made during development sessions. It provides context for future work sessions (including AI-assisted sessions with Claude Code) to quickly understand:

- What has been done previously
- What problems were encountered and solved
- User preferences and coding standards
- Important discoveries about the codebase
- Ongoing tasks and next steps

## Entry Format

Each entry follows this structure:

```markdown
---

### [YYYY-MM-DD HH:MM] Brief Title

**Request**: What was asked/needed
**Actions**: What was done (bullet points)
**Learnings**: Technical discoveries, gotchas, patterns found
**Feedback**: User feedback, what worked/didn't work
**Preferences**: User preferences discovered (coding style, workflow, etc.)
**Next**: Potential follow-up tasks (optional)

---
```

## Session Log

---

### [2024-12-16 22:30] Project Resurrection - Unity 2019 to 2022.3 Migration

**Request**:
Resurrect an old Unity 2019 project, upgrade to Unity 2022.3 LTS, fix all compilation errors caused by outdated code and plugin updates (Odin Inspector, DOTween, NiceVibrations).

**Actions**:
- Fixed namespace conflicts and imports:
  - `UnityReusables.ScriptableDef` → `UnityReusables.ScriptableObjects.Variables`
  - `UnityReusables.Events` → `UnityReusables.ScriptableObjects.Events`
  - Added `DOOM.FPS` namespace to resolve AudioManager conflict
  - Added `RollToInfinity` namespace to resolve ReplaceWithPrefab conflict
- Updated NiceVibrations compatibility:
  - Old API: `MoreMountains.NiceVibrations` / `MMVibrationManager`
  - New API: `Lofelt.NiceVibrations` / `HapticPatterns`
  - Added preprocessor guards for both versions
- Fixed Odin Inspector deprecated APIs:
  - `ListDrawerSettings(Expanded=)` → `ShowFoldout`
  - `BeginShakeableGroup(key)` → keyless overload
- Added URP preprocessor guards in QualityManager (project uses Built-in RP)
- Fixed Unity API deprecations:
  - `Texture2D.Resize` → `Reinitialize`
  - `PrefabUtility.GetPrefabType` → `GetPrefabAssetType`
  - Removed obsolete `SystemInfo.supportsImageEffects` check
- Fixed cross-platform path handling in RodyMaker scripts
- Removed unused EasyMobile plugin references

**Learnings**:
- Project has a submodule: `Assets/UnityReusables` - changes need to be committed there separately
- NiceVibrations plugin changed ownership (More Mountains → Lofelt/Meta) with namespace changes
- Project uses ScriptableObject architecture heavily (Variables, Events, Managers)
- **Critical**: Much game logic is wired through Unity Inspector, not visible in code (see INSPECTOR_WIRING.md)
- Two AudioManager classes existed - one in UnityReusables (singleton), one in DOOM (simple)
- BillboardComponent had a duplicate that was deleted but Unity cache held reference

**Feedback**:
- User confirmed DOOM scene is playable after fixes
- User wanted documentation split into multiple focused files
- User prefers concise commit messages following conventional commits style

**Preferences**:
- Commit message style: conventional commits (`fix:`, `feat:`, `docs:`, `chore:`)
- Documentation: split into focused files rather than monolithic
- Code organization: use namespaces to avoid conflicts
- Step-by-step approach for complex tasks

**Discoveries**:
- Project contains multiple game prototypes: DOOM FPS, RollToInfinity, RodyMaker
- UnityReusables is a reusable library used across projects
- Heavy use of Odin Inspector for editor tooling
- ScriptableObject events (`SimpleEventSO`) used for decoupled communication
- `BetterEvents` system for inspector-wired collision/trigger callbacks
- Project uses both old and new Unity Input System

---

### [2024-12-16 23:00] Documentation Reorganization

**Request**:
Split monolithic documentation into multiple meaningful files, then create git commits with concise messages.

**Actions**:
- Created documentation structure:
  - `Docs/README.md` - Entry point with documentation index
  - `Docs/ARCHITECTURE.md` - Patterns, namespaces, preprocessor defines
  - `Docs/UNITY_REUSABLES.md` - Reusable library documentation
  - `Docs/DOOM_FPS.md` - FPS module documentation
  - `Docs/INSPECTOR_WIRING.md` - Guide for inspector-based logic
  - `Docs/THIRD_PARTY.md` - Plugin dependencies
  - `Docs/MIGRATION_2022.md` - Unity 2019 to 2022.3 changes
- Created 7 commits in main project, 3 in UnityReusables submodule
- Commits organized by logical grouping (namespace fixes, API updates, docs, config)

**Learnings**:
- Git submodules require separate commits in each repository
- Commit organization: group related changes, separate concerns

**Feedback**:
- User approved the documentation structure
- User wants a development log for tracking session progress

**Preferences**:
- Documentation should be navigable with clear index
- Important warnings (like inspector wiring) should be prominent
- Commits should be atomic and focused

---

### [2024-12-16 22:45] Cross-Platform Path Fix (Windows to macOS)

**Request**:
Fix FileNotFoundException errors when running the project on macOS after migrating from Windows. Unity was reporting backslashes in file names which are not supported.

**Actions**:
- Fixed path separators in `RM_SaveLoad.cs` - replaced `\\` with `Path.DirectorySeparatorChar` and `Path.Combine()`
- Fixed hardcoded backslash paths in:
  - `Title.cs` (line 41) - sprite loading path
  - `SceneLoading.cs` (line 47) - gamePath comparison
  - `MenuManager.cs` (line 34) - spritePath construction
  - `RM_MainLayout.cs` (lines 26-27) - path and spritePath construction
- Cleaned up StreamingAssets files that had literal backslashes in their filenames
- Updated `.gitignore` to exclude `UserSettings/` and `firebase-debug.log`

**Learnings**:
- Windows allows backslashes in paths, macOS/Linux do not
- When code writes files using backslash paths on macOS, it creates files with literal backslashes *in the filename* rather than subdirectories
- Unity's AssetDatabase scanner explicitly rejects files with backslashes in names
- `Path.Combine()` and `Path.DirectorySeparatorChar` are the correct cross-platform solutions
- The RodyMaker game was creating these malformed files on every play session because `gamePath` + `\\Sprites\\` was being used to construct save paths

**Root Cause**:
The original Windows development used hardcoded `\\` for path concatenation. On macOS, this created files with names like `Rody Et Mastico\Sprites\0.png` instead of proper directory structures.

---

### [2024-12-16 23:15] Refactoring Session: PathManager & SceneData

**Request**:
Analyze codebase for refactoring opportunities and implement quick wins while documenting future improvements.

**Actions**:
- Created `PathManager.cs` - centralized path construction utility
- Created `SceneData.cs` - 6 model classes replacing magic string arrays
- Created `SceneDataParser.cs` - centralizes index constants  
- Refactored 4 files to use PathManager
- Added `LoadSceneData()` method for structured loading
- Created `REFACTORING_ROADMAP.md` for tracking future work

**Architecture Decisions**:
- Prefer observable pattern over ScriptableObject variables
- Explicit code over Inspector wiring
- Local-first design with Firebase preparation

**Future Plans Documented**:
- IStoryProvider interface (Firebase preparation)
- Phoneme dictionary system (natural language → phonemes)
- ML exploration using existing phoneme corpus

---


## Quick Reference

### Key Files Modified This Session
- `Assets/DOOM/FPS/Scripts/` - Player, Audio, Weapons
- `Assets/UnityReusables/Scripts/` - Multiple fixes
- `Assets/RollToInfinity/Scripts/ReplaceWithPrefab.cs`
- `Assets/Pixelation/Example/Scripts/ImageEffectBase.cs`
- `Assets/Scripts/RodyMaker/` - Path handling fixes

### Known Issues
- Unity cache may show duplicate class errors → Run "Assets > Reimport All"

### Environment
- Unity: 2022.3 LTS
- Render Pipeline: Built-in (not URP)
- Key Plugins: Odin Inspector, DOTween, NiceVibrations (Lofelt)
