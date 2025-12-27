#if UNITY_EDITOR
using UnityEditor;
using UnityReusables.ScriptableObjects.Events;

[CustomEditor(typeof(StringPEventListenerComponent), true)]
[CanEditMultipleObjects]
public class StringPEventListenerEditor : BasePEventListenerEditor
{ }
#endif