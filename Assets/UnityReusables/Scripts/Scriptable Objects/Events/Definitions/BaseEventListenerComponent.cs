using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.ScriptableObjects.Events
{
    public interface IBaseEventListener
    {
    }

    public abstract class BaseEventListenerComponent<TGameEvent> : SerializedMonoBehaviour, IBaseEventListener
        where TGameEvent : IBaseEvent
    {
        [SerializeField] 
        protected List<TGameEvent> gameEvents;

        private void OnEnable()
        {
            if (gameEvents == null)
                gameEvents = new List<TGameEvent>();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        protected void Subscribe()
        {
            foreach (var gameEvent in gameEvents)
                gameEvent.AddListener(this);
        }

        protected void Unsubscribe()
        {
            foreach (var gameEvent in gameEvents)
                gameEvent.RemoveListener(this);
        }

        public void AddGameEvent(TGameEvent gameEvent)
        {
            gameEvents.Add(gameEvent);
            gameEvent.AddListener(this);
        }

        public void RemoveGameEvent(TGameEvent gameEvent)
        {
            gameEvents.Remove(gameEvent);
            gameEvent.RemoveListener(this);
        }

        public List<TGameEvent> GetEvents()
        {
            return gameEvents;
        }
    }
}