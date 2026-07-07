# Curated corpus examples

Real pairs from the original 1988-1992 games, grouped by the phenomenon they
illustrate. TX = display text, PH = phoneme string as shipped.

The originals are not perfectly consistent (bien appears as both `b_i_in` and
`b_i_un`, magasin as `m_a_g_a_z_in` and `m_a_g_a_z_un`). When in doubt, follow
SKILL.md, not a single corpus line.

To regenerate the full 633-line corpus from the repo root:

```bash
python3 -c "
import os, re
for story in sorted(os.listdir('original-stories')):
    f = os.path.join('original-stories', story, 'levels.rody')
    if not os.path.isfile(f): continue
    txt = open(f, encoding='utf-8').read()
    for m in re.finditer(r'## phonems\n(.*?)## texts \[string\]\n(.*?)## musics', txt, re.S):
        ph = [l.strip() for l in m.group(1).strip().split('\n') if l.strip()]
        tx = [l.strip() for l in m.group(2).strip().split('\n') if l.strip()]
        if len(tx) == len(ph) + 1:
            for p, t in zip(ph, tx[1:]):
                print('TX:', t); print('PH:', p); print()
"
```

## Short questions (one breath group, final schwa dropped)

```
TX: Montre le phare.
PH: m_on_t_r_l_e_f_a_r_

TX: Où est le tuba de Rody?
PH: ou_et_l_e_t_u_b_a_d_e_r_o_d_i_

TX: Où est la bouche du volcan?
PH: ou_et_l_a_b_ou_ch_d_u_v_o_l_c_an_

TX: Vois-tu la chauve-souris?
PH: v_oi_t_u_l_a_ch_o_v_s_ou_r_i_

TX: Quel est le nuage qui parle?
PH: c_ai_l_et_l_e_n_u_a_j_c_i_p_a_r_l_
```

## Liaison written explicitly

```
TX: Montre les yeux de la colombe.
PH: m_on_t_r_l_et_z_i_e_d_e_l_a_c_o_l_on_b_

TX: Cherche un corbeau parmi les oiseaux?
PH: ch_ai_r_ch_in_c_oh_r_b_o_p_a_r_m_i_l_et_z_oi_z_o_

TX: De quoi Bob est très étonné?
PH: d_e_c_oi_b_oh_b_et_t_r_ai_z_et_t_o_n_et_

TX: Vois-tu le poussin qui sort d'un oeuf?
PH: v_oi_t_u_l_e_p_ou_s_in_c_i_s_oh_r_d_in_n_eu_f_

TX: Un lapin observe Rody. Où est-il?
PH: in_l_a_p_in__o_b_s_ai_r_v_r_o_d_i_._ou_et_t_i_l

TX: Montre le plus gros des icebergs.
PH: m_on_t_r_l_e_p_l_u_g_r_o_d_et_z_a_i_s_b_ai_r_g_
```

## et /e/ vs ai /ɛ/

```
TX: Où est la cheminée de la maison?
PH: ou_ai_l_a_ch_e_m_i_n_et_d_e_l_a_m_et_z_on_

TX: Où est le roi des ténèbres?
PH: ou_et_l_e_r_oi_d_et_t_et_n_ai_b_r_

TX: Aide Rody à trouver la clé!
PH: ai_d_r_o_d_i__a_t_r_ou_v_et_l_a_c_l_et_

TX: Où est allé se réfugier l'aigle?
PH: ou_et_a_l_et_s_e_r_et_f_u_j_i_et_l_ai_g_l_
```

## o /o/ vs oh /ɔ/

```
TX: Où est la porte magique?
PH: ou_et_l_a_p_oh_r_t_m_a_j_i_c_

TX: Où est tombé le sac à dos de Rody?
PH: ou_et_t_on_b_et_l_e_s_a_c_a_d_o_d_e_r_o_d_i_

TX: Montre le rostre de l'espadon.
PH: m_on_t_r_l_e_r_oh_s_t_r_d_e_l_et_s_p_a_d_on_

TX: Quelle est la plus petite des cloches?
PH: c_ai_l_et_l_a_p_l_u_p_e_t_i_t_d_et_c_l_oh_ch_
```

