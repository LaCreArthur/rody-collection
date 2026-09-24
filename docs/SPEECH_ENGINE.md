# Speech engine

Current implementation reviewed against source on 2026-09-09. Runtime checks and
listening results below are dated evidence from earlier sessions; this documentation
audit did not rerun them or build Unity.

The game and voice workbench use `RodySpeechEngine`, a C# port of the extracted
Atari ST preprocessor and PCM interpreter. French authoring is implemented:
write French, listen, correct a word. French source and accepted word scores are
saved together; playback never reconverts them. There is one rendering path for
French-authored dialogue, imported notation and original-expression templates.

## Sources and ownership

| Producer / artifact | Owns | Consumers |
|---|---|---|
| `Assets/Scripts/Models/SpeechDocument.cs` | French source, word spans, accepted lossless scores, pronunciation-correction flags; aggregate notation is computed | Workbench, story serialization and gameplay |
| `Assets/Scripts/synth/FrenchSpeechDraft.cs` | Source punctuation, IPA-to-native mapping, authored-name overrides, story respellings (`Spoken`) and the workbench's preservation of word corrections/expression | Maker and workbench conversion |
| `Assets/Scripts/RodyMaker/RM_Speech.cs` | Maker conversion requests and the browser receiver object `RodyMakerSpeech` | Maker dialogue sync and fix preview |
| `Assets/Scripts/RodyMaker/RM_VoiceLayout.cs` | Maker pronunciation-fix mode: word picking, respelling preview, story-wide apply | Maker text editing |
| `Assets/Scripts/synth/FrenchPhonemizer.cs` | Local dependency invocation and source alignment | French draft conversion |
| [French dependency README](../tools/french-converter/README.md) | Pinned upstream source, licenses, checksums, macOS rebuild and browser/native ABI | Dependency maintenance; do not duplicate its setup here |
| `Assets/Scripts/RodySpeechEngine.cs` | Notation parsing, native preprocessing, PCM rendering and full-context passage selection | Game, workbench and offline CLI |
| `Assets/Scripts/SoundManager.cs` | Playback, character pitch, generated clip lifetime and completion | Gameplay and preview |
| [Original extraction](../tools/original-extraction/DECODED.md) | Historical reference implementation, original binaries, captures and extraction evidence | Engine audit and comparison tools |

`Assets/Resources/Speech/Rody1.bytes` is the 109,598-byte runtime copy of
`tools/original-extraction/banks/rody1_PA.ROD`. `Tables.bytes` contains 466 bytes at
TEXT-relative `0x4aee..0x4cbf` from `banks/rody1_AAA.PRG` (28-byte header). The
documentation audit compared both copies with these sources; both were equal.
One Rody 1 bank serves every story, as the former remake recordings did.
The extraction's `catalog/phoneme_table.tsv` and `data/rep_tokens.json` supply
representative authored mappings; the verifier derives expected values from them.

The port renders unsigned 8-bit PCM at a calibrated **13,000 Hz**. Unity receives
mono AudioClips with samples `(byte - 128) / 128f`. Speech plays continuously until
an explicit effect splits it. Completion follows AudioSource playback, including
pitch. Replacing playback releases the previous generated clip; disabling the
manager stops its coroutines and releases its clip. Scene teardown owns the
external speech AudioSource. The same-object AudioSource is a separate music
source. The sound prefab's named `noise`, `bird` and `pop` fields own dialogue
effects; speech cleanup never destroys effects or music assets.

## French authoring

The Maker converts every accepted dialogue edit (`RM_DialLayout.SyncSpeech` →
`RM_Speech` → `FrenchSpeechDraft.Convert(text, spoken, result)`). Its score is a
function of the French text and `Story.respellings` only (lower-case written word
without quotes → one-word French respelling); earlier scores are never merged.
`Spoken` replaces each respelled word in place, so the whole utterance keeps its
liaison context and maps 1:1 back to the written words; a respelling that splits
into several words is rejected. The fix mode previews with a session copy of the
respellings; Validate stores it on the draft and resyncs every non-original
dialogue containing a changed word. Source-less original scores are never
reconverted until their text changes. Browser results arrive by `SendMessage` to
`RodyMakerSpeech`, a runtime object active for the whole Maker scene.

