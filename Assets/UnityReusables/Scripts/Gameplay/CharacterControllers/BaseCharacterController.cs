using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.ScriptableObjects.Events;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.CharacterControllers
{
    public class BaseCharacterController : MonoBehaviour
    {
        [Header("Jump")] public bool canJump;
        [ShowIf("canJump")] public float jumpForce = 400f; // Amount of force added when the player jumps.
        [ShowIf("canJump")] public float fallMultiplier = 50f; // make the player fall faster.
        [ShowIf("canJump")] public bool airControl = true; // Whether or not a player can steer while jumping;
        [Header("Crouch")] public bool canCrouch;

        [ShowIf("canCrouch")] [Range(0, 1)]
        public float crouchSpeed = .36f; // Amount of maxSpeed applied to crouching movement. 1 = 100%

        [ShowIf("canCrouch")] public Transform ceilingCheck; // A position marking where to check for ceilings
        [ShowIf("canCrouch")] public LayerMask whatIsCeiling; // A collider that will be disabled when crouching

        [Header("Movement")] [Range(0, .3f)]
        public float movementSmoothing = .05f; // How much to smooth out the movement

        public float runSpeed = 6.5f;

        [Header("Events")] public SimpleEventSO jumpEvent;
        public SimpleEventSO deathEvent;
        public BoolVariable isGrounded;
        public BoolVariable isCrouched;

        protected bool IsGrounded
        {
            get => isGrounded.v;
            set => isGrounded.v = value;
        }

        protected bool IsCrouched
        {
            get => isCrouched.v;
            set => isCrouched.v = value;
        }

        protected bool isFrozen;

        protected const float
            CEILING_RADIUS = .2f; // Radius of the overlap circle to determine if the player can stand up

        protected const float TOLERANCE = 0.001f;
        protected bool facingRight = true; // For determining which way the player is currently facing.
        protected Vector3 velocity = Vector3.zero;

        public virtual void Move(float move, bool crouch, bool jump)
        {
            if (isFrozen) return;
            crouch = crouch && canCrouch;
            jump = jump && canJump;

            // If crouching, check to see if the character can stand up
            if (IsCrouched && !crouch)
            {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                crouch &= CeilingCheck();
            }

            //only control the player if grounded or airControl is turned on
            if (isGrounded || airControl)
            {
                // If crouching
                if (crouch)
                {
                    if (!IsCrouched)
                    {
                        IsCrouched = true;
                    }

                    // Reduce the speed by the crouchSpeed multiplier
                    move *= crouchSpeed;

                    // Disable one of the colliders when crouching
                    SetCrouchCollider(false);
                }
                else
                {
                    SetCrouchCollider(true);

                    if (IsCrouched)
                    {
                        IsCrouched = false;
                    }
                }

                SetMoveVelocity(move);

                // If the input is moving the player right and the player is facing left...
                if (move > 0 && !facingRight)
                {
                    Flip();
                }
                // Otherwise if the input is moving the player left and the player is facing right...
                else if (move < 0 && facingRight)
                {
                    Flip();
                }
            }

            // If the player should jump...
            if (IsGrounded && jump)
            {
                // Add a vertical force to the player.
                IsGrounded = false;
                jumpEvent.Raise();
                SetJumpVelocity();
            }
        }

        protected virtual bool CeilingCheck()
        {
            return false;
        }

        protected virtual void SetCrouchCollider(bool val)
        {
        }

        protected virtual void SetMoveVelocity(float move)
        {
        }

        protected virtual void SetJumpVelocity()
        {
        }

        protected virtual void Flip()
        {
        }
    }
}