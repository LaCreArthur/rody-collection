using System.Collections;
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
    public Text saveStatusText;  // Optional: assign in Inspector for save feedback

    void Start() {
        SetActiveBtn();
    }

    public void SetActiveBtn(){
        objBtn.interactable = IntroBtn.interactable = (gm.currentScene == 0)?false:true;
    }

    public void LoadSprites()
    {
        // JSON story loading
        if (PathManager.IsJsonStory)
        {
            LoadSpritesFromJson();
            return;
        }

        // Legacy folder-based loading
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

    /// <summary>
    /// Loads sprites from JSON story using RM_SaveLoad helper methods.
    /// </summary>
    private void LoadSpritesFromJson()
    {
        int sceneCount = PlayerPrefs.GetInt("scenesCount");
        int i;

        // Load miniscene thumbnails
        miniScenes[0].GetComponent<Image>().sprite = RM_SaveLoad.LoadTitleSprite();

        for (i = 1; i <= sceneCount; i++)
        {
            miniScenes[i].GetComponent<Image>().sprite = RM_SaveLoad.LoadSceneThumbnail(i);
        }

        // Activate new scene button
        if (i < 29)
        {
            miniScenes[i].GetComponent<Button>().interactable = true;
            miniScenes[i].GetComponent<Image>().sprite = miniAddSceneSprite;
        }

        // Load current scene sprite
        if (gm.currentScene == 0)
        {
            gm.scenePanel.GetComponent<SpriteRenderer>().sprite = RM_SaveLoad.LoadTitleSprite();
        }
        else
        {
            // Load scene sprites from JSON
            var sceneSprites = RM_SaveLoad.LoadSceneSprites(gm.currentScene);
            if (sceneSprites.Count > 0)
            {
                gm.scenePanel.GetComponent<SpriteRenderer>().sprite = sceneSprites[0];
            }

            gm.framesCount = sceneSprites.Count - 1;

            // Reset and populate frame list
            RM_ImgAnimLayout.frames.Clear();
            for (i = 1; i < sceneSprites.Count; i++)
            {
                RM_ImgAnimLayout.frames.Add(sceneSprites[i]);
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
        var warningLayout = gm.warningLayout.GetComponent<RM_WarningLayout>();
        warningLayout.test = true;
        warningLayout.newScene = gm.currentScene;
        warningLayout.warningText.text = "TU TESTES LA SCENE\nAttention Rody, les modifications non sauvegardées seront perdues ! Es-tu sûr de vouloir continuer ?";
        UnsetLayouts(gm.mainLayout);
        SetLayouts(gm.warningLayout);
    }
    public void SaveClick()
    {
        Debug.Log("Save button clicked");
#if UNITY_WEBGL && !UNITY_EDITOR
        SaveClickAsync();
#else
        RM_SaveLoad.SaveGame(gm);
        gm.Reset();
        StartCoroutine(ShowSaveFeedback());
#endif
    }

    private IEnumerator ShowSaveFeedback()
    {
        // Flash the current miniscene to indicate save
        if (gm.currentScene < miniScenes.Length)
        {
            var miniImage = miniScenes[gm.currentScene].GetComponent<Image>();
            Color originalColor = miniImage.color;

            // Flash white briefly
            miniImage.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            miniImage.color = originalColor;
            yield return new WaitForSeconds(0.1f);
            miniImage.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            miniImage.color = originalColor;
        }

        // Also show text feedback if available
        if (saveStatusText != null)
        {
            saveStatusText.text = "Sauvegardé !";
            saveStatusText.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            saveStatusText.gameObject.SetActive(false);
        }

        Debug.Log("[RM_MainLayout] Save feedback shown");
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    private void SaveClickAsync()
    {
        // Disable save button while saving
        Debug.Log("[RM_MainLayout] Starting async save to Firebase...");

        RM_SaveLoad.SaveGameAsync(gm,
            () => {
                Debug.Log("[RM_MainLayout] Save completed successfully!");
                gm.Reset();
            },
            error => {
                Debug.LogError($"[RM_MainLayout] Save failed: {error}");
                // TODO: Show error UI to user
            }
        );
    }
#endif

    public void RM_DeleteClick()
    {
        Debug.Log("Delete button clicked");

        // Only allow deletion of scenes >= 18 (or new unsaved scenes)
        if (gm.currentScene < 18 && gm.currentScene <= PlayerPrefs.GetInt("scenesCount"))
        {
            Debug.Log("Cannot delete scenes 1-17");
            return;
        }

        var warningLayout = gm.warningLayout.GetComponent<RM_WarningLayout>();
        warningLayout.newScene = gm.currentScene;
        warningLayout.isDeleteMode = true;

        if (gm.currentScene > PlayerPrefs.GetInt("scenesCount"))
            warningLayout.warningText.text = "TU ANNULES CETTE NOUVELLE SCENE\nAttention Rody, cela va effacer la scène ! Es-tu sûr de vouloir continuer ?";
        else
            warningLayout.warningText.text = "TU SUPPRIMES LA SCENE " + gm.currentScene + "\nAttention Rody, cela va effacer définitivement la scène ! Es-tu sûr de vouloir continuer ?";

        UnsetLayouts(gm.mainLayout);
        SetLayouts(gm.warningLayout);
    }


    public void RM_SceneClick(int scene)
    {
        Debug.Log("Scene " + scene + " button clicked");
        RM_WarningLayout warningLayout = gm.warningLayout.GetComponent<RM_WarningLayout>();

        string strChangeScene = "TU CHANGES DE SCENE\nAttention Rody, les modifications non sauvegardées seront perdues ! Es-tu sûr de vouloir continuer ?";
        string strRemoveScene = "TU SUPPRIMES LA SCENE " + scene + "\nAttention Rody, cela va effacer définitivement la scène ! Es-tu sûr de vouloir continuer ?";
        string strCancelScene = "TU ANNULES CETTE NOUVELLE SCENE\nAttention Rody, cela va effacer la scène ! Es-tu sûr de vouloir continuer ?";
        string strNewScene    = "TU AJOUTES UNE NOUVELLE SCENE\nAttention Rody, les modifications non sauvegardées seront perdues ! Es-tu sûr de vouloir continuer ?";

        if (scene == gm.currentScene) {
            // Clicking on current scene: >= 18 means delete/cancel, < 18 does nothing
            if (scene >= 18) {
                warningLayout.isDeleteMode = true;  // Use delete mode
                if (scene > PlayerPrefs.GetInt("scenesCount")) {
                    Debug.Log("cancel new scene?");
                    warningLayout.warningText.text = strCancelScene;
                }
                else {
                    Debug.Log("delete scene?");
                    warningLayout.warningText.text = strRemoveScene;
                }
            }
            else {
                // Scenes 1-17 cannot be deleted by clicking
                return;
            }
        }
        else if (scene > PlayerPrefs.GetInt("scenesCount")) {
            // Adding a new scene
            warningLayout.warningText.text = strNewScene;
        }
        else {
            // Changing to a different existing scene
            Debug.Log("change scene?");
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
