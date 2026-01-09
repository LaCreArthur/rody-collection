# Project Learnings

> Meta-knowledge extracted from past work. Useful for this and other Unity projects.

---

## BetterEvent/SO Variable Migration

### Why Migrate Away

1. **Inspector wiring creates invisible coupling** - Code shows "set this value" but effects (animator changes, object activation) are hidden in Inspector via Odin serialization
2. **Unity migration strips data** - Opening scenes with missing scripts permanently loses serialized field data
3. **Hard to trace and debug** - Logic flow requires checking code AND scene/prefab Inspectors

### Migration Pattern

```csharp
// BEFORE: ScriptableObject Variable pattern
public class PlayerController : MonoBehaviour {
    public BoolVariable isWalking;  // Set in Inspector
    void Update() => isWalking.v = moving;  // Who listens? Unknown from code
}

// AFTER: Static event pattern
public class PlayerController : MonoBehaviour {
    public static event Action<bool> OnWalkingChanged;
    bool _isWalking;
    bool IsWalking {
        get => _isWalking;
        set { if (_isWalking != value) { _isWalking = value; OnWalkingChanged?.Invoke(value); }}
    }
}

// Explicit subscriber - dependencies visible in code
[RequireComponent(typeof(Animator))]
public class WalkAnimatorSync : MonoBehaviour {
    Animator _anim;
    void Awake() => _anim = GetComponent<Animator>();
    void OnEnable() => PlayerController.OnWalkingChanged += v => _anim.SetBool("isWalking", v);
    void OnDisable() => PlayerController.OnWalkingChanged -= v => _anim.SetBool("isWalking", v);
}
```

### Detection Commands

```bash
# Find scripts using SO Variable pattern
grep -rl "Variable\." Assets/Scripts/ --include="*.cs"

# Find VariableListener usages in scenes/prefabs
grep -r "VariableListener" Assets/ --include="*.prefab" --include="*.unity"

# Find BetterEvent usages
grep -r "BetterEvent" Assets/Scripts/ --include="*.cs"
```

---

## Missing Scripts Investigation

### Four Categories

| Category | Cause | Fix |
|----------|-------|-----|
| **Package** | Library corruption | Delete `Library/` folder, reimport |
| **Deleted** | Script removed from project | Restore from git history |
| **Renamed** | Script renamed, new GUID | Update scene YAML or restore old .meta |
| **Unknown** | External package never committed | Remove component from scene |

### Key Insight: Prefab vs Scene Instance

Missing scripts can be on **scene instances** (overrides), not the prefab itself. Always check BOTH prefab and scene file.

### Investigation Commands

```bash
# Extract script GUIDs from scene
grep -o "m_Script: {fileID: 11500000, guid: [a-f0-9]*" "Scene.unity" | \
  sed 's/m_Script: {fileID: 11500000, guid: //' | sort -u

# Check if GUID exists
grep -rl "guid: YOUR_GUID" Assets/ --include="*.meta"

# Find when GUID was deleted
git log --all -p --full-history -S "YOUR_GUID" -- "*.meta"

# View scene data at specific commit
git show COMMIT:"path/Scene.unity" | grep -A50 "guid: SCRIPT_GUID"
```

---

## Unity Migration API Changes

### Unity 2019 → 2022.3 LTS

| Old | New |
|-----|-----|
| `PrefabUtility.GetPrefabType()` | `PrefabUtility.GetPrefabAssetType()` |
| `SystemInfo.supportsImageEffects` | Removed (always true) |
| `ListDrawerSettings(Expanded=)` | `ListDrawerSettings(ShowFoldout=)` |

### Namespace Conflicts Resolution

- Use fully qualified names: `UnityEngine.SceneManagement.Scene`
- Add namespaces to conflicting classes: `DOOM.FPS.AudioManager`

### NiceVibrations API

| Old (MoreMountains) | New (Lofelt) |
|---------------------|--------------|
| `MMVibrationManager.Haptic(HapticTypes.X)` | `HapticPatterns.PlayPreset(PresetType.X)` |

---

## Code Cleanup Patterns

### Dead Code Detection

```bash
# Find unused providers
grep -r "new JsonStoryProvider" Assets/Scripts/

# Find dead imports
grep -r "streamingAssetsPath" Assets/Scripts/

# Verify removal
grep -r "PATTERN" Assets/Scripts/ # Should return nothing
```

### PlayerPrefs Anti-Pattern

```csharp
// BAD: Using PlayerPrefs for runtime state passing
PlayerPrefs.SetInt("scenesCount", WorkingStory.SceneCount);  // Redundant!
int count = PlayerPrefs.GetInt("scenesCount");

// GOOD: Use the source directly
int count = WorkingStory.SceneCount;
```

### Brittle String Matching

```csharp
// BAD: Magic strings for special cases
bool isIbiza = currentGame.Contains("Ibiza");

// GOOD: Add flag to data model
bool isIbiza = story.metadata.isIbiza;
```

---

## Key Principles

1. **Git history is authoritative** - Check git for original data when current scene shows empty fields
2. **Code analysis alone is insufficient** - Scene instances can have additional components not visible in prefab
3. **Static events > Inspector wiring** - Dependencies visible in code, survives migrations
4. **Verify before concluding** - Always grep to confirm patterns are gone before declaring fix complete
