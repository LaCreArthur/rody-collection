# Third-Party Plugins

## Odin Inspector (Sirenix)

- **Location**: `Assets/Plugins/Sirenix/`
- **Purpose**: Enhanced Unity inspector, serialization, and editor tools

### Common Attributes Used
```csharp
[ShowIf("condition")]      // Conditional visibility
[HideIf("condition")]      // Conditional hiding
[Button]                   // Inspector button
[Title("Section")]         // Section headers
[OnValueChanged("Method")] // Change callbacks
[ListDrawerSettings]       // List customization
```

## DOTween (Demigiant)

- **Package**: Installed via Asset Store
- **Purpose**: High-performance tweening library

### Common Usage
```csharp
transform.DOMove(target, duration);
transform.DOScale(scale, duration);
image.DOFade(alpha, duration);
DOVirtual.DelayedCall(delay, callback);
```

## Nice Vibrations (Lofelt)

- **Location**: `Assets/Feel/NiceVibrations/`
- **Namespace**: `Lofelt.NiceVibrations`
- **Previous Namespace**: `MoreMountains.NiceVibrations`

### API Comparison

| Old API | New API |
|---------|---------|
| `MMVibrationManager.Haptic(HapticTypes.X)` | `HapticPatterns.PlayPreset(PresetType.X)` |
| `HapticTypes` enum | `HapticPatterns.PresetType` enum |

### Preprocessor Defines
- `MOREMOUNTAINS_NICEVIBRATIONS` - Old plugin
- `NICEVIBRATIONS_INSTALLED` - New Lofelt version

### Haptic Types
```csharp
HapticPatterns.PresetType.Selection
HapticPatterns.PresetType.Success
HapticPatterns.PresetType.Warning
HapticPatterns.PresetType.Failure
HapticPatterns.PresetType.LightImpact
HapticPatterns.PresetType.MediumImpact
HapticPatterns.PresetType.HeavyImpact
HapticPatterns.PresetType.RigidImpact
HapticPatterns.PresetType.SoftImpact
```

## Cinemachine

- **Package**: `com.unity.cinemachine` v2.10.3
- **Purpose**: Advanced camera system

### Usage
- Virtual cameras for smooth transitions
- Camera shake via `CinemachineImpulseSource`

## TextMeshPro

- **Package**: `com.unity.textmeshpro` v3.0.7
- **Purpose**: Advanced text rendering

### Usage
- All UI text displays
- Ammo counters, health bars, etc.

## Post Processing

- **Package**: `com.unity.postprocessing` v3.4.0
- **Purpose**: Visual effects (bloom, color grading, etc.)

**Note**: This is the legacy post-processing stack, not URP post-processing.

## NOT Installed

The following plugins are referenced in code but not installed:

| Plugin | Status |
|--------|--------|
| EasyMobile | References removed, code guarded with `#if EASY_MOBILE` |
| URP | Code guarded with `#if UNITY_PIPELINE_URP` |
