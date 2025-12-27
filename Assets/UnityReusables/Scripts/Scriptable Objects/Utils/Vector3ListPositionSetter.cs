using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

public class Vector3ListPositionSetter : VariableListSetter
{
    [PropertyOrder(-10)]
    [SerializeField] private Vector3ListVariable list;

    protected override void Clear()
    {
        list.v.Clear();
    }

    protected override void AddMyVariable()
    {
        list.Add(transform.position);
    }

    protected override void AddChildVariable(Transform child)
    {
        list.Add(child.position);
    }

    protected override void RemoveMyVariable()
    {
        list.v.Remove(transform.position);
    }
}