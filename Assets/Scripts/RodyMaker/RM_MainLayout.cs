using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RM_MainLayout : RM_Layout
{
    public Color inactiveSceneColor;
    public Color activeSceneColor;
    public GameObject[] sceneThumbnails;
    public Slider thumbnailSlider;
    public Sprite addSceneSprite;
    public Button objectsButton;
    public Button introButton;
    public Button saveButton, discardButton;
    public Button recoveryRetryButton;
    public GameObject saveStatusPanel;
    public Text saveStatusText;

    protected override void Awake()
    {
        base.Awake();
        recoveryRetryButton.onClick.AddListener(StoryRoot.RetryRecovery);
    }

    void OnEnable()
    {
        StoryRoot.StateChanged += UpdateButtonStates;
        float statusHeight = saveStatusPanel.GetComponent<RectTransform>().rect.height;
        gm.scenePreview.rectTransform.sizeDelta = new Vector2(320, 130 - statusHeight);
        gm.scenePreview.rectTransform.anchoredPosition = new Vector2(0, -35 + statusHeight * .5f);
        UpdateButtonStates();
    }

    void OnDisable()
    {
        StoryRoot.StateChanged -= UpdateButtonStates;
        gm.scenePreview.rectTransform.sizeDelta = new Vector2(320, 130);
        gm.scenePreview.rectTransform.anchoredPosition = new Vector2(0, -35);
    }

    public void UpdateButtonStates()
    {
        bool editable = gm.CanEdit;
        objectsButton.interactable = introButton.interactable = editable && StoryRoot.Session.EditorSceneIndex != 0;
        saveButton.interactable = editable;
        discardButton.interactable = editable && StoryRoot.Session.IsDirty;
        saveStatusPanel.SetActive(StoryRoot.Session.HasWorkspace);
        recoveryRetryButton.gameObject.SetActive(StoryRoot.RecoveryError != null);
        recoveryRetryButton.interactable = editable;
        if (StoryRoot.Session.HasWorkspace)
            saveStatusText.text = "Mon histoire : " + StoryRoot.Session.Draft.story.title +
                (StoryRoot.Session.IsDirty ? " — Modifications non enregistrées" : " — Aucune modification") +
                "\n" + (StoryRoot.RecoveryError == null ? "Enregistrer télécharge l’histoire complète."
                    : "Récupération automatique indisponible.");
    }

    public void LoadSprites()
    {
        var session = StoryRoot.Session;
        int count = session.Draft.scenes.Count;
        for (int i = 0; i < sceneThumbnails.Length; i++)
        {
            var thumbnail = sceneThumbnails[i];
            thumbnail.GetComponent<Button>().interactable = i <= count + 1;
            thumbnail.GetComponent<Image>().sprite = i == 0 ? session.LoadSprite(SpriteCache.TitleName, 320, 200)
                : i <= count ? session.LoadSprite(SpriteCache.SceneFrameName(i, 1))
                : i == count + 1 ? addSceneSprite : null;
        }
        ShowBaseImage();
    }

    public void ShowBaseImage()
    {
        int scene = StoryRoot.Session.EditorSceneIndex;
        gm.scenePreview.sprite = scene == 0
            ? StoryRoot.Session.LoadSprite(SpriteCache.TitleName, 320, 200)
            : StoryRoot.Session.LoadSprite(SpriteCache.SceneFrameName(scene, 1));
    }

    public void OnIntroClick()
    {
        if (!gm.CanEdit || StoryRoot.Session.EditorSceneIndex == 0) return;
        SetLayouts(gm.introLayout, gm.introTextObj);
        UnsetLayouts(gm.mainLayout);
        gm.introLayout.GetComponent<RM_IntroLayout>().Bind();
    }

    public void OnImagesClick()
    {
        if (!gm.CanEdit) return;
        SetLayouts(gm.imagesLayout);
        UnsetLayouts(gm.mainLayout);
        gm.imagesLayout.GetComponent<RM_ImagesLayout>().SetActiveBtn();
    }

    public void OnObjectsClick()
    {
        if (!gm.CanEdit || StoryRoot.Session.EditorSceneIndex == 0) return;
        SetLayouts(gm.introTextObj, gm.title, gm.objectsLayout);
        UnsetLayouts(gm.mainLayout);
    }

    public void OnTestClick()
    {
        if (!gm.CanEdit) return;
        var session = StoryRoot.Session;
        session.ActivateWorkspace();
        session.CurrentSceneIndex = session.EditorSceneIndex;
        StoryRoot.FlushWorkspace();
        SceneManager.LoadScene(session.EditorSceneIndex == 0 ? AppScenes.Title : AppScenes.Game);
    }

    public void OnSaveClick()
    {
        if (!gm.CanEdit) return;
        StoryRoot.SaveWorkspace();
    }

    public void OnRevertClick()
    {
        if (!gm.CanEdit) return;
        StoryRoot.DiscardWorkspace(gm.Refresh);
    }

    public void OnSceneThumbnailClick(int scene)
    {
        if (!gm.CanEdit || scene < 0 || scene >= sceneThumbnails.Length) return;
        var session = StoryRoot.Session;
        int count = session.Draft.scenes.Count;
        if (scene > count + 1) return;
        if (scene == session.EditorSceneIndex)
        {
            // Preserve the existing later-scene deletion affordance.
            if (scene < 18) return;
            StoryRoot.Confirm("Supprimer la scène " + scene + " et ses images de cette histoire ?", () =>
            {
                session.DeleteScene(scene);
                session.EditorSceneIndex = scene - 1;
                StoryRoot.FlushWorkspace();
                gm.Refresh();
            });
            return;
        }
        if (scene == count + 1) session.CreateNewScene(scene);
        session.EditorSceneIndex = scene;
        StoryRoot.FlushWorkspace();
        gm.Refresh();
    }

    public void UpdateActiveThumbnail()
    {
        int selected = StoryRoot.Session.EditorSceneIndex;
        for (int i = 0; i < sceneThumbnails.Length; i++)
            sceneThumbnails[i].GetComponent<Image>().color = i == selected ? activeSceneColor : inactiveSceneColor;
        UpdateThumbnailPositions((int)thumbnailSlider.value);
    }

    public void OnThumbnailSliderChanged()
    {
        if (gm.CanEdit) UpdateThumbnailPositions((int)thumbnailSlider.value);
    }

    public void UpdateThumbnailPositions(int sliderValue)
    {
        int last = StoryRoot.Session.HasWorkspace ? StoryRoot.Session.Draft.scenes.Count + 1 : 0;
        for (int i = 0; i < sceneThumbnails.Length; i++)
        {
            bool visible = i >= 6 * sliderValue && i <= 6 * sliderValue + 17 && i <= last;
            sceneThumbnails[i].SetActive(visible);
            if (!visible) continue;
            var transform = sceneThumbnails[i].transform;
            var position = transform.localPosition;
            transform.localPosition = new Vector3(position.x, 22.5f - (i / 6 - sliderValue) * 22f, position.z);
        }
    }
}
