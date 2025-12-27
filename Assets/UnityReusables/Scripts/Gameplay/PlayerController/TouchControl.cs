using System;
using UnityEngine;

namespace UnityReusables.PlayerController
{
    public abstract class TouchControl : MonoBehaviour
    {
        public bool touchDisabled;
        protected event EventHandler<bool> TouchChange;
        public bool IsTouching => isTouchingInternal;

        bool isTouchingInternal;
        
        public void EnableTouch(bool value)
        {
            touchDisabled = !value;
            OnTouchChange(value);
        }

        protected virtual void OnTouchDown(){}
        protected virtual void OnTouchHold(){}
        protected virtual void OnTouchUp(){}

        protected virtual void Update()
        {
            if (touchDisabled) return;
            if (Input.GetMouseButtonDown(0))
            {
                isTouchingInternal = true;
                OnTouchDown();
            }
            else if (Input.GetMouseButton(0))
                OnTouchHold();
            else if (Input.GetMouseButtonUp(0))
            {
                OnTouchUp();
                isTouchingInternal = false;
            }
        }

        protected virtual void OnTouchChange(bool v) => TouchChange?.Invoke(this, v);
    }
}