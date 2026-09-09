# Development Log

Dated historical evidence, not current instructions. Start with [the roadmap](docs/ROADMAP.md)
and [game design](docs/GAME_DESIGN.md); old APIs, conclusions and screenshots can be superseded.

## 2026-09-09: Documentation audit, product design and AX consolidation

Arthur requested a full documentation audit, clearer UX recommendations, one
comprehensive code-free design/specification, and docs that help future agents
understand the project and resume efficiently.

- Created `docs/GAME_DESIGN.md` as the product owner: Collection, story loop, Maker,
  voice, content limits and explicitly unapproved next UX. A cold review found
  scene-scoped drafts left structural changes ambiguous; the revised recommendation
  is one whole-story draft, Save, story-wide restore, safe exit and draft Test.
  This is not approval to change the older scene-only Reset behavior.
- Replaced pre-June-migration architecture and roadmap assertions with source-backed
  current ownership and a focused audit. Browser persistence exists in source;
  startup/error paths, Reset, dirty state, paintbrush fork protection, identity
  collisions, invalid imports, multiple targets and image/frame saving remain issues.
  My initial session statement that Save still downloaded was itself based on stale
  docs; direct save/store/interop reads corrected it. Doc labels are not a status oracle.
- `CLAUDE.md` now routes a fresh agent by task instead of duplicating version,
  schema and API inventories. README links the product design. Current Unity version,
  scene order and package/deploy facts point to their actual config owners.
- Rewrote French player/creator procedures; inspected all nine historical tutorial
  PNGs and labelled them as historical. Removed misleading folder-save and old
  keyboard screenshots from the active instruction flow, retaining the image files.
- Consolidated speech documents while retaining native-machine validation evidence,
  earlier false-parity findings, listening limits and exact accepted speech direction.
- Consolidated DOOM's optional design backlog, corrected configured-value claims,
  and replaced unsafe migration diagnoses with the existing toolkit's real limits.
- Itch documents remain local publication drafts. Public itch page text was checked;
  no external page was edited. The web reader could not open the GitHub Pages URL,
  so no live-game or current deployment claim follows from this pass.

### Retired meaning and where it went

1. `docs/unify/MIGRATION.md`: completed provider/shim/platform migration steps,
   per-file build recipes, speculative replacement types and implementation estimates.
   Current architecture and source gaps replace them; June decision provenance remains.
2. `docs/SAVE_AWARENESS_PLAN.md`: superseded download-only saving, alternate choice
   dialog and proposed tooltip implementation scaffolding. Clear saving, backup
   awareness and honest warnings survive in the design; current gaps in the audit.
3. `docs/PHONEME-TTS.md`: obsolete clip-based audit, old dirty-token/corpus counts,
   option/effort matrix and hybrid-dictionary proposal. Current speech reference
   replaces them; historical originals remain in Git.
4. `docs/FRENCH_SPEECH_HANDOFF.md`: completed handoff and paused-agent coordination.
   Unique accepted direction and still-relevant constraints moved to speech reference.
5. `docs/DOOMASTICO_GAMEPLAY_AUDIT.md`: separate duplicate inventory and untested
   numeric prescriptions, snippets, phased priorities and effort promises. Every
   retained player-outcome idea is in DOOM_FPS's explicitly unapproved backlog.
6. Root `plan.md` and `plan-rodyCollection.prompt.md`: abandoned floating-slot layout,
   obsolete WorkingStory snippets and duplicated schedule. Shared actions already
   exist; title/cover management and discoverable deletion remain parked outcomes.
7. Migration guide: categorical Library-corruption / serialization-loss / unknown-
   component deletion rules; a broken lambda-unsubscribe example; blanket conversion
   to static events; copied package-GUID and command inventories. Replaced by traced
   dependency recovery and links to existing command help.
