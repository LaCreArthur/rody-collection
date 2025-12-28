# JSON Migration Plan

> Goal: Unify all story storage to `.rody.json` format with a single code path for all platforms.
> **Updated: 2025-12-28** - Simplified to truly uniform architecture

---

## Current State (Messy)

| Component | Desktop | WebGL |
|-----------|---------|-------|
| Official Stories | `StreamingAssets/` folders | `Resources/Stories/*.rody.json` |
| User Stories | `persistentDataPath/` folders | N/A |
| Providers | `LocalStoryProvider`, `UserStoryProvider`, `JsonStoryProvider` | `ResourcesStoryProvider` |

**Problem**: 4 providers, 2 formats, platform-specific paths = complexity.

---

## Target Architecture (Simple)

**Single path for ALL platforms:**

```
Official Stories → Resources/Stories/*.rody.json (embedded in build, read-only)
User Stories     → Import/Export .rody.json files (no persistent storage)
```

**One provider:**
```
StoryProviderManager
    └── ResourcesStoryProvider (loads from Resources/Stories/)
```

**User workflow:**
1. **Import**: User selects `.rody.json` file → loaded into memory for play/edit
2. **Export**: User saves edited story → downloads/saves `.rody.json` file
3. **No persistent user directory** - stories live as files on user's device

**Delete everything else:**
- `LocalStoryProvider.cs`
- `UserStoryProvider.cs`
- `JsonStoryProvider.cs` (merge needed parts into ResourcesStoryProvider)
- `StoryImporter.cs`
- `StreamingAssets/` story folders

---

## Why This Is Better

| Aspect | Before | After |
|--------|--------|-------|
| Providers | 4 | 1 |
| Platform-specific code | Lots of `#if UNITY_WEBGL` | None |
| User story storage | Platform-dependent | Portable JSON files |
| Sharing stories | Export then share | Just share the file |
| Code paths | Desktop ≠ WebGL | Identical |

---

## Story Editing Flow (Detailed)

### Key Question: Where Does the Edited Story Live?

**Answer: In memory only, until explicitly exported.**

The game holds ONE "working story" in memory at a time. This is the story being played or edited. It's volatile - if the user closes the game without exporting, changes are lost.

### Scenarios

#### 1. User Plays an Official Story
```
User selects "Rody Et Mastico" → Load from Resources into memory → Play
                                                                    ↓
                                                              Game closes
                                                                    ↓
                                                              Nothing saved (read-only, no changes)
```
- Official stories are read-only
- No persistence needed
- Memory cleared on exit

#### 2. User Edits an Official Story (Fork)
```
User selects "Rody Et Mastico" → clicks "Edit"
        ↓
    Fork to memory (deep copy)
        ↓
    Working story now = copy of official story
    Working story marked as "user story" (not official)
    Working story has new name: "Rody Et Mastico (copie)"
        ↓
    User edits in RodyMaker
        ↓
    Changes saved to in-memory working story
        ↓
    User clicks "Export" → Save dialog → user picks location → .rody.json saved
        ↓
    OR user closes without export → changes lost (warn user!)
```

#### 3. User Imports a .rody.json File
```
User clicks "Import" → File picker → selects "MyStory.rody.json"
        ↓
    Parse JSON → Load into memory as working story
        ↓
    User can Play or Edit
        ↓
    If edited → changes in memory only
        ↓
    User clicks "Export" → Save dialog → .rody.json saved
        ↓
    OR user closes without export → changes lost (warn user!)
```

#### 4. User Creates a New Story
```
User clicks "New Story" → enters title "Mon Histoire"
        ↓
    Create blank story in memory
    - 1 scene with placeholder content
    - Default cover image
        ↓
    RodyMaker opens automatically
        ↓
    User edits
        ↓
    User clicks "Export" → Save dialog → .rody.json saved
        ↓
    OR user closes without export → story lost (warn user!)
```

### In-Memory State

