using System;
using UnityEngine;

namespace UnityReusables.ScriptableObjects.Variables
{
    public class BoolVariableListener : MonoBehaviour
    {
        public bool logOnChange;
        public bool callOnStart;
        public BoolVariable variable;
        public BetterEvent onTrue;
        public BetterEvent onFalse;

        private void OnEnable() => variable.AddOnChangeCallback(OnChange);
        private void OnDisable() => variable.RemoveOnChangeCallback(OnChange);

        void Start() { if (callOnStart) OnChange(); }

        private void OnChange()
        {
            BetterEvent events = variable.v ? onTrue : onFalse;
            if (logOnChange) Debug.Log(variable.v ? "onTrue Called" : "onFalse called");
            events.Invoke();
        }
    }
}