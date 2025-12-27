using Sirenix.OdinInspector;
using UnityEngine;

public abstract class VariableListSetter : MonoBehaviour
{
    [Header("This Transform")]
    [SerializeField] private bool addMyVariableOnAwake;
    [ShowIf("addMyVariableOnAwake")]
    [SerializeField] private bool autoRemove = true;
    
    [Header("Child Transforms")]
    [SerializeField] private bool addChildVariablesOnAwake;

    //private static bool firstUse = true;
    
    void Awake()
    {
        // leftovers and initial wrong values need to be cleaned before use
        // if (firstUse)
        // {
        //     Clear();
        //     firstUse = false;
        // }
        if (addMyVariableOnAwake) AddMyVariable();
        if (addChildVariablesOnAwake) AddChildrenVariable();
    }
    
    protected abstract void Clear();
    protected abstract void AddMyVariable();
    protected abstract void AddChildVariable(Transform child);

    protected virtual void AddChildrenVariable()
    {
        //Debug.Log($"{this.name} has {transform.childCount} children");
        Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            AddChildVariable(transform.GetChild(i));
        }   
    }

    private void OnDestroy()
    {
        if (addMyVariableOnAwake && autoRemove)
        {
            RemoveMyVariable();
        }
    }

    protected abstract void RemoveMyVariable();
}