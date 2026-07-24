#!/usr/bin/env python3
"""Offline replica of SoundManager phoneme playback -> wav file.

Mirrors StringToPhonemes/getPhoneme/playPhoneme at pitch 1:
- split words on ' ', tokens on '_'
- unknown token -> rienp (slot 36, virgule clip); '.' -> slot 0 (point); '-' -> 37
- each word appends rienp
- each clip contributes (length - 0.01s): next clip cuts the last 10ms
Usage: render.py out.wav "phoneme string"
Close the loop with STT: whisper-cli -m <model> -f out.wav --language fr
"""
import re, sys, wave, pathlib
import numpy as np

ROOT = pathlib.Path(__file__).resolve().parents[4]  # repo root

TOKEN2SLOT = {
    "i":1,"u":2,"ou":3,"a":4,"oh":5,"o":6,"et":7,"ai":8,"eu":9,"ee":10,"e":11,
    "an":12,"on":13,"in":14,"un":15,"y":16,"oi":17,"ui":18,"l":19,"r":20,"p":21,
    "t":22,"c":23,"b":24,"d":25,"g":26,"m":27,"n":28,"gn":29,"s":30,"f":31,
    "ch":32,"z":33,"v":34,"j":35,",":36,".":0,"-":37,"ti":38,"ouu":39,
    "cuicui":40,"pop":41,
}
RIENP = 36

def slot_files():
    guid2wav = {}
    for meta in ROOT.rglob("Assets/Sounds/**/*.meta"):
        if not meta.name.endswith((".wav.meta", ".mp3.meta")):
            continue
        m = re.search(r"guid: (\w+)", meta.read_text())
        if m:
            guid2wav[m.group(1)] = meta.with_suffix("")
    prefab = (ROOT / "Assets/Prefabs/SoundManager.prefab").read_text()
    block = prefab.split("phonemes:")[1].split("sounds_fx_debutObj:")[0]
    slots = []
    for line in block.strip().split("\n"):
        m = re.search(r"guid: (\w+)", line)
        slots.append(guid2wav.get(m.group(1)) if m else None)
    return slots

def load_mono(path):
    w = wave.open(str(path))
    assert w.getsampwidth() == 2 and w.getframerate() == 44100, path
    data = np.frombuffer(w.readframes(w.getnframes()), dtype=np.int16)
    if w.getnchannels() == 2:
        data = data.reshape(-1, 2).mean(axis=1).astype(np.int16)
    return data

def phoneme_indices(s):
    out = []
    for word in s.split(" "):
        for tok in word.split("_"):
            out.append(TOKEN2SLOT.get(tok, RIENP))
        out.append(RIENP)
    return out

def render(s, slots, cache={}):
    cut = int(0.01 * 44100)
    parts = []
    for idx in phoneme_indices(s):
        path = slots[idx] if 0 <= idx < len(slots) else None
        if path is None:  # null clip slot -> skipped by TryAssignPhonemeClip
            continue
        if idx not in cache:
            cache[idx] = load_mono(path)
        parts.append(cache[idx][:-cut])
    return np.concatenate(parts)

if __name__ == "__main__":
    out, text = sys.argv[1], sys.argv[2]
    audio = render(text, slot_files())
    w = wave.open(out, "wb")
    w.setnchannels(1); w.setsampwidth(2); w.setframerate(44100)
    w.writeframes(audio.tobytes())
    w.close()
    print(f"{out}: {len(audio)/44100:.2f}s")
