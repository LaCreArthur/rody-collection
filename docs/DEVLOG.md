# Development Log

> Condensed session log for quick project context. See [REFACTORING_ROADMAP.md](REFACTORING_ROADMAP.md) for ongoing work.

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
| Unity Version | 2022.3 LTS |
| Render Pipeline | Built-in (not URP) |
| Input System | Both legacy and new |
| Key Plugins | Odin Inspector, DOTween, NiceVibrations |
