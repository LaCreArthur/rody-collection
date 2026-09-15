## TL;DR (Arthur)

Le Maker garde le cadre, les pixels et les boutons du jeu original.
L’introduction apparaît complète ; cliquer une réplique la sélectionne et l’édite.
La saisie est automatiquement limitée à la place disponible, retours compris, sans défilement.
Chaque objectif présente ensemble son indice, sa voix et sa cible.
Glisser dans le décor redessine directement ; la proximité suit avec un réglage +/−.
Scènes remplace Intro ; Musique remplace Objets ; Images et Voix gardent leurs outils actuels.
Test lance le vrai jeu ; le retour retrouve la sélection et Rétablir reste disponible.
Implémenté localement ; correction des pixels le 15 septembre ; validation navigateur restante.

---

## Technical body (executing agent)

### Authority and baseline

Arthur explicitly requested implementation after the plan below. This records the
accepted September 14 conversation, including its final corrections: show all three
introduction texts together with the active passage highlighted; no text scrolling;
fit actual rendered text including line breaks; automatically crop the insertion
that exceeds that space instead of rejecting the whole paste.

Baseline `11f0e90`, master one commit ahead of fetched origin/master. Unrelated
lighting/navigation assets and ProjectAuditorSettings were already dirty; leave them
untouched. No build, publication or push is authorized. The frozen
[HTML study](prototypes/rody-maker.html) predates the final composed-intro/text-fit
corrections and uses fake art, voice, music and imports. It is an interaction
reference, not the Unity implementation or portable story format.

### Accepted interface

- Keep uGUI, 320×200 canvas, upper 70px panel, 320×130 picture, 28×32 tool cells
  and their existing positions. Main tools: Music/Test/Save above Scenes/Images/Restore.
  Contexts use the existing rightmost 2×2 cells. Existing pictograms, font, borders,
  tooltip and feedback panels remain; no long labels forced into the tool icons.
- Replace permanent thumbnails with clickable scene title, composed introduction
  or selected objective clue, and `INTRO 1 2 3 · OBJETS 1 2 3` along the bottom.
  Allocate title 12px, body 48px, selectors 8px and borders 2px within the 70px panel.
  Do not shrink the picture or add a status strip.
- Intro displays all nonempty `intro1/2/3` in order with the same newline joining
  as gameplay. A discreet background and edge marker identify the active passage.
  Click a passage to select and edit in one gesture; other passages stay visible.
  Numbered selectors also reach empty slots without predecessor-text gating.
- An objective selection owns its displayed clue, speech and near/target pair.
  Only the selected objective appears; hide zones on intro and nonediting panels.
- Text tools: Voice/Done, Speaker-or-Target/Return. Speaker is Mastico versus the
  illustrated character, not audio mute. Title editing has Done/Return. Typed text
  is accepted into the draft; Done/Return/Escape retain it. Escape closes the current
  editing context before global navigation and must not trigger InputField rollback.
- Real SynthManager remains the voice editor, with independent displayed/spoken text,
  full SpeechDocument Apply/Cancel, character pitch and fixed Mastico/Zambla pitch.
  Images, animation groups/import/remove/preview, music selection and listening reuse
  current screens. Returning restores the selected passage. The picture never opens Images.

### Text fitting contract

- Fit by native TextGenerator, font Rody15, lineSpacing0.6, no rich text/best-fit,
  actual wrapping and explicit newlines. Measure the composed introduction, not
  independent 190-character limits. Title and each clue have separate fit checks.
- Accepted candidates fit both the player rectangle (body252.6×54.45,
  title255.1×11.6) and the narrower Maker rectangle (224×48, title224×12).
  Four-tool contexts can use the original wider text area. No scrolling, shrinking
  type, ellipsis of story text or extra text-entry popup.
