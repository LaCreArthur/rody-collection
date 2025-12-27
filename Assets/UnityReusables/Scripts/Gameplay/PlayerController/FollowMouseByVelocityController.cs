using UnityEngine;

namespace UnityReusables.PlayerController
{
    [RequireComponent(typeof(Rigidbody))]
    public class FollowMouseByVelocityController : TouchControl
    {
        public float velocityFactor = 10.0f;
        public float lerpFactor = 0.2f;
        public float distanceFactor = 5.0f;

        Camera _cam;
        Rigidbody _rb;
        Plane _objPlane;
        Vector3 _mouseOffset;
        Vector3 _destPos;
        bool _isTouching;

        protected void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _destPos = transform.position;
            _cam = Camera.main;
        }

        protected override void OnTouchDown()
        {
            var position = transform.position;
            _objPlane = new Plane(Vector3.up, position);

            //calc mouse offset
            var mRay = _cam.ScreenPointToRay(Input.mousePosition);
            _objPlane.Raycast(mRay, out var rayDist);
            _mouseOffset = position - mRay.GetPoint(rayDist) * distanceFactor;
        }

        protected override void OnTouchHold()
        {
            var mRay = _cam.ScreenPointToRay(Input.mousePosition);
            if (_objPlane.Raycast(mRay, out var rayDist))
            {
                _isTouching = true;
                _destPos = mRay.GetPoint(rayDist) * distanceFactor + _mouseOffset;
                var t = Mathf.Sin(Time.deltaTime * Mathf.PI * 0.5f) * lerpFactor; // ease out
                _mouseOffset = Vector3.Lerp(_mouseOffset, transform.position - mRay.GetPoint(rayDist) * distanceFactor,
                    t);
            }
        }

        protected override void OnTouchUp()
        {
            _isTouching = false;
        }

        void FixedUpdate()
        {
            if (!_isTouching) return;
            var position = transform.position;
            var diff = _destPos - position; // gives the direction the player needs to go
            diff.y = _rb.velocity.y / velocityFactor;
            //rb.velocity = velocityFactor * diff; // clamp the speed
            _rb.velocity = velocityFactor * diff;
            //m_destPos.y = position.y;

            transform.LookAt(_destPos);
        }


        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_destPos, 0.3f);
        }
    }
}