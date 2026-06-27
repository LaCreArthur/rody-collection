using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quantizes textures to authentic Rody Atari ST colors.
/// The master palette includes the clean original scene colors; each image is limited to 16 colors.
/// </summary>
public static class AtariPalette
{
    const int MaxSceneColors = 16;

    // Clean original scene colors from the official stories. Ibiza compression noise is intentionally excluded.
    static readonly Color32[] Palette =
    {
        new(0x00, 0x00, 0x00, 0xFF),
        new(0x00, 0x40, 0x80, 0xFF),
        new(0x00, 0x40, 0xA0, 0xFF),
        new(0x00, 0x60, 0xC0, 0xFF),
        new(0x00, 0xA0, 0xC0, 0xFF),
        new(0x00, 0xA0, 0xE0, 0xFF),
        new(0x00, 0x80, 0x60, 0xFF),
        new(0x00, 0x60, 0x20, 0xFF),
        new(0x00, 0x60, 0x40, 0xFF),
        new(0x40, 0x80, 0x00, 0xFF),
        new(0x00, 0xA0, 0x00, 0xFF),
        new(0x00, 0xC0, 0x60, 0xFF),
        new(0x20, 0x20, 0x20, 0xFF),
        new(0x40, 0x00, 0x60, 0xFF),
        new(0x60, 0x00, 0x60, 0xFF),
        new(0x60, 0x40, 0x60, 0xFF),
        new(0x80, 0x00, 0x20, 0xFF),
        new(0x80, 0x00, 0x40, 0xFF),
        new(0xC0, 0x00, 0x00, 0xFF),
        new(0x80, 0x40, 0x20, 0xFF),
        new(0x60, 0x40, 0x00, 0xFF),
        new(0x60, 0x40, 0x20, 0xFF),
        new(0xA0, 0x80, 0x40, 0xFF),
        new(0xE0, 0xC0, 0x60, 0xFF),
        new(0xE0, 0x80, 0x20, 0xFF),
        new(0x80, 0x40, 0xE0, 0xFF),
        new(0xA0, 0xA0, 0xE0, 0xFF),
        new(0xC0, 0x40, 0xC0, 0xFF),
        new(0xE0, 0xA0, 0xA0, 0xFF),
        new(0xE0, 0xA0, 0xC0, 0xFF),
        new(0xE0, 0xE0, 0xE0, 0xFF),
    };

    static readonly HashSet<int> PaletteKeys = BuildPaletteKeys();

    // 4x4 Bayer matrix normalized to 0..1
    static readonly float[] BayerMatrix =
    {
         0f/16f,  8f/16f,  2f/16f, 10f/16f,
        12f/16f,  4f/16f, 14f/16f,  6f/16f,
         3f/16f, 11f/16f,  1f/16f,  9f/16f,
        15f/16f,  7f/16f, 13f/16f,  5f/16f,
    };

    /// <summary>
    /// Quantizes a texture to the allowed Rody colors, with no more than 16 colors per image. Modifies in-place.
    /// </summary>
    public static void ApplyPalette(Texture2D tex)
    {
        var pixels = tex.GetPixels32();

        if (UsesAllowedScenePalette(pixels))
            return;

        bool sourceUsesOnlyAllowedColors = UsesOnlyAllowedColors(pixels);
        Color32[] scenePalette = PickBestScenePalette(pixels);
        bool useDither = !sourceUsesOnlyAllowedColors && scenePalette.Length > 1;
        int w = tex.width;

        for (int i = 0; i < pixels.Length; i++)
        {
            if (useDither)
            {
                int x = i % w;
                int y = i / w;
                float threshold = BayerMatrix[(y & 3) * 4 + (x & 3)];

                FindTwoNearest(pixels[i], scenePalette, out int idxA, out int idxB, out float t);
                pixels[i] = t > threshold ? scenePalette[idxB] : scenePalette[idxA];
            }
            else
            {
                pixels[i] = FindNearest(pixels[i], scenePalette);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
    }

    static bool UsesAllowedScenePalette(Color32[] pixels)
    {
        var colors = new HashSet<int>();

        for (int i = 0; i < pixels.Length; i++)
        {
            int key = ColorKey(pixels[i]);
            if (!PaletteKeys.Contains(key))
                return false;

            colors.Add(key);
            if (colors.Count > MaxSceneColors)
                return false;
        }

        return true;
    }

    static bool UsesOnlyAllowedColors(Color32[] pixels)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            if (!PaletteKeys.Contains(ColorKey(pixels[i])))
                return false;
        }

        return true;
    }

