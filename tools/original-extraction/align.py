#!/usr/bin/env python3
"""Corpus alignment: label every (bank,P) phoneme descriptor by aligning each
Rody1 dialogue's bit-exact (bank,P) command stream against the French phonemes
of its own whisper transcript, then voting across all 102 dialogues.

Substrate (validated against DECODED.md, not the record-hi-byte hypothesis):
  - preprocess.py turns a record into the exact command stream.
  - A phoneme play command is `ph{bank}:P{p}` (banks 0/2/6). Bank 4 = diphone
    onset (coarticulation), ignored for identity.
  - Identity is the pair (bank, P). Bank 0 = vowels/continuants (P0-9 are the
    ear-verified anchors i e a o ou u eu in an on). Banks 2/6 = consonants.
    Same P in different banks is a DIFFERENT phoneme (context grain), so we
    vote per (bank,P) cell independently (PROGRESS.md: naive same-P anchor is
    wrong; banks 2/6 carry the consonants).

Text anchor: dialogues/rendered/dlg*.txt (whisper large-v3 fr of each render).
The transcript and the (bank,P) stream are two views of the SAME utterance, so
no cross-dialogue matching is needed. Vowel anchors + the hard vowel<->bank0 /
consonant<->bank2/6 type constraint drive a Needleman-Wunsch alignment; each
aligned (french_phoneme, bank, P) increments a vote. Labels emerge from the
vote majority across the whole corpus.

Usage:
  align.py                 # run alignment + voting, print the (bank,P) table
  align.py --write         # also write catalog/phoneme_table.tsv, update labels.tsv
"""
import sys, os, re, json, glob
from collections import defaultdict, Counter

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
sys.path.insert(0, os.path.join(HERE, '..', 'phoneme-dict'))
import preprocess as pp
import derive_dict as dd

# ear-verified bank-0 vowel anchors (labels.tsv)
ANCHOR = {0: 'i', 1: 'é', 2: 'a', 3: 'o', 4: 'ou', 5: 'u', 6: 'eu', 7: 'in', 8: 'an', 9: 'on'}
ANCHOR_SET = set(ANCHOR.values())

# Rody phoneme-token vocabulary split by manner (drives the type constraint).
VOWELS = {'i', 'é', 'a', 'o', 'ou', 'u', 'eu', 'in', 'an', 'on',
          'ai', 'oh', 'un', 'oi', 'e', 'et'}
SEMIVOWELS = {'y', 'w'}
CONSONANTS = {'p', 'b', 't', 'd', 'c', 'g', 'm', 'n', 'gn',
              's', 'z', 'ch', 'j', 'f', 'v', 'r', 'l'}

# normalise only spelling variants of the SAME sound; keep genuine vowel
# distinctions (é/ai, o/oh, eu/e-schwa, in/un) separate so each maps to its own slot
VOWEL_ANCHOR = {'et': 'é'}


def dlg_stream(i):
    """Collapsed (bank,P) phoneme sequence for dialogue i (bank 4 / silences dropped)."""
    seq = pp.fmt(pp.preprocess(pp.dialogue_tokens(i)))
    out = []
    for t in seq:
        if not t.startswith('ph') or t.startswith('ph4'):
            continue
        bank = int(t[2])
        P = int(t.split(':P')[1].split('V')[0])
        out.append((bank, P))
    coll = []
    for x in out:
        if not coll or coll[-1] != x:
            coll.append(x)
    return coll


def manner(tok):
    if tok in VOWELS:
        return 'V'
    if tok in SEMIVOWELS:
        return 'S'
    if tok in CONSONANTS:
        return 'C'
    return '?'


def stream_manner(bp):
    return 'V' if bp[0] == 0 else 'C'


def norm_vowel(tok):
    """Collapse near-vowel tokens so 'ai'/'é' etc. don't split votes during EM."""
    return VOWEL_ANCHOR.get(tok, tok)


def make_score(model):
    """Return a substitution scorer. `model` maps (bank,P) -> expected phoneme
    (normalised), learned from the previous EM round; empty on round 0."""
    def score(etok, bp):
        em = manner(etok)
        sm = stream_manner(bp)
        if em == 'V':
            if sm != 'V':
                return -2.0
            want = model.get(bp)
            if want is None:
                return 0.6                       # unknown vowel slot: compatible
            return 3.0 if want == norm_vowel(etok) else -1.0
        if em in ('C', 'S'):
            if sm != 'C':
                return -2.0
            want = model.get(bp)
            if want is None:
                return 0.8
            return 3.0 if want == etok else -0.6
        return -0.5
    return score


GAP = -1.2


def nw_align(E, D, score):
    """Needleman-Wunsch. E: french tokens, D: (bank,P) list. Returns aligned pairs."""
    n, m = len(E), len(D)
    dp = [[0.0] * (m + 1) for _ in range(n + 1)]
    for a in range(1, n + 1):
        dp[a][0] = dp[a - 1][0] + GAP
    for b in range(1, m + 1):
        dp[0][b] = dp[0][b - 1] + GAP
    for a in range(1, n + 1):
        Ea = E[a - 1]
        for b in range(1, m + 1):
            dp[a][b] = max(
                dp[a - 1][b - 1] + score(Ea, D[b - 1]),
                dp[a - 1][b] + GAP,
                dp[a][b - 1] + GAP,
            )
    a, b, out = n, m, []
    while a > 0 or b > 0:
        if a > 0 and b > 0 and dp[a][b] == dp[a - 1][b - 1] + score(E[a - 1], D[b - 1]):
            out.append((E[a - 1], D[b - 1])); a -= 1; b -= 1
        elif a > 0 and dp[a][b] == dp[a - 1][b] + GAP:
            out.append((E[a - 1], None)); a -= 1
        else:
            out.append((None, D[b - 1])); b -= 1
    out.reverse()
    return out


