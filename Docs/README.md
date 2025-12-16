# RodyMaker Documentation

> **Project History**: Originally created in Unity 2019, resurrected and upgraded to Unity 2022.3 LTS in December 2024.

## Quick Start

This project contains multiple game prototypes built on a shared utility library called **UnityReusables**.

### Project Structure

```
Assets/
├── DOOM/                 # FPS minigame (playable)
├── RollToInfinity/       # Another game prototype
├── Pixelation/           # Visual effect module
├── Feel/NiceVibrations/  # Haptic feedback plugin
├── UnityReusables/       # Shared utility library
└── Plugins/Sirenix/      # Odin Inspector
```

## Documentation Index

| Document | Description |
|----------|-------------|
| [Architecture](ARCHITECTURE.md) | Patterns, namespaces, and design philosophy |
| [UnityReusables Library](UNITY_REUSABLES.md) | Reusable components and systems |
| [DOOM FPS Module](DOOM_FPS.md) | FPS minigame documentation |
| [Inspector Wiring](INSPECTOR_WIRING.md) | **Important!** Understanding serialized logic |
| [Third-Party Plugins](THIRD_PARTY.md) | External dependencies |
| [Migration Notes](MIGRATION_2022.md) | Unity 2019 → 2022.3 changes |
| [Development Log](DEVLOG.md) | Session history and progress tracking |

## Key Concepts

### ScriptableObject Architecture
This project uses ScriptableObjects extensively for:
- **Variables** - Shared state (e.g., `BoolVariable`, `IntVariable`)
- **Events** - Decoupled communication (e.g., `SimpleEventSO`)
- **Managers** - Configuration and services

### Inspector-Based Wiring
**Important**: Much of the game logic is configured in the Unity Inspector, not in code. See [Inspector Wiring](INSPECTOR_WIRING.md) for details.

## Unity Version

- **Current**: Unity 2022.3 LTS
- **Render Pipeline**: Built-in (not URP)
- **Input System**: Both legacy and new input system

---

*Last updated: December 2024*
