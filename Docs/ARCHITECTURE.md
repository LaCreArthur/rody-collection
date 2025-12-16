# Architecture

## ScriptableObject Architecture

The project uses a ScriptableObject-based architecture pattern (similar to Ryan Hipple's "Game Architecture with ScriptableObjects" GDC talk):

1. **Variables as ScriptableObjects**: Shared data without singleton coupling
2. **Events as ScriptableObjects**: Decoupled event system
3. **Managers as ScriptableObjects**: Runtime-independent configuration

### Benefits
- Decoupled components
- Easy testing and debugging
- Inspector-visible state
- No singleton dependencies

## Namespace Conventions

```
UnityReusables.ScriptableObjects.Variables  - SO Variables
UnityReusables.ScriptableObjects.Events     - SO Events
UnityReusables.Managers.Audio_Manager       - Audio management (singleton)
UnityReusables.PlayerController             - Player controllers
UnityReusables.CharacterControllers         - Character controllers
UnityReusables.Progression                  - Progress tracking
UnityReusables.Utils                        - General utilities
UnityReusables.Utils.Components             - Utility components
UnityReusables.Utils.Extensions             - Extension methods
UnityReusables.Utils.BetterColliderTrigger  - Collision events
UnityReusables.Utils.LayerTagDropdown       - Editor utilities
DOOM.FPS                                    - DOOM FPS module classes
RollToInfinity                              - RollToInfinity module classes
```

## Preprocessor Defines

| Define | Purpose |
|--------|---------|
| `MOREMOUNTAINS_NICEVIBRATIONS` | Old NiceVibrations plugin |
| `NICEVIBRATIONS_INSTALLED` | New Lofelt NiceVibrations |
| `UNITY_PIPELINE_URP` | Universal Render Pipeline |
| `EASY_MOBILE` | EasyMobile plugin (not installed) |

## Singleton Pattern

The project uses `SingletonMB<T>` for MonoBehaviour singletons:

```csharp
public class AudioManager : SingletonMB<AudioManager>
{
    protected override void OnAwake() { /* initialization */ }
}

// Usage
AudioManager.instance.Play("SoundName");
```

**Features**:
- Lazy initialization
- DontDestroyOnLoad
- Duplicate prevention

## File Organization

### Editor Scripts
Editor-only scripts are placed in `Editor/` folders to prevent inclusion in builds.

### Assembly Definitions
The project does not use Assembly Definition files - all scripts compile into `Assembly-CSharp`.
