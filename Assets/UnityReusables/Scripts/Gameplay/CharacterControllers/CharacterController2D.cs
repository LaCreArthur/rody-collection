using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.CharacterControllers
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterController2D : BaseCharacterController
    {
        [Header("2D Specifics")] [ShowIf("canCrouch")]
        public Collider2D crouchDisableCollider;

        private Rigidbody2D _rb2D;

        private void Awake()
        {
            _rb2D = GetComponent<Rigidbody2D>();
        }

        protected override void SetCrouchCollider(bool val)
        {
            if (crouchDisableCollider != null)
                crouchDisableCollider.enabled = val;
        }

        protected override bool CeilingCheck()
        {
            if (Physics2D.OverlapCircle(ceilingCheck.position, CEILING_RADIUS, whatIsCeiling))
            {
                return true;
            }

            return false;
        }

        protected override void SetMoveVelocity(float move)
        {
            float newVel = move * runSpeed * 10f * Time.fixedDeltaTime;
            // Move the character by finding the target velocity
            var rbV = _rb2D.linearVelocity;
            Vector3 targetVelocity = new Vector2(newVel, rbV.y);
            if (targetVelocity.y < 0)
            {
                targetVelocity += Vector3.up * (Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime);
            }

            // And then smoothing it out and applying it to the character
            _rb2D.linearVelocity = Vector3.SmoothDamp(rbV, targetVelocity, ref velocity, movementSmoothing);
        }

        protected override void SetJumpVelocity()
        {
            _rb2D.AddForce(new Vector2(0f, jumpForce));
        }

        protected override void Flip()
        {
            // Switch the way the player is labelled as facing.
            facingRight = !facingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }
}