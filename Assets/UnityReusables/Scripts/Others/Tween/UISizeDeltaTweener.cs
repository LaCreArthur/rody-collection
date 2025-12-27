using DG.Tweening;
using UnityEngine;

public class UISizeDeltaTweener : MonoBehaviour
{
    RectTransform _transform;

    void Start()
    {
        _transform = GetComponent<RectTransform>();
    }
    
    public void Zoom(Vector2 value, float duration, Ease ease)
    {
        _transform.DOSizeDelta(value, duration).SetEase(ease);
    }

    public void ZoomToFixed(Vector2 zoomTo, float zoomDuration, Ease ease, Vector2 fixedTarget)
    {
        _transform.DOSizeDelta(zoomTo, zoomDuration).SetEase(ease).OnComplete(() =>
        {
            _transform.sizeDelta = fixedTarget;
        });
    }
    
    public void FixedToZoom(Vector2 fixedTarget, Vector2 zoomTo, float zoomDuration, Ease ease)
    {
        _transform.sizeDelta = fixedTarget;
        _transform.DOSizeDelta(zoomTo, zoomDuration).SetEase(ease);
    }
}
