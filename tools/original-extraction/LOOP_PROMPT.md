# Autonomous loop prompt: reverse-engineer Rody & Mastico speech synthesis

GOAL: (A) crack the token->grain mapping of the original Rody & Mastico
(Atari ST, Lankhor 1988) speech engine so each of the 102 Rody 1 dialogues
renders to audio from data alone; (B) produce the complete unit->phoneme label
table for the Rody 1 bank.

READ FIRST, IN ORDER:
1. tools/original-extraction/PROGRESS.md - current state + ruled-out
   hypotheses (append your iterations there; never retry ruled-out ideas as-is)
2. tools/original-extraction/FORMAT.md - decoded container format
3. tools/original-extraction/record_tokens.txt - all 102 decoded dialogue
   token streams (the input whose mapping to sound is the open problem)

SUCCESS METRIC (check every iteration, never claim success without it):
Render all 102 dialogues to wav (u8 PCM grains, s16 = (byte-128)*256,
26000 Hz for rody1 - NOT 13000; see PROGRESS.md rate finding). Transcribe each:
  whisper-cli -m ~/voice-agent/models/ggml-large-v3.bin -f <wav> --language fr \
    -t 8 --no-timestamps -otxt -of <out>
Compare transcripts against the known texts (## texts sections of
"original-stories/Rody Et Mastico/levels.rody") with normalized word-sequence
similarity (difflib ratio, lowercase, accents stripped), best 1:1 assignment
(dialogue order may differ from scene order; use greedy/Hungarian matching).
TARGET: mean similarity >= 0.55 (the remake's own renders score ~0.69, so
0.55 proves correct decoding). Whisper hallucinations on noise ("Sous-titrage
Société Radio-Canada", "Merci d'avoir regardé...", "Sous-titres par ...",
"Abonnez-vous") score 0. Secondary: labels.tsv covering all 137 units, agreeing
with the ear-verified anchors (units 0..9 = i, é, a, o, ou, u, eu, in, an, on).
ON SUCCESS: write DECODED.md (byte-level mapping spec), labels.tsv,
dialogues/*.wav + *.txt under tools/original-extraction/, append the final
PROGRESS.md entry, STOP.

ASSETS:
- Banks: tools/original-extraction/banks/*.ROD (all six; rody1_PA.ROD is the
  target). Slicer: tools/original-extraction/slice_exact.py (validated).
- Original executables + disks: "/Users/bretzelstudio/Downloads/
  Steem.SSE.3.7.3.Win32/PRG/" - fichiers/RODY_1.PRG (main program, compiled
  code + a6 runtime library), AAA.PRG, disk images Rody_2.st, "RODY 3.ST",
  Rody&Mas6.st, RodyNoel.st, banks/Rody_5.st (decompressed from MSA). The
  fichiers/ + "fichiers originaux"/ folders hold loose game files incl. all
  .ROD data. TOS ROMs for emulation: tos102fr.img, tos162fr.img in the same
  Steem folder.
- Ear-verified ground truth (Arthur): unit order and trio structure in
  FORMAT.md / PROGRESS.md. The Unity project's clips (Assets/Sounds/
  Phoneme_v2) are hand-cut approximations - NEVER ground truth.

LEVERS, ranked (1 is the expected winner):
1. EMULATOR TRACING (installed: hatari 2.6.1 at /opt/homebrew/bin/hatari).
   Boot a Rody disk in Hatari with the French TOS
   (hatari --tos ".../tos102fr.img" --disk-a <image.st>), reach speech
   playback, and use the built-in debugger (AltGr+Pause or --debug; hatari
   debugger supports CPU breakpoints, memory watchpoints 'w', tracing
   --trace cpu_disasm, and memory dumps). Strategy: find where PA.ROD is
   loaded in RAM (watch GEMDOS Fread or search RAM for the known bank bytes),
   set a read watchpoint on the record region and on the audio region, trigger
   one dialogue, log which record bytes are read and which audio offsets get
   consumed in what order/repetition. ONE traced dialogue = the mapping solved
   by alignment; then verify with the STT metric over all 102. Headless/
   scriptable: hatari accepts --debug-except and debugger command files
   (--parse <file>); screenshots via --screenshot; you may also drive it via
   its FIFO control socket (hatari --control-socket). Copy-protection note:
   the .st images boot the CRACKED versions (AAA.PRG swapped) except possibly
   some; if a disk refuses to boot, try another episode - the engine is shared.
   Rody 1 itself exists only as loose files in fichiers/: you can build a
   bootable disk by copying them onto a blank .st (Blank Disk.st in the Steem
   folder, mountable FAT12) - or run RODY_1.PRG from a GEMDOS hard-disk drive
   (hatari --harddrive fichiers/ boots TOS with the folder as drive C:).
2. GHIDRA DECOMPILATION (installed: 12.1.2, headless at /opt/homebrew/Cellar/
   ghidra/12.1.2/libexec/support/analyzeHeadless). Import RODY_1.PRG as raw
   binary, language 68000:BE:32:default, load at base 0 with the GEMDOS header
   stripped (text starts at file offset 0x1C) or write a small loader script.
   Identify the runtime library (strings; 1988 French dev - candidates: GFA
   BASIC, STOS, OSS Personal Pascal, Megamax C). Find the digi-play routine
   (pokes $FF8800 via helper with d3=0x88000000) and decompile the accessor
   that turns a record token into (sample pointer, length, repeats).
3. STATISTICAL CIPHER: 74 phoneme-id values with Zipfian distribution vs
   French phoneme frequencies computed from the remake corpus (all phonems
   lines in original-stories/*/levels.rody). Rank-align ids to phonemes,
   assign grains via the ear-verified unit order + spectral class (vowel trio
   vs noise solo), render, STT, hill-climb the permutation on the similarity
   score. Slow but fully autonomous; combine with anchors to cut the space.
4. CROSS-BANK DIFF: the five family banks share the format; a dialogue with
   identical text across two episodes may expose encoding invariants.

DISCIPLINE:
- Each iteration: hypothesis -> implement -> render -> STT -> append to
  PROGRESS.md (hypothesis, evidence, measured score, verdict). Append-only.
- Never claim success without the metric. Shipping noise labeled as speech is
  the worst outcome. Honest blockers are good outcomes.
- Do not modify the Unity project. Do not commit to git. Keep outputs under
  tools/original-extraction/ and /tmp/rod_banks/.
- If Hatari needs interactive fiddling that truly cannot be scripted, document
  the exact manual steps needed in PROGRESS.md and continue with levers 2/3.
- Sessions inheriting this loop: read PROGRESS.md first, continue from the
  best known state.
