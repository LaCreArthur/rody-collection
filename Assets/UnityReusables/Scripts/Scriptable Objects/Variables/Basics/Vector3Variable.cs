using UnityEngine;

namespace UnityReusables.ScriptableObjects.Variables
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Basic Variable/Vector3")]
    public class Vector3Variable : BaseVariable<Vector3>
    {
        public void SetValFromTransformPos(Transform t)
        {
            SetValue(t.position);
        }
    }
}