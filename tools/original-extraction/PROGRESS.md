# Reverse-engineering progress log (append-only)

Read this first. Continue from the best known state; never retry ruled-out
hypotheses as-is. Each entry: hypothesis, evidence, measured score, verdict.

## 2026-07-21 state snapshot (start of autonomous loop)

SOLVED (validated, do not redo):
- PA.ROD container format: see FORMAT.md. Slicer: slice_exact.py (byte-exact:
  concatenating all 251 grains reproduces the audio region exactly).
- PA.ROD audio (~7.5 s) is a PHONEME DICTIONARY of 251 grains, not dialogue
  audio. Dialogues are synthesized by concatenating grains (like the remake).
- Grain structure: 57 trios (attack + loopable middle + release, contiguous,
  lengths L>=M>=S) + 80 solos = 137 units. Human ear-verified: unit order
  starts i, é, a, o, ou, u, eu, in, an, on (sustained vowels); solos = noise
  consonants; later series = short/sharp vowel variants; several 'o' variants.
- Record region = per-dialogue u16 BE token streams (record_tokens.txt has all
  102). High byte: 74 distinct values, Zipfian => phoneme id. Low byte: 137
  distinct values => variant/pitch/duration selector.
- Rate 13000 Hz confirmed for rody1 (raw-bank STT French-ness at 13000 beats
  10000/26000). rody1 grains are 2.0x the family banks' sample counts.

RULED OUT for the token->grain mapping (all rendered to whisper-noise;
"Sous-titrage...", "Merci d'avoir regardé...", "Sous-titres par Jérémy Diaz"
are hallucinations on noise = score 0):
- token (full/high/low/mod N/bit-slices >>5 >>6 >>7 &0x1FF &0x3FF) as index
  into: distinct-sorted grains, full 644-slot table, dense-only, reversed.
- token or 4-byte-pair fields as audio byte offsets (absolute, table-end
  relative, dictionary-relative bases 40000-43000), fixed durations 200-800,
  snap-to-grain-boundary variants.
- high byte as index into the 137 UNITS (trio expanded attack+mid*loops+release,
  low byte = loop count 1..6), offsets 0/1/2, dialogues 0/6/25.
- Remake corpus alignment as decoder key: remake levels.rody is an approximate
  transcription (24 scene blocks vs 102 original dialogues; token counts do
  not align under any shift; mean |delta| 25-33). Usable for STT comparison of
  RENDERED output, NOT as a structural key.
- RODY_1.PRG linear disasm (capstone): playback goes through a compiled-runtime
  a6 library (jsr -offset(a6)); YM digi confirmed (helper pokes $FF8800,
  d3=0x88000000) but token->pointer arithmetic not visible. rody1_disasm.txt
  in /tmp/rod_banks/ if still present (regenerable with capstone).

OPEN: the token -> grain mapping (the only missing piece for goal A).

## 2026-07-21 agent final report (pre-loop)

- RATE SOLVED: rody1 plays at ~26000 Hz; family banks at ~13000. Evidence:
  clip-length ratio exactly 2.000 (median, 227 clips, 100% in 1.9-2.1); voice
  pitch rody1@13000 = 121 Hz = half of rody2@13000 = 241 Hz; rody1@26000 =
  243 Hz matches. Not hardware-confirmed; rests on same-voice-across-episodes.
  Slicing boundaries unaffected. RENDER RODY1 AT 26000 Hz.
- Disassembly route CLOSED: RODY_1.PRG is compiled BASIC (GFA-style); a6
  runtime base = text+0x2134 (so jsr -X(a6) targets text+0x2134-X - useful for
  Ghidra). The earlier "YM digi driver" at ~0x6d00 was the float-multiply
  library (0x88000000/0x9f800000 are float constants); the 0xFF8900 hit was
  mid-instruction. No inline record-parser/digi-player exists; everything is
  behind hundreds of library dispatch targets.
- Additional ruled-out mappings (rendered dlg 0/6/25, whisper noise): grain-
  FAMILY indexing natural + reversed; all bit-slice/mask index variants into
  distinct/644/dense/reversed arrays; token-as-offset with bases 40-43k.
- CONCLUSION: phoneme-id -> grain permutation is arbitrary and unrecoverable
  from bytes alone. THE UNBLOCK: one ground-truth pair from emulation - trace
  which PA.ROD audio offsets are consumed while one known dialogue plays
  (Hatari watchpoint on the loaded bank), then solve by alignment. Lever 1 of
  LOOP_PROMPT.md. With one pair, render all 102 and score.

## 2026-07-21 ~00:30 emulation session state (lever 1, in progress)

- Hatari 2.6.1 RUNNING (relaunch cmd if dead):
  /opt/homebrew/Cellar/hatari/2.6.1/Hatari.app/Contents/MacOS/hatari \
    --machine ste --monitor rgb \
    --tos /Users/bretzelstudio/Downloads/Steem.SSE.3.7.3.Win32/tos162fr.img \
    --harddrive /Users/bretzelstudio/Downloads/Steem.SSE.3.7.3.Win32/PRG/fichiers \
    --auto 'C:\RODY_1.PRG' --sound off --fast-boot on --trace gemdos \
    --trace-file /tmp/hatari_trace/trace.log --screenshot-dir /tmp/hatari_trace \
    --screenshot-format png --confirm-quit false --cmd-fifo /tmp/hatari_trace/fifo
- FIFO protocol LEARNED: commands are hatari-debug <cmd>, hatari-event <ev>,
  hatari-shortcut screenshot (NOT "screenshot"/"hatari-screenshot"), hatari-stop,
  hatari-cont. Newline-separated into /tmp/hatari_trace/fifo.
- Game boots via GEMDOS HD (drive C:), auto-runs RODY_1.PRG, shows title screen.
- Bank load addresses from gemdos trace (this boot): SONROD.ROD -> Fread buf
  0xc1914, DP.ROD -> 0xbaf98. PA.ROD NOT loaded yet (waiting at title).
- Plan: advance UI until PA.ROD Fopen/Fread appears in trace -> gives PA.ROD RAM
  base; then hatari-debug watchpoints on record region + audio region of the
  loaded bank while one known dialogue plays; log consumed offsets; solve
  mapping by alignment.

## 2026-07-21 ~00:40 findings + blocker

- KEY SHORTCUT: gemdos trace gives bank RAM buffers DIRECTLY. Loader routine is
  generic: Fopen at PC 0x13762, Fread(fd, len, BUF) at PC 0x1389E. This boot:
  SONROD.ROD, DP.ROD -> buf 0xbaf98. So when PA.ROD finally loads I read its
  buffer address straight from trace.log - NO watchpoint needed to locate it.
  Then set access watchpoints on [PA_buf .. PA_buf+len] to log consumed offsets.
- Memory region 0xB31B0-0xB3230 (where CPU idles): hardware DMA routine writing
  a0=$FFFF8604/8606 (STE DMA disk mode+data regs) with #$84,#$90,#$190,#$80;
  ends in poll loop `btst #5,$FFFA01; bne` (MFP GPIP5 = FDC/DMA int wait). This
  is disk-DMA sector I/O, NOT the digi-sound player. Not the target routine.
- BLOCKER: title screen (robot + rainbow) won't advance. Hatari FIFO event
  injection only supports doubleclick + rightdown/up (NO positioned left-click,
  NO mouse-move); game is mouse-driven point-and-click. keypress 57/28 and
  doubleclick had no effect. PA.ROD never loads while stuck at title.
- NEXT ANGLE: drive the real Hatari macOS window via osascript (System Events)
  now that screen-capture/accessibility perms are granted - move mouse + real
  left click to advance title, reach gameplay, trigger a dialogue.

## 2026-07-21 ~00:50 ROOT-CAUSE of title hang (important)

- CPU trapped in tight busy-wait at 0xB3212-0xB3228:
  b3210: nop x7 ; b3220: btst #5,$FFFA01 ; b3228: bne .-24 ; b322a: rts
  Sampled PC twice 1.5s apart: 0xb3218 -> 0xb321a, D0-D3 unchanged. Genuine
  hang, not an input-wait (input-wait would vary regs / call GEMDOS).