8. Repeated root architecture/API/schema/version/submodule tables and DEVLOG's
   undated current-state/preferences footer. Preserve dated history and project code
   preferences; actual config owns current facts. UnityReusables and plugin files
   are tracked, so submodule/local-only setup instructions were stale.
9. Player/tutorial copy: unsupported universal platform/control/fullscreen promises,
   wrong objective acronym expansions, duplicated sharing instructions, old folder-
   save procedures, phoneme keyboard/catalog and scene/object-count advertising,
   automatic community-inclusion wording, and an unrelated font-editing walkthrough.
   Keep downloadable font/palette, community invitation, credits and Arthur's original
   acknowledgement/apology. Free/non-commercial intent remains; unsupported categorical
   legal conclusions about what may be sold were removed from marketing copy.

Validation: current code and serialized consumers, catalog plus all seven embedded
story envelopes, historical evidence, local Markdown links/anchors and diff formatting.
No source code or Unity asset changes, compilation, tests, player build, runtime
playthrough or publication. The design/roadmap received independent cold review;
remaining crosslink and wording fixes received a fresh-pass self-review.

---

## 2026-09-07: Accepted expression and French-entry handoff

Arthur accepted full expression as the best version after the original Atari
recording and explicitly released the old C# structures as a compatibility
constraint. Captured the French-entry session's user decisions and integration
boundary in [the speech direction](docs/SPEECH_ENGINE.md#accepted-direction); full speech details remain owned by
docs/SPEECH_ENGINE.md. The paused agent's converter files remain untouched. Cold handoff review
clarified approval provenance and that the current string storage is replaceable.

Refreshed pending Unity imports to exercise the actual new workbench. Original
insertion retained exact notation and played 51,870 samples at 13kHz. Invalid
volume blocked Play/Apply; selected playback retained inherited gain. Inspected
the composited 960×600 workbench; expression help and template labels fit without
control overlap. Stopped Play Mode and restored the clean collection scene.
No forced compilation command, player build, browser integration or push.

---

## 2026-09-07: Lossless editable speech expression

Accepted next step: preserve original duration/emphasis/pauses in editable
speech, then compare recordings. Extended the existing single dialogue-string
grammar with strict [envelope,amplitude,rate] fields and all 64 descriptor names.
No JSON schema migration or parallel native playback. Three opening templates
are formatted directly from PA.ROD into the workbench's existing picker.
The CLI exports original audio plus editable text through the same parser.

All 65,536 token values round-trip; all 101 originals retain exact native commands
and 6,615,080 samples through the editable path. 850 authored/notation checks pass.
Review exposed context loss when previewing a selected substring: passage playback
now traces native token/command/sample boundaries and slices the full-context
PCM. Partition and inherited-gain fixtures cover that behavior; cold follow-up
review found no blocking defect. No Unity compile/build or new UI runtime claim;
the inspected Editor still had the previous script version loaded.

Removed the unwritten final word pause (151 samples, ~11.6ms) so original records
can be represented exactly. Existing phoneme/space/pause defaults otherwise stay
unchanged, including bare on. Removed the authoring validator's duplicate sound
inventory; it now uses the game parser and requires .NET 9. Added the missing Codex
project skill link; the repository remains the only instruction source.
No original bank, story text, recorded effect or character setting was deleted.

Matched opening sentence: simplified 3.036s versus full-expression 3.990s, at the
same pitch. Native early waveform windows correlate 0.885/0.913 with the Atari
recording at about 1.04 duration scale; whole-phrase correlation remains 0.421.
Prepared equal-level Atari/simplified/full listening comparison and editable
text under ~/Downloads/Rody-full-expression. Transcription found the same sentence
three times; that is a content check, not listening acceptance. Details and
limits are owned by docs/SPEECH_ENGINE.md.

---

## 2026-09-07: Independent original-machine speech audit

Arthur questioned the completeness of the reverse engineering after rejecting
the preview as rushed/chopped. Re-extracted AAA.PRG and PA.ROD directly from the
original disk and ran relocated original 68000 instructions in Hatari. Found and
fixed shared Python/C# errors: truncated and mistranscribed type-4 handler,
P22 context rules, missing table prefix, integer amplitude rounding, empty
interval first-sample behavior, end-command handling and speed-byte wrapping.
The original bank has 101 records, not 102 (the extra fixture was invalid/empty).

C# now matches 1,429 original CPU command streams: 101 corpus, 304 control-field
batches, 1,024 batches covering 262,144 descriptor triples. All 1,280 native
amplitude samples match. Python/C# parity additionally passes 101 records /
6,615,080 samples and 841 authored cases. The full original game separately
showed the assumed speed/mode/end-marker for three opening calls.

Replaced incomplete disassembly with a complete original-byte listing for the
speech code ranges; switched static lookup ownership from a RAM snapshot to the
original executable. Removed the fictitious 102nd record and silent skipping of
invalid command/variant input. No story text, phoneme vocabulary, original bank,
recording or character setting was removed. No new story format was introduced. Removed unsupported fidelity/prosody
guarantees from active docs; historical STT results remain labeled as history.

Added a repeatable native oracle using Hatari's existing debugger and the shared
C# command-line tool. No Unity build or browser check. Source-only checks for
empty-interval/end/wrap behavior are distinguished from native executed probes.
The listening complaint is not declared fixed: authored prosody loss and the
approximate sample clock / omitted PSG output curve remain documented limits.
Cold independent review found no blocking defect and separately compared the
304 field / 1,024 context captures and all amplitude samples.
See docs/SPEECH_ENGINE.md for evidence, exact scope and reproduction.

---

## 2026-09-07: Listening comparison preparation

Prepared local listening files from the preserved Atari capture and current
authored story notation. Whisper checked the reference excerpt at 121 seconds
in `r1_full.wav`: it ends after "elle ne te trouvera pas", so the current preview
omits the following sentence for a matched-text comparison. Reference duration
is 13.607 seconds including surrounding pauses; current preview is 7.490 seconds.
This is a pacing concern for listening, not proof of its cause. Native-record
PCM parity does not establish authored-notation timing equivalence.

The offline renderer crashed on the original intro: `i * rate` overflowed as an
integer before conversion to double. Promote before multiplication. The same
render command and two Ibiza previews now complete; original preview transcript
contains the intended passage (with STT substitutions). No Unity build or tests.
Listening pack is local under `~/Downloads/Rody-listening`; no audio acceptance
or browser validation claimed.

---

## 2026-09-07: Dialogue voice workbench

Replaced the phoneme keyboard with a sentence workbench shared by the collection
Voix entry and story dialogue editors. Includes full/passage playback, native
sound examples, insertion, pitch, clipboard and explicit apply/cancel. Removed
five obsolete button forwarding scripts and their unused prefab. Uses existing
Alata font for readable French accents. Extended shared engine/authoring whitespace
handling to multiline notation; 841 authored checks now pass.

Live runtime review caught and fixed deferred input focus selecting all text,
Stop becoming unavailable during invalid edits, clipboard request races and scene
teardown touching a destroyed external AudioSource. Teardown now releases only
the speech controller's owned coroutine/clip. Checked actual menu navigation,
insert/paste then typing, apply/cancel, and full-frame UI captures. Independent
source/serialized-reference review completed. No player build, browser clipboard
or human listening sign-off; release checks remain in docs/SPEECH_ENGINE.md.

---

## 2026-09-06: Original speech engine port

Replaced recorded-phoneme concatenation with the extracted engine's C# port.
Existing notation, pauses, character pitch and three actual sound effects remain;
recorded speech is archived outside Assets for comparison. Fixed the missing
third-dialogue speaking flag. Offline authoring previews now execute the same
C# source. Detailed data ownership, mapping evidence, removals and reproduction
commands are in docs/SPEECH_ENGINE.md.

Validation: all 102 reference records match command/PCM bytes (6,611,070 samples);
840 authored cases render without unknown tokens, with specified PCM/effect checks.
Unity imported the data and executed the new engine. Independent source/reference
review found no blocking regression. No player build, WebGL check or new human
listening sign-off; those remain explicit limits.

---

## 2026-07-22: Original 1988 Speech Engine — Phase 2 (Authenticity) Complete

**Changes**:
- Preprocessor (`tools/original-extraction/preprocess.py`) made BIT-EXACT vs emulator
  ground truth: 472/472 commands (records 0,1,3). Fixes: consonant onset tail
  (b4514-b45b0) with voicing table 0x4b2a and 0x11/0x12->0x10 clamp; vowel dispatch
  fall-throughs (a 5,6 -> b4298; a>7 -> b42da).
- Bank-4 diphone matrix solved and rendered: clip index 0x45c/4 + P + 14*X, 151 real
  transition grains (the missing coarticulation).
- 0x61 decoded = 5-level AMPLITUDE envelope (x0.5..x1.5), not frequency. 0x66 = speed
  via inter-sample delay (period ~372+10*delay cycles), rendered as relative resampling.
- Silences cycle-counted: word gap 11.6ms, sentence pause 323ms (was 3x too short —
  explained the entire 13% duration gap vs original).
- Rody1 AAA.PRG extracted from rody1.st (FAT12); engine disassembled offline with capstone.
- Original speech located in existing capture (r1_full.wav @121s) + fresh Hatari capture
  of scene 2 (3 reps). All irreplaceable /tmp assets rescued into `captures/`.

**Validation**:
- Arthur blind QA (5 sentences): ~96% word accuracy by ear vs whisper 0.57 mean.
- Instant-switch A/B player vs real captures: intro "sounds perfect", scene 2 "perfection".
- Whisper transcribes our render == original, word for word, on scene 2.

**What worked**:
- Command-stream diffing (gtdiff.py) vs live-captured GT made every preprocessor bug
  visible as a clean insert/replace pattern; disasm confirmed each fix.
- Whisper as cut-verifier and duration matching as speed-calibration (no waveform xcorr needed).

**Mistakes (do not repeat)**:
- Shipped 3 music cuts to Arthur's ears without whisper-verifying them first (tool was
  already in session). Rule now in project memory: verify artifacts with the direct tool.
