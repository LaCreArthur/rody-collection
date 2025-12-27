using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class BetterEventComponent : MonoBehaviour
{
    public BetterEvent Event;

    [DisableIf("onEnable")]
    public bool onStart;
    
    [DisableIf("onStart")]
    public bool onEnable;
    [ShowIf("onEnable")]
    public bool onlyOnce;

    [SerializeField] private bool logInvoke;
    
    private bool hasBeenInvoked;
    
    private void OnEnable()
    {
        if (onEnable)
        {
            if (!onlyOnce)
            {
                InvokeEvents();
            }
            else if (!hasBeenInvoked)
            {
                InvokeEvents();
            }
            
        }
    }

    private void Start()
    {
        if (onStart)
            InvokeEvents();
    }

    public void InvokeEvents()
    {
        if (logInvoke) Debug.Log($"BetterEvents on {gameObject.name} invoked !", this);
        Event.Invoke();
        hasBeenInvoked = true;
    }
}