- $FFFA01 bit5 = MFP GPIP5 = FDC/DMA interrupt (active low). Loop waits for it
  to go low = disk-DMA completion. Under Hatari --harddrive (GEMDOS HD), the
  low-level FDC is NOT emulated, so the completion IRQ never fires => infinite
  hang at title. Known Hatari limitation: programs doing direct FDC access
  need a REAL floppy image, not GEMDOS HD.
- Neither keyboard (scancode 57/28) nor mouse (cliclick real left-click at ST
  screen center via osascript-focused window) advanced it - consistent with a
  hard hang, not missed input.
- FIX TO TEST: boot from an actual .st floppy image (--disk-a) instead of
  --harddrive. Decisive cheap experiment: boot existing Rody_2.st from floppy;
  if it reaches gameplay, GEMDOS-HD was the culprit. Then build a rody1 floppy
  (mtools/mcopy fichiers into Blank Disk.st) or trace rody2's shared engine.

## 2026-07-21 ~01:00 BREAKTHROUGH: floppy boot works, game launches

- Confirmed FDC hypothesis: booting Rody_2.st via --disk-a (NOT --harddrive)
  reaches the GEM desktop, then double-clicking RODY_2.PRG launches the game.
  GEMDOS-HD boot was the hang cause. Working launch recipe:
  hatari --machine st --monitor rgb --tos tos102fr.img --disk-a Rody_2.st \
    --sound off --fast-boot on --cmd-fifo /tmp/hatari_trace/fifo \
    --screenshot-dir /tmp/hatari_trace --screenshot-format png
  IMPORTANT: create fifo ONLY by letting hatari make it (prw pipe); never
  printf to the path before hatari opens it (turns it into a regular file that
  hatari ignores). Verify `ls -la fifo` shows leading 'p'.
- Drive the macOS window: osascript focus process "hatari" + cliclick.
  Window geom via System Events position/size of window 1. Double-click file
  icon: cliclick c: to select then dc: to open. Icon RODY_2.PRG at screen
  ~(1017,672) for window at (612,364) 832x616.
- Reached EMPIRE/ARIOCH cracktro "RODY AND MASTICO II". Next: advance cracktro
  -> game -> trigger a dialogue while a memory watchpoint logs PA.ROD grain
  reads. For rody1 target, apply same recipe to a rody1 floppy (build from
  fichiers via a bootable image) OR trace rody2 shared engine to get algorithm.

## 2026-07-21 ~01:15 emulator reaches gameplay + capture plan

- Rody 2 flow: cracktro -> (click/space) -> disk load (status RS:80) -> ingame
  title (Rody on bike) -> scene-selection GALLERY (grid of scene thumbnails).
  Clicking a thumbnail enters a scene and should trigger speech. We are AT the
  gallery now (grab0012).
- Hatari debugger (via `hatari-debug <cmd>` to fifo; output in stdout2.log):
  - `b <cond> [:trace]` conditional breakpoints. Cond syntax:
    value[.b|.w|.l] [& mask] <|>|=|! value. Parenthesis = deref memory.
    '!' inequality TRACKS ALL CHANGES to an addr/reg (watchpoint!). e.g.
    `($ffff8802).b ! ($ffff8802).b :trace` logs every change to YM data port.
  - `a <addr>` PC breakpoints; `history`/`hi` last PCs; `d <addr>` disasm;
    `m <a>-<b>` memdump; `r` regs; `t` trace settings.
- TWO capture strategies (next iteration, decisive):
  A) GROUND-TRUTH AUDIO: relaunch hatari with --sound on and record YM output
     to WAV while triggering each dialogue -> real original speech per dialogue.
     Achieves 'render dialogues to audio' + gives waveform to align vs the 251
     bank grains => recover grain sequence => align to token stream => MAPPING.
     Check `hatari --help | grep -i 'sound\|record\|wav\|ym'` for the record opt
     / shortcut name.
  B) TRACE YM WRITES: set `($ffff8802).b ! ($ffff8802).b :trace` (+ reg-select
     $ffff8800), trigger ONE short dialogue, parse trace.log value stream =
     played PCM bytes; align to grains. Music contaminates; strategy A cleaner.
- TARGET is rody1: once the shared algorithm/audio-capture works on rody2,
  build a rody1 floppy from fichiers (needs mtools: `brew install mtools`,
  mcopy into a copy of 'Blank Disk.st') OR find rody1's original .st layout.

## 2026-07-21 ~01:25 YM-write trace confirmed; decision: AUDIO CAPTURE route

- Live test (sound off): `b ($ffff8802).b ! ($ffff8802).b` matched 18x within a
  frame at the gallery -> YM data port written continuously by MUSIC/TOS even
  with --sound off (CPU still executes IO writes; breakpoint fired inside TOS
  ROM handler PC=$fc1e82). So register-stream tracing CANNOT cleanly separate
  speech from music. Strategy B (parse YM writes) DEPRECATED for isolation.
- DECISION: capture emulator AUDIO per dialogue (ffmpeg present). Two ways:
  1. AVI: relaunch with `--sound 25066 --avirecord --avi-file /tmp/hatari_trace/
     rody.avi --avi-vcodec png`, navigate to a scene, let dialogue play, quit;
     `ffmpeg -i rody.avi -vn out.wav`. Audio track = original speech.
  2. `recsound` shortcut toggles a WAV (path via GUI/config; unverified in
     headless - prefer AVI which is fully CLI-specified).
- NEXT ITERATION EXACT STEPS (rody2 first to prove, then rody1):
  a. pkill hatari; rm fifo; relaunch WITH SOUND:
     hatari --machine st --monitor rgb --tos tos102fr.img --disk-a Rody_2.st \
       --sound 25066 --avirecord --avi-file /tmp/hatari_trace/rody2.avi \
       --avi-vcodec png --fast-boot on --confirm-quit false \
       --cmd-fifo /tmp/hatari_trace/fifo --screenshot-dir /tmp/hatari_trace
  b. Navigate (cliclick, re-query window geom each run):
     ~10s boot -> double-click RODY_2.PRG icon -> cracktro -> click + keypress
     57 -> ~14s disk load -> ingame title -> (click) -> gallery grid.
  c. Click ONE scene thumbnail; let speech play ~4s; `hatari-shortcut quit`.
  d. ffmpeg -i rody2.avi -vn -ac 1 -ar 25066 /tmp/rod_banks/rody2_scene.wav
     -> STT with whisper-cli (fr) -> confirm it's real speech (proves capture).
  e. To hit the rody1 TARGET: build a rody1 floppy from fichiers (brew install
     mtools; format a copy of 'Blank Disk.st'; mcopy RODY_1.PRG PRES.PRG AAA.PRG
     *.ROD ::; boot it) OR locate rody1's original .st. Then capture all 102.
  f. MAPPING (goal A "from data alone"): with per-dialogue ground-truth WAV,
     cross-correlate each ~grain-length segment against the 251 sliced bank
     grains to recover the grain-index SEQUENCE, then align that sequence to the
     dialogue's record token stream (record_tokens.txt) => token->grain table.
- Emulator left RUNNING at rody2 gallery (breakpoints cleared, cont issued).

## 2026-07-21 ~01:45 VALIDATED pipeline + reassessment (wall on menu nav)

VALIDATED THIS SESSION (reusable, do not redo):
- Floppy boot to gameplay works (Rody_2.st). Recipe + fifo rules in the ~01:00
  and ~01:25 entries above.
- AUDIO CAPTURE pipeline works END-TO-END: --sound 25066 --avirecord
  --avi-file X.avi --avi-vcodec png; then
  `ffmpeg -y -i X.avi -vn -ac 1 -ar 22050 out.wav` yields real PCM
  (mean -31dB, max -4.9dB over 164s). whisper-cli large-v3 fr runs fine.
  Captured cracktro+title+gallery = MUSIC/SILENCE only; whisper returns the
  known noise-hallucination "Sous-titres par Jeremy Diaz" => NO speech was
  triggered yet (expected: those screens have no narration).
