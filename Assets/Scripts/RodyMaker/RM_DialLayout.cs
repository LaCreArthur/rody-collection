using UnityEngine;
using UnityEngine.UI;

public class RM_DialLayout : RM_Layout
{
    public GameObject isMasticoBtn;
    public Button phonemsBtn, returnBtn;
    public InputField textInputField;
    public Sprite masticoUnmute, masticoMute;

    int activeDial = 1;
    SpeechDocument Dialogue => activeDial == 1 ? gm.CurrentScene.dialogues.intro1
        : activeDial == 2 ? gm.CurrentScene.dialogues.intro2 : gm.CurrentScene.dialogues.intro3;
    float Pitch => activeDial == 1 ? gm.CurrentScene.voice.pitch1
        : activeDial == 2 ? gm.CurrentScene.voice.pitch2 : gm.CurrentScene.voice.pitch3;
    bool IsMastico => activeDial == 1 ? gm.CurrentScene.voice.isMastico1
        : activeDial == 2 ? gm.CurrentScene.voice.isMastico2 : gm.CurrentScene.voice.isMastico3;
    string DisplayText => activeDial == 1 ? gm.CurrentScene.texts.intro1
        : activeDial == 2 ? gm.CurrentScene.texts.intro2 : gm.CurrentScene.texts.intro3;

    protected override void Awake()
    {
        base.Awake();
        textInputField.interactable = false;
        textInputField.onValueChanged.AddListener(SetText);
        textInputField.onEndEdit.AddListener(_ => StoryRoot.FlushWorkspace());
    }

    public void Bind(int dialogue)
    {
        activeDial = dialogue;
        textInputField.SetTextWithoutNotify(DisplayText);
        updateMasticoSprite();
    }

    void SetText(string text)
    {
        if (!gm.CanEdit || DisplayText == text) return;
        var texts = gm.CurrentScene.texts;
        if (activeDial == 1) texts.intro1 = text;
        else if (activeDial == 2) texts.intro2 = text;
        else texts.intro3 = text;
        StoryRoot.Session.NotifyEdited();
        gm.RefreshText();
    }

    public void RM_ReturnClick()
    {
        if (!gm.CanEdit) return;
        SetLayouts(gm.dialoguesLayout, gm.introTextObj);
        UnsetLayouts(gm.dialLayout);
        gm.dialoguesLayout.GetComponent<RM_DialoguesLayout>().SetDialButtons();
    }

    public void RM_PhonemesClick()
    {
        if (!gm.CanEdit) return;
        phonemsBtn.interactable = false;
        bool mastico = IsMastico;
        float voicePitch = mastico ? (gm.CurrentScene.voice.isZambla ? 0.9f : 1f) : Pitch;
        SynthManager.Open(Dialogue, voicePitch, !mastico, "INTRO · DIALOGUE " + activeDial,
            (speech, pitch) =>
            {
                if (SameSpeech(Dialogue, speech) && (mastico || Pitch == pitch)) return;
                var scene = gm.CurrentScene;
                switch (activeDial)
                {
                    case 1: scene.dialogues.intro1 = speech; if (!mastico) scene.voice.pitch1 = pitch; break;
                    case 2: scene.dialogues.intro2 = speech; if (!mastico) scene.voice.pitch2 = pitch; break;
                    case 3: scene.dialogues.intro3 = speech; if (!mastico) scene.voice.pitch3 = pitch; break;
                }
                StoryRoot.Session.NotifyEdited();
                StoryRoot.FlushWorkspace();
            }, () => phonemsBtn.interactable = true);
    }

    public void RM_TextClick()
    {
        if (!gm.CanEdit) return;
        textInputField.interactable = !textInputField.interactable;
        phonemsBtn.interactable = returnBtn.interactable = isMasticoBtn.GetComponent<Button>().interactable
            = !textInputField.interactable;
    }

    public void RM_IsMasticoClick()
    {
        if (!gm.CanEdit) return;
        var voice = gm.CurrentScene.voice;
        if (activeDial == 1) voice.isMastico1 = !voice.isMastico1;
        else if (activeDial == 2) voice.isMastico2 = !voice.isMastico2;
        else voice.isMastico3 = !voice.isMastico3;
        StoryRoot.Session.NotifyEdited();
        updateMasticoSprite();
    }

    public void updateMasticoSprite() => isMasticoBtn.GetComponent<Image>().sprite = IsMastico ? masticoUnmute : masticoMute;
}