The collection's **Voix** button opens the workbench standalone (scene 7); the
Maker no longer opens it. **Copier les phonèmes** copies the score only, not French
source or word corrections. The rest of this section describes the workbench.

The French field accepts up to 2,000 characters. Conversion uses the whole
utterance for liaison/context, while the lower field edits the selected word's
pronunciation. Selecting a word does not reconvert it in isolation. Source
punctuation owns pauses because the dependency can omit punctuation from IPA:

| French source | Generated native score | Nominal duration |
|---|---|---|
| Spaces | No pause | — |
| `,` `;` `:` | `,[9,0,0]` | ~116 ms |
| `.` `?` `!` `…` | `.[0,0,0]` | ~323 ms |

Each punctuation mark emits its own pause (`...` therefore differs from `…`).
Decimal commas/periods between digits stay inside numbers. These French rules
are distinct from the legacy notation whitespace rules below.

The local ephone/eSpeak dependency supplies pronunciation and aligned IPA, not
audio or a neural voice. Native conversion supports macOS arm64/x64; WebGL has a
packaged ES module and jslib bridge. Windows/Linux native conversion is not
supplied. English pronunciation guesses remain enabled for some foreign words.
The case-insensitive authored-name dictionary contains Rody → `r_o_d_i`;
story respellings replace the word before this lookup, and workbench
per-word corrections take precedence. Unknown words/typos receive a
pronunciation attempt; unsupported IPA produces a visible conversion error.
The native bank approximates French distinctions, including a complete
`on[1,1,0]` for the nasal vowel, rather than promising perfect phonetics.

`SpeechDocument` holds source text and word fragments containing the one accepted
score plus correction status. `StoryJson` reads legacy dialogue strings once as
source-less documents without altering their notation. Format 2 saves document
objects; source-less imported/original scores stay editable in the workbench and play
unchanged in the Maker until their text is edited. Opening,
playing or cloning a document never reconverts it. Official story assets were
not rewritten by the French-entry implementation.

Valid pronunciation edits commit at an edit boundary so partially typed native
controls cannot freeze pronunciation. Unchanged source words are sequence-aligned
across unrelated edits; corrected words retain their score, while changed words
receive a new pronunciation. Automatic reset clears the correction while retaining
expression on surviving sounds. Identical pronunciation preserves the exact score,
including contextual aliases. If pronunciation changes, matching native envelopes,
pauses and effects survive through the engine parser; expression on removed sounds
is removed with them. Pronunciation and expressive delivery remain different facts.

Whole-line and passage playback, audition/insertion examples, pitch, clipboard
actions and restore share this document. Original templates for **Rody 1 · ouverture**
read bank records 0, 1 and 3 when selected. Inserting one replaces the entire reply
with its original score; it does not fabricate French alignment. Official story
dialogue restoration still requires mapping through original callers and spoken
content, not guessed text indices. The picker does not present a written space
as a French pause.

Unknown/invalid score tokens appear before playback and disable Apply. Stop stays
available during playback even after an invalid edit. Rody/other introductory
characters use per-line pitch; Mastico uses 1.0 and Zambla 0.9. The Maker speaker
button cycles Mastico → 0.8, 0.9, 1.1, 1.2, 1.3 → Mastico; other stored pitches stay
until clicked. All three non-Mastico introductory lines set their speaking flag.
Mastico retains its speaking/processing animation sequence.

Browser paste freezes its target and range until the clipboard callback arrives;
copy failure cannot cancel a pending paste. Each workbench has a unique receiver
name. `SpeechInputField` applies carets after uGUI's deferred activation. Escape
closes on key release.

## Lossless notation

A sound may carry `[envelope,amplitude,rate]`, for example
`r[1,5,3]_o[4,1,0]_o[1,0,5]`. These fields encode the remaining ten bits alongside
the six-bit native descriptor:

| Field | Range | Meaning |
|---|---|---|
| Envelope / `forme` | 0–15 | Native shape/repetition code, including attack/body/release or consonant behavior; not a linear duration slider |
| Amplitude / `volume` | 0–7 | 0 inherits state when emitted; 1 normal, 2 half, 3 three-quarters, 4 five-quarters, 5–7 one-and-a-half, using native integer rounding |
| Rate / `vitesse` | 0–7 | 0 inherits state when emitted; 1–7 emit native rate parameters -3 through +3; sample-clock conversion remains approximate |

