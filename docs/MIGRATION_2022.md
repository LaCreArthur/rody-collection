# Migration Notes: Unity 2019 → 2022.3 LTS

This document tracks all changes made during the December 2024 migration.

## Summary of Changes

### Namespace Updates

| Old Namespace | New Namespace |
|---------------|---------------|
| `UnityReusables.ScriptableDef` | `UnityReusables.ScriptableObjects.Variables` |
| `UnityReusables.Events` | `UnityReusables.ScriptableObjects.Events` |
| `MoreMountains.NiceVibrations` | `Lofelt.NiceVibrations` |

### Type Renames

| Old Type | New Type |
|----------|----------|
| `BetterEvenSO` (typo) | `SimpleEventSO` |
| `HapticTypes` | `HapticPatterns.PresetType` |
| `PrefabType` | `PrefabAssetType` |

### API Changes

| Old API | New API |
|---------|---------|
| `MMVibrationManager.Haptic()` | `HapticPatterns.PlayPreset()` |
| `PrefabUtility.GetPrefabType()` | `PrefabUtility.GetPrefabAssetType()` |
| `SystemInfo.supportsImageEffects` | Removed (always true) |
| `ListDrawerSettings(Expanded=)` | `ListDrawerSettings(ShowFoldout=)` |
| `SirenixEditorGUI.BeginShakeableGroup(key)` | `SirenixEditorGUI.BeginShakeableGroup()` |

### Namespace Conflicts Resolved

**AudioManager Conflict**
- DOOM had a global namespace `AudioManager` class
- Conflicted with `UnityReusables.Managers.Audio_Manager.AudioManager`
- **Fix**: Moved DOOM's AudioManager to `DOOM.FPS` namespace

**ReplaceWithPrefab Conflict**
- Two classes with same name in different folders
- **Fix**: Moved RollToInfinity version to `RollToInfinity` namespace

**Scene Type Conflict**
- `Scene` type was ambiguous
- **Fix**: Used fully qualified `UnityEngine.SceneManagement.Scene`

### Removed Plugin References

| Plugin | Action |
|--------|--------|
| EasyMobile | Removed import, code already guarded with `#if EASY_MOBILE` |

### URP Compatibility

Project uses Built-in Render Pipeline, not URP.
- URP-specific code wrapped in `#if UNITY_PIPELINE_URP`
- Affected file: `QualityManager.cs`

## Files Modified

### PlayerCharacterController.cs
- Updated namespace import

### PlayerWeaponsManager.cs
- Updated namespace import
- Fixed `BetterEvenSO` → `SimpleEventSO`

### ColliderBetterEvents.cs
- Removed `EasyMobile.Internal` import

### KinematicDragTouchController.cs
- Removed direct NiceVibrations import

### CollectibleIMGPoolingController.cs
- Added preprocessor guards for NiceVibrations
- Added support for new Lofelt API

### VibrationManagerSO.cs
- Added support for both old and new NiceVibrations API
- Added fallback `HapticTypes` enum stub

### QualityManager.cs
- Wrapped URP code in preprocessor guards

### AudioManager.cs (DOOM)
- Added `DOOM.FPS` namespace

### AudioUtility.cs
- Added `using DOOM.FPS` import

### SelectGameObjectsWithMissingScripts.cs
- Used fully qualified Scene type

### ReplaceWithPrefab.cs (RollToInfinity)
- Added `RollToInfinity` namespace
- Updated to new PrefabAssetType API

### BoolPEventListenerComponent.cs, TransformPEventListenerComponent.cs, Vector3PEventListenerComponent.cs
- Changed `Expanded` to `ShowFoldout`

### BaseVariableDrawer.cs
- Updated to keyless Odin API

### ImageEffectBase.cs
- Removed obsolete `supportsImageEffects` check

### ReviewManager.cs
- Moved field inside preprocessor block

### ObjectiveToast.cs
- Removed unused field

## Cache Issues

If duplicate class errors occur after migration:
1. Run **Assets > Reimport All**, or
2. Delete the `Library/` folder and reopen project

## Render Pipeline

- **Current**: Built-in Render Pipeline
- **Post Processing**: `com.unity.postprocessing` v3.4.0
- **NOT installed**: Universal Render Pipeline (URP)

## Input System

- **Installed**: New Input System (`com.unity.inputsystem` v1.14.0)
- **Mode**: Both old and new input systems active
