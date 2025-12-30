using UnityEngine;

/// <summary>
/// Syncs the walking state from PlayerCharacterController to an Animator.
/// Replaces the BoolVariableListener + Bool_isWalking ScriptableObject pattern.
/// </summary>
[RequireComponent(typeof(Animator))]
public class WalkAnimatorSync : MonoBehaviour
{
    static readonly int IsWalking = Animator.StringToHash("isWalking");

    Animator _animator;

    void Awake() => _animator = GetComponent<Animator>();
    void OnEnable() => PlayerCharacterController.OnWalkingChanged += SetWalking;
    void OnDisable() => PlayerCharacterController.OnWalkingChanged -= SetWalking;
    void SetWalking(bool value) => _animator.SetBool(IsWalking, value);
}
