# Story and editor audit

Source and serialized-reference review: **2026-09-09**, commit `2d7555e`.
No Unity compile, player build or browser playtest was run for this documentation
pass. Findings below distinguish direct code behavior from risks requiring runtime
reproduction. Current architecture is in [ARCHITECTURE.md](ARCHITECTURE.md);
release sequencing is in [ROADMAP.md](../ROADMAP.md).

## Save and content integrity

| Finding | Evidence and consequence | Narrow verification for a fix |
|---|---|---|
| **Reset is not a complete last-save rollback.** | `RM_WarningLayout` calls `RM_GameManager.Reset`, which reloads the in-memory session. `RM_ImagesLayout.ProcessImportedTexture` writes imported images to that session immediately. Reset therefore does not restore the previous persisted image. Draft text lives in panel/manager fields, so its lifetime differs. | Import an image and edit text, Reset, then compare both with the last successfully saved story. |
| **Dirty state is not the complete editor draft.** | Dialogues/text/zone changes can remain in editor fields until `RM_SaveLoad.SaveGame`; `StorySession.IsDirty` is set by session mutation. The Escape warning checks only that session flag and user provenance. Its text still says "not exported" and "lost when closing", conflating save and export. | Edit only dialogue/text in a saved user story and leave without Save; check the warning and retained content. |
| **Gameplay paintbrush bypasses the fork used by other entries.** | The `DrawClick` event in `3_StoryScene.unity` targets `ClickHandler.DrawClick`, which loads Maker directly. Collection and story-menu entry call `ForkForEditing`; `RM_GameManager.Start` does not. A built-in loaded through that paintbrush can reach Save still marked built-in. | Enter from all three places, edit/save, check original content and a distinct user copy after reload. |
| **Story identity can collide.** | `StorySession.CreateNew`, `SetTitle` and `ForkForEditing` derive ids from titles. Import retains ids; `StoryStore.SaveUser` overwrites the same id's path; `StoryCatalog.Resolve` prefers an existing user file over a built-in. Fresh-id guarantees in the old plan were not implemented. | Same-title creations, repeated duplicates, repeated imports, and an import matching a built-in id must not silently replace unrelated work. |
| **Import failure can retain the previous story.** | `StorySession.LoadFromJson` returns without clearing an existing session when validation/parsing fails. `RA_NewGame.OnImportComplete` checks only `IsLoaded`, so it can persist/report success for that previous story. | Import invalid JSON with a valid story already loaded and verify honest failure without altering that story. |
| **Create/import/delete ignore storage errors.** | Their `SaveUser`/`DeleteUser` callbacks in `RA_NewGame` discard the error argument; editor Save handles it. A success message is not proof of a durable write. | Force a storage failure for each operation; success must not be shown and the original story must remain accessible. |
| **Only one target pair per objective is saved.** | `SceneData.ObjectZones` contains one `ObjectZone` for each of `obj`, `ngp`, `fsw`. `RM_SaveLoad.GameObjectsToObjectZone` reads index 0 even though the drawing UI retains multi-zone machinery. | Draw multiple targets and save/reopen. Decide the product contract before exposing multiple targets; do not silently discard them. |
| **Image save behavior diverges from the format's intended dimensions.** | `RM_SaveLoad.SaveSceneToSession` resizes the title to 320×240, while title creation/import/render expect 320×200. It also writes the main image into frames 1–4 before overlaying animation frames. Existing trailing keys are not pruned by that routine. | Compare title aspect and exact frame sequence before and after save/reopen. |

## Browser integration still needs evidence

- **Startup hydration wiring:** `Bootstrap.Start` requests `StoryRoot.InitStore`,
  but the Bootstrap script GUID (`4052599b664f54fbd8faaeb1c5153e2e`) was not found in
  the inspected `Assets` scenes, prefabs or serialized `.asset` files. The C# search
  found no runtime Bootstrap construction. Lazy `StoryRoot` creation constructs
  services but does not call store initialization. `RA_ScrollView.Start` builds
  the catalog immediately, without subscribing to `Bootstrap.OnInitialized`.
  This leaves the application's explicit hydrate-before-catalog contract unproven.
  Unity's own player startup filesystem synchronization is a competing explanation
  for saved files appearing anyway; a fresh browser reload must settle it.
- **Overlapping saves:** `WebFs` has one pending callback per operation, overwritten
  by the next request. `StoryStore` and `RodyWeb.jslib` do not queue requests.
  Rapid saves or navigation during a save can therefore lose/misattribute completion
  handling; the real browser callback order needs reproduction.
- **No application page-hide backstop found** in `Assets/Scripts`,
  `Assets/Plugins/WebGL` or `Assets/WebGLTemplates` (searched `visibilitychange`,
  `pagehide`, `beforeunload`, and sync callers). The old decision was not implemented
  in these surfaces. Do not infer that all browser/platform durability is absent.
- **Export state is not represented:** session state records dirty/last-save values;
  export downloads without marking an export revision. The carousel paints title
  and cover without the former dirty badge. A "saved" label cannot demonstrate the
  requested "backed up by export" state.
- Browser file pickers deliberately report unavailable outside WebGL. Desktop
  import/export parity promised by old guides is not the current implementation.

## Editor usability and remaining duplication

- The saved scene contains two `RM_ButtonTooltip` components (Save and Intro).
  Save's tooltip is still `Exporte l'histoire`; wider tooltip coverage and first-save
  guidance remain unfinished. The first-run hint now sets its preference only if
  the key is absent, so the old repeated-first-run bug is no longer a pending item.
- New stories start with one scene. Thumbnail UI contains 30 slots, with asymmetric
  add/delete rules (`i < 29`, deletion by re-click only at scene 18 or later).
  The former "16 to 29 scenes" promise is not a reliable description of that UI.
- Animation UI exposes two groups of three buttons, but enables only existing
  frame indices; the importer can append the next index. Empty-frame creation
  needs a real UI check before promising all six frames are authorable.
- Object drawing maps screen coordinates with fixed 320×200 assumptions. Verify
  zones against the rendered scene after browser resize/fullscreen changes.
- Editor draft fields still mirror `SceneData` through manual copy-in/copy-out.
  Music selection retains inverse switch mappings. These are maintenance costs,
  not independent reasons for a new architecture project.
- User catalog cards still deserialize complete saved stories. Optimize only if a
  representative user library shows a meaningful startup cost.
- `isZambla` remains in voice data; its authoring affordance was deferred. Preserve
  Ibiza character playback when changing editor or scene-entry behavior.

## Historical evidence and retired plans

The old pre-migration audit, architecture proposal and step-by-step migration are
recoverable at `git show 2d7555e:docs/unify/<filename>.md`. Storage implementation
landed on 2026-06-30: `88e248e` (store), `0b8cef7` (Save becomes local persistence),
`38e4410` (catalog carousel), `0830497` (provider/shim removal), `6e9dacf` (desktop
file-picker removal), `8611a4c` (cleanup). These commits establish code changes,
not browser acceptance.

Retired instructions: repeated provider/WorkingStory migration steps, per-file
compile/build recipes, the unimplemented `WebShare` replacement type, and old
line-count/effort inventories. The save-awareness plan's download-only choice was
superseded; its remaining user outcomes (clear Save/Export, honest warnings,
backup indication, button help) survive above and in the recorded decisions.