```csharp
public static class WorkingStory
{
    // The currently loaded story (one at a time)
    public static ExportedStory Current { get; private set; }
    
    // Is this an official story? (affects edit behavior - must fork first)
    public static bool IsOfficial { get; private set; }
    
    // Has the story been modified since load/last save?
    public static bool IsDirty { get; private set; }
    
    // Last save location (null = never saved, prompt "Save As")
    // Non-null = can quick-save to same location
    public static string LastSavePath { get; private set; }
    
    // Story title for display
    public static string Title => Current?.story?.title ?? "Sans titre";
    
    // Load official story from Resources (read-only until forked)
    public static void LoadOfficial(string storyId) { ... }
    
    // Load from JSON string (imported file)
    public static void LoadFromJson(string json, string savePath = null) { ... }
    
    // Fork for editing (deep copy, mark as non-official)
    public static void ForkForEditing() { ... }
    
    // Create new blank story
    public static void CreateNew(string title) { ... }
    
    // Update scene data (marks dirty)
    public static void SaveScene(int sceneIndex, SceneData data) { ... }
    
    // Export to JSON string
    public static string ExportToJson() { ... }
    
    // Save to file (updates LastSavePath, clears dirty)
    public static void SaveToFile(string path) { ... }
    
    // Clear on exit
    public static void Clear() { ... }
}
```

### UI Flow (Simplified)

**Main Menu:**
- Official stories list (from Resources)
- [Import] button → file picker → load JSON into memory
- [New] button → create blank story in memory → go to editor

**No multiple stories loaded** - one at a time is sufficient.

**Editor (RodyMaker):**
- [Save] button:
  - First time OR imported without path → "Save As" dialog
  - Subsequent saves → overwrite `LastSavePath` directly (quick save)
- [Save As] button (optional) → always show dialog
- [Export] could just be renamed to [Save] since it's the same thing

**Save behavior:**
```
User clicks Save:
  if (LastSavePath != null)
    → Quick save to LastSavePath
  else
    → Show "Save As" dialog
    → User picks location
    → Save and remember LastSavePath
```

### What's NOT Needed

1. **Export button in main menu** - Current code has this for exporting user stories from UserStories folder. After migration, there's no UserStories folder, so this is unnecessary. Users save from the editor.

2. **Multiple story support** - One story in memory at a time is enough.

3. **Separate "Save" vs "Export"** - They're the same operation (write JSON to disk). Just call it "Save".

### Persistence Summary

| Scenario | Persistent? | Where? |
|----------|-------------|--------|
| Play official story | No | Memory only |
| Edit official story | No | Memory only (until export) |
| Import user story | No | Memory only (until export) |
| Create new story | No | Memory only (until export) |
| Export | Yes | User's file system (user chooses location) |

### WebGL Considerations

- **Import**: Use browser file picker API (already works via StandaloneFileBrowser WebGL implementation)
- **Export**: Trigger browser download of .rody.json file
- **No persistent storage**: Same as desktop - memory only

### Edge Cases

1. **User tries to edit while playing**: Not allowed - must exit to menu first
2. **User imports while editing**: Warn about unsaved changes, then load new
3. **User creates new while editing**: Warn about unsaved changes, then create new
4. **Browser refresh (WebGL)**: Changes lost - acceptable (same as closing desktop app)
5. **Large story (memory)**: Should be fine - JSON files are typically < 20MB

---

## Feasibility Analysis (Code Review)

### ✅ What Already Exists

**1. JSON Export/Import Infrastructure**
- `StoryExporter.cs` - Already exports to `.rody.json` with base64 sprites
- `StoryExporter.ExportedStory` - Data model for JSON (formatVersion, story, scenes, sprites)
- `JsonStoryProvider.cs` - Already loads/saves JSON, has `SaveScene()`, `SaveSprite()`, `WriteToFile()`
- `ResourcesStoryProvider.cs` - Already loads from `Resources/Stories/` for WebGL

