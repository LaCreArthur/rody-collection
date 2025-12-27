using UnityEngine;
using UnityReusables.ScriptableObjects.Events;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.CharacterControllers
{
    public class GroundCheckBase : MonoBehaviour
    {
        [SerializeField] protected LayerMask _whatIsGround = default; // A mask determining what is ground to the character

        [SerializeField]
        protected Transform _groundCheck = default; // A position marking where to check if the player is grounded.

        [SerializeField] protected float _groundedRadius = .3f; // Radius of the overlap circle to determine if grounded

        private bool IsGrounded
        {
            get => isGrounded.v;
            set => isGrounded.v = value;
        }

        public BoolVariable isGrounded;
        public SimpleEventSO landEvent;

        private void FixedUpdate()
        {
            bool wasGrounded = IsGrounded;
            IsGrounded = false;

            // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
            // This can be done using layers instead but Sample Assets will not overwrite your project settings.
            int size = GetOverlapCollidersSize();
            for (int i = 0; i < size; i++)
            {
                if (IsNotItself(i))
                {
                    IsGrounded = true;
                    if (!wasGrounded)
                    {
                        landEvent.Raise();
                    }
                }
            }
        }

        protected virtual bool IsNotItself(int i)
        {
            return false;
        }

        protected virtual int GetOverlapCollidersSize()
        {
            return 0;
        }
    }
}