Amplitude/rate fields act only when the preprocessor emits their controls. The
formatter spells every field explicitly and joins atoms with underscores.
`e2` distinguishes the second native e descriptor; unlabelled descriptors use
`sonNN` rather than invented phoneme identities. `,[0,0,0]` and `.[0,0,0]` encode
exact native pauses; `fin[0,0,0]` preserves descriptor 62's original record marker,
not a new phrase/effect boundary. All 64 descriptors can round-trip. Invalid
fields fail workbench/CLI validation rather than being masked into range.

Bare phonemes retain representative original-token defaults, including contextual
bank selection and diphone onsets. `et` maps to the catalog's `é`, `eu` to `e`,
`un` to `in`, `oi` to `ou+a`, `gn` to `n+y`, and `ui` to `u+i`. These are synthesis
approximations. Composite/contextual aliases `oi`, `ui`, `gn` and `ti` must be
expanded to individual native sounds before adding explicit controls.

Empty underscore tokens and whitespace separators add native word pauses (151
samples, ~11.6 ms); bare `.` adds a sentence pause (4,196 samples, ~323 ms). Entirely
empty/whitespace-only notation produces no speech. No unwritten final word pause
is appended: the lossless-expression change removed 151 samples from ordinary
endings. Write `_,` to request it. Unknown tokens still play as pauses with a
warning in gameplay; the workbench and offline renderer reject them.

| Authored sound | Native interpretation | Recorded evidence / limit |
|---|---|---|
| `ti` | Bank-2 consonant P5 via descriptor 27's contextual bank toggle | `22b_T` correlates with bank 2 (0.931), normal `22a_T` with bank 6 (0.969); following native context chooses the toggle, with unchanged duration |
| `ouu` | Descriptor 7, envelope 2 (`0x2207`) | Both recorded `ou` variants match this grain; native emphasis does not reproduce their slight rate difference; listening pending |
| `ee` | Descriptor 9, envelope 3 (`0x3209`) | Recording correlation 0.981 versus descriptor 10's 0.659; sustained vowel used in cow dialogue and feedback, not an animal effect; listening pending |
| `-`, `cuicui`, `pop` | Retained noise, bird and pop recordings | Played between speech segments on the same source at character pitch |

Bare `on` (`0x420d`) selects an attack fragment; a complete standalone `on` commonly
uses `0x120d` (`on[1,1,0]`). Bare notation lacks the original per-sound envelopes and
some pauses. Its short pause formerly used ~125 ms recordings; these legacy
defaults were retained through the expression change to isolate that comparison
from global retuning. Original native instructions remain intact in PA.ROD.

Passage playback receives the full notation plus a character range. It traces
token → command → sample boundaries through full-context preprocessing and slices
the resulting PCM, retaining inherited state. Effects retain phrase boundaries.
Diphone transitions belong to the native emit that produced them, not inferred
acoustic word boundaries. Do not render an isolated substring or infer live gain
from the last lexical control field.

## Accepted direction

Arthur accepted full expression on 2026-09-07: “full expression is the best next
to atari original, no question”. This accepts the sound direction, not exact
hardware timing. His later instruction permits replacement of old structures:
“you don't have to carry my legacy c# phonemes structures or anything, it's very
old and flawed, you can do something better if you have the opportunituy”.

The French-entry approval was: “[pause courte] or [pause longue] no need if the
keys , and . are clearly understood and explained it'll be enough. otherwise
great plan. let's build it”. The accepted outcome is French entry, contextual
pronunciation, word correction persistence and preserved original expression.
Bracketed notation is interchange/inspection, not a required primary experience.
A separate spelling assistant was not prioritized. These user statements do not
approve a particular storage algorithm or require the former phoneme editor to
remain underneath. French entry was implemented in `2d7555e`; the handoff is closed.

## Evidence and fidelity limits

### Original-machine audit — 2026-09-07

