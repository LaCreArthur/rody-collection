using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.ScriptableObjects.Variables
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Game State Reference")]
    public class GameStateVariable : BaseVariable<GameStateSO>
    {
        [SerializeField] private bool debugStateChange;
        
        public override void SetValue(GameStateSO newVal)
        {
            if (v == newVal)
            {
                Debug.LogWarning($"Trying to switch from and to the same state {newVal.name}");
                return;
            }

            if (v.validNextStates.Contains(newVal))
            {
                v = newVal;
                if (debugStateChange) Debug.Log($"Game state switch from {PreviousValue.name} to {newVal.name}");
            }
            else
            {
                Debug.LogWarning($"Game state try to switch from {v.name} to {newVal.name} which is not possible, state not changed !");
            }
        }

        [Button]
        void SetValueForced(GameStateSO newVal) => v = newVal;
    }
}