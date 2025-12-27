# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Rody Collection** is a Unity 6 (6000.3.2f1) recreation of classic Atari ST point-and-click adventure games with a built-in story editor. The game supports dual platforms: desktop (local storage) and WebGL (Firebase backend).

**Live URL**: https://lacrearthur.github.io/rody-collection/

## Build Commands

```bash
# Build WebGL from Unity Editor
# File > Build Settings > WebGL > Build
# Output: build/WebGL/
# Resolution: 960x600 (3x Atari ST 320x200)

# Build WebGL from CLI (used by CI)
# Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod BuildScript.Build

# Deploy to GitHub Pages (CI builds on push to master)
git push origin master  # Triggers .github/workflows/deploy-pages.yml

# Deploy Firebase rules
firebase deploy --only firestore:rules
firebase deploy --only storage:rules

# Update Firebase Storage CORS (requires gcloud CLI)
gsutil cors set cors.json gs://rody-maker.firebasestorage.app
```

**⚠️ CI Note**: Do NOT push to `master` on every commit. CI builds WebGL on each push to master. Batch commits locally and push when ready for a build.

## Local-Only Plugins

These plugins are installed locally but excluded from git (add them manually):
- **Odin Inspector** - Enhanced Unity inspector

Note: DOTween is now included in the repo for CI builds.

## Architecture

### Platform-Specific Storage
| Type | Location | Provider |
|------|----------|----------|
| Official Stories (Desktop) | `StreamingAssets/` | `LocalStoryProvider` |
| Official Stories (WebGL) | Firebase | `FirebaseStoryProvider` |
| User Stories (Desktop) | `persistentDataPath/UserStories/` | `UserStoryProvider` |
| JSON Stories (`.rody.json`) | Any location | `JsonStoryProvider` |

- **Abstraction**: `IStoryProvider` interface in `Assets/Scripts/Providers/`
- Platform detection handled by `StoryProviderManager.cs` using `#if UNITY_WEBGL && !UNITY_EDITOR`
- **JSON Stories**: Direct loading from `.rody.json` files with base64-encoded sprites (no folder extraction needed)

### User Stories Feature
User-created content is stored separately from official stories:
- **Fork-on-Edit**: Editing official story auto-forks to `UserStories/{name}_edit`
- **Export/Import**: Portable `.rody.json` format with base64 sprites
- **Key files**: `UserStoryProvider.cs`, `StoryExporter.cs`, `StoryImporter.cs`
- **See**: `docs/USER_STORIES_FEATURE.md` for full documentation

### Initialization Flow
```
Bootstrap.cs → StoryProviderManager.Initialize() → Provider ready
    ↓
Scene 0 (0_MenuCollection) loads stories via Provider
    ↓
Scene 1-5: Title → Menu → Game → Credits
Scene 6: RM_Main (Level Editor)
```

### Key Code Paths
| Path | Purpose |
|------|---------|
| `Assets/Scripts/GameManager.cs` | Main gameplay controller |
| `Assets/Scripts/RodyMaker/` | Level editor (RM_ prefix) |
| `Assets/Scripts/RodyAnthology/` | Story selection (RA_ prefix) |
| `Assets/Scripts/Providers/` | Storage abstraction layer |
| `Assets/Scripts/Models/SceneData.cs` | Typed scene data model |
| `Assets/Scripts/Utils/PathManager.cs` | Cross-platform path handling |
| `Assets/Scripts/SoundManager.cs` | Phoneme TTS and audio |
| `Assets/Editor/FirebaseMigrationTool.cs` | Upload stories to Firebase Storage |
| `Assets/Editor/StoryExportTool.cs` | Batch export stories to JSON |

### Phoneme Text-to-Speech
Custom TTS concatenates pre-recorded phonemes. Syntax: `b_r_a_v_o` (underscores between phonemes, spaces between words).
- Phoneme indices: `P.cs`
- Playback: `SoundManager.StringToPhonemes()`
- Editor: `Assets/Scripts/synth/`

### Inspector Wiring
**Critical**: Heavy use of Inspector-based wiring (BetterEvents, ScriptableObject Events, UnityEvents). Code alone won't show full behavior—check prefabs and scenes for event connections.

## Story Data Format

Stories live in `StreamingAssets/{StoryName}/`:
```
├── levels.rody     # Scene definitions (26 lines per scene, ~ separator)
├── credits.txt     # Story credits
└── Sprites/
    ├── cover.png   # Story thumbnail
    └── {scene}.{frame}.png  # Scene images (320×130)
```

Scenes use magic indices in `levels.rody`:
- Lines 0-5: Phoneme dialogues (intro1-3, obj, ngp, fsw)
- Lines 6-10: Display texts
- Line 11: Music (intro,loop)
- Line 12: Pitch values
- Line 13: isMastico flags
- Lines 14-25: Object zone positions/sizes

## Preprocessor Defines

| Define | Purpose |
|--------|---------|
| `UNITY_WEBGL` | WebGL-specific code paths |
| `MOREMOUNTAINS_NICEVIBRATIONS` | Old NiceVibrations |
| `NICEVIBRATIONS_INSTALLED` | Lofelt NiceVibrations |

## Submodules

`Assets/UnityReusables` is a git submodule with shared utilities (ScriptableObject Variables/Events, SingletonMB pattern). Commit separately if modified.

## Namespaces

```
DOOM.FPS               - FPS module (separate game)
RollToInfinity         - Ball rolling module (separate game)
UnityReusables.*       - Shared utilities
```

## Code Preferences

- Use `Path.Combine()` for all file paths
- Prefer Observable pattern over PlayerPrefs for state
- Prefer explicit code over Inspector wiring for new features
- Use conventional commits: `fix:`, `feat:`, `docs:`
- **Never null-check serialized fields** - If a prefab/reference isn't assigned in the Inspector, let it fail loudly with NullReferenceException rather than silently skipping
- **Cache static/constant data** - If generating the same data every time (e.g., blank textures, default configs), compute once and cache as static field or constant. Don't recreate identical data repeatedly.
