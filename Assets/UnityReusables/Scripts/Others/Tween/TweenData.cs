using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TweenData
{
    public enum TweenType {Move, Jump, Rotate, Scale, Fade }
    public enum fadeType {Image, CanvasGroup, TMP_Text }

    public TweenType tweenType;
    public Transform objectToTween;
    public Ease ease;
    
    [ShowIf("tweenType", TweenType.Move)]
    public Vector3 movePositionOffset;
    [ShowIf("tweenType", TweenType.Jump)]
    public Vector3 jumpPositionOffset;
    [ShowIf("tweenType", TweenType.Jump)]
    public float jumpPower;
    [ShowIf("tweenType", TweenType.Jump)]
    public int numJumps;
    [ShowIf("tweenType", TweenType.Rotate)]
    public Vector3 targetRotation;
    [ShowIf("tweenType", TweenType.Scale)]
    public float targetScale = 1f;
    [ShowIf("tweenType", TweenType.Fade)][Range(0,1)]
    public float targetFade;
    [ShowIf("tweenType", TweenType.Fade)]
    public fadeType whatToFade;
    
    [ShowIf("@this.tweenType == TweenType.Move || this.tweenType == TweenType.Rotate || this.tweenType == TweenType.Jump")]
    public bool isLocal;
    
    public float duration;
    public bool join = false;

    public Tween GetTween()
    {
        Tween tween = null;

        switch (tweenType)
        {
            case TweenType.Move:
                Vector3 moveTargetPosition = objectToTween.position + movePositionOffset;
                tween = isLocal ? objectToTween.DOLocalMove(moveTargetPosition, duration).SetEase(ease) :
                    objectToTween.DOMove(moveTargetPosition, duration).SetEase(ease);
                break;
            case TweenType.Jump:
                Vector3 jumpTargetPosition = objectToTween.position + jumpPositionOffset;
                tween = isLocal ? objectToTween.DOLocalJump(jumpTargetPosition, jumpPower, numJumps, duration).SetEase(ease) :
                objectToTween.DOJump(jumpTargetPosition, jumpPower, numJumps, duration).SetEase(ease);
                break;
            case TweenType.Rotate:
                tween = isLocal ? objectToTween.DOLocalRotate(targetRotation, duration).SetEase(ease):  
                objectToTween.DORotate(targetRotation, duration).SetEase(ease);
                break;
            case TweenType.Scale: objectToTween.DOScale(targetScale, duration).SetEase(ease);
                break;
            case TweenType.Fade:
                switch (whatToFade)
                {
                    case fadeType.Image:
                        try
                        {
                            tween = objectToTween.GetComponent<Image>().DOFade(targetFade, duration).SetEase(ease);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("No image was found on the object to tween", objectToTween);
                            Console.WriteLine(e);
                            throw;
                        }
                        break;
                    case fadeType.CanvasGroup:
                        try
                        {
                            tween = objectToTween.GetComponent<CanvasGroup>().DOFade(targetFade, duration).SetEase(ease);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("No CanvasGroup was found on the object to tween", objectToTween);
                            Console.WriteLine(e);
                            throw;
                        }
                        break;
                    case fadeType.TMP_Text:
                        try
                        {
                            tween = objectToTween.GetComponent<TMP_Text>().DOFade(targetFade, duration).SetEase(ease);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("No image was found on the object to tween", objectToTween);
                            Console.WriteLine(e);
                            throw;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            default: break;
        }

        return tween;
    }
}
