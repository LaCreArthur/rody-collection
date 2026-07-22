---
name: french-to-rody-phonemes
description: Convert French text into the Rody Collection phoneme notation used by the game's Atari-ST-style sampled TTS (e.g. "bravo" -> b_r_a_v_o). Use whenever writing or editing spoken dialogue for Rody stories - .rody.json files, levels.rody files, the RodyMaker story editor synth field - or whenever the user asks to make the game "say" something in French. Also works in reverse, decoding an existing phoneme string back to French. Not for natural/neural TTS voices; this notation drives a deliberately robotic 1988-style voice.
---

# French to Rody Phonemes

The game speaks by concatenating ~40 pre-recorded phoneme clips, exactly like the
original Atari ST games (Rody et Mastico, Lankhor, 1988). A dialogue is a plain
string: `_` separates phonemes inside a breath group, a space separates groups
(and inserts a short pause). Example:

```
r_o_d_i m_a_m_an_a_ou_v_ai_r_d_ou_s_e_m_an_l_a_p_oh_r_t_d_e_t_a_ch_an_b_r
```
reads: "Rody, maman a ouvert doucement la porte de ta chambre."

**Critical property of the engine: any token that is not in the inventory below
plays as a SILENT PAUSE, with no error and no warning.** A capital letter, an
accent, or a typo silently eats a sound (the original data contains `M_a_l_eu_r`
for "Malheur", which plays "alheur"). Always validate before delivering.

## Phoneme inventory (the ONLY valid tokens)

### Vowels

| Token | Sound (IPA) | French spellings | Example |
|-------|-------------|------------------|---------|
| `a`  | /a/ | a, à, â | chat -> `ch_a` |
| `i`  | /i/ | i, y | Rody -> `r_o_d_i` |
| `u`  | /y/ | u | tu -> `t_u` |
| `ou` | /u/ | ou; also glide /w/ | loup -> `l_ou`, oui -> `ou_i` |
| `o`  | /o/ closed | au, eau, ô, final -o | bateau -> `b_a_t_o`, gros -> `g_r_o` |
| `oh` | /ɔ/ open | o in closed syllable | porte -> `p_oh_r_t`, encore -> `an_c_oh_r` |
| `et` | /e/ closed | é, -er, -ez, les, et | été -> `et_t_et`, chercher -> `ch_ai_r_ch_et` |
| `ai` | /ɛ/ open | è, ê, ai, ei, e+double cons. | elle -> `ai_l`, mer -> `m_ai_r`, est -> `ai` |
| `e`  | /ə/ and /ø/ | e muet, closed eu | le -> `l_e`, bleu -> `b_l_e`, peut -> `p_e` |
| `eu` | /œ/ open | eu/œ before consonant | peur -> `p_eu_r`, œuf -> `eu_f`, œil -> `eu_y` |
| `an` | /ɑ̃/ | an, en, am, em | chambre -> `ch_an_b_r`, comment -> `c_o_m_an` |
| `on` | /ɔ̃/ | on, om | bonjour -> `b_on_j_ou_r` |
| `in` | /ɛ̃/ | in, ain, ein, un | lapin -> `l_a_p_in`, un -> `in` |
| `un` | /œ̃/ | un (rare; corpus prefers `in`) | brun -> `b_r_un` |
| `oi` | /wa/ | oi | moi -> `m_oi` |
| `y`  | /j/ | -il(l), y between vowels | soleil -> `s_o_l_ai_y`, rayon -> `r_ai_y_on` |
| `ui` | /ɥi/ | ui (corpus usually writes `u_i`) | fruit -> `f_r_u_i` |
| `ee` | (none) | onomatopoeia only (cow "meuh") | `m_ee_ee` |

### Consonants

| Token | Sound | French spellings | Example |
|-------|-------|------------------|---------|
| `p` `b` `t` `d` `f` `v` `m` `n` `l` `r` | as written | | robot -> `r_o_b_o` |
| `c`  | /k/ | c, k, qu | quel -> `c_ai_l`, magique -> `m_a_j_i_c` |
| `g`  | /g/ hard only | g, gu | regarde -> `r_e_g_a_r_d` |
| `j`  | /ʒ/ | j, ge, gi | nuage -> `n_u_a_j`, magie -> `m_a_j_i` |
| `s`  | /s/ | s, ç, ss, ti(on) | ça -> `s_a` |
| `z`  | /z/ | z, s between vowels, liaison | maison -> `m_et_z_on` |
| `ch` | /ʃ/ | ch | chose -> `ch_o_z` |
| `gn` | /ɲ/ | gn (also English -ing) | gagné -> `g_a_gn_et` |

### Pauses and specials

| Token | Effect |
|-------|--------|
| space | end of breath group + short pause |
| `,` | short pause (write it as a token: `r_o_d_i_,_v_i_in`) |
| `.` | longer silence between sentences: `..._r_o_b_o_._m_e_v_oi_t_u...` |
| `__` (double underscore) | extra pause inside a group; `___`/`____` for longer |
| `-` | white-noise burst (static/glitch effect) |
| `ti` | /t/ clip with an i-color; stylistic before u/i: vois-tu -> `v_oi_ti_u` |
| `ouu` | emphatic long "ou": oui! -> `ouu_i` |
| `cuicui` | bird chirp sound effect |
| `pop` | pop sound effect |

## Conversion algorithm

