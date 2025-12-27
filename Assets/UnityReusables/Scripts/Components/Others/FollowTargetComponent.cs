using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.Utils.Components
{
    public class FollowTargetComponent : MonoBehaviour
    {
        public bool freezeX;
        public bool freezeY;
        public bool freezeZ;
        public bool isLookingAt;
        public bool isSO;
        [ShowIf("isSO")]
        public TransformVariable targetSO;

        [HideIf("isSO")] public Transform target;
        public bool keepStartingPos;
        public Vector3 offset;

        public bool isActive = true;
        public bool rotateWith;

        public bool isSmoothed;
        [ShowIf("isSmoothed")]
        public float smoothFollowFactor;

        float m_startX;
        float m_startY;
        float m_startZ;
        bool initialized;
        Transform targetInternal;

        private Vector3 _velocity;
        private void Start()
        {
            initialized = false;
            var position = transform.position;
            m_startX = position.x;
            m_startY = position.y;
            m_startZ = position.z;

            targetInternal = isSO ? targetSO.v : target;
        }
        

        private void LateUpdate()
        {
            if (!isActive) return;
            if (targetInternal == null)
            {
                isActive = false;
                return;
            }
            var tarPos = targetInternal.position;
            if (!isSmoothed)
            {
                transform.position = new Vector3(
                    freezeX ? m_startX : tarPos.x + offset.x,
                    freezeY ? m_startY : tarPos.y + offset.y,
                    freezeZ ? m_startZ : tarPos.z + offset.z);
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, new Vector3( 
                    freezeX ? m_startX : tarPos.x + offset.x,
                    freezeY ? m_startY : tarPos.y + offset.y,
                    freezeZ ? m_startZ : tarPos.z + offset.z), ref _velocity, smoothFollowFactor * Time.deltaTime);
            }

            if (isLookingAt) 
                transform.LookAt(targetInternal);
            if (rotateWith)
                transform.rotation = targetInternal.rotation;
        }

        public void SetActive(bool active)
        {
            isActive = active;
            if (isActive && keepStartingPos && !initialized)
            {
                offset += transform.position - targetInternal.position;
                initialized = true;
            }
        }
    }
}