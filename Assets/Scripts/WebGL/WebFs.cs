using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>IDBFS sync bridge. StoryRoot owns startup and the single in-flight writer.</summary>
public static class WebFs
{
    public const string ReceiverObject = "RodyStoryRoot";
    public const string FlushCallback = "OnSyncFsComplete";
    public const string HydrateCallback = "OnSyncFsHydrated";

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void RodySyncFs(string gameObjectName, string methodName);
    [DllImport("__Internal")] static extern void RodySyncFsHydrate(string gameObjectName, string methodName);
#endif

    static Action<string> _onFlush;
    static Action<string> _onHydrate;

    public static void Flush(Action<string> onComplete)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (_onFlush != null || _onHydrate != null) throw new InvalidOperationException("A filesystem sync is already running.");
        _onFlush = onComplete;
        RodySyncFs(ReceiverObject, FlushCallback);
#else
        onComplete(null);
#endif
    }

    public static void Hydrate(Action<string> onComplete)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (_onFlush != null || _onHydrate != null) throw new InvalidOperationException("A filesystem sync is already running.");
        _onHydrate = onComplete;
        RodySyncFsHydrate(ReceiverObject, HydrateCallback);
#else
        onComplete(null);
#endif
    }

    public static void HandleFlushComplete(string error)
    {
        var callback = _onFlush;
        _onFlush = null;
        callback?.Invoke(string.IsNullOrEmpty(error) ? null : error);
    }

    public static void HandleHydrateComplete(string error)
    {
        var callback = _onHydrate;
        _onHydrate = null;
        callback?.Invoke(string.IsNullOrEmpty(error) ? null : error);
    }
}
