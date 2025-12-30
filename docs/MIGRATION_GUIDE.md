# Unity Migration Guide

Comprehensive guide for Unity project migrations, including:
- BetterEvent/Odin serialization discovery and migration
- Missing scripts detection and recovery
- GUID management and investigation

## Toolkit

All tools are consolidated in `tools/unity-migration-toolkit.sh`:

```bash
# Make executable once
chmod +x tools/unity-migration-toolkit.sh

# See all commands
./tools/unity-migration-toolkit.sh help
```

### BetterEvent Commands
```bash
# List all scripts using BetterEvent with GUIDs
./tools/unity-migration-toolkit.sh be-list-scripts

# Full audit of a scene (finds components + decodes serialization)
./tools/unity-migration-toolkit.sh be-audit Assets/Scenes/Main.unity

# Decode Odin hex manually
./tools/unity-migration-toolkit.sh be-decode '53006500740042006f006f006c00'
```

### Missing Scripts Commands
```bash
# Find missing scripts in current scene
./tools/unity-migration-toolkit.sh missing-scripts Assets/Scenes/Main.unity

# Check historical version
./tools/unity-migration-toolkit.sh missing-scripts 665f705:Assets/Scenes/Main.unity

# Look up what a GUID belongs to
./tools/unity-migration-toolkit.sh guid-lookup db2f33d946594fbcbe1cd91669ccfd39
```

### Unused Files Command
```bash
./tools/unity-migration-toolkit.sh unused-files
```

---

## Quick Diagnosis

```bash
# Extract all script GUIDs from a scene
grep -o "m_Script: {fileID: 11500000, guid: [a-f0-9]*" "Scene.unity" | \
  sed 's/m_Script: {fileID: 11500000, guid: //' | sort -u

# Check if GUID exists in project
grep -rl "guid: YOUR_GUID" Assets/ --include="*.meta"

# Check Unity packages
grep -rl "guid: YOUR_GUID" Library/PackageCache/ 2>/dev/null
```

## Four Categories of Missing Scripts

| Category | Cause | Fix |
|----------|-------|-----|
| **Package** | Library corruption | Delete `Library/` folder, reimport |
| **Deleted** | Script removed from project | Restore from git history |
| **Renamed** | Script renamed, new GUID generated | Update scene YAML or restore old .meta |
| **Unknown** | External package never committed | Remove component from scene |

## Git Recovery Commands

```bash
# Find when GUID was deleted
git log --all -p --full-history -S "YOUR_GUID" -- "*.meta"

# Recover deleted file with its GUID
git checkout COMMIT -- path/to/file.cs path/to/file.cs.meta

# View original scene data at specific commit
git show COMMIT:"path/Scene.unity" | grep -A50 "guid: SCRIPT_GUID"
```

## Critical: Check Before Deleting

**ALWAYS** verify no references exist before deleting any script:

```bash
grep -r "YOUR_GUID" Assets/ --include="*.unity" --include="*.prefab" --include="*.asset"
```

## Odin Binary Decoding

BetterEvent serialization uses Odin Inspector's binary format with UTF-16LE strings:

```bash
# Decode Odin binary to readable text
echo "HEX_BYTES" | sed 's/\(..\)00/\1/g' | xxd -r -p
```

**Key fields in decoded output**:
- `declaringType` - Target class (e.g., `UnityEngine.Animator`)
- `methodName` - Method to call (e.g., `SetTrigger`, `Play`, `SetActive`)
- `ParameterValues` - Actual parameter values (e.g., `Fire`, `true`)

**Mapping targets**: `unityReferences: [{fileID: XXXXX}]` → trace fileID to find target GameObject/Component

## Key Learnings

1. **Unity migration strips data**: Opening scenes with missing scripts in newer Unity versions permanently loses serialized field data. The current scene becomes authoritative.

2. **Git history is authoritative**: Always check git history for original serialization data when current scene shows empty fields.

3. **Package GUIDs are stable**: If Unity package scripts (ugui, TMP, etc.) show as missing, it's Library corruption, not the package.

4. **Prefer explicit serialization**: Use serialized fields over `FindObjectOfType` for dependency injection. `GetComponent<T>()` in Awake is acceptable for same-object discovery.

5. **Avoid complex serialization**: When reimplementing lost behaviors, use simple serialized fields and Unity's native serialization instead of Odin BetterEvent to survive future migrations.

6. **ScriptableObject Variable pattern creates invisible wiring**: Patterns like `BoolVariable`, `IntVariable`, `FloatVariable` with corresponding Listeners (`BoolVariableListener`, etc.) create implicit dependencies that are invisible when reading code. The code only shows "set this SO value" but the actual effects (animator changes, object activation) are wired in the Inspector via Odin-serialized BetterEvents.

## ScriptableObject Variable Anti-Pattern

The BaseVariable/VariableListener pattern from UnityReusables creates hard-to-trace dependencies:

```
┌─────────────────────────┐      ┌─────────────────────────┐
│  ScriptA.cs             │      │  SceneB.unity           │
│  sets myBoolVar.v=true  │──────│  BoolVariableListener   │
│  (no clue what happens) │      │  → Animator.SetBool()   │
└─────────────────────────┘      └─────────────────────────┘
         INVISIBLE COUPLING via Inspector + Odin serialization
```

**Detection**:
```bash
# Find all scripts using BaseVariable pattern
grep -rl "Variable\." Assets/ --include="*.cs" | head -20

# Find all VariableListener usages in scenes
./tools/unity-migration-toolkit.sh so-find-listeners Assets/Scenes/
```

**Recommended migration**:
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

// Explicit subscriber
[RequireComponent(typeof(Animator))]
public class WalkAnimatorSync : MonoBehaviour {
    Animator _anim;
    void Awake() => _anim = GetComponent<Animator>();
    void OnEnable() => PlayerController.OnWalkingChanged += v => _anim.SetBool("isWalking", v);
    void OnDisable() => PlayerController.OnWalkingChanged -= v => _anim.SetBool("isWalking", v);
}
```

**Benefits**:
- Dependencies visible in code (no need to check Inspector/scene files)
- No Odin serialization dependency
- Survives Unity migrations without data loss
- Easier to debug and trace

## Common Unity Package GUIDs

| GUID | Script | Package |
|------|--------|---------|
| `dc42784cf147c0c48a680349fa168899` | GraphicRaycaster | com.unity.ugui |
| `f4688fdb7df04437aeb418b961361dc5` | TextMeshProUGUI | com.unity.ugui |
| `fe87c0e1cc204ed48ad3b37840f39efc` | Image | com.unity.ugui |
| `8b9a305e18de0c04dbd257a21cd47087` | PostProcessVolume | com.unity.postprocessing |
| `948f4100a11a5c24981795d21301da5c` | PostProcessLayer | com.unity.postprocessing |

## Recovery Workflow

```
1. IDENTIFY: grep scene for missing script GUIDs
2. CATEGORIZE: Package, Deleted, Renamed, or Unknown
3. EXTRACT: git show COMMIT:"Scene.unity" | grep -A50 "guid: GUID"
4. DECODE: echo "bytes_hex" | sed 's/\(..\)00/\1/g' | xxd -r -p
5. READ: declaringType, methodName, ParameterValues, unityReferences
6. REIMPLEMENT: Create clean C# scripts with explicit serialized fields
7. WIRE: Attach scripts, configure in Inspector
8. CLEANUP: Remove missing script components
```