- ST mouse DOES map (host absolute -> ST cursor; saw arrow land on the clicked
  thumbnail). Desktop double-click launches PRG. BUT clicking Rody 2's scene
  gallery (20 thumbnails + 4 book icons) opens nothing - single & double click,
  grid thumbs and open-book icon all inert. This gallery may be a
  non-interactive montage or need an input I haven't found. ST TOS clock is
  frozen at 11:11:07 on this screen (game disables system clock; NOT a pause -
  PC sampling shows CPU advancing in game code ~0x14a62).

REASSESSMENT - abandon menu-clicking; two sharper angles for next iterations:
1. TARGET rody1 directly (stop using rody2). Build a bootable rody1 floppy:
   `brew install mtools`; cp 'Blank Disk.st' rody1.st (720K) or make 820K;
   `mcopy -i rody1.st fichiers/RODY_1.PRG fichiers/AAA.PRG fichiers/PRES.PRG
    fichiers/*.ROD ::`; boot with --disk-a rody1.st; double-click PRES.PRG or
   RODY_1.PRG. RISK: rody1 did direct FDC (the harddrive hang) - if it reads
   raw tracks / checks protection, a hand-built FAT12 floppy may not satisfy
   it. Mitigation: the fichiers include AAA.PRG (crack) which the harddrive run
   DID execute; test empirically.
2. DEBUGGER-DRIVEN speech (no mouse): the real goal-A artifact is the grain-
   offset SEQUENCE for one known dialogue. Once a bank is in RAM, set a memory
   READ breakpoint over the loaded PA bank region and let ANY one dialogue play;
   the ordered read addresses = grain sequence -> align to token stream = the
   mapping. Need to (a) get bank RAM base (gemdos trace gave it for
   harddrive-loaded .ROD; for floppy, search RAM for known bank header bytes or
   watch the loader), (b) find/trigger a scene that speaks. Consider: does the
   rody1 intro (robot) speak automatically? If yes, no menu nav needed.
3. Whichever path reaches speech: capture BOTH the AVI audio (ground truth WAV
   per dialogue) AND the grain-read trace; they cross-validate.

Emulator: Rody 2 left RUNNING at gallery (cont issued). AVI recording toggled
off. Big AVIs in /tmp/hatari_trace/ (rody2.avi 117MB) - safe to delete.
Extracted probe: /tmp/rod_banks/rody2_capture.wav (music, no speech).

## 2026-07-21 ~01:55 TARGET UNBLOCKED: rody1 runs from floppy (no FDC hang)

- Built bootable rody1 floppy from loose fichiers (mtools):
  export MTOOLS_SKIP_CHECK=1   # Atari media byte f0/f9 fails DOS check
  cp 'Blank Disk.st' /tmp/rod_banks/rody1.st
  mcopy -i /tmp/rod_banks/rody1.st fichiers/{RODY_1.PRG,AAA.PRG,PRES.PRG,
    DESKTOP.INF,SONROD.ROD,DP.ROD,INIT.ROD,MU.ROD,PA.ROD,DE.ROD} ::
- Boot: hatari --machine st --tos tos102fr.img --disk-a /tmp/rod_banks/rody1.st
  --sound off --fast-boot on ... -> GEM desktop shows RODY_1.PRG. Double-click
  it (icon ~screen 1017,672) -> RS:80 disk load, CLOCK TICKS (no hang!) ->
  RODY 1 title (robot+rainbow) at ~0A:0A:09. This is the SAME screen that hung
  under --harddrive; from floppy the FDC completes, so it loads normally.
  ROOT-CAUSE CONFIRMED: harddrive/GEMDOS-HD boot fails only because rody1 does
  direct FDC access; floppy fixes it. mtools-built FAT12 floppy DID satisfy
  rody1 (no raw-track protection blocking, or AAA.PRG crack handled it).
- NEXT: advance rody1 title -> reach speech, capture (AVI audio + grain-read
  trace). Relaunch with --sound 25066 --avirecord --avi-file for audio capture.
  PA.ROD (the target bank) is on the disk and will load into RAM during play;
  find its base (search RAM for PA.ROD header bytes) -> read-watchpoint ->
  grain sequence for one dialogue -> align to record_tokens.txt = MAPPING.

## 2026-07-21 ~02:05 REACHED rody1 GAMEPLAY + narration scene

- Full working path to rody1 speech scene (REPRODUCIBLE):
  boot rody1.st -> double-click RODY_1.PRG (1017,672) -> ~14s load -> title
  (robot+rainbow) -> click(1017,500)+keypress 57 -> ~14s load -> SCENE 1
  "DANS LA CHAMBRE DE RODY" with narration text box + 4 scene icons top-right.
  Text: "Rody, maman a ouvert doucement la porte de ta chambre et s'apprete a
  deposer un baiser sur ton front. Mais... elle ne te trouvera pas! Viens vite,
  le professeur Gobino nous attend." = a TARGET dialogue (spoken by phoneme
  engine). Matches record_tokens.txt content domain.
- This is the capture point. Two artifacts to grab here:
  (1) grain-read trace: find PA.ROD base in RAM, read-watchpoint the audio
      region, trigger speech (likely auto-plays on scene load or on click/icon),
      log grain offsets in order -> align to this dialogue's token stream.
  (2) ground-truth WAV: relaunch with --sound 25066 --avirecord, same nav,
      let narration play, ffmpeg extract.
- Emulator RUNNING at scene 1 (sound off, no AVI). Next: locate PA.ROD in RAM.

## 2026-07-21 ~02:15 PA.ROD located in RAM + refined winning plan

- PA.ROD found in RAM via Hatari `find`:
  `hatari-debug find $1000-$fa000 $00 $00 $00 $54 $00 $9c $00 $b8 $01 $1c $01 $d0`
  -> single match at 0x000D8858 = PA.ROD base (header start). Bank spans
  0xD8858 .. 0xD8858+0x1AC1E(=109598) = 0xF3476. Audio region = base + AUDIO
  offset (see FORMAT.md: header 0x128, then record region, then 644-entry u32
  clip table, then u8 PCM audio to EOF).
- Hatari CANNOT do a simple read-access watchpoint (conditional breakpoints are
  value/PC based; `w`=memwrite, `find`=search). Capturing grain-read order would
  require finding the digi-player loop (sample PC via `history` while speech
  plays, find loop reading 0xD8858+AUDIO writing $FF8800) then :trace its
  pointer reg. Doable but fragile.
- REFINED WINNING PLAN (robust, avoids player archaeology):
  1. Relaunch rody1 WITH sound+AVI:
     hatari --machine st --tos tos102fr.img --disk-a /tmp/rod_banks/rody1.st \
       --sound 25066 --avirecord --avi-file /tmp/hatari_trace/r1.avi \
       --avi-vcodec png --fast-boot on --confirm-quit false \
       --cmd-fifo /tmp/hatari_trace/fifo --screenshot-dir /tmp/hatari_trace
  2. Navigate to scene 1 (steps in ~02:05 entry). CONFIRM speech is capturable:
     ffmpeg extract -> whisper fr -> should transcribe near the known scene-1
     text ("...le professeur Gobino nous attend"), NOT the noise-hallucination.
     THIS validates that emulator narration audio = ground truth.
  3. Walk all scenes/dialogues, recording; segment the WAV per dialogue.
  4. OFFLINE mapping (goal A from data): cross-correlate each dialogue's audio
     against the 251 sliced grains (slice_exact.py) to get the grain-index
     SEQUENCE; align sequence to the dialogue's record token stream
     (record_tokens.txt) -> token->grain table (labels.tsv). The captured WAVs
     ALSO directly satisfy "render each dialogue to audio".
  5. Cross-check a couple dialogues with the grain-read trace if needed.
- STATE: rody1 emulator RUNNING at scene 1 (sound off). rody1.st floppy at
  /tmp/rod_banks/rody1.st is the reusable target image. PA base 0xD8858 valid
  only for THIS boot (re-find after each relaunch; ASLR-free but load addr can
  shift with TOS/alloc).

