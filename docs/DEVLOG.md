# Development Log

> Condensed session log for quick project context. See [ROADMAP.md](ROADMAP.md) for ongoing work.

---

## 2025-12-28: Intro Text Format Refactor ✅

**Problem:** Intro text wasn't being saved in Rody Maker. The old format stored intro dialogs as a single string with embedded quotes (`"Dialog1" "Dialog2" "Dialog3"`), which broke when parsed on new scenes.

**Solution:** Changed to separate `intro1`, `intro2`, `intro3` fields in DisplayTexts, matching the existing pattern in PhonemeDialogues.

### Files Modified

| File | Changes |
|------|---------|
| `SceneData.cs` | Changed `DisplayTexts.intro` → `intro1`, `intro2`, `intro3` |
| `SceneDataParser.cs` | Added `ParseIntroTexts()` to parse old format into separate fields |
| `RM_GameManager.cs` | Uses separate `introText1/2/3` fields |
| `RM_DialLayout.cs` | Simplified `GetDialText()`/`SetDialText()` to use separate fields directly |
| `RM_SaveLoad.cs` | Updated `GameManagerToSceneData()` to save intro1/2/3 |
| `RM_DialoguesLayout.cs` | `SetDialButtons()` checks if intro texts are non-empty |
| `RM_TextInput.cs` | Directly writes to `introText1/2/3` instead of parsing quotes |
| `GameManager.cs` | Added `CombineIntroTexts()` helper for gameplay display |
| `WorkingStory.cs` | Updated default scene creation to use intro1/2/3 |

**Action Required:** Run `Tools > Rody > Export All Stories Now` to regenerate JSON files.

---

## 2025-12-28: PlayerPrefs Cleanup ✅

**Goal:** Replace PlayerPrefs state passing with WorkingStory properties.

### Changes Made

| File | Changes |
|------|---------|
| `WorkingStory.cs` | Added `CurrentSceneIndex` property, reset in `Clear()` |
| `RM_GameManager.cs` | Replaced `PlayerPrefs currentScene/scenesCount` → `WorkingStory` |
| `GameManager.cs` | Replaced `PlayerPrefs.GetInt("currentScene")` → `WorkingStory.CurrentSceneIndex` |
| `MenuManager.cs` | Replaced 4x `PlayerPrefs.SetInt("currentScene")` |
| `ClickHandler.cs` | Replaced all `PlayerPrefs currentScene/scenesCount` |
| `RM_WarningLayout.cs` | Replaced all `PlayerPrefs currentScene/scenesCount` |
| `Intro.cs` | Replaced `PlayerPrefs.GetInt("scenesCount")` |
| `RM_SaveLoad.cs` | Removed redundant `SetSceneCount` and scenesCount PlayerPrefs |
| `RM_MainLayout.cs` | Replaced 3x `PlayerPrefs.GetInt("scenesCount")` |

### Result

- `grep -rn 'PlayerPrefs.*"currentScene"' Assets/Scripts/` → **No matches**
- `grep -rn 'PlayerPrefs.*"scenesCount"' Assets/Scripts/` → **No matches**
- Unity compiles without errors ✅

---

## 2025-12-28: Phase 2 - WebGL File Picker

**Goal:** Enable import/export of `.rody.json` stories on WebGL builds.

### Implementation

**Problem:** The existing `UploadFile()` jslib function returns blob URLs via `URL.createObjectURL()`, which Unity WebGL can't read content from.

**Solution:** Added `UploadFileContent()` that uses `FileReader.readAsText()` to return actual file content as a string.

### Files Created

| File | Purpose |
|------|---------|
| `Assets/Scripts/WebGL/WebGLFileBrowser.cs` | Singleton helper with DllImport declarations and async callbacks |

### Files Modified

| File | Changes |
|------|---------|
| `StandaloneFileBrowser.jslib` | Added `UploadFileContent()` function |
| `RA_NewGame.cs` | WebGL import uses `WebGLFileBrowser.OpenFileAsText()` → `WorkingStory.LoadFromJson()` |
| `RA_NewGame.cs` | WebGL export uses `WebGLFileBrowser.DownloadTextFile()` |

