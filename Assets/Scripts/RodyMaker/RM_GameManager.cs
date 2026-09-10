using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RM_GameManager : MonoBehaviour
{
    public GameObject mainLayout, imagesLayout, imgAnimLayout, introLayout;
    public GameObject dialoguesLayout, dialLayout, objectsLayout, objLayout, musicLayout;
    public GameObject welcomePanel, introTextObj, objTextObj, ngpTextObj, fswTextObj, title;
    public GameObject objNearTemplate, objTemplate;
    public Image scenePreview;
    public SoundManager sm;
    public CanvasGroup editorControls;

    bool ready;

    public SceneData CurrentScene => StoryRoot.Session.EditorSceneIndex == 0 ? null
        : StoryRoot.Session.LoadDraftScene(StoryRoot.Session.EditorSceneIndex);
    public bool CanEdit => ready && !StoryRoot.IsBusy && SceneManager.GetActiveScene() == gameObject.scene;

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
        mainLayout.SetActive(true);
        imagesLayout.SetActive(false);
        imgAnimLayout.SetActive(false);
        introLayout.SetActive(false);
        dialoguesLayout.SetActive(false);
        dialLayout.SetActive(false);
        objectsLayout.SetActive(false);
        objLayout.SetActive(false);
        musicLayout.SetActive(false);
        introTextObj.SetActive(false);
        title.SetActive(false);
        objNearTemplate.SetActive(false);
        objTemplate.SetActive(false);
        welcomePanel.SetActive(PlayerPrefs.GetInt("rodyMakerFirstTime") == 1);
        PlayerPrefs.SetInt("rodyMakerFirstTime", 0);
        ready = true;
        Refresh();
    }

    public void Refresh()
    {
        RefreshText();
        var layout = mainLayout.GetComponent<RM_MainLayout>();
        layout.LoadSprites();
        layout.UpdateActiveThumbnail();
        layout.UpdateButtonStates();
    }

    public void RefreshText()
    {
        var data = CurrentScene;
        title.GetComponent<Text>().text = data == null ? "Titre" : data.texts.title;
        introTextObj.GetComponent<Text>().text = data == null || string.IsNullOrEmpty(data.texts.intro1)
            ? "Dialogues de la scène" : data.texts.intro1;
        objTextObj.GetComponent<Text>().text = data == null ? "" : data.texts.obj;
        ngpTextObj.GetComponent<Text>().text = data == null ? "" : data.texts.ngp;
        fswTextObj.GetComponent<Text>().text = data == null ? "" : data.texts.fsw;
    }

    void Update()
    {
        editorControls.interactable = CanEdit;
        editorControls.blocksRaycasts = CanEdit;
        if (CanEdit && Input.GetKeyUp(KeyCode.Escape))
        {
            StoryRoot.FlushWorkspace();
            SceneManager.LoadScene(AppScenes.Menu);
        }
    }

    public void OnWelcomePanelExit() => welcomePanel.SetActive(false);
    public void OnWelcomePanelWebsite() => Application.OpenURL("https://lacrearthur.itch.io/rody-maker");
    public void OnWelcomePanelYoutube() => Application.OpenURL("https://youtu.be/1vx8D2irVLI?t=2m17s");
}
