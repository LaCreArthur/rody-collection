using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.Utils.Extensions;
using UnityReusables.Utils.LayerTagDropdown;

namespace UnityReusables.Utils.BetterColliderTrigger
{
    public enum ColliderEventType
    {
        CollisionEnter, CollisionStay, CollisionExit,
        TriggerEnter, TriggerStay, TriggerExit
    }
    
    [RequireComponent(typeof(Collider))]
    public class ColliderBetterEvents : MonoBehaviour
    {
        public ColliderEventType type;
        public bool useTag;
        [ShowIf("useTag")]
        [TagDropdown] public string otherTag = "";
        [HideIf("useTag")]
        public LayerMask otherLayer;

        public bool onlyOnce;
        [ShowIf("onlyOnce")]
        public bool destroyGO;
        [ShowIf("destroyGO")]
        public float delay = 0f;

        public bool asBeenTriggered;
        public BetterEvent events;
        
        void OnCollisionEnter(Collision other) => CheckCollisions(other, ColliderEventType.CollisionEnter);
        void OnCollisionStay(Collision other) => CheckCollisions(other, ColliderEventType.CollisionStay);
        void OnCollisionExit(Collision other) => CheckCollisions(other, ColliderEventType.CollisionExit);
        void OnTriggerEnter(Collider other) => CheckTrigger(other, ColliderEventType.TriggerEnter);
        void OnTriggerStay(Collider other) => CheckTrigger(other, ColliderEventType.TriggerStay);
        void OnTriggerExit(Collider other) => CheckTrigger(other, ColliderEventType.TriggerExit);

        void CheckCollisions(Collision other, ColliderEventType t)
        {
            if (ShouldEventsBeInvoked(other.collider, t))
                InvokeEvents();
        }

        void CheckTrigger(Collider other, ColliderEventType t)
        {
            if (ShouldEventsBeInvoked(other, t))
                InvokeEvents();
        }

        bool ShouldEventsBeInvoked(Collider other, ColliderEventType t)
        {
            Debug.Log("seveerf");
            if (type != t) return false;
            if (useTag && !other.gameObject.CompareTag(otherTag)) return false;
            if (!useTag && !otherLayer.MatchWith(other.gameObject.layer)) return false;
            Debug.Log("IOUI");
            return true;
        }

        void InvokeEvents()
        {
            if (onlyOnce && asBeenTriggered) return;

            events.Invoke();
            asBeenTriggered = true;
            if (onlyOnce)
            {
                if (destroyGO)
                    Destroy(gameObject, delay);
            }
        }
    }
}