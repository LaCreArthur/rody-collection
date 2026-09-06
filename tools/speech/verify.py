#!/usr/bin/env python3
"""Compare the actual Unity speech source against the Python extraction reference.

Runs a small .NET executable, not Unity or a Unity build. Fixtures are temporary.
Also renders the embedded story dialogue and fixed feedback vocabulary.
"""
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import tempfile

ROOT = Path(__file__).resolve().parents[2]
REFERENCE = ROOT / 'tools/original-extraction'
sys.path.insert(0, str(REFERENCE))
# The baseline includes amplitude and speed; don't accidentally compare an ablation.
os.environ.pop('NOAMP', None)
os.environ.pop('NOSPEED', None)
import preprocess as pp
import render_all as renderer
import speak

runtime = ROOT / 'Assets/Resources/Speech'
assert (runtime / 'Rody1.bytes').read_bytes() == pp.PA, 'Runtime bank differs from extracted bank'
assert (runtime / 'Tables.bytes').read_bytes() == bytes(pp.MEM[i] for i in range(0x4b00, 0x4cc0)), 'Runtime tables differ from captured lookup memory'

with tempfile.TemporaryDirectory(prefix='rody-speech-') as temp:
    folder = Path(temp)
    records, notation = [], []
    for i in range(102):
        name = f'dlg{i:03d}'
        tokens = pp.dialogue_tokens(i)
        commands = pp.preprocess(tokens)
        records.append({'id': name, 'tokens': tokens})
        (folder / f'{name}.commands').write_bytes(bytes(commands))
        (folder / f'{name}.pcm').write_bytes(renderer.interpret(commands))

    def notation_case(name, text, tokens):
        filename = name + '.pcm'
        (folder / filename).write_bytes(renderer.interpret(pp.preprocess(tokens)) if tokens else b'')
        notation.append({'id': name, 'text': text, 'expected': filename})

    # Canonical mapping is derived independently from the original descriptor catalog,
    # not copied from C#. This catches a correct renderer wired to the wrong phonemes.
    for phoneme in ['i', 'et', 'ai', 'a', 'oh', 'o', 'ou', 'u', 'e', 'eu', 'in', 'un',
                    'an', 'on', 'p', 'b', 'm', 'f', 'v', 't', 'd', 'n', 's', 'z', 'l',
                    'ch', 'j', 'c', 'g', 'r', 'y', 'oi', 'gn']:
        tokens, unknown = speak.word_to_tokens([phoneme])
        assert not unknown, unknown
        notation_case('phoneme_' + phoneme, phoneme, tokens + [0x3c])
    notation_case('empty', '', [])
    notation_case('blank', ' ', [])
    notation_case('multiline', 'a\ni\tu', [0x1203, 0x3c, 0x1200, 0x3c, 0x1208, 0x3c])
    notation_case('ui', 'ui', [0x1208, 0x1200, 0x3c])
    notation_case('ouu', 'ouu', [0x2207, 0x3c])
    notation_case('ee', 'ee', [0x3209, 0x3c])
    notation_case('authored_pauses', 'a___i , . u', [0x1203, 0x3c, 0x3c, 0x1200,
                                                   0x3c, 0x3c, 0x3c, 0x3d, 0x3c, 0x1208, 0x3c])
    # The ti recording is the native bank-2 t. Test both sides of contextual bank selection.
    for vowel in ['i', 'et', 'u', 'a', 'on', 'e']:
        token = speak.word_to_tokens([vowel])[0][0]
        t = 0x3a1b if vowel in ['i', 'et', 'u'] else 0x8a1b
        notation_case('ti_' + vowel, 'ti_' + vowel, [t, token, 0x3c])
    notation_case('ti_word_boundary', 'ti a', [0x8a1b, 0x3c, 0x1203, 0x3c])
    notation.append({'id': 'effect_order', 'text': 'a_-_cuicui_pop_i', 'effects': ['Noise', 'Bird', 'Pop']})

    for path in sorted((ROOT / 'Assets/Resources/Stories').glob('*.rody.json')):
        story = json.loads(path.read_text())
        for index, scene in enumerate(story['scenes']):
            for field, text in scene['data']['dialogues'].items():
                if not isinstance(text, str):
                    continue
                tokens = [p for word in text.split(' ') for p in word.split('_')]
                effects = [{'cuicui': 'Bird', 'pop': 'Pop', '-': 'Noise'}[p]
                           for p in tokens if p in ('cuicui', 'pop', '-')]
                notation.append({'id': f'{path.stem}:{index}:{field}', 'text': text, 'effects': effects})
    source = (ROOT / 'Assets/Scripts/SoundManager.cs').read_text()
    feedback = source[source.index('public string RandomOui'):source.index('public AudioClip getMusic')]
    for index, text in enumerate(re.findall(r'return "([^"]*)";', feedback)):
        notation.append({'id': f'feedback:{index}', 'text': text, 'effects': []})
    (folder / 'manifest.json').write_text(json.dumps({'records': records, 'notation': notation}))
    subprocess.run(['dotnet', 'run', '--project', str(ROOT / 'tools/speech'), '--', 'verify', str(folder)],
                   cwd=ROOT, check=True)
print('Runtime bank and tables match their extracted sources.')
