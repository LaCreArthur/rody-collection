# Rody Collection - Unification Decisions (resolved)

Authoritative record of decisions for the architecture in `ARCHITECTURE.md` / `MIGRATION.md`.
Confirmed with Arthur on 2026-06-29. No em dashes by intent.

---

## Confirmed by the user

1. **Save semantics: Save = persist locally.**
   The editor's Save writes the story to browser-backed local storage (persistentDataPath +
   IndexedDB flush) and it survives a page reload. Export becomes a SEPARATE explicit "share as
   file" action. The old Save=download behavior is dropped.
   Consequence: the entire unsaved-work nag chain (ExportReminder, beforeunload, the
   `_rodyHasUnsavedWork` JS flag) is deleted. Current export button is only in the main menu 0_MenuCollection, in the editor scene 6_RM_Main there is only a save button, but also a ResetButton, which is must revert to last save state and discard temporary edits of the current selected game scene (user can only edit one scene at the time) not yet saved.   

2. **Save trigger: explicit Save button + page-hide backstop flush.**
   The user clicks Save to persist (in the RM editor). A flush also fires on tab hide/visibilitychange as a safety
   net. NOT auto-save-on-every-edit (avoids many large IndexedDB writes; each story is 1.4-2.6 MB, allows for the reset feature).
   Dirty flag clears only in the syncfs success callback.

3. **Editing a built-in: silent copy.**
   Editing an official story transparently produces an editable user copy that appears as a new
   entry in the list. No mandatory "Duplicate" click first, no "(copie)" rename friction at edit
   time. Copy-on-load makes this free; "fork-on-edit" stops being a special data path. Editing an official story can only happens when playing the original story and clicking on the "edit this scene" in the 3_StoryScene scene, or on the story menu in 2_Menu with the dedicated edit button. this must automatically creates a user copy and set it as the current story, so they can go back to playing their edited version after editing.

4. **Durability: local store + export-as-file backup.**
   Browser-local storage is the convenience layer. Export-to-file is the only hard backup. No
   cloud/server sync now. The storage layer must fail gracefully on storage-full / eviction.
   (Storage layer should not be over-built for a remote seam that is not planned.) In 0_MenuCollection, the unexported and modified stories must have a visual cue differenciating them, telling the user they are not yet exported. 

---

## Proposed defaults for the remaining decisions (pending user objection)

5. **Story display order. RESOLVED (Arthur, 2026-06-29).** The 6 official Atari ST stories come
   first in this fixed order, then Arthur's handmade "Rody Et Mastico A Ibiza" LAST, those 7 are the baked in stories that cannot be directly modified:
   1. Rody Et Mastico
   2. Rody Et Mastico II
   3. Rody Et Mastico III
   4. Rody Noel  (the Christmas "IV")
   5. Rody Et Mastico V
   6. Rody Et Mastico VI
   7. Rody Et Mastico A Ibiza  (handmade, last)
   This equals the code's current `OrderStories` whitelist, NOT the on-disk `index.json` (which
   wrongly lists Ibiza 6th and Noel last). The generated `catalog.json` MUST use the order above;
   `index.json`'s order is discarded. User-created stories sort after all built-ins, newest-last
   by save time. Note: Ibiza currently ships inside Resources/Stories, so it stays a built-in
   (last) in the catalog; it is not reclassified as a user story by this migration.

6. **Duplicate naming.** DEFAULT: assign a fresh internal id (so a copy never collides with a
   built-in id; built-in stays visible alongside the copy), keep the visible " (copie)" title
   suffix for user recognition. No name prompt. Revisit if a name prompt is wanted later.

7. **rodyMakerFirstTime.** DEFAULT: keep it as the one surviving PlayerPref (editor-hint state,
   not story state), AND fix the bug where it is re-set to 1 on every menu visit so the hint
   fires on every editor entry. After the fix the hint shows once.

8. **Scene 7 (additive phoneme editor) and the isZambla editor UI.** DEFAULT: OUT OF SCOPE for
   the storage unification. The Ibiza/Zambla runtime hack is still replaced by reading the
   existing data-model `isZambla` flag (that is part of deleting the gamePath key), but wiring a
   new editor UI for it, and for scene 7, is deferred. (RM_ObjLayout persisting only zone[0] is
   likewise a pre-existing gap this migration exposes but does not fix.)

---

## Alignment note

Decisions 1-4 match what `MIGRATION.md` already assumed (explicit Save + backstop flush, silent
copy-on-load, local persistence with export backup). No changes to ARCHITECTURE.md or MIGRATION.md
are required from these answers. Defaults 5-8 are reflected in the migration steps (5 in Step 5,
7/8 noted in Step 7 and the open-decisions list).
