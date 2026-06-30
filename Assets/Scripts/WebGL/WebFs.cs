using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Bridge to the browser IndexedDB flush/hydrate for persistentDataPath.
/// The single place that touches the IDBFS sync P/Invoke.
///
/// FS.syncfs is asynchronous and Unity does not await it: completion arrives via
/// SendMessage to the StoryRoot receiver GameObject, which forwards to the
/// Handle* methods below (a static class cannot be a SendMessage target). On
/// non-WebGL / in the Editor the calls are synchronous no-ops, because the
/// native filesystem needs no flush.
///
/// One pending callback per operation is tracked; a backstop flush with no
/// callback may supersede a pending one, which is acceptable on page-hide.
/// </summary>
public static class WebFs
{
    /// <summary>Name of the GameObject (StoryRoot) that receives the jslib SendMessage callbacks.</summary>
    public const string ReceiverObject = "RodyStoryRoot";
    public const string FlushCallback = "OnSyncFsComplete";
    public const string HydrateCallback = "OnSyncFsHydrated";

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void RodySyncFs(string gameObjectName, string methodName);
    [DllImport("__Internal")] static extern void RodySyncFsHydrate(string gameObjectName, string methodName);
#endif

    static Action<string> _onFlush;
    static Action<string> _onHydrate;

    /// <summary>
    /// Persist persistentDataPath to IndexedDB. onComplete(error) fires when the
    /// sync finishes (error == null on success). On non-WebGL it runs synchronously.
    /// </summary>
    public static void Flush(Action<string> onComplete = null)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        _onFlush = onComplete;
        RodySyncFs(ReceiverObject, FlushCallback);
#else
        onComplete?.Invoke(null);
#endif
    }

    /// <summary>
    /// Hydrate persistentDataPath FROM IndexedDB. Call once at startup before the
    /// first user-story read. On non-WebGL it runs synchronously.
    /// </summary>
    public static void Hydrate(Action<string> onComplete = null)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        _onHydrate = onComplete;
        RodySyncFsHydrate(ReceiverObject, HydrateCallback);
#else
        onComplete?.Invoke(null);
#endif
    }

    /// <summary>Invoked by StoryRoot when the jslib reports flush completion ("" = success).</summary>
    public static void HandleFlushComplete(string error)
    {
        var cb = _onFlush;
        _onFlush = null;
        cb?.Invoke(string.IsNullOrEmpty(error) ? null : error);
    }

    /// <summary>Invoked by StoryRoot when the jslib reports hydrate completion ("" = success).</summary>
    public static void HandleHydrateComplete(string error)
    {
        var cb = _onHydrate;
        _onHydrate = null;
        cb?.Invoke(string.IsNullOrEmpty(error) ? null : error);
    }
}
