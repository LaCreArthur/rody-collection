# Refactoring Roadmap

> Active tracking for improvements and technical debt.

## Completed (2024-12-16)

### PathManager
`Assets/Scripts/Utils/PathManager.cs` - Centralized cross-platform path construction.

### SceneData Model
`Assets/Scripts/Models/` - Typed data model replacing magic string array indices.
- `SceneData.cs` - PhonemeDialogues, DisplayTexts, MusicSettings, VoiceSettings, ObjectZones
- `SceneDataParser.cs` - Index constants + parsing logic

### IStoryProvider Interface
`Assets/Scripts/Providers/` - Abstraction layer for local/remote storage.
- `IStoryProvider.cs` - Interface + StoryMetadata/StoryData
- `LocalStoryProvider.cs` - StreamingAssets implementation
- `StoryProviderManager.cs` - Provider accessor

```csharp
var scene = StoryProviderManager.Provider.LoadScene("Rody Et Mastico", 1);
```

---

## Next Up: Phoneme Dictionary

**Goal:** Natural French text → phoneme conversion

**Approach:**
1. Dictionary of ~500 common French words
2. "Learn" feature for unknown words
3. Firebase sync for shared dictionary
4. Future: ML training on existing corpus

**Tasks:**
- [ ] Create base dictionary
- [ ] Build editor UI with learning feature
- [ ] Firebase sync (after IStoryProvider migration)

---

## Future

- **Firebase Storage** - Online story sharing (uses IStoryProvider)
- **Observable Pattern** - Replace PlayerPrefs state
- **SoundManager Refactor** - Break up 360-line monolith

---

## Code Preferences

- **Explicit code** over Inspector wiring
- **Observable pattern** for state
- **Path.Combine()** always for paths
