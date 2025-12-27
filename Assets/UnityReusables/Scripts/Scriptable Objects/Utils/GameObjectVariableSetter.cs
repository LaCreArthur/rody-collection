using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

public class GameObjectVariableSetter : MonoBehaviour
{
    public GameObjectVariable GameObjectVariable;
    public bool onAwake;
    
    void Awake()
    {
        if (onAwake) Set();
    }

    public void Set()
    {
        GameObjectVariable.v = this.gameObject;
    }
}