**2. WebGL File Picker**
- `StandaloneFileBrowser.jslib` already has:
  - `UploadFile()` - Opens file picker, returns file via `URL.createObjectURL`
  - `DownloadFile()` - Triggers browser download with byte array
- Works cross-platform (Mac, Windows, Linux, WebGL)

**3. In-Memory Editing**
- `JsonStoryProvider` already maintains `cachedStory` in memory
- `SaveScene()` updates cache, `WriteToFile()` persists
- `RM_SaveLoad.GetJsonProvider()` maintains static provider instance

**4. Dirty State Detection**
- `RM_GameManager.modified` flag exists but underused
- `RM_WarningLayout` already shows unsaved changes warnings

### ⚠️ Challenges & Required Changes

**1. PlayerPrefs("gamePath") Dependency** - 🔴 MAJOR
- 12 files reference `PlayerPrefs.GetString("gamePath")`
- 7 files call `PlayerPrefs.SetString("gamePath", ...)`
- Currently stores FILE PATH, needs to change to STORY ID or be removed

**Solution**: Replace `gamePath` with `WorkingStory` static class:
```csharp
// Instead of: PathManager.GamePath (reads PlayerPrefs)
// Use: WorkingStory.Current (in-memory story)
```

Files to update:
- `GameManager.cs` (3 refs)
- `Title.cs` (3 refs)
- `MenuManager.cs` (4 refs)
- `RM_SaveLoad.cs` (2 refs)
- `RA_ScrollView.cs` (2 refs)
- `RA_NewGame.cs` (1 ref)
- `SceneLoading.cs` (1 ref)
- `PathManager.cs` (source of GamePath)

**2. RM_SaveLoad Static Class** - 🟡 MEDIUM
- 921 lines, mixes JSON and folder paths
- `GetJsonProvider()` creates provider based on `PathManager.GamePath`
- After migration: Should work with `WorkingStory.Current` directly

**Current flow:**
```
RM_SaveLoad.SaveGame() 
  → PathManager.IsJsonStory? 
    → SaveGameToJson() → JsonStoryProvider.SaveScene() → WriteToFile()
```

**Target flow:**
```
RM_SaveLoad.SaveGame()
  → WorkingStory.SaveScene() → (in memory only)
  
User clicks Export:
  → WorkingStory.ExportToJson() → StandaloneFileBrowser.SaveFilePanel()
```

**3. Fork Logic** - 🟢 EASY
- `PathManager.ForkJsonStory()` already copies JSON to UserStories
- `UserStoryProvider.ForkStory()` copies folders
- After migration: Fork = deep copy `ExportedStory` in memory, change title

**4. WebGL Import** - 🟡 MEDIUM
- `StandaloneFileBrowser.jslib` uses `URL.createObjectURL()` which returns blob URL
- Need to read file content, not just URL
- May need to use `FileReader` API in jslib

**Current WebGL limitation:**
```javascript
// Returns: "blob:http://localhost/abc-123"
// Need: actual file content as string
```

**Solution**: Update jslib to use `FileReader.readAsText()`:
```javascript
var reader = new FileReader();
reader.onload = function(e) {
    SendMessage(gameObjectName, methodName, e.target.result);
};
reader.readAsText(event.target.files[0]);
```

**5. Scene Count via PlayerPrefs** - 🟡 MEDIUM
- `PlayerPrefs.GetInt("scenesCount")` used throughout
- Should come from `WorkingStory.Current.story.sceneCount`

Files affected:
- `RM_GameManager.cs`
- `RM_MainLayout.cs`
- `RM_SaveLoad.cs`
- `MenuManager.cs`

### 🎯 Simplifications Discovered

**1. Can Delete Entirely:**
- `LocalStoryProvider.cs` (296 lines) - Desktop will use ResourcesStoryProvider
- `UserStoryProvider.cs` (421 lines) - No persistent user storage
- `StoryImporter.cs` (~200 lines) - No folder conversion needed
- `PathManager` folder properties (SpritesPath, LevelsFile, CreditsFile)