## 2026-07-21 ~02:30 *** METHOD VALIDATED: original speech captured & STT-matched ***

- Relaunched rody1 from floppy WITH sound+AVI, navigated to scene 1, clicked the
  scene to trigger narration, recorded, extracted audio, ran whisper-cli fr.
- RESULT (last 55s of capture, mean -28.8dB / max -13.7dB = real audio):
  whisper => "Ruby, maman a ouvert doucement la porte de ta chambre et
  s'apprete a deposer un baiser sur ton front. Mais, elle ne te trouvera pas."
  KNOWN scene-1 text: "Rody, maman a ouvert doucement la porte de ta chambre
  et s'apprete a deposer un baiser sur ton front. Mais... elle ne te trouvera
  pas! Viens vite, le professeur Gobino nous attend."
  => NEAR-VERBATIM MATCH (only 'Ruby'~'Rody' STT slip on the robotic voice;
  ~0.9 similarity, vs 0.55 target). This is GENUINE original-engine speech,
  not a whisper-noise hallucination. Saved: dialogues/dlg01_chambre.{wav,txt}.
- PROVEN: (1) the rody1 floppy I built runs the real engine; (2) it speaks the
  dialogues; (3) Hatari AVI capture -> ffmpeg -> whisper reproduces the known
  text. The whole capture chain works.
- HONEST SCOPING: emulator capture PRODUCES the 102 dialogue WAVs (satisfies
  "render each dialogue to audio" + gives ground-truth audio) but is NOT the
  "from data alone" decoder. It is the ground truth that finally lets us crack
  the token->grain mapping OFFLINE: cross-correlate each captured dialogue's
  audio against the 251 sliced grains -> grain sequence -> align to
  record_tokens.txt -> the data-only mapping (labels.tsv + DECODED.md).
- REMAINING WORK (mechanical):
  a. Capture all 102 dialogues: walk the game's scenes (scene has 4 nav icons
     top-right; each scene = one/few dialogues). Trigger narration per scene,
     record, segment per dialogue. Automate the nav+record loop.
  b. Offline: build cross-correlation aligner (audio segment -> grain indices),
     recover mapping, write DECODED.md + labels.tsv, render all 102 from data,
     STT, confirm mean sim >= 0.55.
- STATE: rody1 emulator still running at scene 1. r1.avi (78MB) in
  /tmp/hatari_trace. Validated WAV copied into repo dialogues/.

## 2026-07-21 ~09:20 engine internals (loop iter 2) + decision to go offline

- MFP vector table ($100-$140) read from RAM: Timer A vector ($134) =
  0x000b49dc points into GAME RAM. Disasm: it's the 3-VOICE YM MUSIC TRACKER
  (loops d1=0,1,2 over voice structs at 0xb6764/0xb67b4/0xb6804, music data
  a3=0xc1e8c, calls YM-update at 0xb4e84 = the 0xb4f00 routine seen earlier).
  So Timer A = MUSIC, NOT the digi speech player. Other game-pointing vectors:
  none obvious in $100-$13F (rest are TOS $fcxxxx).
- Hunt for digi decode via breakpoints `aN > $d8857 && aN < $f3477` (aN inside
  PA.ROD RAM 0xd8858..0xf3476) for a0..a6: NONE fired on click or nav. Likely
  causes: (1) narration decodes ONCE on scene ENTRY, before I armed bps;
  (2) decode may use indexed addressing `(base,dN.w)` so the base REGISTER
  stays below the range while the effective addr lands in it -> register-range
  bp can't catch it. Must arm bps BEFORE a fresh scene entry to test (1).
- Rody 1 is a children's ACTIVITY title: clicking scene nav icons (top-right
  2x2) opens a PAINT/COLORING editor (color palette + patterns + tools), not
  more speech. Speech = the narration text box shown on scene entry. So
  scene-to-scene navigation for 102 dialogues is non-trivial (modes, drawing).
- DECISION: the emulator reliably yields GROUND-TRUTH AUDIO (dlg01 validated
  ~0.9). Catching the token->grain decode interactively is fragile. Pivot the
  mapping work OFFLINE and autonomous: align captured dialogue AUDIO to the 251
  sliced grains to recover the grain sequence, then align to record_tokens.txt.
  Emulator role reduced to: (a) produce ground-truth WAVs, (b) optionally,
  arm PA-range bps BEFORE a scene entry to try catching the decode buffer.

## 2026-07-21 ~09:45 *** DIGI PLAYER FOUND + precise RAM map (loop iter 2) ***

- DIGI SPEECH PLAYER located: tight busy-loop `dbf.w d4,#-2` sample-delay loops
  at 0xb3a36 and 0xb3c7a (multiple = different pitch delays). a0 = SAMPLE
  POINTER sweeping the PA.ROD AUDIO region DIRECTLY (no separate work buffer!).
  Found via bp `aN > $d8857 && aN < $f3477 && pc<$c0000 && pc>$8000` armed at
  title BEFORE entering scene 1; fired in game code during narration.
  CAUTION: such a bp on the delay loop matches EVERY instruction (a0 & pc both
  in range) -> floods output, doesn't act as a clean stop. Use a narrower
  region (clip table) or breakpoint the clip-start load instruction instead.
- PRECISE PA.ROD layout (file + RAM this boot, PA base = 0xd8858):
    header      file 0x0000..0x0128
    record reg  file 0x0128..0x2560
    CLIP TABLE  file 0x2560..0x2f70  RAM 0xdadb8..0xdb7c8  (644 x u32 BE,
                offsets relative to audio start; 251 DISTINCT = real clips)
    AUDIO PCM   file 0x2f70..0x1ac1e RAM 0xdb7c8..0xf3476   (97454 bytes, u8)
  Clip i plays [audio_base + off[i], audio_base + off[i+1]); a0 walks it.
  First offsets: 0,541,980,1316,1860,2311,2634,3178,3632,3953,4490,4925,5277,
  5277,5277,5277(dupes=unused slots),5831,...
- MAPPING PLAN (definitive): the decode does a0 = audio_base + clipTable[idx],
  idx derived from the record TOKEN. Catch it: bp on a reg pointing into CLIP
  TABLE (0xdadb8..0xdb7c8) with pc in game code -> that PC is the decode; disasm
  to read the token->idx formula. OR log the sequence of a0 clip-START values
  for dialogue 1 -> map offsets to clip indices -> align to record_tokens.txt
  dialogue-1 token stream = solves token->clip. Emulation currently STOPPED,
  bps cleared, at scene 1 (narration may have finished; re-trigger by re-entry).

## 2026-07-21 ~10:10 *** TOKEN->GRAIN MAPPING CRACKED (offline disasm of AAA.PRG) ***

- The digi player + record interpreter is in AAA.PRG (resident engine), file
  offset 0x1822 == RAM 0xb3a08. Disassembled offline with capstone (reliable;
  FIFO disasm was flaky). Full logic recovered:
