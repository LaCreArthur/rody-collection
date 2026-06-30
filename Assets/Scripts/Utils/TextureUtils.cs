using UnityEngine;

/// <summary>
/// Texture helpers shared across the storage and editor layers.
/// </summary>
public static class TextureUtils
{
    /// <summary>
    /// Returns a fresh readable RGBA copy of a texture (required for EncodeToPNG and
    /// pixel ops, including on WebGL where source textures are usually non-readable).
    /// Always a copy, so callers can scale or recolor it without touching the source.
    /// Replaces the two duplicate MakeTextureReadable implementations.
    /// </summary>
    public static Texture2D MakeReadable(Texture2D source)
    {
        RenderTexture tmp = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(source, tmp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;

        Texture2D readable = new Texture2D(source.width, source.height);
        readable.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        readable.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);
        return readable;
    }
}
