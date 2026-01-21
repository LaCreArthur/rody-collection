# Menu Story Slots UI/UX Enhancement Plan

## User Request
Add floating action buttons to story slots in the menu:
- **Official stories**: Fork button (top-right corner)
- **User stories**: Edit button (title/cover/delete) + Export button
- **Keep**: Big "Importer" button as-is
- **Assess**: Feasibility of future multi-story support

---

## UX Assessment (Expert Opinion)

**Your proposal is solid.** Here's my analysis:

### Strengths
1. **Contextual actions** - Buttons appear where relevant (fork on official, edit/export on user)
2. **Progressive disclosure** - Only shows actions that apply to each story type
3. **Reusing create popup for edit** - Consistent UI, reduces cognitive load
4. **Clear visual hierarchy** - Floating buttons don't compete with cover art

### Considerations
| Aspect | Assessment |
|--------|------------|
| **Touch targets** | At 960x600 resolution, 20x20 buttons are ~40px on most screens - acceptable |
| **Visual clutter** | 1-2 small icons per slot is minimal - good |
| **Discoverability** | Users may not notice small icons initially - consider tooltip on hover |
| **Consistency** | Fork/Edit/Export icons should use consistent style (outline vs filled) |

### Design Decisions
- Show buttons **only on selected slot** (the enlarged one) to reduce clutter
- Fork action opens edit popup first (user can customize before committing)
- Delete option inside edit popup (consolidates editing actions)

---

## Feasibility Assessment

### Current API Readiness: 95%

| Feature | API Support | Notes |
|---------|-------------|-------|
| Fork official story | ✅ `WorkingStory.ForkForEditing()` | Deep copy, adds "(copie)" suffix |
| Edit title | ✅ `WorkingStory.SetTitle(string)` | Updates title + regenerates ID |
| Edit cover | ✅ `WorkingStory.SaveSprite("cover.png", tex)` | Already used in RA_NewGame |
| Delete story | ✅ File.Delete + UI refresh | Already in RA_NewGame.UnsetfeedbackPanel(3) |
| Export JSON | ✅ `WorkingStory.ExportToJson()` | Already wired in RA_NewGame.OnExportClick() |
| Detect story type | ✅ `WorkingStory.IsOfficial` / `IsUserStory` | Clean differentiation |

### Multi-Story Future Support: Medium Effort

**Current limitation:** `WorkingStory` is static single-story class.

**What would change:**
```csharp
// Current
public static ExportedStory Current { get; }

// Future (not in scope, just estimation)
public static Dictionary<string, ExportedStory> Stories { get; }
public static string ActiveStoryId { get; set; }
```

**Estimation:** 2-3 days refactoring if needed later. Current changes **don't block** this path.

---

## Implementation Plan

### Step 1: Create SlotItem Component

**Create new `Assets/Scripts/RodyAnthology/RA_SlotItem.cs`:**

```csharp
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component attached to each Slot prefab to hold serialized references to its children.
/// </summary>
public class RA_SlotItem : MonoBehaviour
{
    [Header("Existing Children")]
    public Image coverImage;
    public Text titleText;

    [Header("Action Buttons")]
    public GameObject actionButtonsContainer;
    public Button forkButton;
    public Button editButton;
    public Button exportButton;

    // Runtime state
    [HideInInspector] public string storyId;
    [HideInInspector] public int slotIndex;
    [HideInInspector] public bool isOfficialStory;
}
```

### Step 2: Modify Slot.prefab Structure

**Add to `Assets/Prefabs/Slot.prefab`:**
```
Slot (existing + add RA_SlotItem component)
├── Image (child 0 - cover) → wire to coverImage
├── Title (child 1 - story name) → wire to titleText
└── ActionButtons (NEW - child 2) → wire to actionButtonsContainer
    ├── ForkButton → wire to forkButton
    ├── EditButton → wire to editButton
    └── ExportButton → wire to exportButton
```

**Button specs:**
- Size: 20x20 pixels
- Position: Top-right corner, anchored to (1, 1)
- ForkButton offset: (-5, -5)
- EditButton offset: (-5, -5)
- ExportButton offset: (-25, -5) - left of edit

**Initially all buttons disabled** - enabled via code based on story type.

### Step 3: Add Icon Sprites

Create or source simple icons:
- Fork: branching arrow or copy icon
- Edit: pencil icon
- Export: download/share icon

