using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RM_MainLayout : RM_Layout
{
    public Button musicButton, scenesButton, imagesButton, testButton, saveButton, discardButton;

    protected override void Awake()
    {
        base.Awake();
        musicButton.onClick.AddListener(() => gm.ShowPanel(RM_Panel.Music));
        scenesButton.onClick.AddListener(() => gm.ShowPanel(RM_Panel.Scenes));
        imagesButton.onClick.AddListener(() => gm.ShowPanel(RM_Panel.Images));
        testButton.onClick.AddListener(OnTestClick);
        saveButton.onClick.AddListener(OnSaveClick);
        discardButton.onClick.AddListener(OnRevertClick);
    }

    void OnEnable() => StoryRoot.StateChanged += UpdateButtonStates;
    void OnDisable() => StoryRoot.StateChanged -= UpdateButtonStates;

    public void UpdateButtonStates()
    {
        bool editable = gm.CanEdit;
        musicButton.interactable = editable && StoryRoot.Session.EditorSceneIndex != 0;
        scenesButton.interactable = imagesButton.interactable = testButton.interactable = saveButton.interactable = editable;
        discardButton.interactable = editable && StoryRoot.Session.IsDirty;
    }

    public void ShowBaseImage()
    {
        int scene = StoryRoot.Session.EditorSceneIndex;
        gm.scenePreview.sprite = scene == 0
            ? StoryRoot.Session.LoadSprite(SpriteCache.TitleName, 320, 200)
            : StoryRoot.Session.LoadSprite(SpriteCache.SceneFrameName(scene, 1));
    }

    public void OnTestClick()
    {
        if (!gm.CanEdit) return;
        gm.workspace.FinishFocus();
        var session = StoryRoot.Session;
        session.ActivateWorkspace();
        session.CurrentSceneIndex = session.EditorSceneIndex;
        StoryRoot.FlushWorkspace();
        SceneManager.LoadScene(session.EditorSceneIndex == 0 ? AppScenes.Title : AppScenes.Game);
    }

    public void OnSaveClick()
    {
        if (!gm.CanEdit) return;
        gm.workspace.FinishFocus();
        StoryRoot.SaveWorkspace();
    }

    public void OnRevertClick()
    {
        if (!gm.CanEdit) return;
        StoryRoot.DiscardWorkspace(gm.Refresh);
    }
}
