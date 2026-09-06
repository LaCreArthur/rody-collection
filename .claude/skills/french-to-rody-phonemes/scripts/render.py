#!/usr/bin/env python3
"""Render with the game's C# speech engine. Requires the .NET 9 SDK.

Usage: render.py out.wav "phonemes" [pitch]
Unity performs its own final audio resampling; this preview writes a 44.1 kHz WAV.
"""
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[4]
if len(sys.argv) not in (3, 4):
    raise SystemExit('Usage: render.py out.wav "phonemes" [pitch]')
raise SystemExit(subprocess.run([
    'dotnet', 'run', '--project', str(ROOT / 'tools/speech'), '--', 'render', *sys.argv[1:]
], cwd=ROOT).returncode)
