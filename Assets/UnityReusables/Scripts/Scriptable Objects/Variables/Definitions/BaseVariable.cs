using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.ScriptableObjects.Variables
{
    public class BaseVariable<T> : RegistrableScriptableObject, IStorable<T>
    {
        [Title("Values")]
        [SerializeField] protected T initialValue;
        [SerializeField] [ReadOnly] protected T previousValue;
        [SerializeField] [ReadOnly] protected T value;
        [Title("Changes")]
        [SerializeField] protected bool isConstant;
        [HideIf("isConstant")]
        [SerializeField] protected bool debugChange;
        [HideIf("isConstant")]
        [SerializeField] private bool isPlayerPref = false;
        [ShowIf("isPlayerPref")]
        [SerializeField]
        protected T defaultValue;
        
        public T InitialValue
        {
            get => initialValue;
            set => initialValue = value;
        }
        
        public T v
        {
            get => value;
            set
            {
                // if (Application.isPlaying)
                // {
                //     if (previousValue.Equals(value)) return;
                if (isConstant)
                {
                    if (debugChange) Debug.Log($"{name} is constant and cannot be modified", this);
                    return;
                }
                // }
                    
                previousValue = this.value;
                this.value = value;
                
                if (Application.isPlaying)
                {    
                    if (debugChange) Debug.Log($"{name} is set to : {value}", this);
                    if (isPlayerPref) Save();

                    TriggerChange();
                }
            }
        }

        public T PreviousValue => previousValue;

        protected override void OnEnable()
        {
            v = isPlayerPref ? Load() : initialValue;
        }

        [Button]
        public virtual void SetValue(T newVal)
        {
            v = newVal;
        }

        public virtual void Save()
        {
        }

        public virtual T Load()
        {
            T t = default(T);
            return t;
        }
    }
}