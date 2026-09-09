# DOOMastico FPS Module

Retro FPS minigame under `Assets/DOOM/`. This document owns the module map and its
unapproved gameplay ideas; project priorities belong in [ROADMAP.md](ROADMAP.md).

Source and serialized-asset review: 2026-09-09. Values below are observations from
the named assets, not playtest results or a second balance configuration.

## Entry and scenes

The collection scene has a button wired to `LoadDOOM.LoadDOOMScene()`, which loads
`IntroMenu`. The intro menu targets `MainScene` and offers a return to the
collection. `GameFlowManager` ends play when required objectives are complete or
the player dies, then fades to `WinScene` or `LoseScene`; those scenes expose
restart and intro-menu targets.

All four scenes are under `Assets/DOOM/FPS/Scenes/` and enabled in
`ProjectSettings/EditorBuildSettings.asset`. Scene inclusion and order are owned
by that asset; do not maintain a second index list here. This audit did not run
the scene transitions or inspect a deployed build.

## Implementation map

Paths in this table are relative to `Assets/DOOM/FPS/Scripts/` unless stated otherwise.

| Responsibility | Source |
|---|---|
| Movement, aiming rotation, crouch, jump, fall damage | `PlayerCharacterController.cs`, `PlayerInputHandler.cs`, `Jetpack.cs` |
| Six weapon slots, switching and firing | `PlayerWeaponsManager.cs`; individual weapons in `WeaponController.cs` |
| Projectiles and damage | `ProjectileBase.cs`, `ProjectileStandard.cs`, `DamageArea.cs`, `Damageable.cs` (a component), `Health.cs` |
| Enemies and navigation | `EnemyController.cs`, `EnemyMobile.cs`, `EnemyTurret.cs`, `DetectionModule.cs`, `NavigationModule.cs`, `EnemyManager.cs` |
| Objectives and game flow | `Objective.cs`, `ObjectiveKillEnemies.cs`, `ObjectivePickupItem.cs`, `ObjectiveReachPoint.cs`, `ObjectiveManager.cs`, `GameFlowManager.cs`, `ActorsManager.cs` |
| HUD, crosshair, health, ammo, compass and notifications | `UI/` |
| Spatial sound and mixer groups | `AudioUtility.cs`, `AudioManager.cs` (`DOOM.FPS`, distinct from the UnityReusables audio manager) |
| Custom weapon sprite UI | `Assets/DOOM/Scripts/WeaponUIManager.cs`, `Assets/DOOM/Scripts/WeaponFireAnimator.cs` |

Walking and firing now use static events: `PlayerCharacterController.OnWalkingChanged`
feeds `WalkAnimatorSync`, and `PlayerWeaponsManager.OnFired` feeds
`WeaponFireAnimator`. Their components are serialized in the Player prefab and/or
main scene. The earlier `BoolVariable`/`SimpleEventSO` descriptions are obsolete
for those two routes. `WeaponUIManager` still has an Odin-serialized weapon
dictionary; this is not evidence that all Odin wiring has been migrated. See
[MIGRATION_GUIDE.md](MIGRATION_GUIDE.md) when tracing serialized dependencies.

## Findings that replace the earlier gameplay audit

Inspect base prefabs, prefab variants and scene-instance overrides together before
tuning. Overrides use `propertyPath` followed by `value`; matching only
`fieldName:` misses them. The review searched both forms under `Assets/DOOM/`.

