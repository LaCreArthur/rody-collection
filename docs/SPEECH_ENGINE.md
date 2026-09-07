# Speech engine

The game and phoneme previews use `RodySpeechEngine`, a C# port of the extracted
Atari ST preprocessor and PCM interpreter. Story JSON stores French source, word-owned lossless scores and voice settings.
Old phoneme strings are read at the import boundary; there is no per-story voice switch.
Music and ordinary game feedback effects keep their existing playback.

## Original-machine audit and listening rejection (2026-09-07)

Arthur rejected the authored preview as "way more rushed and chopped" and asked
for a fresh reverse-engineering audit. The earlier Python/C# agreement reproduced
shared mistakes. It was not independent evidence of Atari fidelity.

Re-extracting AAA.PRG and PA.ROD directly through the original disk's FAT12
chains produced exactly the archived files. Running the original 68000 code in
Hatari then exposed these defects, now corrected in both ports:

- The archived disassembly stopped inside the type-4 consonant handler and
  mistranscribed a variant byte as `0xaa` instead of `3`. Restore its complete
  attack/body/release and following-vowel path, including its missing table prefix.
- Low-consonant P22 used the wrong look-behind rule for three preceding vowels
  and failed to terminate its onset path in other contexts.
- Floating-point amplitude scaling rounded differently from the original signed
  arithmetic shifts. All five levels now reproduce native integer arithmetic.
- The interpreter skipped empty intervals, whereas the original loop reads one
  sample before testing its end pointer. It also ignored the end command and
  omitted byte wrapping in speed arithmetic. Invalid command/variant input now
  fails explicitly instead of being silently discarded; this is a diagnostic
  contract, not emulation of the original invalid-input/copy-protection branch.
- The bank contains **101** records. Header entry 101 is the endpoint; the old
  102nd fixture was an empty artifact of an invalid range.

The corrected C# preprocessor matches **1,429 command streams executed by the
original 68000 instructions**: all 101 source records, 304 batches varying all
16 duration / 8 amplitude / 8 rate fields over the 37 corpus descriptors, and
1,024 batches covering every one of the 262,144 descriptor triples with default
control fields. The C# interpreter also matches 1,280 sample values captured
from the original amplitude instructions (256 bytes × 5 levels). These are
finite checks at base speed -1 and mode 0, not proof for every possible input.
Source inspection additionally covers the empty-interval loop, speed-byte wrap,
and end dispatch; these are not presented as full interpreter execution tests.
A separate unmodified game boot observed base speed -1, mode 0 and a cleared
termination marker for opening calls 1, 2 and 4, including their end dispatch.

This rules out disagreement between the two ports as the only explanation:
the old Python preprocessor itself disagreed with original CPU output on records
25 and 51 and on synthetic contexts. Disk extraction independently checks the
assumption that the archived source files were authentic. The full regenerated
listing replaces the incomplete one; original binaries are preserved unchanged.

**Remaining fidelity limits:** 13,000 Hz PCM and the speed-to-sample conversion
are calibrated approximations. Original playback transforms each sample through
three YM2149 volume tables, writes the channels sequentially, and incurs CPU
and segment overhead. The port does not model that hardware output or exact
clock. Base speeds >=5 also change preprocessing duration; they are outside the
fixed-speed runtime contract and are not supported by this audit.

The existing bare remake notation still lacks original per-phoneme controls,
repeated-descriptor envelopes and some pauses. Its `on` default (`0x420d`)
selects an attack fragment from such an envelope; a complete standalone `on`
commonly uses `0x120d`. Short authored pauses formerly used ~125ms recordings
and now map to an ~11.6ms native pause. These defaults were retained in the
following lossless-notation leg so its comparison measures restored expression
rather than unrelated global retuning. Native instructions remain intact in PA.ROD.

Direct waveform matching of native record 0 against the archived Atari capture
found correlation 0.883 for the first second and 0.881 for its 1.5–2s interval,
at capture/native duration scale about 1.04. This is local timing evidence, not
an exact global clock measurement or listening acceptance. Do not apply a global
slowdown to compensate for missing authored expression. No Unity build, browser
check or new human listening acceptance was obtained in this audit.

## Listening decision and next-leg boundary (2026-09-07)

Arthur's listening verdict: “full expression is the best next to atari original,
no question”. Full expression is the accepted direction; this is not acceptance
of exact hardware timing. He also explicitly permits replacing obsolete C#
phoneme structures rather than retaining them for compatibility.

