using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.Utils
{
    public class QuickReposComponent : MonoBehaviour
    {
        public Vector3[] pos;

        public bool posOnStart;
        [ShowIf("posOnStart")]
        public int startPosIndex = 0;

        private void Start()
        {
            if (posOnStart) transform.position = pos[startPosIndex];
        }

        [Button("Set Position")]
        public void SetPos(int index)
        {
            if (index < 0 || index > pos.Length - 1) return;
            transform.position = pos[index];
        }
    }
}