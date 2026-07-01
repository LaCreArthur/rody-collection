# Rody Collection - Unification Migration Plan

Incremental plan for the architecture in `ARCHITECTURE.md`. One concern per step (repo rule: for N>=3 homogeneous items, default to an incremental loop, one item per cycle, verified and committed per item). Each step is independently verifiable and revertible. No em dashes by intent.

Guiding constraints:

- Keep every commit compiling. A thin `WorkingStory`-delegates-to-session shim survives until the very end so consumers migrate one file at a time.
- Verify with the narrowest credible check. Compile-only where the change is logic-internal. WebGL build + manual play/edit/import/persist-across-reload where a serialized asset, native plugin, or scene wiring is touched. Heavy Unity batchmode checks are opt-in only.
- Steps marked RISKY touch serialized assets, native plugins, or Inspector-wired scenes and require a scene-open or WebGL-build verification, not just compile.

---

## Step 0 - Relocate the jslib and add the flush (RISKY: serialized native plugin)

Move `Assets/StandaloneFileBrowser/Plugins/StandaloneFileBrowser.jslib` to `Assets/Plugins/WebGL/RodyWeb.jslib`. Add `RodySyncFs()` wrapping `FS.syncfs(false, cb)` and a hydrate path `FS.syncfs(true, cb)`. Do NOT delete the StandaloneFileBrowser tree yet. Do NOT change any C# yet.

Files: the jslib only.

Verify: WebGL build. Confirm existing import/export/beforeunload still bind (DllImports resolve to the relocated jslib). This is the one native-plugin landmine; Unity tracks plugins by .meta GUID and platform flags, and a move can silently drop the WebGL flag.

Rollback: move the jslib back; revert the .meta.

---

## Step 1 - Promote the model (compile-only)

Add `Story.cs` (renamed/promoted `ExportedStory` + `StorySource` enum), `StoryCard.cs`, `StoryJson.cs` (the one serializer). Keep JSON property names identical so existing `.rody.json` files still deserialize. No call-site logic changes; everything still compiles against the old static `WorkingStory`.

Files added: `Assets/Scripts/Stories/Story.cs`, `StorySource.cs`, `StoryCard.cs`, `StoryJson.cs`.

Verify: compile.

Rollback: delete the new files.

---

## Step 2 - Add the session behind the shim (compile-only, reversible)

Add `StorySession.cs`. Route `WorkingStory.Current` / `CurrentSceneIndex` / `IsDirty` THROUGH a session instance behind the existing static facade (facade delegates to session). Invert the S4 dependency here: the session raises `DirtyChanged`; `ExportReminder` subscribes instead of being called from the dirty setter.

Files added: `Assets/Scripts/Stories/StorySession.cs`. Changed: `WorkingStory.cs` (now a shim), `ExportReminder.cs` (subscribe, do not get called from the setter).

Verify: compile + open scene 0 and one gameplay scene to confirm selection still loads a story.

Rollback: revert the shim; the static facade is unchanged in surface.

---

## Step 3 - Extract the single SpriteCache (compile-only)

Add `SpriteCache.cs` + the sprite-naming helper. Point both `WorkingStory.LoadSprite/LoadSceneSprites` and `ResourcesStoryProvider.LoadSprite` at the one cache. Deletes the duplicate decoder and duplicate teardown without touching any consumer (the read contract is preserved).

Files added: `Assets/Scripts/Stories/SpriteCache.cs`. Changed: `WorkingStory.cs`, `ResourcesStoryProvider.cs`.

Verify: compile + open a gameplay scene; confirm sprites render and covers paint in scene 0.

Rollback: revert the two consumers to their inline decoders.

---

## Step 4 - Add the persistence store and prove the round-trip (RISKY: WebGL runtime capability)