The French-entry workflow provides “write French → listen → correct a word”.
French spaces separate written words; commas and periods supply pauses. There
is no requirement for extra visible pause-marker syntax. That French contract
must not inherit the legacy notation parser's whitespace-to-pause rule.
The audited native expression and playback foundation remains unchanged. Bracketed
notation is a lossless interchange/inspection surface, not a required primary
editing experience. Integration details and the quoted user decisions are in
[FRENCH_SPEECH_HANDOFF.md](FRENCH_SPEECH_HANDOFF.md).

## Lossless editable expression (2026-09-07)

Arthur approved preserving original duration, emphasis and pauses in editable
dialogue, followed by a recording comparison. In this completed implementation
leg, the existing dialogue string remains the sole stored score; there is no native/shorthand pair, prerecorded-original
playback path, new JSON schema or story migration. This storage shape is
replaceable in the French-entry leg; preserving native expression is the contract.
Original templates and newly
authored text pass through the same parser, preprocessor and PCM interpreter.

A sound may carry `[envelope,amplitude,rate]`, for example
`r[1,5,3]_o[4,1,0]_o[1,0,5]`. The fields exactly encode the remaining ten bits
alongside the six-bit descriptor:

| Field | Range | Meaning |
|---|---|---|
| Envelope / `forme` | 0–15 | Native shape/repetition code. It can select attack/body/release or consonant behavior; it is **not** a linear duration slider. |
| Amplitude / `volume` | 0–7 | 0 inherits current state when emitted; 1 normal, 2 half, 3 three-quarters, 4 five-quarters, 5–7 one-and-a-half, using native integer rounding. Controls are only applied when the native preprocessor emits them. |
| Rate / `vitesse` | 0–7 | 0 inherits current state when emitted; 1–7 emit native rate parameters -3 through +3. The sample-clock conversion remains approximate. |

The formatter spells all fields explicitly and joins atoms with underscores.
`e2` distinguishes the second native e descriptor. Unlabelled variants retain
`sonNN` names rather than invented French phoneme labels. `,[0,0,0]` and
`.[0,0,0]` are exact native pauses; `fin[0,0,0]` preserves descriptor62's original
record marker (not a new phrase/effect boundary). All 64 descriptors can round-trip.
Bare familiar phonemes retain their prior defaults. Composite aliases such as
`oi`, `ui`, `gn`, and contextual `ti` must be expanded to individual native sounds
before adding explicit controls. Invalid fields are rejected by the workbench
and CLI with an actionable error; they are never silently masked to fit a bit range.

**One deliberate behavior removal:** the parser no longer appends a word pause
that was not written after the last group. This removes 151 samples (~11.6ms)
from ordinary existing speech endings; native end-of-record context still closes
the last sound. Write `_,` when that final pause is wanted. Other authored sound,
whitespace and pause defaults are unchanged. This is what allows exact original
notation without a special playback mode or a format-dependent trailing-pause rule.

The workbench's existing picker adds three **Rody 1 · ouverture** templates,
corresponding to original records 0, 1 and 3. Audition previews them; Insert copies
the full expression into the same editable field. They are generated from PA.ROD
when selected, not duplicated as stored text. Editing, Apply, story save/export
and import retain the string. Existing official stories were not rewritten using
guessed record-to-scene correspondence.

Passage playback now receives full text and a character range. It preprocesses
with all neighboring sounds and inherited state, traces token-to-command and
command-to-sample boundaries, then returns the selected PCM slice. Effects keep
their existing phrase boundaries. It does not render a context-free substring or
try to infer live amplitude by scanning lexical fields. Selection boundaries are
native token emission boundaries; diphone transitions belong to the native emit
that produced them, not a separately inferred acoustic word boundary.

Validation covers every one of the 65,536 native token values round-tripping,
all 101 original records retaining their 6,615,080 PCM samples through editable
notation, strict field errors, and selected passages partitioning the full audio.
A separate expected command fixture checks inherited gain in a selected vowel.
All 850 authored/notation cases pass their specified checks. Cold review caught
and prompted the passage-context fix; follow-up review found no blocking defect.
The original implementation leg used offline checks only. Its subsequent
workbench completion check is recorded below. Browser/player checks remain
release work; full expression has now been accepted in the listening comparison.

### Workbench completion check

After refreshing the Editor's pending asset imports (normal script reload, no
forced compilation command or player build), the running workbench was exercised:

- Its picker had 46 options; inserting the first original template exactly matched
  the bank-derived notation. Playback created 51,870 samples at 13,000Hz.
- Invalid `on[1,8,0]` disabled Play and Apply and named the out-of-range volume.
- Full-context selected playback of the final vowel in
  `r[1,5,3]_o[4,0,0]_o[1,0,5]` created 1,307 samples with normalized range
  -0.8203125..0.75, consistent with the inherited-gain fixture.
