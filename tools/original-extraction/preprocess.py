#!/usr/bin/env python3
"""Reimplementation of Rody1 AAA.PRG speech PREPROCESSOR (record u16 -> byte command stream).
Faithful translation of preprocessor_disasm.txt (RAM 0xb3f80-0xb45d0), reloc 0xb2202.
State s[0..0x11]: 3 stages of 6 bytes: A=[0..5] B=[6..b] C=[c..11]; fields {P,type,e,var,hinib,x}.
Validated against dlg000 captured command stream.
"""
import struct, json, sys, os

HERE = os.path.dirname(os.path.abspath(__file__))
MEM = {int(k): v for k, v in json.load(open(os.path.join(HERE, 'data/mem.json'))).items()}
def tb(a): return MEM.get(a, 0)

PA = open(os.path.join(HERE, 'banks/rody1_PA.ROD'), 'rb').read()
def u16(o): return struct.unpack('>H', PA[o:o+2])[0]

def dialogue_tokens(i):
    # game reads from 0x12c + hdr[i] for (hdr[i+1]-hdr[i])/2 tokens (skips 2 leading words)
    hdr = [u16(2*k) for k in range(148)]
    start = 0x12c + hdr[i]
    n = (hdr[i+1] - hdr[i]) // 2
    return [u16(start + 2*j) for j in range(n)]

