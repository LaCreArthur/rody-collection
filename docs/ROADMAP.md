# Roadmap

## TL;DR (Arthur)

The finish line is a polished browser release for retro fans and creators.
The single personal story and unified Save/Discard flow are implemented locally.
Editor checks retained edits across scenes, Test, original play and return.
The original artwork is retained; the preview now fits complete pictures.
The voice engine and French workbench are also implemented locally.
Core browser checks passed: download/reimport, refresh/Discard and target drawing.
Preserve stories from the old browser library before publishing.
The local WebGL build passed; broader release checks and publication remain.

---

## Technical body (executing agent)

**Updated: 2026-09-10.** This is the owner of project status and priorities.
The single-workspace implementation is local, based on plan commit `d1d2fe8`.
Fetched `origin/master` remains `4dc870d`; local changes are not evidence of a
published release. Source, serialized assets and focused Editor runtime journeys
were inspected. Arthur subsequently authorized a local WebGL build: it succeeded
with zero errors, and the three core browser checks passed. [AUDIT.md](unify/AUDIT.md)
owns exact evidence and remaining limits; this is not full release acceptance.

### Intent and release sequence

[GAME_DESIGN.md](GAME_DESIGN.md) owns product identity, audience, capabilities and
accepted next UX. It is the code-free product reference requested by Arthur on September 9.
This roadmap owns work priority and release status, not another copy of that design.

The requested sequence was authentic engine → Maker UX → integrated/standalone
voice workbench → WebGL verification → publish. Engine and workbench implementation
have progressed; browser release acceptance is now the next boundary.

### Implementation status

| Outcome | Current evidence | Remaining boundary |
|---|---|---|
| One draft and independent whole-story restore point | Direct Maker bindings, unified entry paths and Editor journeys; [architecture](unify/ARCHITECTURE.md) owns mechanisms | Sprite/structure rollback after authoring; [audit](unify/AUDIT.md) owns remaining evidence. |
| Originals plus one personal slot | Eight rendered cards; personal draft remains while an original is played; legacy library APIs/UI removed | Preserve any old browser-only stories before public cutover. |
| One file Save and automatic workspace recovery | Actual browser download/reimport and dirty refresh followed by whole-story Discard passed | Picker cancellation, storage failures and delayed-handoff journeys. |
| Original 1988 engine replaces recorded-clip concatenation | C# port and native-oracle evidence in [SPEECH_ENGINE.md](SPEECH_ENGINE.md) | Full in-game/browser listening and performance; hardware timing/output remain approximate. |
| One voice workbench in stories and standalone Voix | French input, word correction, full notation, selected playback, explicit apply/cancel | French conversion, clipboard and story round-trip in a player/browser. Evidence and limits belong to the speech reference. |
| Editor help and content views | Save/Discard labels, nonblocking status/retry, frame removal and complete aspect-fit preview; independent reference review | Browser display-size, interrupted redraw and gameplay hit checks; completed pointer drawing passed. |
| Unity upgrade | Project version setting and September 6 upgrade commit | Current dependency/version values belong to project config, not copied tables. |

### Next work, in order

1. **Local browser candidate obtained.** Build, download/reimport, dirty refresh
   followed by Discard, and pointer target authoring passed. Reuse that evidence;
   rebuild only after changes that require it. No CI push is needed for local QA.
2. **Remaining release checks.** Exercise replacement Save/Discard/Cancel,
   slow/failing storage, failed hydration and delayed file handoff. Check real target
   drawing (including leaving midway), all frame controls and full French speech
   round-trips. Extend release QA to original/Ibiza voices, pitch, endings, clipboard,
   fullscreen/resize and DOOMastico entry/return. Compilation alone is not acceptance.
3. **Review release evidence and remaining authoring limits.** Independent source
   and serialized reviews have completed for the implementation. The full release
   review must include browser evidence. Scene-capacity/management changes remain
   separate product scope; the user has settled one target pair per objective.
4. **Publish after acceptance and an explicit publishing instruction.** Preserve
   any personal work that exists only in the old browser library before cutover. Refresh
   current tutorial screenshots, approve the local [itch copy](itch-pages/ITCH_RODY_COLLECTION.md),
   then deploy the validated build. GitHub Pages CI runs on pushes to `master`.

### Parked, not silently cancelled

- **Story title/cover editing:** the old floating-slot plan proposed these outcomes.
  The shared action bar already replaced its layout; metadata editing remains a
  separate UX decision. Discoverable personal-story deletion retires with the
  accepted removal of the personal library; scene deletion is retained.
- **Expanded scene/animation capacity:** parked; the current frame controls remain
  and need browser acceptance. **Multiple targets are excluded by the explicit
  one-target-per-objective decision.** Do not revive old “16–29 / six objects”
  advertising as an implementation requirement.
- **Conversion skill distribution:** keep one repository-owned
  [French-to-Rody skill](../.claude/skills/french-to-rody-phonemes/SKILL.md), make it
  discoverable, and separate optional machine-local tooling if portability work is
  needed. In-game French entry supersedes the July agent-only restriction.
- **DOOMastico tuning:** [DOOM_FPS.md](DOOM_FPS.md) owns the optional gameplay ideas.
- **Performance/cleanup:** profile a representative large personal story before
  optimizing recovery/serialization; redundant editor state is removed in service of the agreed UX.
  Zambla authoring controls remain a separate decision.

Use the [agent entry point](../CLAUDE.md) to find the owner of each reference.
Pre-migration work lists and completed handoffs are retired; do not execute them.
