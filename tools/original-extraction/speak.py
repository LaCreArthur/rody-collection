#!/usr/bin/env python3
"""Say NEW French sentences in the authentic 1988 Rody voice.

Inverse of the speech engine: French text -> Rody phonemes -> record u16 tokens
-> the EXISTING bit-exact render (preprocess + interpret). speak.py adds no DSP
and no coarticulation logic of its own: preprocess.py already inserts bank-2/6
selection, bank-4 diphone onsets and pauses. We only choose the phoneme-identity
token per sound, reusing a real per-descriptor token from the corpus so duration
and amplitude stay authentic.

  d2 = tok & 0x3f :  vowel P -> d2=P ; consonant P -> d2=P+0x16 ;
                     word-gap 0x3c ; sentence 0x3d.

Usage:
  speak.py "bonjour arthur"  out.wav
  speak.py --phonemes "b_on_j_ou_r"  out.wav     # skip the dict, give phonemes
"""
import os, sys, json, re
from collections import Counter

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
sys.path.insert(0, os.path.join(HERE, '..', 'phoneme-dict'))
import preprocess as pp
import render_all as ra
import derive_dict as dd

WG, SENT = 0x3c, 0x3d


def load_phoneme_to_d2():
    """token(French rody phoneme) -> d2 byte, from catalog/phoneme_table.tsv.
    bank0 rows are vowels (d2=P); bank2/6 rows are consonants (d2=P+0x16)."""
    best = {}  # phoneme -> (votes, d2)  keep the highest-voted slot per phoneme
    path = os.path.join(HERE, 'catalog/phoneme_table.tsv')
    for line in open(path):
        if line.startswith('#') or line.startswith('bank'):
            continue
        c = line.rstrip('\n').split('\t')
        if len(c) < 5 or not c[0].isdigit():
            continue
        bank, P, ph, votes = int(c[0]), int(c[1]), c[2], int(c[4])
        if ph in ('-', '?'):
            continue
        d2 = P if bank == 0 else P + 0x16
        if ph not in best or votes > best[ph][0]:
            best[ph] = (votes, d2)
    return {ph: d2 for ph, (_, d2) in best.items()}


def build_rep_tokens():
    """d2 -> representative real u16 token (most common in the 102 records)."""
    cache = os.path.join(HERE, 'data/rep_tokens.json')
    if os.path.exists(cache):
        return {int(k): v for k, v in json.load(open(cache)).items()}
    per = {}
    for i in range(102):
        try:
            toks = pp.dialogue_tokens(i)
        except Exception:
            continue
        for t in toks:
            per.setdefault(t & 0x3f, Counter())[t] += 1
    rep = {d2: c.most_common(1)[0][0] for d2, c in per.items()}
    json.dump({str(k): v for k, v in rep.items()}, open(cache, 'w'))
    return rep


PH2D2 = load_phoneme_to_d2()
REP = build_rep_tokens()

# French rody tokens with no direct descriptor: expand to nearest slot phonemes.
EXPAND = {
    'oi': ['ou', 'a'],   # /wa/ glide
    'w': ['ou'],
    'un': ['in'],        # engine has no distinct /oe~/
    'eu': ['e'],         # eu and schwa share the P9/P10 mid-rounded grain
    'gn': ['n', 'y'],    # approximate palatal nasal
    'et': ['é'],         # spelling variant already normalised in table (P1)
}


def phonemes_of(text, lex):
    toks, oov = dd.convert_line(text, lex)
    if oov:
        print(f'  [oov] no dict entry, dropped: {oov}', file=sys.stderr)
    return toks


def word_to_tokens(phon_tokens):
    """Rody phoneme tokens (one word) -> record u16 tokens."""
    out, unmapped = [], []
    for ph in phon_tokens:
        expanded = EXPAND.get(ph, [ph])
        for e in expanded:
            d2 = PH2D2.get(e)
            if d2 is None:
                unmapped.append(e)
                continue
            out.append(REP.get(d2, d2))  # real token if we have one, else bare d2
    return out, unmapped


def synth(words):
    """list of words (each a list of rody phoneme tokens) -> u16 record tokens."""
    tokens, unmapped = [], []
    for wi, w in enumerate(words):
        if wi:
            tokens.append(REP.get(WG, WG))
        wt, um = word_to_tokens(w)
        tokens += wt
        unmapped += um
    tokens.append(REP.get(SENT, SENT))
    return tokens, unmapped


def speak(text, out_path, phonemes=None):
    if phonemes is not None:
        words = [[t for t in w.split('_') if t] for w in phonemes.split()]
    else:
        lex = dd.build_dict()
        words = []
        for w in re.split(r'\s+', text.strip()):
            ph = phonemes_of(w, lex)
            if ph:
                words.append(ph)
    phon_str = ' '.join('_'.join(w) for w in words)
    tokens, unmapped = synth(words)
    if unmapped:
        print(f'  [unmapped phonemes, dropped]: {sorted(set(unmapped))}', file=sys.stderr)
    pcm = ra.interpret(pp.preprocess(tokens))
    ra.wav(pcm, out_path)
    print(f'phonemes: {phon_str}')
    print(f'{len(tokens)} tokens -> {out_path}  ({len(pcm)} samples, {len(pcm)/13000:.2f}s)')
    return phon_str


if __name__ == '__main__':
    args = sys.argv[1:]
    if len(args) >= 3 and args[0] == '--phonemes':
        speak(None, args[2], phonemes=args[1])
    elif len(args) >= 2:
        speak(args[0], args[1])
    else:
        print('usage: speak.py "texte français" out.wav', file=sys.stderr)
        sys.exit(1)
