using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Main editor layout controller. Handles scene thumbnails, button clicks,
/// and navigation between editor panels.
/// </summary>
public class RM_MainLayout : RM_Layout
{
    [Header("Scene Thumbnails")]
    [FormerlySerializedAs("notActiveColor")]
    public Color inactiveSceneColor;

    [FormerlySerializedAs("activeColor")]
    public Color activeSceneColor;

    [FormerlySerializedAs("miniScenes")]
    public GameObject[] sceneThumbnails;

    [FormerlySerializedAs("sliderScenes")]
    public Slider thumbnailSlider;

    [FormerlySerializedAs("miniAddSceneSprite")]
    public Sprite addSceneSprite;

    [Header("Buttons")]
    [FormerlySerializedAs("objBtn")]
    public Button objectsButton;

    [FormerlySerializedAs("IntroBtn")]
    public Button introButton;

    [Header("Feedback")]
    public Text saveStatusText;

    void Start() {
        UpdateButtonStates();
    }

    public void UpdateButtonStates(){
        objectsButton.interactable = introButton.interactable = (gm.currentScene != 0);
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

        // Load scene thumbnails
        sceneThumbnails[0].GetComponent<Image>().sprite =
            RM_SaveLoad.LoadSprite(spritePath+0+".png",0,36,21);

        for (i = 1; i < PlayerPrefs.GetInt("scenesCount") + 1; i++) {
            Sprite thumbnail = RM_SaveLoad.LoadSprite(spritePath + (i), 1, 36, 21);
            sceneThumbnails[i].GetComponent<Image>().sprite = thumbnail;
        }

        // Activate new scene button
        if (i < 29) {
            sceneThumbnails[i].GetComponent<Button>().interactable = true;
            sceneThumbnails[i].GetComponent<Image>().sprite = addSceneSprite;
        }

        // Load main scene sprite
        if (gm.currentScene == 0)
            gm.scenePanel.GetComponent<SpriteRenderer>().sprite =
                RM_SaveLoad.LoadSprite(spritePath+0+".png",0,320,240);
        else {
            gm.scenePanel.GetComponent<SpriteRenderer>().sprite =
                RM_SaveLoad.LoadSprite(spritePath+(gm.currentScene),1,320,130);

            // Load animation frames
            DirectoryInfo dir = new DirectoryInfo(spritePath);
            var files = dir.GetFiles(gm.currentScene + ".*.png");
            gm.framesCount = files.Length - 1;

            RM_ImgAnimLayout.frames.Clear();
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

        // Load scene thumbnails
        sceneThumbnails[0].GetComponent<Image>().sprite = RM_SaveLoad.LoadTitleSprite();

        for (i = 1; i <= sceneCount; i++)
        {
            sceneThumbnails[i].GetComponent<Image>().sprite = RM_SaveLoad.LoadSceneThumbnail(i);
        }

        // Activate new scene button
        if (i < 29)
        {
            sceneThumbnails[i].GetComponent<Button>().interactable = true;
            sceneThumbnails[i].GetComponent<Image>().sprite = addSceneSprite;
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

    public void OnIntroClick()
    {
        Debug.Log("Intro button clicked");
        SetLayouts(gm.introLayout,gm.introTextObj);
        UnsetLayouts(gm.mainLayout);
        gm.introLayout.GetComponent<RM_IntroLayout>().titleInputField.text = gm.titleText;
    }

    public void OnImagesClick()
    {
        Debug.Log("Images button clicked");
        SetLayouts(gm.imagesLayout);
        UnsetLayouts(gm.mainLayout);
        gm.imagesLayout.GetComponent<RM_ImagesLayout>().SetActiveBtn();
    }

    public void OnObjectsClick()
    {
        Debug.Log("Objects button clicked");
        SetLayouts(gm.introTextObj, gm.title, gm.objectsLayout);
        UnsetLayouts(gm.mainLayout);
    }

    public void OnTestClick()
    {
        Debug.Log("Test button clicked");
        var warningLayout = gm.warningLayout.GetComponent<RM_WarningLayout>();
        warningLayout.isTestMode = true;
        warningLayout.targetScene = gm.currentScene;
        warningLayout.messageText.text = "TU TESTES LA SCENE\nAttention Rody, les modifications non sauvegardées seront perdues ! Es-tu sûr de vouloir continuer ?";
        UnsetLayouts(gm.mainLayout);
        SetLayouts(gm.warningLayout);
    }

    public void OnSaveClick()
    {
        Debug.Log("Save button clicked");
#if UNITY_WEBGL && !UNITY_EDITOR
        OnSaveClickAsync();
#else
        RM_SaveLoad.SaveGame(gm);
        gm.Reset();
        StartCoroutine(ShowSaveFeedback());
#endif
    }

    private IEnumerator ShowSaveFeedback()
    {
        // Flash the current scene thumbnail to indicate save
        if (gm.currentScene < sceneThumbnails.Length)
        {
            var thumbnailImage = sceneThumbnails[gm.currentScene].GetComponent<Image>();
            Color originalColor = thumbnailImage.color;

            // Flash white briefly
            thumbnailImage.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            thumbnailImage.color = originalColor;
            yield return new WaitForSeconds(0.1f);
            thumbnailImage.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            thumbnailImage.color = originalColor;
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
    private void OnSaveClickAsync()
    {
        Debug.Log("[RM_MainLayout] Starting async save to Firebase...");

        RM_SaveLoad.SaveGameAsync(gm,
            () => {
                Debug.Log("[RM_MainLayout] Save completed successfully!");
                gm.Reset();
            },
            error => {
                Debug.LogError($"[RM_MainLayout] Save failed: {error}");
            }
        );
    }
#endif

    public void OnRevertClick()
    {
        Debug.Log("Revert button clicked");

        var warningLayout = gm.warningLayout.GetComponent<RM_WarningLayout>();
        warningLayout.targetScene = gm.currentScene;
        warningLayout.isRevertMode = true;
        warningLayout.messageText.text = "TU ANNULES LES MODIFICATIONS\nAttention Rody, les modifications non sauvegardées seront perdues ! Es-tu sûr de vouloir continuer ?";

        UnsetLayouts(gm.mainLayout);
        SetLayouts(gm.warningLayout);
    }

    public void OnSceneThumbnailClick(int scene)
    {
        int scenesCount = PlayerPrefs.GetInt("scenesCount");
        Debug.Log($"[RM_MainLayout] OnSceneThumbnailClick({scene}) - currentScene: {gm.currentScene}, scenesCount: {scenesCount}");
        RM_WarningLayout warningLayout = gm.warningLayout.GetComponent<RM_WarningLayout>();

        string strChangeScene = "TU CHANGES DE SCENE\nAttention Rody, les modifications non sauvegardées seront perdues ! Es-tu sûr de vouloir continuer ?";
        string strRemoveScene = "TU SUPPRIMES LA SCENE " + scene + "\nAttention Rody, cela va effacer définitivement la scène ! Es-tu sûr de vouloir continuer ?";
        string strCancelScene = "TU ANNULES CETTE NOUVELLE SCENE\nAttention Rody, cela va effacer la scène ! Es-tu sûr de vouloir continuer ?";
        string strNewScene    = "TU AJOUTES UNE NOUVELLE SCENE\nAttention Rody, les modifications non sauvegardées seront perdues ! Es-tu sûr de vouloir continuer ?";

        if (scene == gm.currentScene) {
            // Clicking on current scene: >= 18 means delete/cancel, < 18 does nothing
            if (scene >= 18) {
                warningLayout.isDeleteMode = true;
                if (scene > scenesCount) {
                    Debug.Log($"[RM_MainLayout] Action: CANCEL new scene (scene {scene} > scenesCount {scenesCount})");
                    warningLayout.messageText.text = strCancelScene;
                }
                else {
                    Debug.Log($"[RM_MainLayout] Action: DELETE scene {scene} (scene <= scenesCount {scenesCount})");
                    warningLayout.messageText.text = strRemoveScene;
                }
            }
            else {
                // Scenes 1-17 cannot be deleted by clicking
                Debug.Log($"[RM_MainLayout] Action: NONE (clicking current scene {scene} < 18, returning)");
                return;
            }
        }
        else if (scene > scenesCount) {
            // Adding a new scene
            Debug.Log($"[RM_MainLayout] Action: ADD new scene (scene {scene} > scenesCount {scenesCount})");
            warningLayout.messageText.text = strNewScene;
        }
        else {
            // Changing to a different existing scene
            Debug.Log($"[RM_MainLayout] Action: CHANGE to scene {scene}");
            warningLayout.messageText.text = strChangeScene;
        }

        warningLayout.targetScene = scene;
        Debug.Log($"[RM_MainLayout] Showing warning dialog, targetScene set to {scene}");
        UnsetLayouts(gm.mainLayout);
        SetLayouts(gm.warningLayout);
    }

    public void UpdateActiveThumbnail() {
        for (int i = 0; i < 30; i++) {
            sceneThumbnails[i].GetComponent<Image>().color = inactiveSceneColor;
        }
        sceneThumbnails[gm.currentScene].GetComponent<Image>().color = activeSceneColor;
        UpdateThumbnailPositions((int)thumbnailSlider.value);
    }

    public void OnThumbnailSliderChanged()
    {
        UpdateThumbnailPositions((int)thumbnailSlider.value);
    }

    public void UpdateThumbnailPositions(int sliderValue)
    {
        for (int i = 0; i < 5; ++i) // for each row
        {
            for (int j = 6*i; j < 6*i+6; ++j) // 6 thumbnails per row
            {
                if (j < 6 * sliderValue || j > 6 * sliderValue + 17 || j > PlayerPrefs.GetInt("scenesCount") + 1)
                    sceneThumbnails[j].SetActive(false);
                else
                {
                    if (j <= PlayerPrefs.GetInt("scenesCount") + 1)
                        sceneThumbnails[j].SetActive(true);
                    Vector3 pos = sceneThumbnails[j].GetComponent<Transform>().localPosition;
                    sceneThumbnails[j].GetComponent<Transform>().localPosition = new Vector3(pos.x, 22.5f - ((i-sliderValue) * 22.0f), pos.z);
                }
            }
        }
    }
}