def preprocess(tokens):
    s = [0]*0x12
    out = []
    def b4180(d2):  # emit speed(0x66)/pitch(0x61) from word d2
        lo = d2 & 0xff
        if lo != 0:
            out.append(0x66); out.append((lo-4) & 0xff)
        hi = (d2 >> 8) & 0xff
        if hi != 0:
            out.append(0x61); out.append((hi-1) & 0xff)
    def word(o): return ((s[o] << 8) | s[o+1]) & 0xffff

    def emit():  # b41a0 : emit stage B ([6..b]) with context A=[0..5], C=[c..11]
        if s[7] == 9:                                  # b41a0
            if s[6] == 0x23: return
            for _ in range(s[0xa]+1): out.append(s[6])
            return
        # b41c0
        if s[7] < 5:
            return emit_cons()                          # b433c
        d3 = 0
        if s[7] == 6:                                   # b41cc
            d3 = tb(0x4b00 + ((s[6]-0xe) & 0xff)*2)
        if s[1] >= 5:                                   # b41ea
            b4180(word(8))
            if s[1] == 9:
                out.append(0x04); out.append(s[6])
                if d3 != 0: out[-1] = d3
                out.append(0x16)
        # b4214 dispatch on s[a]
        if s[0xa] == 1:                                 # b431e
            out.append(0x00); out.append(s[6]); out.append(0x00)
            if s[0xd] != 9: out[-1] = 0x01
            return
        if s[0xa] > 1:                                  # b425c
            if s[0xa] < 4:                              # b42fc (a==2,3)
                d2 = (s[0xa]-2) & 0xff
                for _ in range(d2+1):
                    out.append(0x00); out.append(s[6])
                    if d3 != 0: out[-1] = d3
                    out.append(0x03)
                out.append(0x00); out.append(s[6]); out.append(0x00)
                if s[0xd] != 9: out[-1] = 0x01
                return
            if s[0xa] == 4:                             # b4298
                if d3 != 0:
                    out.append(0x00); out.append(s[6]); out.append(0x03)
                else:
                    out.append(0x00); out.append(s[6]); out.append(0x03)
                return
            # s[a] >= 5 : b4268
            if s[0xa] < 7:                              # b4276 (a==5,6) falls into b4298
                d2 = (s[0xa]-5) & 0xff
                for _ in range(d2+1):
                    out.append(0x00); out.append(s[6])
                    if d3 != 0: out[-1] = d3
                    out.append(0x03)
                out.append(0x00); out.append(s[6]); out.append(0x03)
                return
            if s[0xa] == 7:                             # b42da
                if d3 != 0:
                    out.append(0x00); out.append(s[6]); out.append(0x00)
                else:
                    out.append(0x00); out.append(s[6]); out.append(0x02)
                return
            # a > 7 : b42b8 falls into b42da
            d2 = (s[0xa]-8) & 0xff
            for _ in range(d2+1):
                out.append(0x00); out.append(s[6])
                if d3 != 0: out[-1] = d3
                out.append(0x03)
            if d3 != 0:
                out.append(0x00); out.append(s[6]); out.append(0x00)
            else:
                out.append(0x00); out.append(s[6]); out.append(0x02)
            return
        # s[a] == 0 : b4220
        if d3 != 0:
            out.append(0x00); out.append(s[6]); out.append(0x00)
            if s[0xd] != 9: out[-1] = 0x01
        else:
            out.append(0x00); out.append(s[6]); out.append(0x02)
            if s[0xd] != 9: out[-1] = 0x04
        return

    def emit_cons():  # b433c : s[7] < 5 (consonant classes)
        if s[7] < 2:                                    # b43e4
            return emit_cons_low()
        if s[7] == 2:
            d3 = 7
        elif s[7] == 4:                                 # b434e
            return emit_special()                        # b45b2
        else:  # s[7]==3
            d3 = 8
        # b435c
        if s[1] >= 5:
            b4180(word(8))
            if s[1] == 9:
                out.append(0x04); out.append(d3); out.append(0x16)
        d2 = s[0xa]                                     # b437e
        if d2 > 0:
            for _ in range(d2):
                out.append(0x00); out.append(d3); out.append(0x03)
        b4180(word(0xe))                               # b4398 uses [e..f]
        if s[0xd] == 6:                                 # b43a0
            d2 = ((s[0xc]-0xe) & 0xff)*2
            out.append(0x04); out.append(tb(0x4b00+d2)); out.append(s[6])
            return
        out.append(0x04); out.append(s[0xc])            # b43c8
        if s[0xc] == 4: out[-1] = 0x03
        out.append(s[6])
        return

    def onset_tail():  # b4514..b45b0 : diphone onset announcing next unit
        if s[0xd] < 2 or s[0xd] == 4 or s[0xd] == 9: return
        b4180(word(0xe))
        if s[0xd] < 5:                                  # next is consonant class 2/3
            d2 = 8 if s[0xd] == 3 else 7
        elif s[0xd] == 5:
            d2 = s[0xc]
        else:                                           # s[0xd]==6
            d2 = tb(0x4b00 + ((s[0xc]-0xe) & 0xff)*2)
        if d2 == 4: d2 = 3
        if tb(0x4b2a + s[6]) != 0:                      # voicing fix mutates stage B
            s[6] = (s[6]+1) & 0xff
        if 0x11 <= s[6] <= 0x12: s[6] = 0x10
        out.append(0x04); out.append(d2); out.append(s[6])

    def emit_cons_low():  # b43e4 : s[7] < 2
        b4180(word(8))
        d = s[0xd]
        if d < 2: d2 = 0xa
        elif d == 2: d2 = 7
        elif d == 3: d2 = 8
        elif d == 6:
            d2 = tb(0x4b00 + ((s[0xc]-0xe) & 0xff)*2)
        elif d > 6: d2 = 0xa
        elif d == 4: d2 = 0xa
        else: d2 = s[0xc]                               # d==5
        # b443c
        d2 = ((d2 * 0x1a) + s[6]) & 0xffff
        d3 = 2
        if tb(0x4b44 + d2) != 0: d3 = 6
        if s[0xa] >= 5:                                 # b445c
            s[0xa] = (s[0xa]-5) & 0xff
            d3 = 2 if d3 != 2 else 6
        # b447a
        if s[7] != 0:                                   # b44dc
            out.append(d3); out.append(s[6]); out.append(0x03)
            d2c = s[0xa]
            if d2c != 0:
                for _ in range(d2c):
                    out.append(d3); out.append(s[6]); out.append(0x04)
            if s[0xd] == 9:
                out.append(d3); out.append(s[6]); out.append(0x05)
            return onset_tail()
        # b4482 s[7]==0
        if s[6] == 0x16:                               # b4488
            if s[0] in (0xa, 0xe, 0x10, 0x15):
                out.append(0x00); out.append(0x10); out.append(0x02)
                s[7] = 9
                return
        d2c = s[0xa]                                    # b44b6
        if d2c != 0:
            for _ in range(d2c):
                out.append(d3); out.append(s[6]); out.append(0x03)
        out.append(d3); out.append(s[6]); out.append(0x04)
        # falls to b4500
        if s[0xd] == 9:
            out.append(d3); out.append(s[6]); out.append(0x05)
        return onset_tail()

    def emit_special():  # b45b2 : s[7]==4
        b4180(word(8))
        d2 = s[0xa]
        if d2 != 0:
            for _ in range(d2):
                out.append(0x02); out.append(s[6]); out.append(0xaa)
        return

    def b40f0(a0):  # compute [e],[f],[10] for stage C from token at a0
        hi = PA[a0]
        s[0x10] = hi >> 4
        # [4d20]!=1 and [4cc2]=0xffff<5 -> skip increment, go to b4166
        tok = u16_at(a0)
        s[0xf] = (tok >> 6) & 7
        s[0xe] = (hi >> 1) & 7

    def u16_at(a0): return (PA[a0] << 8 | PA[a0+1]) if a0+1 < len(PA) else 0

    # main loop: iterate over record tokens (in RAM). We hold tokens as list.
    # a0 walks the record; process fills stage C; shift then emit stage B.
    # Emulate: for each token -> shift; process token -> C; emit B.
    def shift():
        s[0], s[1], s[2], s[3] = s[6], s[7], s[8], s[9]
        s[4], s[5] = s[0xa], s[0xb]
        s[6], s[7], s[8], s[9] = s[0xc], s[0xd], s[0xe], s[0xf]
        s[0xa], s[0xb] = s[0x10], s[0x11]
        s[0xc]=s[0xd]=s[0xe]=s[0xf]=s[0x10]=s[0x11]=0
        s[0xc]=0x20; s[0xd]=9

    def process(tok, hi):
        d2 = tok & 0x3f
        if d2 == 0x3c: s[0xc]=0x20; s[0xd]=9
        elif d2 == 0x3d: s[0xc]=0x2e; s[0xd]=9
        elif d2 == 0x3e: s[0xc]=0x23; s[0xd]=9
        elif d2 < 0x16:                                  # b40be
            s[0xd]=5
            if d2 >= 0xe: s[0xd]=6
            s[0xc]=d2
        elif d2 <= 0x2f:                                 # b40a4
            d2 -= 0x16; s[0xc]=d2; s[0xd]=tb(0x4b10+d2)
        else:                                            # b40dc
            d2 -= 0x16; s[0xc]=d2; s[0xd]=4
        # b40f0 fields
        s[0x10] = hi >> 4
        s[0xf] = (tok >> 6) & 7
        s[0xe] = (hi >> 1) & 7

    # main loop per b38b4: init s[4..0x11]=0; s[6]=0x20; s[c]=0x20; s[d]=9
    for k in range(4, 0x12): s[k] = 0
    s[6] = 0x20; s[0xc] = 0x20; s[0xd] = 9
    for tok in tokens:                 # shift, process, emit
        shift()
        process(tok, tok >> 8)
        emit()
    shift(); emit()                    # one flush
    out.append(0x23)
    return out


