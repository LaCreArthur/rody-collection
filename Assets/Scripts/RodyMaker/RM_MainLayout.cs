using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class RM_MainLayout : RM_Layout
{

    public Color notActiveColor ;
    public Color activeColor;
    public GameObject[] miniScenes;
    public Slider sliderScenes;
    public Sprite miniAddSceneSprite;

    public Button objBtn, IntroBtn;

    void Start() {
        SetActiveBtn();
    }

    public void SetActiveBtn(){
        objBtn.interactable = IntroBtn.interactable = (gm.currentScene == 0)?false:true;
    }

    public void LoadSprites()
    {
        string spritePath = PathManager.SpritesPath + Path.DirectorySeparatorChar;
        int i;
        // Debug.Log("LoadSprites spritePath : " + spritePath);
           
        // Load miniscenes
        miniScenes[0].GetComponent<Image>().sprite = 
            RM_SaveLoad.LoadSprite(spritePath+0+".png",0,36,21);

        for (i = 1; i < PlayerPrefs.GetInt("scenesCount") + 1; i++) { 
            Sprite miniSprite = RM_SaveLoad.LoadSprite(spritePath + (i), 1, 36, 21);
            miniScenes[i].GetComponent<Image>().sprite = miniSprite;
            //miniScene.GetComponent<Image>().color = notActiveColor;
            //Debug.Log(spritePath+i+".1.png");
        }

        // activate new scene button
        if (i < 29) {// 29 is the last scene
            miniScenes[i].GetComponent<Button>().interactable = true;
            miniScenes[i].GetComponent<Image>().sprite = miniAddSceneSprite;
        }
        // Load Scene sprite
        if (gm.currentScene == 0)
            gm.scenePanel.GetComponent<SpriteRenderer>().sprite = 
                RM_SaveLoad.LoadSprite(spritePath+0+".png",0,320,240);
        else {
            gm.scenePanel.GetComponent<SpriteRenderer>().sprite = 
                RM_SaveLoad.LoadSprite(spritePath+(gm.currentScene),1,320,130);
            
            // Load animations frames
            DirectoryInfo dir = new DirectoryInfo(spritePath);
            var files = dir.GetFiles(gm.currentScene + ".*.png");
            gm.framesCount = files.Length - 1; // - 1 because the x.1.png is not part of the animation
            //Debug.Log("there are " + files.Length + " sprites for the scene " + gm.currentScene);
            
            // reset frame List
		    RM_ImgAnimLayout.frames.Clear();
            
            // add the scene frames
            for (i = 0; i < gm.framesCount; ++i) {
			    RM_ImgAnimLayout.frames.Add(RM_SaveLoad.LoadSprite(spritePath+(gm.currentScene), i+2, 320, 130));
            }
        }
    }

    public void IntroClick()
    {
        Debug.Log("Intro button clicked");
        SetLayouts(gm.introLayout,gm.introTextObj);
        UnsetLayouts(gm.mainLayout);
        gm.introLayout.GetComponent<RM_IntroLayout>().titleInputField.text = gm.titleText;
    }

    public void ImagesClick()
    {
        Debug.Log("Images button clicked");
        SetLayouts(gm.imagesLayout);
        UnsetLayouts(gm.mainLayout);
        gm.imagesLayout.GetComponent<RM_ImagesLayout>().SetActiveBtn();
    }
    public void RM_ObjectsClick()
    {
        Debug.Log("Objects button clicked");
        SetLayouts(gm.introTextObj, gm.title, gm.objectsLayout);
        UnsetLayouts(gm.mainLayout);
    }

    public void TestClick()
    {
        Debug.Log("Test button clicked");
        gm.warningLayout.GetComponent<RM_WarningLayout>().test = true;
        gm.warningLayout.GetComponent<RM_WarningLayout>().newScene = gm.currentScene;
        UnsetLayouts(gm.mainLayout);
        SetLayouts(gm.warningLayout);
    }
    public void SaveClick()
    {
        Debug.Log("Save button clicked");
        RM_SaveLoad.SaveGame(gm);
        gm.Reset();
    }

    public void RM_ResetClick()
    {
        Debug.Log("Reset button clicked");
        gm.warningLayout.GetComponent<RM_WarningLayout>().newScene = gm.currentScene;
        UnsetLayouts(gm.mainLayout);
        SetLayouts(gm.warningLayout);
    }


    public void RM_SceneClick(int scene)
    {
        Debug.Log("Scene " + scene + " button clicked");
        RM_WarningLayout warningLayout = gm.warningLayout.GetComponent<RM_WarningLayout>();
        
        string strChangeScene = "TU CHANGES DE SCENE \nAttention Rody, cela va effacer les modification non sauvegardées ! Es-tu sûr de vouloir continuer ?";
	    string strRemoveScene = "TU SUPPRIMES LA SCENE \nAttention Rody, cela va effacer la scène ! Es-tu sûr de vouloir continuer ?";
        string strCancelScene = "TU ANNULES CETTE NOUVELLE SCENE \nAttention Rody, cela va effacer la scène ! Es-tu sûr de vouloir continuer ?";
        string strNewScene    = "TU AJOUTE UNE NOUVELLE SCENE \nAttention Rody, cela va effacer les modification non sauvegardées ! Es-tu sûr de vouloir continuer ?";
        
        if (scene == gm.currentScene) {
            // >= 18 : remove or cancel
            if (scene >= 18) {
                if (scene > PlayerPrefs.GetInt("scenesCount")) {
                    Debug.Log("cancel?");
                    warningLayout.warningText.text = strCancelScene;
                }
                else {
                    Debug.Log("delete?");
                    warningLayout.warningText.text = strRemoveScene;
                }
            }
            // < 18 : nothing
            else return;
        }
            
        else if (scene > PlayerPrefs.GetInt("scenesCount"))
                warningLayout.warningText.text = strNewScene;
        else {
            Debug.Log("change?");
            warningLayout.warningText.text = strChangeScene;
        }
        
        warningLayout.newScene = scene;
        UnsetLayouts(gm.mainLayout);
        SetLayouts(gm.warningLayout);
        
    }

    public void MiniSceneUpdate() {
        for (int i = 0; i<30; i++) {
            miniScenes[i].GetComponent<Image>().color = notActiveColor;
            //Debug.Log(spritePath+i+".1.png");
        }
        miniScenes[gm.currentScene].GetComponent<Image>().color = activeColor;
        MoveMini((int)sliderScenes.value); // reset new scene button if a scene was deleted
    }

    public void SliderHandler ()
    {
        MoveMini((int)sliderScenes.value);
    }

    public void MoveMini(int sliderValue)
    {
        for (int i=0; i<5; ++i) // for each rows
        {
            for (int j = 6*i; j < 6*i+6; ++j) // 6 miniatures per row
            {
                if (j < 6 * sliderValue || j > 6 * sliderValue + 17 || j > PlayerPrefs.GetInt("scenesCount") + 1) // mini over the top or under the bottom of layout
                    miniScenes[j].SetActive(false);
                else
                {
                    if (j <= PlayerPrefs.GetInt("scenesCount") + 1)
                            miniScenes[j].SetActive(true);
                    Vector3 minipos = miniScenes[j].GetComponent<Transform>().localPosition;
                    miniScenes[j].GetComponent<Transform>().localPosition = new Vector3(minipos.x, 22.5f - ((i-sliderValue) * 22.0f), minipos.z);
                }
            }
        }
    }
}