    static Color32[] PickBestScenePalette(Color32[] pixels)
    {
        List<WeightedColor> sourceColors = BuildHistogram(pixels);
        if (sourceColors.Count == 0)
            return new[] { Palette[0] };

        int maxColors = Mathf.Min(MaxSceneColors, Palette.Length);
        var selectedColors = new List<Color32>(maxColors);
        var selected = new bool[Palette.Length];
        var bestDistances = new float[sourceColors.Count];

        for (int i = 0; i < bestDistances.Length; i++)
            bestDistances[i] = float.MaxValue;

        for (int pick = 0; pick < maxColors; pick++)
        {
            int bestCandidate = -1;
            double bestScore = double.NegativeInfinity;

            for (int candidate = 0; candidate < Palette.Length; candidate++)
            {
                if (selected[candidate])
                    continue;

                double score = 0d;

                for (int i = 0; i < sourceColors.Count; i++)
                {
                    float distance = ColorDistance(sourceColors[i].color, Palette[candidate]);

                    if (pick == 0)
                        score -= (double)distance * sourceColors[i].count;
                    else if (distance < bestDistances[i])
                        score += (double)(bestDistances[i] - distance) * sourceColors[i].count;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCandidate = candidate;
                }
            }

            if (bestCandidate < 0 || (pick > 0 && bestScore <= 0d))
                break;

            selected[bestCandidate] = true;
            selectedColors.Add(Palette[bestCandidate]);

            for (int i = 0; i < sourceColors.Count; i++)
            {
                float distance = ColorDistance(sourceColors[i].color, Palette[bestCandidate]);
                if (distance < bestDistances[i])
                    bestDistances[i] = distance;
            }
        }

        return selectedColors.ToArray();
    }

    static List<WeightedColor> BuildHistogram(Color32[] pixels)
    {
        var counts = new Dictionary<int, int>();

        for (int i = 0; i < pixels.Length; i++)
        {
            int key = ColorKey(pixels[i]);
            counts.TryGetValue(key, out int count);
            counts[key] = count + 1;
        }

        var histogram = new List<WeightedColor>(counts.Count);
        foreach (var pair in counts)
            histogram.Add(new WeightedColor(ColorFromKey(pair.Key), pair.Value));

        return histogram;
    }

    static Color32 FindNearest(Color32 pixel, Color32[] palette)
    {
        int bestIndex = 0;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < palette.Length; i++)
        {
            float distance = ColorDistance(pixel, palette[i]);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }

        return palette[bestIndex];
    }

    /// <summary>
    /// Finds the two closest palette colors and the interpolation factor between them.
    /// </summary>
    static void FindTwoNearest(Color32 pixel, Color32[] palette, out int idxA, out int idxB, out float t)
    {
        float bestDist = float.MaxValue;
        float secondDist = float.MaxValue;
        idxA = 0;
        idxB = 0;

        for (int i = 0; i < palette.Length; i++)
        {
            float d = ColorDistance(pixel, palette[i]);
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

        if (palette.Length == 1)
        {
            idxB = idxA;
            t = 0f;
            return;
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

    static HashSet<int> BuildPaletteKeys()
    {
        var keys = new HashSet<int>();
        for (int i = 0; i < Palette.Length; i++)
            keys.Add(ColorKey(Palette[i]));
        return keys;
    }

    static int ColorKey(Color32 color)
    {
        return (color.r << 16) | (color.g << 8) | color.b;
    }

    static Color32 ColorFromKey(int key)
    {
        return new Color32((byte)(key >> 16), (byte)(key >> 8), (byte)key, 0xFF);
    }

    struct WeightedColor
    {
        public readonly Color32 color;
        public readonly int count;

        public WeightedColor(Color32 color, int count)
        {
            this.color = color;
            this.count = count;
        }
    }
}
