using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.ScriptableObjects.Events
{
    public class Vector3PEventListenerComponent : BasePEventListenerComponent<Vector3>
    {
        [OnValueChanged("OnEventChange")] [SerializeField]
        [ListDrawerSettings(ShowFoldout = true)]
        protected List<Vector3PEventSO> castedGameEvents;

        private void OnEventChange()
        {
            GameEvents = castedGameEvents;
        }

        private List<Vector3PEventSO> GameEvents
        {
            get => base.gameEvents.ConvertAll(x => (Vector3PEventSO) x);
            set => base.gameEvents = new List<BasePEventSO<Vector3>>(value);
        }
    }
}