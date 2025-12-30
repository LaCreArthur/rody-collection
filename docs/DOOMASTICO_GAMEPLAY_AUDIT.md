# DOOMastico Gameplay Audit

Quick wins analysis for DOOMastico FPS module - gameplay improvements without major refactoring.

## Summary

| Category | Count | Effort |
|----------|-------|--------|
| Inspector-only tweaks | 15+ parameters | Zero code |
| Enable existing systems | 5 systems | Zero code |
| Code additions | 4 features | ~40 lines |
| Elite enemy variants | 2 prefabs | Duplicate + tune |

---

## A. Inspector-Only Tweaks

### A1. Movement Feel
**Prefab**: `Assets/DOOM/FPS/Prefabs/Player.prefab`
**Component**: `PlayerCharacterController`

| Parameter | Default | Suggested | Rationale |
|-----------|---------|-----------|-----------|
| `sprintSpeedModifier` | 2.0 | 1.5 | 2x feels too fast, reduces tactical awareness |
| `jumpForce` | 9.0 | 7.0 | Lower jump = more grounded combat feel |
| `aimingRotationMultiplier` | 0.4 | 0.25 | Tighter aim-down-sights precision |
| `footstepSFXFrequencyWhileSprinting` | 1.0 | 1.5 | Faster footsteps while sprinting adds urgency |

### A2. Weapon Balance
**Prefabs**: `Assets/DOOM/FPS/Prefabs/Weapons/*.prefab`
**Component**: `WeaponController`

| Weapon | Parameter | Suggested | Rationale |
|--------|-----------|-----------|-----------|
| Pistol | `delayBetweenShots` | 0.3 | Faster pacing for starter weapon |
| Shotgun | `bulletSpreadAngle` | 8.0 | Wider spread = satisfying close-range |
| Shotgun | `bulletsPerShot` | 6 | Pellet count for spread shot feel |
| Launcher | `ammoReloadDelay` | 3.0 | High-damage = longer reload penalty |

### A3. Enemy Difficulty
**Prefabs**: `Assets/DOOM/FPS/Prefabs/Enemies/*.prefab`
**Component**: `DetectionModule`

| Enemy | Parameter | Suggested | Rationale |
|-------|-----------|-----------|-----------|
| Turret | `detectionRange` | 25.0 | Turrets should cover larger areas |
| HoverBot | `attackRange` | 8.0 | Force closer engagement |
| Poulpe | `knownTargetTimeout` | 6.0 | More persistent pursuit |

### A4. Projectile Speed Variety
**Prefabs**: `Assets/DOOM/FPS/Prefabs/Projectiles/*.prefab`
**Component**: `ProjectileStandard`

| Projectile | `speed` | Rationale |
|------------|---------|-----------|
| Pistol | 25.0 | Fast, precise |
| Shotgun | 20.0 | Standard |
| Launcher | 15.0 | Slow, powerful |
| Enemy projectiles | 15.0 | Slightly slower than player = fairness |

### A5. Jetpack Tuning
**Prefab**: `Assets/DOOM/FPS/Prefabs/Player.prefab`
**Component**: `Jetpack`

| Parameter | Default | Suggested | Rationale |
|-----------|---------|-----------|-----------|
| `consumeDuration` | 1.5 | 2.0 | Slightly more airtime |
| `refillDurationGrounded` | 2.0 | 1.5 | Reward grounded play |
| `refillDurationInTheAir` | 5.0 | 8.0 | Penalize hovering playstyle |

---

## B. Underutilized Systems to Enable

### B1. Fall Damage
**Status**: Built but disabled
**File**: `PlayerCharacterController.cs`

Enable on Player prefab:
- `recievesFallDamage` = true
- `minSpeedForFallDamage` = 12.0 (forgiving threshold)
- `fallDamageAtMinSpeed` = 10.0
- `fallDamageAtMaxSpeed` = 50.0

**Impact**: Adds risk/reward to vertical gameplay and jetpack use.

### B2. Critical Health Vignette
**Status**: Exists but too subtle
**File**: `FeedbackFlashHUD.cs`

Tune on GameHUD prefab:
- `pulsatingVignetteFrequency` = 6.0 (was 4.0, more urgent)
- `criticaHealthVignetteMaxAlpha` = 0.9 (was 0.8, more visible)

**Impact**: Players notice low health sooner.

### B3. Weapon Overheat System
**Status**: Sophisticated but unused
**File**: `OverheatBehavior.cs`

Features available:
- Steam VFX emission scales with heat
- Color gradient feedback (full heat = bright glow)
- Cooling sound with volume curve

**Opportunity**: Enable on high-fire-rate weapons (Pistol, Blaster) to add tactical depth.

### B4. Enemy Loot Drops
**Status**: System exists, needs configuration
**File**: `EnemyController.cs`

Configure per enemy prefab:
- `dropRate` = 0.0-1.0 probability
- `lootPrefab` = Health/Ammo pickup prefab

Suggested drops:
| Enemy | Drop | Rate |
|-------|------|------|
| HoverBot | Small Ammo | 0.5 |
| Poulpe | Health | 0.3 |
| Turret | Large Ammo | 0.7 |

### B5. Objective Visibility Delay
**Status**: Built but unused
**File**: `Objective.cs`

Parameters available:
- `delayVisible` - seconds before objective appears
- `waitForShow` - require explicit show trigger

**Opportunity**: Reveal secondary objectives after primary complete for mission progression feel.

---

## C. Code Additions (Optional)

### C1. Low Ammo Audio Warning
**File**: `Assets/DOOM/FPS/Scripts/UI/AmmoCounter.cs`
**Effort**: ~10 lines

