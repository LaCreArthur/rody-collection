using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

public class ComponentListSetter<T> : MonoBehaviour
{
    [SerializeField] private BaseListVariable<T> BaseList;
    [SerializeField] private bool addMyComponentOnAwake;
    [SerializeField] private bool addChildComponentsOnAwake;
    
    void Awake()
    {
        if (addMyComponentOnAwake) AddMyComponent();
        if (addChildComponentsOnAwake) AddChildComponents();
    }

    private void AddMyComponent()
    {
        BaseList.Add(GetComponent<T>());
    }
    
    private void AddChildComponents()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            BaseList.Add(transform.GetChild(i).GetComponent<T>());
        }
    }
}