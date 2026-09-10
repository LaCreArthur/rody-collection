## TL;DR (Arthur)

Keep the originals and one “My story” that you can edit, play and recover after refresh.
Save downloads the whole story; Discard restores its initial or most recently saved version.
Changing scenes, testing and browsing originals keep your edits without save prompts.
New, Duplicate and Import replace “My story”, with Save / Discard / Cancel for changed work.
Remove the personal library, separate Export action and scene-only Reset.
First unify editing and saving, then add browser recovery and finish the simpler collection.
Check the complete browser journey before release; preserve existing browser-only stories first.
Implementation and focused Editor checks are complete locally; browser acceptance and publication remain pending.

---

## Technical body (executing agent)

**Status: implemented locally; browser acceptance pending. Updated: 2026-09-10.** Planning source baseline `89fdf46`;
fetched `origin/master` is `4dc870d`. The existing local Unity asset/UserSettings
changes are unrelated and must remain untouched. Do not push: `master` deploys.

### Mandate and authority

Arthur's exact design request:

> hmm yeah i'm a bit lost and i want a simple UX. I think allowing only one edited / imported story at once simplifies the ux, so we can either create a story from scratch, duplicate or import one, and that's the only currently editable. if we import or duplicate another we are prompt that it will replace the previous, asked if we want to save the previous (if unsaved changes) or discard. then once a story is loaded for edition, we save one restore point, then every changes are in local/cache memory, then we can either save it for good (exporting the file) or discard the change. I think we should reduce the complexity and have a single save/export, not have both; wdyt ? I'ma bit lost here

After the recommendation below, Arthur said:

> great i love this. please /plan the implementation