- Nearly fetched a YouTube longplay while the clean capture sat on disk, because a proxy
  statistic wrongly said "no speech exists". Verify absence with the mechanism, not a heuristic.
- STT mean is a LOWER BOUND on intelligibility: adding real diphones dropped whisper 0.57->0.47
  while humans heard clear improvement. Never optimize the render against whisper.

**Next**:
- Corpus alignment: 94 known texts <-> bit-exact command streams => label every grain +
  diphone cell => French-phoneme->grain table => speak NEW sentences in the authentic voice.

## 2026-03-12: Phoneme Conversion Investigation

**Changes**:
- Studied the runtime phoneme parser and token inventory in `SoundManager` / `P.cs`.
- Audited the bundled story JSONs as a text-to-phoneme corpus instead of treating them as opaque content.
- Logged the conclusion that assisted conversion is the right first implementation, not naive full automation.

**What worked**:
- The shipped stories already provide hundreds of aligned French text and phoneme examples, which is enough to bootstrap a dictionary and rule set.
- The current runtime contract is simple and stable: underscore-separated tokens, spaces between words, automatic pause per word.
- The roadmap already points in the right direction with a phoneme dictionary and learn feature.

**Hindsight**:
- The system is a custom retro spelling language, not generic French phonetics. Treating it like IPA would be wrong from the start.
- The corpus has dirty tokens (`!`, `.p`, `M`, `ca`, `il`, `w`) that the runtime does not recognize cleanly, so normalization has to happen before any converter work.
- The first useful version should generate suggestions, expose uncertainty, and let authors correct them in-editor. Pretending it can be perfect on day one would be bullshit.