Add `StoryStore.cs` (Resources read + `persistentDataPath` System.IO read/write/list/delete + `Init()` hydrate + `Flush()` -> `RodySyncFs`). The DllImport and its call site carry the one surviving `#if UNITY_WEBGL && !UNITY_EDITOR` guard; the flush is a no-op elsewhere. Do not wire callers yet.

Files added: `Assets/Scripts/Stories/StoryStore.cs`, `Assets/Scripts/WebGL/WebFs.cs` (the flush wrapper if kept separate from the bridge).

Verify: WebGL build. Write a story to `persistentDataPath`, flush, reload the page, read it back. This de-risks the single genuinely new capability before any refactor depends on it. Clear the dirty flag only in the syncfs callback.

Rollback: delete the new files; no caller depends on them yet.

---

## Step 5 - Add the catalog and the export-time cover manifest (RISKY: build-time asset)

Extend the export tool to emit `Assets/Resources/Stories/catalog.json` (id, title, sceneCount, cover bytes per builtin), replacing `index.json` as the single source of membership AND order. Add `StoryCatalog.cs` (merge builtin manifest + user `persistentDataPath` headers; `Resolve(id)` materializes an owned `Story` via `Story.Clone()` / deserialize). Keep `WorkingStory.LoadOfficial` delegating to `StoryCatalog.Resolve` so nothing else breaks.

Files added: `Assets/Scripts/Stories/StoryCatalog.cs`, `Assets/Resources/Stories/catalog.json` (generated). Changed: `Assets/Editor/StoryExportTool.cs` / `StoryExporter` (emit the manifest), `WorkingStory.cs` (delegate).

Verify: WebGL build. Confirm the carousel lists builtin + a persisted user story together, covers paint from the manifest without deserializing full bodies, and selecting one resolves the full body.

Rollback: keep `index.json`; revert the catalog to deserialize-all behavior; the manifest emit is additive.

Note: this step changes on-screen story ORDER if the manifest order differs from the current hardcoded `OrderStories` whitelist (verified divergence: manifest has Mastico/II/III/V/VI/Ibiza/Noel; code wants Noel 4th). The manifest order must be set to the intended display order before this ships. See open decisions.

---

## Step 6 - Add the composition root (RISKY: scene wiring)

Add `StoryRoot.cs` (MonoBehaviour, `DontDestroyOnLoad`). It constructs session + catalog + store + sprite cache and owns the jslib `SendMessage` callback surface. `Bootstrap` spawns `StoryRoot` instead of `StoryProviderManager` and calls `StoryStore.Init()` (hydrate) + catalog warm. Burn `StoryProviderManager`. The `WorkingStory` shim now reads the session off `StoryRoot`.

Files added: `Assets/Scripts/Stories/StoryRoot.cs`. Changed: `Bootstrap.cs`. Removed: `StoryProviderManager.cs`. Scene: confirm `0_MenuCollection` (or the bootstrap scene) spawns the root.

Verify: open scene 0, enter play, confirm a story selects and a gameplay scene loads (the cross-scene channel survives `LoadScene`).

Rollback: restore `StoryProviderManager` spawn in Bootstrap; remove `StoryRoot`.

---

## Step 7 - Retarget gameplay consumers off the shim (compile + scene-open per file)

One file per cycle, since the read contract is identical and this is a receiver-token swap: `GameManager`, `Title`, `Intro`, `ClickHandler`, `MenuManager`, `Scene`, `SceneLoading`. In the same passes: delete `MenuManager.ForkAndEdit`'s `IsOfficial` branch (session already owns the copy), delete `Title`'s dead `scenesCount` write, replace `SceneLoading`'s `gamePath`/Ibiza hack with `SceneData.VoiceSettings.isZambla`, and swap bare scene-index literals for `AppScenes` consts.

Files changed (one per cycle): `GameManager.cs`, `Title.cs`, `Intro.cs`, `ClickHandler.cs`, `MenuManager.cs`, `Scene.cs`, `SceneLoading.cs`. Added: `AppScenes.cs`.

