#if UNITY_EDITOR
using UnityEditor;
using UnityReusables.ScriptableObjects.Events;

[CustomEditor(typeof(IntPEventListenerComponent), true)]
[CanEditMultipleObjects]
public class IntPEventListenerEditor : BasePEventListenerEditor
{ }
#endif