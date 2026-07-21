#!/usr/bin/env python3
"""Audition tool for Rody1 PA.ROD phoneme grains — to identify vowel variants.

Usage:
  python3 audition.py grains          # render every distinct grain (raw), in order
  python3 audition.py descriptors     # render each bank/P as a SUSTAINED phoneme
  python3 audition.py one <bank> <P> [loops]   # render one sustained phoneme, print path
  python3 audition.py play <clip_index>        # render one raw clip by table index

Grains = the 251 distinct clips in PA.ROD (the raw phoneme material).
Descriptor P (bank 0/2/6) = clip-table entries [base+3P .. base+3P+3]:
  attack=+0..+1, middle(loopable)=+1..+2, release=+2..+3.
SUSTAINED render = attack + middle*loops + release (so vowels are clearly audible).
"""
import struct, wave, sys, os

_HERE = os.path.dirname(os.path.abspath(__file__))
PA = open(os.path.join(_HERE, 'banks/rody1_PA.ROD'), 'rb').read()
def u32(o): return struct.unpack('>I', PA[o:o+4])[0]
CLIP, AUDIO, RATE = 0x2560, 0x2f70, 13000
clipoff = [u32(CLIP + 4*k) for k in range(644)]
audio = PA[AUDIO:]
BANKBASE = {0: 0, 2: 0x10c//4, 6: 0x2b4//4}   # clip-entry base per bank
OUT = os.path.join(_HERE, 'catalog')

def wav(pcm, path):
    w = wave.open(path, 'wb'); w.setnchannels(1); w.setsampwidth(2); w.setframerate(RATE)
    w.writeframes(b''.join(struct.pack('<h', (b-128)*256) for b in pcm)); w.close()

def seg(a, b):
    return audio[clipoff[a]:clipoff[b]] if 0 <= a < 644 and 0 <= b < 644 else b''

def sustained(bank, P, loops=4):
    base = BANKBASE[bank]; i = base + 3*P
    if i+3 >= 644: return b''
    attack = seg(i, i+1); middle = seg(i+1, i+2); release = seg(i+2, i+3)
    return attack + middle*loops + release

def distinct_grains():
    """Return list of (clip_index, start, end) for the 251 distinct non-empty clips."""
    out = []
    for k in range(643):
        if clipoff[k+1] > clipoff[k]:
            out.append((k, clipoff[k], clipoff[k+1]))
    return out

def main():
    os.makedirs(OUT, exist_ok=True)
    cmd = sys.argv[1] if len(sys.argv) > 1 else 'descriptors'
    if cmd == 'grains':
        g = distinct_grains(); idx = open(f'{OUT}/grains_index.tsv', 'w')
        idx.write('grain_num\tclip_index\tstart\tend\tms\twav\n')
        for n, (k, s, e) in enumerate(g):
            path = f'{OUT}/grain_{n:03d}_clip{k:03d}.wav'
            # loop short grains 3x so they're audible
            pcm = audio[s:e];
            if e-s < 800: pcm = pcm*3
            wav(pcm, path)
            idx.write(f'{n}\t{k}\t{s}\t{e}\t{(e-s)/13:.0f}\t{os.path.basename(path)}\n')
        print(f'{len(g)} distinct grains -> {OUT}/ (see grains_index.tsv)')
    elif cmd == 'descriptors':
        anchors = ['i','é','a','o','ou','u','eu','in','an','on']
        idx = open(f'{OUT}/descriptors_index.tsv', 'w')
        idx.write('bank\tP\tclip_start\tclip_end\tattack_ms\tmid_ms\trel_ms\tguess\twav\n')
        for bank in (0, 2, 6):
            for P in range(45):
                base = BANKBASE[bank]; i = base+3*P
                if i+3 >= 644: break
                if clipoff[i+3] <= clipoff[i]: continue  # dead/padding slot
                pcm = sustained(bank, P)
                if not pcm: continue
                path = f'{OUT}/desc_b{bank}_P{P:02d}.wav'
                wav(pcm, path)
                a=(clipoff[i+1]-clipoff[i])/13; m=(clipoff[i+2]-clipoff[i+1])/13; r=(clipoff[i+3]-clipoff[i+2])/13
                guess = anchors[P] if bank==0 and P<10 else ''
                idx.write(f'{bank}\t{P}\t{i}\t{i+3}\t{a:.0f}\t{m:.0f}\t{r:.0f}\t{guess}\t{os.path.basename(path)}\n')
        print(f'descriptor catalog -> {OUT}/ (see descriptors_index.tsv)')
    elif cmd == 'one':
        bank, P = int(sys.argv[2]), int(sys.argv[3])
        loops = int(sys.argv[4]) if len(sys.argv) > 4 else 4
        path = f'{OUT}/one_b{bank}_P{P:02d}.wav'; wav(sustained(bank, P, loops), path)
        print(path); os.system(f'afplay "{path}"')
    elif cmd == 'play':
        k = int(sys.argv[2]); path = f'{OUT}/clip_{k:03d}.wav'
        wav(audio[clipoff[k]:clipoff[k+1]]*3, path); print(path); os.system(f'afplay "{path}"')

if __name__ == '__main__':
    main()
