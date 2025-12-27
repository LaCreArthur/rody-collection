using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.ScriptableObjects.Events
{
    public class IntPEventListenerComponent : BasePEventListenerComponent<int>
    {
        [OnValueChanged("OnEventChange")] [SerializeField]
        protected List<IntPEventSO> castedGameEvents;

        private void OnEventChange()
        {
            GameEvents = castedGameEvents;
        }

        private List<IntPEventSO> GameEvents
        {
            get => base.gameEvents.ConvertAll(x => (IntPEventSO) x);
            set => base.gameEvents = new List<BasePEventSO<int>>(value);
        }
    }
}