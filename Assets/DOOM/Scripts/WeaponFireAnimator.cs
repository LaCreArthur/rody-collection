using UnityEngine;

namespace DOOM.FPS
{
    /// <summary>
    ///     Triggers animator on weapon fire event.
    ///     Attach to UI element with Animator, wire PlayerWeaponsManager reference.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class WeaponFireAnimator : MonoBehaviour
    {
        [SerializeField] string triggerName = "Fire";
        Animator _animator;

        void Awake() => _animator = GetComponent<Animator>();

        void OnEnable() => PlayerWeaponsManager.OnFired += OnWeaponFired;

        void OnDisable() => PlayerWeaponsManager.OnFired -= OnWeaponFired;

        void OnWeaponFired()
        {
            if (_animator != null)
                _animator.SetTrigger(triggerName);
        }
    }
}