| Earlier assumption | Evidence in the current assets/source |
|---|---|
| Sprint and jump still need the proposed reduction | `Prefabs/Player.prefab` already has sprint multiplier 1.5 and jump force 7. |
| Fall damage is disabled | Player prefab enables it, with speed thresholds 20/40 and damage 10/30. This is a tuning question, not a system to enable. |
| Pistol shot delay 0.3 would make it faster | `Prefabs/Weapons/Weapon_Pistol.prefab` uses 0.1; raising the delay would slow firing. |
| Shotgun needs spread 8 | `Weapon_Shotgun.prefab` already uses spread 8 and 24 pellets. The earlier six-pellet suggestion would materially change damage output. |
| Turret range 25 and Poulpe memory 6 would extend detection/pursuit | Base turret detection is 30; Poulpe target timeout is 12. Those proposed numbers reduce the existing values. |
| Critical-health feedback needs enabling/stronger values | `Prefabs/UI/GameHUD.prefab` and the main scene serialize frequency 8 and maximum alpha 1, already above the earlier 6/0.9 suggestion. Legibility still needs observation. |
| Overheat is unused | `OverheatBehavior` is enabled in Blaster and Launcher prefabs. It drives steam, emission color and cooling sound from ammo state; reload/heat gameplay is governed by `WeaponController`, including `reloadsOverTime`. Adding the effect alone does not enable a new firing constraint. |
| Enemy loot needs initial configuration | HoverBot has health loot at rate 1; Poulpe and Hagrid at 0.2; base turret has no drop. Main-scene instances also override drop rates to 0. |
| Objective delay is unused | Kill/reach prefabs serialize delays 4/6; main-scene instances override delays to 2. `waitForShow` suppresses automatic toast display; it does not implement objective sequencing. |
| 52 VFX prefabs are ready to wire | The VFX directory contains 26 `.prefab` files; `.meta` files are not extra effects. Availability does not establish whether an effect is already wired. |

## Gameplay backlog — unapproved

These are retained design options from the earlier audit, not accepted requirements,
release commitments, or defects demonstrated by this documentation review. Choose
the player outcome and observe the current game before changing values. The prior
numeric targets, code snippets, line-count estimates and phased priority order
came without playtest evidence in that document and are not implementation instructions.

| Player outcome to explore | Existing controls / implementation consideration |
|---|---|
| More deliberate movement and precise aim | Movement speeds, aim multiplier and sprint footstep cadence on Player. Check rhythm at the current sprint/jump settings first. |
| Distinct close-range, precise and heavy weapons | Shot delay, spread, pellet count, projectile speed and reload settings in weapon/projectile prefabs. Include actual damage and ammo cost when comparing weapons. |
| Readable enemy threats and dodge windows | Detection/attack range, target timeout and enemy projectile speeds. Compare distance and time-to-impact; slower enemy projectiles are an option, not inherently fairer. |
| Meaningful vertical risk and grounded recovery | Existing fall-damage curve and jetpack consume/grounded-refill/air-refill durations. Longer airtime and slower midair recovery were suggested; terrain and unlock availability determine their effect. |
| Low health and low ammo noticed sooner | Existing HUD vignette plus optional audio on entering a low-ammo state for the active weapon. Define when the warning re-arms after refill or weapon switching. |
| Clear confirmation when aim reaches an enemy | Optional target-acquisition sound in CrosshairManager; distinguish a real acquisition from forced refresh on weapon switching. |
| Sustained fire has a visible recovery cost | Evaluate existing overheat effects with the weapon's actual ammo/reload mode before changing Pistol or Blaster behavior. |
| Pickups shape encounter pacing | Existing loot system; former ideas were ammo from HoverBots/turrets and health from Poulpes. Account for scene overrides and whether the player owns the pickup's weapon. |
| Secondary goals appear at meaningful moments | Delay/reveal controls exist for objective toasts; progression-triggered reveals require an explicit trigger reaching the intended toast. |
| Victory and kill progress feel rewarding | Optional brief victory slow motion; coordinate with pause, existing fade timers and restoring time scale. Optional sounds when crossing 50%/75% kill milestones must trigger once even when a kill skips over the exact fraction. |
| Elite enemies are recognizable before attacking | Optional HoverBot/turret variants with stronger health and detection; faster HoverBot movement or shorter turret fire delay. Distinguish them visually (previous suggestion: red/orange HoverBot tint). |
| Combat effects distinguish source and state | Existing `VFX_Alert`, `VFX_Angry`, `VFX_AngrySteam` and `VFX_LazerSparksBlue/Red/Green`. Previous color idea: blue player, red enemy, green environment. Trace current wiring before adding effects. |

No gameplay code or prefab settings changed in this audit. No compile, build,
playtest or browser QA was run.
