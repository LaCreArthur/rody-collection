using SFB;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RM_ImgAnimLayout : RM_Layout {

	public static List<Sprite> frames = new List<Sprite>();
    public int offset = 0;
    public Button[] frameBtn;

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
        Debug.Log("[RM_ImgAnimLayout] File browser not available on WebGL");
        return;
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
}
