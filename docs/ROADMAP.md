# Roadmap

## TL;DR (Arthur)

The finish line is a polished browser release for retro fans and creators.
The original speech engine and French dialogue workbench are implemented locally.
The simpler editor direction is accepted: originals plus one personal story.
Save downloads a file; Discard restores; the browser remembers work automatically.
The next priority is implementing that consistent editing experience.
Then verify the complete browser experience and publish.
The implementation plan is ready; the new behavior has not been implemented here.

---

## Technical body (executing agent)

**Updated: 2026-09-09.** This is the owner of project status and priorities.
Planning baseline: `89fdf46`, six local commits ahead of fetched `origin/master`
at `4dc870d`. Those commits are not evidence of a deployed release.
This pass inspected code, serialized references, embedded JSON, docs and history;
it did not compile Unity, build a player or perform browser acceptance testing.

### Intent and release sequence

[GAME_DESIGN.md](GAME_DESIGN.md) owns product identity, audience, capabilities and
accepted next UX. It is the code-free product reference requested by Arthur on September 9.
This roadmap owns work priority and release status, not another copy of that design.

The requested sequence was authentic engine → Maker UX → integrated/standalone
voice workbench → WebGL verification → publish. Engine and workbench implementation
have progressed; returning to unfinished Maker UX is the next useful leg.

### Implementation status

| Outcome | Current evidence | Remaining boundary |
|---|---|---|
| One story model, catalog and local save path | June 30 migration commits; current [architecture](unify/ARCHITECTURE.md) | Rework ownership and persistence for the accepted single workspace; [audit](unify/AUDIT.md) owns current failure evidence. |
| Saved user stories listed beside seven built-ins | Catalog reads local story files; shared actions offer edit/duplicate, import, new, export | Replace the personal library with one slot and the separate Export with Save; preserve browser-only stories before public cutover. |
| Simpler document editor accepted and planned | September 9 user proposal and explicit acceptance; [implementation plan](EDITOR_WORKSPACE_PLAN.md) contains mandate and cold review | Implement, then verify in the browser. No code/assets changed in the planning pass. |
| Original 1988 engine replaces recorded-clip concatenation | C# port and native-oracle evidence in [SPEECH_ENGINE.md](SPEECH_ENGINE.md) | Full in-game/browser listening and performance; hardware timing/output remain approximate. |
| One voice workbench in stories and standalone Voix | French input, word correction, full notation, selected playback, explicit apply/cancel | French conversion, clipboard and story round-trip in a player/browser. Evidence and limits belong to the speech reference. |
| Editor help | Save and Intro tooltip components wired; first-run hint preference fixed | Align labels/tooltips with accepted Save/Discard and recovery behavior. |
| Unity upgrade | Project version setting and September 6 upgrade commit | Current dependency/version values belong to project config, not copied tables. |

### Next work, in order

1. **Implement the accepted editor contract.** Follow the
   [workspace plan](EDITOR_WORKSPACE_PLAN.md): direct whole-story draft and restore,
   one file Save, persistent browser recovery, one personal slot, and removal of
   obsolete library/save paths. Its integrity fixes cover content already being
   edited; they do not authorize expansion of scene/frame/target capacity.
2. **Resolve remaining visible authoring limits.** Choose consistent scene
   management and repair unsupported visible frame/target controls without silently
   dropping data. These broader capacity/UX choices remain separate from the
   accepted workspace contract. Use the [audit](unify/AUDIT.md) for source evidence.
3. **Validate the release candidate in WebGL.** A fresh browser session must recover
   the personal draft and its restore point, download/reimport a usable `.rody.json`,
   and handle replacement, storage and clipboard failures honestly. Exercise French correction and
   native-expression round-trips, original and Ibiza voices, pitch, title/ending
   flow, resized object-zone interaction, and DOOMastico entry/return. Run an
   independent release review at this stage; compilation alone is not acceptance.
4. **Publish after acceptance and an explicit publishing instruction.** Preserve
   any personal work that exists only in the old browser library before cutover. Refresh
   current tutorial screenshots, approve the local [itch copy](itch-pages/ITCH_RODY_COLLECTION.md),
   then deploy the validated build. GitHub Pages CI runs on pushes to `master`.

### Parked, not silently cancelled

- **Story title/cover editing:** the old floating-slot plan proposed these outcomes.
  The shared action bar already replaced its layout; metadata editing remains a
  separate UX decision. Discoverable personal-story deletion retires with the
  accepted removal of the personal library; scene deletion is retained.
- **Expanded scene/animation capacity and multiple targets:** only expansion is
  parked. Agreeing the existing supported limits and fixing broken visible controls
  belongs in the Maker UX leg above. Do not revive old "16–29 / six objects"
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
