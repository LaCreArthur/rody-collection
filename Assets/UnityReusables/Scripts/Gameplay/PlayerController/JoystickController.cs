using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.PlayerController
{
    public class JoystickController : TouchControl
    {
        public float sensibility;
        public bool isStatic;
        [HideIf("isStatic")]
        public float followSpeed;
        public bool useCustomCamera;
        [ShowIf("useCustomCamera")]
        public Camera cam;
        public Vector2Variable direction;
        public Vector2Variable currentPos; // starting point of the joystick
        public BoolVariable isTouching;
        
        private Vector2 _currPos; // current point of the joystick

        
        private readonly float
            _screenRatio = (float) Screen.height / Screen.width; // used to correct the force apply in Y direction

        private void Start()
        {
            if (cam == null)
                cam = Camera.main;
        }

        protected override void OnTouchDown()
        {
            isTouching.v = true;
            _currPos = Vector2.zero;
            currentPos.v = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        protected override void OnTouchHold()
        {
            //_isTouching = true;
            _currPos = cam.ScreenToViewportPoint(Input.mousePosition);
            Vector2 newDirection = _currPos - currentPos.v;
            
            // convert viewportPoint.y to dir.z (must swap sign)
            newDirection.y = newDirection.y * _screenRatio;
            
            // since player moves on a 2D plane, dont apply y forces
            newDirection = newDirection * sensibility;
            if (!isStatic && newDirection.sqrMagnitude > 0.45f)
            {
                currentPos.v = Vector2.Lerp(currentPos.v, currentPos.v + newDirection, Time.deltaTime * followSpeed);
            }
            direction.v = Vector2.ClampMagnitude(newDirection * sensibility, 1);
        }

        protected override void OnTouchUp()
        {
            isTouching.v = false;
            _currPos = Vector2.zero;
            direction.v = Vector3.zero;
        }
    }
}