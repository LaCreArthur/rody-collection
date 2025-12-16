# RodyMaker Refactoring Roadmap

> Tracking document for ongoing refactoring and improvements.

## Completed

### PathManager (2024-12-16)
Centralized all path construction in `Assets/Scripts/Utils/PathManager.cs`:
- `GamePath` - Root story folder
- `SpritesPath` - Sprites subfolder
- `LevelsFile` - levels.rody path
- `CreditsFile` - credits.txt path
- `GetSpritePath()` - Individual sprite paths

### SceneData Model (2024-12-16)
Created data model layer in `Assets/Scripts/Models/`:
- `SceneData.cs` - Structured scene data (replaces magic string arrays)
- `SceneDataParser.cs` - Centralizes index constants and parsing
- Added `RM_SaveLoad.LoadSceneData()` for structured loading

---

## Completed

### PathManager (2024-12-16)
Centralized all path construction in `Assets/Scripts/Utils/PathManager.cs`:
- `GamePath` - Root story folder
- `SpritesPath` - Sprites subfolder
- `LevelsFile` - levels.rody path
- `CreditsFile` - credits.txt path
- `GetSpritePath()` - Individual sprite paths

### SceneData Model (2024-12-16)
Created data model layer in `Assets/Scripts/Models/`:
- `SceneData.cs` - Structured scene data (replaces magic string arrays)
- `SceneDataParser.cs` - Centralizes index constants and parsing
- Added `RM_SaveLoad.LoadSceneData()` for structured loading

### IStoryProvider Interface (2024-12-16)
Created abstraction layer in `Assets/Scripts/Providers/`:
- `IStoryProvider.cs` - Interface + StoryMetadata/StoryData classes
- `LocalStoryProvider.cs` - Full StreamingAssets implementation
- `StoryProviderManager.cs` - Singleton accessor with provider switching

**Usage:**
```csharp
// Access via singleton
var stories = StoryProviderManager.Provider.GetStories();
var scene = StoryProviderManager.Provider.LoadScene("Rody Et Mastico", 1);

// Future: Switch to Firebase
StoryProviderManager.SetProvider(new FirebaseStoryProvider());
```

---

## Priority 3: Phoneme Dictionary System

### Goals
- Allow natural French text input
- Auto-convert to phonemes
- Learn unknown words from user

### Approach (Hybrid)
1. **Dictionary of common words** (~500 most used French words)
2. **User learning feature** - Add new words when unknown
3. **Firebase sync** - Share dictionary across users
4. **Future: ML exploration** - Train on existing phoneme corpus

### UI Concept
```
[Natural Text]: Bonjour, c'est moi!
[Phonemes]:     b_on_j_ou_r s_et [moi?]
                              ↑ Click to teach
```

### Tasks
- [ ] Create French word → phoneme dictionary (start with 500 words)
- [ ] Build dictionary UI with "Learn" feature
- [ ] (Future) Firebase sync for shared dictionary
- [ ] (Future) Explore ML training on existing phoneme data

---

## Future Ideas

### Firebase Storage Integration
- Replace StreamingAssets with Firebase Storage for online sharing
- Users can upload/download custom stories
- Requires `IStoryProvider` abstraction first

### Observable Pattern Migration
- Replace `PlayerPrefs` state with observable values
- Prefer explicit code over Inspector wiring
- Use events for state changes

---

## Notes

### Code Patterns Preference
- **Explicit code over Inspector wiring** - Keep logic in code, not serialized
- **Observable pattern** - For state management
- **Path.Combine()** - Always use for cross-platform paths

### Technical Debt Identified
- `PlayerPrefs` used for game state (should be explicit)
- Object zone parsing still uses raw strings in SceneData
- Some classes are large monoliths (e.g., SoundManager)
