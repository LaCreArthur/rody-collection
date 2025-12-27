#if UNITY_EDITOR
using UnityEditor;
using UnityReusables.ScriptableObjects.Events;

[CustomEditor(typeof(Vector3PEventListenerComponent), true)]
[CanEditMultipleObjects]
public class Vector3PEventListenerEditor : BasePEventListenerEditor {}

#endif