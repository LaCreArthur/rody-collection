# Phoneme TTS: Audit and French-to-Phoneme Converter Design

Date: 2026-07-03. Status: audit complete, converter designed but NOT implemented.

## Part 1: Audit of the current pipeline

### How it works

1. Dialogue is stored as a phoneme string (`levels.rody` source, `.rody.json`
   runtime, `RM_DialLayout.phonems` in the editor).
2. `SoundManager.StringToPhonemes()` parses it: spaces split breath groups,
   underscores split phonemes inside a group. Each token maps to a clip index
   via `getPhoneme()` (inventory in `P.cs`, 42 clips: 35 sounds + pauses +
   sfx). A short pause (`P.rienp`) is appended after every group.
3. `PlayDialog()` + `playPhoneme()` recursively chain one coroutine per clip on
   a single AudioSource, waiting `clip.length - crossTime` before the next.
4. Pitch is set once on the AudioSource per dialogue (per-scene values from
   story data; Mastico speaks at 1.0, or 0.9 when `isZambla`).

### Findings

| # | Finding | Impact | Suggested action |
|---|---------|--------|------------------|
| 1 | `getPhoneme()` returns `P.rienp` (pause) for ANY unknown token. Typos are silently swallowed. | 11 lines in `original-stories/*/levels.rody` contain dead tokens (`M`, `ca`, `il`, `w`, `.p`, `!`) that play as silence, e.g. "Malheur" plays "alheur". | Run `.claude/skills/french-to-rody-phonemes/scripts/validate.py` over story data; fix the 11 source lines and re-export. Optionally log a warning in `getPhoneme()` default case (1 line, no behavior change). |
| 2 | Timing formula `crossTime = 0.01 ± pitch/100` does not compensate pitch correctly. Real clip duration is `length / pitch`, but the wait is based on `length`. | At pitch < 1 each phoneme is cut off early (~10% at 0.9); at pitch > 1 gaps of silence appear between phonemes. This is part of the shipped sound of the remake by now. | Leave as-is unless dialogue at extreme pitch sounds broken. Correct formula would be `clip.length / pitch - overlap`. |
| 3 | `InitPhoneme(isMastico: true)` calls `MasticoSpeak()` without `StartCoroutine`, so the returned IEnumerator is never iterated: that branch does nothing. All real callers (`Scene.cs`, `Intro.cs`) start `MasticoSpeak` themselves. | Dead + broken code path, misleading API. | Delete the `isMastico`/`process` parameters from `InitPhoneme`. |
| 4 | `PlayDialog` consumes the list passed to it (`RemoveAt(0)`), `MasticoSpeak` defensively copies, direct callers rely on `getDial()` building a fresh list every call. | Fragile contract; harmless today. | Note only. If it ever bites, copy once at the top of `InitPhoneme`. |
| 5 | `RandomOui/RandomNon/RandomPresque` re-run `StringToPhonemes` on hardcoded strings every call. | Negligible (editor-era code, tiny strings). | Ignore. |

The robotic, chunky delivery is the intended aesthetic; nothing above is a
quality problem for the voice itself. Finding 1 is the only real defect class
(silent data corruption) and it is exactly the failure mode a converter or
validator prevents.

### Corpus facts (basis for everything below)

- 8 original stories, 140 scenes, 633 phoneme lines aligned with display text.
- Token inventory is closed: 47 distinct tokens ever used, 41 valid + 6 typos.
- Story texts: 5,718 word tokens, 1,523 distinct words.
- The original data is itself inconsistent (bien = `b_i_in` and `b_i_un`,
  magasin = `m_a_g_a_z_in` and `m_a_g_a_z_un`): the bar is "understandable and
  charming", not phonetically perfect. Errors are cheap; the editor's play
  button is the correction loop.

## Part 2: Player-facing converter options

Problem: players writing stories in RodyMaker must hand-write phoneme strings.
The synth screen (phoneme buttons + free InputField + play preview + pitch
slider) works but demands phonetics intuition; it is the steepest wall in the
editor UX.

### Option A: prebaked top-N word dictionary (rejected as sole solution)

Measured on the actual story corpus:

- Top-100 French words cover 54.8% of word tokens. The chance that a 10-word
  sentence is fully covered is 0.2%. Practically every sentence fails.
- Top-1000 covers 90.9%: still roughly one unconverted word per sentence.

