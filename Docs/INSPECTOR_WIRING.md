# Inspector-Based Wiring

> **Important**: This document describes a critical architectural consideration that affects how you work with this codebase.

## Overview

This project makes **extensive use of inspector-based wiring** through the Better Events system and Unity's serialization. A significant portion of the game logic is **not visible in the code**.

## What This Means

Game logic is configured through:

1. **BetterEvent fields** - Serialized in the Unity Inspector
2. **ScriptableObject Events** (`SimpleEventSO`, etc.) - Wired via Inspector references
3. **UnityEvent callbacks** - Standard Unity serialized events
4. **Component references** - Cross-object dependencies set in Inspector

## Implications

| Aspect | Impact |
|--------|--------|
| **Code Reading** | Cannot fully understand behavior by reading code alone |
| **Refactoring** | Renaming fields/classes may break Inspector references silently |
| **Debugging** | Logic flow requires checking both code AND scene/prefab Inspectors |
| **Version Control** | Scene/prefab changes contain logic changes (harder to review) |
| **Onboarding** | Need to explore scenes, not just codebase |

## How to Navigate

1. **Check Prefabs**: Many components have pre-wired events in prefabs
2. **Check Scenes**: Scene-specific wiring overrides prefab defaults
3. **Search for ScriptableObject assets**: Events and Variables are assets in Project
4. **Use Find References**: Unity's "Find References In Scene" helps trace connections

## Event Flow Pattern

```
┌─────────────────┐     Inspector Reference     ┌─────────────────┐
│  Component A    │ ─────────────────────────▶  │  SimpleEventSO  │
│  (raises event) │                             │  (asset file)   │
└─────────────────┘                             └────────┬────────┘
                                                         │
                    ┌────────────────────────────────────┘
                    │ Listener Component Reference
                    ▼
┌─────────────────────────────────────┐
│  SimpleEventListenerComponent       │
│  (attached to receiving GameObject) │
│  ┌─────────────────────────────┐   │
│  │ UnityEvent onEventRaised    │   │ ◀── Also wired in Inspector
│  │  - Call MethodX on ObjectY  │   │
│  └─────────────────────────────┘   │
└─────────────────────────────────────┘
```

## Files Using Heavy Inspector Wiring

| File | Inspector-Configured Elements |
|------|------------------------------|
| `ColliderBetterEvents.cs` | Event callbacks on collision/trigger |
| `SimpleEventListenerComponent.cs` | Response actions to SO events |
| `BoolVariableListener.cs` | Reactions to variable changes |
| `TMPCountdown.cs` | `onFinished` event wiring |
| `DestructibleComponent.cs` | Destruction effects and sounds |
| `KinematicDragTouchController.cs` | Game events (`levelCompleted`, `gameOver`) |

## Best Practices

### When Modifying Code
1. Search for usages in scenes/prefabs before renaming
2. Use `[FormerlySerializedAs]` attribute when renaming serialized fields
3. Test in editor after changes to catch broken references

### When Debugging
1. Check the Inspector for event subscribers
2. Use Debug.Log in event handlers to trace flow
3. Enable Odin Inspector's debugging features

### For Documentation
Consider documenting critical event flows:
- Create diagrams showing event connections
- Add comments in prefabs/scenes
- Document expected Inspector configuration in code comments