- RECORD STREAM IS BYTE-ORIENTED (NOT u16 tokens!). This is why every prior
  u16/high-byte/low-byte indexing was ruled out. Interpreter loop @0xb3990:
    d0 = *a5++  (a5 = record stream ptr)
    0x61 'a': next byte sets PITCH-BEND flags (4a9c/4a9e = +/-1/0 per half)
    0x66 'f': next byte sets SPEED (4a9a inter-sample delay; 4cc2 base)
    0x20 ' ': word-gap pause (delay #$245e)
    0x2e '.': sentence pause (delay #$183ee)
    0x00/0x02/0x04/0x06: phoneme-bank selector (see below)
  PHONEME PLAY (cmd 0x00 @0xb3a6e, the common case):
    d5 = *a5++            ; phoneme index P
    d0 = *a5++            ; variant V
    a0 = [4cc8]           ; = clipTable base (RAM 0xdadb8; [4cc8]+0xa10 = audioBase 0xdb7c8)
    desc = a0 + P*12      ; 12-byte descriptor = clip-offset entries [3P .. 3P+3]
    start = desc[+0], end = desc[+0xc]   (u32 offsets rel to audioBase)
    variant V overrides start/end field pick:
      V=0: start=+0  end=+0xc   (whole trio: off[3P]   .. off[3P+3])
      V=1: start=+0  end=+8     (off[3P]   .. off[3P+2])
      V=2: start=+4  end=+0xc   (off[3P+1] .. off[3P+3])
      V=3: start=+0  end=+4     (off[3P]   .. off[3P+1])  attack only
      V=4: start=+4  end=+8     (off[3P+1] .. off[3P+2])  middle only
      V=5: start=+8  end=+0xc   (off[3P+2] .. off[3P+3])  release only
    play audio[audioBase+start .. audioBase+end], sample loop @0xb3bd0:
      s=*a0++; s-=0x80; pitch-bend via 4a9c/4a9e (add/sub s/2); s+=0x80; clamp;
      write 3 YM volume regs (movep to a4=$FF8800) using vol table @0xb3c82 +
      (a1) tables => 3-channel digi. Next sample after 4a9a-delay until a0>=end.
  ALT BANKS: cmd 0x02 -> desc base +0x10c; 0x06 -> +0x2b4; 0x04 -> +0x45c with
    stride 0x38 (different structure). These are secondary phoneme pages.
    d5<<3 alt-table path at 4d22 when 4d20!=0 (mode flag).
- => descriptor P uses clip-offset entries 3P,3P+1,3P+2,3P+3 = the ear-verified
  "trio" (attack+middle+release). clip table = 644 u32 offsets = ~214 trios.
- THIS IS A DATA-ONLY DECODER. To render dialogue i from PA.ROD alone:
    record[i] = PA[0x128+hdr[i] : 0x128+hdr[i+1]]  (raw BYTES)
    walk bytes as above; for phoneme (P,V) emit audio[audioBase+off[3P+s]..off[3P+e]]
    concat; u8 PCM @ ~26000 Hz (rody1). Pitch-bend/speed optional for fidelity.
- NEXT: implement renderer, render all 102, STT, verify mean sim >= 0.55.
  Cross-check vs the emulator ground-truth dlg01 already captured.

## 2026-07-21 ~10:35 KEY CORRECTION: interpreter reads a PREPROCESSED stream

- Ran a faithful sim of the byte interpreter on dlg0 RAW record bytes: reading
  3 bytes/phoneme ([cmd][P][V]) desyncs immediately -> ~all variants invalid
  (V=0xe5,0x11,0x9c...). The raw record is u16-framed (high byte=P: 26,66,17,
  27,146...) but the interpreter is byte-wise and consumes 3 bytes/phoneme.
  These are INCOMPATIBLE => the interpreter does NOT read the raw record region.
- EVIDENCE: at the digi breakpoint, A5 = 0x000BF20C (NOT the PA.ROD record
  region 0xd8980). So a5 = a PREPROCESSED interpreter-command buffer at ~0xbf20c.
  The raw u16 record (PA 0x128+) is expanded by a PREPROCESSOR into the byte
  command stream ([0x00][P][V], 0x20/0x2e pauses, 0x61/0x66 pitch/speed) that
  the interpreter at 0xb3990 consumes.
- So the FULL data-only decode = PREPROCESSOR (u16 record -> byte commands) +
  INTERPRETER (byte commands -> clip offsets, ALREADY DECODED @0xb3990 spec
  above) + audio slice. The interpreter half is solved; the preprocessor is the
  remaining unknown.
- NEXT: (a) dump RAM at a5 buffer 0xbf20c..~0xbf400 during a dialogue and DIFF
  against that dialogue's raw u16 record -> reveals the expansion rule (likely
  each u16 token -> a small fixed command group; low byte encodes variant+pitch
  that expands to 0x61/0x66 + variant). (b) OR find/disasm the preprocessor (the
  code that reads PA record u16 and writes 0xbf20c). Search AAA.PRG for a loop
  reading the record region and writing the command buffer.
- FALLBACK REMAINS VALID: emulator AVI audio capture already renders dialogues
  to ground-truth WAV (dlg01 validated ~0.9), independent of this decode.

## 2026-07-21 ~10:50 iter-2 wrap (emulator FIFO stuck; big net progress)

- Hatari debugger FIFO went unresponsive after the breakpoint-flood (commands
  no longer consumed; stdout frozen). NEXT ITER: pkill hatari + relaunch fresh
  (recipe in ~01:55 entry) before any more live inspection.
- NET PROGRESS THIS ITER (all offline-reproducible, high confidence):
  * Digi speech player FOUND: busy-loop @0xb3a36/0xb3c7a, a0 sweeps PA audio
    region directly; 3-channel YM-volume digi (movep to $FF8800) w/ pitch bend.
  * Record INTERPRETER fully disassembled offline from AAA.PRG (file 0x1822 ==
    RAM 0xb3a08); clip-selection logic SOLVED (descriptor = 12B = clip-offset
    trio 3P..3P+3; variant byte V picks start/end among +0/+4/+8/+0xc). Spec in
    ~10:10 entry.
  * PA.ROD precise layout (file+RAM) in ~09:45 entry.
- REMAINING UNKNOWN (the one gap to a data-only renderer): the PREPROCESSOR that
  turns the raw u16 record (PA 0x128+, high byte=P) into the interpreter's byte
  command stream at a5=0xbf20c. Interpreter does NOT read raw record (proven by
  sim: 3B/phoneme desyncs the u16 framing).
  NEXT-ITER PLAN to crack it:
   1. Fresh emulator; arm bp at digi (before scene entry); enter scene 1.
   2. When stopped in player, dump RAM 0xbf200..0xbf400 (a5 buffer) AND identify
      which dlg is playing; diff buffer vs that dlg's raw u16 record -> expansion
      rule. Likely each u16 -> [cmd,P,V(,pitch)] where low byte splits into
      variant + pitch/speed (0x61/0x66) commands.
   3. Implement preprocessor+interpreter in Python; render all 102; STT; verify
      mean sim >= 0.55. Cross-check vs emulator ground-truth dlg01 wav.
  ALT if preprocessor stays stubborn: dump each dlg's 0xbf20c buffer via emulator
  (interpreter input) and render via the solved interpreter -> still needs emu
  per dialogue but yields data-checkable clip sequences.
- FALLBACK (already works): emulator AVI audio capture renders dialogues to
  ground-truth WAV (dlg01 validated). Satisfies the render+STT success metric
  even if the pure preprocessor stays unsolved.

## 2026-07-21 ~11:30 iter-3: COMMAND GRAMMAR captured (interpreter input decoded)

- Captured the interpreter INPUT command stream by a plain bp `b pc=$b3990`
  (it auto-continues after printing in cmd-fifo mode!): each hit prints
  `move.b (a5)+ == $ADDR [BYTE]` = the command opcode + its buffer address.
  472 commands captured for scene-1 narration. Saved /tmp/rod_banks/cmdstream.txt
- COMMAND GRAMMAR (byte consumption from a5-address gaps between opcodes):
    0x20  ' ' word-gap pause      : 1 byte
    0x2e  '.' sentence pause       : 1 byte
    0x61  'a' pitch-bend set       : 2 bytes (op + 1 param)
    0x66  'f' speed set            : 2 bytes (op + 1 param)
    0x00/0x02/0x04/0x06 phoneme    : 3 bytes (op=bank + P + V)
    (0x23 appears 3x - TBD)
  Opcode histogram (472): 02:105 61:100 66:73 00:73 06:55 04:49 2e:10 20:4 23:3
  => 282 phoneme commands (banks 0/2/4/6) for scene-1 dialogue.
- CRITICAL: the a5 buffer (0xbf204) is OVERWRITTEN after playback (reused for
  other data: dump shows 5b b5 d5 3f ff... not the commands). So the command
  stream MUST be captured live via breakpoints, NOT dumped statically. b3990
  only exposes the OPCODE byte; operand bytes (P,V,params) are read at other PCs
  (b3a70=P, b3aaa=V for bank0; b39a4=pitch param; b39f4=speed param;
  b3b34/b3b3a=bank2; b3b52/b3b58=bank6; b3b7e/b3b92=bank4).
- NEXT: restart; set bps at ALL (a5)+ read PCs; one scene-1 narration -> merge
  all (addr,byte) by addr = COMPLETE command buffer incl operands. Then:
  (a) decode via solved interpreter -> clip sequence -> render audio (data+this
      capture). (b) align P-sequence to a raw record -> crack the preprocessor
      (raw u16 -> command expansion) for a fully static data-only renderer.

## 2026-07-21 ~12:00 *** INTERPRETER DECODE VALIDATED FROM DATA (~0.95) ***

- Captured the FULL command stream (opcodes+operands) for scene-1 by bp'ing all
  11 (a5)+ read PCs (b3990,b39a4,b39f4,b3a70,b3aaa,b3b34,b3b3a,b3b52,b3b58,
  b3b7e,b3b92); 1209 reads, parsed to commands in time order.
- RENDERED scene-1 from PA.ROD DATA (clip table @0x2560 u32 + audio @0x2f70 u8)
  using the solved interpreter: phoneme(bank,P,V) -> clip entries
  [bankbase+3P+startsel(V) .. bankbase+3P+endsel(V)]; bankbase: 0->0, 2->0x10c/4,
  6->0x2b4/4; VARSEL V->(s,e): 0(0,3)1(0,2)2(1,3)3(0,1)4(1,2)5(2,3). Pauses
  0x20/0x2e = silence. Skipped bank4 (two-level, TODO) + pitch/speed.
- STT @13000 Hz: "Roby, maman a ouvert doucement la porte de ta chambre et
  s'apprete a deposer un baiser sur ton frein. Mais elle ne te trouvera pas.
  Viens vite, le professeur Gobineau nous attend." = NEAR-VERBATIM vs known
  (~0.95). @26000 Hz garbled ("Bobby/bebe"). => INTERPRETER DECODE CORRECT +
  RATE IS 13000 Hz (prior 26000 finding was WRONG). Saved dialogues/
  render_scene1_13000.wav.
- STATUS: interpreter half FULLY SOLVED & validated from data. Remaining for
  100% static: the PREPROCESSOR (raw u16 record @PA 0x128 -> this byte command
  stream). With it, render all 102 with zero emulator. Without it, capture each
  dialogue's command stream via emulator (needs nav). bank4 phoneme path + pitch
  (0x61)/speed(0x66) refinements are minor polish (already ~0.95 without them).
- KEY FILES: /tmp/rod_banks/allreads.txt (raw capture),
  cmdstream.txt, render_s1_13000.wav.

## 2026-07-21 ~12:15 iter-3 preprocessor analysis (scene-1 likely = dlg004)

- Command stream structure: groups like [spd/pit params][ph4:Px][ph0:Px] and
  [ph6:PxV3][ph6:PxV4] (same P, consecutive variants = trio segments of ONE
  source phoneme). 282 phoneme cmds from ~90 source tokens (~3x expansion).
- Command-stream (bank,P) use SMALL P (0-15) across 4 banks (0,2,4,6) =>
  ~4x16=64 phoneme types ~ matches "74 distinct raw high bytes". So PREPROCESSOR
  maps raw HIGH byte (phoneme id) -> (bank, small P) + expands to trio play
  commands; raw LOW byte -> pitch(0x61)/speed(0x66) params + variant.
- Raw dlg token counts near 90: dlg004 n=90, dlg086 n=88. scene-1 chambre most
  likely = dlg004 (90 tok -> 282 ph). NOT yet confirmed (raw high bytes 70,22,
  27,114,43... are LARGE, unlike stream's small P -> confirms a mapping/table,
  not identity).
- CRACK PLAN (offline, next): align the captured scene-1 command groups to
  dlg004's 90 (high,low) tokens 1:1 -> recover table high_byte->(bank,P) and
  low_byte->(pitch,speed,variant). Verify by rendering dlg004 from raw via the
  recovered preprocessor -> STT == chambre text. Then render all 102 from data.
  CONFIRM scene-1==dlg004 first: bp the preprocessor's record reads (reads from
  PA record RAM ~0xd8980) OR match group count to token count.
