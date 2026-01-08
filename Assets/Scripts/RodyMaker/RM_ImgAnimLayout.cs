using SFB;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RM_ImgAnimLayout : RM_Layout {

	public static List<Sprite> frames = new List<Sprite>();
    public int offset = 0;
    public Button[] frameBtn;

#if UNITY_WEBGL && !UNITY_EDITOR
    int _pendingFrameIndex;
#endif

    public void SetActiveBtn() {
        Debug.Log("RM_ImgAnimLayout::SetButton : frameCount = " + frames.Count);
        for (int i=0; i<3; i++) {
            frameBtn[i].interactable = i + offset <= frames.Count ? true: false;
        }
    }

	public void ReturnClick(){
		Debug.Log("Images return button clicked");
		SetLayouts(gm.imagesLayout);
		UnsetLayouts(gm.imgAnimLayout);
	}

	public void ImportClick(int i)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        _pendingFrameIndex = i;
        WebGLFileBrowser.Instance.OpenImageAsBase64("image/png,image/jpeg", OnWebGLFrameImported);
#else
        Debug.Log("Import button clicked");
        var extensions = new[] {new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),};
        string path = null;
        string[] files = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
        if (files.Length != 0)
            path = files[0];
        else
            return;

        // i + offset <= frames.Count because it should not be possible to add the i+1 frame if the i doesn't exist
        if ((i + offset) >= frames.Count)
            frames.Add(RM_SaveLoad.LoadSprite(path,0,320,130));
		else frames[i + offset] = RM_SaveLoad.LoadSprite(path,0,320,130);

        SetActiveBtn();
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    void OnWebGLFrameImported(string dataUrl)
    {
        if (string.IsNullOrEmpty(dataUrl)) return;

        var tex = WebGLFileBrowser.DataUrlToTexture(dataUrl);
        if (tex == null) return;

        // Resize to Atari ST resolution: 320x130
        RM_TextureScale.Point(tex, 320, 130);
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);

        int frameIndex = _pendingFrameIndex + offset;
        if (frameIndex >= frames.Count)
            frames.Add(sprite);
        else
            frames[frameIndex] = sprite;

        SetActiveBtn();
    }
#endif
}
