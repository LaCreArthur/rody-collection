using Sirenix.Utilities;
using UnityEngine;
using UnityReusables.Utils.LayerTagDropdown;

namespace UnityReusables.Utils
{
    [RequireComponent(typeof(Collider))]
    public class ActivateChildPhysic : MonoBehaviour
    {
        [TagDropdown] public string tagToTrigger;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(tagToTrigger))
            {
                transform.GetComponentsInChildren<Rigidbody>().ForEach(rb => rb.isKinematic = false);
            }
        }
    }
}