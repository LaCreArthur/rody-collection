using UnityEngine;

namespace UnityReusables.Utils.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class RBExplosionComponent : MonoBehaviour
    {
        public float endExplosionForce;
        public float endExplosionRadius;

        public LayerMask impactedLayers;
        private RaycastHit[] _hits = new RaycastHit[64];

        public void Explode()
        {
            // Cast a sphere to apply an explosive force to everything inside its radius
            var size = Physics.SphereCastNonAlloc(transform.position, endExplosionRadius, Vector3.up, _hits, 10,
                impactedLayers);
            foreach (var hit in _hits)
            {
                if (hit.rigidbody != null)
                    hit.rigidbody.AddExplosionForce(endExplosionForce, transform.position, endExplosionRadius);
            }
        }
    }
}