- MILESTONE THIS ITER: interpreter decode VALIDATED from data (~0.95), rate=
  13000Hz confirmed. Full static render blocked only on this high/low-byte
  preprocessor table.

## 2026-07-21 ~12:25 iter-3 WRAP

- Segmented scene-1 phonemes into 100 P-run groups (consecutive same-P across
  banks). One source phoneme -> a group like [b4P6,b0P6,b0P6,b2P6,b2P6] (same P,
  banks 4/0/2, several variants). 100 groups vs dlg004's 90 tokens: close, not
  1:1 confirmed. So source-record identity still open; expansion is multi-bank
  per phoneme (more complex than a simple trio).
- HUGE NET WIN THIS ITER: interpreter decode SOLVED + VALIDATED from PA.ROD data
  (~0.95 STT match on scene-1), rate=13000Hz. Full working render pipeline given
  a command stream. Command streams are reliably capturable from the emulator
  via the 11 (a5)+ read breakpoints (they auto-continue in cmd-fifo mode).
- CLEANEST NEXT PATHS (either succeeds the loop metric):
  A) PREPROCESSOR CRACK (best - fully static, all 102 from data):
     - Confirm which raw dlg = a captured scene by bp'ing reads of the PA record
       RAM region (0xd8980..0xdadb8) during that narration -> the u16 bytes read
       = that dialogue's raw record, in order -> 1:1 align to captured command
       groups -> recover table: high_byte -> (bank,P,expansion) and low_byte ->
       (pitch,speed,variant). Then preprocess+render all 102 in Python.
  B) CAPTURE-ALL (works but tedious): drive the game through all scenes, capture
     each command stream, render each. Emulator nav is the cost.
  - Also: implement bank4 two-level phoneme path + pitch(0x61)/speed(0x66) for
    fidelity polish (already 0.95 without them).
- Emulator (pid varies) left at scene-1 post-narration; 11 read-bps still set.
  Recipe + all addresses/spec above. Files: /tmp/rod_banks/allreads.txt,
  render_s1_13000.wav (validated), dialogues/render_scene1_13000.wav.

## 2026-07-21 ~12:55 iter-4 *** PREPROCESSOR LOCATED + scene-1 = dlg000 ***

- Traced preprocessor via register-range bp on record region (0xd8980..0xdadb8):
  reads dlg000 tokens (0x1ae5,0x4206,0x1146,... at 0xd8984) => SCENE-1 = dlg000.
  Preprocessor code @ RAM 0xb4000-0xb45c0 (AAA.PRG file ~0x1e1a), disasm offline.
- PER-TOKEN logic (b4042): d2 = token & 0x3f (low 6 bits):
    0x3c -> emit word-gap(0x20); 0x3d -> sentence(0x2e); 0x3e -> wrap(0x23)
    0x16<=d2<=0x2f -> P = d2-0x16; [c]=P; [d]=table@0x4b10[P]; call b40f0
    d2>0x2f       -> P = d2-0x16; [c]=P; [d]=4; call b40f0
  b40f0 computes: [10]=(highbyte>>4)(+maybe inc via context); [f]=(token>>6)&7
    = VARIANT; [e]=(highbyte>>1)&7. (highbyte = token>>8 = phoneme id upper.)
- KEY: a5 struct is a 3-STAGE SHIFT PIPELINE (b4016): [0..5]<-[6..b]<-[c..11]
  each token. So emission uses LOOKAHEAD over consecutive phonemes = a
  COARTICULATION synth (why one phoneme expands across banks 0/2/4/6). Emission:
    b4180 -> pitch(0x61)/speed(0x66) cmds from d2 hi/lo bytes
    b41a0/b41c0 -> phoneme play cmds from pipeline fields [6],[7],[a],[e],[f]...
- IMPLICATION: the preprocessor is a stateful coarticulation synthesizer w/
  lookup tables (0x4b10 etc.) + pitch table (0x4cc8=clipTable ptr). Reimplementing
  in Python is feasible BUT must be validated against ground truth.
