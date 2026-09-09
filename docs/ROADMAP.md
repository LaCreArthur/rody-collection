# Roadmap

## TL;DR (Arthur)

The finish line is a polished browser release for retro fans and creators.
The original speech engine and French dialogue workbench are implemented locally.
Browser story saving and the shared library actions are also implemented.
The next priority is trustworthy saving, restoring, testing and leaving the editor.
Then verify the complete browser experience and publish.
The proposed editing flow is ready for a decision; it has not been implemented here.

---

## Technical body (executing agent)

**Updated: 2026-09-09.** This is the owner of project status and priorities.
Source baseline: `2d7555e`, five local commits ahead of fetched `origin/master` at the
start of this audit. Those commits are not evidence of a deployed release.
This pass inspected code, serialized references, embedded JSON, docs and history;
it did not compile Unity, build a player or perform browser acceptance testing.

### Intent and release sequence

[GAME_DESIGN.md](GAME_DESIGN.md) owns product identity, audience, capabilities and
proposed UX. It is the code-free product reference requested by Arthur on September 9.
This roadmap owns work priority and release status, not another copy of that design.

The requested sequence was authentic engine → Maker UX → integrated/standalone
voice workbench → WebGL verification → publish. Engine and workbench implementation
have progressed; returning to unfinished Maker UX is the next useful leg.

### Implementation status

| Outcome | Current evidence | Remaining boundary |
|---|---|---|
| One story model, catalog and local save path | June 30 migration commits; current [architecture](unify/ARCHITECTURE.md) | Startup, failures, collisions and editor save/reset consistency need work; [audit](unify/AUDIT.md) owns specifics. |
| Saved user stories listed beside seven built-ins | Catalog reads local story files; shared actions offer edit/duplicate, import, new, export | Browser reload and error-path checks; accurate backup indication. |
| Original 1988 engine replaces recorded-clip concatenation | C# port and native-oracle evidence in [SPEECH_ENGINE.md](SPEECH_ENGINE.md) | Full in-game/browser listening and performance; hardware timing/output remain approximate. |
| One voice workbench in stories and standalone Voix | French input, word correction, full notation, selected playback, explicit apply/cancel | French conversion, clipboard and story round-trip in a player/browser. Evidence and limits belong to the speech reference. |
| Editor help | Save and Intro tooltip components wired; first-run hint preference fixed | Correct Save tooltip, remaining toolbar help and first-save explanation. |
| Unity upgrade | Project version setting and September 6 upgrade commit | Current dependency/version values belong to project config, not copied tables. |

### Next work, in order

1. **Agree the editing contract.** The [game design](GAME_DESIGN.md) proposes one
   whole-story draft, explicit Save, story-wide restore, draft preview and safe exit.
   This replaces the old scene-only Reset proposal and avoids a prompt on every
   scene change. Arthur asked for this design help on September 9; that request is
   not approval of every proposed behavior.
2. **Close story-integrity issues.** Use the [audit](unify/AUDIT.md#save-and-content-integrity)
   as evidence, especially reset scope, built-in protection, id collisions, invalid
   import handling and honest storage failures. Do not mark the old migration
   "release ready" merely because its types exist.
3. **Complete Maker UX.** Implement the agreed contract, then concise labels/help.
   Define supported scene/frame/object limits and make every visible supported
   control reliable. Correct title/image/frame saving and stop silent target loss;
   expansion of capabilities is a separate decision.
4. **Validate the release candidate in WebGL.** A fresh browser session must load
   saved stories, preserve edits across reload, import/export a usable `.rody.json`,
   and handle storage/clipboard failures honestly. Exercise French correction and
   native-expression round-trips, original and Ibiza voices, pitch, title/ending
   flow, resized object-zone interaction, and DOOMastico entry/return. Run an
   independent release review at this stage; compilation alone is not acceptance.
5. **Publish after acceptance and an explicit publishing instruction.** Refresh
   current tutorial screenshots, approve the local [itch copy](itch-pages/ITCH_RODY_COLLECTION.md),
   then deploy the validated build. GitHub Pages CI runs on pushes to `master`.

### Parked, not silently cancelled

- **Story title/cover editing and discoverable deletion:** the old floating-slot
  plan proposed these outcomes. The shared action bar already replaced its layout;
  a metadata editor remains a separate UX decision, not a reason to rebuild slots.
- **Expanded scene/animation capacity and multiple targets:** only expansion is
  parked. Agreeing the existing supported limits and fixing broken visible controls
  belongs in the Maker UX leg above. Do not revive old "16–29 / six objects"
  advertising as an implementation requirement.
- **Conversion skill distribution:** keep one repository-owned
  [French-to-Rody skill](../.claude/skills/french-to-rody-phonemes/SKILL.md), make it
  discoverable, and separate optional machine-local tooling if portability work is
  needed. In-game French entry supersedes the July agent-only restriction.
- **DOOMastico tuning:** [DOOM_FPS.md](DOOM_FPS.md) owns the optional gameplay ideas.
- **Performance/cleanup:** profile a representative saved library before optimizing
  user-card loading; remove redundant editor state only in service of the agreed UX.
  Zambla authoring controls remain a separate decision.

Use the [agent entry point](../CLAUDE.md) to find the owner of each reference.
Pre-migration work lists and completed handoffs are retired; do not execute them.