## 2026-03-12: RM_Main Tooltip Prototype Stabilized

**Changes**:
- Added a reusable hover tooltip component for editor UI controls.
- Replaced the bad save-status-panel tooltip hack with a dedicated tooltip panel authored in `6_RM_Main` and wired into `RM_MainLayout`.
- Verified the pattern on two buttons first: save and intro.

**What worked**:
- A dedicated tooltip panel avoids hover flicker and keeps the user visually anchored on the hovered button.
- Auto-sizing from text plus padding produces a cleaner retro UI fit than a fixed banner.
- Edge handling matters immediately: the tooltip needs horizontal clamping and vertical flipping to stay inside the canvas.

**Hindsight**:
- Prototype one tooltip, then two, before touching the rest of a complex scene. That catches architectural mistakes early.
- Inspector-wired scene references are better than runtime-generated tooltip UI for a Unity editor screen like this; the object is visible, styleable, and debuggable in-scene.
- Animation polish should be treated as a later pass. Behavior stability matters first.

## 2026-03-12: 2_Menu Fixed Grid Placeholder Rules

**Changes**:
- Documented the 16-slot preview grid in `2_Menu` as an intentional Atari-style limitation, not a pagination bug.
- Recorded the safe runtime rule for short stories: unused slots keep placeholder art, tint their inner image dark grey, and are non-interactable.

