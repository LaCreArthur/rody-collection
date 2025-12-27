using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.Utils
{
    public class LookAtComponent : MonoBehaviour
    {
        public bool lookAtMainCam;
        [HideIf("lookAtMainCam")] 
        public Transform target;
        public Vector3 rotationOffset;
        public bool isActive = true;
        public bool flatLookAt;
        [ShowIf("flatLookAt")] 
        public float flatSlerp = 4f;

        private void Start()
        {
            if (!lookAtMainCam) return;
            if (Camera.main != null)
                target = Camera.main.transform;
        }

        private void LateUpdate()
        {
            if (isActive)
            {
                if (flatLookAt)
                {
                    var flatVectorToTarget = transform.position - target.position;
                    flatVectorToTarget.y = 0;
                    var newRotation = Quaternion.LookRotation(flatVectorToTarget).eulerAngles;
                    newRotation.x = 0.0f;
                    newRotation.z = 0.0f;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(newRotation), Time.deltaTime * 4);

                }
                else
                    transform.LookAt(target, Vector3.up);
                
                transform.rotation *= Quaternion.Euler(rotationOffset);
            }
        }

        public void SetActive(bool active)
        {
            isActive = active;
        }
    }
}