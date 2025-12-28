# Development Log

> Condensed session log for quick project context. See [REFACTORING_ROADMAP.md](REFACTORING_ROADMAP.md) for ongoing work.

---

## 2025-12-28: Firebase Removal & Static JSON Migration

### Goal
Remove Firebase dependency entirely. WebGL builds now use embedded JSON in Resources folder instead of HTTP requests.

### Why?
- Firebase billing account closed (412 Precondition Failed errors)
- Static JSON is simpler - no CORS, no HTTP, no async complexity
- Official stories are read-only anyway - no need for cloud storage

### Architecture Change

| Before | After |
|--------|-------|
| WebGL: Firebase Storage + Firestore | WebGL: Resources folder (embedded in build) |
| HTTP requests for each story/sprite | Synchronous Resources.Load |
| CORS configuration required | No CORS needed |
| ~14MB downloaded at runtime | ~14MB added to build size |

### New Provider: ResourcesStoryProvider

```csharp
// Stories in Assets/Resources/Stories/*.rody.json
var provider = new ResourcesStoryProvider("Stories");
var stories = provider.GetStories();  // Synchronous!
var sprite = provider.LoadSprite(storyId, "cover.png", 320, 240);
```

- Loads `index.json` listing all stories
- Parses each `.rody.json` on init
- Sprites decoded from base64 on demand
- No async callbacks needed

### Files Deleted
| File | Reason |
|------|--------|
| `FirebaseStoryProvider.cs` | No longer needed |
| `FirebaseConfig.cs` | No longer needed |
| `FirebaseMigrationTool.cs` | No longer needed |
| `WebJsonStoryProvider.cs` | Replaced by ResourcesStoryProvider |
| `firebase.json` | No Firebase |
| `.firebaserc` | No Firebase |

### Files Created
| File | Purpose |
|------|---------|
| `ResourcesStoryProvider.cs` | IStoryProvider loading from Resources folder |
| `Assets/Resources/Stories/*.rody.json` | 7 official stories + index.json |

### Files Modified
| File | Changes |
|------|---------|
| `StoryProviderManager.cs` | Uses ResourcesStoryProvider for WebGL, removed async init |
| `RA_ScrollView.cs` | `LoadCover()` now synchronous |
| `RM_SaveLoad.cs` | Removed dead Firebase async methods |
| `deploy-pages.yml` | Removed "Copy Stories" step (embedded in build) |

### Remaining Work
- [x] Update any callers of `LoadCoverAsync` to use `LoadCover` (done - `LoadCover` is synchronous)
- [ ] Test WebGL build with embedded stories
- [x] Update CLAUDE.md Firebase references (done - table shows ResourcesStoryProvider)
- [x] Review for leftover dead code (see `FIREBASE_REMOVAL_REVIEW.md`)

---

## 2025-12-27: New Scene Creation Fixes

### Bugs Fixed

**1. IndexOutOfRangeException on new scene**
- Root cause: Object zone format was `"0"` instead of `"(x,y);"`
- Fix: `JsonStoryProvider.CreateNewScene()` and `SceneDataParser.CreateGlitchScene()` now use correct format `"(0,0);"`

**2. New scene not displaying after creation**
- Root cause: Main panel kept previous scene's sprite when new scene had no sprites
- Fix: Added blank white placeholder sprite (320x130) when creating new scenes

**3. Clicking new scene triggered delete**
- Root cause: Empty scenes (no sprites) triggered delete mode on thumbnail click
- Fix: Now moot - scenes always have sprites. Added fallback check for safety.

### Key Implementation: Blank Sprite Caching

```csharp
// WRONG: Creating texture every time
private void CreateBlankSprite(int sceneIndex) {
    Texture2D tex = new Texture2D(320, 130);  // Wasteful!
    // ... fill, encode, convert every call
}

// RIGHT: Cache static data
private static string _blankSpriteBase64;
private static string BlankSpriteBase64 {
    get {
        if (_blankSpriteBase64 == null) {
            // Generate once, cache forever
        }
        return _blankSpriteBase64;
    }
}
```

**Lesson**: When generating identical data repeatedly (blank textures, default configs), compute once and cache as static field.

### Files Modified
| File | Changes |
|------|---------|
| `JsonStoryProvider.cs` | Fixed object zone format, added `CreateBlankSprite()` with cached base64 |
| `SceneDataParser.cs` | Fixed `CreateGlitchScene()` object zone format |
| `RM_MainLayout.cs` | Clear panel when no sprites (fallback), empty scene click handling |
| `RM_SaveLoad.cs` | Added `WriteToFile()` after `CreateNewScene()` |

