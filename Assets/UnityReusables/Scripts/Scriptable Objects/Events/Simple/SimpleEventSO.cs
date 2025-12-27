using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.ScriptableObjects.Events
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Simple Event")]
    public class SimpleEventSO : BaseEventSO<SimpleEventListenerComponent>
    {
        [Button]
        public void Raise()
        {
            Log();
            foreach (var listener in listeners)
                if (listener != null)
                {
                    listener.Invoke();
                }
                else
                {
                    Debug.LogWarning($"Event {name} had a null listener that has been removed", this);
                    listeners.Remove(listener);
                }
        }
    }
}