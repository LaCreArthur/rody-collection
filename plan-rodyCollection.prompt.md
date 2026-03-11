## Plan: Remaining Work Roadmap

Recommended approach: split the backlog into 3 buckets so execution stays sane. First, finish release-critical verification and the already-designed menu slot UX. Second, close the deferred editor UX items that reduce user confusion. Third, leave larger architecture changes and cleanup in a clearly separated later bucket so they do not bloat the next release.

**Steps**
1. Phase 1: Release confidence. Verify WebGL import and export end to end using the current WorkingStory flow, because docs mark this as implemented but unvalidated. This gates any release decision.
2. Phase 1: Implement menu story slot actions from the existing design in /Users/bretzelstudio/unity-projects/RodyMaker/plan.md. Reuse the current static story model instead of designing for multi-story support now. This depends on step 1 only for release ordering, not for code dependency.
3. Phase 1: Re-test the story management UX after slot actions are added: official story fork flow, user story edit flow, user story export flow, and unsaved-state indicators. This depends on step 2.
4. Phase 2: Add deferred Rody Maker tooltips so the editor explains the destructive or non-obvious controls. This can run in parallel with late testing if implementation is straightforward.
5. Phase 2: Resolve the minor object-zone editor TODOs in RM_ObjLayout so zone editing behavior is less brittle and easier to reason about. This is independent from step 4.
6. Phase 2: Sweep remaining legacy storage references such as gamePath and gameToDelete, and decide whether they are still needed or should be removed. This should happen after release-critical work so cleanup does not distract from functional validation.
7. Phase 3: Revisit larger backlog items only after the above is done: first-time export guidance, ExportedStory model relocation, SoundManager breakup, and any future multi-user-story architecture. These are explicitly out of the next-release critical path.

**Relevant files**
- /Users/bretzelstudio/unity-projects/RodyMaker/docs/ROADMAP.md — source of truth for current status; the unchecked WebGL validation items are the main release-confidence gap.
- /Users/bretzelstudio/unity-projects/RodyMaker/plan.md — existing detailed design for menu story slot actions; reuse the RA_SlotItem approach and selected-slot-only action visibility.
- /Users/bretzelstudio/unity-projects/RodyMaker/DEVLOG.md — confirms the slot-button work is planned, not implemented, and captures recent WebGL file-dialog fixes that affect verification.
- /Users/bretzelstudio/unity-projects/RodyMaker/Assets/Scripts/RodyAnthology/RA_ScrollView.cs — main menu story-slot behavior; expected integration point for slot action visibility and callbacks.
- /Users/bretzelstudio/unity-projects/RodyMaker/Assets/Scripts/RodyAnthology/RA_NewGame.cs — import and export entry points; likely place to support edit-mode reuse and export wiring.
- /Users/bretzelstudio/unity-projects/RodyMaker/Assets/Prefabs/Slot.prefab — slot UI prefab to extend with action buttons.
- /Users/bretzelstudio/unity-projects/RodyMaker/Assets/Scripts/Providers/WorkingStory.cs — core story lifecycle API to reuse, especially LoadOfficial, ForkForEditing, LoadFromJson, ExportToJson, SetTitle, and dirty-state handling.
- /Users/bretzelstudio/unity-projects/RodyMaker/Assets/Scripts/RodyMaker/RM_ObjLayout.cs — contains the remaining object-zone TODOs and should be reviewed before calling the editor UX complete.
- /Users/bretzelstudio/unity-projects/RodyMaker/docs/SAVE_AWARENESS_PLAN.md — reference for deferred tooltip and first-time export guidance items.

**Verification**
1. Build and run a WebGL build, then verify importing a .rody.json story loads into WorkingStory and can be played through at least one scene transition.
2. In WebGL, export a story and confirm the downloaded JSON contains expected story metadata and embedded sprite data.
3. In the menu, verify official slots expose only Fork on selection and user slots expose Edit and Export on selection.
4. Verify fork-on-edit does not mutate official stories in place and that the new user story shows unsaved-state UI until exported.
5. In the editor, verify the save/export prompts and exit warnings still behave correctly after story-slot actions are added.
6. If tooltip work is included, manually verify all target buttons display the intended French help text and do not block button clicks.
7. Grep for remaining legacy keys and unresolved TODO markers after cleanup so the roadmap can be updated with facts instead of assumptions.

**Decisions**
- Planning horizon: both. Produce a short next-release sequence and a longer backlog, but keep them separated.
- WebGL validation remains an explicit todo until someone tests it in an actual browser build.
- Next-release scope includes verification, menu slot actions, and small editor UX fixes. It excludes multi-story architecture work.
- Multi-user-story support is treated as a future design track, not a prerequisite for shipping the current slot-action UX.

**Further Considerations**
1. After WebGL verification, update docs immediately so ROADMAP stops mixing implemented code with unverified assumptions.
2. If menu-slot actions reveal friction in the single-static WorkingStory model, document the exact pain before starting a collection refactor.
3. If tooltip work threatens schedule, ship without it; the slot-action UX and browser-flow validation matter more.
