using UnityEngine;

/*
 * Check if the GameObject collides with a designated layer
 * ie : check within _groundedRadius from _GroundCheck if collision with _GroundLayer
 */
namespace UnityReusables.CharacterControllers
{
    public class GroundCheck3D : GroundCheckBase
    {
        private Collider[] _colliders = new Collider[36];

        protected override bool IsNotItself(int i)
        {
            return _colliders[i].gameObject != gameObject;
        }

        protected override int GetOverlapCollidersSize()
        {
            return Physics.OverlapSphereNonAlloc(_groundCheck.position, _groundedRadius, _colliders, _whatIsGround);
        }
    }
}