**Root cause**:
- `MenuManager` originally only replaced preview art when a scene thumbnail existed.
- For stories with fewer than 16 scenes, unused slots could keep stale or misleading visuals and still behave like selectable scenes unless disabled explicitly.

**Hindsight**:
- Fixed-layout nostalgia UIs still need explicit empty-state rules.
- Placeholder behavior should be designed, not left as whatever the serialized scene happened to contain.
- Future refactor should move selection ownership out of `Clickable.Update()` polling and into explicit menu-state events.

## 2026-03-12: Final Scene Intro Skip Bug

**Changes**:
- Fixed `ClickHandler.NextClick()` so pressing `Next` during intro on the final scene no longer jumps straight to credits when that scene still has an object phase.
- Added a safe fallback: only skip directly to credits from intro if the current scene has no primary `obj` target phase at all.

**Root cause**:
- `ClickHandler.NextClick()` treated `CurrentSceneIndex + 1 > SceneCount` as proof that the current scene had no gameplay left.
- That assumption is wrong: the object-search phase belongs to the current scene, including the last one.
- This was easy to misdiagnose because `2_Menu.unity` also serializes `MenuManager.sceneToLoad`; manually selecting scene 2 in-editor made the bug look like malformed UGC was skipping object gameplay.

**Observed failure mode**:
- Start directly on the final scene of a 2-scene story.
- Click `Next` in intro.
- Log shows `next clicked in intro`, then credits load immediately.
- Object handlers (`Founded`, `Near`, `Miss`) never become relevant because gameplay never enters the object phase.

**Hindsight**:
- Do not decide credits routing from scene count inside intro UI handlers.
- Let scene progression own the transition: last-scene object completion can still advance to credits naturally on the next gameplay transition.
- When testing from `2_Menu`, treat serialized scene-selection state as part of the repro. The scene asset can preserve a non-default starting scene across runs.

---

## 2026-01-09: UI/UX Enhancement Planning

**Changes**: Created implementation plan for menu story slot floating action buttons

**Historical plan summary** (`git show 2d7555e:plan.md`, retired):
- Fork button on official stories (top-right)
- Edit + Export buttons on user stories
- Buttons show only on selected slot
- Reuse create popup for edit mode (with delete option)
- API readiness: 95% (WorkingStory already has ForkForEditing, SetTitle, etc.)

**Key Pattern**: Use serialized fields (`RA_SlotItem` component) instead of `Transform.Find()` for button references

