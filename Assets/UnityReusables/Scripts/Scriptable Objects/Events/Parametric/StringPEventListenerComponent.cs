using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.ScriptableObjects.Events
{
    public class StringPEventListenerComponent : BasePEventListenerComponent<string>
    {
        [OnValueChanged("OnEventChange")] [SerializeField]
        protected List<StringPEventSO> castedGameEvents;

        private void OnEventChange()
        {
            GameEvents = castedGameEvents;
        }

        private List<StringPEventSO> GameEvents
        {
            get => base.gameEvents.ConvertAll(x => (StringPEventSO) x);
            set => base.gameEvents = new List<BasePEventSO<string>>(value);
        }
    }
}