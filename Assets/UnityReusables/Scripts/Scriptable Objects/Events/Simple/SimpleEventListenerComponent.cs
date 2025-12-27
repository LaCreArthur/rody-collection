using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityReusables.ScriptableObjects.Variables;
using UnityReusables.Utils.Extensions;

namespace UnityReusables.ScriptableObjects.Events
{
    public class SimpleEventListenerComponent : BaseEventListenerComponent<SimpleEventSO>
    {
        [SerializeField] private BetterEvent actions;
        [SerializeField] private bool addExtraDelay;
        
        [ShowIf("addExtraDelay")]
        [SerializeField] private bool useFloatVariable;
        
        [ShowIf("@addExtraDelay&&useFloatVariable")]
        [SerializeField] private FloatVariable extraDelayVar;
        
        [ShowIf("@addExtraDelay&&useFloatVariable")]
        [SerializeField] private float extraOffset;
        
        [ShowIf("@addExtraDelay&&!useFloatVariable")]
        [SerializeField] private bool useRandomRange;
        
        [ShowIf("@addExtraDelay&&!useFloatVariable&&!useRandomRange")][Min(0f)]
        [SerializeField] private float extraDelay;
        
        [ShowIf("@addExtraDelay&&!useFloatVariable&&useRandomRange")][MinMaxSlider(0,10,true)]
        [SerializeField] private Vector2 randomRangeDelay;
        
        private BetterEvent Actions
        {
            get
            {
                if (actions.Equals(null))
                    actions = new BetterEvent();
                return actions;
            }
        }

        public void Invoke()
        {
            if (addExtraDelay)
                StartCoroutine(DelayedInvoke());
            else 
                foreach (var eventEntry in Actions.Events.Where(eventEntry => eventEntry != null)) eventEntry.Invoke();
        }

        IEnumerator DelayedInvoke()
        {
            float delay = useFloatVariable ? (extraDelayVar.v + extraOffset) : 
                            useRandomRange ? randomRangeDelay.RandomInside() : extraDelay;
            
            // safety for some extraDelayVar & extraOffset cases 
            if (delay < 0) delay = 0;
            
            yield return new WaitForSeconds(delay);
            foreach (var eventEntry in Actions.Events.Where(eventEntry => eventEntry != null)) eventEntry.Invoke();
        }

        public void AddCallback(UnityAction callback)
        {
            actions.AddCallback(callback);
        }

        public void RemoveCallback(UnityAction callback)
        {
            actions.RemoveCallback(callback);
        }
    }
}