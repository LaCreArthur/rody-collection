using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.ScriptableObjects.Events
{
    public class BoolPEventListenerComponent : BasePEventListenerComponent<bool>
    {
        [OnValueChanged("OnEventChange")] [SerializeField]
        [ListDrawerSettings(ShowFoldout = true)]
        protected List<BoolPEventSO> castedGameEvents;

        private void OnEventChange()
        {
            GameEvents = castedGameEvents;
        }

        private List<BoolPEventSO> GameEvents
        {
            get => base.gameEvents.ConvertAll(x => (BoolPEventSO) x);
            set => base.gameEvents = new List<BasePEventSO<bool>>(value);
        }
    }
}