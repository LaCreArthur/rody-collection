using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace UnityReusables.ScriptableObjects.Events
{
    public class BasePEventListenerComponent<T> : BaseEventListenerComponent<BasePEventSO<T>>
    {
        [SerializeField] private BetterEvent actions;

        private BetterEvent Actions
        {
            get
            {
                if (actions.Equals(null))
                    actions = new BetterEvent();
                return actions;
            }
        }

        public void Invoke(T t)
        {
            foreach (var eventEntry in Actions.Events.Where(eventEntry => eventEntry != null))
            {
                if (eventEntry.ParameterValues.Length > 0)
                    eventEntry.ParameterValues[0] = t;
                eventEntry.Invoke();
            }
        }

        public void AddCallback(UnityAction callback)
        {
            actions.AddCallback(callback);
        }

        public void RemoveCallback(UnityAction callback)
        {
            actions.RemoveCallback(callback);
        }
    }
}