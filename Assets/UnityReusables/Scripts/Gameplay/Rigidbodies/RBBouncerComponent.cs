using System.Collections;
using UnityEngine;

namespace UnityReusables.Utils.Components
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class RBBouncerComponent : MonoBehaviour
    {
        public float bounceDelay;
        public float bounceForce;
        private Rigidbody _rb;

        public LayerMask layersToBounceOn;
        private bool _isBouncing;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private IEnumerator DoBounce()
        {
            if (_isBouncing) yield break;
            _isBouncing = true;
            yield return new WaitForSeconds(bounceDelay);
            _rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
            _isBouncing = false;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (1 << other.gameObject.layer == layersToBounceOn.value)
            {
                StartCoroutine(DoBounce());
            }
        }
    }
}