The accepted recommendation added these explicit protections: the restore point
advances after Save; all scenes form one draft; Test uses the draft; browsing or
playing originals preserves the workspace; invalid/cancelled imports preserve it;
the browser automatically remembers both draft and restore point. Only New,
Duplicate and Import replace it. The planning request authorized planning only. Arthur subsequently set this document
as the execution goal and instructed “proceed”; implementation is now authorized.
Publication still requires an explicit instruction. [GAME_DESIGN §9](GAME_DESIGN.md#9-saving-and-editing)
owns the product behavior; this document owns the execution sequence and technical decisions.

Arthur resolved the visible multiple-target mismatch during implementation:

> One dependable target area per objective, nothing change from the original DA here

Each objective therefore retains exactly one near/target pair. Remove misleading
extra-target commands; keep the original art direction. A near/target redraw is
one accepted edit: incomplete geometry remains a disposable preview, and leaving
before completing the target preserves the previous pair.

### Minimum journey and non-goals

Open a story → edit any scene → test → keep editing → Save a portable file →
edit further → Discard back to that Save. Play an original in between without
disturbing the draft. Refresh and recover the draft plus the same restore point.

No personal-story library, cloud account, version history, undo stack, separate
Export, backup badge, new file format, new dependency or general document framework.
Preserve speech expression and French word corrections, original story order,
gameplay, available editor tools and current portable-file reading. Do not expand
scene/frame/target capacity, redesign the visual theme, add metadata management,
restore desktop pickers or tune the voice engine in this leg.

### Clean-sheet ownership

Keep the existing root, session, store, catalog and serializer. Change their duties
directly; do not leave the old library beneath a new facade.

| Producer → sole owner | Consumers and lifetime |
|---|---|
| New / Duplicate / valid Import → `StorySession` workspace draft | All editor controls, personal-story play and file Save; survives scene changes and playing originals. |
| Initial open / file Save → workspace restore snapshot | Whole-story Discard only. Deep independent `Story`, including metadata, scenes, sprites and complete speech documents. |
| Editing → workspace dirty state and editor cursor | Toolbar, replacement prompt, cache. Browsing and playback never mark it dirty or move the editor cursor. |
| Original selection / personal play → session active play target and play cursor | Existing `Current`, title, credits, scene and sprite read contract. An original is a separate loaded read-only story; personal play reads the workspace draft. |
| Built-in manifest → `StoryCatalog` | Original cards and original resolution only. The collection projects its one personal card from the workspace, never from catalog id lookup. |
| Workspace changes → `StoryStore` recovery record | One fixed-path record containing draft, restore point, dirty state and editor cursor. No title/id-addressed user files. |
| Store/browser operation → root/UI feedback | Pending operation and actual error/completion; never inferred from a label or `IsLoaded`. |

`StorySession` remains the single model owner: a small workspace record inside it
is data, not a second singleton/session. Keep one active-story sprite cache and
clear it when its owning content changes. UI-created textures/zones are disposable
views; destroy stale views when rebinding. The restore point needs no decoded assets.

`Current` can remain the gameplay read projection. Editor mutation must explicitly
target the workspace. Audit gameplay readers for writes before allowing them to
read its draft; keep progress/cursor in runtime state. Do not clone a second live
preview story or swap the draft out when playing a built-in.

### Observable contract and implementation defaults

| Boundary | Required result |
|---|---|
| Open from New / Duplicate / Import | Prepare the complete candidate first. Install draft and independent initial restore snapshot together only after replacement is accepted. New keeps the existing title/optional-cover form; Duplicate keeps the current copy-title convention. |
| Initial state | Initial content is the restore point and has no edits since that point. Save remains available even when unchanged, so a fresh duplicate or imported file can be downloaded. Dirty means edits since the restore point, not proof of a file on disk. |
| Edit | Update the draft where a value is accepted. Text changes enter it as typed; a completed target pair, image or frame edit updates the corresponding typed data. Programmatic UI refresh and opening/closing unchanged controls must not mark dirty. |
| Voice workbench | Keep its explicit Apply/Cancel audition buffer. Apply writes the full `SpeechDocument` and voice settings into the draft immediately; Cancel changes nothing. This purposeful local audition buffer is not a second story draft. |
| Change scene / Test / return / collection / standalone Voix | No save prompt. Retain draft, restore point and editor cursor. Test uses the selected draft scene, including title preview; returning resumes that editor scene even if play progressed. |
| Save | Serialize an immutable capture of the full draft through `StoryJson`; stamp the outgoing file, not the live model during a read. Dispatch one `.rody.json` download from the user gesture. Once dispatch reports success, make exactly that captured content the restore point and clear dirty. Request a cache write; cache success never substitutes for file Save. |
| Discard changes | Confirm whole-story scope; replace draft with a deep clone of the restore point, clear dirty, rebuild views/cache and clamp the editor cursor to surviving content. Disabled when unchanged. Added/deleted scenes and every sprite are restored too. |
| Replace changed workspace | Name both stories and offer `Enregistrer / Ne pas enregistrer / Annuler`. Save invokes the same file-save operation and then installs the candidate; an observed save error leaves the old draft and prompt intact. Discard installs the candidate; Cancel retains old content and cursor. No separate implicit discard before installation. |
| Invalid/cancelled import | Parse and check the candidate before opening the replacement prompt. Cancellation, reader failure, invalid data and cancellation of replacement leave draft, restore point and dirty state untouched. Do not use “a story is loaded” as import success. |
| Browser recovery | Restore draft and restore point together, including dirty state. Never promote cached edits into a file-save checkpoint. No extra cache Save button or success badge. |

The initial clean state is a planning default derived from “once a story is loaded
for edition, we save one restore point.” It intentionally replaces the current
automatic dirty flag on creation/duplication. It does not suppress prompts after edits.

Keep one short Save operation lock in the existing root, from capture through
download handoff, checkpoint advancement and any accepted replacement. Disable
editing and conflicting Save/Discard/New/Duplicate/Import actions while it runs;
release on either success or reported error. Do not queue those actions. This
prevents Save-and-replace from discarding edits made after its capture, or a late
completion from advancing the checkpoint of another workspace. The lock lasts
only until browser handoff, not until a file reaches disk. Cache writes continue
under their separate single-writer lifecycle and do not hold the editing lock.

The standard browser download only reports a handoff, not a completed disk write.
Use `Téléchargement lancé` and a concise first-use explanation that Save downloads
a file. Do not claim `Fichier enregistré sur votre appareil` or treat the Editor's
current no-op download callback as success. In Save-and-replace, an OS/browser
cancellation after handoff is unobservable; this is a limit of the agreed download
model, not permission to invent a second saving mode or an extra backup history.

### Existing mechanisms and source findings

Inspected source, not runtime acceptance:

- `StorySession.Load` clears the only current story; `StoryCatalog.Resolve` prefers
  a same-id user file. These cause workspace replacement and identity ambiguity;
  a title-derived id alone cannot explain or solve both. Typed personal selection
  and separate workspace ownership remove the need for collision dialogs/UUIDs.
- `RM_GameManager` mirrors scene fields; panels often write them only on Return.
  `RM_SaveLoad` reconstructs a scene from them; images already mutate the session.
  Therefore changing only the Reset button cannot restore a complete draft.
- `RM_SaveLoad` rescales title images to 320×240, synthesizes four copies of the
  base frame and leaves trailing frame keys; normal Save can alter imagery.
- `RM_ImgAnimLayout.frames` and scene zone GameObjects are additional content
  owners. The typed story currently stores one target pair per objective.
- Collection edit/export reload a stored story. New/import write personal files
  and discard storage errors. Gameplay paintbrush, story menu and collection have
  different editor-entry paths; all must reach the same replacement decision.
- `WebFs` holds one replaceable callback per operation. `StoryRoot` can initialize
  lazily, while hydration is invoked separately by `Bootstrap`. Collection startup
  does not wait on an explicit ready result. The exact browser startup ordering is
  not yet observed; do not diagnose storage failure from these source facts alone.
- `RodyWeb.jslib` already supplies file-content input, Blob download and IDBFS sync.
  Import currently conflates cancellation and errors; file-input cancellation and
  operation completion must settle the UI once, including reopening the same file.

**Prior art — searched / found / adopt:** existing `Story.Clone`, `StoryJson`,
`StoryStore`, `WebFs`, `WebGLFileBrowser` and the full `RodyWeb.jslib` were read.
The [Emscripten filesystem API](https://emscripten.org/docs/api_reference/Filesystem-API.html)
documents IDBFS and asynchronous `FS.syncfs(populate, callback)`; reuse that boundary.
The [browser download API](https://developer.mozilla.org/en-US/docs/Web/API/HTMLAnchorElement/download)
documents that `download` does not establish whether a download occurred.
Adopt existing serialization/storage/interop; build only the workspace record and
direct state transitions they currently lack. No package, persistence framework,
File System Access alternative, migration layer or new browser storage backend.

### Ordered implementation legs

#### 1. One editable story through the whole application

Refactor `StorySession` ownership and adapt every consumer in the same coherent
leg. Route New/Duplicate/Import through candidate preparation and one replacement
operation. Reuse existing feedback panels, adding the third choice explicitly;
the root owns the pending candidate/action, not a soon-to-be-destroyed panel.
Resolve built-ins only through the manifest/catalog. Keep the personal slot distinct
from imported ids even when it has the same title/id as an original.

Bind ordinary editor controls directly to typed draft data. Remove the mirrored
content fields, Save-time scene reconstruction and static authoritative frame
list. Preserve the workbench's explicit audition/Apply/Cancel contract. Keep the
existing UI hierarchy/style; use explicit calls for new navigation boundaries.
Route collection, `MenuManager`, `ClickHandler` paintbrush and editor direct-play
initialization through this model. Preserve editor cursor separately from gameplay.

Change Save to the existing download boundary, create/advance the restore point,
and make Discard whole-story. Remove prompts for scene navigation, Test and leaving
the editor. Add/delete existing supported scenes mutate this same draft, with the
existing destructive scene confirmation naming what it removes.

**Content integrity required here:** retain the original 320×200 title contract;
store only the explicitly authored base/animation sequence; remove obsolete frame
keys when the user removes frames, not through Save-time resynthesis. Rebind zone
views from typed data and capture accepted edits directly. No format/capacity
expansion is implied: do not silently cut a visible authoring tool to make a
round-trip pass; report a concrete unsupported-control case before changing scope.

**Done when:** edits to two scenes, voice, picture, zones and a structural change
survive navigation/Test/original play; Save captures all; a later Discard restores
the capture; all entry points protect originals and the personal workspace.

#### 2. Recover exactly that workspace in the browser

Replace per-id persistence with one recovery envelope at a fixed path distinct
from `Stories/`. Reuse the same story serializer settings inside it. Serialize
the complete record before replacing the previous file; use a temporary write
and same-filesystem replace, with no backup archive or second authoritative copy.

Make root initialization start once and return an explicit ready/error result.
The collection waits before presenting/replacing a cached personal slot; originals
may remain available while recovery loads. Failure must not masquerade as an empty
workspace and trigger an overwrite of unread recovery data.

Schedule recovery writes from every draft mutation, restore-point change,
replacement and editor-cursor change, not only the dirty flag's first transition.
Use one short coalescing delay for typing/dragging, request promptly at completed
edits/navigation, and allow only one flush in flight with the latest pending record.
No overlapping `FS.syncfs` calls or overwritten callbacks. A later completion must
not replace a newer workspace or clear newer unsaved edits. Do not rely on a final
page-close async flush; already completed background writes provide recovery.

Cache errors leave current in-memory work available and Save usable, with one
actionable recovery-error message and retry. After choosing to work through a
storage outage, the existing Maker status area keeps a failure-only retry action
reachable; Save feedback must not promise available recovery during the outage. Do not clear dirty or reset the restore
point. A failed hydration/read retains existing on-disk data and blocks cache writes
until recovery succeeds or the user explicitly chooses to replace unrecoverable data.
There is no promise of recovering an edit whose write was interrupted by refresh,
browser crash, storage denial or clearing site data.

**Done when:** browser reload after completed writes restores changed draft plus
its older checkpoint; Discard after reload returns to that checkpoint; blocked
storage reports failure without destroying the open draft or disabling download.

#### 3. Finish the collection and remove obsolete machinery

Present the seven manifest-ordered originals and one `Mon histoire` slot (empty
state offers New/Import). Personal card reads title/cover directly from the draft;
selecting Play/Éditer never reloads a stale file. Keep New/Import available without
first opening an original. Dupliquer belongs to originals; Éditer to the workspace.
Rebind the former personal Export affordance to the same Save action; no second
operation or indicator. Save/Discard remain visible in Maker, with whole-story
labels, dirty text beside the personal story identity and concise help.

Delete per-user membership/sort/resolve/read/write/delete APIs after tracing all
consumers. Remove personal-story deletion shortcuts, obsolete events/listeners,
serialized fields/references and Save/Export/Reset copy from scenes, prefabs and
code. Preserve scene deletion. Delete dead `RM_SaveLoad` conversion/save wrappers
and warning modes once their final consumers move; keep any unrelated live helper
until its consumer is directly moved. No forwarding compatibility shim.

Update architecture and audit to actual implemented behavior, retire resolved
findings, and update player/tutorial text only now. Game design remains product
SSOT; roadmap marks implementation separately from browser acceptance. This plan
becomes historical at completion rather than a second permanent architecture.

**Done when:** the collection has exactly one editable slot; no library management
or separate Export remains in reachable UI; current controls/help describe the same
Save/Discard behavior. Inspect the actual serialized UI and references, including
prefab variants, and run an independent review for those reference changes.

#### 4. Browser release acceptance and cutover

Use the narrow acceptance journeys below in a release candidate. Build/compile
verification remains opt-in under the Unity skill; do not invoke batchmode as a
routine per-leg gate. At the release-candidate stage, obtain the needed browser
artifact through the agreed release workflow and perform browser checks there.
Editor-only success cannot establish download, IndexedDB or native picker behavior.

The fetched predecessor already contains the multi-story store. Existing users
may have personal stories only in browser storage; source inspection cannot tell
which browser/profile has them. Before public cutover, establish whether any need
preserving and export them from the preceding version while it is still available.
Do not delete old `Stories/` data or auto-select one and strand the rest. Do not
build a permanent legacy library/migration UI. If people need an in-app recovery
route beyond this preparation, that is a specific release decision before removal
is published, not an unapproved compatibility layer in this implementation.

Run independent release review, record the browser/version and actual results,
then request publishing only when the candidate is concrete and reviewable. No
push or deployment is authorized by this planning request.

### Narrow acceptance evidence

| Journey | Observable pass |
|---|---|
| Original → Duplicate → edit → play original → return | Original content intact; same personal draft and editor scene return. Repeat editor entry via collection, story menu and gameplay paintbrush. |
| Edit across two scenes | Text while focused, French correction/expression, voice, image, frame removal and target movement all survive navigation; no save prompts. |
| Test and return | Latest content is played without Save; gameplay advancement does not move the remembered editor cursor or mutate authored data. |
| Save → edit → Discard | Downloaded full-story file includes all accepted changes; Discard restores that version, including sprites and added/deleted scenes. Repeat before the first Save to exercise initial restore. |
| New / Duplicate / Import replacement | Exercise Save / Discard / Cancel. Invalid JSON, unsupported/malformed story content, picker cancellation/read error and observed download error leave the old workspace intact. Same-title/id import cannot replace an original. Delay download handoff: edits and conflicting actions stay blocked until success/error, then unlock; no wrong-workspace checkpoint or lost newer edit. |
| Browser refresh | After background persistence completes, reload restores the dirty draft; Discard then restores the older snapshot. Repeat after Save and replacement. Test slow/failed writes and hydration failure without empty-record overwrite. |
| Reimport actual downloaded file | Title image dimensions, scenes, exact frame sequence, targets and full French/phonetic speech documents match the export capture; unedited legacy portable stories still read. |
| Reachable UI | Original order/bonuses/Voix remain; exactly one personal slot; correct Save/Discard help; no reachable old Export, library deletion or scene/Test/exit save warnings. |

Per-leg checks are targeted consumer/reference searches and artifact inspection,
with focused runtime observation where it governs the claim. No new unit-test
framework, broad suite or test-only seams. Runtime/browser rows above remain
pending until actually exercised; a grep or Editor callback is not their substitute.

### Deletions and scope accounting

**Explicitly requested/accepted meaning removed:** multiple personal stories in
the app; separate local Save and file Export; scene-only Reset; save prompts on
navigation/Test/ordinary exits; playing an original replacing personal work;
separate local-save/export/backup indicators.

**Required consequences, not extra features:** separate personal/editor lifetime
from active play; initial and advancing deep restore snapshot; transactional import
and replacement; direct draft binding; correct full-story image/frame persistence;
single ordered browser recovery write; unified entry points and feedback; removal
of personal-library deletion/sorting/id-precedence rules and their serialized UI.
The fixed workspace key makes new collision-id machinery unnecessary.

**Parked scope:** scene-count policy and expanded capacity; animation/target
capacity changes; title/cover/credits management UI; desktop support; browser
multi-tab conflict handling; legacy recovery UI; audio/speech-engine improvements;
visual redesign and publication. Do not treat a parked scope as permission to
delete existing content or a visible capability. Raise a bounded product decision
if a demonstrated case cannot preserve it within the agreed one-workspace model.

**Planning validation (historical):** source/consumer and serialized evidence from the docs
audit, platform API prior-art check, documentation link/diff inspection and cold
plan review. The cold reviewer identified a Save-and-replace race if editing stays
enabled during a delayed handoff; the operation lock and delayed-handoff acceptance
row above resolve the plan gap. No other material findings were returned. No C# or
Unity assets changed; no build, runtime test or publication.

### Implementation review findings — 2026-09-10

A fresh source-only review found two material defects in the first implementation:

- Continue after failed hydration left no later recovery retry and Save feedback
  still promised automatic recovery. A failure-only retry action in the existing
  Maker status area and error-aware Save copy address those boundaries.
- Committing only the redrawn near region could persist a larger old target outside
  it when the author left midway. The disposable zone views now preview a pair;
  only completion of the target commits both rectangles together.

Failure/retry and interrupted pointer-drawing journeys still need browser evidence.
A separate independent serialized review subsequently checked the assembled UI,
including final preview and spacing corrections, without material findings.
No build or publication has run. Current evidence is in [the audit](unify/AUDIT.md).

### Integration findings and scope accounting

Live Editor inspection found original images at both 1× and 2× resolution. The old
SpriteRenderer mapping treated each encoded pixel as one logical pixel, enlarging
the 2× images. `SpriteCache` now derives pixels-per-unit from the declared logical
width; encoded bytes are unchanged. Maker's preview now uses the existing Canvas's
native `Image` with aspect preservation, reserving the status strip outside the
picture on the main screen and restoring the full 320×130 area in detailed editors.
This replaces the old preview SpriteRenderer, not the artwork. The collection's
existing layout group has wider action spacing so the longer Enregistrer label fits.
Unity's [Sprite.Create contract](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Sprite.Create.html)
provides the pixel/world mapping; no image conversion on Save or rendering shim.

These are integrity/layout corrections required by the visible result, beyond the
literal one-workspace wording. Existing original images, editor capacity, voice
content and the visual theme remain. Removed extra meaning: the old debug “glitch”
content that was used as a new-scene template; new scenes start as ordinary blanks.
