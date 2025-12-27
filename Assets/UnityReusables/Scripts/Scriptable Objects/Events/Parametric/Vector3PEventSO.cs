using UnityEngine;

namespace UnityReusables.ScriptableObjects.Events
{
    [CreateAssetMenu(menuName = "Scriptable Objects/PEvents/Vector3 PEvent")]
    public class Vector3PEventSO : BasePEventSO<Vector3>
    {
        public void RaiseWithTransformPosition(Transform t)
        {
            Raise(t.position);
        }
        
        public void RaiseWithRectTransformPosition(RectTransform t)
        {
            Raise(Camera.main.ScreenToWorldPoint(t.anchoredPosition));
        }
    }
}