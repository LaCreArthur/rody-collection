# Unity Serialization Recovery

Repository-specific workflow for missing references and BetterEvent/Odin wiring.
Reviewed against `tools/unity-migration-toolkit.sh` on 2026-09-09. This guide does
not prescribe a new migration or removal of working components.

## Preserve evidence before changing anything

1. Record the failing object, expected behavior, Console errors, Editor version and
   package resolution state. Preserve the affected scene/prefab and `.meta` files
   before saving through a different Editor or serializer version.
2. Trace the actual component and its references in the current scene, its source
   prefab, variants and overrides. Compare with a known-working revision when
   fields appear empty; neither a newer file nor an older commit automatically
   defines the intended behavior.
3. Resolve the failing boundary before replacing code: missing asset, unavailable
   package/submodule, broken GUID, unresolved script type, or lost serialized wiring.
   An unrecognized GUID is not permission to delete a component or rebuild Library.

Moving an asset with its `.meta` preserves its identity; losing that metadata can
break references. See Unity's [asset metadata documentation](https://docs.unity3d.com/6000.0/Documentation/Manual/AssetMetadata.html).
Do not treat merely opening a scene as proof that serialization was permanently
lost: inspect file changes and the affected fields first.

## Use the existing toolkit as a search aid

Run from the repository root; the script is already executable. `help` is the
command reference, so new commands belong in the tool rather than a second list here.

```bash
./tools/unity-migration-toolkit.sh help
./tools/unity-migration-toolkit.sh build-cache
./tools/unity-migration-toolkit.sh missing-scripts Assets/DOOM/FPS/Scenes/MainScene.unity
./tools/unity-migration-toolkit.sh be-audit Assets/DOOM/FPS/Scenes/MainScene.unity
./tools/unity-migration-toolkit.sh so-find-listeners Assets/Scenes
```

The names and success messages overstate the scanner's coverage:

| Command family | What its implementation actually checks / misses |
|---|---|
| `missing-scripts` | Compares **every textual GUID** in the supplied file, not just `m_Script`, against a cache of `.meta` files under `Assets` and `Library/PackageCache`. Results may be textures/prefabs/other assets, not scripts. Null script references without a GUID and unresolved compiled types are outside this check. |
| `build-cache` | Writes shared `/tmp/unity_guid_cache.txt`; rebuild for this project before scanning. The automatic one-hour expiry does not detect a cache from another project. Embedded/local package locations outside those scanned folders need separate inspection. |
| Historical `missing-scripts COMMIT:path` | Reads old serialized data but compares it with the **current** cache. A report does not establish whether that historical project was broken. |
| `guid-lookup` | Searches current asset/package-cache metadata, then git history. A history hit is not proof of deletion; inspect the matching change. |
| `be-*` | Finds literal BetterEvent mentions and recognizable fragments of Odin bytes. This is a heuristic, not a complete serializer or inheritance/reference-graph audit. |
| `so-find-listeners`, `so-usages` | Search selected textual forms. `so-usages` scans scenes, prefabs and C# but omits `.asset` consumers; follow those separately. |
| `unused-files` | Writes a candidate log after a limited GUID/name search. It omits dependency forms such as `.asset` references, Resources/string loading, packages and build-time consumers. Its output is not a deletion list. |

## Resolve a candidate through the governing system

Start with the component's complete `m_Script` reference, including GUID and
fileID. Do not replace it merely because a different script has a similar name.

```bash
# Candidate script references in the actual scene (also inspect prefab sources).
rg -n 'm_Script:' Assets/DOOM/FPS/Scenes/MainScene.unity

# Replace YOUR_GUID with the candidate; include ignored local assets/packages.
rg --hidden --no-ignore -l '^guid: YOUR_GUID$' Assets Packages Library/PackageCache -g '*.meta'

# History is recovery evidence; inspect before restoring named files.
git log --all -p --full-history -S 'YOUR_GUID' -- '*.meta'
git show 'COMMIT:Assets/path/Scene.unity'
```

When the Editor is available, resolve the GUID with
[`AssetDatabase.GUIDToAssetPath`](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AssetDatabase.GUIDToAssetPath.html)
and inspect the exact referenced script/subasset. For a `MonoScript`, inspect its
[`GetClass()`](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/MonoScript.GetClass.html)
result as well as existing Console errors; finding metadata alone does not verify
that the script type is usable. Discover the installed Pipeline commands with
`unity list --project-path /absolute/path/to/project` and use the ready Editor's
inspection tools rather than launching a second Editor or adding a scanner.

| Evidence | Next action |
|---|---|
| Asset/package/submodule absent from resolved project | Restore the intended dependency at the recorded revision; check manifest, lockfile, submodule state and local-only plugin setup. A known package GUID alone does not diagnose cache corruption. |
| Matching script exists but GUID changed | Compare the old asset and metadata with the new file; restore the original `.meta` only when identity is established. Review all affected references. |
| GUID resolves but script type cannot load | Inspect the existing compiler/import errors, assembly and platform configuration, and class identity before touching scene YAML. |
| Component exists but behavior/wiring is missing | Inspect UnityEvents, Odin data, referenced objects and prefab overrides; recover the specific lost behavior from evidence. |
| Component's purpose is still unknown | Preserve it and record the unresolved dependency. Remove it only after its behavior is understood and its removal is in scope. |

## Recover event wiring without inventing behavior

Keep the complete original Odin serialization block and `unityReferences` array.
The toolkit strips zero bytes to expose some ASCII-like strings; it is not a
UTF-16LE or Odin binary decoder and can misread Unicode, values and reference
positions. Use `declaringType`, `methodName` and `ParameterValues` fragments as
leads, then inspect the original serialization with the matching serializer when
exact recovery matters. Trace each target by its actual local fileID or external
GUID/fileID pair; the script's nearby-line extraction does not establish ownership.

Follow the full path: caller → ScriptableObject/event → listener → target →
serialized arguments. Include inactive objects, source prefabs, variant overrides
(`propertyPath` plus the following `value`) and `.asset` consumers. A source-code
search cannot reconstruct Inspector-only wiring by itself.

When a direct code dependency replaces a recovered behavior, preserve its target,
arguments, enable/disable lifecycle and initial state. The existing
`Assets/DOOM/FPS/Scripts/WalkAnimatorSync.cs` demonstrates subscribing and
unsubscribing the **same named handler**. Separate inline lambdas in `+=` and `-=`
do not remove the original subscription. A state-change event also needs an
explicit initial-state policy for a subscriber enabled after the last change.
New static events are appropriate for singleton/manager broadcasts, not a
universal replacement for state or per-instance relationships.

Before deleting old assets, trace serialized references, C# consumers, runtime
loading keys and build/package consumers for the affected asset. Inspect the final
diff for lost behavior and references. Report separately what source/serialization
inspection established and what still requires runtime verification; compile and
build checks remain opt-in for this project.
