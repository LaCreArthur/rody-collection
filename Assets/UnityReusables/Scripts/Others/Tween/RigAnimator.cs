#if BURST_ENABLED
using DG.Tweening;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(Rig))]
public class RigAnimator : MonoBehaviour
{
    Rig _rig;
    
    void Awake() => _rig = GetComponent<Rig>();

    public void AnimateWeight(float from, float to, float duration, Ease ease = Ease.Linear)
    {
        DOVirtual.Float(from, to, duration, (v) => _rig.weight = v).SetEase(ease);
    }
}
#endif