### Planning
- Created `docs/JSON_MIGRATION_PLAN.md` - 6-phase roadmap to remove legacy folder structure
- Added ObjectZone modernization to `docs/REFACTORING_ROADMAP.md` backlog

---

## 2025-12-27: Editor UX Fixes & New Scene Creation

### Git Workflow Change
**IMPORTANT**: Do not push to `master` on every commit. CI now builds WebGL on each push to master.
- Batch commits locally, push when ready for a build
- DOTween now included in repo (needed for CI builds)

### Editor Button Behavior
| Button | Behavior |
|--------|----------|
| **Save** | Saves current scene, shows flash feedback on thumbnail |
| **Revert** | Discards unsaved changes, reloads scene from disk, stays in editor |
| **Test** | Warns about unsaved changes, loads game in play mode |
| **Thumbnail click** | Navigate to scene, OR delete (scenes ≥18 only when clicking current scene) |

### Bugs Fixed

**1. New Scene Not Appearing in Thumbnails**
- Root cause: `scenesCount` wasn't updated until save, so thumbnails didn't show new scene
- Fix: Update `scenesCount` immediately when navigating to new scene
- For JSON stories: `CreateNewScene()` creates scene entry with placeholder data before navigation

**2. JSON Story New Scene Creation**
- Added `JsonStoryProvider.CreateNewScene(sceneIndex)` - creates scene with template data
- New scenes get: "Nouveau titre", "Nouveau texte d'introduction", music from previous scene
- No sprites (expected warning until user imports image)

**3. Delete Scene** - Working correctly (was not actually broken, logs confirmed)

### Code Changes
- Renamed fields with `FormerlySerializedAs` for Inspector compatibility:
  - `miniScenes` → `sceneThumbnails`, `newScene` → `targetScene`
  - `test` → `isTestMode`, `warningText` → `messageText`
- Added `isRevertMode` flag for Revert button behavior
- Added detailed debug logging for scene operations

### Files Modified
| File | Changes |
|------|---------|
| `RM_MainLayout.cs` | Revert button, save feedback, logging |
| `RM_WarningLayout.cs` | isRevertMode, CreateNewScene call before navigation |
| `RM_SaveLoad.cs` | `CreateNewScene()` helper method |
| `JsonStoryProvider.cs` | `CreateNewScene()` with placeholder data |
| `RM_ImagesLayout.cs` | Fixed stale `miniScenes` reference |
| `CLAUDE.md` | CI note, DOTween now in repo |

---

## 2024-12-16: Project Resurrection & Refactoring

### Migration (Unity 2019 → 2022.3 LTS)
- Fixed namespace conflicts: `UnityReusables.ScriptableDef` → `UnityReusables.ScriptableObjects.Variables`
- Updated NiceVibrations: `MoreMountains` → `Lofelt.NiceVibrations`
- Fixed Odin Inspector deprecated APIs
- Added namespaces to resolve conflicts: `DOOM.FPS`, `RollToInfinity`

### Cross-Platform Fix (Windows → macOS)
- **Root cause**: Hardcoded `\\` paths created literal backslash filenames on macOS
- **Solution**: `Path.Combine()` and `Path.DirectorySeparatorChar` throughout

### Refactoring Completed
1. **PathManager** - Centralized path construction (`Assets/Scripts/Utils/`)
2. **SceneData** - Typed model replacing magic indices (`Assets/Scripts/Models/`)
3. **IStoryProvider** - Firebase-ready abstraction (`Assets/Scripts/Providers/`)

### Git Cleanup
- Removed paid plugins (Odin, DOTween) from git history using `git-filter-repo`
- Plugins added to `.gitignore` - exist locally but not in public repo

---

## 2024-12-19: Firebase WebGL Integration

### WebGL Build with Firebase Backend
- **Goal**: Load stories from Firebase instead of local StreamingAssets for WebGL builds
- **Live URL**: https://lacrearthur.github.io/rody-collection/

### Key Changes

**1. JSON Parsing Fix (WebGL)**
- `JsonUtility.FromJson` fails silently with nested arrays in WebGL
- **Solution**: Added `com.unity.nuget.newtonsoft-json` package
- Updated `FirebaseStoryProvider.cs` to use `JsonConvert.DeserializeObject`

**2. Firebase Storage CORS**
- WebGL requests blocked by CORS policy
- **Solution**: Created `cors.json` and applied with `gsutil cors set cors.json gs://rody-maker.firebasestorage.app`
- Installed Google Cloud SDK via Homebrew for `gsutil` command

