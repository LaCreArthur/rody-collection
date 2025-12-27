using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.Managers.Audio_Manager
{
    [System.Serializable]
    public class Sound
    {
        public string name;

        public List<AudioClip> clips;

        [FoldoutGroup("Options")] [Range(0f, 1f)]
        public float volume = 1f;

        [FoldoutGroup("Options")] [Range(0f, 1f)]
        public float volumeVariance;

        [FoldoutGroup("Options")] [Range(.1f, 3f)]
        public float pitch = 1f;

        [FoldoutGroup("Options")] [Range(0f, 1f)]
        public float pitchVariance;

        [FoldoutGroup("Options")] public bool loop;

        [HideInInspector] public AudioSource source;
    }
}