# User Stories Feature

> Implemented: 2024-12-26

## Overview

This feature enables players to create, edit, save, and share their own stories with:
- Local storage in `Application.persistentDataPath/UserStories/`
- Fork-on-edit protection for official stories
- Export/Import via portable `.rody.json` files
- Desktop-first implementation (WebGL support planned for future)

## Architecture

### Storage Locations

| Type | Location | Provider |
|------|----------|----------|
| Official Stories (Desktop) | `StreamingAssets/` | `LocalStoryProvider` |
| Official Stories (WebGL) | `Resources/Stories/` | `ResourcesStoryProvider` |
| User Stories (Desktop) | `persistentDataPath/UserStories/` | `UserStoryProvider` |

### Key Design Decisions

1. **Fork-on-Edit**: When editing an official story, a copy is automatically created in user space with `_edit` suffix (e.g., "Rody Et Mastico_edit"). This protects original stories from modification.

2. **Same File Format**: User stories use the same format as official stories (levels.rody, Sprites/, credits.txt) for compatibility.

3. **Portable Export**: The `.rody.json` format bundles all story data including sprites as base64, making files ~1-2MB and easily shareable.

4. **UI Separation**: Story selection shows official stories first, then a separator, then user stories section.

## Files Created

### `Assets/Scripts/Providers/UserStoryProvider.cs`
Full `IStoryProvider` implementation for user stories with:
- `ForkStory(sourcePath)` - Copies official story to user space
- `DeleteStory(storyId)` - Removes user story
- Standard provider methods (GetStories, LoadScene, SaveScene, etc.)

### `Assets/Scripts/Providers/StoryExporter.cs`
Exports stories to portable JSON format:
- `ExportToJson(storyPath)` - Returns JSON string
- `ExportToFile(storyPath, outputPath)` - Writes .rody.json file
- `GetExportFileName(storyPath)` - Suggests filename

Export format:
```json
{
  "formatVersion": 1,
  "exportedAt": "2024-12-26T12:00:00Z",
  "story": { "id": "...", "title": "...", "sceneCount": 5 },
  "credits": "...",
  "scenes": [ { "index": 1, "data": {...} } ],
  "sprites": { "cover.png": "base64...", "1.1.png": "base64..." }
}
```

### `Assets/Scripts/Providers/StoryImporter.cs`
Imports stories from .rody.json files:
- `ImportFromJson(json)` - Creates story folder, returns path
- `ImportFromFile(filePath)` - Reads file and imports
- `ValidateJson(json)` - Validates without importing

## Files Modified

### `Assets/Scripts/Utils/PathManager.cs`
Added:
- `UserStoriesPath` - Path to user stories folder
- `IsUserStory` - Checks if current game is a user story
- `GetUserStoryPath(storyId)` - Gets path for specific user story
- `EnsureUserStoriesDirectory()` - Creates folder if needed
- `GetUniqueForkName(originalName)` - Generates _edit, _edit2, etc.

### `Assets/Scripts/MenuManager.cs`
Added `ForkAndEdit()` method:
- Called when Edit button is pressed
- Checks if story is user story (edit in place) or official (fork first)
- Updates gamePath to forked location before loading editor

### `Assets/Scripts/RodyAnthology/RA_ScrollView.cs`
Added:
- `slotSeparatorPrefab` - Visual separator between sections
- `slotImportPrefab` - Import button slot
- `userStorySlotIndices` - Tracks user story slots
- `LoadUserStories()` - Loads user stories from persistentDataPath
- `GetSlotPath(index)` - Handles both official and user paths
- `GetSelectedUserStoryPath()` - For export functionality
- Updated `OnSuppr()` to handle user story deletion

### `Assets/Scripts/RodyAnthology/RA_NewGame.cs`
Added:
- `OnImportClick()` - Opens file dialog, imports .rody.json
- `OnExportClick()` - Exports selected user story
- Updated delete logic to handle absolute paths (user stories)

## Inspector Wiring Required

On `RA_ScrollView` component in the scene, assign:
- `slotSeparatorPrefab` - Prefab for visual separator
- `slotImportPrefab` - Prefab for import button

## User Flow

### Creating a New Story
1. Click "New Game" in story selection
2. Story is created in StreamingAssets (existing behavior)
3. First edit will fork to UserStories

### Editing Official Story
1. Select official story, click Edit
2. System automatically forks to `UserStories/{name}_edit`
3. All changes saved to forked copy
4. Original remains unchanged

### Editing User Story
1. Select user story, click Edit
2. Changes saved directly (no fork needed)

### Exporting Story
1. Select a user story
2. Click Export button (or use menu)
3. Choose save location
4. .rody.json file created with all data

### Importing Story
1. Click Import button
2. Select .rody.json file
3. Story imported to UserStories folder
4. Appears in "My Stories" section

## Future Enhancements

- [ ] WebGL support using IndexedDB for user stories
- [ ] Cloud sync for user stories (Firebase)
- [ ] Story thumbnail generation on export
- [ ] Batch import/export
- [ ] Story versioning

## Discussion Context

This feature was designed during a brainstorming session about UGC (user-generated content). Key constraints discussed:
- Storage costs for cloud hosting
- Moderation concerns
- Authentication complexity

The "local-first with export/import" approach was chosen as it:
- Avoids cloud storage costs
- Eliminates moderation needs
- Enables sharing via files
- Works offline
- Gives users control of their data

The implementation prioritizes desktop for easier debugging, with WebGL (IndexedDB) planned as a future iteration.