def fmt(cmds):
    r=[]; i=0
    while i < len(cmds):
        op=cmds[i]
        if op in (0x20,0x2e,0x23): r.append({0x20:'WG',0x2e:'SENT',0x23:'WRAP'}[op]); i+=1
        elif op==0x61: r.append(f'pit{cmds[i+1]:02x}'); i+=2
        elif op==0x66: r.append(f'spd{cmds[i+1]:02x}'); i+=2
        elif op in (0,2,6): r.append(f'ph{op}:P{cmds[i+1]}V{cmds[i+2]}'); i+=3
        elif op==4: r.append(f'ph4:P{cmds[i+1]}x{cmds[i+2]}'); i+=3
        else: r.append(f'?{op:02x}'); i+=1
    return r

if __name__=='__main__':
    cmds = preprocess(dialogue_tokens(0))
    seq = fmt(cmds)
    print('dlg000 reimpl: %d cmds' % len(cmds))
    print(' '.join(seq[:60]))
    # ground truth from allreads
    gt=[l.split() for l in open(os.path.join(HERE,'data/allreads.txt'))]
    gcmds=[]; i=0
    while i<len(gt):
        if gt[i][0]!='b3990': i+=1; continue
        op=int(gt[i][2],16); ops=[]; i+=1
        while i<len(gt) and gt[i][0]!='b3990': ops.append(int(gt[i][2],16)); i+=1
        gcmds.append((op,ops))
    def gfmt(op,ops):
        if op in (0x20,0x2e,0x23): return {0x20:'WG',0x2e:'SENT',0x23:'WRAP'}[op]
        if op==0x61: return f'pit{ops[0]:02x}'
        if op==0x66: return f'spd{ops[0]:02x}'
        if op in (0,2,6): return f'ph{op}:P{ops[0]}V{ops[1]}'
        if op==4: return f'ph4:P{ops[0]}x{ops[1]}'
        return f'?{op:02x}'
    gseq=[gfmt(*c) for c in gcmds]
    print('\nground truth: %d cmds' % len(gcmds))
    print(' '.join(gseq[:60]))
    # match
    n=min(len(seq),len(gseq)); match=sum(1 for a,b in zip(seq,gseq) if a==b)
    print(f'\nfirst-{n} match: {match}/{n} = {match/n:.2f}')
