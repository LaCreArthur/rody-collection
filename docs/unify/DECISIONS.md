# Story storage decisions

This preserves the product intent recorded on 2026-06-29. A historical attribution
is not fresh approval to implement every clause. Current behavior is owned by
[ARCHITECTURE.md](ARCHITECTURE.md), gaps by [AUDIT.md](AUDIT.md), and priority by
[ROADMAP.md](../ROADMAP.md).

The September 9 [game design](../GAME_DESIGN.md) now proposes a whole-story draft
and story-wide restore instead of the earlier scene-only Reset. That proposal is
not accepted yet; retain the distinction when implementing the next leg.

## Recorded product direction

- **Save keeps a local story; Export shares a file.** Save should survive a browser
  reload. Browser storage is a convenience copy; the export is the portable backup.
  No cloud/server sync or speculative remote-storage architecture was requested.
  This supersedes the January 2026 export-only save-awareness proposal.
- **Explicit Save, not autosave on every edit.** Reset should discard temporary edits
  of the selected scene and return to its last saved state. The proposed page-hide
  flush is a safety net for already-written files, not permission to save drafts.
- **Built-ins remain intact.** Entering the editor from a built-in story should
  transparently create a user copy and let the player continue that edited story.
  No mandatory naming prompt was requested. The record specifically names gameplay
  paintbrush and story-menu entry; it does not justify removing collection actions.
- **Show backup state.** The collection should distinguish stories changed or not yet
  exported. A locally saved story and an exported copy are different facts.
- **Built-in order:** original, II, III, Noël (IV), V, VI, Ibiza last; user stories
  follow, newest saved last. Ibiza remains built-in. The catalog generation code
  owns this ordering; do not maintain another runtime whitelist.

## Historical defaults, not additional approval

The old plan proposed fresh internal ids and a visible ` (copie)` suffix, with no
name prompt. Its earlier prose also said "no suffix"; that conflict was never a
sound reason to silently change naming. Current behavior keeps the suffix but
uses title-derived ids. Collision handling remains an implementation issue.

The first-run editor hint was to remain a preference and show once. Voice-workbench
and Zambla UI work were outside the storage migration; voice work subsequently
proceeded under the separate [speech direction](../SPEECH_ENGINE.md#accepted-direction).
The editor's multiple-object-zone limitation was explicitly deferred, not accepted
as data loss.

The broader plan proposed removing desktop file-picker code and using WebGL as the
release target. That removal is reflected in the implementation, but the historical
record is not authorization for further platform or feature cuts.