Arthur rejected the earlier preview as “way more rushed and chopped”. Earlier
Python/C# agreement reproduced shared mistakes, so it was not independent Atari
fidelity evidence. Re-extraction through the disk's FAT12 chains matched the
archived AAA.PRG and PA.ROD. Execution of original 68000 code in Hatari exposed
the following defects, corrected in both ports in `73cb04c`:

- The archived disassembly stopped inside the type-4 consonant handler and
  mistranscribed variant `3` as `0xaa`. The full attack/body/release and
  following-vowel path, including the missing table prefix, was restored.
- Low-consonant P22 used the wrong look-behind rule for three preceding vowels
  and failed to terminate its onset path in other contexts.
- Floating amplitude scaling differed from signed arithmetic shifts. All five
  levels were changed to native integer arithmetic.
- The interpreter skipped empty intervals, although the original reads one
  sample before checking the end pointer; it also omitted end dispatch and
  speed-byte wrapping. Invalid command/variant input now fails explicitly as a
  diagnostic contract, not emulation of invalid-input/copy-protection behavior.
- The bank contains **101** records. Header entry 101 is the endpoint; the former
  102nd fixture was an empty artifact of an invalid range.

Recorded independent check: **1,429 command streams** from original instructions
matched C#: 101 source records, 304 batches varying all 16 duration / 8 amplitude /
8 rate fields over 37 corpus descriptors, and 1,024 batches covering all 262,144
descriptor triples at default controls. **1,280 sample values** matched the
original amplitude instructions (256 bytes × 5 levels). This finite coverage uses
base speed -1 and mode 0; it does not prove every possible input. Empty-interval,
speed-wrap and end-dispatch behavior also received source inspection, not full
original interpreter execution tests.

The old Python preprocessor disagreed with original CPU output on records 25 and
51 and synthetic contexts, ruling out a C#-only port error. Disk extraction checks
the separate assumption that archived inputs are authentic. Original binaries
remain unchanged and the incomplete listing was replaced. An unmodified game
boot observed base speed -1, mode 0 and a cleared termination marker at opening
calls 1, 2 and 4, including end dispatch; machine details/hashes live in
[original-caller.json](../tools/speech/original-caller.json).

**Fidelity limits:** 13,000 Hz and speed-to-sample conversion are calibrated
approximations. Original output transforms samples through three YM2149 volume
tables, writes channels sequentially and incurs CPU/segment overhead. The port
does not model that hardware output or exact clock. Base speeds >=5 alter
preprocessing duration and fall outside this fixed-speed runtime audit. Native
record 0 initially correlated 0.883 over its first second and 0.881 over 1.5–2 s,
with capture/native duration scale about 1.04. This is local timing evidence;
do not add a global slowdown to compensate for missing authored expression.

### Lossless expression and listening comparison — 2026-09-07

`2eebca7` added lossless editable expression. Recorded checks covered all 65,536
native token values round-tripping and all 101 records retaining **6,615,080 PCM
samples** through notation. **850 authored/notation cases** covered specified PCM,
strict fields and effect-order comparisons. Passage selections partitioned full
audio; a separate expected-command fixture checked inherited vowel gain. These
Python comparisons share translation ancestry and do not prove exact hardware
timing. Cold review prompted the full-context passage fix.

The matching sentence was “Rody, maman a ouvert doucement la porte de ta chambre.”
The Atari excerpt is `tools/original-extraction/captures/r1_full.wav` at 121.95 s
for 4.35 s, including surrounding silence. Full expression uses original record
0 through formatter/parser/renderer; the simplified baseline is the same sentence.

| Measurement | Recorded result |
|---|---|
| Simplified / full expression duration, pitch 1 | 3.036 s / 3.990 s |
| Full-expression local correlations at 0–1 s / 1.5–2 s | 0.885 / 0.913 |
| Corresponding capture/native duration scales | 1.040 / 1.039 |
| Whole-utterance correlation, full / simplified | 0.421 / 0.171 |

The local `~/Downloads/Rody-full-expression/Listen.wav` plays Atari, simplified,
then full expression with one-second separators and equal RMS level, without
pitch/tempo adjustment. Its transcript repeats the sentence (Whisper substitutes
“Roby”); `comparison.json` and `sources.json` retain parameters, durations and
source hashes. Local drift and hardware output differences remain. Waveform
correlation itself is not perceptual acceptance; Arthur's verdict above supplies
the listening decision.

