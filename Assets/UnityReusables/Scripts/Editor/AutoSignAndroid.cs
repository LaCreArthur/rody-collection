using UnityEditor;
 
[InitializeOnLoad]
public class AutoSignAndroid {
#if UNITY_ANDROID
    static AutoSignAndroid() {
 
        PlayerSettings.Android.keystorePass = "oittanos0103";
        PlayerSettings.Android.keyaliasPass = "010392";
    }
#endif
}