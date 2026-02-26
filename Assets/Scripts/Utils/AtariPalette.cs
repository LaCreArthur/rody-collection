using UnityEngine;

/// <summary>
/// Quantizes textures to the 16-color Atari ST palette using 4x4 Bayer ordered dithering.
/// Produces the classic checkerboard/crosshatch blending patterns of original Atari ST art.
/// </summary>
public static class AtariPalette
{
    static readonly Color32[] Palette =
    {
        new(0x80, 0x00, 0x20, 0xFF), // Burgundy
        new(0xC0, 0x40, 0xC0, 0xFF), // Purple
        new(0x60, 0x40, 0x60, 0xFF), // Dark Purple
        new(0x00, 0x60, 0xC0, 0xFF), // Blue
        new(0x00, 0xA0, 0xE0, 0xFF), // Cyan
        new(0x00, 0x80, 0x60, 0xFF), // Teal
        new(0x00, 0x60, 0x20, 0xFF), // Dark Green
        new(0x00, 0xA0, 0x00, 0xFF), // Green
        new(0xE0, 0xC0, 0x60, 0xFF), // Gold
        new(0x60, 0x40, 0x00, 0xFF), // Brown
        new(0xA0, 0x80, 0x40, 0xFF), // Tan
        new(0xE0, 0x80, 0x20, 0xFF), // Orange
        new(0xC0, 0x00, 0x00, 0xFF), // Red
        new(0xE0, 0xA0, 0xA0, 0xFF), // Pink
        new(0xE0, 0xE0, 0xE0, 0xFF), // White
        new(0x00, 0x00, 0x00, 0xFF), // Black
    };

    // 4x4 Bayer matrix normalized to 0..1
    static readonly float[] BayerMatrix =
    {
         0f/16f,  8f/16f,  2f/16f, 10f/16f,
        12f/16f,  4f/16f, 14f/16f,  6f/16f,
         3f/16f, 11f/16f,  1f/16f,  9f/16f,
        15f/16f,  7f/16f, 13f/16f,  5f/16f,
    };

    /// <summary>
    /// Quantizes a texture to the 16-color Atari ST palette with ordered dithering. Modifies in-place.
    /// </summary>
    public static void ApplyPalette(Texture2D tex)
    {
        var pixels = tex.GetPixels32();
        int w = tex.width;
        int h = tex.height;

        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % w;
            int y = i / w;
            float threshold = BayerMatrix[(y & 3) * 4 + (x & 3)];

            FindTwoNearest(pixels[i], out int idxA, out int idxB, out float t);
            pixels[i] = t > threshold ? Palette[idxB] : Palette[idxA];
        }

        tex.SetPixels32(pixels);
        tex.Apply();
    }

    /// <summary>
    /// Finds the two closest palette colors and the interpolation factor between them.
    /// </summary>
    static void FindTwoNearest(Color32 pixel, out int idxA, out int idxB, out float t)
    {
        float bestDist = float.MaxValue;
        float secondDist = float.MaxValue;
        idxA = 0;
        idxB = 0;

        for (int i = 0; i < Palette.Length; i++)
        {
            float d = ColorDistance(pixel, Palette[i]);
            if (d < bestDist)
            {
                secondDist = bestDist;
                idxB = idxA;
                bestDist = d;
                idxA = i;
            }
            else if (d < secondDist)
            {
                secondDist = d;
                idxB = i;
            }
        }

        // Interpolation factor: 0 = pure A, 1 = pure B
        float total = bestDist + secondDist;
        t = total > 0f ? bestDist / total : 0f;
    }

    /// <summary>
    /// Redmean weighted Euclidean distance — cheap perceptual color difference.
    /// </summary>
    static float ColorDistance(Color32 a, Color32 b)
    {
        float rMean = (a.r + b.r) * 0.5f;
        float dr = a.r - b.r;
        float dg = a.g - b.g;
        float db = a.b - b.b;
        return (2f + rMean / 256f) * dr * dr + 4f * dg * dg + (2f + (255f - rMean) / 256f) * db * db;
    }
}
