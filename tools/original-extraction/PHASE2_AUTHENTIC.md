# Phase 2 goal: FULLY AUTHENTIC replication of Rody 1 speech

Phase 1 DONE (see PROGRESS.md / DECODED.md): data-only decoder renders all 102
dialogues, mean STT similarity 0.572 >= 0.55. Understandable but NOT bit-exact
to the original — some vowel variants differ. Phase 2 = close that gap to an
authentic match. Verify by WAVEFORM comparison vs emulator ground truth, not
just STT.

## Remaining work, ranked by fidelity impact

1. **Bank-4 diphone coarticulation onsets** (biggest lever). The original emits
   `[04][current_P][previous_P]` transition grains between phonemes; our
   interpreter SKIPS bank 4 (render_all.py `op==0x04: i+=3`) and the preprocessor
   doesn't emit them. Decode the interpreter's bank-4 two-level path (b3b6e:
   reads P via lsl#2 index into subtable @clipTable+0x45c, then a 0x38-stride
   entry) and the preprocessor onset emission, then render them. These blend
   adjacent phonemes and largely explain the "different vowel" perception.

2. **Preprocessor onset/coarticulation bug**. preprocess.py matches the ground-
   truth command stream for the opening then drifts (missing the `pit00 ph4:PxX`
   onsets). Onset = `[04][cur_P][prev_P]` diphone (confirmed from GT pattern).
   The b41ea `s[1]>=5` condition (fires per disasm when prev is consonant) reads
   OPPOSITE to observed (onset before vowels) — resolve the stage-offset / type
   inversion. Ground truth: /tmp/rod_banks/allreads.txt (dlg000 command stream).
   Validate: preprocess(dlg000) must reproduce allreads.txt EXACTLY (472 cmds).

3. **Pitch-bend (0x61) + speed (0x66)**. Currently parsed but IGNORED in
   render_all.py. The original warps sample rate / interpolates per these — this
   is likely the short/long and é/è/eu distinctions the user hears. Implement
   the sample loop's pitch-bend (b3bd0: subi #0x80, asr, add/sub via 4a9c/4a9e)
   and per-command speed (4a9a inter-sample delay).

4. **Complete phonetic labels for all units (goal B)**. Use audition.py catalog +
   user's ear. Banks 2 & 6 carry MOST phonemes (bank6 P15, bank2 P10 dominate),
   NOT bank 0. Fill descriptors_index.tsv 'guess' column with user feedback,
   then rewrite labels.tsv for all ~137 units. NOTE: naive "bank0 P=anchor" is
   WRONG (P4 is a dead/padding slot); real phonemes are in banks 2/6 + higher P.

## Tools ready
- `audition.py` — render/play any grain: `python3 audition.py one <bank> <P>` (auto
  afplay), `grains`, `descriptors`. Catalog in `catalog/` + index TSVs.
- `preprocess.py` `render_all.py` `score.py` — pipeline + metric.
- `DECODED.md` `preprocessor_disasm.txt` — full spec + annotated disasm.
- Emulator ground truth: rebootable via /tmp/rod_banks/rody1.st (Hatari recipe in
  PROGRESS.md ~01:55); capture command streams via bp on the 11 (a5)+ read PCs;
  capture reference AUDIO via `--sound 25066 --avirecord` + ffmpeg.

## Authenticity metric (stricter than phase-1 STT)
Capture the original's per-dialogue audio from the emulator, render the same
dialogue from data, and compare waveforms (cross-correlation / spectral distance)
+ have the user A/B listen. Target: indistinguishable.

User (2026-07-21): "if you have the original phonemes, we can rebuild them, i can
help finding the good ones if I can test the variants" — the audition workflow is
set up for this collaborative labeling.
