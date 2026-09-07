using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

// Only pronunciation comes from eSpeak. Rody owns its sound, pauses and authored score.
public static class FrenchPhonemizer
{
    public const int MaxCharacters = 2000;

    [Serializable]
    public class Point
    {
        public int source;
        public int ipa;
    }

    [Serializable]
    public class Result
    {
        public int requestId;
        public string ipa = "";
        public Point[] sourceMap = Array.Empty<Point>();
        public string error = "";
    }

    public static void Request(string text, int requestId, string receiver, Action<Result> completed)
    {
        if (text.Length > MaxCharacters)
        {
            completed(new Result { requestId = requestId, error = "Une réplique peut contenir jusqu’à 2 000 caractères." });
            return;
        }
#if UNITY_WEBGL && !UNITY_EDITOR
        RodyFrench_Convert(text, requestId, receiver, Application.streamingAssetsPath + "/RodyFrench/ephone.mjs");
#else
        var result = new Result { requestId = requestId };
        try
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            ConvertNative(text, result, Path.Combine(Application.streamingAssetsPath, "RodyFrench"));
#else
            result.error = "La conversion française est disponible sur Mac et dans la version navigateur.";
#endif
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            result.error = "La conversion n’a pas abouti. Réessaie avec Écouter.";
        }
        completed(result);
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void RodyFrench_Convert(string text, int requestId, string receiver, string moduleUrl);
#endif

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    static bool initialized;

    [DllImport("RodyFrench", CallingConvention = CallingConvention.Cdecl)]
    static extern int espeak_InitForTextToIpa([MarshalAs(UnmanagedType.LPUTF8Str)] string voice,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    [DllImport("RodyFrench", CallingConvention = CallingConvention.Cdecl)]
    static extern IntPtr espeak_TextToIpaWithSourceMap(ref IntPtr text, [Out] int[] map, int capacity);

    [DllImport("RodyFrench", CallingConvention = CallingConvention.Cdecl)]
    static extern void RodyFrench_Free(IntPtr result);

    static void ConvertNative(string text, Result result, string dataPath)
    {
        if (!initialized)
        {
            int error = espeak_InitForTextToIpa("roa/fr", dataPath);
            if (error != 0) throw new InvalidOperationException("French pronunciation initialization failed: " + error);
            initialized = true;
        }
        if (string.IsNullOrWhiteSpace(text)) return;

        byte[] utf8 = Encoding.UTF8.GetBytes(text + "\0");
        IntPtr original = Marshal.AllocHGlobal(utf8.Length);
        IntPtr output = IntPtr.Zero;
        try
        {
            Marshal.Copy(utf8, 0, original, utf8.Length);
            IntPtr cursor = original;
            var map = new int[utf8.Length * 2 + 2];
            output = espeak_TextToIpaWithSourceMap(ref cursor, map, map.Length);
            if (output == IntPtr.Zero) throw new InvalidOperationException("French pronunciation returned no result.");
            result.ipa = Marshal.PtrToStringUTF8(output);
            int count = 0;
            while (count * 2 + 1 < map.Length && map[count * 2] >= 0 && map[count * 2 + 1] >= 0) count++;
            result.sourceMap = new Point[count];
            for (int i = 0; i < count; i++)
                result.sourceMap[i] = new Point { source = map[i * 2], ipa = map[i * 2 + 1] };
        }
        finally
        {
            if (output != IntPtr.Zero) RodyFrench_Free(output);
            Marshal.FreeHGlobal(original);
        }
    }
#endif
}