Place in `Assets/Sprites/RodyAnthology/Icons/`

### Step 4: Modify RA_ScrollView.cs

**Add list to track SlotItems:**
```csharp
List<RA_SlotItem> slotItems = new List<RA_SlotItem>();
```

**Modify slot instantiation (official stories):**
```csharp
// After instantiating slot
var slotItem = slot.GetComponent<RA_SlotItem>();
if (slotItem != null)
{
    slotItem.storyId = story.id;
    slotItem.slotIndex = slots.Count;
    slotItem.isOfficialStory = true;
    slotItems.Add(slotItem);

    // Official stories: show fork button only
    slotItem.forkButton.gameObject.SetActive(true);
    slotItem.editButton.gameObject.SetActive(false);
    slotItem.exportButton.gameObject.SetActive(false);

    // Wire fork button
    string capturedId = story.id;
    slotItem.forkButton.onClick.AddListener(() => OnForkClick(capturedId));

    // Hide action buttons initially (shown on selection)
    slotItem.actionButtonsContainer.SetActive(false);
}
```

**For user stories (in LoadUserStories):**
```csharp
var slotItem = slot.GetComponent<RA_SlotItem>();
if (slotItem != null)
{
    slotItem.storyId = storyId;
    slotItem.slotIndex = slotIndex;
    slotItem.isOfficialStory = false;
    slotItems.Add(slotItem);

    // User stories: show edit + export, hide fork
    slotItem.forkButton.gameObject.SetActive(false);
    slotItem.editButton.gameObject.SetActive(true);
    slotItem.exportButton.gameObject.SetActive(true);

    // Wire buttons
    int capturedIndex = slotIndex;
    slotItem.editButton.onClick.AddListener(() => OnEditClick(capturedIndex));
    slotItem.exportButton.onClick.AddListener(() => OnExportClick(capturedIndex));

    // Hide action buttons initially
    slotItem.actionButtonsContainer.SetActive(false);
}
```

**Add new methods:**
```csharp
void OnForkClick(string storyId)
{
    WorkingStory.LoadOfficial(storyId);
    WorkingStory.ForkForEditing();
    ngScript.OpenEditPopup();
}

void OnEditClick(int slotIndex)
{
    // Load story if needed, then open edit popup
    ngScript.OpenEditPopup();
}

void OnExportClick(int slotIndex)
{
    ngScript.OnExportClick();
}
```

**Modify updateSlotSprites to show buttons on selection:**
```csharp
void updateSlotSprites(int index)
{
    for (int i = 0; i < slotItems.Count; i++)
    {
        var slotItem = slotItems[i];
        if (slotItem != null && slotItem.actionButtonsContainer != null)
        {
            // Only show action buttons on selected slot
            slotItem.actionButtonsContainer.SetActive(i == index);
        }
    }
    // ... existing selection logic
}
```

### Step 5: Add Edit Popup to RA_NewGame.cs

**Add new serialized fields:**
```csharp
[Header("Edit Mode")]
public Button deleteButton;
public Text acceptButtonText;
bool isEditMode = false;
```

**Add OpenEditPopup method:**
```csharp
public void OpenEditPopup()
{
    isEditMode = true;

    // Pre-fill with current story data
    titleInput.text = WorkingStory.Title;
    imgInput.text = WorkingStory.HasSprite("cover.png") ? "cover.png" : "";

    // Change button label
    if (acceptButtonText != null)
        acceptButtonText.text = "Sauvegarder";

    // Show delete option
    if (deleteButton != null)
        deleteButton.gameObject.SetActive(true);

    newGamePanel.SetActive(true);
}
```

**Modify NG_OnAcceptClick:**
```csharp
public void NG_OnAcceptClick()
{
    string title = titleInput.text;
    if (string.IsNullOrEmpty(title))
    {
        // ... existing error handling
        return;
    }

    if (isEditMode)
    {
        // Update existing story
        WorkingStory.SetTitle(title);
        if (coverImgSprite != null)
            WorkingStory.SaveSprite("cover.png", coverImgSprite.texture);

        // Success feedback
        feedbackTxt.text = $"L'histoire \"{title}\" a été mise à jour!";
        yeapTxt.text = "ok";
        buttonYeap.SetActive(true);
        buttonYeap.GetComponent<Button>().onClick.RemoveAllListeners();
        buttonYeap.GetComponent<Button>().onClick.AddListener(delegate {
            UnsetfeedbackPanel(0);
            sv.Reset(); // Refresh menu
        });
        newGamePanel.SetActive(false);
        feedbackPanel.SetActive(true);

        // Reset edit mode
        isEditMode = false;
        if (acceptButtonText != null)
            acceptButtonText.text = "Créer";
        if (deleteButton != null)
            deleteButton.gameObject.SetActive(false);
    }
    else
    {
        // ... existing create logic
    }
}
```

