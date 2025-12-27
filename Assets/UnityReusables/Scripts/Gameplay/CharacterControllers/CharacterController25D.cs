using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.CharacterControllers
{
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterController25D : BaseCharacterController
    {
        [Header("2.5D Specifics")] [ShowIf("canCrouch")]
        public Collider crouchDisableCollider; // A collider that will be disabled when crouching

        public bool moveOnX = true;

        [Space] public DOTweenAnimation arrowAnim;

        private Rigidbody _rb;
        private Collider[] _ceilingCheckColliders = new Collider[36];
        private Vector3 _startRotEuler;
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _startRotEuler = transform.rotation.eulerAngles;
        }

        protected override void SetMoveVelocity(float move)
        {
            // Move the character by finding the target velocity
            float newVel = move * runSpeed * 10f * Time.fixedDeltaTime;
            var rbV = _rb.velocity;
            Vector3 targetVelocity = new Vector3(moveOnX ? newVel : 0, rbV.y, moveOnX ? 0 : newVel);
            if (targetVelocity.y < 0)
            {
                targetVelocity += Vector3.up * (Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime);
            }

            // And then smoothing it out and applying it to the character
            _rb.velocity = Vector3.SmoothDamp(rbV, targetVelocity, ref velocity, movementSmoothing);
        }

        protected override void SetJumpVelocity()
        {
            _rb.AddForce(new Vector2(0f, jumpForce));
        }

        protected override bool CeilingCheck()
        {
            int size = Physics.OverlapSphereNonAlloc(ceilingCheck.position, CEILING_RADIUS, _ceilingCheckColliders,
                whatIsCeiling);
            if (size > 0)
            {
                return true;
            }

            return false;
        }

        protected override void Flip()
        {
            // Switch the way the player is labelled as facing.
            facingRight = !facingRight;
            transform.rotation = Quaternion.Euler(_startRotEuler.x, _startRotEuler.y + (facingRight ? 0 : 180),
                _startRotEuler.z);
        }

        public void ChangeRunSpeed(float runSpeed)
        {
            this.runSpeed = runSpeed;
        }

        public void SetFreeze(bool isFrozen)
        {
            if (!isFrozen) arrowAnim.DORestart();
            _rb.velocity = Vector3.zero;
            this.isFrozen = isFrozen;
        }
    }
}