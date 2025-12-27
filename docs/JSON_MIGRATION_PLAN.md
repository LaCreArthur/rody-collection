# JSON Migration Plan

> Goal: Unify all story storage to `.rody.json` format, remove legacy folder-based code.

---

## Current State

| Component | Format | Status |
|-----------|--------|--------|
| Official Stories (StreamingAssets) | Folder (`levels.rody` + `Sprites/`) | Legacy |
| User Stories (persistentDataPath) | Folder | Legacy |
| JSON Stories (`.rody.json`) | JSON with base64 sprites | Target |
| Firebase (WebGL) | Firestore + Storage | Separate concern |

---

## Migration Phases

### Phase 1: Convert Official Stories to JSON
**Risk: Low | Effort: Small**

1. Use existing `StoryExportTool` to batch export all StreamingAssets stories
2. Place exported `.rody.json` files in `StreamingAssets/` alongside folders
3. Update `LocalStoryProvider` to prefer `.rody.json` if present
4. Test: Official stories load from JSON

**Files to modify:**
- `LocalStoryProvider.cs` - Add JSON detection/loading

---

### Phase 2: New Stories Create JSON Only
**Risk: Medium | Effort: Medium**

1. Update `RA_NewGame.cs` to create `.rody.json` instead of folder structure
2. Remove `CopyRodyBaseFolder()` - use `JsonStoryProvider.CreateNewScene()` pattern
3. New stories go directly to `UserStories/*.rody.json`

**Files to modify:**
- `RA_NewGame.cs` - JSON creation path
- `PathManager.cs` - Add `CreateNewJsonStory()` helper

---

### Phase 3: Remove Folder Save Paths
**Risk: Medium | Effort: Medium**

1. Remove folder-based save in `RM_SaveLoad.SaveGame()`
2. All saves go through `SaveGameToJson()`
3. Keep folder LOAD for backward compatibility (Phase 4 removes)

**Files to modify:**
- `RM_SaveLoad.cs` - Remove lines 705-780 (folder save)

---

### Phase 4: Migrate Existing User Stories
**Risk: High | Effort: Small**

1. Create migration tool: scan `UserStories/` for folders
2. Convert each folder → `.rody.json` using `StoryExporter`
3. Delete original folders after successful conversion
4. Add migration check on app startup (one-time)

**Files to create:**
- `UserStoryMigrator.cs` - One-time migration utility

---

### Phase 5: Remove Folder Load Paths
**Risk: High | Effort: Large**

1. Remove `UserStoryProvider.cs` entirely
2. Remove folder load path in `RM_SaveLoad.LoadSceneTxt()`
3. Remove folder load path in `RM_MainLayout.LoadSprites()`
4. Remove folder path properties in `PathManager.cs`
5. Remove `StoryImporter.cs` (creates folders from JSON)

**Files to delete:**
- `UserStoryProvider.cs`
- `StoryImporter.cs`

**Files to modify:**
- `RM_SaveLoad.cs` - Remove folder load branches
- `RM_MainLayout.cs` - Remove folder sprite loading
- `PathManager.cs` - Remove `SpritesPath`, `LevelsFile`, `CreditsFile`
- `StoryProviderManager.cs` - Remove UserStoryProvider references

---

### Phase 6: Clean Up StreamingAssets (Optional)
**Risk: Low | Effort: Small**

1. Delete folder-based stories from `StreamingAssets/`
2. Keep only `.rody.json` files
3. Update `LocalStoryProvider` to be JSON-only

---

## Code to Remove (Final State)

### Files to Delete
```
Assets/Scripts/Providers/UserStoryProvider.cs
Assets/Scripts/Providers/StoryImporter.cs
```

### Code Blocks to Remove
| File | Lines | Description |
|------|-------|-------------|
| `RM_SaveLoad.cs` | 705-780 | Folder-based `SaveGame()` |
| `RM_SaveLoad.cs` | 871-930 | Folder-based `LoadSceneTxt()` |
| `RM_MainLayout.cs` | 56-92 | Folder-based `LoadSprites()` |
| `PathManager.cs` | 179-222 | Folder path properties |

### Properties to Remove from PathManager
```csharp
// DELETE these:
public static string SpritesPath => ...
public static string LevelsFile => ...
public static string CreditsFile => ...
public static string GetSpritePath(int scene, int frame) => ...
```

---

## Provider Architecture (Final State)

```
StoryProviderManager
    ├── LocalStoryProvider    (StreamingAssets/*.rody.json)
    ├── JsonStoryProvider     (UserStories/*.rody.json)
    └── FirebaseStoryProvider (WebGL - unchanged)
```

Note: `LocalStoryProvider` could potentially merge into `JsonStoryProvider` with path configuration.

---

## Testing Checklist

### Phase 1
- [ ] Official stories load from JSON
- [ ] Thumbnails display correctly
- [ ] All scenes playable
- [ ] Credits load correctly

### Phase 2
- [ ] New story creates `.rody.json` file
- [ ] New story appears in story list
- [ ] Can edit and save new story

### Phase 3
- [ ] Edits save to JSON (not folder)
- [ ] Scene changes persist
- [ ] Sprite changes persist

### Phase 4
- [ ] Migration converts all user folders
- [ ] Converted stories still work
- [ ] No data loss

### Phase 5
- [ ] App works without folder code
- [ ] No console errors about missing paths
- [ ] Build size reduced

---

## Rollback Plan

Keep git commits atomic per phase. If issues arise:
1. Revert to previous phase commit
2. Fix issues
3. Re-attempt phase

---

## Firebase Consideration

Firebase stays separate for now. Future option:
- Store `.rody.json` files directly in Firebase Storage
- Single API call to fetch entire story
- Simplifies caching and offline support
