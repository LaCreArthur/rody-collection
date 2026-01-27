using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Utility for showing export reminders before leaving scenes with unsaved work.
/// Also manages browser beforeunload warning for WebGL.
/// </summary>
public static class ExportReminder
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool ShowConfirmDialog(string message);

    [DllImport("__Internal")]
    private static extern void SetUnsavedWorkFlag(int hasUnsaved);

    [DllImport("__Internal")]
    private static extern void InitBeforeUnloadHandler();
#endif

    private static bool _initialized;

    /// <summary>
    /// Initialize the beforeunload handler. Call once at app startup.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

#if UNITY_WEBGL && !UNITY_EDITOR
        InitBeforeUnloadHandler();
        Debug.Log("[ExportReminder] Initialized beforeunload handler");
#endif
    }

    /// <summary>
    /// Update the browser's unsaved work flag based on WorkingStory state.
    /// Call this whenever IsDirty might have changed.
    /// </summary>
    public static void UpdateUnsavedFlag()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        bool hasUnsaved = WorkingStory.IsLoaded && !WorkingStory.IsOfficial && WorkingStory.IsDirty;
        SetUnsavedWorkFlag(hasUnsaved ? 1 : 0);
#endif
    }

    /// <summary>
    /// Returns true if navigation should proceed, false if user cancelled.
    /// Shows a confirmation dialog if there are unsaved changes.
    /// </summary>
    public static bool CheckBeforeNavigating()
    {
        // No reminder needed if no story or official story or already saved
        if (!WorkingStory.IsLoaded || WorkingStory.IsOfficial || !WorkingStory.IsDirty)
            return true;

        string message = "L'histoire \"" + WorkingStory.Title + "\" n'est pas exportee!\n\n" +
                        "Elle sera perdue si tu fermes le navigateur.\n\n" +
                        "Continuer sans exporter?";

#if UNITY_WEBGL && !UNITY_EDITOR
        return ShowConfirmDialog(message);
#else
        // Desktop: Log warning but allow navigation
        Debug.LogWarning($"[ExportReminder] Unsaved changes in story: {WorkingStory.Title}");
        return true;
#endif
    }

    /// <summary>
    /// Navigate to scene 0 with export reminder check.
    /// </summary>
    public static void NavigateToMenuWithCheck()
    {
        if (CheckBeforeNavigating())
        {
            SceneManager.LoadScene(0);
        }
    }
}
