using UnityEngine;
using UnityReusables.Utils.Extensions;

/*
 * Attach to a rigidbody to move with whatIsMovingMy  
 */
namespace UnityReusables.Utils
{
    public class RBMovableByPlatform : MonoBehaviour
    {
        public LayerMask whatIsMovingMe;
        private Transform _trueParent;
        private int _trueSiblingIndex;

        private void Awake()
        {
            _trueParent = transform.parent;
            _trueSiblingIndex = transform.GetSiblingIndex();
        }

        private void OnCollisionEnter(Collision other)
        {
            if (whatIsMovingMe.MatchWith(other.gameObject.layer))
            {
                transform.parent = other.transform;
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (whatIsMovingMe.MatchWith(other.gameObject.layer))
            {
                transform.parent = _trueParent;
                transform.SetSiblingIndex(_trueSiblingIndex);
            }
        }
    }
}