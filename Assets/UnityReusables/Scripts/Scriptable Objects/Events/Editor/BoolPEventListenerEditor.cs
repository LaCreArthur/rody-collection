#if UNITY_EDITOR
using UnityEditor;
using UnityReusables.ScriptableObjects.Events;

[CustomEditor(typeof(BoolPEventListenerComponent), true)]
[CanEditMultipleObjects]
public class BoolPEventListenerEditor : BasePEventListenerEditor {}
#endif