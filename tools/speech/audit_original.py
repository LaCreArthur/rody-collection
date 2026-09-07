#!/usr/bin/env python3
"""Run original Rody 1 68000 code in Hatari and compare the runtime C# engine.

Requires Hatari, .NET 9 and a user-supplied TOS ROM. No Python speech port is
used as an oracle. Generated debugger scripts, native bytes and manifest stay
in the printed temporary directory for inspection. No Unity build is run.
"""
import argparse
import hashlib
import itertools
import json
import os
from pathlib import Path
import struct
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'tools/original-extraction'
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--tos', required=True, type=Path)
args = parser.parse_args()
out = Path(tempfile.mkdtemp(prefix='rody-original-'))
print(f'Audit artifacts: {out}', flush=True)

# Independently extract the two files through the disk's FAT12 root and chains.
disk = (SOURCE / 'captures/rody1.st').read_bytes()
u16 = lambda p: struct.unpack_from('<H', disk, p)[0]
sector, spc, reserved = u16(11), disk[13], u16(14)
entries, fat_sectors = u16(17), u16(22)
fat = disk[reserved * sector:(reserved + fat_sectors) * sector]
root = (reserved + disk[16] * fat_sectors) * sector
data = root + ((entries * 32 + sector - 1) // sector) * sector
files = {}
for offset in range(root, root + entries * 32, 32):
    entry = disk[offset:offset + 32]
    if not entry[0]:
        break
    if entry[0] == 229 or entry[11] & 0x18:
        continue
    name = entry[:8].decode('ascii').strip() + '.' + entry[8:11].decode('ascii').strip()
    if name not in ('AAA.PRG', 'PA.ROD'):
        continue
    cluster, size = struct.unpack_from('<H', entry, 26)[0], struct.unpack_from('<I', entry, 28)[0]
    chunks, seen = [], set()
    while 2 <= cluster < 0xff8:
        assert cluster not in seen, 'FAT cycle'
        seen.add(cluster)
        start = data + (cluster - 2) * spc * sector
        chunks.append(disk[start:start + spc * sector])
        value = int.from_bytes(fat[cluster * 3 // 2:cluster * 3 // 2 + 2], 'little')
        cluster = value >> 4 if cluster & 1 else value & 4095
    files[name] = b''.join(chunks)[:size]
    assert files[name] == (SOURCE / 'banks' / ('rody1_' + name)).read_bytes(), name
    (out / name).write_bytes(files[name])
program, bank = files['AAA.PRG'], files['PA.ROD']
assert hashlib.sha256(program).hexdigest() == '71f3d429a18b977f7e5b6c95969a77f537dc8eedfab438995651a7fab27fa321'
assert (ROOT / 'Assets/Resources/Speech/Rody1.bytes').read_bytes() == bank
assert (ROOT / 'Assets/Resources/Speech/Tables.bytes').read_bytes() == program[28 + 0x4aee:28 + 0x4cc0]

# GEMDOS relocations only: executable instructions are otherwise unmodified.
size = struct.unpack_from('>I', program, 2)[0]
code = bytearray(program[28:28 + size])
rel = 28 + size
pos = struct.unpack_from('>I', program, rel)[0]
rel += 4
while pos:
    struct.pack_into('>I', code, pos, struct.unpack_from('>I', code, pos)[0] + 0x10000)
    while True:
        delta = program[rel]
        rel += 1
        if delta == 1:
            pos += 254
            continue
        pos = pos + delta if delta else 0
        break
(out / 'code.bin').write_bytes(code)

def script(name, lines):
    path = out / (name + '.ini')
    path.write_text('\n'.join(lines) + '\n')
    return path

def emulate(name, setup):
    start = script(name + '-start', [f'loadbin {out}/code.bin $10000', *setup])
    boot = script(name + '-boot', [f'breakpoint VBL = 100 :once :trace :file {start}'])
    with (out / (name + '.log')).open('w') as log:
        subprocess.run(['hatari', '--tos', str(args.tos.expanduser().resolve()), '--machine', 'ste',
                        '--memsize', '4', '--fast-boot', 'on', '--fast-forward', 'on',
                        '--sound', 'off', '--confirm-quit', 'off', '--parse', str(boot)],
                       env={**os.environ, 'SDL_VIDEODRIVER': 'dummy', 'SDL_AUDIODRIVER': 'dummy'},
                       stdout=log, stderr=log, check=True, timeout=300)

headers = struct.unpack_from('>148H', bank)
count = next(i for i in range(147) if headers[i + 1] <= headers[i])
assert count == 101
cases = []
for i in range(count):
    tokens = list(struct.unpack_from('>' + str((headers[i + 1] - headers[i]) // 2) + 'H', bank, 0x12c + headers[i]))
    cases.append(dict(id=f'record-{i:03d}', tokens=tokens, record=i + 1))
descriptors = sorted({t & 63 for c in cases for t in c['tokens']})
for duration in range(16):
    tokens = []
    for d, gain, rate in itertools.product(descriptors, range(8), range(8)):
        tokens.extend([0x1203, duration << 12 | gain << 9 | rate << 6 | d, 0x1200, 0x3c])
    for i in range(0, len(tokens), 512):
        cases.append(dict(id=f'fields-{duration}-{i}', tokens=tokens[i:i + 512]))
tokens = []
for a, b, c in itertools.product(range(64), repeat=3):
    tokens.extend([0x1200 | a, 0x1200 | b, 0x1200 | c, 0x3d])
    if len(tokens) == 1024:
        cases.append(dict(id=f'context-{len(cases)}', tokens=tokens))
        tokens = []
for i, case in enumerate(cases):
    name = case['id']
    source = out / 'PA.ROD'
    if 'record' not in case:
        raw = bytearray(0x12c)
        struct.pack_into('>H', raw, 2, len(case['tokens']) * 2)
        raw.extend(struct.pack('>' + str(len(case['tokens'])) + 'H', *case['tokens']))
        source = out / (name + '.input')
        source.write_bytes(raw)
    capture = script(name + '-capture', [f'savebin {out}/{name}.commands $60000 "a4 - $60000 + 1"',
        *([f'parse {out}/{cases[i + 1]["id"]}.ini'] if i + 1 < len(cases) else ['quit', 'cont'])])
    script(name, [f'loadbin {source} $30000', f'memwrite w $14cc0 {case.get("record", 1)}',
                  'cpureg sr=$2700', 'cpureg a7=$90000', 'cpureg pc=$11642',
                  f'breakpoint pc = $116ce :once :trace :file {capture}'])
emulate('commands', ['memwrite l $14cc4 $30000', 'memwrite l $14a8e $60000',
                     'memwrite w $14cc2 $ffff', 'memwrite w $14d20 0',
                     f'parse {out}/{cases[0]["id"]}.ini'])
print(f'Captured {len(cases)} original command streams.', flush=True)

# Execute the actual sample-load/amplitude instructions for every possible byte.
(out / 'samples.bin').write_bytes(bytes(range(256)))
store = 'memwrite b "$61000 + a0 - $70001" "d0"'
sample = script('sample', [store, 'cpureg pc=$119ce'])
for level, (half, quarter) in enumerate([(0, 0), (-1, 0), (0, -1), (0, 1), (1, 0)]):
    end = script(f'amplitude-end-{level}', [store, f'savebin {out}/amplitude-{level}.pcm $61000 256',
        *([f'parse {out}/amplitude-{level + 1}.ini'] if level < 4 else ['quit', 'cont'])])
    script(f'amplitude-{level}', [f'memwrite w $14a9c ${half & 65535:04x}',
        f'memwrite w $14a9e ${quarter & 65535:04x}', 'cpureg a0=$70000', 'cpureg d0=0',
        'cpureg pc=$119ce', f'breakpoint pc = $11a36 && a0 = $70100 :once :trace :file {end}'])
emulate('amplitude', [f'loadbin {out}/samples.bin $70000', 'cpureg sr=$2700', 'cpureg a7=$90000',
                     f'breakpoint pc = $11a36 && a0 < $70100 :trace :file {sample}',
                     f'parse {out}/amplitude-0.ini'])
(out / 'manifest.json').write_text(json.dumps({'records': cases}))
subprocess.run(['dotnet', 'run', '--project', str(ROOT / 'tools/speech'), '--', 'audit-original', str(out)], check=True)
print('Disk extraction and runtime bank/tables match. No hardware timing or PSG waveform claim.')
