## TL;DR (Arthur)
Full expression is the accepted sound foundation.
The next workflow is: write French, listen, correct a word when needed.
Spaces separate words; commas and periods control pauses.
Unrelated text edits preserve pronunciation corrections.
The old phoneme structures and editor are replaceable.
Original expression must survive editing and saving.
The remaining Atari clock difference is separate from French conversion.

---
## Technical body (executing agent)

### User decisions, not inferred approval

- “full expression is the best next to atari original, no question”
- “you don't have to carry my legacy c# phonemes structures or anything, it's very old and flawed, you can do something better if you have the opportunituy”
- In the paused session, Arthur accepted the French-entry recommendation and said:
  “[pause courte] or [pause longue] no need if the keys , and . are clearly understood and explained it'll be enough. otherwise great plan. let's build it”
- This session received that handoff with the other agent paused until this leg
  finishes. Do not treat the older agent's “keep the existing phoneme editor
  underneath” as a structural constraint; the later user instruction permits
  replacement. Preserve authored meaning and expression rather than accidental
  implementation structure.

### Boundary and available implementation

The speech specification, grammar, audit evidence and limits have one owner:
[SPEECH_ENGINE.md](SPEECH_ENGINE.md). This handoff does not redefine them.
Commits 73cb04c and 2eebca7 repaired original behavior and added lossless expression.

`RodySpeechEngine.Render(IReadOnlyList<ushort>)` consumes original native tokens
without shorthand conversion or implicit authored word pauses. `Preprocess`
and `Interpret` are the audited stages. Native controls carry envelope behavior,
amplitude and rate; do not reduce a repeated native envelope to a representative
phoneme token. The original bank remains the recording source.

`FormatDialogue` and `ParseDialogue` round-trip every ushort value. The bracketed
text is useful for diagnostics and interchange. It is not a requirement to make
French authors edit native integers, nor to route French conversion through a
legacy string grammar. Choose the simplest editing/storage model that preserves
French source, explicit corrections, and the accepted expression without
independent competing speech versions. Do not implement synchronization between
lossy shorthand and a second authoritative native score.

`OriginalDialogue` reads source records directly from the runtime bank. The
workbench offers records 0,1,3 as original opening templates. Official story
strings have not been automatically replaced; the old text index is not proof
of record-to-scene correspondence. Map through the actual original caller and
spoken content before restoring story dialogue.

### French workflow and carried-forward recommendations

Source: the paused conversation Arthur pasted into this session on 2026-09-07.
He endorsed the French-entry recommendation, then approved building it with
commas/periods instead of extra pause-marker UI. Word-level correction persistence,
typos and postponing a spelling assistant were design recommendations in that
accepted conversation, not separately specified data structures or algorithms.
Retain those user-facing outcomes while revisiting implementation choices when
evidence supports a simpler design. Do not invent additional approval from this
handoff's wording.


- Convert the whole sentence for context (liaisons, h aspiré, grammatical readings).
- Keep source word boundaries for selection/correction, but do not emit a pause
  for every word or repeated written space. Legacy notation spaces do emit pauses;
  that is not the French text contract.
- Derive normal pauses from source commas and sentence punctuation. Do not rely
  on punctuation surviving IPA conversion. No additional pause-marker UI is required.
- Permit local pronunciation correction and playback. Preserve corrections when
  unrelated text changes; changing a corrected word resets its pronunciation.
- Unknown words/typos get a usable pronunciation attempt and editable source text.
  Do not silently rewrite the French or block the full sentence. A separate spelling
  assistant was not prioritized for the first version.
- A selected preview must use the full utterance's contextual rendering, including
  inherited expression. The current engine traces token → command → sample
  boundaries and slices the full PCM. Do not regress to rendering an isolated
  substring or guessing inherited volume/rate from the last lexical field.
- Pronunciation and expressive delivery are different editable facts. Retain native
  controls during pronunciation work; the envelope field is not a linear duration.

### Integration status (2026-09-08)

The French workflow is implemented locally. Current behavior, storage model,
platform support, checks and limitations are owned by
[SPEECH_ENGINE.md](SPEECH_ENGINE.md#french-entry-2026-09-08). The dependency's
pinned source/build/ABI details remain in `tools/french-converter/README.md`.
This handoff retains the accepted direction and original-engine evidence; it is
not an outstanding implementation plan. Browser/player validation remains.

### Evidence and remaining work

- Original-machine oracle: 1,429 command streams and 1,280 gain samples match C#.
- Editable representation: all 65,536 token values and 101 source records preserve
  native data; 6,615,080 original PCM samples are unchanged through the editable path.
- 850 authored/notation checks include strict fields, selection partitioning,
  inherited gain and effect order. The Python authoring validator now calls the
  same C# parser; it requires .NET 9.
- The accepted listening pack is local at `~/Downloads/Rody-full-expression/`.
  Its three versions are Atari, simplified and full expression, equal-level only.
- Exact Atari sample timing/PSG output is still approximate. Do not add a blanket
  slowdown to make one recording fit; treat clock work as a separate measured task.
- Live workbench completion checks covered original insertion/playback, invalid
  fields, inherited selection and the composited UI. The collection scene was
  restored after Play Mode. Details stay in the linked speech spec.
- No player build, browser-in-Unity conversion or release push is part of this leg.
