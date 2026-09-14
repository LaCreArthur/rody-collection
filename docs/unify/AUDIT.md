# Story and editor audit

Updated **2026-09-10** during the single-workspace implementation. This file owns
unresolved defects and acceptance evidence; [architecture](ARCHITECTURE.md) owns
the implemented mechanisms, [game design](../GAME_DESIGN.md) the product contract,
and [roadmap](../ROADMAP.md) release status. Local code is not a deployed release.

## Changes inspected; browser acceptance pending

The previous audit covered commit `2d7555e` and the multiple-story/local-Save model.
Its findings and exact prior evidence remain in Git at `d1d2fe8:docs/unify/AUDIT.md`.
That model is replaced, so its old library/Export repairs are not separate work.

| Prior defect | Current source change | Evidence still required |
|---|---|---|
| Reset retained imported images; dirty omitted panel edits | One typed draft plus deep whole-story restore snapshot; ordinary controls mutate the draft directly | Edit text, speech, images and structure across scenes, then Discard before/after Save |
| Paintbrush bypassed original duplication; playing an original replaced personal work | All editor entries use the root replacement boundary; original play target is separate from the workspace | Enter from collection, menu and paintbrush; original content and personal editor cursor survive |
| Same-id files took precedence over originals | Original-only catalog; one personal card projects the draft rather than resolving its id | Import the same title/id as an original and exercise both cards |
| Invalid import could report success for the previous story | Candidate structure and images are checked before replacement; picker cancellation/error is explicit | Malformed JSON, missing content, invalid speech ranges, unreadable image, same-file reopening |
| Save resized titles, synthesized frames and retained removed frames | Save serializes encoded content; image import owns conversion; frame removal compacts keys | Download/reimport preserved all encoded sprites; newly added/removed animation sequence still needs authoring evidence |
| Multiple target UI saved only its first target | User explicitly chose one target pair per objective; extra-target machinery removed | Draw all three objectives, Test and reimport; original art direction unchanged |
| Unclear hydration and overwritten sync callbacks | Root initializes once; fixed draft/snapshot envelope; coalesced single writer | Browser refresh after completed writes; slow/failed flush and failed hydration |
| Inconsistent Save/Export/error copy | Single file Save and whole-story Discard; shared modal and explicit unavailable platform errors | Reachable controls and feedback in the assembled scenes and browser |

All seven built-ins were subsequently resolved through the live Editor catalog and
read as format 2, with scene counts 24/18/16/17/14/17/17. This covers runtime parsing
and the legacy upgrade, not full playthroughs or browser file import.

## Focused Editor evidence — 2026-09-10

Used the running Unity Editor, actual serialized button callbacks and input change
events. The following narrow journeys passed:

- Startup rendered seven originals and one empty personal slot. Duplicating the
  first original opened a clean 24-scene workspace.
- Text entered in two scenes remained in the draft after navigation and marked it
  changed. Test played the current title; after simulated gameplay cursor movement,
  the actual paintbrush callback returned to the remembered editor scene.
- Playing the first original through its collection callback used the original
  text, separately from the changed personal draft. Returning to the personal story
  kept its text and editor cursor.
- The actual Save button reported download unavailable outside WebGL. Dirty state
  and the older restore snapshot remained intact. Discard confirmation restored
  both edited scenes and disabled Discard afterwards.
- Editing the second objective's text preserved the first objective's text. The
  detailed editor retained its 320×130 preview geometry.
- The local recovery record contained the changed draft and older restore snapshot.
  Restarting Play Mode after Discard recovered the clean workspace. Dirty browser
  reload and IndexedDB persistence were not exercised.

Inspected rendered collection and Maker views after correcting action-label spacing
and the full-title preview. Final screenshots are outside Assets at
`/tmp/rody-workspace-verification/workspace-collection-final.png` and
`/tmp/rody-workspace-verification/workspace-title-final.png`; they are session evidence,
not permanent tutorial assets. A native-pointer probe did not activate the Unity UI,
so these callback checks do not establish pointer input or target dragging.
Removed only the probe's newly created local workspace record and temporary images;
the pre-existing story directory was untouched. Restored stopped Play Mode and the
original clean `2_Menu` scene.

## Independent implementation review

A fresh source-only review identified two material defects in the first draft:

1. Continue after failed hydration left automatic recovery permanently disabled for
   the session with no later retry; Save still claimed the browser remembered work.
   Save feedback now reflects unavailable recovery. September 14 removed the
   unapproved status strip and its separate retry action; error-dialog retry remains.
   A retry route after dismissing that dialog is unresolved; no replacement was approved.
