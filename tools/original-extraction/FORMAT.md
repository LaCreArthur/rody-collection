# Original Rody & Mastico speech banks (PA.ROD)

Extracted from the original Atari ST disk images (Lankhor, 1988-1991).
`banks/` holds the six PA.ROD speech banks; `slice_exact.py` slices any of them
into individual clips with the engine's own boundaries.

```bash
python3 slice_exact.py banks/rody1_PA.ROD /tmp/out   # wavs + manifest.txt
```

## File format (big-endian, decoded 2026-07-20)

```
[0x0000,0x0128)  header: 148 u16. First D entries = per-dialogue byte offsets
                 into the record region (D = dialogue count; 102 for Rody 1).
[0x0128, TBL)    record region: per-dialogue phoneme scripts. Dialogue i =
                 [0x128+hdr[i], 0x128+hdr[i+1]). Size == last header value.
                 ENTRY ENCODING NOT YET DECODED (not plain (clip,param) pairs).
[TBL, AUDIO)     clip offset table: 644 entries, offsets RELATIVE to audio
                 start, non-decreasing, first == 0, last distinct == audio
                 length. u16 if audio < 64 KiB (all banks except Rody 1),
                 u32 otherwise (Rody 1). Adjacent duplicates = empty slots.
[AUDIO, EOF)     unsigned 8-bit PCM. Replay: ~13000 Hz for the five family
                 banks, ~26000 Hz for Rody 1 (2.0x sample density, pitch-
                 verified). Clip i = [offset[i], offset[i+1]).
```

## Clip structure (ear-verified by Arthur, 2026-07-20)

Phonemes are stored as **trios**: attack grain, loopable middle grain, release
grain (lengths long >= mid >= short, contiguous). The engine sustained vowels
by looping the middle grain - duration/rhythm were runtime parameters. Noise
consonants and specials are single grains. Rody 1 bank: 251 non-empty clips =
57 trios + 80 solos.

Bank order starts with sustained vowels in canonical order:
**i, é, a, o, ou, u, eu, in, an, on**, ... then consonant/noise units, then a
second series of sharper/shorter vowel variants (short-vowel recordings), and
multiple 'o' variants. Full unit->phoneme labeling pending (plan: decode the
record region, then align decoded dialogue sequences against the remake's
phoneme strings in original-stories/ to vote in labels automatically).

## Cross-bank facts

- Episodes 2, 3, 5, 6, Noël share one recording set (70-80% verbatim overlap,
  byte-identical clip-length sequences). Each ships a subset.
- Rody 1 uses a DIFFERENT recording session, with exactly 2.0x the samples per
  clip (541,439,336 vs 271,220,168 ...): same phoneme set at double resolution.
- The remake's Unity clips (Assets/Sounds/Phoneme_v2) were hand-captured from
  Rody 1's bank (cross-correlation 0.90 at 13 kHz).
- SONROD.ROD is byte-identical on all six disks (driver/config, undecoded).

## Open items

1. Record-region entry encoding (per-phoneme id + loop/pitch params).
3. Unit->phoneme label table (after item 1, via corpus alignment).
4. One long unindexed tail clip in Rody 1 (~0.93 s at offset 97453).

Contact sheet review artifact: contact_sheet.txt maps listen-order timestamps
to unit numbers and byte ranges (wav regenerable via the scripts in git
history / this folder).
