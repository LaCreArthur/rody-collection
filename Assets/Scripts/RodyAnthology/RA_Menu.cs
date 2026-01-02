using System.Collections;
using UnityEngine;

public class RA_Menu : MonoBehaviour
{
    [Header("Pixelation Transition")]
    [Tooltip("Starting chunky resolution (fewer pixels = more chunky)")]
    [SerializeField] int startBlockCount = 32;

    [Tooltip("Target resolution - should match DOOM game resolution (e.g., 320 for Atari ST)")]
    [SerializeField] int targetBlockCount = 320;

    [SerializeField] float transitionDuration = 1.5f;

    [Tooltip("Exit resolution - animate back to chunky before scene change")]
    [SerializeField] int exitBlockCount = 32;

    int _currentBlockCount;
    bool _loading = true;
    float _transitionTime;

    /// <summary>
    /// Current block count (32 = chunky, 320 = native resolution)
    /// </summary>
    public int BlockCount => _currentBlockCount;

    /// <summary>
    /// True when exit transition is complete
    /// </summary>
    public bool ExitTransitionComplete { get; private set; }

    PixelationRendererFeature Feature => PixelationRendererFeature.Instance;

    void Start()
    {
        if (Feature != null)
        {
            _currentBlockCount = startBlockCount;
            Feature.SetBlockCount(_currentBlockCount);
            Feature.SetEnabled(true);
            _transitionTime = 0f;
        }
    }

    void Update()
    {
        if (!_loading || Feature == null) return;

        _transitionTime += Time.deltaTime;
        float t = Mathf.Clamp01(_transitionTime / transitionDuration);

        // Ease-out curve for smooth transition
        float easedT = 1f - (1f - t) * (1f - t);

        _currentBlockCount = Mathf.RoundToInt(Mathf.Lerp(startBlockCount, targetBlockCount, easedT));
        Feature.SetBlockCount(_currentBlockCount);

        if (t >= 1f)
        {
            _loading = false;
            // Keep at target resolution (same as DOOM game)
            Feature.SetBlockCount(targetBlockCount);
        }
    }

    /// <summary>
    /// Animates block count down to chunky pixels before scene exit.
    /// Use in a coroutine: yield return StartCoroutine(menu.AnimateExitTransition());
    /// </summary>
    public IEnumerator AnimateExitTransition()
    {
        if (Feature == null) yield break;

        ExitTransitionComplete = false;
        Feature.SetEnabled(true);

        float startValue = targetBlockCount;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            // Ease-in curve for exit
            float easedT = t * t;

            _currentBlockCount = Mathf.RoundToInt(Mathf.Lerp(startValue, exitBlockCount, easedT));
            Feature.SetBlockCount(_currentBlockCount);
            yield return null;
        }

        _currentBlockCount = exitBlockCount;
        Feature.SetBlockCount(_currentBlockCount);
        ExitTransitionComplete = true;
    }

    void OnDestroy()
    {
        // Disable pixelation when leaving scene
        Feature?.SetEnabled(false);
    }
}
