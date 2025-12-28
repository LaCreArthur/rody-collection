# User Stories Feature

> **Updated:** 2025-12-28 - Simplified to import/export model

## Overview

Users can create, edit, and share their own stories via:
- **Import:** Load `.rody.json` files into memory
- **Export:** Save edited stories as `.rody.json` files
- **Fork-on-edit:** Editing official stories creates an in-memory copy

## Architecture

### Single Runtime Format

All stories (official and user) use the same in-memory format via `WorkingStory`:

```
WorkingStory.Current (ExportedStory)
    ├── story: { id, title, sceneCount }
    ├── credits: string
    ├── scenes: SceneData[]
    └── sprites: { "name.png": "base64..." }
```

### Storage

| Type | Location | Access |
|------|----------|--------|
| Official Stories | `Resources/Stories/*.rody.json` | Read-only, embedded in build |
| User Stories | User's file system | Import/export via file picker |

**No persistent user storage.** Stories exist in memory while the app is running. Users must export to save.

## User Flows

### Playing Official Story

1. Select story from list
2. `WorkingStory.LoadOfficial(storyId)` loads into memory
3. Play through scenes
4. Exit - nothing to save (read-only)

### Editing Official Story (Fork)

1. Select official story, click Edit
2. `WorkingStory.ForkForEditing()` creates deep copy
3. Story marked as non-official, title gets "(copie)" suffix
4. Edit in RodyMaker
5. Click Save → file picker → export as `.rody.json`
6. Exit without saving → changes lost (warning shown)

### Importing User Story

1. Click Import in menu
2. Select `.rody.json` file
3. `WorkingStory.LoadFromJson(json, path)` loads into memory
4. `LastSavePath` remembered for quick-save
5. Play or edit
6. Save → overwrites original location (quick-save)

### Creating New Story

1. Click New Story, enter title
2. `WorkingStory.CreateNew(title)` creates blank story
3. RodyMaker opens
4. Edit scenes
5. First Save → file picker (no `LastSavePath` yet)
6. Subsequent saves → quick-save to same location

### Sharing Stories

Just share the `.rody.json` file. Works on any platform.

## Key Files

| File | Purpose |
|------|---------|
| `WorkingStory.cs` | In-memory story state management |
| `StoryExporter.cs` | `ExportedStory` data model |
| `RA_NewGame.cs` | Import/export UI buttons |
| `RM_SaveLoad.cs` | Editor save/load operations |

## WorkingStory API

```csharp
public static class WorkingStory
{
    // State
    public static ExportedStory Current { get; }
    public static bool IsOfficial { get; }      // Read-only until forked
    public static bool IsDirty { get; }         // Unsaved changes?
    public static string LastSavePath { get; }  // Quick-save location
    public static bool IsLoaded { get; }
    public static string Title { get; }
    public static int SceneCount { get; }

    // Loading
    public static void LoadOfficial(string storyId);
    public static void LoadFromJson(string json, string savePath = null);
    public static void CreateNew(string title);

    // Editing
    public static void ForkForEditing();
    public static void SaveScene(int index, SceneData data);
    public static void SaveSprite(string name, Texture2D texture);
    public static void CreateNewScene(int sceneIndex);

    // Reading
    public static SceneData LoadScene(int sceneIndex);
    public static Sprite LoadSprite(string spriteName);
    public static List<Sprite> LoadSceneSprites(int sceneIndex);
    public static string GetCredits();

    // Export
    public static string ExportToJson();
    public static void MarkSaved(string path);
}
```

## WebGL Considerations

- **Import:** Browser file picker via `StandaloneFileBrowser.jslib`
- **Export:** Browser download trigger via `DownloadFile()` in jslib
- **No persistent storage:** Same as desktop - memory only

## Future: Persistent User Stories

If user feedback requests a "My Stories" list:

- **Desktop:** Auto-save to `persistentDataPath/UserStories/`
- **WebGL:** IndexedDB via jslib wrapper

Currently deferred - import/export is sufficient for MVP.
