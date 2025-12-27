using System;
using UnityEngine;

namespace UnityReusables.Managers
{
    /// <summary>
    /// To put on root step gameObject if animated or on empty gameobject for StartPos of the Step
    /// </summary>
    [Serializable]
    public class StepComponent : MonoBehaviour
    {
        [SerializeField] private BetterEvent onStepIn, onStepOut = default;
        public Vector3 StartPos { get; private set; }
        public bool isAnimated;
        
        private void Start()
        {
            StartPos = transform.position;
            if (isAnimated) gameObject.SetActive(false);
        }

        public void OnStepIn()
        {
            if (onStepIn.Events.Count > 0)
                onStepIn.Invoke();
        }

        public void OnStepOut()
        {
            if (onStepOut.Events.Count > 0)
                onStepOut.Invoke();
        }
    }
}