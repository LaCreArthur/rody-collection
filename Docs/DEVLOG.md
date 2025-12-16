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
- Fixed: `RM_SaveLoad.cs`, `Title.cs`, `MenuManager.cs`, `RM_MainLayout.cs`
- Cleaned StreamingAssets files with malformed names

### Refactoring Completed
1. **PathManager** - Centralized path construction (`Assets/Scripts/Utils/`)
2. **SceneData** - Typed model replacing magic indices (`Assets/Scripts/Models/`)
3. **IStoryProvider** - Firebase-ready abstraction (`Assets/Scripts/Providers/`)

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
