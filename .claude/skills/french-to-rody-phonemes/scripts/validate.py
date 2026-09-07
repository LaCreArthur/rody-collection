#!/usr/bin/env python3
"""Validate through the game's C# parser. Requires the .NET 9 SDK.

Usage: validate.py "b_r_a_v_o" (or pipe text on stdin).
Exit 0 = valid, exit 1 = invalid tokens or control fields.
"""
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[4]
text = " ".join(sys.argv[1:]) if len(sys.argv) > 1 else sys.stdin.read()
raise SystemExit(subprocess.run([
    'dotnet', 'run', '--project', str(ROOT / 'tools/speech'), '--', 'validate', text
], cwd=ROOT).returncode)