```csharp
// Add fields after line 32:
[Header("Low Ammo Warning")]
public AudioClip lowAmmoWarning;
public float lowAmmoThreshold = 0.2f;
bool m_LowAmmoPlayed;

// Add in Update() after line 56:
if (currenFillRatio < lowAmmoThreshold && !m_LowAmmoPlayed && isActiveWeapon)
{
    AudioUtility.CreateSFX(lowAmmoWarning, transform.position, AudioUtility.AudioGroups.HUDObjective, 0f);
    m_LowAmmoPlayed = true;
}
else if (currenFillRatio >= lowAmmoThreshold)
    m_LowAmmoPlayed = false;
```

### C2. Crosshair Target Audio
**File**: `Assets/DOOM/FPS/Scripts/UI/CrosshairManager.cs`
**Effort**: ~5 lines

```csharp
// Add field after line 7:
public AudioClip targetAcquiredSound;

// Add inside first if block (line 38-43), after setting crosshair:
if (targetAcquiredSound)
    AudioUtility.CreateSFX(targetAcquiredSound, transform.position, AudioUtility.AudioGroups.HUDObjective, 0f);
```

### C3. Victory Slow-Mo
**File**: `Assets/DOOM/FPS/Scripts/GameFlowManager.cs`
**Effort**: ~10 lines

```csharp
// Add fields after line 22:
[Header("Victory Effect")]
public float victorySlowMoDuration = 0.5f;
public float victorySlowMoScale = 0.5f;

// Add in EndGame(true) block, after line 86:
StartCoroutine(VictorySlowMo());

// Add method:
System.Collections.IEnumerator VictorySlowMo()
{
    Time.timeScale = victorySlowMoScale;
    yield return new WaitForSecondsRealtime(victorySlowMoDuration);
    Time.timeScale = 1f;
}
```

### C4. Kill Milestone Feedback
**File**: `Assets/DOOM/FPS/Scripts/ObjectiveKillEnemies.cs`
**Effort**: ~10 lines

```csharp
// Add field after line 13:
public AudioClip milestoneSound;

// Add in OnKillEnemy() after line 48:
float progress = (float)m_KillTotal / killsToCompleteObjective;
if ((progress >= 0.5f && progress < 0.55f) || (progress >= 0.75f && progress < 0.8f))
{
    if (milestoneSound)
        AudioUtility.CreateSFX(milestoneSound, transform.position, AudioUtility.AudioGroups.HUDObjective, 0f);
}
```

---

## D. Elite Enemy Variants

Create by duplicating existing prefabs and tweaking values:

### Elite HoverBot
**Source**: `Enemy_HoverBot.prefab`
**Create**: `Enemy_HoverBot_Elite.prefab`

| Component | Parameter | Change |
|-----------|-----------|--------|
| Health | `maxHealth` | +50% |
| NavigationModule | `moveSpeed` | +25% |
| DetectionModule | `detectionRange` | +20% |
| Material | color tint | Red/Orange |

### Elite Turret
**Source**: `Enemy_Turret.prefab`
**Create**: `Enemy_Turret_Elite.prefab`

| Component | Parameter | Change |
|-----------|-----------|--------|
| Health | `maxHealth` | +50% |
| EnemyTurret | `detectionFireDelay` | -50% (faster) |
| DetectionModule | `detectionRange` | +30% |

---

## E. Existing VFX Library

52 VFX prefabs available in `Assets/DOOM/FPS/Prefabs/VFX/`:

### Ready to Wire
| VFX | Use Case |
|-----|----------|
| `VFX_Alert` | Enemy first detects player |
| `VFX_Angry` | Enemy enters attack mode |
| `VFX_AngrySteam` | Enemy takes damage |
| `VFX_LaserSpark_Red/Green/Blue` | Color-coded hit feedback |

### Suggested Color Coding
- Player weapons: Blue sparks
- Enemy weapons: Red sparks
- Environment/neutral: Green sparks

---

## Priority Order

### Phase 1: Inspector Tuning (Zero Code)
1. Movement feel (A1)
2. Weapon balance (A2)
3. Enable fall damage (B1)
4. Configure enemy loot drops (B4)
5. **Playtest**

### Phase 2: Audio Feedback (Optional Code)
1. Crosshair target beep (C2)
2. Low ammo warning (C1)
3. **Playtest**

### Phase 3: Polish (Optional)
1. Victory slow-mo (C3)
2. Kill milestones (C4)
3. Elite enemy variants (D)
4. Critical health vignette tuning (B2)

---

## Key Files Reference

| Purpose | Path |
|---------|------|
| Player movement | `Assets/DOOM/FPS/Scripts/PlayerCharacterController.cs` |
| Weapons | `Assets/DOOM/FPS/Scripts/WeaponController.cs` |
| Projectiles | `Assets/DOOM/FPS/Scripts/ProjectileStandard.cs` |
| Enemy AI | `Assets/DOOM/FPS/Scripts/EnemyController.cs` |
| Enemy detection | `Assets/DOOM/FPS/Scripts/DetectionModule.cs` |
| Turret behavior | `Assets/DOOM/FPS/Scripts/EnemyTurret.cs` |
| Game flow | `Assets/DOOM/FPS/Scripts/GameFlowManager.cs` |
| HUD feedback | `Assets/DOOM/FPS/Scripts/UI/FeedbackFlashHUD.cs` |
| Ammo display | `Assets/DOOM/FPS/Scripts/UI/AmmoCounter.cs` |
| Crosshair | `Assets/DOOM/FPS/Scripts/UI/CrosshairManager.cs` |
| Kill objectives | `Assets/DOOM/FPS/Scripts/ObjectiveKillEnemies.cs` |
| Jetpack | `Assets/DOOM/FPS/Scripts/Jetpack.cs` |
| Overheat | `Assets/DOOM/FPS/Scripts/OverheatBehavior.cs` |
