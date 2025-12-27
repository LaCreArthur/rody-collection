using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.ScriptableObjects.Events
{
    public class GameObjectPEventListenerComponent : BasePEventListenerComponent<GameObject>
    {
        [OnValueChanged("OnEventChange")] [SerializeField]
        protected List<GameObjectPEventSO> castedGameEvents;

        private void OnEventChange()
        {
            GameEvents = castedGameEvents;
        }

        private List<GameObjectPEventSO> GameEvents
        {
            get => base.gameEvents.ConvertAll(x => (GameObjectPEventSO) x);
            set => base.gameEvents = new List<BasePEventSO<GameObject>>(value);
        }
    }
}