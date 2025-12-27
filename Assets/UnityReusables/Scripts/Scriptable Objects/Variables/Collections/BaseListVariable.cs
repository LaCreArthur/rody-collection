using System.Collections.Generic;

namespace UnityReusables.ScriptableObjects.Variables
{
    public class BaseListVariable<T> : BaseVariable<List<T>>
    {
        public bool clearOnEnable;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            if (v == null) v = new List<T>();
            if (clearOnEnable) Clear();
        }

        public T this[int i]
        {
            get => v[i];
            set => v[i] = value;
        }


        public void Add(T x) => v.Add(x);
        public void Clear() => v.Clear();
        public int Count => v.Count;
    }
}