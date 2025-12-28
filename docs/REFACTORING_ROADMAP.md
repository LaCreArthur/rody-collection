# Refactoring Roadmap

> Active tracking for improvements and technical debt.
> **Updated: 2025-12-28**

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
- Multiple provider implementations (to be unified - see Next Up)

---

## Next Up: JSON-Only Migration ⚡ PRIORITY

**Goal:** Unify all storage to `.rody.json` with single code path for all platforms.

**Current mess:**
- 4 providers: `LocalStoryProvider`, `UserStoryProvider`, `JsonStoryProvider`, `ResourcesStoryProvider`
- Platform-specific code (`#if UNITY_WEBGL`)
- Two formats: folders (`levels.rody` + `Sprites/`) and JSON

**Target:**
```
ALL PLATFORMS:
    Official Stories → Resources/Stories/*.rody.json (embedded, read-only)
    User Stories     → Import/Export .rody.json files (no persistent storage)
```

**Files to delete:**
- `LocalStoryProvider.cs` (296 lines)
- `UserStoryProvider.cs` (421 lines)
- `StoryImporter.cs` (~200 lines)
- `StreamingAssets/` story folders

**Code reduction:** ~1,500 lines removed

**See:** `JSON_MIGRATION_PLAN.md` for detailed phases

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
