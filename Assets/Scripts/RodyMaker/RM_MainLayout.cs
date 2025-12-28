using System.Collections;
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

    /// <summary>
    /// Loads sprites from WorkingStory for editor display.
    /// </summary>
    public void LoadSprites()
    {
        int sceneCount = WorkingStory.SceneCount;
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
            // Load scene sprites from WorkingStory
            var sceneSprites = RM_SaveLoad.LoadSceneSprites(gm.currentScene);
            if (sceneSprites.Count > 0)
            {
                gm.scenePanel.GetComponent<SpriteRenderer>().sprite = sceneSprites[0];
            }
            else
            {
                // New scene with no sprites - clear the panel
                gm.scenePanel.GetComponent<SpriteRenderer>().sprite = null;
            }

            gm.framesCount = Mathf.Max(0, sceneSprites.Count - 1);

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
        // Save works on all platforms now via WorkingStory (in-memory)
        RM_SaveLoad.SaveGame(gm);
        gm.Reset();
        StartCoroutine(ShowSaveFeedback());
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
        int scenesCount = WorkingStory.SceneCount;
        Debug.Log($"[RM_MainLayout] OnSceneThumbnailClick({scene}) - currentScene: {gm.currentScene}, scenesCount: {scenesCount}");
        RM_WarningLayout warningLayout = gm.warningLayout.GetComponent<RM_WarningLayout>();

        string strChangeScene = "TU CHANGES DE SCENE\nAttention Rody, les modifications non sauvegardées seront perdues ! Es-tu sûr de vouloir continuer ?";
        string strRemoveScene = "TU SUPPRIMES LA SCENE " + scene + "\nAttention Rody, cela va effacer définitivement la scène ! Es-tu sûr de vouloir continuer ?";
        string strCancelScene = "TU ANNULES CETTE NOUVELLE SCENE\nAttention Rody, cela va effacer la scène ! Es-tu sûr de vouloir continuer ?";
        string strNewScene    = "TU AJOUTES UNE NOUVELLE SCENE\nAttention Rody, les modifications non sauvegardées seront perdues ! Es-tu sûr de vouloir continuer ?";

        if (scene == gm.currentScene) {
            // Clicking on current scene: >= 18 means delete/cancel, < 18 does nothing
            if (scene >= 18) {
                // Check if scene has sprites - empty scenes shouldn't trigger delete on click
                var sceneSprites = RM_SaveLoad.LoadSceneSprites(scene);
                bool hasContent = sceneSprites != null && sceneSprites.Count > 0;

                if (scene > scenesCount) {
                    // Scene beyond scenesCount - cancel new scene creation
                    warningLayout.isDeleteMode = true;
                    Debug.Log($"[RM_MainLayout] Action: CANCEL new scene (scene {scene} > scenesCount {scenesCount})");
                    warningLayout.messageText.text = strCancelScene;
                }
                else if (hasContent) {
                    // Scene with content - offer to delete
                    warningLayout.isDeleteMode = true;
                    Debug.Log($"[RM_MainLayout] Action: DELETE scene {scene} (scene <= scenesCount {scenesCount})");
                    warningLayout.messageText.text = strRemoveScene;
                }
                else {
                    // Empty scene (no sprites) - just ignore the click
                    Debug.Log($"[RM_MainLayout] Action: NONE (scene {scene} is empty, ignoring click)");
                    return;
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
                if (j < 6 * sliderValue || j > 6 * sliderValue + 17 || j > WorkingStory.SceneCount + 1)
                    sceneThumbnails[j].SetActive(false);
                else
                {
                    if (j <= WorkingStory.SceneCount + 1)
                        sceneThumbnails[j].SetActive(true);
                    Vector3 pos = sceneThumbnails[j].GetComponent<Transform>().localPosition;
                    sceneThumbnails[j].GetComponent<Transform>().localPosition = new Vector3(pos.x, 22.5f - ((i-sliderValue) * 22.0f), pos.z);
                }
            }
        }
    }
}
