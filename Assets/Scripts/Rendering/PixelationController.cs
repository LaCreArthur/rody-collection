using UnityEngine;

/// <summary>
/// Simple controller to enable pixelation at a fixed resolution.
/// Add to any scene that needs retro pixel rendering (e.g., DOOM).
/// </summary>
public class PixelationController : MonoBehaviour
{
    [Header("Pixelation Settings")]
    [Tooltip("Number of horizontal pixels (e.g., 320 for Atari ST)")]
    [SerializeField] int horizontalPixels = 320;

    [Tooltip("Enable pixelation on scene start")]
    [SerializeField] bool enableOnStart = true;

    void Start()
    {
        if (enableOnStart)
            EnablePixelation();
    }

    void OnDestroy()
    {
        DisablePixelation();
    }

    public void EnablePixelation()
    {
        var feature = PixelationRendererFeature.Instance;
        if (feature != null)
        {
            feature.SetBlockCount(horizontalPixels);
            feature.SetEnabled(true);
        }
        else
        {
            Debug.LogWarning("PixelationController: PixelationRendererFeature not found in renderer");
        }
    }

    public void DisablePixelation()
    {
        var feature = PixelationRendererFeature.Instance;
        if (feature != null)
            feature.SetEnabled(false);
    }

    public void SetHorizontalPixels(int pixels)
    {
        horizontalPixels = pixels;
        var feature = PixelationRendererFeature.Instance;
        if (feature != null && feature.IsEnabled)
            feature.SetBlockCount(pixels);
    }
}