**Files to create/modify**:
- `RA_SlotItem.cs` (NEW) - Component with serialized button refs
- `Slot.prefab` - Add ActionButtons container
- `RA_ScrollView.cs` - Wire buttons, selection visibility
- `RA_NewGame.cs` - Edit mode, delete functionality

**Estimated effort**: 7-11 hours

---

## 2026-01-09: WebGL Image Upload Fixes (Round 2)

**Changes**:
- `StandaloneFileBrowser.jslib`: Fixed 7 `removeChild` errors (safe DOM cleanup via `parentNode`), changed `onmouseup` → `onmousedown` (4 locations) to fix double-click + ghost dialog issues
- `RM_ImagesLayout.cs`: Fixed PPU from 100 to 1f for WebGL sprite creation, added sprite persistence to WorkingStory
- `RA_NewGame.cs`: Fixed PPU, added replace confirmation dialog in `OnImportClick()`
- `RA_ScrollView.cs`: Added `slotExportPrefab` field for distinct Import/Export buttons
- Created `Assets/Prefabs/Slot Export.prefab` with "Exporter" label
- Updated `Slot LoadGame.prefab` label to "Importer"

**Learnings**:
- **Sprite keys are NOT interchangeable**: `cover.png` = menu thumbnail (set in story creation popup), `0.png` = title scene image (set in RodyMaker scene 0 editing). Both stored in JSON but saved at different times.
- **WebGL jslib timing**: `document.onmouseup` fires AFTER the handler is registered, causing double-click requirement. `document.onmousedown` triggers immediately on user gesture.
- **DOM cleanup in jslib**: Always check `element.parentNode` before `removeChild()` - elements can become detached but still found by `getElementById`.
- **Unity Sprite PPU**: PPU=100 (Unity default) makes 320x130 sprites display as 3.2x1.3 units. Use PPU=1f for pixel-perfect display.

**Hindsight**:
- When debugging WebGL file dialogs, check BOTH the jslib event handlers AND the C# callback flow
- Sprite key naming convention: `cover.png` (menu), `0.png` (title), `{scene}.{frame}.png` (gameplay)
- Import/Export buttons need separate prefabs to show distinct labels - same prefab = identical appearance

**Context**: `Assets/Scripts/RodyMaker/RM_ImagesLayout.cs`, `Assets/Scripts/RodyAnthology/RA_NewGame.cs`, `Assets/Scripts/RodyAnthology/RA_ScrollView.cs`, `Assets/StandaloneFileBrowser/Plugins/StandaloneFileBrowser.jslib`

---

## 2026-01-08: DOOM FPS Enemy Navigation System

**Changes**: Research/documentation only - no code changes

**Learnings**:
- Enemy movement: `EnemyMobile.cs` (AI state machine) + `EnemyController.cs` (NavMeshAgent wrapper)
- AI states: Patrol → Follow → Attack, transitions in `UpdateAIStateTransitions()`
- Speed configured via `NavigationModule`, applied in `EnemyController.Start():149-155`

**Hindsight**:
- Unity 6 deprecated built-in Navigation window - use **AI Navigation package** instead
- NavMeshSurface component replaces global bake settings
- To mark floors walkable: Layer → NavMeshSurface "Include Layers" → Bake

---

## 2026-01-02: URP Migration Complete

**Changes**: Migrated from Built-in Render Pipeline to URP.

**Key files**: `URPAsset.asset`, `DefaultVolumeProfile.asset`, `RA_Menu.cs`, `CameraController.cs`, `RollGameManager.cs`

**Deleted**: `Assets/Pixelation/` (legacy shaders), `Assets/URPDefaultResources/`

**Hindsight**:
- **URP upscaling filter must be Point (2)** for retro pixel effects - default Auto blurs pixels
- **QualitySettings overrides GraphicsSettings** - both must reference same URP Asset
- **Post-Processing Stack V2 → URP Volume API**: `PostProcessVolume` → `Volume`, `ColorGrading` → `ColorAdjustments`
- **Render scale trick** for pixelation: animate `URPAsset.renderScale` 0.1→1.0

