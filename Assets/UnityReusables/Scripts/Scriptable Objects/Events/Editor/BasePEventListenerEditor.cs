#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEngine;

public class BasePEventListenerEditor : OdinEditor
{
    private PropertyTree _tree;
    private InspectorProperty m_GameEvents;
    private InspectorProperty m_Actions;
    protected override void OnEnable()
    {
        _tree = this.Tree;
        m_GameEvents = _tree.GetPropertyAtPath("castedGameEvents");
        m_Actions    = _tree.GetPropertyAtPath("actions");

    }

    public override void OnInspectorGUI()
    {
        _tree.BeginDraw(true);
        serializedObject.Update();
        m_GameEvents.Draw(new GUIContent("Game Events"));
        m_Actions.Draw();
        _tree.EndDraw();
    }
}
#endif