### Key Pattern: jslib + SendMessage Callback

```javascript
// jslib reads file as text and sends to Unity
UploadFileContent: function(gameObjectName, methodName, filter) {
    var reader = new FileReader();
    reader.onload = function(e) {
        SendMessage(gameObjectName, methodName, e.target.result);
    };
    reader.readAsText(file);
}
```

```csharp
// C# receives callback with file content
public void OnFileContentLoaded(string content) {
    WorkingStory.LoadFromJson(content, null);
}
```

### Validation Pending

- [ ] WebGL build and test import
- [ ] WebGL build and test export/download

---

## 2025-12-28: Completed Unify Runtime to WorkingStory

**Goal:** Remove ALL folder-based loading - all runtime story ops go through WorkingStory exclusively.

### Major Refactoring

| File | Before | After | Changes |
|------|--------|-------|---------|
| `RM_SaveLoad.cs` | ~1073 lines | ~450 lines | Removed all folder-based fallbacks |
| `GameManager.cs` | ~451 lines | ~290 lines | Single InitFromWorkingStory() path |
| `MenuManager.cs` | ~217 lines | ~139 lines | Simplified ForkAndEdit() |
| `Title.cs` | ~150 lines | ~135 lines | Single InitFromWorkingStory() path |
| `PathManager.cs` | 228 lines | 29 lines | Keeps only UserStoriesPath |
| `RA_NewGame.cs` | ~350 lines | ~300 lines | Removed dead folder-copy methods |

### WorkingStory Additions

- Added `DeleteScene()` method for scene deletion support

### Dead Code Removed

- All StreamingAssets paths
- LocalStoryProvider references
- Folder-based sprite loading (`LoadSprite(path, scene, w, h)`)
- Legacy folder deletion in RA_NewGame (CopyGameFolder, CopyRodyBaseFolder, CopyCoverImage)
- PathManager: GamePath, IsJsonStory, IsOfficialStory, SpritesPath, LevelsFile, CreditsFile, GetSpritePath, etc.

### Hindsight

- **All runtime story data flows through WorkingStory.** No fallbacks, no folder paths.
- Stories are loaded into WorkingStory in `RA_ScrollView.LoadFolder()` BEFORE scene transitions
- Every scene now checks `WorkingStory.IsLoaded` first and returns to scene 0 if false
- The only remaining file system operations are: user story JSON listing (PathManager.UserStoriesPath) and file picker/export

### Verification

```bash
# These patterns should return no matches in Assets/Scripts/
grep -r "streamingAssetsPath" Assets/Scripts/  # ✅ None
grep -r "LocalStoryProvider" Assets/Scripts/   # ✅ None
```

---

## 2025-12-28: JSON-Only Migration (In Progress)

### Goal
Unify all story storage to `.rody.json` with single code path. No more platform-specific storage.

### Phase 1 ✅ COMPLETE - Unified Official Stories

**Changes:**
- `StoryProviderManager` now always uses `ResourcesStoryProvider` (removed `#if UNITY_WEBGL`)
- Deleted `LocalStoryProvider.cs` (296 lines of folder-based loading)
- Deleted all `StreamingAssets/` story folders (backed up to `original-stories/`)
- Both desktop and WebGL now load official stories identically

**Files Deleted:**
| File | Lines |
|------|-------|
| `LocalStoryProvider.cs` | 296 |
| `StreamingAssets/` stories | ~1,200 files |

### Hotfix: Editor + WebGL Target Loading Issue

**Problem:** Official stories didn't load when running in Editor with WebGL build target.

**Root Cause:** `#if UNITY_WEBGL && !UNITY_EDITOR` evaluates to FALSE in Editor (even with WebGL target), so code fell back to folder-based loading paths that no longer exist.

**Solution:** Removed platform checks from loading code - now both official and user stories use runtime detection:

```csharp
// Detect story type by path format
bool isUserStory = gamePath.StartsWith("json:") || gamePath.StartsWith("user:") ||
                   gamePath.Contains("/") || gamePath.Contains("\\");

if (isUserStory)
    InitUserStory();    // File-based loading
else
    InitFromProvider(); // ResourcesStoryProvider
```

