using System;
using UnityEngine;

namespace UnityReusables.Managers
{
    /// <summary>
    /// To put on level prefabs
    /// </summary>
    [Serializable]
    public class LevelComponent : MonoBehaviour
    {
        [SerializeField] private BetterEvent onLevelIn, onLevelOut = default;
        
        public void OnLevelIn()
        {
            if (onLevelIn.Events.Count > 0)
                onLevelIn.Invoke();
        }

        public void OnLevelOut()
        {
            if (onLevelOut.Events.Count > 0)
                onLevelOut.Invoke();
        }
    }
}
