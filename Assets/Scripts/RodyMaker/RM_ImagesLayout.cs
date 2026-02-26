using SFB;
using UnityEngine;
using UnityEngine.UI;

public class RM_ImagesLayout : RM_Layout {

 	public Button imgAnimBtn1;
	public Button imgAnimBtn2;
	void Start(){
        SetActiveBtn();
	}

	public void SetActiveBtn(){
		imgAnimBtn1.interactable = imgAnimBtn2.interactable = (gm.currentScene == 0)?false:true; // launch screen doesn't have animations

		// if 3 or more frames, the 4 to 6 frames editor is accessible
		if (RM_ImgAnimLayout.frames.Count < 3)
			imgAnimBtn2.interactable = false;
		else
			imgAnimBtn2.interactable = true;
	}
	public void ReturnClick(){
		Debug.Log("Images return button clicked");
		SetLayouts(gm.mainLayout);
		UnsetLayouts(gm.imagesLayout);
	}
	public void ImgAnimClick(bool isSecond){

		gm.imgAnimLayout.GetComponent<RM_ImgAnimLayout>().offset = isSecond ? 3 : 0;
		gm.imgAnimLayout.GetComponent<RM_ImgAnimLayout>().SetActiveBtn();
		Debug.Log("Img Animes button clicked");
		SetLayouts(gm.imgAnimLayout);
		UnsetLayouts(gm.imagesLayout);
	}
    public void ImportClick()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLFileBrowser.Instance.OpenImageAsBase64("image/png,image/jpeg", OnWebGLImageImported);
#else
        Debug.Log("Import button clicked");
        var extensions = new[] {new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),};
        string[] files = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
        if (files.Length == 0) return;

        byte[] bytes = System.IO.File.ReadAllBytes(files[0]);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        ProcessImportedTexture(tex);
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    void OnWebGLImageImported(string dataUrl)
    {
        if (string.IsNullOrEmpty(dataUrl)) return;

        var tex = WebGLFileBrowser.DataUrlToTexture(dataUrl);
        if (tex == null) return;

        ProcessImportedTexture(tex);
    }
#endif

    void ProcessImportedTexture(Texture2D tex)
    {
        int width = 320;
        int height = gm.currentScene == 0 ? 200 : 130;
        RM_TextureScale.Point(tex, width, height);
        AtariPalette.ApplyPalette(tex);

        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
        gm.scenePanel.GetComponent<Transform>().localPosition = new Vector3(0, -35, 0);
        gm.scenePanel.GetComponent<SpriteRenderer>().sprite = sprite;

        // Thumbnail
        var thumbTex = new Texture2D(tex.width, tex.height);
        thumbTex.SetPixels(tex.GetPixels());
        RM_TextureScale.Point(thumbTex, 36, 21);
        var thumbSprite = Sprite.Create(thumbTex, new Rect(0, 0, 36, 21), new Vector2(0.5f, 0.5f), 1f);
        gm.mainLayout.GetComponent<RM_MainLayout>().sceneThumbnails[gm.currentScene].GetComponent<Image>().sprite = thumbSprite;

        // Persist to WorkingStory
        if (gm.currentScene == 0)
            WorkingStory.SaveSprite("0.png", tex);
        else
            WorkingStory.SaveSprite($"{gm.currentScene}.1.png", tex);
    }
}
