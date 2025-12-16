# UnityReusables Library

A comprehensive reusable components library located at `Assets/UnityReusables/`.

## ScriptableObject Variables

**Location**: `Scripts/Scriptable Objects/Variables/`
**Namespace**: `UnityReusables.ScriptableObjects.Variables`

| Type | Class |
|------|-------|
| Bool | `BoolVariable` |
| Int | `IntVariable` |
| Float | `FloatVariable` |
| String | `StringVariable` |
| Vector3 | `Vector3Variable` |
| Transform | `TransformVariable` |
| GameObject | `GameObjectVariable` |

### Features
- Change callbacks: `AddOnChangeCallback()` / `RemoveOnChangeCallback()`
- Persistence: `Save()` / `Load()` using `EncryptedPlayerPrefs`
- Implicit conversion operators

### Usage
```csharp
public BoolVariable isGamePaused;

void Start() {
    isGamePaused.AddOnChangeCallback(OnPauseChanged);
}

void OnPauseChanged() {
    if (isGamePaused.v) { /* paused */ }
}
```

## ScriptableObject Events

**Location**: `Scripts/Scriptable Objects/Events/`
**Namespace**: `UnityReusables.ScriptableObjects.Events`

| Type | Class |
|------|-------|
| Simple (no params) | `SimpleEventSO` |
| Bool | `BoolPEventSO` |
| Int | `IntPEventSO` |
| String | `StringPEventSO` |
| Vector3 | `Vector3PEventSO` |
| Transform | `TransformPEventSO` |
| GameObject | `GameObjectPEventSO` |

### Usage
```csharp
// Raising an event
public SimpleEventSO onPlayerDied;
onPlayerDied.Raise();

// Listening (attach SimpleEventListenerComponent to GameObject)
```

## ScriptableObject Managers

**Location**: `Scripts/Scriptable Objects/Managers/`

- `VibrationManagerSO` - Haptic feedback abstraction
- `SceneManagerSO` - Scene loading utilities
- `TimeManagerSO` - Time-related utilities
- `SocialsManagerSO` - Social sharing features

## Character Controllers

**Location**: `Scripts/Gameplay/CharacterControllers/`

- `BaseCharacterController` - Abstract base class
- `CharacterController2D` - 2D platformer controller
- `CharacterController25D` - 2.5D controller
- `TopDownCharacterController` - Top-down controller
- `GroundCheck`, `GroundCheck2D`, `GroundCheck3D` - Ground detection

## Player Controllers

**Location**: `Scripts/Gameplay/PlayerController/`

- `TouchControl` - Base touch input handling
- `KinematicDragTouchController` - Drag-based object manipulation
- `JoystickController` / `JoystickUI` - Virtual joystick
- `SimpleTouchControl` - Basic touch handling

## Pooling System

**Location**: `Scripts/Pooling/`

### PrefabPoolingSystem
Static pooling manager for performance optimization.

```csharp
// Get pooled object
var obj = PrefabPoolingSystem.Spawn(prefab, position, rotation);

// Return to pool
PrefabPoolingSystem.Despawn(obj);
```

### CollectibleIMGPoolingController
UI collectible animations with pooling support.

## Reusable Components

**Location**: `Scripts/Components/`

| Component | Purpose |
|-----------|---------|
| `BillboardComponent` | Camera-facing sprites |
| `FollowTargetComponent` | Transform following |
| `LookAtComponent` | Look-at behavior |
| `DestructibleComponent` | Destructible objects |
| `CountableComponent` | Counting utilities |

## Better Events

**Location**: `Scripts/Better Events Components/`

### ColliderBetterEvents
Enhanced collision/trigger event system with:
- Tag/Layer filtering
- One-time trigger option
- Auto-destroy capability

## Extensions & Utilities

**Location**: `Scripts/Others/Extensions/`

- `EncryptedPlayerPrefs` - Secure PlayerPrefs wrapper
- `ColorExtensions` - Color manipulation utilities
- `Extensions` - General extension methods
- `BetterButton` - Enhanced UI button
