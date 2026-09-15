using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RM_SceneBrowser : MonoBehaviour
{
    public ScrollRect scroll;
    public Button cardTemplate, previousButton, nextButton, addButton, returnButton;
    readonly List<Button> cards = new List<Button>();
    RM_GameManager gm;

    public void Initialize(RM_GameManager owner)
    {
        gm = owner;
        previousButton.onClick.AddListener(() => gm.SelectScene(StoryRoot.Session.EditorSceneIndex - 1));
        nextButton.onClick.AddListener(() => gm.SelectScene(StoryRoot.Session.EditorSceneIndex + 1));
        returnButton.onClick.AddListener(gm.ReturnHome);
        addButton.onClick.AddListener(AddScene);
    }

    public void Refresh()
    {
        foreach (var card in cards) { card.gameObject.SetActive(false); Destroy(card.gameObject); }
        cards.Clear();
        var session = StoryRoot.Session;
        int selected = session.EditorSceneIndex;
        for (int i = 0; i <= session.Draft.scenes.Count; i++)
        {
            int scene = i;
            var card = Instantiate(cardTemplate, scroll.content);
            card.name = "Scene " + scene;
            card.gameObject.SetActive(true);
            string title = scene == 0 ? "ECRAN-TITRE" : scene + ". " + session.LoadDraftScene(scene).texts.title;
            var preview = card.transform.Find("Preview").GetComponent<Image>();
            preview.sprite = scene == 0 ? session.LoadSprite(SpriteCache.TitleName, 320, 200)
                : session.LoadSprite(SpriteCache.SceneFrameName(scene, 1));
            card.GetComponent<RM_ButtonTooltip>().SetText(title);
            card.GetComponent<Image>().color = scene == selected ? new Color(1f, .91f, .58f) : new Color(.79f, .72f, .48f);
            card.onClick.AddListener(() => gm.SelectScene(scene));
            var delete = card.transform.Find("Delete").GetComponent<Button>();
            delete.gameObject.SetActive(scene == selected && scene >= 18);
            delete.onClick.AddListener(() => DeleteScene(scene));
            cards.Add(card);
        }
        previousButton.interactable = selected > 0;
        nextButton.interactable = selected < session.Draft.scenes.Count;
        addButton.interactable = session.Draft.scenes.Count < 29;
        LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
        var grid = scroll.content.GetComponent<GridLayoutGroup>();
        float row = selected / grid.constraintCount;
        float contentHeight = scroll.content.rect.height;
        float viewportHeight = scroll.viewport.rect.height;
        float top = Mathf.Clamp(row * (grid.cellSize.y + grid.spacing.y), 0, Mathf.Max(0, contentHeight - viewportHeight));
        scroll.verticalNormalizedPosition = contentHeight <= viewportHeight ? 1 : 1 - top / (contentHeight - viewportHeight);
    }

    void AddScene()
    {
        if (!gm.CanEdit || StoryRoot.Session.Draft.scenes.Count >= 29) return;
        var session = StoryRoot.Session;
        int index = session.Draft.scenes.Count + 1;
        session.CreateNewScene(index);
        session.EditorPassage = EditorPassage.Intro1;
        gm.SelectScene(index);
        gm.EditTitle();
        gm.workspace.FocusTitle();
    }

    void DeleteScene(int index)
    {
        if (!gm.CanEdit || index < 18 || index != StoryRoot.Session.EditorSceneIndex) return;
        string title = StoryRoot.Session.LoadDraftScene(index).texts.title;
        StoryRoot.Confirm("Supprimer la scène " + index + " : «" + title + "» et ses images ?", () =>
        {
            StoryRoot.Session.DeleteScene(index);
            gm.SelectScene(index - 1);
        });
    }
}