- Crop only the contiguous prefix of the new insertion/paste that fits around its
  selected replacement range. Preserve unchanged prefix/suffix and other passages.
  If none fits, retain the prior selection and text. Never skip rejected letters
  to admit later letters. Preserve Unicode text elements and normalize CRLF.
- Crop at user input only; never on bind, scene navigation, load or return from Test.
  Remove the arbitrary InputField39/190 limits, not the independent speech limits.
  A brief existing-tooltip message explains a crop; no confirmation or misleading
  character-remaining promise. Typed additions stop naturally when the box is full.
- The composite is derived, never stored or parsed back into separate dialogue data.
  Place passage blocks using the composite line metrics, not the sum of independent
  preferred heights (which adds inter-block vertical space). Highlights do not raycast.
- Read-only native TextGenerator measurements found all titles, composed intros and
  clues of the seven built-ins fit 224×48 (titles224×11.6). This is not visual/runtime
  acceptance of the assembled new UI. 42px would overflow several existing intros.

### Scene browser and existing capacities

Scenes temporarily replaces the text with four columns of image-only thumbnails,
vertical scrolling, selected-card marker and automatic visibility of that card.
September15 correction: titles/numbers belong only in the existing tooltip; remove
card captions and their ellipsis code. Native-size Rody15 text is the minimum,
including tooltips, padding units and delete controls. Use native28×32 tool sprites,
never squeeze an80×56 menu illustration into a tool cell. The960×600 Game view is
only a3× enlargement of the hard320×200 layout; inspect captures at320×200. Card selection closes the
browser, including clicking the selected card. Right controls are an inert Scenes
illustration, Return, Previous, Next. Arrows select adjacent scenes, not grid pages;
disable at the ends. Include a distinct title-screen card, with no scene dialogues,
objectives or music controls; its Test uses the real title scene.

Keep the existing authoring rules: create up to29 scenes; show every scene already
loaded; no reorder. Add creates the next index, selects Intro1 and focuses its title.
Delete is an explicit cross on the currently selected eligible card only (index≥18),
with named confirmation; then select the preceding scene and retain the passage.
Do not silently expand deletion to arbitrary cards or redesign the capacity policy.

### Direct target drawing

Select an objective → pointer-down anywhere on the picture starts zone editing and
the same drag draws a new exact target. No prior activation and no move/resize handles.
Reuse the 320×130 surface and nonraycastable overlays. Use RectTransformUtility and
the Canvas event camera, not Screen.width scaling. Clamp and round new endpoints to
logical picture pixels; reverse drags work; dragging beyond the image continues bounded.

Only local preview changes while drawing. A plain click/zero-area drag retains the
previous pair and opens its settings. The left two tool cells form +/value/−; the right
cells are Validate/Cancel. Passage/panel changes are blocked only during this context.
Validate atomically writes both rectangles once; Cancel/Escape/forced context exit
abandons the entire edit. Pointer interruption/focus loss abandons the current gesture,
retaining any preceding local preview. Never commit at mouse-up.

- New/unconfigured target or first redraw of old free-form near: start at8px (Arthur's
  explicit choice). Valid uniform existing margins survive reopening/redrawing.
- +/− changes1px immediately; minimum0; maximum the smallest integer expansion that
  covers the whole picture. Disable + at that saturation. No unbounded ineffective values.
- Old asymmetric zones retain exact floats until an accepted edit. Show `—`; first
  +/− proposes9/7 from the8px base. Changing padding alone preserves absolute target.
- Keep ObjectZone's eight floats: near centered in image coordinates, target center
  relative to near center. Views can be siblings using absolute target center;
  convert explicitly on commit. No persistent padding field or bulk conversion.
- Infer uniform margin as Ceil(maximum observed side margin−epsilon), then verify
  re-expansion/clamping at0.001 tolerance. Maximum uses Ceil(max distances to bounds,0).
  Zero-size targets are unconfigured, never inferred as valid zero-padding targets.
  Existing out-of-image coordinates stay untouched when simply loaded/viewed.

