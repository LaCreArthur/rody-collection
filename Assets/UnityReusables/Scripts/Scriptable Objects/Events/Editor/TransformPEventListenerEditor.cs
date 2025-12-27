#if UNITY_EDITOR
using UnityEditor;
using UnityReusables.ScriptableObjects.Events;

[CustomEditor(typeof(TransformPEventListenerComponent), true)]
[CanEditMultipleObjects]
public class TransformPEventListenerEditor : BasePEventListenerEditor {}
#endif