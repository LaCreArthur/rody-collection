# Speech engine

The game and phoneme previews use `RodySpeechEngine`, a C# port of the extracted
Atari ST preprocessor and PCM interpreter. Story JSON still stores phoneme
strings and voice settings; no story migration or per-story voice switch exists.
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

The remake notation still loses original per-phoneme controls, repeated-descriptor
envelopes and some pauses. Its `on` default (`0x420d`) selects an attack fragment
from such an envelope; a complete standalone `on` commonly uses `0x120d`.
Short authored pauses formerly used ~125ms recordings and now map to an ~11.6ms
native pause. A provisional pause/token patch was withdrawn because it did not
restore that expressive information. Native instructions remain intact in PA.ROD.
The next design step is one lossless editable speech representation; no format
migration or new notation is implemented or approved by this audit.

Direct waveform matching of native record 0 against the archived Atari capture
found correlation 0.883 for the first second and 0.881 for its 1.5–2s interval,
at capture/native duration scale about 1.04. This is local timing evidence, not
an exact global clock measurement or listening acceptance. Do not apply a global
slowdown to compensate for missing authored expression. No Unity build, browser
check or new human listening acceptance was obtained in this audit.

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
settings remain supported. Empty underscore tokens and every space-delimited
group add a native word pause (151 samples); a period adds one native sentence
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
841 authored dialogue/notation cases rendered without unknown tokens, with the
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
The command fails on unknown notation. The conversion skill remains the
French-to-phoneme authoring route; there is no in-game dictionary converter.

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

The old phoneme-key grid is replaced by a multiline sentence field, whole-line
and passage playback, a pitch slider, 43 sound/pause/effect examples with audition
and insertion, clipboard actions, and restore. Unknown tokens are shown before
playback. Stop remains available while an invalid or empty edit is being played.
Rody pitch is editable; Mastico/Zambla previews use their actual fixed pitch.
French-to-phoneme conversion remains the agent authoring skill; the workbench
edits phoneme notation, not ordinary French spelling.

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
