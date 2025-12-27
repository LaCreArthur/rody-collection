using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.Utils.Extensions;

public class RandomScaleComponent : MonoBehaviour
{
    public bool uniform;
    [ShowIf("uniform")]
    public Vector2 scaleRange;
    [HideIf("uniform")]
    public Vector2 xRange, yRange, zRange;
    public bool onStart;

    void Start()
    {
        if (!onStart) return;
        SetRandomScale();
    }

    public void SetRandomScale()
    {
        transform.localScale = uniform ? 
                Vector3.one * scaleRange.RandomInside() : 
                new Vector3(xRange.RandomInside(), yRange.RandomInside(), zRange.RandomInside());
    }
}