1. **Say the sentence out loud in French. Transcribe the sounds, never the
   spelling.** Every silent letter disappears: final -s, -t, -d, -x, -p, mute h,
   -ent verb endings. "ils parlent" -> `i_l_p_a_r_l`.
2. **Drop the final schwa.** porte -> `p_oh_r_t`, montre -> `m_on_t_r`,
   monstre -> `m_on_s_t_r`. Keep interior schwas that are pronounced:
   doucement -> `d_ou_s_e_m_an`.
3. **Join words that flow in one breath into one `_` group.** A short sentence
   is typically one single group. Use spaces (or `_,_` / `_._`) where a speaker
   would breathe. Rhythm is the main charm lever: too few pauses = drone, too
   many = choppy. Intelligibility tip (STT-verified): joining many content words
   into one dense group hurts word segmentation for the listener. Prefer spaces
   between content words in long sentences; only join words a liaison or clitic
   glues together (nous_avons, l'école, vas-tu).
4. **Write liaisons explicitly where a French speaker makes them:**
   nous avons -> `n_ou_z_a_v_on`, les oiseaux -> `l_et_z_oi_z_o`,
   un œuf -> `in_n_eu_f`, est-il -> `ai_t_i_l`, très étonné -> `t_r_ai_z_et_t_o_n_et`.
   Aspirated h blocks liaison: insert a `__` pause instead, la hache -> `l_a__a_ch`.
5. **Numbers and abbreviations are spelled out as sounds:**
   56 -> `s_in_c_an_t_s_i_s`.
6. **Everything lowercase, no accents, no punctuation other than `,` `.` `-`
   used as tokens.** Question marks and exclamations have no token; intonation
   comes from the per-dialogue pitch setting in the editor, not the string.
7. **Validate** (see below), then read the result back sound by sound to check
   it against the original sentence.

### Frequent-spelling cheat sheet

| Spelling | Tokens | Example |
|----------|--------|---------|
| -tion | `s_i_on` | félicitations -> `f_et_l_i_s_i_t_a_s_i_on` |
| oin | `ou_in` | besoin -> `b_e_z_ou_in`, loin -> `l_ou_in` |
| ien | `i_in` | bien -> `b_i_in`, viens -> `v_i_in` |
| ill/eil | `i_y` / `ai_y` | fille -> `f_i_y`, soleil -> `s_o_l_ai_y` |
| yeux | `i_e` | les yeux -> `l_et_z_i_e` |
| qu | `c` | quel -> `c_ai_l` |
| ph | `f` | phare -> `f_a_r` |
| x | `c_s` or `g_z` | exact -> `ai_g_z_a_c_t` |
| w (loanwords) | `ou` | chewing-gum -> `ch_e_ou_i_n_g_oh_m` |

## Worked examples (from the original games)

| French | Phonemes |
|--------|----------|
| Bravo ! | `b_r_a_v_o` |
| Non, recommence. | `n_on _ r_e_c_o_m_an_s` |
| Nous avons gagné, Rody ! | `n_ou_z_a_v_on_g_a_gn_et___r_o_d_i` |
| Où est la porte magique ? | `ou_et_l_a_p_oh_r_t_m_a_j_i_c` |
| Je suis Mastico, le robot. Me vois-tu sur l'écran ? | `j_e_s_u_i_m_a_s_t_i_c_o_._l_e_r_o_b_o_._m_e_v_oi_t_u_s_u_r_l_et_c_r_an` |
| Un lapin observe Rody. Où est-il ? | `in_l_a_p_in__o_b_s_ai_r_v_r_o_d_i_._ou_et_t_i_l` |
| Cette porte n'est pas fermée. Entrons ! | `s_ai_t_p_oh_r_t__n_ai_p_a_f_ai_r_m_et_._an_t_r_on` |

More curated pairs grouped by phenomenon: `references/corpus-examples.md`.
The full corpus (633 lines) lives in `original-stories/*/levels.rody`;
compare the `## phonems` section against the `## texts [string]` section.

## Reverse direction (phonemes to French)

Read each token with the table above and say it out loud; French will emerge.
`s_et_b_i_un` -> "sé-bi-un" -> "c'est bien". Useful for proofreading existing
stories or explaining a string to a player.

## Validation

```bash
python3 .claude/skills/french-to-rody-phonemes/scripts/validate.py "b_r_a_v_o l_e_v_o"
```

Exits non-zero and lists every invalid token (each one would play as a silent
pause in game). Run it on every string you produce. The most common failures:
uppercase letters, accented characters (é, à), and spelled-out French instead
of sounds ("est" instead of `ai`).

To hear the result without launching Unity, render it to a wav with the exact
game playback (same clips, same timing):

```bash
python3 .claude/skills/french-to-rody-phonemes/scripts/render.py /tmp/out.wav "b_r_a_v_o l_e_v_o"
```

Full closed loop: transcribe the wav with local STT and compare to the intended
French (`whisper-cli -m ~/voice-agent/models/ggml-large-v3.bin -f /tmp/out.wav
--language fr --no-timestamps`). Calibration from the original stories: the
shipped corpus itself scores ~0.7 mean / 0.77 median word agreement, so treat
that as the ceiling. The score is a strict lower bound on human intelligibility:
lines scoring as low as 0.27-0.53 have been human-verified as perfectly clear
(whisper trips on short lines, proper nouns, and a few clip confusions like
t-before-u heard as "p"). A high score proves the line works; a low score only
means "check by ear" - never rewrite a natural-sounding line to chase the score.
