using UnityEngine;

/*
 * Check if the GameObject collides with a designated layer
 * ie : check within _groundedRadius from _GroundCheck if collision with _GroundLayer
 */

namespace UnityReusables.CharacterControllers
{
    public class GroundCheck2D : GroundCheckBase
    {
        private Collider2D[] _colliders = new Collider2D[16];

        protected override bool IsNotItself(int i)
        {
            return _colliders[i].gameObject != gameObject;
        }

        protected override int GetOverlapCollidersSize()
        {
            var filter = new ContactFilter2D();
            filter.SetLayerMask(_whatIsGround);
            filter.useTriggers = true;
            return Physics2D.OverlapCircle(_groundCheck.position, _groundedRadius, filter, _colliders);
        }
    }
}