## e (schwa, closed eu) vs eu (open)

```
TX: As-tu remarqué le ciel bleu?
PH: a_t_u_r_e_m_a_r_c_et_l_e_s_i_ai_l_b_l_e

TX: Où Rody peut-il se cacher?
PH: ou_r_o_d_i_p_e_t_i_l__s_e_c_a_ch_et_

TX: Si je craque, Rody meurt. Que suis-je ?
PH: s_i_j_e_c_r_a_c_,_r_o_d_i_m_eu_r_._c_e_s_u_i_j_e_

TX: Quel oeil rody va-t-il atteindre?
PH: c_ai_l_eu_y_r_o_d_i_v_a_t_i_l_a_t_in_d_r_

TX: Où est la voiture facteur?
PH: ou_et_l_a_v_oi_t_u_r_f_a_c_t_eu_r_
```

## Nasals, oin, semivowels

```
TX: Où est le trésor? J'en ai besoin.
PH: ouu_et_l_e_t_r_ai_z_oh_r_._j_an_n_ai_b_e_z_ou_in

TX: Aperçois-tu la main de Rody?
PH: a_p_ai_r_s_oi_t_u_l_a_m_in_d_e_r_o_d_i_

TX: Le soleil apparait. Le distingues-tu?
PH: l_e_s_o_l_ai_y_a_p_a_r_ai_._l_e_d_i_s_t_in_g_t_u_

TX: Vois-tu la poudre de sommeil?
PH: v_oi_ti_u_l_a_p_ou_d_r_d_e_s_o_m_ai_y_

TX: Où Rody va-t-il s'asseoir?
PH: ou_r_o_d_i_v_a_t_i_l_s_a_s_ou_oi_r_
```

## Pauses, rhythm, punctuation tokens

```
TX: Où Rody a-t-il rangé ses chaussures?
PH: ou_r_o_d_i_,_a_t_i_l_r_an_j_et_s_et_ch_o_s_u_r_

TX: Cette porte n'est pas fermée. Entrons!
PH: s_ai_t_p_oh_r_t__n_ai_p_a_f_ai_r_m_et_._an_t_r_on_

TX: On ne fait pas ça en public, touche le!
PH: on_n_e_f_ai_p_a_s_a_an_p_u_b_l_i_c_,_t_ou_ch_l_e_

TX: Coa, coa, coa. Gaëtan!   (frog voice, long pauses)
PH: c_o_a____c_o_a____c_o_a_._g_a_et_ti_an_

TX: Vois-tu la main droite de Gaëtan?   (aspirated-style pause, no liaison)
PH: v_oi_t_u_l_a_m_in_d_r_oi_t___d_e_g_a_et_t_an
```

## Long narration (multi-sentence, from the game intro)

```
TX: "Rody, maman a ouvert doucement la porte de ta chambre et s'apprête à
    déposer un baiser sur ton front. Mais... elle ne te trouvera pas! Viens
    vite, le professeur Gobino nous attend."
PH: r_o_d_i m_a_m_an_a_ou_v_ai_r_d_ou_s_e_m_an_l_a_p_oh_r_t_d_e_t_a_ch_an_b_r et_s_a_p_r_ai_t_a_d_et_p_o_z_et_un_b_ai_z_et_s_u_r_t_on_f_r_on . m_ai ai_l_n_e_t_e_t_r_ou_v_e_r_a_p_a . v_i_in_v_i_t_._l_e_p_r_o_f_ai_s_eu_r_g_o_b_i_n_o_n_ou_z_a_t_an_
```

## Mastico's feedback lines (hardcoded in SoundManager.cs)

```
TX: Oui, c'est bien!
PH: ouu_i _ s_et_b_i_un

TX: Non, recommence.
PH: n_on _ r_e_c_o_m_an_s

TX: Félicitations, moussaillon!
PH: f_et_l_i_s_i_t_a_s_i_on _ m_ou_s_a_y_on

TX: C'est presque ça, cherche encore un peu.
PH: s_et_p_r_ai_s_c_e_s_a _ ch_ai_r_ch_in_p_e_m_i_e
```