2. Accepting the redrawn near region before its new target could persist an invalid
   pair when leaving midway. The two view rectangles now form one pending geometry
   edit and commit together only after a completed target drag. Exercise a smaller
   near-region redraw → Escape/reopen, then a completed pair and gameplay hit test.

Both corrections still need their failure/pointer runtime journeys. A separate
fresh serialized-reference review inspected the assembled scenes and shared modal,
including the final preview/spacing changes, with no material findings. It checked
removed script GUID consumers across Assets/Packages, base references, prefab
overrides and all 30 scene thumbnail indices. Supplied final renders were also
inspected. This was a read-only review, not a browser interaction test.
That implementation checkpoint preceded the browser build below; no test suite ran.

## Local browser evidence — 2026-09-10

Arthur authorized the local build, then explicitly requested less ceremony and
token expenditure. This pass covered the three agreed browser boundaries rather
than expanding into the full release matrix.

- Unity 6000.5.10f1 WebGL build succeeded: zero errors, 77,555,874 bytes, 455 seconds.
  Four warnings concerned disabled player Pipeline control and large TMP generated
  methods. Code revision `14d4c0d`; pre-existing local baked-asset changes were
  preserved, so this is not a clean-checkout release candidate.
- Chrome 152.0.7977.83, isolated profile, `http://127.0.0.1:8765/`: actual Save
  downloaded a file. Native file selection reimported the later edited download;
  its complete parsed content matched both the draft and restore point, with 24
  scenes, 100 sprites and clean state.
- Typed scene 9 title `RODY RECUPERE`; IndexedDB contained that dirty draft and
  older `LA GROTTE` snapshot. Reload, personal-card Edit and the visible title
  retained the edit. Actual Discard confirmation restored the entire snapshot;
  the persisted draft matched it and dirty became false.
- Real pointer drags drew a near region and inner target. Their positions/sizes
  matched the rendered rectangles and persisted as one pair; the download/reimport
  above preserved them. Interrupted redraw, resized/fullscreen alignment and actual
  gameplay hit testing remain unverified.

No runtime source change was needed. Early failed probes used wrong controls or
did not establish an accepted edit; they are not product failures or passing tests.
Browser logs also reported unavailable FSR upscaling and Unity's future persistent
filesystem sync change; no application exception was captured in these journeys.
Artifacts and screenshots: `/tmp/rody-workspace-verification/`. Nothing published.

## Release boundaries still open

The [workspace plan's acceptance journeys](../EDITOR_WORKSPACE_PLAN.md#narrow-acceptance-evidence)
own the required browser scenarios. In particular, Editor callbacks cannot prove:

- Native picker cancellation/read failure; normal import and download passed above.
- Temporary-file replacement and IDBFS behavior during failures, beyond the normal
  writes and refresh exercised above.
- Single-writer ordering during slow/failing storage and Save/replacement locking
  during delayed browser handoff.
- Newly authored French corrections/expression, structural/image edits and frame
  removal through browser round-trips; the intact downloaded story passed above.

The predecessor deployed branch contains per-story browser storage. Before public
cutover, establish whether people have browser-only personal stories and export
those through the previous version. No existing `Stories/` data is deleted, and
this implementation does not add a permanent legacy-library recovery surface.

## Deliberately retained limits

- New stories start with one scene; the editor contains 30 thumbnail positions.
  Later-scene deletion by re-click remains available only from scene 18 onward.
  A coherent scene-management policy is separate scope; do not advertise an
  unrestricted or former “16–29 scenes” promise.
- The existing two groups of three animation controls remain. Only explicitly
  authored frames are saved; preview/removal must be exercised in the assembled UI.
- The target editor uses the existing 320×200 coordinate/layout contract. Verify
  the rendered target after browser resize/fullscreen before claiming alignment.
- The retro font's character coverage remains limited. No visual redesign is
  authorized; new help must fit the existing art and legibility constraints.
- Browser pickers/downloads report unavailable in the Editor and desktop player.
  Desktop parity, simultaneous multi-tab editing and automatic legacy migration
  are outside this implementation.
- `isZambla` remains in voice data; its authoring control is deferred. Preserve
  Ibiza playback. Speech-engine/timing evidence belongs to [SPEECH_ENGINE.md](../SPEECH_ENGINE.md).
