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

## Story Flow Bugs

### Final Scene Intro Is Not Credits

- In gameplay, the intro `Next` button must not infer `last scene => go to credits` from `CurrentSceneIndex + 1 > SceneCount` alone.
- The object phase still belongs to the current scene, including the final one. Credits should only happen after the object flow finishes, or when the current scene truly has no primary object target.
- The bug lived in `ClickHandler.NextClick()` and was exposed by starting directly on the final scene from `2_Menu`.

### Scene Picker State Persists In Scene Asset

- `2_Menu.unity` serializes `MenuManager.sceneToLoad`, so a manual editor selection can become the default next test run.
- If a flow looks scene-dependent, verify the serialized scene state before debugging runtime logic.
- `Clickable.cs` updates `sceneToLoad` from toggle state, but nothing forces a clean default when the menu scene opens.

---

## Menu Grid Quirks

### Fixed 16-Slot Scene Grid Is Intentional

- `2_Menu` deliberately recreates the old Atari ST layout with exactly 16 preview slots.
- Official stories with more than 16 scenes are intentionally truncated in this menu.
- Preserve that limitation unless the product goal changes. It is a behavior choice, not a bug.

### Empty Preview Slots Need Explicit Placeholder State

- The preview grid cannot rely on missing thumbnails alone. If a story has fewer than 16 scenes, unused slots must be treated as "not a scene" with explicit visuals and interaction rules.
- The current safe behavior is: keep the baked placeholder preview art from the scene, use the existing `Scene_LayoutOff`/`Scene_LayoutOn` framing, tint the inner preview dark grey when disabled, and make the toggle non-interactable.
- If empty slots are left interactable, the menu can launch ghost scene indices that do not exist in the current story.

### Runtime Depends On Scene-Authored Placeholder Art

- `MenuManager` currently caches the default preview sprites already authored into the 16 slot Images, then restores those for missing scenes.
- This is pragmatic and preserves the original layout, but it couples runtime behavior to whatever art happens to be serialized into `2_Menu.unity`.
- Refactor direction: expose a dedicated serialized placeholder preview sprite or prefab contract instead of reading the scene's initial child Image state at runtime.

### Selection State Is Still Polling-Based

- `Clickable.cs` updates `sceneToLoad` and `actionToLoad` every frame from toggle state in `Update()`.
- This works, but it spreads menu state across polling logic and serialized toggles, which makes bugs harder to reason about when a slot is disabled, preselected, or manually changed in the editor.
- Refactor direction: move scene/action selection to explicit `Toggle.onValueChanged` handlers and let `MenuManager` own the authoritative menu state.

### Animation Logic Assumes The Layout Never Changes

- `MenuManager` now initializes previews from `scenes.Length`, but the reveal animation still hardcodes the Atari order for 16 slots.
- That is fine as long as the layout stays frozen. If anyone ever changes slot count or ordering, the animation code will silently become wrong.
- Refactor direction: encode reveal order as data, or derive it from the arranged slot list instead of hardcoding index walks.

---

## Editor Tooltip Pattern

### Use A Dedicated Tooltip Panel, Not Existing Feedback UI

- Reusing the save/export status panel as a tooltip was a bad idea: it stole focus, covered too much of the screen, and flickered because hover state fought the overlay.
- The working pattern in `6_RM_Main` is a dedicated tooltip GameObject authored in the scene and wired into `RM_MainLayout` via serialized references.
- Keep tooltip UI separate from persistent feedback panels, warnings, and modal layouts.

### Hover Wiring Is Reusable

- `RM_ButtonTooltip` is the reusable hover component. It can be attached to any interactable UI control and delegates the actual display to `RM_MainLayout`.
- `RM_MainLayout` currently demonstrates two wiring paths: by object name for one-off prototype targets and by serialized `Button` reference for stable editor buttons.
- Prefer serialized references when they already exist; use name lookup only for quick prototypes.

### Tooltip Sizing And Placement Rules

- Tooltip size should be driven by inner text dimensions plus explicit border padding, not by a fixed-width banner.
- Placement should default below the hovered button, clamp horizontally inside the canvas, and flip above the button when the lower placement would exit the screen.
- Tooltip visuals must ignore raycasts, or hover can flap on and off when the tooltip overlaps the source button.

### Refactor Direction

- The current implementation is good enough to scale across the editor, but animation and style polish should stay separate from the base behavior work.
- If the pattern spreads widely, promote tooltip registration to a more explicit catalog instead of scattering raw French strings through layout controllers.

---

## Rody Phoneme Conversion

### The Runtime Alphabet Is Fixed And Small

- `SoundManager.StringToPhonemes()` and `getPhoneme()` already define the real runtime contract: underscore-separated tokens inside words, spaces between words, and an automatic `rienp` pause appended after each word.
- `P.cs` exposes the core inventory: roughly 40 phoneme tokens plus punctuation and a few special sounds (`ti`, `ouu`, `cuicui`, `pop`).
- This is not IPA and not generic French phonemization. It is a game-specific spelling system tuned to the old Atari ST robot voice.

### The Shipped Stories Form A Useful Training Corpus

- The current shipped story JSONs contain about 510 aligned French text to phoneme examples across the bundled adventures.
- That is enough data to learn frequent words, common spelling patterns, and the system's preferred approximations.
- The corpus repeatedly shows stable custom spellings such as `c'est -> s_et`, `pere-noel -> p_ai_r_n_o_ai_l`, `etoile -> et_t_oi_l`, `ou -> ou`, and `grele -> g_r_ai_l`.

### The Corpus Is Dirty

- The shipped JSONs currently contain tokens the runtime parser does not recognize, including `!`, `.p`, `M`, `ca`, `il`, and `w`.
- Because unknown tokens currently fall back to pause, these strings can silently degrade playback instead of failing loudly.
- Any future French-to-Rody converter needs a normalization pass first, otherwise it will learn from inconsistent source data and reproduce garbage.

### Full Auto-Conversion Is The Wrong First Milestone

- A one-click "perfect" French-to-Rody phoneme converter is not realistic yet. The phoneme language is custom, approximate, and inconsistent in places.
- The correct first milestone is assisted conversion: dictionary hits plus rules for common French patterns, confidence scoring, and manual correction for the weird cases.
- The best user flow is: write French text, generate a suggested phoneme string, highlight low-confidence chunks, audition it in the existing synth, then save corrections.

### Refactor Direction

- Start with a corpus extractor and normalization tool before touching editor UX.
- Build a local dictionary from shipped story pairs first, then add deterministic French spelling rules for the common cases (`est`, `ai`, nasal vowels, `ch`, `gn`, `oi`, `ui`, soft/hard `c` and `g`, etc.).
- Store user corrections locally and feed them back into the suggestion system. That matches the existing roadmap idea better than chasing full automation from day one.

---

## Key Principles

1. **Git history is authoritative** - Check git for original data when current scene shows empty fields
2. **Code analysis alone is insufficient** - Scene instances can have additional components not visible in prefab
3. **Static events > Inspector wiring** - Dependencies visible in code, survives migrations
4. **Verify before concluding** - Always grep to confirm patterns are gone before declaring fix complete