Verify: per file, compile + open the owning scene and confirm the read still works.

Rollback: per file, revert to the shim read.

---

## Step 8 - Retarget editor consumers off the shim (compile + scene-open per file) (RISKY: editor scene wiring)

One file per cycle: `RM_SaveLoad`, `RM_GameManager`, `RM_MainLayout`, `RM_ImagesLayout`, `RM_ImgAnimLayout`. Make Save mean `StoryStore.Save` + flush (persist), not Export. Delete `RM_SaveLoad`'s dead legacy string-array machinery, the duplicate `MakeTextureReadable`, and the desktop `LoadSprite`. Unify the divergent anim import path to apply `AtariPalette` and the same pixelsPerUnit as the main image path (fixes the latent visual divergence, AUDIT S9).

Files changed (one per cycle): `RM_SaveLoad.cs`, `RM_GameManager.cs`, `RM_MainLayout.cs`, `RM_ImagesLayout.cs`, `RM_ImgAnimLayout.cs`.

Verify: per file, compile + open `RM_Main`, edit a scene, save, reload page, confirm the edit persisted.

Rollback: per file, revert to the shim + the old Save=Export flow.

---

## Step 9 - Rebuild the selection UI (RISKY: heavily Inspector-wired god-object)

Rewrite `RA_ScrollView` to bind slots to `StoryCard` from `StoryCatalog.Cards()`. Delete `LoadUserStories`, `json:` slots, `GetSlotKind`, `userStorySlotIndices`, the `OrderStories` duplication, the `HandleEditClicked` fork branch, all PlayerPrefs writes, and the desktop Quit/Delete/Return keys. Change `RA_ActionPanel.Show` to take one `isUser` bool. Collapse `RA_NewGame` to `WebShare` + catalog (New -> session.New + Save; Import -> WebShare + catalog.Import; Export -> WebShare; Delete -> catalog.Delete). This is the largest single change; do it after consumers are stable.

Files changed: `RA_ScrollView.cs`, `RA_ActionPanel.cs`, `RA_NewGame.cs`. Added: `WebShare.cs` (slimmed from `WebGLFileBrowser`). Scene: `0_MenuCollection.unity` (remove the 3 unused slot-prefab fields; confirm action-bar static-event wiring intact).

Verify: WebGL build. Open scene 0, confirm carousel lists builtin + user, select/play/edit/duplicate/import/export/delete all work, action bar enables/labels correctly. Because these are UnityEvent/BetterEvent wired, signature changes need scene-open verification, not just compile.

Rollback: this is the hardest to revert; commit it alone after Steps 7-8 are green, and keep the prior `RA_*` files in the previous commit.

---

## Step 10 - Delete the dead types and the nag chain (compile + WebGL build)

Now that nothing references them: delete `WorkingStory.cs`, `IStoryProvider.cs`, `ResourcesStoryProvider.cs`, `PathManager.cs`, `ExportReminder.cs`, `WebGLFileBrowser.cs` (folded into `WebShare` + `StoryRoot`), `StoryData`/`StoryMetadata`. Strip the jslib of `ShowConfirmDialog` / `SetUnsavedWorkFlag` / `InitBeforeUnloadHandler` and remove `window._rodyHasUnsavedWork`. Delete all dead PlayerPrefs writes (`scenesCount`, `customGame`, `gameToDelete`, `gameToDeleteType`, `gamePath`).

Files removed: as listed. Changed: the jslib, `Bootstrap.cs` (drop `ExportReminder.Initialize`, `IsWebGL`, `HasFileSystem`).

Verify: grep `Assets/Scripts/` for `WorkingStory`, `IsOfficial`, `gamePath`, `scenesCount`, `json:`, `IStoryProvider` -> zero runtime hits. WebGL build + full play pass.

Rollback: restore from the prior commit; this step is pure deletion so revert is clean.

---

## Step 11 - Delete the StandaloneFileBrowser tree and quarantine the export tool (RISKY: serialized native tree)