### Workbench and French-entry checks — 2026-09-07–08

Recorded live Editor checks covered standalone entry, additive Apply/Cancel with
Maker controls restored, editing after insertion/paste, Stop after invalid input,
and fixed Zambla pitch. The notation-only baseline had 46 picker options; the
later French version removed its written-space option. Original-template insertion
matched the bank score and produced 51,870 samples. `on[1,8,0]` disabled Play/Apply
and named the invalid volume. Selecting the final vowel of
`r[1,5,3]_o[4,0,0]_o[1,0,5]` produced 1,307 samples, normalized range
-0.8203125..0.75, matching the inherited-gain fixture.

French-entry checks covered contextual liaison, correction retention, changed-word
reset, JSON save/reopen and exact original-score retention. A fresh-source probe
using the shipped native library and actual C# converter/engine checked Rody,
full envelopes/implicit pauses, multiple source edits and contextual `ti`. Native
and browser dependency output and jslib callbacks were exercised separately;
source/reference review prompted edit-boundary and expression fixes. Inspected
960×600 captures covered the collection, workbench and picker. Latest source was
probed outside Play Mode's loaded assembly to preserve Arthur's active session.

**Still unverified for release:** browser-in-Unity French conversion and playback,
browser clipboard permissions, physical held-Escape behavior, player builds, and
new in-game listening for French conversion, Ibiza voices and extended sounds.
These legs used no forced Unity compile or player build. Exact Atari clock/PSG
modeling remains separate from French-authoring acceptance.

## Reproduce and preview

Run from the repository root with Python 3 and the .NET 9 SDK. The verifier links
the actual Unity engine source into the CLI, compares the runtime bank/tables to
their sources, creates temporary Python fixtures, checks original records at
command/PCM boundaries and examines embedded dialogue/fixed feedback. It does not
rewrite story data or expected results; it is not a French-conversion or Unity test.

```bash
python3 tools/speech/verify.py
dotnet run --project tools/speech -- render /tmp/rody.wav "b_r_a_v_o l_e_v_o" 1.0
dotnet run --project tools/speech -- validate "on[4,1,0]_on[1,0,3]"
dotnet run --project tools/speech -- render-original /tmp/original.wav 0
```

`render-original` takes a zero-based bank record and also writes `/tmp/original.txt`.
Render/validate reject unknown notation. Previews resolve effects from the sound
prefab and write mono 44.1 kHz WAV with linear resampling; Unity's final mixer can
differ. The repository's [authoring skill](../.claude/skills/french-to-rody-phonemes/SKILL.md)
uses thin render/validate wrappers around this same CLI.

The independent CPU audit additionally requires Hatari and a user-supplied TOS
ROM (not distributed here):

```bash
python3 tools/speech/audit_original.py --tos /path/to/tos162fr.img
```

It extracts executable/bank from the disk, applies standard GEMDOS relocations
and feeds original CPU preprocessor/amplitude output directly to C#, without
the Python port as oracle. Raw captures, debugger scripts, logs and the manifest
remain in the printed temporary folder. It pins the known Rody 1 executable hash;
it is not a general emulator framework. The recorded run used Hatari 2.6.1 / STE /
TOS 1.62 French. Neither command requires a Unity build.

## Retired implementation and retained evidence

- Recursive recorded-phoneme playback, guessed overlap waits, consumed lists,
  mutable clip-index constants and the indexed speech array/overrides were removed.
  The separate agent recorded-clip renderer was replaced by the shared C# CLI.
- Forty former recordings and import metadata remain in
  `tools/original-extraction/remake-clips/`, outside runtime Assets. The three
  actual effects remain in Assets; reference recordings and dialogue were retained.
- The keyboard grid, its help panel, five forwarding scripts and unused
  ButtonPhoneme prefab were replaced by the workbench. Alata supplies readable
  accents and interface line metrics. No authored text was removed.
- The July recorded-clip audit and hybrid dictionary proposal are superseded by
  the native engine and contextual French dependency. Old dirty-token counts,
  corpus/coverage figures, effort estimates and STT scores are historical, not current defects or
  acceptance thresholds. Their original detail remains in Git history.
