# French -> Rody phoneme dictionary

Offline pipeline that derives a `word<TAB>rody_phonemes` dictionary for the
in-editor "generate phonemes from text" feature, from the Lexique French
lexical database.

## Usage

```bash
# one-time: fetch Lexique 3.83
curl -sL -o /tmp/Lexique383.zip "http://www.lexique.org/databases/Lexique383/Lexique383.zip"
unzip /tmp/Lexique383.zip -d /tmp/lexique383

# regression benchmark: dictionary output vs the hand-tuned original stories
python3 tools/phoneme-dict/derive_dict.py bench

# bake the shipping dictionary
python3 tools/phoneme-dict/derive_dict.py bake rody-fr-dict.tsv
```

Benchmark baseline (2026-07-20): token agreement vs the 326 original story
lines: mean 0.94, median 0.95, OOV 1.6% (typos and English loanwords). Treat
any change that lowers this as a regression.

## License / attribution

The baked dictionary is a derivative of
[Lexique](http://www.lexique.org) (Boris New & Christophe Pallier) and is
licensed **CC BY-SA 4.0** like its source. Ship an attribution line with the
game credits: "Dictionnaire phonétique dérivé de Lexique (lexique.org),
New & Pallier, CC BY-SA 4.0."

The derivation script itself follows the repository license.
