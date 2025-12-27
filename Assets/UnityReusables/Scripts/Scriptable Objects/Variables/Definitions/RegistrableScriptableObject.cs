using System;
using Sirenix.OdinInspector;

namespace UnityReusables.ScriptableObjects.Variables
{
    public class RegistrableScriptableObject : SerializedScriptableObject
    {
        private Action m_onChange;

        protected virtual void OnEnable() {}

        protected void TriggerChange()
        {
            m_onChange?.Invoke();
        }

        public void AddOnChangeCallback(Action callback)
        {
            m_onChange += callback;
        }

        public void RemoveOnChangeCallback(Action callback)
        {
            m_onChange -= callback;
        }
    }
}