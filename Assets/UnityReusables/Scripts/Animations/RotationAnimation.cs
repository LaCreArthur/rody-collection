using UnityEngine;

namespace UnityReusables.Utils
{
    public class RotationAnimation : MonoBehaviour
    {
        [Range(-1.0f, 1.0f)] public float xDirection;
        [Range(-1.0f, 1.0f)] public float yDirection;
        [Range(-1.0f, 1.0f)] public float zDirection;
        public bool randomStartRotation = true;
        public bool worldPivote;
        public float speedMultiplier = 1;

        private Space spacePivot = Space.Self;

        void Start()
        {
            if (worldPivote) spacePivot = Space.World;
            if (!randomStartRotation) return;
            
            transform.Rotate(xDirection * Random.Range(-180,180)
                , yDirection * Random.Range(-180,180)
                , zDirection * Random.Range(-180,180)
                , spacePivot);
        }

        void Update()
        {
            transform.Rotate(xDirection * speedMultiplier
                , yDirection * speedMultiplier
                , zDirection * speedMultiplier
                , spacePivot);
        }
    }
}