Delete the entire `Assets/StandaloneFileBrowser/` tree including the native DLLs/.so/.bundle (jslib already relocated in Step 0). Remove every `using SFB;`. Quarantine `StoryExporter`'s `levels.rody` File-IO export methods into an Editor-only asmdef; keep the model classes in runtime (already moved to `Story.cs` in Step 1). Point `StoryExportTool` at the Editor asmdef.

Files removed: `Assets/StandaloneFileBrowser/` tree. Added: `Assets/Editor/StoryTooling.asmdef`. Changed: `StoryExporter` location, `StoryExportTool.cs`.

Verify: WebGL build (confirm no broken DllImport references to the deleted tree) + run the editor export tool once to confirm builtin export still works and emits `catalog.json`.

Rollback: restore the tree from the prior commit; this is why it is last.

---

## Step 12 - Final pass (verification)

Grep for residual desktop/branching artifacts: `StandaloneFileBrowser`, `HasFileSystem`, `#else` near `UNITY_WEBGL`, `SlotKind`, `userStorySlotIndices`, `memory:`, `download:`. Confirm zero runtime hits. Full WebGL playthrough: select official, play, duplicate-and-edit, save, reload (survives), import a shared file, export a file. Confirm the only `#if UNITY_WEBGL` left is the `RodySyncFs` P/Invoke guard.

---

## Open Decisions for the User (needed before coding starts)

1. **Save semantics (most user-visible).** Today "Save" in the editor means "export a download". The plan makes Save mean "persist locally" and Export a separate sharing action. Confirm this UX flip and that the button labels change accordingly. This is a behavior change, not just a refactor.

2. **Auto-save vs explicit Save.** Persist on every edit + flush (removes the unsaved-work nag entirely, more frequent IndexedDB writes), or explicit Save button (fewer writes, reintroduces a small "unsaved work" surface)? The plan assumes explicit Save plus a page-hide backstop flush, but the choice drives how aggressively the flush fires.

3. **Editing a builtin: what does the user SEE?** (a) Editing silently produces a user copy (current fork behavior minus the "(copie)" rename), or (b) require an explicit "Duplicate" first and keep builtins strictly read-only in the catalog? Copy-on-load makes (a) free, but whether a new entry appears in the list is a product decision.

4. **Imported (shared) stories: auto-persist on import, or session-only until saved?** The plan assumes auto-persist so imports survive reload like everything else. Confirm.

5. **Story display order.** The manifest order and the current hardcoded `OrderStories` whitelist disagree (manifest: Mastico/II/III/V/VI/Ibiza/Noel; code wants Noel 4th). Making the manifest the single source WILL reorder the menu unless the manifest is set to the intended order. Provide the intended order, and the sort rule for user stories (mtime vs alphabetical vs creation order).

6. **Duplicate naming.** Forking currently appends " (copie)". Keep that, prompt for a name, or assign a fresh GUID id? Also: if a user duplicate shares a builtin id, which wins in the catalog and should the builtin stay visible alongside the copy?

7. **Durability expectation.** `persistentDataPath` is IndexedDB-backed and can be evicted by the browser or cleared in private browsing. Is local persistence acceptable as the convenience store (with export-as-file as the only hard backup), or is server/cloud sync a future requirement that should shape `StoryStore` now?

8. **`rodyMakerFirstTime`.** Leave as the one surviving PlayerPref (recommended, it is editor-hint state not story state), or move onto the session? Note it is currently re-set to 1 on every menu visit so the hint fires on every editor entry; is that a bug to fix during migration?

9. **Scene 7 (additive phoneme editor) and the `isZambla` flag with no editor UI.** In scope to wire up during this migration, or leave as-is? They touch the same files but are not strictly storage unification. (`RM_ObjLayout` persisting only zone[0] is similarly a pre-existing gap the unification exposes but does not have to fix.)