**2. Can Simplify:**
- `StoryProviderManager.cs` - No platform switching, always ResourcesStoryProvider
- `RM_SaveLoad.cs` - Remove all folder code (~400 lines)
- `PathManager.cs` - Remove UserStoriesPath, ForkJsonStory, etc.

**3. JsonStoryProvider → WorkingStory:**
- Rename and simplify to be purely in-memory
- Remove file path dependencies
- Add `IsDirty` flag and dirty tracking

### 📊 Complexity Assessment

| Phase | Effort | Risk | Blockers |
|-------|--------|------|----------|
| Phase 1: ResourcesStoryProvider everywhere | Medium | Low | None |
| Phase 2: Import/Export flow | Medium | Medium | WebGL FileReader |
| Phase 3: WorkingStory class | Medium | Low | None |
| Phase 4: Remove folder code | Large | Low | Thorough testing |
| Phase 5: Cleanup | Small | Low | None |

**Total Estimate**: ~3-4 days of focused work

### 🚨 Breaking Changes

1. **User stories in UserStories/ folder will be orphaned**
   - Need migration notice or one-time export tool
   
2. **PlayerPrefs("gamePath") no longer used**
   - Any external tools/scripts using this will break
   
3. **Folder-based stories no longer supported**
   - StreamingAssets folders become backup only

---

## Migration Phases

### Phase 1: Unify to ResourcesStoryProvider
**Risk: Low | Effort: Medium**

Make desktop use `ResourcesStoryProvider` like WebGL already does.

**Steps:**
1. Ensure all official stories are in `Resources/Stories/*.rody.json`
2. Update `StoryProviderManager` - remove platform checks:
   ```csharp
   public static IStoryProvider Provider
   {
       get
       {
           if (_provider == null)
               _provider = new ResourcesStoryProvider("Stories");
           return _provider;
       }
   }
   ```
3. Delete `LocalStoryProvider.cs`
4. Delete `StreamingAssets/` story folders (backup externally first)

**Files to delete:**
- `LocalStoryProvider.cs`
- `StreamingAssets/Rody Et Mastico/` (and all other story folders)

**Validation:**
- [ ] Desktop loads official stories from Resources
- [ ] WebGL unchanged
- [ ] Same code path both platforms

---

### Phase 2: Import/Export Only for User Stories
**Risk: Medium | Effort: Medium**

Remove persistent user story storage. Users import/export JSON files.