**Add OnDeleteClick method:**
```csharp
public void OnDeleteClick()
{
    feedbackTxt.text = "Supprimer cette histoire définitivement?";
    yeapTxt.text = "oui";
    buttonYeap.SetActive(true);
    buttonYeap.GetComponent<Button>().onClick.RemoveAllListeners();
    buttonYeap.GetComponent<Button>().onClick.AddListener(delegate {
        WorkingStory.Clear();
        feedbackPanel.SetActive(false);
        newGamePanel.SetActive(false);
        buttonYeap.SetActive(false);
        buttonNop.SetActive(false);
        isEditMode = false;
        if (acceptButtonText != null)
            acceptButtonText.text = "Créer";
        if (deleteButton != null)
            deleteButton.gameObject.SetActive(false);
        sv.Reset();
    });
    buttonNop.SetActive(true);
    newGamePanel.SetActive(false);
    feedbackPanel.SetActive(true);
}
```

**Modify NG_OnCancelClick to reset edit mode:**
```csharp
public void NG_OnCancelClick()
{
    newGamePanel.SetActive(false);
    isEditMode = false;
    if (acceptButtonText != null)
        acceptButtonText.text = "Créer";
    if (deleteButton != null)
        deleteButton.gameObject.SetActive(false);
}
```

---

## Files to Create/Modify

| File | Changes |
|------|---------|
| `Assets/Scripts/RodyAnthology/RA_SlotItem.cs` | **NEW** - Component with serialized button references |
| `Assets/Prefabs/Slot.prefab` | Add RA_SlotItem component, ActionButtons child with Fork/Edit/Export buttons, wire all references |
| `Assets/Sprites/RodyAnthology/Icons/` | Add icon sprites (fork, edit, export) |
| `Assets/Scripts/RodyAnthology/RA_ScrollView.cs` | Add slotItems list, wire buttons via serialized refs, add OnForkClick/OnEditClick/OnExportClick methods, show buttons on selection |
| `Assets/Scripts/RodyAnthology/RA_NewGame.cs` | Add deleteButton/acceptButtonText fields, OpenEditPopup(), modify NG_OnAcceptClick() for edit mode, add OnDeleteClick(), modify NG_OnCancelClick() |
| `0_MenuCollection` scene | Add delete button to newGamePanel, wire acceptButtonText reference |

---

## Verification Plan

1. **Fork button (official stories)**
   - Select an official story → fork button visible
   - Click fork → story copied with "(copie)" suffix
   - Edit popup opens with forked story data
   - Save → new story appears in menu as user story

2. **Edit button (user stories)**
   - Select a user story → edit button visible
   - Click edit → popup opens with current title/cover
   - Change title → save → menu refreshes with new title
   - Click delete → confirmation → story cleared, menu refreshes

3. **Export button (user stories)**
   - Click export → JSON downloads
   - Same behavior as current export

4. **Button visibility**
   - Buttons only show on selected (enlarged) slot
   - Scroll to different slot → buttons move to new selection

5. **Cancel behavior**
   - Cancel edit popup → resets to create mode
   - UI labels restore to "Créer"

---

## Estimated Effort

| Task | Effort |
|------|--------|
| Create RA_SlotItem.cs | 30 min |
| Prefab modifications + icons | 1-2 hours |
| RA_ScrollView button wiring | 2-3 hours |
| RA_NewGame edit mode | 2-3 hours |
| Scene wiring + testing | 1-2 hours |
| **Total** | **7-11 hours** |

---

## Key Pattern: Serialized Fields

**Why serialized fields instead of Transform.Find():**
- **Inspector visibility** - Easy to verify wiring is correct
- **Compile-time safety** - Missing references show as warnings
- **Performance** - No runtime string lookups
- **Refactoring safety** - Rename children without breaking code
- **Unity convention** - Follows standard Unity patterns