- The composited 960×600 game view showed the full-expression text, original
  template option and two-line help without overlapping neighboring controls.
  The help text uses the existing vertical overflow; no scene change was needed.
- Play Mode was stopped and the original clean collection scene restored.

This closes the new-template runtime check. Browser behavior, French conversion
integration and player builds were not exercised here.

### Matching recording comparison

The passage is “Rody, maman a ouvert doucement la porte de ta chambre.”
The Atari excerpt is `captures/r1_full.wav` at 121.95s for 4.35s, including some
surrounding silence; transcription checks the spoken content. The full score is
original record 0, exported through the formatter/parser and `RenderDialogue`.
The simplified baseline is the same sentence from the earlier listening pack.

- Simplified notation: **3.036s**. Full expression: **3.990s** at unchanged pitch 1.
- Native windows 0–1s and 1.5–2s align to the Atari capture with normalized waveform
  correlations **0.885 / 0.913**, at capture/native duration scales **1.040 / 1.039**.
- Whole-utterance correlation remains only **0.421** (simplified **0.171**) under
  a single best time scale. Local drift / hardware output differences remain;
  this does not establish exact timing or perceptual acceptance.

The local `~/Downloads/Rody-full-expression/Listen.wav` plays Atari, simplified,
then full expression, with one-second separators and equal RMS level. No pitch
or tempo adjustment was applied to these listening versions. Its transcript
contains the same sentence three times (with Whisper's “Roby” substitution).
Raw render durations and comparison parameters are retained in that folder.

## Sources and ownership

- `Assets/Scripts/RodySpeechEngine.cs`: pure C#, native token preprocessing,
  PCM rendering, and translation of the authored notation.
- `Assets/Resources/Speech/Rody1.bytes`: exact runtime copy of
  `tools/original-extraction/banks/rody1_PA.ROD` (109,598 bytes). One recording bank
  serves all stories, as the previous remake also used Rody 1 recordings.
- `Assets/Resources/Speech/Tables.bytes`: 466 lookup bytes at TEXT-relative
  `0x4aee..0x4cbf` from the original `banks/rody1_AAA.PRG` (28-byte header).
- The extraction directory owns the historical reference implementation and
  captured data. `tools/speech/verify.py` checks both runtime data copies against
  their sources before comparing output.
- Canonical authored token values are derived from `catalog/phoneme_table.tsv`
  and `data/rep_tokens.json`. Verification derives its expected mappings from
  those sources rather than copying the C# lookup table.
- `SoundManager` owns one-shot playback, generated clip lifetime, character
  pitch, and completion. The game's existing external `soundSource` reference
  is retained; its same-object AudioSource is a different music source.
- The sound prefab's named `noise`, `bird`, and `pop` fields own the three
  in-dialogue effect references. No indexed phoneme clip array remains.

The port renders unsigned 8-bit PCM at a calibrated 13,000 Hz. Unity receives a generated mono
AudioClip with samples `(byte - 128) / 128f`. A speech segment plays continuously;
only explicit effects split it. Completion follows AudioSource playback, not a
per-phoneme duration guess. Replacing playback or disabling the manager stops
its coroutine and releases generated clips. Effects and music assets are never
destroyed by speech cleanup.

## Notation and character behavior

Existing underscores, spaces, commas, periods, feedback lines and character
settings remain supported. Empty underscore tokens and whitespace separators
add a native word pause (151 samples); a period adds one native sentence
pause (4,196 samples). An entirely empty/whitespace-only dialogue produces no
speech. Unknown tokens retain the former pause behavior and now log a warning;
the offline renderer rejects them so an agent cannot silently export bad speech.

Most tokens use each descriptor's representative original token, preserving the
original context-dependent bank selection and diphone onsets. `et` maps to the
catalog's `é`, `eu` to `e`, `un` to `in`, `oi` expands to `ou+a`, `gn` to `n+y`,
and `ui` to `u+i`. These are synthesis approximations, not claims that every
French phonetic distinction exists in the original bank.

Extended authored sounds require explicit native controls:

| Notation | Native interpretation | Evidence / limit |
|---|---|---|
| `ti` | bank-2 descriptor 5, with the engine's contextual bank-toggle control | Recorded `22b_T` correlates with bank 2 (0.931), normal `22a_T` with bank 6 (0.969). The toggle is chosen from the following native token; duration stays unchanged. |
| `ouu` | descriptor 7, duration nibble 2 (`0x2207`) | Both recorded `ou` variants match this grain. Increased native duration retains emphasis, but does not duplicate the recording's slight rate difference. Listening pending. |
| `ee` | descriptor 9, duration nibble 3 (`0x3209`) | The recording correlates with descriptor 9 (0.981), versus descriptor 10 (0.659). It is a sustained vowel used both in cow dialogue and ordinary feedback, not a separate animal effect. Listening pending. |
| `-`, `cuicui`, `pop` | original retained noise, bird and pop recordings | Effects are played between synthesized segments on the same AudioSource at the selected character pitch. |

Rody/other introductory characters retain per-line pitch. Mastico retains 1.0,
and Zambla 0.9, as before. Mastico's speaking/processing animation sequence is
unchanged. The third non-Mastico introductory line now sets its speaking flag,
fixing the previous missing mouth animation.

## Reproduce the comparison

Requires Python 3 and the .NET 9 SDK. No Unity build, editor test suite, emulator,
network service, or new package is needed:

```bash
python3 tools/speech/verify.py
```

The verifier links the actual Unity C# source into the small .NET command-line
tool. Temporary fixtures come from the Python extraction renderer, which shares
translation ancestry with C#. It checks all 101 original records at both command and PCM boundaries, canonical
notation against the catalog-derived mapping, authored pauses and extended
sounds, and all embedded story / fixed-feedback dialogue for unknown tokens
and effect ordering. It never rewrites story data or expected results.

Current result (2026-09-07): 101 records, 6,615,080 PCM samples byte-identical;
850 authored dialogue/notation cases rendered without unknown tokens, with the
specified PCM and effect-order comparisons passing. This establishes fidelity
to the Python reference, not independent proof of perfect 1988 hardware timing.
The reference's sample-delay calibration remains approximate. New in-game A/B
listening, especially Ibiza character voices and extended sounds, remains a
release requirement. WebGL verification belongs to the final release leg.

For the independent original-machine check, also install Hatari and supply a
local TOS ROM (the ROM is not distributed here):

```bash
python3 tools/speech/audit_original.py --tos /path/to/tos162fr.img
```

This extracts the original executable and bank from the disk, applies only
standard GEMDOS relocations, and runs its preprocessor and amplitude routines.
It feeds original CPU results directly to the C# executable, without using the
Python port as an oracle. The printed temporary folder retains raw captures,
debugger scripts, logs and the manifest. The script intentionally pins the known
Rody 1 executable hash; it is not a general bank or emulator framework.
The audit was run with Hatari 2.6.1 / STE / TOS 1.62 French. Original full-game
caller observations are recorded separately in `tools/speech/original-caller.json`.

## Offline preview

```bash
dotnet run --project tools/speech -- render /tmp/rody.wav "b_r_a_v_o l_e_v_o" 1.0
# Equivalent entry point used by the authoring skill:
python3 .claude/skills/french-to-rody-phonemes/scripts/render.py /tmp/rody.wav "b_r_a_v_o l_e_v_o" 1.0
```

The tool executes the same native synthesizer and notation handling as Unity,
resolves effects from the sound prefab, and writes a mono 44.1 kHz WAV. Its
linear final resampling is a preview; Unity's mixer resampling can differ.
Export an editable original with the same playback path (zero-based record index):

```bash
dotnet run --project tools/speech -- render-original /tmp/original.wav 0
# Also writes /tmp/original.txt. Paste it into the workbench or a story dialogue.
dotnet run --project tools/speech -- validate "on[4,1,0]_on[1,0,3]"
```

The command fails on unknown notation. The conversion skill remains available
for agent authoring; the in-game French workflow is described below.

## Removed implementation / retained evidence

- Removed recursive playback of individual recorded phonemes, guessed overlap
  waits, consumed phoneme lists, mutable clip-index constants, and the obsolete
  indexed speech array with its scene overrides.
- Removed unused serialized clip overrides on three standalone phoneme buttons.
- Archived 40 former recordings and their import metadata under
  `tools/original-extraction/remake-clips/` for comparison. They are outside
  Unity's Assets tree and no longer feed runtime speech. The three actual
  effects remain in Assets. No dialogue or reference recording was deleted.
- Replaced the agent's separate recorded-clip renderer with the shared C# tool.
  Historical STT scores for the old renderer are not acceptance thresholds for
  this engine.

## Dialogue workbench (2026-09-07)

The collection's **Voix** button opens scene 7 standalone. Intro and object
editors open the same scene additively with explicit text, pitch and apply/close
callbacks. No story manager is required for standalone use. Apply changes the
calling editor's working dialogue; Cancel leaves it untouched. This does not
replace the story editor's existing scene-save/export boundary.

The old phoneme-key grid was replaced by a multiline sentence field, whole-line
and passage playback, a pitch slider, 43 sound/pause/effect examples plus three original-expression templates with audition
and insertion, clipboard actions, and restore. Unknown tokens are shown before
playback. Stop remains available while an invalid or empty edit is being played.
Rody pitch is editable; Mastico/Zambla previews use their actual fixed pitch.
This was the notation-only workbench baseline; French entry now extends it as
described below.

`SpeechInputField` applies requested carets after uGUI's deferred activation in
LateUpdate. Browser paste captures its range and makes text read-only until the
clipboard result arrives; copy failure cannot cancel a pending paste. Escape is
handled on key release while Maker ignores input when the additive scene is
active. Each instance has a unique clipboard receiver name.

Removed the old grid, its help panel, five button forwarding scripts, and the
unused ButtonPhoneme prefab. Their capabilities are represented by the sentence
field, sound examples, direct controls and persistent guidance; no authored text
or source recording was removed. Alata replaces the old Rody font in this editor
because its accented glyphs and line metrics remain readable at interface sizes.

Runtime checks use the live Editor and actual UI components: standalone menu
entry, native editing after insertion/paste, stop after invalid input, and additive
apply/cancel with controls restored, and fixed Zambla pitch (0.9, slider locked).
Full-frame captures cover the menu, workbench and open picker.
Independent source and serialized-reference review checked the new scene and
collection entry. Browser clipboard permissions, physical held-Escape behavior,
player build and human listening remain release checks; no Unity test suite or
additional forced compile was run.

## French entry (2026-09-08)

The same workbench accepts ordinary French above a selected-word pronunciation
field. Whole-utterance conversion retains liaison context; selecting a word never
reconverts it in isolation. Commas emit `,[9,0,0]` (~116ms), periods emit
`.[0,0,0]` (~323ms); written spaces emit no pause. Semicolons/colons use the short
pause and question/exclamation/ellipsis the long one. Decimal separators remain
inside numbers. IPA uses the native bank's available sounds, including complete
`on[1,1,0]`; this is an approximation, not perfect French phonetics.

The ephone/eSpeak dependency supplies aligned IPA locally, without a service or
neural voice. Pinned source, checksums, license notices, reproducible macOS build
and browser ABI belong to `tools/french-converter/README.md`. Native binaries
support macOS arm64/x64; WebGL uses the packaged ES module through its jslib.
Windows/Linux native conversion is not supplied. Ordinary English pronunciation
guesses remain enabled. The case-insensitive authored-name dictionary currently
contains Rody → `r_o_d_i`; per-dialogue corrections take precedence.

`SpeechDocument` owns source text and word fragments containing the one accepted
score plus explicit-correction status. Aggregate notation is computed, never
stored as a competing score. JSON format 2 saves these objects; `StoryJson` wraps
legacy strings without altering their exact notation. No official story asset
was rewritten. Scene save, export/import, clone and gameplay consume this same
representation. Opening an existing document never reconverts it.

Valid pronunciation edits commit at the edit boundary, so partially typed native
controls cannot accidentally freeze pronunciation. Unchanged source words are
sequence-aligned across unrelated edits. Corrected words retain their exact score;
changed words receive a new pronunciation. Automatic reset clears the correction
while retaining expression on surviving sounds. Identical pronunciation keeps the
exact authored score, including context-sensitive aliases. When pronunciation
changes, matching native sound envelopes and explicit/implicit pauses and effects
are retained through the actual engine parser. Expression on removed sounds is
removed with those sounds. Full-context PCM selection remains the playback path.

Original templates explicitly replace the entire reply with their original score;
they do not claim a fabricated French alignment. Source-less original scores
remain directly editable. Paste targets and ranges are frozen while browser
clipboard access is pending. The picker no longer offers written space as a
pause; native score whitespace is still supported for existing authored data.

Narrow checks: live Editor conversion, contextual liaison, correction retention,
changed-word reset, story JSON save/reopen and exact original-score retention.
A fresh-source standalone probe using the shipped native library and actual C#
converter/engine checked Rody, full envelopes plus implicit pauses, multi-site
source edits and contextual `ti` retention. Dependency native/browser output and
jslib callbacks were exercised independently. Source and serialized-reference
review prompted the edit-boundary and expression fixes. A 960×600 workbench
capture was inspected. Latest code was probed outside Play Mode's loaded assembly
so Arthur's active text/session was not interrupted. No forced Unity compile,
player build, browser-in-Unity conversion or new human listening acceptance was
performed in this leg; browser/player validation remains release work.
