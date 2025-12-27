using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialOffsetAnimation : MonoBehaviour
{
    public Vector2 offsetAnimationTarget; 
    public float offsetAnimationDuration; 
    public Ease offsetAnimationEase;  
    public int offsetAnimationLoop; 
    
    private Renderer _renderer;
    private Material _material;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _material = _renderer.material;
        AnimateOffset();
    }

    public void AnimateOffset()
    {
        _material.DOOffset(offsetAnimationTarget, offsetAnimationDuration).SetEase(offsetAnimationEase).SetLoops(offsetAnimationLoop);
    }
}