def load_transcripts():
    txt = {}
    for f in sorted(glob.glob(os.path.join(HERE, 'dialogues/rendered/dlg*.txt'))):
        i = int(re.search(r'dlg(\d+)', f).group(1))
        s = open(f).read().strip()
        # drop whisper hallucination lines
        if any(h in s.lower() for h in ('sous-titr', 'sous titr', 'merci', 'abonnez', 'amara')):
            s = re.sub(r'.*(sous-titr|sous titr|merci|abonnez|amara)[^.]*\.?', '', s, flags=re.I)
        txt[i] = s.strip()
    return txt


def expected_tokens(text, lex):
    toks, _ = dd.convert_line(text, lex)
    return [t for t in toks if manner(t) != '?']


def vote_round(pairs_by_dlg, score):
    """Align every dialogue with `score`, return votes[(bank,P)] -> Counter."""
    votes = defaultdict(Counter)
    npairs = 0
    for E, D in pairs_by_dlg:
        for etok, bp in nw_align(E, D, score):
            if etok is None or bp is None:
                continue
            # manner must be compatible with the slot to count as a vote
            if (manner(etok) in ('C', 'S')) != (bp[0] != 0):
                continue
            key = norm_vowel(etok) if bp[0] == 0 else etok
            votes[bp][key] += 1
            npairs += 1
    return votes, npairs


def model_from_votes(votes, min_n=4, min_conf=0.45):
    model = {}
    for bp, c in votes.items():
        tot = sum(c.values())
        lab, cnt = c.most_common(1)[0]
        if tot >= min_n and cnt / tot >= min_conf:
            model[bp] = lab
    return model


def main():
    write = '--write' in sys.argv
    lex = dd.build_dict()
    trans = load_transcripts()

    pairs_by_dlg = []
    for i in sorted(trans):
        if not trans[i]:
            continue
        E = expected_tokens(trans[i], lex)
        D = dlg_stream(i)
        if E and D:
            pairs_by_dlg.append((E, D))

    # EM: round 0 uses manner-only; later rounds score against the learned model.
    model = {}
    for it in range(4):
        votes, npairs = vote_round(pairs_by_dlg, make_score(model))
        new_model = model_from_votes(votes)
        changed = sum(1 for k in set(model) | set(new_model)
                      if model.get(k) != new_model.get(k))
        print(f'round {it}: {npairs} votes, {len(new_model)} labeled slots, {changed} changed')
        model = new_model
        if changed == 0 and it > 0:
            break

    print(f'\naligned {len(pairs_by_dlg)} dialogues\n')

    vrows, crows = [], []
    for bp in sorted(votes):
        c = votes[bp]
        tot = sum(c.values())
        lab, cnt = c.most_common(1)[0]
        row = (bp, lab, cnt, tot, cnt / tot, c.most_common(4))
        (vrows if bp[0] == 0 else crows).append(row)

    print('=== bank-0 vowel/continuant descriptors  (labels.tsv anchor in [])===')
    for (bp, lab, cnt, tot, conf, dist) in sorted(vrows, key=lambda r: r[0][1]):
        anc = ANCHOR.get(bp[1], '?')
        flag = '' if (anc == '?' or anc == lab) else f'  !=anchor[{anc}]'
        mark = '' if (tot >= 5 and conf >= 0.5) else '  (weak)'
        print(f'  b0.P{bp[1]:<3} -> {lab:<3} conf={conf:.2f} n={tot:<4} {dist}{flag}{mark}')

    print('\n=== bank-2/6 consonant descriptors ===')
    for (bp, lab, cnt, tot, conf, dist) in sorted(crows, key=lambda r: -r[3]):
        mark = '' if (tot >= 5 and conf >= 0.5) else '  (weak)'
        print(f'  b{bp[0]}.P{bp[1]:<3} -> {lab:<3} conf={conf:.2f} n={tot:<4} {dist}{mark}')

    if write:
        write_outputs(vrows + crows)


def load_clip_ranges():
    """(bank,P) -> (clip_start, clip_end) from labels.tsv."""
    r = {}
    path = os.path.join(HERE, 'labels.tsv')
    for line in open(path):
        p = line.rstrip('\n').split('\t')
        if len(p) < 5 or not p[0].isdigit():
            continue
        r[(int(p[1]), int(p[0]))] = (p[2], p[3])
    return r


def write_outputs(rows):
    clips = load_clip_ranges()
    out = os.path.join(HERE, 'catalog/phoneme_table.tsv')
    with open(out, 'w') as f:
        f.write('# (bank,P) -> French phoneme, from corpus alignment (align.py).\n')
        f.write('# bank0 = vowels/continuants, bank2/6 = consonant context grains.\n')
        f.write('# Supersedes the P2-P9 ear anchors in labels.tsv (see PROGRESS 2026-07-24).\n')
        f.write('bank\tP\tphoneme\tconf\tvotes\tclip_start\tclip_end\tsource\tdist\n')
        for (bp, lab, cnt, tot, conf, dist) in sorted(rows):
            strong = tot >= 5 and conf >= 0.5
            src = 'aligned' if strong else 'aligned-weak'
            cs, ce = clips.get(bp, ('', ''))
            f.write(f'{bp[0]}\t{bp[1]}\t{lab}\t{conf:.2f}\t{tot}\t{cs}\t{ce}\t{src}\t{dict(dist)}\n')
    print(f'\nwrote {out}')


if __name__ == '__main__':
    main()
