using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.ScriptableObjects.Events
{
    public class TransformPEventListenerComponent : BasePEventListenerComponent<Transform>
    {
        [OnValueChanged("OnEventChange")] [SerializeField]
        [ListDrawerSettings(ShowFoldout = true)]
        protected List<TransformPEventSO> castedGameEvents;

        private void OnEventChange()
        {
            GameEvents = castedGameEvents;
        }

        private List<TransformPEventSO> GameEvents
        {
            get => base.gameEvents.ConvertAll(x => (TransformPEventSO) x);
            set => base.gameEvents = new List<BasePEventSO<Transform>>(value);
        }
    }
}