A small dictionary alone cannot ship. As a component of Option D it is fine.

### Option B: full lexicon lookup

Derive a word -> phonemes dictionary offline from an open French lexicon with
phonetic transcriptions. Candidate sources:

- Lexique 3.83 (lexique.org): ~140k inflected forms with phonological codes,
  frequency data. Believed openly licensed; VERIFY the current license before
  embedding (a web check was not completed during this design pass).
- WikiPron / Wiktionary extractions (CC BY-SA).
- espeak-ng French G2P output (GPLv3: license contamination risk for the repo,
  avoid embedding).

A python build script maps the source phone alphabet to the 35 Rody tokens.
The mapping is nearly 1:1; distinctions the game lacks get merged (/ø/ -> `e`,
/œ̃/ -> `in`, /ɥ/ -> `u`). Ship as one compressed text asset (~1-2 MB), load
into a `Dictionary<string, string>` when the synth screen opens. Works on
desktop and WebGL (pure C#, no platform dependency).

Coverage: ~91-97% of typical story vocabulary. Fails on proper nouns
(Mastico, Gobino), invented words, typos.

### Option C: rule-based G2P algorithm

French spelling-to-sound is regular enough that a few hundred ordered rewrite
rules reach roughly 90-95% word accuracy; classic systems (LIA_PHON, espeak)
prove it. But it is weeks of tuning, the exception tail is long (femme,
monsieur, oignon, ville/fille, loanwords), and it is strictly worse than a
lexicon for every word the lexicon knows. As the ONLY mechanism: poor ROI.
As a fallback for out-of-vocabulary words: exactly right, and it does not
need to be good, because OOV words are mostly names the player will tune by
ear anyway.

### Option D (recommended): hybrid lexicon + rule fallback + preview loop

Pipeline for a sentence typed in plain French:

1. Tokenize, lowercase, strip punctuation into pause tokens (`,` `.`).
2. Lexicon lookup per word (Option B data).
3. OOV words go through a small fallback rule set (50-100 rules, C-lite).
4. Liaison pass on a finite trigger list (les/des/ses/mes/nous/vous/ils/elles/
   on/en/un/deux/trois/est/ont/sont/tout + vowel-initial next word -> insert
   `z`/`n`/`t`). Skippable for v1: players can add `_z_` by hand.
5. Join words between pauses into breath groups with `_`, drop final schwas.
6. Write the result into the EXISTING phoneme InputField: editable, previewable
   with the existing play button. The converter is an assistant, not an
   authority; the phoneme string stays the single source of truth. No data
   model change, no new persistence.

UI: one "Texte -> phonèmes" button + a plain-text field on the synth screen.
Optional polish: highlight OOV words (rule-generated) so players know what to
check by ear.

Typos in player input: lexicon miss falls through to rules, which still emit
something plausible; the audio preview catches the rest. Fuzzy matching is
optional polish, not v1.

Effort estimate: offline dictionary build script (~100 lines python), C#
converter (~200-300 lines including liaison + fallback rules), UI wiring
(1 button + field). The 633 aligned corpus lines double as a regression
benchmark: convert every TX line, diff against the shipped PH line, track
token-level agreement while tuning. That makes converter quality a measurable
number instead of vibes.

### Option E: LLM-based conversion (shipped)

`.claude/skills/french-to-rody-phonemes/` in this repo: inventory, conversion
rules, curated corpus examples, and a token validator script. Serves story
authoring here and any player who clones the repo with Claude Code access.
Zero game code, best conversion quality (handles context, style, rhythm), but
requires third-party tooling: it complements D, it does not replace it.

### Option F: external converter web page/service (rejected)

A JS port hosted next to the GitHub Pages build could reuse the same
dictionary, but adds a second implementation and a hosting/dev surface for no
benefit over doing it in C# inside the editor, which already runs on WebGL.

### Recommendation and phasing

1. Done now: skill (Option E) for authors and Claude-equipped players.
2. v1 in-editor (Option D minus liaison): dictionary lookup + breath-group
   join + schwa drop + editable output. Smallest shippable version of the
   golden UX. Prerequisite: confirm lexicon license.
3. v2: liaison pass, OOV fallback rules, OOV highlighting.
4. Optional: corpus regression harness to CI, and fix the 11 corrupt phoneme
   lines in `original-stories` (finding 1).