**Files Fixed:**
| File | Changes |
|------|---------|
| `RA_ScrollView.cs` | Unified `Start()` → `InitWithProvider()`, removed `Init()` |
| `GameManager.cs` | Added story type detection, renamed `InitSceneWebGL()` → `InitSceneFromProvider()` |
| `Title.cs` | Added story type detection, renamed methods |
| `MenuManager.cs` | Added story type detection, renamed `init()` → `InitUserStory()`, `InitWebGL()` → `InitFromProvider()` |

**Key Insight:** Official stories now use just the story ID (e.g., "Rody Et Mastico") as `gamePath`, while user stories use full paths or prefixed paths.

### Phase 2 ✅ COMPLETE - WorkingStory Class

**Created `WorkingStory.cs`** - Static class for in-memory story state:

```csharp
public static class WorkingStory
{
    public static ExportedStory Current { get; }  // One story in memory
    public static bool IsOfficial { get; }         // Read-only until forked
    public static bool IsDirty { get; }            // Unsaved changes?
    public static string LastSavePath { get; }     // Quick-save location

    // Loading
    public static void LoadOfficial(string storyId);
    public static void LoadFromJson(string json, string savePath);
    public static void CreateNew(string title);

    // Editing
    public static void ForkForEditing();  // Deep copy, mark as user story
    public static void SaveScene(int index, SceneData data);
    public static void SaveSprite(string name, Texture2D texture);

    // Export
    public static string ExportToJson();
}
```

**Key Design:**
- ONE story in memory at a time (volatile)
- Official stories are read-only until forked
- User stories exist only as `.rody.json` files (no persistent directory)
- Export = Save (same operation)

**✅ Integrated:**
- `MenuManager.cs` - `ForkAndEdit()` uses `WorkingStory.ForkForEditing()` (works on ALL platforms including WebGL)
- `RA_NewGame.cs`:
  - `NG_OnAcceptClick()` uses `WorkingStory.CreateNew()` (works on ALL platforms)
  - `OnImportClick()` uses `WorkingStory.LoadFromJson()` (desktop only, WebGL file picker TODO)
  - `OnExportClick()` uses `WorkingStory.ExportToJson()` (desktop only, WebGL download TODO)
- `PathManager.cs` - `IsJsonStory` now checks `WorkingStory.IsLoaded` + `memory:`/`json:` prefixes
- `RM_SaveLoad.cs` - All load/save methods now delegate to `WorkingStory` when loaded:
  - `LoadTitleSprite()`, `LoadSceneThumbnail()`, `LoadSceneSprites()` → `WorkingStory.LoadSprite()`
  - `LoadSceneData()`, `LoadSceneTxt()` → `WorkingStory.LoadScene()`
  - `SaveGameToJson()` → new `SaveGameToWorkingStory()` method
  - `CreateNewScene()` → `WorkingStory.CreateNewScene()`
  - `CountScenesTxt()` → `WorkingStory.SceneCount`

**Deleted:**
- `UserStoryProvider.cs`, `StoryImporter.cs` - no longer needed
- `RM_MainLayout.cs` - removed WebGL save check (now works on all platforms via WorkingStory)

**Bug Fixed:** Fork-and-edit for official stories failed because `PathManager.IsJsonStory` only checked `.json` extension, but `gamePath` was `memory:xxx`. Fixed by checking `WorkingStory.IsLoaded` first.

**Remaining WebGL checks (10, all legitimate):**
- Bootstrap.cs - WebGLResizeHandler + IsWebGL property
- WebGLResizeHandler.cs - browser integration
- RM_ImagesLayout, RM_ImgAnimLayout, RA_NewGame - file browser (needs jslib wrapper)

**Next:**
- Add WebGL file browser support (jslib wrapper for upload/download)
- Delete `JsonStoryProvider.cs` (still used for listing user stories)

### ObjectZone Refactoring ✅ COMPLETE

**Goal:** Replace raw string format (`"(-1.8, 0.0);"`) with typed floats in JSON.

