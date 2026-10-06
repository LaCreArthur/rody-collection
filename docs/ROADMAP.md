# Roadmap

## TL;DR (Arthur)

The finish line is a polished browser release for retro fans and creators.
The single personal story and unified Save/Discard flow are implemented.
Editor checks retained edits across scenes, Test, original play and return.
The native Maker workspace is implemented: direct text and target editing, temporary scene navigation.
The voice engine is implemented. In the Maker the voice follows the text,
with story-wide word fixes; the Voix workbench stays standalone.
Earlier browser checks passed; the Maker voice fix passed a local browser build; the rest of the new Maker UI still needs browser acceptance.
Every push to master deploys the site: the new Maker has been live since the September pushes,
before full browser acceptance. Saving old browser-library stories before that switch is overtaken.
The web page shows the game on an 80s TV in a painted room: one screen, smooth resizing.

---

## Technical body (executing agent)

**Updated: 2026-09-15.** This is the owner of project status and priorities.
The single-workspace implementation is based on plan commit `d1d2fe8`. It is all on
`origin/master`, and every push to `master` deploys GitHub Pages (deploy of `49d961b`
succeeded 2026-09-24), so the live site runs it without full browser acceptance. Source, serialized assets and focused Editor runtime journeys
were inspected. Arthur subsequently authorized a local WebGL build: it succeeded
with zero errors, and the three core browser checks passed. [AUDIT.md](unify/AUDIT.md)
owns exact evidence and remaining limits; this is not full release acceptance.

The native Maker UI is now implemented locally per [MAKER_UI_UX_PLAN.md](MAKER_UI_UX_PLAN.md).
That plan owns the focused Editor evidence and limitations. September15 corrected
sub-native font sizes and a compressed icon after Arthur's pixel-budget feedback;
actual320×200 camera captures were inspected. A subsequent accepted pass added native
scene/navigation/proximity pictograms, text hover and selection affordances, contextual
cursors and gesture help; focused Editor checks and independent wiring review completed.
No new build/browser run covers this UI.

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
| Originals plus one personal slot | Eight rendered cards; personal draft remains while an original is played; legacy library APIs/UI removed | Old browser-only stories: the cutover is already live; recover only if Arthur needs some. |
| One file Save and automatic workspace recovery | Actual browser download/reimport and dirty refresh followed by whole-story Discard passed | Picker cancellation, storage failures and delayed-handoff journeys. |
| Original 1988 engine replaces recorded-clip concatenation | C# port and native-oracle evidence in [SPEECH_ENGINE.md](SPEECH_ENGINE.md) | Full in-game/browser listening and performance; hardware timing/output remain approximate. |
| Maker voice follows the text; standalone Voix workbench | Conversion on each edit, in-place word-fix mode with story-wide respellings, speaker/pitch cycle; Editor play-mode checks on 2026-09-25 (fix, story-wide apply, reset, cancel, save round-trip). Local WebGL build 2026-09-25, real mouse and keyboard: async conversion on edit, word pick, one-word refusal greys Validate, fix applied to the edited and another line, removal restores the fresh score (checked in the downloaded file), no console errors. Workbench: French input, full notation, clipboard | Listening by ear in a browser; workbench clipboard in a browser. Evidence and limits belong to the speech reference. |
| Native Maker workspace | Composed intro/direct text edits, insertion crop, shared objective text/target selection, transactional target drawing, temporary image-only scene browser, real Test/return; focused Editor probes and320×200 renders | New UI browser display, input and interrupted-drawing acceptance. |
| Unity upgrade | Project version setting and September 6 upgrade commit | Current dependency/version values belong to project config, not copied tables. |
| Web page: the game on an 80s TV in a painted room | WebGL template plus the `WebPage` sound receiver; layout rules live in the template comments, decisions in the [development log](../DEVLOG.md) (2026-10-05). Local preview against the live build, Chromium, 10 window sizes | Sound button on the deployed build; Safari and Firefox. Phones are not a target. |

### Next work, in order

1. **Previous local browser candidate obtained.** It predates the native Maker UI. Build, download/reimport, dirty refresh
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
4. **Publish after acceptance and an explicit publishing instruction.** GitHub
   Pages already serves every push to `master`, including the old browser library cutover.
   Refresh current tutorial screenshots and approve the local
   [itch copy](itch-pages/ITCH_RODY_COLLECTION.md).

### Asked, not started

- **Voix workbench redesign:** Arthur asked (2026-10-05) to redesign the Atelier des
  voix "like the Maker". Start from the Maker's native UI; he judges every image.

### Parked, not silently cancelled

- **Wider room painting for the web page:** wide windows cannot show a smaller TV
  because the painting must cover the window. A wider, sharper painting would allow it;
  offered 2026-10-05, Arthur has not decided.

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
