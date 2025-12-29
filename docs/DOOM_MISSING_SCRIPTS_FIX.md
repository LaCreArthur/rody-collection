# DOOM Missing Scripts Fix

**Scene**: `Assets/DOOM/FPS/Scenes/MainScene.unity`
**Status**: ✅ WeaponFireAnimator working, remaining items in progress

## Fix Summary

| Category | Count | Fix | Status |
|----------|-------|-----|--------|
| Package scripts (ugui, postprocessing, etc.) | 14 | Delete `Library/` folder | ✅ Done |
| BillboardComponent | 1 | Restored from git | ✅ Done |
| BetterEventSOListener (weapon fire) | 2 | `WeaponFireAnimator.cs` + C# event | ✅ Done |
| ZamblaAllongé trigger | 1 | `SimpleTriggerAction.cs` (PlayAudio) | Pending |
| Pickup_Shotgun (scene instance) | 1 | `SimpleTriggerAction.cs` (EnableTarget) | Pending |
| ObjectiveReachPoint (1) | 1 | `SimpleTriggerAction.cs` (EnableTarget) | Pending |
| AI Navigation deprecated script (Player + NavMeshSurface) | 2 | Remove in Inspector | Pending |

## Wiring Instructions

### Step 1: Delete Library (fixes 14 package scripts)
```bash
# Close Unity first!
rm -rf Library/
# Reopen Unity
```

### Step 2: Wire WeaponFireAnimator ✅
**On UI element with weapon animator** (Shotgun/Pistol UI):
1. Find the UI GameObject that has the weapon Animator
2. Remove yellow "Missing Script" component (old BetterEventSOListener)
3. Add Component → `WeaponFireAnimator` (auto-wires Animator via RequireComponent)

### Step 3: Wire SimpleTriggerAction

| GameObject | Action | Target | Trigger Once | Notes |
|------------|--------|--------|--------------|-------|
| ZamblaAllongé | PlayAudio | - | false | Repeating audio trigger |
| Pickup_Shotgun (scene instance) | EnableTarget | AfterShotgunPickedUp | true | Activates object when picked up |
| ObjectiveReachPoint (1) | EnableTarget | Coq (1) | true | Shows rooster when objective reached |

For ZamblaAllongé:
1. Find GameObject in Hierarchy
2. Remove yellow "Missing Script" component
3. Add Component → `SimpleTriggerAction`
4. Set Action = PlayAudio, Target Tag = "Player", Trigger Once = false

For Pickup_Shotgun:
1. Find `Pickup_Shotgun` in Hierarchy (it's a scene instance, not just a prefab)
2. Remove yellow "Missing Script" component
3. Add Component → `SimpleTriggerAction`
4. Set Action = EnableTarget, Target Tag = "Player", Trigger Once = true
5. Drag `AfterShotgunPickedUp` GameObject to the Target field

For ObjectiveReachPoint (1):
1. Find `ObjectiveReachPoint (1)` in Hierarchy (not the first one!)
2. Remove yellow "Missing Script" component
3. Add Component → `SimpleTriggerAction`
4. Set Action = EnableTarget, Target Tag = "Player", Trigger Once = true
5. Drag `Coq (1)` GameObject to the Target field

### Step 4: Remove deprecated AI Navigation script
This script (GUID: `c8534e17a0f90604c9afd4f5c73d829f`) was removed in a Unity AI Navigation package update. It only stored an ID for scene linking and has no functional purpose.

**On NavMeshSurface:**
1. Find `NavMeshSurface` GameObject
2. Remove the yellow "Missing Script" component

**On Player (prefab instance):**
1. Find `Player` in Hierarchy (it's a prefab instance)
2. Remove the yellow "Missing Script" component

## Learnings

1. **Code analysis alone is insufficient** - `ObjectiveReachPoint.cs` has `OnTriggerEnter`, but ObjectiveReachPoint (1) scene instance had ADDITIONAL behavior via BetterTriggerEnter component (EnableTarget → Coq). Always check scene history.
2. **Prefab vs scene instance** - Missing scripts can be on scene instances (overrides) not the prefab itself. Check BOTH prefab and scene file.
3. **Static events** - `public static event Action` eliminates Inspector wiring for subscribers
4. **RequireComponent + GetComponent** - Auto-wires same-object dependencies without serialization
5. **Use automation for investigations** - Created `tools/find-missing-scripts.sh` after repeated manual investigation mistakes

## Files Changed

| File | Change | Purpose |
|------|--------|---------|
| `Assets/DOOM/FPS/Scripts/PlayerWeaponsManager.cs` | Added `static event Action OnFired` | Fire event (static = no wiring needed) |
| `Assets/DOOM/Scripts/WeaponFireAnimator.cs` | Created | Subscribes to fire event, triggers animator |
| `Assets/DOOM/Scripts/SimpleTriggerAction.cs` | Created | Trigger zone actions (replaces BetterTriggerEnter) |
| `Assets/DOOM/Scripts/BillboardComponent.cs` | Restored from git | Sprite billboarding |

## Technical Reference

### Reusable Script
Use `tools/find-missing-scripts.sh` to scan for missing scripts:
```bash
# Current scene
./tools/find-missing-scripts.sh Assets/DOOM/FPS/Scenes/MainScene.unity

# Git history
./tools/find-missing-scripts.sh 2b5924a:Assets/DOOM/FPS/Scenes/MainScene.unity
```

See `docs/UNITY_MISSING_SCRIPTS_RECOVERY.md` for investigation techniques.

**Original data source**: Git commit `665f705` (pre-migration state with intact serialization)
