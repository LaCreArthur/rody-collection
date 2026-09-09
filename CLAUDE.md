# Rody Collection — agent entry point

**Always invoke `developing-unity-games` when working in this repo.**
Read applicable `~/unity-projects/CLAUDE.md` guidance and the workspace project
memory index. Use `studio-repo-discipline` before edits or Git writes.

## Read for the task

| Need | Owner | What it establishes |
|---|---|---|
| Understand the product | [Game design](docs/GAME_DESIGN.md) | Collection, stories, Maker, voice, creative limits and clearly labelled UX proposals; deliberately code-free |
| Decide what to do next | [Roadmap](docs/ROADMAP.md) | Priorities, completed implementation, remaining release checks |
| Work on story storage / editor state | [Architecture](docs/unify/ARCHITECTURE.md) | Current owners, data flow, save/export and content format |
| Fix a known editor/storage issue | [Audit](docs/unify/AUDIT.md) | Source evidence, uncertainty and the narrow check needed for each finding |
| Check historical storage intent | [Decisions](docs/unify/DECISIONS.md) | Recorded direction versus old unapproved defaults |
| Work on voices | [Speech engine](docs/SPEECH_ENGINE.md) | Current speech contract, French authoring, evidence and reproduction commands |
| Explain a creator flow | [Maker tutorial](docs/RODY_MAKER_TUTORIAL.md) | French instructions for current behavior and limitations |
| Work on the FPS bonus | [DOOMastico](docs/DOOM_FPS.md) | Current module and unapproved gameplay ideas |
| Recover serialized wiring | [Migration guide](docs/MIGRATION_GUIDE.md) | Toolkit limits and evidence-led recovery |
| Recover why something changed | [Development log](DEVLOG.md) | Dated history, not current implementation instructions |

Read the design and roadmap to orient a fresh session, then the relevant reference.
Do not load every historical investigation for a routine edit. Current source and
serialized consumers establish behavior; the design describes product intent;
roadmap status does not prove runtime success or approval of a proposal.

## Project facts and boundaries

Rody Collection is a French retro adventure collection with its own story editor
and voice workbench. WebGL is the current player target. The Editor can work with
local story files, but browser import/export wrappers are not desktop file pickers.

- Unity version: [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).
- Packages: [manifest.json](Packages/manifest.json), with resolved versions in its lockfile.
- Build scene paths/order: [EditorBuildSettings.asset](ProjectSettings/EditorBuildSettings.asset).
- Deployment: [GitHub Pages workflow](.github/workflows/deploy-pages.yml).
  A push to `master` triggers a build/deploy. Batch local work; publish only when asked.
- Story runtime: `Assets/Scripts/Stories/`; `StoryRoot.Session` owns the selected story.
  The removed provider/WorkingStory architecture is historical.
- Maker: `Assets/Scripts/RodyMaker/`; collection: `Assets/Scripts/RodyAnthology/`.
- Gameplay: `Assets/Scripts/GameManager.cs`; speech: `SoundManager.cs`,
  `RodySpeechEngine.cs`, `Assets/Scripts/synth/` and the speech reference.
- Built-in generation and portable story format: architecture reference above.
- `Assets/UnityReusables` is currently vendored as ordinary tracked files, not a
  Git submodule. Odin/Sirenix and DOTween files are tracked too; inspect the actual
  checkout/package state before assuming a plugin must be installed manually.

## Inspector wiring

Existing behavior also lives in BetterEvents, ScriptableObject events, UnityEvents,
prefabs and scenes. Follow the actual event/reference chain; reading a method body
alone does not establish that the user can reach it. Prefab variants encode field
changes under `propertyPath` with the value on the next line. Check those as well
as base fields before claiming an assignment is absent.

## Code Preferences

- Use `Path.Combine()` for all file paths
- Prefer Observable pattern over PlayerPrefs for state
- Prefer explicit code over Inspector wiring for new features
- Use conventional commits: `fix:`, `feat:`, `docs:`
- **Never null-check serialized fields** - If a prefab/reference isn't assigned in the Inspector, let it fail loudly with NullReferenceException rather than silently skipping
- **Static events for singleton/manager classes** - Use `public static event Action OnFired` instead of instance events to avoid Inspector wiring for subscribers
- **[RequireComponent] + GetComponent for same-object deps** - Use `[RequireComponent(typeof(T))]` + `GetComponent<T>()` in Awake instead of serialized fields for guaranteed same-object components
- **Expression-bodied members** - Use `void Awake() => _x = GetComponent<T>();` for simple one-liners
- **Omit redundant `private`** - C# defaults to private, write `void Update()` not `private void Update()`
- **Cache static/constant data** - If generating the same data every time (e.g., blank textures, default configs), compute once and cache as static field or constant. Don't recreate identical data repeatedly.
- **Always grep for patterns before declaring a fix complete** - When fixing platform-specific code (`#if UNITY_WEBGL`), always run `grep -r "PATTERN" Assets/Scripts/` to find ALL occurrences. Don't rely on testing alone—code paths may not be exercised until later scenes.
- **Fix for simplicity, never add complexity** - When something doesn't work, INVESTIGATE THE ROOT CAUSE first. Don't jump to adding backward compatibility, abstractions, or workarounds. The simplest fix is usually the correct one. Example: If exports fail, check if the source path changed—don't add code to handle missing data.
