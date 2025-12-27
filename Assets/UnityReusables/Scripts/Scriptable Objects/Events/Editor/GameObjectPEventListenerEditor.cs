#if UNITY_EDITOR
using UnityEditor;
using UnityReusables.ScriptableObjects.Events;

[CustomEditor(typeof(GameObjectPEventListenerComponent), true)]
[CanEditMultipleObjects]
public class GameObjectPEventListenerEditor : BasePEventListenerEditor
{}
#endif