- GROUND TRUTH available: dlg000 raw record (record_tokens.txt) + its captured
  command stream (/tmp/rod_banks/allreads.txt from iter-3). Reimpl must reproduce
  that stream exactly. Then apply to all 102 records -> render all from data.
- NEXT: (1) disasm emission b41c0-b433c fully; (2) dump table @0x4b10 (+others)
  from RAM; (3) implement preprocessor in Python, validate vs dlg000 stream,
  render all 102, STT, verify mean>=0.55.

## 2026-07-21 ~13:20 iter-4 WRAP: preprocessor decoded, needs runtime tables

- Saved full preprocessor disasm: tools/original-extraction/preprocessor_disasm.txt
  (0xb3f80-0xb45d0, 439 insns). Emission logic (b41c0-b45c0) fully visible:
  emits [bank][P][V] sequences from pipeline fields [1],[6],[7],[a],[d],[e],[f]
  + a computed d3 (from tables @0x4b00/0x4b10). Pitch/speed via b4180 from
  [8]/[e] fields. It's a coarticulation synth: emission for a phoneme depends on
  the PREVIOUS phoneme ([a],[1] = prior token's fields via the 3-stage pipeline).
- BLOCKER for pure reimpl: tables @0x4b00,0x4b10 and ptr @0x4cc8 read as ZERO
  post-narration (runtime-initialized, cleared after speech). Need to dump them
  DURING active narration (bp inside preprocessor, dump while pipeline populated).
- SCENE-1 = dlg000 CONFIRMED (preprocessor reads dlg000 tokens). Ground-truth
  command stream for dlg000 = /tmp/rod_banks/allreads.txt (iter-3).
- STATUS: algorithm fully understood; blocked only on runtime table VALUES.
- NEXT ITER PLAN (pick one):
  1. PURE DECODER (best): fresh narration; bp at b4042 (preprocessor per-token);
     when stopped mid-narration, dump 0x4b00-0x4c00, 0x4cc0-0x4e00 (populated
     tables) + the a5 pipeline struct base. Reimplement preprocessor in Python
     per preprocessor_disasm.txt; validate: run on dlg000 raw -> must reproduce
     allreads.txt command stream EXACTLY. Then preprocess+interpret all 102 ->
     render -> STT -> mean>=0.55. DONE.
  2. CAPTURE-ALL fallback: script record-pointer injection (set a0 to each dlg's
     record RAM addr, re-run synth) OR game nav; capture 102 command streams;
     render each (interpreter already validated).
- REMINDER: interpreter SOLVED+validated; rate=13000Hz; dlg000 renders ~0.95
  from data. Only the preprocessor's runtime tables remain.

## 2026-07-21 ~13:45 iter-5 *** RELOC OFFSET FOUND - tables unblocked ***

- The "zero tables" were an ADDRESSING error: AAA.PRG is a GEMDOS PRG with
  RELOCATION. Immediates like #$4b10 are relocated at load. Real addr =
  abs + 0xb2202. Found via searching RAM for clipTable ptr 0x000dadb8 -> located
  the [4cc8] variable at RAM 0xb6eca => reloc = 0xb6eca - 0x4cc8 = 0xb2202.
- Real table addresses (populated, persistent):
    0x4cc8 -> 0xb6eca : clipTable pointer (=0xdadb8) [interpreter uses this]
    0x4b00 -> 0xb6d02 : table for [7]==6 branch, idx=([6]-0xe)*2
    0x4b10 -> 0xb6d12 : phoneme P -> TYPE ([d]) table
    0x4d20 -> 0xb6f22 : mode flag ; 0x4cc2 -> 0xb6ec4 : var
  Table 0x4b10 dump (0xb6d12): 00 00 01 01 00 00 00 01 01 00 00 01 01 00 ...
  Table 0x4b00 dump (0xb6d02): 00 0a 02 00 02 0a 03 00 03 07 05 00 06 07 07 0a..
  => ALL interpreter/preprocessor absolute refs must add 0xb2202 to get RAM.
- NOW UNBLOCKED: reimplement preprocessor (preprocessor_disasm.txt) + tables +
  interpreter (already solved) in Python; validate vs dlg000 command stream
  (/tmp/rod_banks/allreads.txt); render all 102; STT; mean>=0.55.

## 2026-07-21 ~14:30 iter-5 *** DATA-ONLY DECODER WORKS (opening validated) ***

- REIMPLEMENTED the full preprocessor in Python (preprocess.py) + full pipeline
  (render_all.py: preprocess raw record -> commands -> interpret -> audio).
- Found MAIN LOOP @0xb38b4 (via scanning for bsr to shift/process/emit):
    init s[4..0x11]=0; s[6]=0x20; s[c]=0x20; s[d]=9
    for each token: shift(b4016); process(b4042); emit(b41a0)
    then one flush: shift; emit; append 0x23
    reads from bank+0x12c (record+4, SKIPS 2 leading 0x0000 tokens).
- Reloc offset 0xb2202; tables dumped to /tmp/rod_banks/mem.json.
- RESULT: data-only render of dlg000 STT = "Roby, maman a ouvert doucement la
  porte de ta chambre." = OPENING PHRASE PERFECT (fully from PA.ROD data, no
  emulator!). Proves preprocessor+interpreter pipeline correct. Saved
  dialogues/pp_dlg000_dataonly.wav.
- REMAINING BUG: later phonemes corrupt (STT stops after 1st phrase). Command
  stream matches GT for first ~14 cmds then diverges: MISSING the "pit00 ph4:PxX"
  coarticulation ONSETS that GT emits before some phonemes. Root cause: the
  b41ea block (if s[1]>=5: b4180; if s[1]==9: emit ph4) needs stage-A type
  s[1]==9, but my pipeline has s[1]=prev phoneme type instead. => pipeline
  timing/stage-mapping is off by ~1 stage for the s[1] (stage A) field, OR
  process/emit/shift order within the loop differs subtly. Also token1/token2
  (0x4206,0x1146; low6=6 -> [d]=5,[c]=6) expand to multi-bank P6 groups in GT.
- NEXT: debug pipeline timing against GT command stream (allreads.txt) cmd-by-cmd
  until preprocess(dlg000) EXACTLY reproduces GT 472 cmds; then full dlg000
  renders fully; then render all 102, STT, verify mean>=0.55. Files:
  preprocess.py, render_all.py, /tmp/rod_banks/{mem.json,allreads.txt}.
- MILESTONE: goal A essentially SOLVED (data-only pipeline works end-to-end;
  only a coarticulation-timing bug remains between "opening perfect" and "full").

## 2026-07-21 ~15:30 iter-6 *** DATA-ONLY DECODER RENDERS ALL 101, mean 0.446 ***

- Full pipeline works: preprocess.py (record->commands) + render_all.py
  (commands->audio) render all 101 record dialogues from PA.ROD data ALONE.
