# RodyMaker - Project Overview

> **Rody et Mastico** is a faithful recreation of a classic Atari ST point-and-click adventure game series, complete with period-accurate limitations and a custom phoneme-based text-to-speech system.

## Game Concept

### Atari ST Recreation
This project recreates the authentic Atari ST experience:

| Limitation | Value | Notes |
|------------|-------|-------|
| **Resolution** | 320 × 240 pixels | Original ST low-res mode |
| **Color Palette** | 16 colors | Per-scene palette restrictions |
| **Audio** | Phoneme-based TTS | Custom recreation of ST speech synthesis |

### Core Gameplay
A point-and-click adventure where players:
1. View a scene with an introductory narration (text-to-speech)
2. Find a **primary object** hidden in the scene
3. Find a **secondary object** ("Ngp" - Near game piece)
4. Optionally find a **third object** ("Fsw" - Final scene win)
5. Progress to the next scene/chapter

## Project Structure

### Story Organization
Stories are stored in `Assets/StreamingAssets/` as separate folders:

```
StreamingAssets/
├── Rody Et Mastico/        # Original story
│   ├── levels.rody         # Scene definitions (text file)
│   ├── credits.txt         # Story credits
│   └── Sprites/            # Scene images (320×130 each)
├── Rody Et Mastico II/
├── Rody Et Mastico III/
├── Rody Et Mastico A Ibiza/
├── Rody Et Mastico V/
├── Rody Et Mastico VI/
├── Rody Noël/              # Christmas special
└── Rody0/                  # Test/template story
```

### Scene Data Format
Each scene in `levels.rody` contains:
- **Phoneme strings** - For TTS narration (intro, object hints, feedback)
- **Display text** - Written text shown to player
- **Music references** - Intro and loop music IDs
- **Pitch values** - Voice pitch modifiers (3 speakers)
- **Speaker flags** - Which character speaks (Mastico/Other)
- **Object zones** - Clickable areas (position + size)

## Phoneme-Based Text-to-Speech

### Overview
The game uses a custom TTS system that concatenates pre-recorded phoneme audio clips to simulate the speech synthesis capabilities of the Atari ST era.

### Phoneme Inventory
Defined in `P.cs` - 42 phonemes total:

| Category | Phonemes |
|----------|----------|
| **Vowels** | `i`, `u`, `ou`, `a`, `oh`, `o`, `et`, `ai`, `eu`, `ee`, `e` |
| **Nasal Vowels** | `an`, `on`, `in`, `un` |
| **Semi-vowels** | `y`, `oi`, `ui` |
| **Consonants** | `l`, `r`, `p`, `t`, `c`, `b`, `d`, `g`, `m`, `n`, `gn`, `s`, `f`, `ch`, `z`, `v`, `j` |
| **Special** | `rien` (silence), `rienp` (pause), `bruitBlanc` (white noise), `ti`, `ouu`, `cuicui` (bird), `pop` |

### Phoneme Syntax
Phoneme strings use underscore-separated tokens with spaces between words:

```
"b_r_a_v_o"                    → "Bravo"
"ouu_i _ s_et_b_i_un"          → "Oui, c'est bien"
"ch_ai_r_ch_an_c_oh_r"         → "Cherche encore"
```

**Syntax Rules:**
- `_` separates phonemes within a word
- ` ` (space) separates words (adds a pause)
- Multiple underscores `__` create longer pauses

### Key Files
| File | Purpose |
|------|---------|
| `SoundManager.cs` | Phoneme playback, pitch control, `StringToPhonemes()` parser |
| `P.cs` | Phoneme index constants (maps names to audio clip indices) |
| `Intro.cs` | Intro sequence with multi-speaker TTS |
| `synth/` folder | Phoneme editor UI for level creator |

## Level Creator (RodyMaker)

The game includes a full level editor allowing users to create custom stories.

### Editor Features
- **Scene Editor** - Create/modify scene images
- **Object Placement** - Define clickable zones for objects
- **Dialogue Editor** - Write phoneme strings for TTS
- **Music Selection** - Choose intro and loop music
- **Animation Frames** - Optional scene animation

### Editor Files (`Assets/Scripts/RodyMaker/`)
| File | Purpose |
|------|---------|
| `RM_SaveLoad.cs` | Save/load story data to StreamingAssets |
| `RM_MainLayout.cs` | Main editor interface |
| `RM_DialLayout.cs` | Dialogue/phoneme editing |
| `RM_ObjLayout.cs` | Object zone placement |
| `RM_ImgAnimLayout.cs` | Animation frame management |

## Story Selection (RodyAnthology)

### Overview
The initial scene presents a scrollable story selection interface.

### Key Files (`Assets/Scripts/RodyAnthology/`)
| File | Purpose |
|------|---------|
| `RA_ScrollView.cs` | Story browser with scroll/selection |
| `RA_NewGame.cs` | Story loading, folder management |
| `RA_SoundManager.cs` | Menu audio |

## Build Scenes

| Index | Scene | Purpose |
|-------|-------|---------|
| 0 | Story Selection | Browse and select stories |
| 1 | Title | Story title screen |
| 2 | Menu | Scene selection menu |
| 3 | Game | Main gameplay scene |
| 5 | Credits | End credits |
| 6 | RodyMaker | Level editor |

---

*See [ARCHITECTURE.md](ARCHITECTURE.md) for code patterns and namespace conventions.*
