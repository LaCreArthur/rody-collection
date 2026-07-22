#!/usr/bin/env python3
"""Align reimpl vs ground-truth command streams (dlg000) and show divergences."""
import os, sys
from difflib import SequenceMatcher
from preprocess import preprocess, dialogue_tokens, fmt

HERE = os.path.dirname(os.path.abspath(__file__))

def gt_seq():
    gt = [l.split() for l in open(os.path.join(HERE, 'data/allreads.txt'))]
    gcmds = []; i = 0
    while i < len(gt):
        if gt[i][0] != 'b3990': i += 1; continue
        op = int(gt[i][2], 16); ops = []; i += 1
        while i < len(gt) and gt[i][0] != 'b3990': ops.append(int(gt[i][2], 16)); i += 1
        gcmds.append((op, ops))
    def gfmt(op, ops):
        if op in (0x20, 0x2e, 0x23): return {0x20: 'WG', 0x2e: 'SENT', 0x23: 'WRAP'}[op]
        if op == 0x61: return f'pit{ops[0]:02x}'
        if op == 0x66: return f'spd{ops[0]:02x}'
        if op in (0, 2, 6): return f'ph{op}:P{ops[0]}V{ops[1]}'
        if op == 4: return f'ph4:P{ops[0]}x{ops[1]}'
        return f'?{op:02x}'
    return [gfmt(*c) for c in gcmds]

if __name__ == '__main__':
    mine = fmt(preprocess(dialogue_tokens(0)))
    gt = gt_seq()
    sm = SequenceMatcher(None, mine, gt, autojunk=False)
    print(f'mine={len(mine)} gt={len(gt)} ratio={sm.ratio():.3f}')
    for tag, i1, i2, j1, j2 in sm.get_opcodes():
        if tag == 'equal':
            print(f'  = [{i1}:{i2}] {" ".join(mine[i1:min(i2,i1+4)])}{" ..." if i2-i1>4 else ""} ({i2-i1})')
        else:
            print(f'  {tag.upper()} mine[{i1}:{i2}]={" ".join(mine[i1:i2])}')
            print(f'       {"":<4} gt[{j1}:{j2}]={" ".join(gt[j1:j2])}')
