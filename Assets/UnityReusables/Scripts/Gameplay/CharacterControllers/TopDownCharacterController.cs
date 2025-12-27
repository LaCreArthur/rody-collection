using UnityEngine;
//using UnityEngine.InputSystem;
using UnityReusables.ScriptableObjects.Events;

namespace UnityReusables.PlayerController
{
    [RequireComponent(typeof(Rigidbody))]
    public class TopDownCharacterController : MonoBehaviour
    {
        public int playerIndex;
        public float moveSpeed;
        public float rotSpeed;
        public float moveDeadZone;

        public GameObjectPEventSO NewPlayerPEvent;
        public IntPEventSO SubmitPEvent;

        private Rigidbody _rb;
        private Vector3 _moveDirection;
        private Vector2 _moveAxisValue = default;

        private void Awake()
        {
            NewPlayerPEvent.Raise(gameObject);
        }

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity = Vector3.ClampMagnitude(_moveDirection, 1f) * moveSpeed;
        }

        // public void OnMove(InputValue value)
        // {
        //     _moveAxisValue = value.Get<Vector2>();
        //     OnMoveInternal();
        // }

        private void OnMoveInternal()
        {
            _moveDirection = Vector3.zero;
            _moveDirection.x = _moveAxisValue.x;
            _moveDirection.z = _moveAxisValue.y;

            if (_moveDirection.magnitude > moveDeadZone)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_moveDirection),
                    Time.deltaTime * rotSpeed);
        }

        // public void OnStopMoving(InputAction.CallbackContext ctx)
        // {
        //     _moveDirection = Vector3.zero;
        // }

        public void OnSubmit()
        {
            SubmitPEvent.Raise(playerIndex);
        }

        public void SetPlayerIndex(int i)
        {
            playerIndex = i;
        }
    }
}