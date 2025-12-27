using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.Managers.Audio_Manager;
using UnityReusables.Utils.Extensions;

/*
 * Call the Destroy Method to disable the destructible part, enable the remaining part and play the FX
 * Root gameobject should not be the object to destroy but an empty container
 */
namespace UnityReusables.Utils.Components
{
    public class DestructibleComponent : MonoBehaviour
    {
        public float destroyDelay;
        public GameObject destructiblePart;
        public bool isPoolable;
        public bool isParticles;
        [ShowIf("isParticles")] 
        public ParticleSystem particles;
        public bool isSound;
        [ShowIf("isSound")] 
        public string sound;
        public bool isRemaining;
        [ShowIf("isRemaining")] 
        public float remainingDelay;
        [ShowIf("isRemaining")] 
        public GameObject remainingPart;

        public void Destruct()
        {
            DOVirtual.DelayedCall(destroyDelay, () =>
            {
                if (isPoolable)
                    PrefabPoolingSystem.Despawn(destructiblePart);
                else 
                    Destroy(destructiblePart);
            });
            
            if (isParticles)
                particles.Play();
            if (isRemaining) 
                StartCoroutine(remainingPart.SetActive(true, remainingDelay));
            if (isSound) 
                AudioManager.instance.Play(sound);
        }
    }
}