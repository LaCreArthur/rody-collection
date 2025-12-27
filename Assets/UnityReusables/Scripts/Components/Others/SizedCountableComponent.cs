using System;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.Utils
{
    public enum Size
    {
        Tiny,
        Small,
        Medium,
        Large,
        Extra
    }

    public class SizedCountableComponent : CountableComponent
    {
        public Size size;

        // track count of each size with this IntArrayVariable
        public IntArrayVariable sizes;

        private static bool m_isInit;

        protected override void Awake()
        {
            // check if collectible sizes array needs to be initialized
            if (!m_isInit)
            {
                // init to the length of Size elements count
                sizes.v = new int[Enum.GetNames(typeof(Size)).Length];
                m_isInit = true;
            }

            if (!countOnAwake) return;
            base.Count();
        }

        public override void Count()
        {
            base.Count();
            sizes.v[(int) size]++;
            sizes.v = sizes.v;
        }
    }
}