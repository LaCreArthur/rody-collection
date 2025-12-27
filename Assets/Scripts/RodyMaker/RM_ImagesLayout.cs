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
        Debug.Log("[RM_ImagesLayout] File browser not available on WebGL");
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

		gm.scenePanel.GetComponent<Transform>().localPosition = new Vector3(0,-35,0);
        if (gm.currentScene == 0)
			gm.scenePanel.GetComponent<SpriteRenderer>().sprite = RM_SaveLoad.LoadSprite(path,0,640,400); // the title is a bigger img
		else
			gm.scenePanel.GetComponent<SpriteRenderer>().sprite = RM_SaveLoad.LoadSprite(path,0,320,130);
        // update the miniature
        gm.mainLayout.GetComponent<RM_MainLayout>().sceneThumbnails[gm.currentScene].GetComponent<Image>().sprite = RM_SaveLoad.LoadSprite(path,0,36,21);
#endif
    }
}
