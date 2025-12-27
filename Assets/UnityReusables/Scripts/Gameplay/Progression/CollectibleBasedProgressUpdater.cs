using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

// Update a FloatVariable stepProgress based on collectibles to pick up
namespace UnityReusables.Progression
{
    public class CollectibleBasedProgressUpdater : MonoBehaviour
    {
        // The current count of remaining collectibles
        public IntVariable collectiblesCurrent;

        public IntVariable collectiblesTotal;

        // stepProgress between 0 and 1
        public FloatVariable stepProgress;

        protected virtual void Start()
        {
            stepProgress.v = 0;
            collectiblesCurrent.AddOnChangeCallback(OnCollect);
        }

        protected virtual void OnCollect()
        {
            stepProgress.v = 1f - (float) collectiblesCurrent.v / collectiblesTotal.v;
            //print(stepProgress.Value);
        }

        protected void OnDestroy()
        {
            collectiblesCurrent.RemoveOnChangeCallback(OnCollect);
        }
    }
}