using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.ScriptableObjects.Variables
{
    [Serializable]
    class GameStateCallbacks
    {
        public BetterEvent onEnter, onLeave;
    }

    public class GameStateListener : SerializedMonoBehaviour
    {
        [SerializeField][DictionaryDrawerSettings(KeyLabel = "Game State", ValueLabel = "Callbacks")]

        private Dictionary<GameStateSO, GameStateCallbacks> callbacks = new Dictionary<GameStateSO, GameStateCallbacks>();

        public GameStateVariable gameState;

        private void Start()
        {
            gameState.AddOnChangeCallback(OnChange);
        }

        void OnChange()
        {
            if (callbacks.ContainsKey(gameState.PreviousValue))
                callbacks[gameState.PreviousValue].onLeave.Invoke();

            if (callbacks.ContainsKey(gameState.v)) callbacks[gameState.v].onEnter.Invoke();
        }

        private void OnDestroy()
        {
            gameState.RemoveOnChangeCallback(OnChange);
        }

        public GameStateVariable GetCurrentState()
        {
            return gameState;
        }

        /// <summary>Add onEnter and/or onLeave callbacks to a GameState through code</summary>
        /// <param name="gameStateSO">The key of the dictionary to which the callbacks will be added</param>
        /// <param name="onEnter">the callbacks to add on the onEnter BetterEvent list</param>
        /// <param name="onLeave">the callbacks to add on the onLeave BetterEvent list</param>
        public void AddCallback(GameStateSO gameStateSO, BetterEvent? onEnter, BetterEvent? onLeave)
        {
            
            // create new game state callbacks if it is not present
            GameStateCallbacks gameStateCallbacks;
            callbacks.TryGetValue(gameStateSO, out gameStateCallbacks);
            
            if (gameStateCallbacks == null)
                gameStateCallbacks = new GameStateCallbacks 
                {
                    onEnter = new BetterEvent {Events = new List<BetterEventEntry>()},
                    onLeave = new BetterEvent {Events = new List<BetterEventEntry>()}
                };
            
            // add the enEnter and onLeave callbacks to the state
            if (onEnter != null)
                gameStateCallbacks.onEnter.Events.AddRange(onEnter?.Events);
            if (onLeave != null)
                gameStateCallbacks.onLeave.Events.AddRange(onLeave?.Events);
        }
    }
}