using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.Progression
{
    public class PositionGetter : MonoBehaviour
    {
        public bool is2D;
        [ShowIf("is2D")] public Vector2Variable vector2Variable;
        [HideIf("is2D")] public Vector3Variable vector3Variable;

        void Start()
        {
            if (is2D)
                vector2Variable.v = transform.position;
            else
                vector3Variable.v = transform.position;
        }
    }
}