**Steps:**
1. Update `RA_ScrollView` - only show official stories + "Import" button
2. Update Import flow:
   - User picks `.rody.json` file
   - Load into memory (don't copy to persistent storage)
   - Set as current story for play/edit
3. Update Export flow:
   - Save current story state to `.rody.json`
   - User chooses save location (or download on WebGL)
4. Delete `UserStoryProvider.cs`
5. Delete `StoryImporter.cs`
6. Remove `PathManager.UserStoriesPath` and related code

**Files to delete:**
- `UserStoryProvider.cs`
- `StoryImporter.cs`

**Files to modify:**
- `RA_ScrollView.cs` - Remove user story listing, keep import button
- `RA_NewGame.cs` - Create story in memory, export to save
- `PathManager.cs` - Remove user story paths

**Validation:**
- [ ] Can import `.rody.json` and play
- [ ] Can edit and export to new `.rody.json`
- [ ] No files written to persistent storage
- [ ] Works identically on Desktop and WebGL

---

### Phase 3: Simplify JsonStoryProvider
**Risk: Low | Effort: Small**

`JsonStoryProvider` becomes a simple in-memory story holder for imported/new stories.

**Steps:**
1. Rename to `EditableStoryProvider` or merge into a simpler class
2. Remove file path dependencies - works purely in memory
3. Add `LoadFromJson(string json)` and `ExportToJson()` methods
4. Remove `WriteToFile()` - export goes through user action

**Files to modify:**
- `JsonStoryProvider.cs` → simplify to in-memory only

**Validation:**
- [ ] Import loads JSON string into memory
- [ ] Edit modifies in-memory state
- [ ] Export serializes to JSON string

---

### Phase 4: Clean Up RM_SaveLoad
**Risk: Medium | Effort: Medium**

Remove all folder-based code from `RM_SaveLoad.cs`.

**Steps:**
1. Remove folder save code:
   - `SaveSprite()` method
   - `WriteToTxt()` method
   - All `Path.Combine()` for folder paths
2. Remove folder load code:
   - Folder paths in `LoadSceneTxt()`
   - Folder paths in `LoadSceneSprites()`
3. All save/load goes through in-memory provider

**Files to modify:**
- `RM_SaveLoad.cs` - Remove ~400 lines of folder code

**Validation:**
- [ ] Save updates in-memory state
- [ ] Load reads from in-memory state
- [ ] No file I/O except import/export

---

### Phase 5: Final Cleanup
**Risk: Low | Effort: Small**

1. Remove `PathManager` folder properties (`SpritesPath`, `LevelsFile`, etc.)
2. Remove `isGameFolder()` from `RA_ScrollView.cs`
3. Remove all `#if UNITY_WEBGL` platform checks for storage
4. Update documentation

**Validation:**
- [ ] Clean compile
- [ ] No platform-specific storage code
- [ ] Identical behavior Desktop/WebGL

---

## Summary: Code Reduction

| Category | Before | After | Removed |
|----------|--------|-------|---------|
| Provider files | 4 | 1 | `LocalStoryProvider`, `UserStoryProvider`, `JsonStoryProvider` |
| Extra files | 1 | 0 | `StoryImporter` |
| RM_SaveLoad.cs | ~920 lines | ~400 lines | ~520 lines |
| PathManager.cs | ~250 lines | ~80 lines | ~170 lines |
| Platform checks | Many | None | All storage-related |
| **Total reduction** | | | **~1,500 lines** |

---

## User Experience

### Playing Official Stories
1. Launch game
2. Select story from list
3. Play the story
4. Exit → nothing to save (read-only)

### Editing Official Stories
1. Select official story → click "Edit"
2. Story forked to memory (becomes editable copy)
3. Edit in RodyMaker
4. Click "Save" → first time shows "Save As" dialog
5. Pick location → saved as `.rody.json`
6. Subsequent saves → quick save to same location
7. ⚠️ Exit without saving → changes lost (warning shown)

### Importing & Editing User Stories
1. Click "Import" in menu
2. Select `.rody.json` file
3. Story loaded into memory, `LastSavePath` remembered
4. Edit in RodyMaker
5. Click "Save" → overwrites original file (quick save)
6. Or "Save As" to save to new location

### Creating New Stories
1. Click "New Story" → enter title
2. Blank story created in memory (`LastSavePath = null`)
3. Edit in RodyMaker
4. Click "Save" → shows "Save As" dialog (first time)
5. Pick location → saved, path remembered
6. ⚠️ Exit without saving → story lost (warning shown)

### Sharing Stories
- Just share the `.rody.json` file
- Works on any platform
- Recipient imports it to play/edit

---

## Testing Checklist

- [ ] Official stories load from Resources (Desktop)
- [ ] Official stories load from Resources (WebGL)
- [ ] Import `.rody.json` works (Desktop)
- [ ] Import `.rody.json` works (WebGL)
- [ ] Edit story in memory
- [ ] Export `.rody.json` works (Desktop)
- [ ] Export/Download `.rody.json` works (WebGL)
- [ ] New story creation works
- [ ] No files in persistent storage
- [ ] No platform-specific code for storage

---

## Rollback Plan

Git commits per phase. Keep external backup of StreamingAssets folders until fully validated.
