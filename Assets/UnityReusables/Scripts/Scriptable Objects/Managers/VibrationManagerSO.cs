#if MOREMOUNTAINS_NICEVIBRATIONS
using MoreMountains.NiceVibrations;
#elif NICEVIBRATIONS_INSTALLED
using Lofelt.NiceVibrations;
using HapticTypes = Lofelt.NiceVibrations.HapticPatterns.PresetType;
#endif
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;
#if !MOREMOUNTAINS_NICEVIBRATIONS && !NICEVIBRATIONS_INSTALLED
// create enum HapticTypes so the class compiles and references are not broken if nicevibration is not imported
public enum HapticTypes { Selection, Success, Warning, Failure, LightImpact, MediumImpact, HeavyImpact, RigidImpact, SoftImpact, None }
#endif
[CreateAssetMenu(menuName = "Scriptable Objects/Managers/Vibration Manager")]
public class VibrationManagerSO : ScriptableObject
{
    [SerializeField]
    private BoolVariable isHaptic = default;

    private void OnEnable()
    {
        isHaptic.AddOnChangeCallback(SetHapticsActive);
    }

    private void OnDisable()
    {
        isHaptic.RemoveOnChangeCallback(SetHapticsActive);
    }

    public void SetHapticsActive()
    {
        #if UNITY_ANDROID
        Handheld.Vibrate();
        #endif
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.SetHapticsActive(isHaptic.v);
        MMVibrationManager.Haptic(HapticTypes.Selection);
        #endif
    } 
    
    public void Vibrate() {
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.Vibrate();
        #endif
    }
    
    public void Haptic(HapticTypes type, bool defaultToRegularVibrate, bool alsoRumble, MonoBehaviour coroutineSupport, int controllerID) {
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.Haptic(type, defaultToRegularVibrate, alsoRumble, coroutineSupport, controllerID);
        #endif
    }
    
    public void Haptic(HapticTypes type) {
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.Haptic(type);
        #endif
    }
    
    public void HapticSuccess() {
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.Haptic(HapticTypes.Success);
        #endif
    }
    
    public void HapticFailure() {
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.Haptic(HapticTypes.Failure);
        #endif
    }
    
    public void HapticSelection() {
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.Haptic(HapticTypes.Selection);
        #endif
    }
    
    public void HapticSoft() {
#if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.Haptic(HapticTypes.SoftImpact);
#endif
    }
    
    public void TransientHaptic(float intensity, float sharpness, bool alsoRumble, MonoBehaviour coroutineSupport, int controllerID)
    {
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.TransientHaptic(intensity, sharpness, alsoRumble, coroutineSupport, controllerID);
        #endif
    }
    
    public void TransientHaptic(bool vibrateiOS, float iOSIntensity, float iOSSharpness, bool vibrateAndroid, float androidIntensity, float androidSharpness, bool vibrateAndroidIfNoSupport, bool rumble, float rumbleIntensity, float rumbleSharpness, int controllerID, MonoBehaviour coroutineSupport, bool threaded) {
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.TransientHaptic(vibrateiOS, iOSIntensity, iOSSharpness, vibrateAndroid, androidIntensity, androidSharpness, vibrateAndroidIfNoSupport, rumble, rumbleIntensity, rumbleSharpness, controllerID, coroutineSupport, threaded);
        #endif
    }
    
    public void ContinuousHaptic(float intensity, float sharpness, float duration, HapticTypes fallbackOldiOS, MonoBehaviour mono, bool alsoRumble, int controllerID, bool threaded, bool fullIntensity) {
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.ContinuousHaptic(intensity, sharpness, duration, fallbackOldiOS, mono, alsoRumble, controllerID, threaded, fullIntensity);
        #endif
    }
    
    public void ContinuousHaptic(bool vibrateiOS, float iOSIntensity, float iOSSharpness, HapticTypes fallbackOldiOS, bool vibrateAndroid, float androidIntensity, float androidSharpness, bool vibrateAndroidIfNoSupport, bool rumble, float rumbleIntensity, float rumbleSharpness, int controllerID, float duration, MonoBehaviour mono, bool threaded, bool fullIntensity) {
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.ContinuousHaptic(vibrateiOS, iOSIntensity, iOSSharpness, fallbackOldiOS, vibrateAndroid, androidIntensity, androidSharpness, vibrateAndroidIfNoSupport, rumble, rumbleIntensity, rumbleSharpness, controllerID, duration, mono, threaded, fullIntensity);
        #endif
    }
    
    public void UpdateContinuousHaptic(float intensity, float sharpness, bool alsoRumble, int controllerID, bool threaded) {
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.UpdateContinuousHaptic(intensity, sharpness, alsoRumble, controllerID, threaded);
        #endif
    }
    
    public void UpdateContinuousHaptic(bool ios, float iosIntensity, float iosSharpness, bool android, float androidIntensity, float androidSharpness, bool rumble, float rumbleIntensity, float rumbleSharpness, int controllerID, bool threaded) {
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.UpdateContinuousHaptic(ios, iosIntensity, iosSharpness, android, androidIntensity, androidSharpness, rumble, rumbleIntensity, rumbleSharpness, controllerID, threaded);
        #endif
    }
    
    public void StopAllHaptics(bool alsoRumble) {
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.StopAllHaptics(alsoRumble);
        #endif
    }
    
    public void StopContinuousHaptic(bool alsoRumble) {
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.StopContinuousHaptic(alsoRumble);
        #endif
    }
}