### State, implementation sequence and deletion accounting

Keep StorySession/StoryWorkspace as sole draft/checkpoint owner. Add a six-value
editor passage cursor to workspace recovery, not portable Story. Scene changes retain
the passage; new scene/workspace defaults to Intro1; title screen suspends display.
One local Maker panel state; remove authoritative activeDial/activeObj duplicates.
Keep actual Test launch and separate gameplay cursor; return to the remembered editor
scene AND passage. Test/recovery do not advance the restore point. Save still downloads
the complete draft; whole-story Restore keeps its existing confirmation and all content.

1. Record this plan and product decisions; implement workspace, composed text/crop,
   shared selection, real voice/image/music access and temporary scene browser.
2. Implement direct transactional zones and verify actual Test/return selection.
3. Delete orphaned menus/callbacks/assets after their final consumers migrate;
   update current references and inspect the assembled UI.

Explicit deletions: permanent thumbnails; intermediate Intro/Dialogues/Objects menus;
near-first/target-second drawing; activation button; mouse-up commit; re-click-to-delete;
character-only display-text caps. Do not transplant mock browser voice/import/test UI.
No generic editor framework, compatibility layer, duplicate draft, undo history or
new format. Trace scene events, base prefab fields AND variant propertyPath overrides.

### Acceptance and status

Targeted checks, not a new test suite:

| Journey | Required observation |
|---|---|
| Three intro passages, one empty, long native text | Complete composition, direct selection, correct speaker/voice, no overlap or scroll |
| Type/paste/replace at start, middle and end, newlines/accents | Only new excess cropped, existing suffix/other passages preserved, correct caret |
| Escape in text or zone editor | Close only foreground context; preserve accepted text, discard unvalidated geometry |
| Objectives1/2/3 | Correct clue, voice and target together |
| Drag/reverse/edge/plain click/focus interruption | Direct gesture, bounded geometry, no premature model mutation |
| Old/free-form/new target; padding and reopen | Old floats unchanged until Validate; new8px; same effective margin after reopening |
| Browser/title/add/eligible delete; image/frame/music tools | Expected navigation and current capacities retained |
| Edit two scenes, voice, art and zones; Test; advance; return | Real gameplay uses draft; editor restores original scene/passage |
| Save, modify, Test, Restore; browser download/reimport/refresh | Whole-story checkpoint independent from play, content and cursor survive |

Planning reviews corrected zero-size margin inference and limited the deletion cross
to the active eligible card. A cold review of insertion cropping found no material gap.
Implemented locally September15. Independent source/serialized review covered the
new bindings; its missing-tooltip, empty-slot focus, frame-return and touch-input
findings were addressed. Removed the three obsolete menu controllers and their
serialized menu objects, old base-layout helpers, and permanent thumbnail instances.

Focused Editor probes exercised native input insertion/replacement cropping (including
newlines, accents and preserved suffix/other intro), title width fitting, empty intro
selection, EventSystem hits for text and picture, reverse/clamped target drawing,
Validate/Cancel, real voice-editor entry/return, scene add/eligible delete, title/image/
music navigation and actual whole-story restore. Test launched the real game; gameplay
advancement was simulated before exercising the actual Maker return event, which
retained the editor scene/passage and restore availability. This was not a played-through
adventure. Workspace serialization retained the passage cursor. Original test workspace
was restored at the end of that probe session; later user work is not overwritten.

September15 pixel correction was checked by rendering the actual camera to320×200
and inspecting Home, Scenes with a tooltip, and zone tools. The scene icon is now a
native28×32 asset; captions and their font9 rendering are deleted, previews enlarged,
tooltips/units/delete labels use Rody15, and +/− uses its integer2× size30. Tooltip
bounds and position round to logical pixels. No OS keyboard/device touch test, new
build or browser acceptance was run for this UI; older browser evidence does not
cover these changes. No automatic build/push.
