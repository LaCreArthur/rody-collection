using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RM_ImgAnimLayout : RM_Layout {

	public static List<Sprite> frames = new List<Sprite>();
    public int offset = 0;
    public Button[] frameBtn;

    int _pendingFrameIndex;

    public void SetActiveBtn() {
        Debug.Log("RM_ImgAnimLayout::SetButton : frameCount = " + frames.Count);
        for (int i=0; i<3; i++) {
            frameBtn[i].interactable = i + offset < frames.Count;
        }
    }

	public void ReturnClick(){
		Debug.Log("Images return button clicked");
		SetLayouts(gm.imagesLayout);
		UnsetLayouts(gm.imgAnimLayout);
	}

	public void ImportClick(int i)
    {
        _pendingFrameIndex = i;
        WebGLFileBrowser.Instance.OpenImageAsBase64("image/png,image/jpeg", OnFrameImported);
    }

    void OnFrameImported(string dataUrl)
    {
        if (string.IsNullOrEmpty(dataUrl)) return;

        var tex = WebGLFileBrowser.DataUrlToTexture(dataUrl);
        if (tex == null) return;

        // Unify with the main image path: Atari palette + pixelsPerUnit 1.
        RM_TextureScale.Point(tex, 320, 130);
        AtariPalette.ApplyPalette(tex);
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);

        // Allow replacing an existing frame or appending the next sequential frame only.
        int frameIndex = _pendingFrameIndex + offset;
        if (frameIndex > frames.Count)
        {
            Debug.LogWarning($"[RM_ImgAnimLayout] Ignoring frame import at missing index {frameIndex} (count={frames.Count})");
            return;
        }

        if (frameIndex == frames.Count)
            frames.Add(sprite);
        else
            frames[frameIndex] = sprite;

        SetActiveBtn();
    }
}
