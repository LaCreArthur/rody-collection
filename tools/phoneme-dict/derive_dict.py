#!/usr/bin/env python3
"""Derive a French word -> Rody-phoneme dictionary from Lexique 3.83.

Usage:
  derive_dict.py bench                 # regression benchmark vs original stories
  derive_dict.py bake out.tsv          # write word<TAB>rody_phonemes dictionary
  add --lexique path/to/Lexique383.tsv (default /tmp/lexique383/Lexique383.tsv;
  download: http://www.lexique.org/databases/Lexique383/Lexique383.zip)

Lexique is CC BY-SA 4.0 (New & Pallier, lexique.org). The baked dictionary is a
derivative and carries the same license; see README.md for attribution.
"""
import csv, re, sys, pathlib, unicodedata, difflib
from collections import defaultdict

ROOT = pathlib.Path(__file__).resolve().parents[2]  # repo root
LEXIQUE = pathlib.Path(sys.argv[sys.argv.index("--lexique") + 1]) if "--lexique" in sys.argv \
    else pathlib.Path("/tmp/lexique383/Lexique383.tsv")

# Words where the game's corpus convention differs from Lexique's transcription.
PHON_OVERRIDES = {"est": "E", "es": "E"}

# Cast and invented words, canonical forms taken from the hand-tuned corpus.
NAMES = {
    "rody": "r_o_d_i", "mastico": "m_a_s_t_i_c_o", "gobino": "g_o_b_i_n_o",
    "gaëtan": "g_a_et_t_an", "gaëlle": "g_a_ai_l", "badedon": "b_a_d_e_d_on",
    "coa": "c_o_a", "ibiza": "i_b_i_z_a",
}

# Lexique ASCII-SAMPA -> Rody tokens. Digraphs first.
DIGRAPHS = {"wa": ["oi"]}
MONO = {
    "a": ["a"], "i": ["i"], "y": ["u"], "u": ["ou"], "o": ["o"], "O": ["oh"],
    "e": ["et"], "E": ["ai"], "°": ["e"], "2": ["eu"], "9": ["eu"],
    "@": ["an"], "§": ["on"], "5": ["in"], "1": ["un"],
    "j": ["y"], "w": ["ou"], "8": ["u"],
    "p": ["p"], "b": ["b"], "t": ["t"], "d": ["d"], "k": ["c"], "g": ["g"],
    "m": ["m"], "n": ["n"], "N": ["gn"], "G": ["g"],
    "s": ["s"], "z": ["z"], "S": ["ch"], "Z": ["j"], "f": ["f"], "v": ["v"],
    "R": ["r"], "l": ["l"], "x": ["r"],
}

def phon_to_rody(phon):
    out, i = [], 0
    while i < len(phon):
        if phon[i:i+2] in DIGRAPHS:
            out += DIGRAPHS[phon[i:i+2]]
            i += 2
        elif phon[i] in MONO:
            out += MONO[phon[i]]
            i += 1
        else:
            raise ValueError(f"unmapped symbol {phon[i]!r} in {phon!r}")
    return out

def build_dict():
    best = {}  # ortho -> (freq, phon)
    with open(LEXIQUE, encoding="utf-8") as f:
        for row in csv.DictReader(f, delimiter="\t"):
            ortho, phon = row["ortho"], row["phon"]
            try:
                freq = float(row["freqfilms2"] or 0)
            except ValueError:
                freq = 0.0
            if not ortho or not phon or " " in ortho:
                continue
            if ortho not in best or freq > best[ortho][0]:
                best[ortho] = (freq, phon)
    return {w: p for w, (_, p) in best.items()}

def words_of(text):
    t = text.lower().replace("’", "'").replace("œ", "oe")
    t = re.sub(r"[^a-zàâäéèêëîïôöùûüçæ' -]", " ", t)
    words = []
    for chunk in t.split():
        for part in chunk.split("-"):
            while "'" in part:
                head, part = part.split("'", 1)
                if head:
                    words.append(head + "'")
            if part:
                words.append(part)
    return words

def convert_line(text, lex):
    toks, oov = [], []
    for w in words_of(text):
        if w in NAMES:
            toks += NAMES[w].split("_")
            continue
        cand = [w, w.rstrip("'")] if w.endswith("'") else [w]
        phon = PHON_OVERRIDES.get(w) or next((lex[c] for c in cand if c in lex), None)
        if phon is None:
            oov.append(w)
        else:
            toks += phon_to_rody(phon)
    return toks, oov

def corpus_pairs():
    pairs = []
    for f in sorted(ROOT.glob("original-stories/*/levels.rody")):
        if f.parent.name == "Rody0":
            continue
        for scene in f.read_text(encoding="utf-8").split("\n~\n"):
            lines = scene.split("\n")
            try:
                pi = next(i for i, l in enumerate(lines) if l.startswith("## phonems"))
                ti = next(i for i, l in enumerate(lines) if l.startswith("## texts"))
                mi = next(i for i, l in enumerate(lines) if l.startswith("## musics"))
            except StopIteration:
                continue
            phon = [l.strip() for l in lines[pi+1:ti] if l.strip()]
            texts = [l.strip() for l in lines[ti+1:mi] if l.strip()]
            if len(texts) == len(phon) + 1:
                pairs += [(t, p, f.parent.name) for p, t in zip(phon, texts[1:])
                          if t.strip("-").strip()]
    return pairs

def hand_tokens(s):
    return [t for w in s.split(" ") for t in w.split("_")
            if t not in ("", ",", ".", "-", "ti", "ouu", "cuicui", "pop")]

def bake(out_path):
    lex = build_dict()
    n = 0
    with open(out_path, "w", encoding="utf-8") as out:
        for w in sorted(lex):
            try:
                toks = phon_to_rody(PHON_OVERRIDES.get(w, lex[w]))
            except ValueError:
                continue
            out.write(f"{w}\t{'_'.join(toks)}\n")
            n += 1
        for w, s in sorted(NAMES.items()):
            out.write(f"{w}\t{s}\n")
            n += 1
    print(f"{out_path}: {n} entries")


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "bake":
        bake(sys.argv[2])
        sys.exit(0)
    lex = build_dict()
    print(f"lexicon: {len(lex)} word forms")
    pairs = corpus_pairs()
    scores, oov_all, results = [], [], []
    for text, phon, story in pairs:
        got, oov = convert_line(text, lex)
        want = hand_tokens(phon)
        s = difflib.SequenceMatcher(None, want, got).ratio()
        scores.append(s)
        oov_all += oov
        results.append((s, story, text, want, got, oov))
    scores_sorted = sorted(scores)
    n = len(scores)
    total_words = sum(len(words_of(t)) for t, _, _ in pairs)
    print(f"{n} lines | token agreement mean {sum(scores)/n:.2f} "
          f"median {scores_sorted[n//2]:.2f} | >=0.8: {sum(s>=0.8 for s in scores)}/{n} "
          f"| <0.6: {sum(s<0.6 for s in scores)}")
    print(f"OOV: {len(oov_all)}/{total_words} words ({100*len(oov_all)/total_words:.1f}%)")
    from collections import Counter
    print("top OOV:", Counter(oov_all).most_common(15))
    results.sort()
    print("\nworst 5:")
    for s, story, text, want, got, oov in results[:5]:
        print(f"  {s:.2f} [{story}] {text[:90]}")
        print(f"     hand: {'_'.join(want)[:100]}")
        print(f"     dict: {'_'.join(got)[:100]}  OOV={oov}")
