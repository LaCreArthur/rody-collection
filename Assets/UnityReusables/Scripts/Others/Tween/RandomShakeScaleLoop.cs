using DG.Tweening;
using UnityEngine;
using static RandomShakeType;

public enum RandomShakeType {Position, Rotation, Scale}

public class RandomShakeScaleLoop : MonoBehaviour
{
    public RandomShakeType shakeType;
    public Vector3 strength;
    public Vector3 scale;
    public int vibrato;
    public float duration, randomness;
    public Ease ease;
    public bool onStart;
    public bool isActive;

    void Start()
    {
        if (onStart) StartLoop();
    }

    public void SetIsActive(bool loop)
    {
        isActive = loop;
        if (loop) StartLoop();
    }

    public void StartLoop()
    {
        isActive = true;
        transform.DOScale(scale, 0.5f).SetEase(Ease.OutBounce).OnComplete(Loop);
    }

    public void StopLoop()
    {
        isActive = false;
    }

    public void Loop()
    {
        if (!isActive)
        {
            transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBounce);
            return;
        }
        switch (shakeType)
        {
            case Position:
                transform.DOShakePosition(duration, strength, vibrato, randomness).SetEase(ease).OnComplete(Loop);
                break;
            case Rotation:
                transform.DOShakeRotation(duration, strength, vibrato, randomness).SetEase(ease).OnComplete(Loop);
                break;
            case Scale:
                transform.DOShakeScale(duration, strength, vibrato, randomness).SetEase(ease).OnComplete(Loop);
                break;
        }
    }

}
