using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

public class TransformListSetter : VariableListSetter
{
    [PropertyOrder(-10)]
    [SerializeField] private TransformListVariable list;
    [SerializeField] private bool addDisable;
    

    protected override void Clear()
    {
        list.Clear();
    }

    protected override void AddMyVariable()
    {
        if (list.v.Contains(this.transform)) return;
        list.Add(this.transform);
    }

    protected override void AddChildVariable(Transform child)
    {
        if (addDisable || child.gameObject.activeSelf)
            list.Add(child);
    }

    protected override void RemoveMyVariable()
    {
        list.v.Remove(this.transform);
    }
}