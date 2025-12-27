using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.Utils.Components
{
    public class FollowMouseComponent : MonoBehaviour
    {
        public Vector3 offset;
        public bool isDisabled;
        public bool isSmoothed;
        [ShowIf("isSmoothed")]
        public float smoothFollowFactor;

        private Vector3 _velocity;

        private void LateUpdate()
        {
            if (isDisabled) return;
            var tarPos = Input.mousePosition;
            if (isSmoothed)
            {
                transform.position = Vector3.SmoothDamp(transform.position, 
                    new Vector3(tarPos.x + offset.x, tarPos.y + offset.y, tarPos.z + offset.z), 
                    ref _velocity, smoothFollowFactor * Time.deltaTime);
            }
            else
                transform.position = new Vector3(tarPos.x + offset.x, tarPos.y + offset.y, tarPos.z + offset.z);
        }

        public void SetActive(bool active) => isDisabled = !active;
    }
}