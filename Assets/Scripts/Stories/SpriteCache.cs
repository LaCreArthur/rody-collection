using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The one base64-to-Sprite decoder, cache, and teardown. Replaces the two
/// copy-pasted decoders (session and Resources provider) and owns the sprite
/// naming convention. Each owner holds its own instance (independent cache
/// lifetimes); the decode/destroy logic lives here once.
/// </summary>
public class SpriteCache
{
    readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

    /// <summary>Story cover thumbnail filename.</summary>
    public const string CoverName = "cover.png";

    /// <summary>Title-screen image filename.</summary>
    public const string TitleName = "0.png";

    /// <summary>Scene animation frame filename, e.g. "3.1.png".</summary>
    public static string SceneFrameName(int sceneIndex, int frame) => $"{sceneIndex}.{frame}.png";

    /// <summary>
    /// Returns the cached sprite for key, otherwise decodes base64, caches, and
    /// returns it. Returns null if base64 is null/empty or decoding fails.
    /// </summary>
    public Sprite Get(string key, string base64, int width = 320, int height = 130)
    {
        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var sprite = Decode(base64, width, height);
        if (sprite != null)
            _cache[key] = sprite;
        return sprite;
    }

    /// <summary>Removes and destroys one cached sprite (e.g. after it is overwritten).</summary>
    public void Evict(string key)
    {
        if (_cache.TryGetValue(key, out var sprite))
        {
            Destroy(sprite);
            _cache.Remove(key);
        }
    }

    /// <summary>Re-keys a cached sprite without re-decoding (used on scene reindex).</summary>
    public void Rename(string oldKey, string newKey)
    {
        if (_cache.TryGetValue(oldKey, out var sprite))
        {
            _cache[newKey] = sprite;
            _cache.Remove(oldKey);
        }
    }

    /// <summary>Destroys all cached sprites and clears the cache.</summary>
    public void Clear()
    {
        foreach (var sprite in _cache.Values)
            Destroy(sprite);
        _cache.Clear();
    }

    static Sprite Decode(string base64, int width, int height)
    {
        if (string.IsNullOrEmpty(base64))
            return null;

        try
        {
            // Strip a "data:image/...;base64," prefix if present.
            if (base64.StartsWith("data:"))
            {
                int comma = base64.IndexOf(',');
                if (comma > 0) base64 = base64.Substring(comma + 1);
            }

            byte[] bytes = Convert.FromBase64String(base64);
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.LoadImage(bytes);

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
        }
        catch (Exception e)
        {
            Debug.LogError($"SpriteCache: failed to decode sprite: {e.Message}");
            return null;
        }
    }

    static void Destroy(Sprite sprite)
    {
        if (sprite != null && sprite.texture != null)
        {
            UnityEngine.Object.Destroy(sprite.texture);
            UnityEngine.Object.Destroy(sprite);
        }
    }
}
