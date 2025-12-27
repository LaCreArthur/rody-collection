using UnityEngine;

namespace UnityReusables.PlayerController
{
    [RequireComponent(typeof(Rigidbody))]
    public class FollowMouseByForceController : TouchControl
    {
        public float forceFactor = 200.0f;
        public float maxVelocity = 10.0f;
        public float dragFactor = 1.0f;
        public bool isEnable = true;
        public Camera cam;

        private Plane _objPlane;
        private Vector3 _mouseOffset;
        private Vector3 _destPos;
        private bool _isTouching;
        private Rigidbody rb; 
        
        protected void Start()
        {
            rb = GetComponent<Rigidbody>();
            _destPos = transform.position;
        }

        protected override void OnTouchDown()
        {
            _isTouching = true;
            var position = transform.position;
            _objPlane = new Plane(Vector3.up, position);

            //calc mouse offset
            var mRay = cam.ScreenPointToRay(Input.mousePosition);
            _objPlane.Raycast(mRay, out var rayDist);
            _mouseOffset = position - mRay.GetPoint(rayDist);
        }

        protected override void OnTouchHold()
        {
            var mRay = cam.ScreenPointToRay(Input.mousePosition);
            if (_objPlane.Raycast(mRay, out var rayDist))
            {
                _destPos = mRay.GetPoint(rayDist) + _mouseOffset;
            }
        }

        protected override void OnTouchUp()
        {
            _isTouching = false;
            _destPos = transform.position;
        }

        void FixedUpdate()
        {
            if (!_isTouching || !isEnable) return;

            var position = transform.position;
            var diff = _destPos - position; // gives the direction the player needs to go
            diff.y = 0;
            rb.AddForce(forceFactor * Time.fixedDeltaTime * diff, ForceMode.Force);

            var dist = Vector3.Distance(_destPos, position);
            rb.drag = dragFactor / dist; // increase drag when distance diminish for a slow-down effect

            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity); // clamp the speed

            transform.LookAt(_destPos, Vector3.up);

            _destPos = Vector3.Lerp(_destPos, position, Time.fixedDeltaTime);
        }


        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_destPos, 0.3f);
        }


        public void SetEnable(bool value)
        {
            isEnable = value;
        }
    }
}