**Before (raw strings in JSON):**
```json
"objects": {
  "obj": {
    "positionRaw": "(-1.8, 0.0);",
    "sizeRaw": "(13.5, 14.0);",
    "nearPositionRaw": "(115.3, -36.0);",
    "nearSizeRaw": "(62.5, 47.0);"
  }
}
```

**After (typed floats):**
```json
"objects": {
  "obj": {
    "x": -1.8, "y": 0.0,
    "width": 13.5, "height": 14.0,
    "nearX": 115.3, "nearY": -36.0,
    "nearWidth": 62.5, "nearHeight": 47.0
  }
}
```

**Files Modified:**
| File | Changes |
|------|---------|
| `SceneData.cs` | ObjectZone now uses typed floats |
| `SceneDataParser.cs` | Parses raw strings → typed floats |
| `GameManager.cs` | Added `CreateZoneList()` for typed data |
| `RM_SaveLoad.cs` | Updated save/load for typed format |
| `StoryImporter.cs` | Format typed floats for folder export |
| `JsonStoryProvider.cs` | Use typed defaults |
| `StoryExportTool.cs` | Export from `./original-stories` to `Resources/Stories/` |

**Re-exported all 7 stories** with new typed format.

### Learnings

**⚠️ CRITICAL: Fix for simplicity, never add complexity**

When the story export returned 0 stories, the WRONG approach was to add backward compatibility to parse the old format. The CORRECT approach was to investigate: "Why no exports? → Stories moved to `./original-stories/` → Fix the export path."

❌ **Wrong thinking:** "Old JSON format → Add backward compatibility parser"
✅ **Correct thinking:** "Old JSON format → Re-export with correct format"

This applies broadly:
- Don't add complexity to handle broken states
- Investigate root cause before adding code
- The simplest fix is usually correct

**⚠️ CRITICAL: Always grep ALL files when fixing a pattern**
When removing `#if UNITY_WEBGL` checks, we missed `MenuManager.cs` initially because we only fixed files we noticed during testing. This caused a runtime error when the scene menu loaded. **Always run `grep -r "UNITY_WEBGL" Assets/Scripts/` to find ALL occurrences before declaring a fix complete.**

**`#if UNITY_WEBGL && !UNITY_EDITOR` is problematic:**
- Evaluates to FALSE in Editor regardless of build target
- Can't test WebGL code paths in Editor
- **Solution:** Use runtime detection instead of compile-time switches

**PlayerPrefs("gamePath") patterns:**
- Official stories: just story ID (e.g., `"Rody Et Mastico"`)
- User folders: full path (e.g., `/Users/.../UserStories/mystory`)
- JSON stories: prefixed (e.g., `json:/path/to/story.rody.json`)
- Use path separators/prefixes to detect story type at runtime

**Remaining platform checks** (7 files still have `#if UNITY_WEBGL`):
- `Bootstrap.cs` - WebGLResizeHandler (OK to keep - WebGL-specific feature)
- `MenuManager.cs` - ✅ FIXED (uses WorkingStory.ForkForEditing)
- `RM_MainLayout.cs` - RodyMaker editor save button (needs WorkingStory)
- `RM_ImagesLayout.cs` - RodyMaker editor file import (needs jslib)
- `RA_NewGame.cs` - 🔄 PARTIAL (new/import use WorkingStory, file picker needs jslib)
- `WebGLResizeHandler.cs` - OK (WebGL-only component)
- `RM_ImgAnimLayout.cs` - RodyMaker editor file import (needs jslib)

### Files Modified
| File | Changes |
|------|---------|
| `StoryProviderManager.cs` | Removed platform checks, always ResourcesStoryProvider |
| `ResourcesStoryProvider.cs` | Added `GetExportedStory()` for WorkingStory |
| `RA_ScrollView.cs` | Unified initialization, removed folder-based code |
| `GameManager.cs` | Runtime story type detection |
| `Title.cs` | Runtime story type detection |
| `MenuManager.cs` | Runtime story type detection |

### Files Created
| File | Purpose |
|------|---------|
| `WorkingStory.cs` | In-memory story state management |
| `original-stories/` | Backup of StreamingAssets stories |

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
