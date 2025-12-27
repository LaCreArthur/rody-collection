using Cinemachine;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class TargetGroupAnimator : MonoBehaviour
{
    CinemachineTargetGroup targetGroup;
    
    void Awake() => targetGroup = GetComponent<CinemachineTargetGroup>();

    public void AnimateWeight(float from, float to, float duration, int targetIndex, Ease ease = Ease.Linear)
    {
        DOVirtual.Float(from, to, duration, v => targetGroup.m_Targets[targetIndex].weight = v).SetEase(ease);
    }
}
