# Firebase Removal & WebGL JSON Review

> Review of the Firebase removal (2025-12-28) and assessment of full JSON-only migration.

---

## Firebase Removal Status: ✅ COMPLETE

### Files Successfully Removed
| File | Status |
|------|--------|
| `FirebaseStoryProvider.cs` | ✅ Deleted |
| `FirebaseConfig.cs` | ✅ Deleted |
| `FirebaseMigrationTool.cs` | ✅ Deleted |
| `WebJsonStoryProvider.cs` | ✅ Deleted |
| `firebase.json` | ✅ Deleted |
| `.firebaserc` | ✅ Deleted |

### Code Cleanup Status
| Check | Status | Notes |
|-------|--------|-------|
| Firebase string references | ✅ None in code | Only in docs (historical) |
| `UnityWebRequest` usage | ✅ None | No HTTP requests |
| `async` methods for storage | ✅ None | All synchronous now |
| `LoadCoverAsync` callers | ✅ Removed | All use `LoadCover` |

### Documentation Leftovers (OK to keep for history)
- `FIREBASE_WEBGL_RESEARCH.md` - Historical research doc
- `docs/DEVLOG.md` - Contains Firebase removal log entries
- `docs/JSON_MIGRATION_PLAN.md` - References Firebase (update recommended)
- `docs/REFACTORING_ROADMAP.md` - Future Firebase mentions (update recommended)

---

## WebGL JSON Implementation Status: ✅ WORKING

### Current Architecture (Before Migration)
```
WebGL:
    StoryProviderManager → ResourcesStoryProvider
        ↓
    Resources/Stories/*.rody.json (embedded in build)
    
Desktop:
    StoryProviderManager → LocalStoryProvider
        ↓
    StreamingAssets/ (folder structure: levels.rody + Sprites/)
```

### Target Architecture (After Migration)
```
ALL PLATFORMS:
    StoryProviderManager → ResourcesStoryProvider
        ↓
    Resources/Stories/*.rody.json (embedded, read-only)
    
User Stories:
    Import/Export .rody.json files (no persistent storage)
```

See `JSON_MIGRATION_PLAN.md` for full migration details.

### Files Involved
| File | Purpose | Status |
|------|---------|--------|
| `ResourcesStoryProvider.cs` | Loads from `Resources/Stories/` | ✅ Working |
| `StoryProviderManager.cs` | Platform switching | ✅ Working (simplify in migration) |
| `RA_ScrollView.cs` | WebGL init path | ✅ Working |
| `Bootstrap.cs` | Provider initialization | ✅ Working |

---

## Dead Code Identified (Remove in JSON Migration)

### 1. LocalStoryProvider.cs - DELETE ENTIRELY
- 296 lines of folder-based loading code
- Desktop will use `ResourcesStoryProvider` like WebGL

### 2. UserStoryProvider.cs - DELETE ENTIRELY
- 421 lines of folder-based user story code
- User stories become import/export only (no persistent storage)

### 3. StoryImporter.cs - DELETE ENTIRELY
- ~200 lines converting `.rody.json` → folder structure
- Not needed - we want to stay in JSON format

### 4. JsonStoryProvider.cs - SIMPLIFY
- Keep in-memory editing capabilities
- Remove file path dependencies
- Rename to `EditableStoryProvider`

### 4. RM_SaveLoad.cs - Legacy Folder Save (Lines 365-430)
```csharp
// Legacy folder-based save - can be removed after Phase 3
newPath = path;
if (scene == 0) {
    SaveSprite(gm.scenePanel.GetComponent<SpriteRenderer>().sprite.texture, newPath, "0", 320, 240);
    return;
}
for (int i = 1; i<5; i++){
    SaveSprite(gm.scenePanel.GetComponent<SpriteRenderer>().sprite.texture, newPath, scene+"."+i);
}
// ... etc
```

### 5. RM_SaveLoad.cs - Legacy Folder Load (Lines 570-620)
```csharp
// Legacy folder-based loading - can be removed after Phase 5
using (StreamReader sr = new StreamReader(PathManager.LevelsFile))
{
    // ... reading from levels.rody
}
```

### 6. PathManager.cs - Folder Path Properties
```csharp
// Remove after JSON migration:
public static string SpritesPath => ...
public static string LevelsFile => ...
public static string CreditsFile => ...
```

### 7. RA_NewGame.cs - Folder Creation Code (Lines 36-90)
```csharp
// CopyGameFolder(), CopyRodyBaseFolder() - not needed for JSON-only
```

### 8. isGameFolder() in RA_ScrollView.cs (Line 637)
- Checks for `levels.rody` + `Sprites/` folder structure
- Not needed after JSON migration

---

## Remaining TODOs from DEVLOG

From 2025-12-28 Firebase Removal:
- [ ] **Test WebGL build with embedded stories** - Critical validation

---

## Recommendations

### Immediate (Before JSON Migration)
1. Test WebGL build to confirm Firebase removal is complete
2. Update `docs/REFACTORING_ROADMAP.md` to remove Firebase references
3. Update `docs/JSON_MIGRATION_PLAN.md` to reflect current state

### For JSON Migration
1. Follow the phased approach in `JSON_MIGRATION_PLAN.md`
2. Keep folder-based code as fallback until Phase 4 migration completes
3. Delete `FIREBASE_WEBGL_RESEARCH.md` after confirming WebGL works
