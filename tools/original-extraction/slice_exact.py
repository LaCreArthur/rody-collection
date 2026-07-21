#!/usr/bin/env python3
"""
Slice Lankhor "Rody et Mastico" (Atari ST, 1988) PA.ROD speech banks into exact clips.

FORMAT (big-endian), verified uniform across all six banks
(rody1 + rody2/rody3/rody5/rody6/noel):

  [0x0000, 0x0128)   header: 148 u16 BE.
                     idx 0..D-1 = per-dialogue byte offsets (relative to 0x0128) into
                     the record region; monotonic, ends at record-region size.
                     (D = 102 for rody1; fewer for the shorter banks.)
  [0x0128, TBL)      record region: per-dialogue phoneme/command scripts.
                     Dialogue i = bytes [0x0128+hdr[i], 0x0128+hdr[i+1]); u16-entry
                     scripts give a sensible phoneme count per dialogue
                     (rody1: median 42, range 0..158). The per-entry field encoding
                     is not fully reversed and is NOT needed to slice audio.
  [TBL, AUDIO)       CLIP OFFSET TABLE: N offsets, each RELATIVE TO AUDIO START.
                     Non-decreasing. offset[0] == 0 (clip 0 begins exactly at audio).
                     The final distinct value == total audio length (terminator);
                     trailing entries repeat it as padding. Duplicate adjacent values
                     are zero-length (unused) slots.
                     Width is u16 when audio < 64 KiB (the five short banks), u32 when
                     audio >= 64 KiB (rody1, whose offsets reach 97453). Auto-detected.
  [AUDIO, EOF)       unsigned 8-bit PCM, replay ~13000 Hz.  AUDIO == TBL end.

  Clip i spans [AUDIO + offset[i], AUDIO + offset[i+1]); the last real clip ends at EOF.

Self-validating identity used to locate the table (no magic constants, no external
reference): find the width and table-end TE such that the u16/u32 values immediately
before TE form a non-decreasing run whose first element is 0 and whose last element
equals (EOF - TE). Audio then starts at TE.
"""
import struct, sys, os, wave

SAMPLE_RATE = 13000

def rd(d, o, w): return struct.unpack('>I' if w == 4 else '>H', d[o:o+w])[0]

def locate_table(d):
    """Return (width, table_start, audio_start, offsets[])."""
    n = len(d)
    best = None
    for w in (2, 4):
        for te in range(1000, n - w + 1, 2):
            last = rd(d, te - w, w)
            if not (n - 2 <= te + last <= n + 2):
                continue
            p = te - w; prev = last; cnt = 1
            while p - w >= 0:
                v = rd(d, p - w, w)
                if v > prev or v > n:
                    break
                prev = v; p -= w; cnt += 1
            if rd(d, p, w) != 0:      # true table starts at the first 0 offset
                continue
            if cnt >= 100 and (best is None or cnt > best[3]):
                offs = [rd(d, p + w*i, w) for i in range(cnt)]
                best = (w, p, te, cnt, offs)
    if not best:
        return None
    w, ts, te, cnt, offs = best
    return w, ts, te, offs

def write_wav(path, pcm):
    with wave.open(path, 'wb') as wv:
        wv.setnchannels(1); wv.setsampwidth(2); wv.setframerate(SAMPLE_RATE)
        wv.writeframes(b''.join(struct.pack('<h', (b - 128) * 256) for b in pcm))

def main():
    src = sys.argv[1] if len(sys.argv) > 1 else 'rody1_PA.ROD'
    outdir = sys.argv[2] if len(sys.argv) > 2 else 'exact'
    d = open(src, 'rb').read(); n = len(d)
    res = locate_table(d)
    if not res:
        sys.exit(f"{src}: no clip table found")
    w, ts, audio, offs = res
    os.makedirs(outdir, exist_ok=True)
    starts = [audio + v for v in offs]
    ends = starts[1:] + [n]
    man = open(os.path.join(outdir, 'manifest.txt'), 'w')
    man.write(f"# {os.path.basename(src)} bytes={n} width=u{w*8} "
              f"clip_table=[{ts},{audio}) entries={len(offs)} "
              f"audio=[{audio},{n}) audio_len={offs[-1]} rate={SAMPLE_RATE}\n")
    man.write("# index\trel_offset\tabs_start\tlength\twav\n")
    written = 0
    for i, (a, b) in enumerate(zip(starts, ends)):
        length = b - a
        name = ""
        if length > 0:
            name = f"clip{i:03d}.wav"
            write_wav(os.path.join(outdir, name), d[a:b])
            written += 1
        man.write(f"{i}\t{offs[i]}\t{a}\t{length}\t{name}\n")
    man.close()
    print(f"{src}: u{w*8} table[{ts},{audio}) {len(offs)} entries, audio=[{audio},{n}), "
          f"{written} non-empty clips -> {outdir}/")

if __name__ == '__main__':
    main()
