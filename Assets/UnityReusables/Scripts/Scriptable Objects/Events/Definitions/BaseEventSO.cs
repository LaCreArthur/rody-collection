using System.Collections.Generic;
using UnityEngine;

namespace UnityReusables.ScriptableObjects.Events
{
    public interface IBaseEvent
    {
        void AddListener(object listener);
        void RemoveListener(object listener);
    }

    public abstract class BaseEventSO<TEventListener> : ScriptableObject, IBaseEvent
    {
        public bool debugListener;

        protected List<TEventListener> listeners = new List<TEventListener>();

        [SerializeField] private bool logOnRaise = false;

        protected string LogMessage => $"[!] Game Event '{name}' ";

        public virtual void AddListener(object listener)
        {
            if (debugListener) Debug.Log($"{name} listener : {(Object) listener}", (Object) listener);
            var castedListener = (TEventListener) listener;

            if (!listeners.Contains(castedListener)) listeners.Add(castedListener);
        }

        public virtual void RemoveListener(object listener)
        {
            var castedListener = (TEventListener) listener;

            if (listeners.Contains(castedListener))
                listeners.Remove(castedListener);
        }

        protected void Log(object value = null)
        {
            if (logOnRaise)
                Debug.Log($"{LogMessage} {(value == null ? "" : $"| value = {value}")}");
        }
    }
}