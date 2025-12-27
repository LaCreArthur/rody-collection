using UnityEngine;
using UnityReusables.PlayerController;

public class TouchHoldAnimatorController : TouchControl
{
    Animator _animator;
    static readonly int Holding = Animator.StringToHash("Holding");

    void Start() => _animator = GetComponent<Animator>();
    protected override void OnTouchDown() => _animator.SetBool(Holding, true);
    protected override void OnTouchUp() => _animator.SetBool(Holding, false);
}