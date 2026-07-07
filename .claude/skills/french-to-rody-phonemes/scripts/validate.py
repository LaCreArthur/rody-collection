#!/usr/bin/env python3
"""Validate a Rody phoneme string.

Every token must map to a sampled clip in the game's SoundManager;
anything else plays as a silent pause with no warning in game.

Usage: validate.py "b_r_a_v_o l_e_v_o"   (or pipe the string on stdin)
Exit 0 = all tokens valid, exit 1 = invalid tokens found (listed on stdout).
"""
import sys

VALID = {
    # vowels
    "a", "i", "u", "ou", "o", "oh", "et", "ai", "e", "eu", "ee",
    "an", "on", "in", "un", "oi", "ui", "y",
    # consonants
    "l", "r", "p", "t", "c", "b", "d", "g", "m", "n", "gn",
    "s", "f", "ch", "z", "v", "j",
    # pauses and specials
    ",", ".", "-", "ti", "ouu", "cuicui", "pop",
    "",  # empty token from "__" = deliberate extra pause
}


def invalid_tokens(s):
    return [tok for word in s.split(" ") for tok in word.split("_")
            if tok not in VALID]


if __name__ == "__main__":
    text = " ".join(sys.argv[1:]) if len(sys.argv) > 1 else sys.stdin.read()
    bad = invalid_tokens(text.strip())
    if bad:
        print("INVALID tokens (each plays as a silent pause in game):")
        for tok in sorted(set(bad)):
            print(f"  {tok!r}")
        sys.exit(1)
    print("OK: all tokens valid")
