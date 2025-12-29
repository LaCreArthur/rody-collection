using UnityEngine;
using UnityEngine.Events;

namespace DOOM.FPS
{
    /// <summary>
    ///     Simple trigger enter handler with auto-discovered actions.
    ///     Replaces lost BetterTriggerEnter component.
    ///     Use Cases:
    ///     - ZamblaAllongé: action=PlayAudio, triggerOnce=false
    ///     - Pickup_Shotgun: action=EnableTarget, target=AfterShotgunPickedUp, triggerOnce=true
    ///     Auto-discovers AudioSource on same object for PlayAudio action.
    ///     For EnableTarget, assign the target GameObject in Inspector.
    ///     For Custom action, use the onTriggerEnter UnityEvent.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SimpleTriggerAction : MonoBehaviour
    {
        public enum ActionType
        {
            PlayAudio, // Plays AudioSource on this object (auto-found)
            EnableTarget, // Enables (SetActive true) the target GameObject
            Custom, // Uses onTriggerEnter UnityEvent
        }

        [Header("Trigger Settings")]
        [SerializeField] ActionType action = ActionType.PlayAudio;
        [SerializeField] string targetTag = "Player";
        [SerializeField] bool triggerOnce = true;

        [Header("EnableTarget (only for ActionType.EnableTarget)")]
        [SerializeField] GameObject target;

        [Header("Custom Action (only for ActionType.Custom)")]
        [SerializeField] UnityEvent onTriggerEnter;

        // Auto-discovered references
        AudioSource _audioSource;
        bool _hasTriggered;

        void Awake()
        {
            // Auto-discover dependencies based on action type
            if (action == ActionType.PlayAudio)
            {
                _audioSource = GetComponent<AudioSource>() ?? GetComponentInChildren<AudioSource>();
                if (_audioSource == null)
                    Debug.LogWarning($"[SimpleTriggerAction] No AudioSource found on {gameObject.name}");
            }
            else if (action == ActionType.EnableTarget && target == null)
            {
                Debug.LogWarning($"[SimpleTriggerAction] No target assigned on {gameObject.name}", this);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (triggerOnce && _hasTriggered) return;
            if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag)) return;

            _hasTriggered = true;
            ExecuteAction();
        }

        void ExecuteAction()
        {
            switch (action)
            {
                case ActionType.PlayAudio:
                    _audioSource?.Play();
                    break;

                case ActionType.EnableTarget:
                    target?.SetActive(true);
                    break;

                case ActionType.Custom:
                    onTriggerEnter?.Invoke();
                    break;
            }
        }

        /// <summary>
        ///     Reset the trigger state to allow it to fire again.
        /// </summary>
        public void ResetTrigger() => _hasTriggered = false;
    }
}
