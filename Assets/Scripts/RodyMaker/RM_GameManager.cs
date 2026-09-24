using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum RM_Panel { Home, Text, Title, Zones, Scenes, Images, Frames, Music, Voice }

public class RM_GameManager : MonoBehaviour
{
    public GameObject mainLayout, imagesLayout, imgAnimLayout, dialLayout, objLayout, musicLayout, scenesLayout;
    public GameObject welcomePanel;
    public RM_DialLayout workspace;
    public RM_ObjLayout zones;
    public RM_VoiceLayout voice;
    public RM_SceneBrowser sceneBrowser;
    public Image scenePreview;
    public SoundManager sm;
    public CanvasGroup editorControls;
    public RM_TooltipDisplay tooltip;
    public RM_EditorPointer pointer;
    public RM_Speech Speech { get; private set; }

    bool ready;
    int escapeFrame = -1;
    public RM_Panel Panel { get; private set; }
    public SceneData CurrentScene => StoryRoot.Session.EditorSceneIndex == 0 ? null
        : StoryRoot.Session.LoadDraftScene(StoryRoot.Session.EditorSceneIndex);
    public EditorPassage Passage => StoryRoot.Session.EditorPassage;
    public bool IsObjective => (int)Passage >= 3;
    public bool CanEdit => ready && !StoryRoot.IsBusy && SceneManager.GetActiveScene() == gameObject.scene;
    public bool IsWorkspaceVisible => Panel == RM_Panel.Home || Panel == RM_Panel.Text || Panel == RM_Panel.Title
        || Panel == RM_Panel.Voice || Panel == RM_Panel.Zones;
    ObjectZone SelectedZone => !IsObjective || CurrentScene == null ? null
        : Passage == EditorPassage.Objective ? CurrentScene.objects.obj
        : Passage == EditorPassage.NewGamePlus ? CurrentScene.objects.ngp : CurrentScene.objects.fsw;

    IEnumerator Start()
    {
        editorControls.interactable = false;
        while (!StoryRoot.IsReady) yield return null;
#if UNITY_EDITOR
        if (!StoryRoot.Session.HasWorkspace)
        {
            StoryRoot.RequestWorkspace(StorySession.CreateNewStory("Test"));
            yield break;
        }
#endif
        if (!StoryRoot.Session.HasWorkspace)
        {
            SceneManager.LoadScene(AppScenes.Selection);
            yield break;
        }
        StoryRoot.Session.ActivateWorkspace();
        Speech = RM_Speech.Create();
        workspace.Initialize(this);
        voice.Initialize(this);
        sceneBrowser.Initialize(this);
        zones.CanInteract = () => CanEdit && IsObjective && CurrentScene != null &&
            (Panel == RM_Panel.Home || Panel == RM_Panel.Text || Panel == RM_Panel.Zones);
        zones.OnBeginEdit = () => ShowPanel(RM_Panel.Zones);
        zones.OnEndEdit = () => ShowPanel(RM_Panel.Home);
        welcomePanel.SetActive(PlayerPrefs.GetInt("rodyMakerFirstTime") == 1);
        PlayerPrefs.SetInt("rodyMakerFirstTime", 0);
        ready = true;
        ShowPanel(RM_Panel.Home);
        Refresh();
    }

    public void ShowPanel(RM_Panel panel)
    {
        if (!CanEdit) return;
        if (panel != Panel) workspace.FinishFocus();
        pointer.Reset();
        Panel = panel;
        mainLayout.SetActive(panel == RM_Panel.Home);
        dialLayout.SetActive(panel == RM_Panel.Text || panel == RM_Panel.Title);
        voice.gameObject.SetActive(panel == RM_Panel.Voice);
        objLayout.SetActive(panel == RM_Panel.Zones);
        scenesLayout.SetActive(panel == RM_Panel.Scenes);
        imagesLayout.SetActive(panel == RM_Panel.Images);
        imgAnimLayout.SetActive(panel == RM_Panel.Frames);
        musicLayout.SetActive(panel == RM_Panel.Music);
        workspace.gameObject.SetActive(IsWorkspaceVisible);
        tooltip.Hide();
        if (panel == RM_Panel.Home) mainLayout.GetComponent<RM_MainLayout>().UpdateButtonStates();
        if (panel == RM_Panel.Scenes) sceneBrowser.Refresh();
        if (panel == RM_Panel.Images) imagesLayout.GetComponent<RM_ImagesLayout>().SetActiveBtn();
        if (panel == RM_Panel.Music) musicLayout.GetComponent<RM_MusicLayout>().SetMusic();
        RefreshText();
    }