---

## 2025-12-30: Reference Scanner Tool

Created `Tools > Reference Scanner` - EditorWindow for finding ScriptableObject references and unused assets.

**Features**: GUID-based project search, Build Report analysis for unused assets, bulk deletion.

**Hindsight**: BuildReport requires temp copy to Assets folder to load (API quirk).

---

## 2025-12-28: JSON-Only Migration (Complete)

**Goal**: Unified all story storage to `.rody.json`. Removed Firebase, folder-based loading, and platform-specific code.

### What Changed
- **Firebase removed** - Static JSON in Resources folder instead of HTTP requests
- **WorkingStory.cs** - Single in-memory story state, all runtime ops go through it
- **LocalStoryProvider deleted** - Both desktop and WebGL use `ResourcesStoryProvider`
- **Folder structure eliminated** - Stories are self-contained JSON with base64 sprites
- **WebGL file picker** - jslib `UploadFileContent()` + SendMessage callback pattern

### WorkingStory API (key methods)
```csharp
WorkingStory.LoadOfficial(storyId)     // From Resources
WorkingStory.LoadFromJson(json, path)  // Import user story
WorkingStory.ForkForEditing()          // Copy official for editing
WorkingStory.ExportToJson()            // Get JSON string
```

### Major Deletions
- `FirebaseStoryProvider.cs`, `LocalStoryProvider.cs`, `UserStoryProvider.cs`, `StoryImporter.cs`
- `StreamingAssets/` story folders (~1,200 files)
- Most of `PathManager.cs` (228 → 29 lines)

### Hindsight
- **`#if UNITY_WEBGL && !UNITY_EDITOR` is problematic** - evaluates FALSE in Editor regardless of build target. Use runtime detection instead.
- **Fix for simplicity, never add complexity** - When export returned 0 stories, the fix was "stories moved to `./original-stories/`" not "add backward compatibility parser"
- **Always grep ALL files** when fixing a pattern - we missed `MenuManager.cs` initially

---

## 2025-12-28: Intro Text Format Refactor

**Problem**: Intro text used embedded quotes format (`"Dialog1" "Dialog2"`) which broke on parse.

**Solution**: Separate `intro1`, `intro2`, `intro3` fields matching PhonemeDialogues pattern.

**Action**: Run `Tools > Rody > Export All Stories Now` to regenerate JSON.

---

## 2025-12-27: Editor UX Fixes

**Git workflow**: Don't push to `master` on every commit - CI builds WebGL on each push.

**Editor buttons**: Save (flash feedback), Revert (reload from disk), Test (warns unsaved), Thumbnail click (navigate or delete ≥18 scenes).

**Bugs fixed**: New scene not appearing (scenesCount timing), JSON new scene creation, object zone format.

**Key pattern**: Cache static data (blank sprite base64) instead of recreating identical textures.

---

## 2024-12-19: Firebase Trial & Removal

**Tried**: Firebase Storage + Firestore for WebGL story loading (avoid embedding 14MB in build).

**Issues**: CORS complexity, async callbacks everywhere, billing account closure (412 errors).

**Decision**: Removed Firebase entirely. Static JSON in Resources folder is simpler - official stories are read-only anyway, no need for cloud storage. Trade-off: +14MB build size, but zero runtime dependencies.

---

## 2024-12-16: Project Resurrection

**Migration**: Unity 2019 → 2022.3 LTS → Unity 6

**Cross-platform fix**: Hardcoded `\\` paths → `Path.Combine()` throughout.

**Refactoring**: Created `PathManager`, `SceneData` (typed model), `IStoryProvider` abstraction.

**Git cleanup**: Removed paid plugins from history using `git-filter-repo`.

---
