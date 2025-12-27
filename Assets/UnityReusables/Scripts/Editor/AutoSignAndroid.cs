using UnityEditor;
 
[InitializeOnLoad]
public class AutoSignAndroid {
#if UNITY_ANDROID
    static AutoSignAndroid() {
 
        PlayerSettings.keystorePass = "oittanos0103";
        PlayerSettings.keyaliasPass = "010392";
    }
#endif
}