**3. Cover.png Reorganization**
- Moved `cover.png` from story root to `Sprites/cover.png` for all 8 stories
- Consistent folder structure between local (StreamingAssets) and Firebase Storage
- Updated files: `RA_ScrollView.cs`, `RA_NewGame.cs`, `FirebaseMigrationTool.cs`

**4. Firebase Migration Tool Enhancements** (`Assets/Editor/FirebaseMigrationTool.cs`)
- Uploads cover from `Sprites/` folder
- "Clean Storage before upload" option to delete old files
- "Clean ALL Storage" button for full reset

**5. WebGL Display Settings**
- Resolution: 960x600 (3x Atari ST 320x200)
- Aspect ratio: 16:10 preserved in fullscreen (black bars, no stretch)
- `image-rendering: pixelated` for crisp pixel art
- Custom fullscreen using browser Fullscreen API (not Unity's SetFullscreen)

**6. GitHub Pages Deployment**
- Custom workflow (`.github/workflows/deploy-pages.yml`) skips submodule checkout
- `UnityReusables` submodule points to deleted repo - not needed for Pages
- Simplified `docs/` structure: `Build/`, `StreamingAssets/`, `TemplateData/` at root

### Current State
- WebGL build loads stories from Firebase Firestore
- Images load from Firebase Storage with CORS configured
- Desktop build still uses local StreamingAssets
- Platform detection in `StoryProviderManager.cs`

### Files Modified
| File | Purpose |
|------|---------|
| `FirebaseStoryProvider.cs` | Newtonsoft.Json for WebGL JSON parsing |
| `RM_SaveLoad.cs` | Fixed double Sprites/ path issue |
| `RA_ScrollView.cs` | Cover path: `/Sprites/cover.png` |
| `RA_NewGame.cs` | Cover path for new/import games |
| `FirebaseMigrationTool.cs` | Upload covers, clean storage |
| `docs/index.html` | WebGL loader, fullscreen handling |
| `docs/style.css` | Atari ST resolution, aspect ratio |
| `Packages/manifest.json` | Added Newtonsoft.Json |

### Pending
- Test migration tool with "Clean Storage" option on all stories
- Consider caching strategy for Firebase images

---

## 2025-12-26: JSON-Only Story Loading

### Goal
Load stories directly from `.rody.json` files without intermediate folder conversion. Eliminates the export→import→folder dance.

### How It Works
1. **Import**: Copies `.rody.json` file directly to `UserStories/`
2. **Detection**: `PathManager.IsJsonStory` checks if path ends with `.json`
3. **Loading**: `RM_SaveLoad` methods detect JSON and use `JsonStoryProvider`
4. **Sprites**: Decoded from base64 on-demand, cached in memory

### Files Created
| File | Purpose |
|------|---------|
| `JsonStoryProvider.cs` | IStoryProvider that loads directly from JSON, decodes base64 sprites |

### Files Modified
| File | Changes |
|------|---------|
| `PathManager.cs` | `IsJsonStory`, `GetUniqueJsonStoryPath()` |
| `RM_SaveLoad.cs` | JSON provider for `LoadSceneTxt()`, `LoadSceneSprites()`, `CountScenesTxt()`, `LoadCredits()` |
| `RA_ScrollView.cs` | Lists `.rody.json` files, `json:` prefix for slots |
| `RA_NewGame.cs` | Import copies JSON directly (no conversion) |
| `MenuManager.cs` | `LoadSceneThumbnail()` for JSON stories |
| `Title.cs` | `LoadTitleSprite()` for JSON stories |

### Key Implementation Details
- **Sprite.Create pixelsPerUnit**: Must be `1f` for pixel art (default is 100)
- **Slot naming**: `json:/full/path.rody.json` vs `user:foldername`
- **Static provider**: Cached in `RM_SaveLoad`, invalidated when path changes

---

## 2025-12-26: JSON Story Editing Support

### Goal
Enable full editing of JSON stories without converting to folder structure. Fork official stories before editing, edit user stories directly.

### Architecture
- **Official stories** (in StreamingAssets): Fork to UserStories before editing
- **User stories** (in UserStories): Edit directly in place
- **Location-based distinction**: `PathManager.IsOfficialStory` / `PathManager.IsUserStory`

### How It Works
1. **Fork**: `PathManager.ForkJsonStory()` copies JSON to UserStories with `_edit` suffix
2. **Edit**: Click edit button on story
   - If official → Fork first, then load editor
   - If user → Load editor directly
3. **Save**: `RM_SaveLoad.SaveGame()` → `SaveGameToJson()` → updates JSON in place
4. **Delete Scene**: `DeleteSceneFromJson()` removes scene + sprites, reindexes remaining

### Files Modified
| File | Changes |
|------|---------|
| `PathManager.cs` | `IsOfficialStory`, `ForkJsonStory()`, `GetUniqueJsonForkPath()` |
| `MenuManager.cs` | `ForkAndEdit()` updated for JSON forking |
| `JsonStoryProvider.cs` | Added save region: `SaveScene()`, `SaveSprite()`, `UpdateSceneCount()`, `WriteToFile()`, `MakeTextureReadable()` |
| `RM_SaveLoad.cs` | `SaveGameToJson()`, `DeleteSceneFromJson()`, updated `SaveGame()` and `DeleteScene()` routing |
| `RM_MainLayout.cs` | `LoadSpritesFromJson()` for editor sprite loading |

### Key Code Patterns

**JSON Save Flow**:
```
SaveGame(gm) → PathManager.IsJsonStory? → SaveGameToJson()
  ↓
JsonStoryProvider.SaveScene(sceneIndex, SceneData)
JsonStoryProvider.SaveSprite(name, Texture2D)
JsonStoryProvider.WriteToFile() → JSON serialized to disk
```

**JSON Delete Flow**:
```
DeleteScene(scene) → PathManager.IsJsonStory? → DeleteSceneFromJson()
  ↓
Remove scene from cachedStory.scenes
Remove sprites matching "scene.*.png"
Reindex subsequent scenes (scene 5 becomes 4, etc.)
WriteToFile()
```

### Hindsight
- `MakeTextureReadable()` is required before `EncodeToPNG()` on non-readable textures
- Scene deletion requires sprite reindexing to maintain consecutive indices
- `RM_MainLayout.LoadSprites()` had direct folder path usage - needed separate JSON path

---

## 2024-12-26: User Stories Feature

### Goal
Enable players to create, edit, and share personalized stories without modifying official content.

### Key Features Implemented
1. **User Stories Storage** - `Application.persistentDataPath/UserStories/`
2. **Fork-on-Edit** - Editing official stories creates a copy in user space
3. **Export/Import** - Portable `.rody.json` format with base64 sprites

### Architecture Decisions
- **Local-first approach** - No cloud storage costs, no moderation needed
- **Same file format** - User stories use levels.rody, Sprites/, credits.txt
- **Desktop-first** - WebGL (IndexedDB) planned for future iteration

### Files Created
| File | Purpose |
|------|---------|
| `UserStoryProvider.cs` | IStoryProvider for user stories + ForkStory() |
| `StoryExporter.cs` | Export to .rody.json with base64 sprites |
| `StoryImporter.cs` | Import from .rody.json files |

### Files Modified
| File | Changes |
|------|---------|
| `PathManager.cs` | UserStoriesPath, IsUserStory, GetUniqueForkName() |
| `MenuManager.cs` | ForkAndEdit() for fork-on-edit protection |
| `RA_ScrollView.cs` | Separator, user stories section, import slot |
| `RA_NewGame.cs` | OnImportClick(), OnExportClick(), user story deletion |

### Export Format (.rody.json)
```json
{
  "formatVersion": 1,
  "story": { "id": "...", "title": "...", "sceneCount": 5 },
  "scenes": [...],
  "sprites": { "cover.png": "base64...", "1.1.png": "base64..." }
}
```

### Inspector Wiring Required
On `RA_ScrollView`: assign `slotSeparatorPrefab` and `slotImportPrefab`

### See Also
- [USER_STORIES_FEATURE.md](USER_STORIES_FEATURE.md) - Full documentation

---

## Key Discoveries

**Architecture:**
- Heavy Inspector wiring - see [INSPECTOR_WIRING.md](INSPECTOR_WIRING.md)
- ScriptableObject Variables/Events pattern from `UnityReusables`
- `UnityReusables` is a git submodule - commit separately

**User Preferences:**
- Observable pattern over SO variables for new code
- Explicit code over Inspector wiring
- Conventional commits: `fix:`, `feat:`, `docs:`

---

## Quick Reference

| Key | Value |
|-----|-------|
| Unity Version | 6000.3.2f1 (Unity 6 LTS) |
| Render Pipeline | Built-in (not URP) |
| Input System | Both legacy and new |
| Key Plugins | Odin Inspector (local), DOTween, NiceVibrations |
