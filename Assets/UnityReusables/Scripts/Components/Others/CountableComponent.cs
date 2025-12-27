using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.Utils
{
    public class CountableComponent : MonoBehaviour
    {
        public bool countOnAwake;
        public IntVariable total;

        protected virtual void Awake()
        {
            if (countOnAwake)
                Count();
        }

        public virtual void Count() => total.Add(1);
    }
}