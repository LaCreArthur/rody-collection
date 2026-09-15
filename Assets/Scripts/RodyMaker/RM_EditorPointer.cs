using UnityEngine;

// One cursor owner in Maker; the original adventure keeps its animated cursor.
public class RM_EditorPointer : MonoBehaviour
{
    public enum Kind { Default, Text, Draw }
    public Texture2D defaultTexture;
    readonly Texture2D[] textures = new Texture2D[3];
    Object owner;
    Kind kind;
    int scale;

    void OnEnable()
    {
        owner = null;
        kind = Kind.Default;
        Rebuild();
    }

    void Update()
    {
        // Only a window resize changes the required integer enlargement.
        if (scale != PixelScale) Rebuild();
    }

    int PixelScale => Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(Screen.width / 320f, Screen.height / 200f)), 1, 6);

    public void Show(Object source, Kind value)
    {
        if (!isActiveAndEnabled) return;
        owner = source;
        if (kind == value) return;
        kind = value;
        Apply();
    }

    public void Clear(Object source)
    {
        if (owner == source) Reset();
    }

    public void Reset()
    {
        owner = null;
        kind = Kind.Default;
        Apply();
    }

    void Rebuild()
    {
        Release();
        scale = PixelScale;
        for (int k = 0; k < textures.Length; k++)
        {
            int width = k == 0 ? defaultTexture.width : 16;
            int height = k == 0 ? defaultTexture.height : 16;
            var pixels = new Color32[width * height];
            if (k == 0) pixels = defaultTexture.GetPixels32();
            else
            {
                // Native one-pixel strokes with a one-pixel white outline.
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        bool ink = Stroke((Kind)k, x, y);
                        bool edge = false;
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dx = -1; dx <= 1; dx++) edge |= Stroke((Kind)k, x + dx, y + dy);
                        pixels[y * width + x] = ink ? new Color32(0, 0, 0, 255)
                            : edge ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                    }
            }
            var enlarged = new Color32[width * height * scale * scale];
            for (int y = 0; y < height * scale; y++)
                for (int x = 0; x < width * scale; x++)
                    enlarged[y * width * scale + x] = pixels[y / scale * width + x / scale];
            var texture = new Texture2D(width * scale, height * scale, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.SetPixels32(enlarged);
            texture.Apply();
            textures[k] = texture;
        }
        Apply();
    }

    static bool Stroke(Kind value, int x, int y) => value == Kind.Text
        ? x == 7 && y >= 3 && y <= 12 || (y == 3 || y == 12) && x >= 5 && x <= 9
        : x == 7 && (y >= 2 && y <= 5 || y >= 9 && y <= 12)
            || y == 7 && (x >= 2 && x <= 5 || x >= 9 && x <= 12);

    void Apply()
    {
        if (!isActiveAndEnabled) return;
        Cursor.visible = true;
        Cursor.SetCursor(textures[(int)kind], kind == Kind.Default ? Vector2.zero : new Vector2(7, 8) * scale,
            CursorMode.ForceSoftware);
    }

    void Release()
    {
        foreach (var texture in textures) if (texture != null) Destroy(texture);
    }

    void OnDisable()
    {
        owner = null;
        kind = Kind.Default;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Release();
    }
}
