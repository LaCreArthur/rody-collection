# Save Awareness & Rody Maker UX Plan

> User stories exist only in memory until exported. This plan improves UX to prevent accidental data loss.

---

## Problem Statement

1. **Save vs Export confusion** - Save button only saves to memory, not to file
2. **No tooltips** - Buttons in Rody Maker have no hover explanations
3. **No exit warnings** - Escape key leaves editor without warning
4. **No visual indicator** - Users can't see if story is exported or not
5. **Browser close loses work** - No beforeunload warning in WebGL

---

## Phase 1: Save Button Clarity

### Current Behavior
- Save button (disquette) saves to `WorkingStory` (memory only)
- Export is separate action in main menu

### Proposed Change

**Option A: Save = Export**
- Save button triggers export dialog directly
- Clearer mental model: "Save" means "save to file"

**Option B: Save with Choice Dialog**
```
"Sauvegarder l'histoire"

[Exporter en fichier]     ← Downloads .rody.json
[Garder en mémoire]       ← Current behavior (temporary)

⚠️ "Garder en mémoire" = perdu si tu fermes le navigateur
```

**Recommendation**: Option A (Save = Export) is simpler and matches user expectations.

### Files to Modify
- `Assets/Scripts/RodyMaker/RM_MainLayout.cs` - Save button handler
- Wire to `RA_NewGame.OnExportClick()` or inline export logic

---

## Phase 2: Tooltips for All Buttons

### Rody Maker Main Toolbar

| Button | Current | Tooltip Text |
|--------|---------|--------------|
| Objets (cadre) | No tooltip | "Définir les objets à trouver" |
| Test (flèche verte) | No tooltip | "Tester la scène en mode jeu" |
| Sauvegarde (disquette) | No tooltip | "Exporter l'histoire en fichier" |
| INTRO | No tooltip | "Modifier titre, dialogues et musique" |
| IMG | No tooltip | "Changer les images de la scène" |
| Reset (flèche rouge) | No tooltip | "Annuler les modifications" |

### Implementation Options

**Option A: Unity UI Tooltip System**
- Create `TooltipTrigger.cs` component
- Add to each button with tooltip text
- Show floating panel on hover

**Option B: TextMeshPro + Event Triggers**
- Use existing UI, add EventTrigger for pointer enter/exit
- Show/hide tooltip text object

### Effort
~100-150 lines + prefab modifications

---

## Phase 3: Exit Warnings

### 3.1 Editor Escape Key Warning

**Location**: `RM_GameManager.cs:224`

```csharp
void Update() {
    if (Input.GetKeyUp(KeyCode.Escape)) {
        if (WorkingStory.IsDirty && !WorkingStory.IsOfficial) {
            ShowExitWarning();
        } else {
            SceneManager.LoadScene(2);
        }
    }
}

void ShowExitWarning() {
    // Reuse existing RM_WarningLayout
    var warningLayout = warningLayout.GetComponent<RM_WarningLayout>();
    warningLayout.isExitMode = true;
    warningLayout.messageText.text =
        "TU QUITTES L'ÉDITEUR\n" +
        "Attention Rody, ton histoire n'est pas exportée!\n" +
        "Elle sera perdue si tu fermes le navigateur.";
    warningLayout.gameObject.SetActive(true);
}
```

### 3.2 Menu Return Export Reminder

When returning to main menu (scene 0) with unsaved user story:

```
"Tu n'as pas exporté ton histoire!"

L'histoire "{title}" existe seulement en mémoire.
Si tu fermes le navigateur, elle sera perdue.

[Exporter maintenant]  [Continuer sans exporter]
```

**Files to modify**:
- `MenuManager.cs` - Check `WorkingStory.IsDirty` before `LoadScene(0)`
- `GameManager.cs:45` - Same check
- `Title.cs:23,92` - Same check

### 3.3 Browser beforeunload (WebGL)

**New jslib function**:
```javascript
// In StandaloneFileBrowser.jslib or new file
SetUnsavedWorkFlag: function(hasUnsaved) {
    window._rodyHasUnsavedWork = hasUnsaved;
},

// Auto-setup on load
$RodyBeforeUnload: {
    setup: function() {
        window.addEventListener('beforeunload', function(e) {
            if (window._rodyHasUnsavedWork) {
                e.preventDefault();
                e.returnValue = '';
            }
        });
    }
}
```

**C# side**:
```csharp
// Call when WorkingStory.IsDirty changes
[DllImport("__Internal")]
private static extern void SetUnsavedWorkFlag(bool hasUnsaved);
```

---

## Phase 4: Visual Indicators

### 4.1 "Non exporté" Badge on User Story Slots

In main menu, user story slots show orange indicator when not exported:

```
┌─────────────────┐
│ [Cover Image]   │ ⚠️ ← Orange badge
│                 │
│ "Mon Histoire"  │
└─────────────────┘
```

Tooltip on badge: "Non exporté - sera perdu si tu fermes le navigateur"

### 4.2 Dirty Indicator in Editor

Show asterisk (*) or dot next to save button when changes exist:
- Clean: 💾
- Dirty: 💾• or 💾*

---

## Phase 5: First-Time Guidance

On first story creation, show one-time tooltip:

```
┌────────────────────────────────────────────┐
│ 💡 Conseil                                  │
│                                            │
│ Ton histoire est sauvegardée en mémoire.   │
│ Clique sur 💾 pour l'exporter en fichier   │
│ et la garder définitivement!               │
│                                            │
│                            [J'ai compris]  │
└────────────────────────────────────────────┘
```

Store flag in PlayerPrefs: `RodyMaker_FirstTimeExportTipShown`

---

## Implementation Priority

| Phase | Description | Effort | Impact |
|-------|-------------|--------|--------|
| 1 | Save = Export | ~30 lines | High |
| 3.1 | Editor exit warning | ~40 lines | High |
| 3.2 | Menu return reminder | ~60 lines | High |
| 3.3 | Browser beforeunload | ~20 lines | Medium |
| 2 | Tooltips | ~150 lines | Medium |
| 4 | Visual indicators | ~50 lines | Medium |
| 5 | First-time guidance | ~40 lines | Low |

**Recommended order**: 1 → 3.1 → 3.2 → 3.3 → 2 → 4 → 5

---

## Files Reference

| File | Changes |
|------|---------|
| `RM_MainLayout.cs` | Save button → export, tooltip triggers |
| `RM_GameManager.cs` | Exit warning on Escape |
| `RM_WarningLayout.cs` | Add `isExitMode` |
| `MenuManager.cs` | Export reminder before scene 0 |
| `GameManager.cs` | Export reminder before scene 0 |
| `Title.cs` | Export reminder before scene 0 |
| `StandaloneFileBrowser.jslib` | beforeunload handler |
| `RA_ScrollView.cs` | "Non exporté" badge |
| `TooltipManager.cs` | NEW - Tooltip system |
| `TooltipTrigger.cs` | NEW - Per-button component |

---

## Decision Log

- **2026-01-21**: Decided against IndexedDB persistence - too unreliable (Safari eviction). Focus on export awareness instead.
- **2026-01-21**: Plan created, referenced in ROADMAP.md
