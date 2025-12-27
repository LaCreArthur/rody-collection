using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.Utils.UI
{
    public class GOBoolVariableEnabler : MonoBehaviour
    {
        public GameObject target;
        public BoolVariable isEnableVariable;

        private void SetActive() => target.SetActive(isEnableVariable.v);
        private void OnEnable() => isEnableVariable.AddOnChangeCallback(SetActive);
        private void OnDisable() => isEnableVariable.RemoveOnChangeCallback(SetActive);
    }
}