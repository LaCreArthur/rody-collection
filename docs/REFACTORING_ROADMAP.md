# Refactoring Roadmap

> Active tracking for improvements and technical debt.
> **Updated: 2025-12-28**

---

## In Progress: JSON-Only Migration 🔄

| Phase | Status | Description |
|-------|--------|-------------|
| Phase 1 | ✅ Done | Unified to ResourcesStoryProvider |
| Phase 1.5 | ✅ Done | Fixed Editor + WebGL target loading |
| Phase 2 | 🔄 Partial | WorkingStory created, UI pending |
| Phase 3 | ⏳ | Simplify JsonStoryProvider |
| Phase 4 | ⏳ | Clean up RM_SaveLoad |
| Phase 5 | ⏳ | Final cleanup (7 files with platform checks) |

**Completed so far:**
- Deleted `LocalStoryProvider.cs` (296 lines)
- Deleted `StreamingAssets/` story folders (backed up to `original-stories/`)
- Created `WorkingStory.cs` - in-memory story state management
- Both desktop/WebGL now use `ResourcesStoryProvider`
- Fixed Editor loading with runtime story type detection
- Unified `RA_ScrollView.cs`, `GameManager.cs`, `Title.cs`

**Key pattern - Runtime story detection:**
```csharp
// Official = just ID, User = has path/prefix
bool isUserStory = gamePath.Contains("/") || gamePath.StartsWith("json:");
```

**Next steps:**
- Integrate `WorkingStory` into `MenuManager.cs` and `RA_NewGame.cs`
- Delete `UserStoryProvider.cs`, `StoryImporter.cs`, `JsonStoryProvider.cs`
- Update `RM_SaveLoad.cs` to use `WorkingStory`
- Remove remaining `#if UNITY_WEBGL` checks (7 files)

**See:** `JSON_MIGRATION_PLAN.md` for detailed phases

---

## Completed

### 2025-12-28: Firebase Removal
- Removed all Firebase dependencies (FirebaseStoryProvider, FirebaseConfig, etc.)
- WebGL now uses `ResourcesStoryProvider` with embedded JSON
- No more HTTP requests, CORS, or async complexity
- See `FIREBASE_REMOVAL_REVIEW.md` for details

### 2024-12-16: Core Refactoring

**PathManager** - `Assets/Scripts/Utils/PathManager.cs`
- Centralized cross-platform path construction

**SceneData Model** - `Assets/Scripts/Models/`
- `SceneData.cs` - PhonemeDialogues, DisplayTexts, MusicSettings, VoiceSettings, ObjectZones
- `SceneDataParser.cs` - Index constants + parsing logic

**IStoryProvider Interface** - `Assets/Scripts/Providers/`
- `IStoryProvider.cs` - Interface + StoryMetadata/StoryData
- Provider implementations (being unified)

---

## Future

### Phoneme Dictionary
**Goal:** Natural French text → phoneme conversion

**Approach:**
1. Dictionary of ~500 common French words
2. "Learn" feature for unknown words
3. Local JSON file storage
4. Future: ML training on existing corpus

**Tasks:**
- [ ] Create base dictionary
- [ ] Build editor UI with learning feature
- [ ] Dictionary persistence

### ObjectZone Format Modernization
**Goal:** Replace string parsing with typed JSON structure

**Current (fragile):**
```csharp
positionRaw = "(0,0);(50,20);"  // String parsing in ReadObjects()
```

**Target:**
```csharp
public List<Vector2Int> positions;  // Proper typed field
```

**Tasks:**
- [ ] Update `ObjectZone` class with typed fields
- [ ] Add JSON serialization for Vector2Int
- [ ] Migrate existing `.rody.json` files

### Other
- **Observable Pattern** - Replace PlayerPrefs state
- **SoundManager Refactor** - Break up 360-line monolith

---

## Code Preferences

- **Explicit code** over Inspector wiring
- **Observable pattern** for state
- **Path.Combine()** always for paths
- **No null checks** on serialized fields
