# DOOM FPS Module

A retro-style FPS minigame located at `Assets/DOOM/`.

## Overview

The DOOM module is a first-person shooter prototype that integrates with the UnityReusables library for events and variables.

## Core Scripts

**Location**: `Assets/DOOM/FPS/Scripts/`

### Player

#### PlayerCharacterController
First-person character movement.
- **Requires**: `CharacterController`, `PlayerInputHandler`, `AudioSource`
- **Features**: Walking, sprinting, crouching, jumping, fall damage
- **Integration**: Uses `BoolVariable` for walking state

#### PlayerWeaponsManager
Weapon inventory and switching.
- Weapon slots system (6 slots)
- Aiming and shooting
- **Integration**: Uses `SimpleEventSO` for fire events

#### PlayerInputHandler
Input abstraction layer for player controls.

### Combat

| Script | Purpose |
|--------|---------|
| `WeaponController` | Individual weapon behavior |
| `ProjectileBase` | Base projectile class |
| `ProjectileStandard` | Standard projectile implementation |
| `DamageArea` | Area-of-effect damage |
| `Damageable` | Damage receiving interface |
| `Health` | Health management with death callbacks |

### Enemies

| Script | Purpose |
|--------|---------|
| `EnemyController` | Base enemy AI |
| `EnemyMobile` | Mobile enemy variant |
| `EnemyTurret` | Stationary turret |
| `DetectionModule` | Player detection logic |
| `NavigationModule` | NavMesh navigation |
| `EnemyManager` | Enemy tracking and management |

### Game Systems

| Script | Purpose |
|--------|---------|
| `GameFlowManager` | Game state management |
| `ObjectiveManager` | Mission objectives |
| `ActorsManager` | Entity management |
| `GameConstants` | Shared constants |

### UI

| Script | Purpose |
|--------|---------|
| `WeaponHUDManager` | Weapon UI display |
| `CrosshairManager` | Dynamic crosshair |
| `PlayerHealthBar` | Health display |
| `AmmoCounter` | Ammunition display |
| `Compass` / `CompassMarker` | Navigation compass |
| `ObjectiveToast` | Objective notifications |

### Audio

#### AudioManager (DOOM.FPS namespace)
Simple audio manager for DOOM module.
- **Note**: Different from `UnityReusables.Managers.Audio_Manager.AudioManager`

#### AudioUtility
Static utility class for spatial audio.
- `CreateSFX()` - Create positioned sound effects
- `GetAudioGroup()` - Get mixer groups
- `SetMasterVolume()` / `GetMasterVolume()`

## Integration with UnityReusables

The DOOM module uses several UnityReusables features:

```csharp
// Variables
using UnityReusables.ScriptableObjects.Variables;
public BoolVariable isWalking;

// Events
using UnityReusables.ScriptableObjects.Events;
public SimpleEventSO fireEvent;
```

## Scenes

The main DOOM scene contains the playable FPS level with all necessary prefabs and UI.
