using UnityEngine;

/// <summary>
/// Controller for water effects in DOOM scenes.
/// Manages water surface and underwater post-processing.
/// </summary>
public class WaterController : MonoBehaviour
{
    [Header("Water Level")]
    [Tooltip("Y position of the water surface")]
    [SerializeField] float waterLevel = 0f;

    [Header("Underwater Effect")]
    [Tooltip("Enable underwater post-process when camera goes below water")]
    [SerializeField] bool enableUnderwaterEffect = true;

    [Tooltip("Color tint when underwater")]
    [SerializeField] Color underwaterTint = new Color(0.2f, 0.5f, 1f, 1f);

    [Tooltip("Tint strength (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] float tintStrength = 0.4f;

    [Tooltip("Wave distortion strength")]
    [Range(0f, 0.05f)]
    [SerializeField] float distortionStrength = 0.015f;

    [Tooltip("Distortion block size for retro look")]
    [Range(1, 32)]
    [SerializeField] int blockSize = 8;

    [Header("Depth Darkening")]
    [Tooltip("How much to darken at max depth (0 = none, 1 = black)")]
    [Range(0f, 1f)]
    [SerializeField] float depthDarkening = 0.8f;

    [Tooltip("Depth at which maximum darkness is reached")]
    [SerializeField] float maxDepth = 20f;

    UnderwaterRendererFeature _underwaterFeature;

    void Start()
    {
        _underwaterFeature = UnderwaterRendererFeature.Instance;

        if (_underwaterFeature != null)
        {
            ApplySettings();
            _underwaterFeature.SetEnabled(enableUnderwaterEffect);
        }
        else
        {
            Debug.LogWarning("WaterController: UnderwaterRendererFeature not found in renderer");
        }
    }

    void ApplySettings()
    {
        if (_underwaterFeature == null) return;

        _underwaterFeature.SetWaterLevel(waterLevel);
        _underwaterFeature.SetTintColor(underwaterTint);
        _underwaterFeature.SetTintStrength(tintStrength);
        _underwaterFeature.SetDistortionStrength(distortionStrength);
        _underwaterFeature.SetBlockSize(blockSize);
        _underwaterFeature.SetDepthDarkening(depthDarkening);
        _underwaterFeature.SetMaxDepth(maxDepth);
    }

    void OnDestroy()
    {
        // Disable underwater effect when leaving scene
        if (_underwaterFeature != null)
            _underwaterFeature.SetEnabled(false);
    }

    /// <summary>
    /// Update water level at runtime (e.g., for tides)
    /// </summary>
    public void SetWaterLevel(float level)
    {
        waterLevel = level;
        _underwaterFeature?.SetWaterLevel(level);
    }

    /// <summary>
    /// Get current water level
    /// </summary>
    public float GetWaterLevel() => waterLevel;

#if UNITY_EDITOR
    void OnValidate()
    {
        // Apply settings in editor when values change
        if (Application.isPlaying && _underwaterFeature != null)
        {
            ApplySettings();
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw water level plane in editor
        Gizmos.color = new Color(0.2f, 0.5f, 0.8f, 0.3f);
        Vector3 center = new Vector3(transform.position.x, waterLevel, transform.position.z);
        Gizmos.DrawCube(center, new Vector3(50f, 0.1f, 50f));

        Gizmos.color = new Color(0.2f, 0.5f, 0.8f, 0.8f);
        Gizmos.DrawWireCube(center, new Vector3(50f, 0.1f, 50f));
    }
#endif
}