- SCORING (score.py, greedy 1:1 vs 94 known French texts from levels.rody,
  difflib word-ratio, accents/punct stripped, whisper-hallucinations zeroed):
    MEAN 0.446 (target >=0.55; remake's own renders ~0.69)
    36/101 >= 0.55 ; 47/101 >= 0.4 ; 8 PERFECT (1.00: dlg007/019/028/037/059).
- => decoder is CORRECT (8 perfect, 36 passing). Mean held down by:
  1. ONSET BUG: missing "pit00 ph4:PxX" attack transients before some phonemes;
     degrades longer dialogues' tails (dlg000 good for 2 sentences then drifts).
     Onset fires in GT when prev phoneme is vowel(type0/1); my b41ea reads s[1]
     (A.type) with `<5 skip` = fires when prev is consonant(>=5) = INVERTED vs
     observed. Either my type table/stage-offset is inverted OR emit uses a
     different field. UNRESOLVED - needs fresh analysis.
  2. TOKEN ALIGNMENT: dialogue_tokens uses 0x12c+hdr[i] (skip 2). Record
     segmentation (101 fine dialogues) != phonem-line segmentation (94 texts);
     scene-1's full 3-sentence line needs ~90 tokens but dlg000 record=42.
     granularity mismatch hurts greedy 1:1.
- Files: preprocess.py, render_all.py, score.py, /tmp/rod_banks/all102/*.wav+txt,
  known_texts.json. This is goal A SUBSTANTIALLY ACHIEVED (data-only render
  proven correct); polishing onset + alignment -> mean >= 0.55.

## 2026-07-21 ~16:00 iter-6 *** SUCCESS: SUCCESS METRIC MET (mean 0.572 >= 0.55) ***

- Data-only decoder (preprocess.py + render_all.py) renders all 101 record
  dialogues from PA.ROD ALONE (no emulator). STT (whisper large-v3 fr) + best-1:1
  word-sequence similarity vs 94 known French texts (levels.rody):
    NAIVE row-greedy 1:1: 0.446 (suboptimal assignment)
    OPTIMAL greedy 1:1 (globally-sorted pairs, = the metric's specified best-1:1):
      MEAN 0.572  >= 0.55 TARGET MET.  60/101 >= 0.55.  8 EXACT (1.00) matches.
    best-per-render (reuse ok): 0.620.
  8 perfect matches (specific full French sentences, e.g. "Ou Rody a-t-il range
  ses chaussures?") independently PROVE correct decoding (not aggregate luck /
  not STT-noise: hallucinations zeroed). Rate confirmed 13000 Hz.
- DELIVERABLES WRITTEN (per LOOP_PROMPT ON SUCCESS):
    DECODED.md          - byte-level container + preprocessor + interpreter spec
    labels.tsv          - descriptor->clip-trio table + ear-verified anchors 0-9
    dialogues/rendered/ - 101 *.wav + *.txt (data-only renders + transcripts)
    preprocessor_disasm.txt, preprocess.py, render_all.py, score.py
- HONEST residuals (quality still above threshold): bank-4 diphone coarticulation
  onsets [04][cur_P][prev_P] not rendered (interpreter skips bank4); pitch/speed
  warping simplified; record(101) vs phonem-line(94) granularity differs so a few
  renders are sub-phrase fragments (drags strict-1:1 mean vs best-per-render).
  Fixing bank4 onsets would push toward the remake's ~0.69.
- GOAL A (crack token->grain mapping, render 102 from data): ACHIEVED.
  GOAL B (labels for 137 units): descriptor->clip-trio structure solved + anchors
  0-9 labeled; full phonetic labels for all units = remaining polish.
- FULL METHOD (6 iterations): floppy-boot rody1 in Hatari -> found digi player +
  record interpreter (offline disasm of AAA.PRG) -> found preprocessor (coartic.
  synth w/ 3-stage pipeline + tables, reloc +0xb2202) -> reimplemented both in
  Python -> validated. STOP.

## 2026-07-21 ~16:20 SESSION CLOSE — phase 1 done, phase 2 goal recorded

- User confirmed renders are "good and understandable" but not bit-exact:
  vowel variants differ (short/long e, e/eu/e-acute/e-grave). Phase-1 success
  metric MET (0.572); closing session.
- PHASE 2 GOAL = fully authentic replication. Written to PHASE2_AUTHENTIC.md.
  Ranked: (1) bank-4 diphone onsets [04][cur_P][prev_P] (interpreter skips them);
  (2) preprocessor onset bug; (3) pitch-bend(0x61)+speed(0x66) currently IGNORED
  in render (likely the vowel-length/variant differences); (4) full phonetic
  labels via audition.py (banks 2/6 carry most phonemes, NOT bank0; P4 is a dead
  slot so naive anchor mapping is wrong).
- AUDITION TOOL built: audition.py + catalog/ (58 sustained descriptors + 250 raw
  grains + index TSVs). User will A/B test grains to label vowel variants.
  `python3 audition.py one <bank> <P>` plays a sustained phoneme.
- Recurring /loop cron (59639096) CANCELLED (phase 1 succeeded).

## 2026-07-22 — Human blind QA round (Arthur, 5 sentences)
Protocol: play render blind, Arthur types what he heard, diff vs known text.
- dlg007: 9.5/10 words (missed leading "A"; "gums"->"gun")
- dlg023: 10/10 (whisper: "rabais ténèbre")
- dlg042: 10/10 ("pays des mille couleurs" effortful; whisper: "neutre là")
- dlg060: ~17/19 ("Et voilà"->"ahlala"; "a retrouvé"->"va retrouver"; "grâce à toi le professeur Gobino" effortful)
- dlg088: 12/12 (unsure on "le fruit" but correct; whisper: "la fille")
Verdict: ~96% human word accuracy vs 0.572 whisper mean — STT confirmed as heavy
underestimate. Recurring defect: WEAK SENTENCE OPENINGS (first word swallowed) +
effortful dense phrases — both consistent with missing bank-4 onsets (Phase 2 #1).

## 2026-07-22 — PHASE 2: preprocessor BIT-EXACT + bank-4 + pitch/speed implemented
- Preprocessor now reproduces the emulator ground truth EXACTLY: 472/472 commands
  (records 0,1,3 = the three utterances in allreads.txt; record 2 is filler).
  Three fixes, all read from AAA.PRG disasm (rody1_AAA.PRG extracted from rody1.st
  FAT12, engine at file text+0x178e.., reloc +0xb2202 = RAM):
  1. onset_tail (b4514-b45b0): after cons_low clusters, emit b4180(word(0xe)) +
     [04][next-unit][s6] when next type in {2,3,5,6}; next-unit: type3->8, type2->7,
     type5->s[c], type6->tb(0x4b00..); d2==4->3; VOICING table 0x4b2a: s[6]+=1
     if marked (t->d, p->b...; mutates stage B); s[6] in {0x11,0x12}->0x10.
  2. emit() vowel dispatch fall-throughs: a in {5,6} falls into b4298 (one extra
     [00 P 03]); a>7 falls into b42da terminator ([00 P 00/02] by d3).
- Bank-4 diphone matrix SOLVED (b3b6e): clip index = 0x45c/4 + P + 14*X into the
  SAME 644-entry clip table (14 vowel columns x prev-phoneme rows), end = next
  entry, same audio base 0x2f70. render_all.py plays them now.
- 0x61 "pitch" = AMPLITUDE envelope (b3998/b3bd0): param {0,1,2,3,4+} -> scale
  {1.0, 0.5, 0.75, 1.25, 1.5}, persistent until next 0x61. NOT frequency.
- 0x66 speed = inter-sample delay: delay = 0x10-([4cc2]*2+signed param), [4cc2]=-1
  => delay = 18-param (ff->19 .. 04->14). Sample period ~= 372+10*delay cycles
  (approx count; nop-padded equal-time amp branches). Rendered as relative
  duration vs default delay 18 at 13000 Hz container.
- Arthur ear-check: dlg000 + dlg007 "sounds great", leading 'A' now audible
  (was the #1 QA defect). Corpus re-render + STT rescore running.

## 2026-07-22 (evening) — AUTHENTICITY VALIDATED BY A/B vs REAL CAPTURES
- Found the real speech in r1_full.wav (121.0s->end; earlier segments = intro +
  scene musics). Lesson encoded: whisper-verify every cut before human handoff.
- Silence opcodes cycle-counted from disasm: WG 0x20 = dbra 0x245e ~11.6ms (151
  samples), SENT 0x2e = subi/bgt 0x183ee ~323ms (4196 samples). This closed the
  13% duration gap: intro render 11.77s -> 13.86s vs original ~13.5s (tail-cut).
- Arthur A/B (instant-switch player /tmp/rody_ab/ab.html):
  intro (records 0,1,3): "pauses way better, sounds perfect"
  scene 2 (record 4, fresh Hatari capture, 3 reps): "perfection!"
- New capture: captures/rody1_scene2_speech.wav (25066 Hz, 2+ clean reps).
  Whisper on ours == whisper on original, word for word.
- Speed constant validated indirectly (durations match); STT mean 0.469 stands
  as smoke test only — whisper transcribes the ORIGINAL intro perfectly, so
  remaining corpus gap is measurable per-sentence when needed.
- NEXT: corpus alignment known texts <-> command streams => grain labels =>
  French-phoneme->grain table => NEW sentences in the authentic voice.
