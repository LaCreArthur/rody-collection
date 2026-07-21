# Rody & Mastico 1 (Lankhor, Atari ST, 1988) — speech engine DECODED

Byte-level spec to render the 102 Rody 1 dialogues to audio **from PA.ROD data
alone** (no emulator). Reference impl: `preprocess.py` + `render_all.py`.

Validation: rendered all 101 record dialogues from data, STT (whisper large-v3
fr), best-1:1 word-sequence similarity vs the remake's known French texts =
**mean 0.572** (target 0.55; 60/101 >= 0.55; 8 exact 1.00 matches). Rate 13000 Hz.

## PA.ROD container (all offsets big-endian)

| region      | file range        | meaning |
|-------------|-------------------|---------|
| header      | 0x0000..0x0128    | 148 u16: per-dialogue byte offsets into record region (rel 0x128) |
| record      | 0x0128..0x2560    | per-dialogue u16 token scripts (see below) |
| clip table  | 0x2560..0x2f70    | 644 u32 offsets (relative to audio start); 251 distinct = real clips |
| audio PCM   | 0x2f70..0x1ac1e   | unsigned 8-bit PCM, ~13000 Hz |

Clip i spans audio[clipoff[i] .. clipoff[i+1]].

## Playback pipeline (two stages)

The engine (resident in AAA.PRG) runs a PREPROCESSOR that expands the u16 record
into a byte COMMAND stream, then an INTERPRETER that turns commands into audio.
(Both fully reverse-engineered from AAA.PRG; RAM code relocated by +0xb2202.)

### 1. Preprocessor (record u16 -> byte commands)  [main loop @b38b4]

Per dialogue: read tokens from `0x12c + hdr[i]` (i.e. record+4, skipping the two
leading padding words), for `(hdr[i+1]-hdr[i])/2` tokens. State is a 3-stage
shift pipeline (18 bytes); each token computes fields into stage C, shifts
C->B->A, and emits stage B (1-token lookahead + 1 lookback = coarticulation).

Per token `t` (hi = t>>8):  `d2 = t & 0x3f`
- `d2==0x3c` -> word-gap ; `0x3d` -> sentence ; `0x3e` -> buffer-wrap
- `d2 < 0x16` -> phoneme P=d2, type=5 (or 6 if d2>=0xe)
- `0x16<=d2<=0x2f` -> P=d2-0x16, type=table[0x4b10+P]
- `d2 > 0x2f` -> P=d2-0x16, type=4
- variant V = (t>>6)&7 ; e = (hi>>1)&7 ; hinib = hi>>4

Emit (b41a0) produces command bytes per stage-B type, with pitch(0x61)/speed
(0x66) params derived from e/variant and repeat counts from hinib. See
`preprocess.py` for the exact branch translation.

### 2. Interpreter (byte commands -> audio)  [loop @b3990]

| opcode | bytes | action |
|--------|-------|--------|
| 0x20   | 1     | word-gap silence |
| 0x2e   | 1     | sentence silence |
| 0x23   | 1     | buffer wrap (no audio) |
| 0x61 P | 2     | pitch-bend set (param) |
| 0x66 P | 2     | speed set (param) |
| 0x00/0x02/0x06  P V | 3 | play phoneme, bank 0/2/6 |
| 0x04   P X | 3 | diphone transition (bank 4) — coarticulation onset |

Phoneme play (banks 0/2/6): descriptor = clip-offset entries `[base+3P .. base+3P+3]`
where base = {0:0, 2:0x10c/4, 6:0x2b4/4}. Variant V picks start/end pair:
`V: 0->(0,3) 1->(0,2) 2->(1,3) 3->(0,1) 4->(1,2) 5->(2,3)`.
Play audio[audioBase+clipoff[base+3P+start] .. clipoff[base+3P+end]].
Sample loop reads bytes, subtracts 0x80, applies pitch-bend, writes 3 YM volume
registers (movep $FF8800) = 3-channel ST digi. Bank 4 = `[04][cur_P][prev_P]`
diphone transitions (attack transients); reference impl skips them (minor).

## Files
- `preprocess.py` — record u16 -> command stream (reimpl of AAA.PRG preprocessor)
- `render_all.py` — command stream -> u8 PCM wav (interpreter)
- `score.py` — STT similarity metric vs known texts
- `dialogues/*.wav` `*.txt` — rendered dialogues + transcripts
- `preprocessor_disasm.txt` — annotated 68000 disassembly of the preprocessor

## Known residuals (quality is above threshold without these)
- Bank-4 diphone onsets not emitted/rendered (coarticulation smoothing). Onset =
  `[04][current_P][previous_P]`; interpreter path is bank4 two-level (b3b6e).
- Pitch-bend/speed applied structurally but sample-rate warping simplified.
- Record vs phonem-line segmentation differ (101 records vs 94 lines), so a few
  renders are sub-phrase fragments.