    public void SelectPassage(int index, bool edit = false)
    {
        if (!CanEdit || zones.IsEditing || CurrentScene == null) return;
        workspace.FinishFocus();
        StoryRoot.Session.EditorPassage = (EditorPassage)index;
        ShowPanel(edit || Panel == RM_Panel.Text ? RM_Panel.Text : RM_Panel.Home);
        RefreshText();
        if (Panel == RM_Panel.Text) workspace.FocusPassage();
        if (IsObjective && Panel == RM_Panel.Home)
            tooltip.ShowBrief("Glisse dans le décor pour dessiner la cible.", scenePreview.rectTransform);
    }

    public void SelectScene(int index)
    {
        if (!CanEdit || zones.IsEditing) return;
        workspace.FinishFocus();
        StoryRoot.Session.EditorSceneIndex = index;
        ShowPanel(RM_Panel.Home);
        Refresh();
        StoryRoot.FlushWorkspace();
    }

    public void EditTitle()
    {
        if (!CanEdit || zones.IsEditing || CurrentScene == null) return;
        ShowPanel(RM_Panel.Title);
    }

    public void Refresh()
    {
        mainLayout.GetComponent<RM_MainLayout>().ShowBaseImage();
        mainLayout.GetComponent<RM_MainLayout>().UpdateButtonStates();
        if (Panel == RM_Panel.Scenes) sceneBrowser.Refresh();
        RefreshText();
    }

    public void RefreshText()
    {
        if (!ready) return;
        if (IsWorkspaceVisible) workspace.Refresh();
        zones.Bind(SelectedZone, CurrentScene != null && IsObjective &&
            (Panel == RM_Panel.Home || Panel == RM_Panel.Text || Panel == RM_Panel.Zones));
    }

    public void ReturnHome()
    {
        if (!CanEdit) return;
        if (zones.IsEditing) { zones.Cancel(); return; }
        ShowPanel(RM_Panel.Home);
        mainLayout.GetComponent<RM_MainLayout>().ShowBaseImage();
        StoryRoot.FlushWorkspace();
    }

    public void HandleEscape()
    {
        if (!CanEdit || escapeFrame == Time.frameCount) return;
        escapeFrame = Time.frameCount;
        if (welcomePanel.activeSelf) { OnWelcomePanelExit(); return; }
        if (zones.IsEditing) { zones.Cancel(); return; }
        if (Panel == RM_Panel.Voice) { voice.Cancel(); return; }
        if (Panel == RM_Panel.Frames) { imgAnimLayout.GetComponent<RM_ImgAnimLayout>().ReturnClick(); return; }
        if (Panel != RM_Panel.Home) { ReturnHome(); return; }
        workspace.FinishFocus();
        StoryRoot.FlushWorkspace();
        SceneManager.LoadScene(AppScenes.Menu);
    }

    void Update()
    {
        editorControls.interactable = CanEdit;
        editorControls.blocksRaycasts = CanEdit;
        pointer.enabled = CanEdit;
        if (Input.GetKeyDown(KeyCode.Escape)) HandleEscape();
    }

    public void OnWelcomePanelExit() => welcomePanel.SetActive(false);
    public void OnWelcomePanelWebsite() => Application.OpenURL("https://lacrearthur.itch.io/rody-maker");
    public void OnWelcomePanelYoutube() => Application.OpenURL("https://youtu.be/1vx8D2irVLI?t=2m17s");
}
