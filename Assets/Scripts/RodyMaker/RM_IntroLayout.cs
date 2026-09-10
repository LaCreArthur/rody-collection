using UnityEngine.UI;

public class RM_IntroLayout : RM_Layout
{
    public InputField titleInputField;
    public Button returnBtn, musicBtn, dialoguesBtn;

    protected override void Awake()
    {
        base.Awake();
        titleInputField.interactable = false;
        titleInputField.onValueChanged.AddListener(SetTitle);
        titleInputField.onEndEdit.AddListener(_ => StoryRoot.FlushWorkspace());
    }

    public void Bind() => titleInputField.SetTextWithoutNotify(gm.CurrentScene.texts.title);

    void SetTitle(string text)
    {
        if (!gm.CanEdit || gm.CurrentScene.texts.title == text) return;
        gm.CurrentScene.texts.title = text;
        StoryRoot.Session.NotifyEdited();
        gm.RefreshText();
    }

    public void TextClick()
    {
        if (!gm.CanEdit) return;
        titleInputField.interactable = !titleInputField.interactable;
        returnBtn.interactable = musicBtn.interactable = dialoguesBtn.interactable = !titleInputField.interactable;
    }

    public void RM_ReturnClick()
    {
        if (!gm.CanEdit) return;
        SetLayouts(gm.mainLayout);
        UnsetLayouts(gm.introTextObj, gm.title, gm.introLayout);
    }

    public void RM_MusicClick()
    {
        if (!gm.CanEdit) return;
        SetLayouts(gm.introTextObj, gm.title, gm.musicLayout);
        UnsetLayouts(gm.introLayout);
        gm.musicLayout.GetComponent<RM_MusicLayout>().SetMusic();
    }

    public void RM_DialoguesClick()
    {
        if (!gm.CanEdit) return;
        SetLayouts(gm.dialoguesLayout, gm.introTextObj, gm.title);
        UnsetLayouts(gm.introLayout);
        gm.dialoguesLayout.GetComponent<RM_DialoguesLayout>().SetDialButtons();
    }
}
