using System;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RM_DialLayout : MonoBehaviour
{
    public RM_TextInputField titleInput, objectiveInput;
    public RM_TextInputField[] introInputs = new RM_TextInputField[3];
    public RectTransform body;
    public Button[] passageButtons = new Button[6];
    public Button voiceButton, doneButton, speakerButton, returnButton;
    public Sprite masticoUnmute, masticoMute, targetSprite;

    const float HomeWidth = 224f, ContextWidth = 252.6f, BodyHeight = 48f;
    static readonly Color SelectorHighlight = new Color(1f, .78f, .35f, 1f);
    readonly TextGenerator composition = new TextGenerator();
    readonly TextGenerator measurement = new TextGenerator();
    readonly int[] starts = new int[3];
    readonly Color[] selectorColors = new Color[6];
    RM_GameManager gm;
    RectTransform workspaceRect;
    float left;
    bool voiceOpen;

    public void Initialize(RM_GameManager manager)
    {
        gm = manager;
        workspaceRect = GetComponent<RectTransform>();
        left = workspaceRect.anchoredPosition.x - workspaceRect.rect.width * .5f;
        Configure(titleInput, true, () =>
        {
            if (gm.Panel != RM_Panel.Title) gm.EditTitle();
        }, value => Fits(value, titleInput.textComponent, new Vector2(HomeWidth, 11.6f), true)
            && Fits(value, titleInput.textComponent, new Vector2(255.1f, 11.6f), true),
            value => SetText(-1, value));

        for (int i = 0; i < introInputs.Length; i++)
        {
            int passage = i;
            Configure(introInputs[i], false, () => ActivatePassage(passage),
                value => FitsBody(ComposeIntro(passage, value)), value => SetText(passage, value));
        }
        Configure(objectiveInput, false, () => ActivatePassage((int)gm.Passage),
            FitsBody, value => SetText((int)gm.Passage, value));
        objectiveInput.textComponent.alignment = TextAnchor.MiddleCenter;

        for (int i = 0; i < passageButtons.Length; i++)
        {
            int passage = i;
            selectorColors[i] = passageButtons[i].image.color;
            passageButtons[i].transition = Selectable.Transition.None;
            passageButtons[i].onClick.AddListener(() =>
                gm.SelectPassage(passage, ReadText(gm.CurrentScene, passage).Length == 0));
        }
        voiceButton.onClick.AddListener(EditVoice);
        speakerButton.onClick.AddListener(ChangeSpeakerOrTarget);
        doneButton.onClick.AddListener(gm.ReturnHome);
        returnButton.onClick.AddListener(gm.ReturnHome);
    }

    void Configure(RM_TextInputField field, bool title, Action activate,
        Func<string, bool> fits, UnityAction<string> changed)
    {
        field.contentType = InputField.ContentType.Custom;
        field.characterLimit = 0;
        field.characterValidation = InputField.CharacterValidation.None;
        field.onValidateInput = null;
        field.lineType = title ? InputField.LineType.SingleLine : InputField.LineType.MultiLineNewline;
        field.transition = Selectable.Transition.None;
        field.shouldActivateOnSelect = false;
        field.readOnly = true;
        field.Fits = fits;
        field.OnActivate = activate;
        field.OnEscape = gm.HandleEscape;
        field.OnHover = entered =>
        {
            if (entered)
            {
                gm.pointer.Show(field, RM_EditorPointer.Kind.Text);
                if (field.readOnly) gm.tooltip.Show("Modifier ce texte", (RectTransform)field.transform);
            }
            else
            {
                gm.pointer.Clear(field);
                gm.tooltip.HideFor((RectTransform)field.transform);
            }
        };
        field.OnCrop = () => gm.tooltip.ShowBrief("La fin ajoutée a été coupée pour tenir dans le cadre.",
            (RectTransform)field.transform);
        field.onValueChanged.AddListener(changed);
        field.onEndEdit.AddListener(_ => StoryRoot.FlushWorkspace());
        var label = field.textComponent;
        label.fontSize = 15;
        label.lineSpacing = .6f;
        label.supportRichText = false;
        label.resizeTextForBestFit = false;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.alignment = title ? TextAnchor.MiddleCenter : TextAnchor.UpperCenter;
        label.raycastTarget = false;
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.pivot = new Vector2(.5f, .5f);
        label.rectTransform.anchoredPosition = Vector2.zero;
        label.rectTransform.sizeDelta = Vector2.zero;
    }

    public void Refresh()
    {
        float width = gm.Panel == RM_Panel.Home ? HomeWidth : ContextWidth;
        workspaceRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        workspaceRect.anchoredPosition = new Vector2(left + width * .5f, workspaceRect.anchoredPosition.y);
        SetRect((RectTransform)titleInput.transform, new Vector2(0, 28), new Vector2(width, 12));
        SetRect(body, new Vector2(0, -2), new Vector2(width, BodyHeight));

        bool hasScene = gm.CurrentScene != null;
        body.gameObject.SetActive(hasScene);
        passageButtons[0].transform.parent.gameObject.SetActive(hasScene && gm.Panel != RM_Panel.Zones);
        gm.zones.paddingLabel.gameObject.SetActive(hasScene && gm.Panel == RM_Panel.Zones);
        titleInput.SetTextWithoutNotify(hasScene ? gm.CurrentScene.texts.title : "ECRAN TITRE");
        bool canSelect = hasScene && gm.CanEdit && gm.Panel != RM_Panel.Zones;
        SetFieldState(titleInput, canSelect, gm.Panel == RM_Panel.Title, gm.Panel == RM_Panel.Title);
        voiceButton.gameObject.SetActive(hasScene && gm.Panel != RM_Panel.Title);
        speakerButton.gameObject.SetActive(hasScene && gm.Panel != RM_Panel.Title);
        if (!hasScene) return;

        for (int i = 0; i < passageButtons.Length; i++)
        {
            passageButtons[i].interactable = canSelect;
            passageButtons[i].image.color = i == (int)gm.Passage ? SelectorHighlight : selectorColors[i];
        }
        if (gm.IsObjective)
        {
            foreach (var field in introInputs) field.gameObject.SetActive(false);
            objectiveInput.gameObject.SetActive(true);
            objectiveInput.SetTextWithoutNotify(ReadText(gm.CurrentScene, (int)gm.Passage));
            SetRect((RectTransform)objectiveInput.transform, Vector2.zero, new Vector2(width, BodyHeight));
            SetFieldState(objectiveInput, canSelect, gm.Panel == RM_Panel.Text, true);
        }
        else
        {
            objectiveInput.gameObject.SetActive(false);
            LayoutIntroduction(width, canSelect);
        }
        voiceButton.interactable = gm.CanEdit && !voiceOpen;
        speakerButton.interactable = gm.CanEdit && !voiceOpen;
        speakerButton.image.sprite = gm.IsObjective ? targetSprite
            : IsMastico(gm.CurrentScene.voice, (int)gm.Passage) ? masticoUnmute : masticoMute;
        speakerButton.GetComponent<RM_ButtonTooltip>().SetText(gm.IsObjective ? "Dessiner la cible"
            : IsMastico(gm.CurrentScene.voice, (int)gm.Passage) ? "Parole de Mastico · changer de personnage"
            : "Parole du personnage · faire parler Mastico");
        voiceButton.GetComponent<RM_ButtonTooltip>().SetText(gm.IsObjective ? "Modifier la voix de cet objectif"
            : "Modifier la voix de la réplique " + ((int)gm.Passage + 1));
    }

    void LayoutIntroduction(float width, bool canSelect)
    {
        string combined = ComposeIntro(-1, null, starts);
        var reference = introInputs[0].textComponent;
        var settings = reference.GetGenerationSettings(new Vector2(width, BodyHeight));
        settings.textAnchor = TextAnchor.MiddleCenter;
        settings.pivot = new Vector2(.5f, .5f);
        composition.Populate(combined, settings);

        for (int i = 0; i < introInputs.Length; i++)
        {
            var field = introInputs[i];
            string value = ReadText(gm.CurrentScene, i);
            bool selected = i == (int)gm.Passage;
            field.gameObject.SetActive(value.Length > 0 || selected);
            field.SetTextWithoutNotify(value);
            SetFieldState(field, canSelect, gm.Panel == RM_Panel.Text && selected, selected);
            if (value.Length == 0)
            {
                // The numbered selector owns an empty passage. Focus it without
                // adding a blank line or taking space from the composed text.
                SetRect((RectTransform)field.transform, Vector2.zero, new Vector2(width, 0));
                continue;
            }
            int line = 0;
            while (line + 1 < composition.lineCount && composition.lines[line + 1].startCharIdx <= starts[i]) line++;
            var blockSettings = field.textComponent.GetGenerationSettings(new Vector2(width, BodyHeight));
            float height = measurement.GetPreferredHeight(value, blockSettings) / field.textComponent.pixelsPerUnit;
            SetRect((RectTransform)field.transform, Vector2.zero, new Vector2(width, height));
            blockSettings = field.textComponent.GetGenerationSettings(new Vector2(width, height));
            measurement.Populate(value, blockSettings);
            float y = composition.lines[line].topY / reference.pixelsPerUnit
                - measurement.lines[0].topY / field.textComponent.pixelsPerUnit;
            ((RectTransform)field.transform).anchoredPosition = new Vector2(0, y);
        }
    }

    static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    static void SetFieldState(RM_TextInputField field, bool canSelect, bool edit, bool highlight)
    {
        field.interactable = canSelect;
        field.readOnly = !edit;
        field.ShowSelection(highlight);
    }

    void ActivatePassage(int passage)
    {
        if (gm.Panel != RM_Panel.Text || (int)gm.Passage != passage) gm.SelectPassage(passage, true);
    }

    public void FocusPassage()
    {
        if (gm.Panel != RM_Panel.Text || gm.CurrentScene == null) return;
        var field = gm.IsObjective ? objectiveInput : introInputs[(int)gm.Passage];
        field.FocusAt(field.text.Length);
    }

    public void FocusTitle() => titleInput.FocusAt(titleInput.text.Length);

    public void FinishFocus()
    {
        if (EventSystem.current.currentSelectedGameObject != null
            && EventSystem.current.currentSelectedGameObject.transform.IsChildOf(transform))
            EventSystem.current.SetSelectedGameObject(null);
        titleInput.DeactivateInputField();
        objectiveInput.DeactivateInputField();
        foreach (var field in introInputs) field.DeactivateInputField();
    }

    bool FitsBody(string value) => Fits(value, introInputs[0].textComponent, new Vector2(HomeWidth, BodyHeight))
        && Fits(value, introInputs[0].textComponent, new Vector2(ContextWidth, 54.45f));

    bool Fits(string value, Text label, Vector2 bounds, bool singleLine = false)
    {
        var settings = label.GetGenerationSettings(bounds);
        settings.horizontalOverflow = singleLine ? HorizontalWrapMode.Overflow : HorizontalWrapMode.Wrap;
        settings.verticalOverflow = VerticalWrapMode.Overflow;
        if (singleLine && measurement.GetPreferredWidth(value, settings) / label.pixelsPerUnit > bounds.x + .001f)
            return false;
        return measurement.GetPreferredHeight(value, settings) / label.pixelsPerUnit <= bounds.y + .001f;
    }

    string ComposeIntro(int replacement, string value, int[] positions = null)
    {
        var combined = new StringBuilder();
        for (int i = 0; i < 3; i++)
        {
            string part = i == replacement ? value : ReadText(gm.CurrentScene, i);
            if (positions != null) positions[i] = -1;
            if (part.Length == 0) continue;
            if (combined.Length > 0) combined.Append('\n');
            if (positions != null) positions[i] = combined.Length;
            combined.Append(part);
        }
        return combined.ToString();
    }

    void SetText(int passage, string value)
    {
        if (!gm.CanEdit || gm.CurrentScene == null || ReadText(gm.CurrentScene, passage) == value) return;
        var texts = gm.CurrentScene.texts;
        switch (passage)
        {
            case -1: texts.title = value; break;
            case 0: texts.intro1 = value; break;
            case 1: texts.intro2 = value; break;
            case 2: texts.intro3 = value; break;
            case 3: texts.obj = value; break;
            case 4: texts.ngp = value; break;
            case 5: texts.fsw = value; break;
        }
        StoryRoot.Session.NotifyEdited();
        gm.RefreshText();
    }

    static string ReadText(SceneData scene, int passage) => (passage switch
    {
        -1 => scene.texts.title,
        0 => scene.texts.intro1,
        1 => scene.texts.intro2,
        2 => scene.texts.intro3,
        3 => scene.texts.obj,
        4 => scene.texts.ngp,
        _ => scene.texts.fsw
    }) ?? "";

    void ChangeSpeakerOrTarget()
    {
        if (!gm.CanEdit || gm.CurrentScene == null) return;
        FinishFocus();
        if (gm.IsObjective) { gm.zones.BeginEdit(); return; }
        var voice = gm.CurrentScene.voice;
        switch ((int)gm.Passage)
        {
            case 0: voice.isMastico1 = !voice.isMastico1; break;
            case 1: voice.isMastico2 = !voice.isMastico2; break;
            case 2: voice.isMastico3 = !voice.isMastico3; break;
        }
        StoryRoot.Session.NotifyEdited();
        StoryRoot.FlushWorkspace();
        Refresh();
    }

    void EditVoice()
    {
        if (!gm.CanEdit || gm.CurrentScene == null || voiceOpen) return;
        FinishFocus();
        var scene = gm.CurrentScene;
        int passage = (int)gm.Passage;
        bool mastico = passage >= 3 || IsMastico(scene.voice, passage);
        float pitch = mastico ? scene.voice.isZambla ? .9f : 1f : ReadPitch(scene.voice, passage);
        voiceOpen = true;
        voiceButton.interactable = false;
        SynthManager.Open(ReadSpeech(scene, passage), pitch, !mastico,
            passage >= 3 ? "MASTICO · CONSIGNE" : "INTRO · DIALOGUE " + (passage + 1),
            (speech, editedPitch) =>
            {
                if (SameSpeech(ReadSpeech(scene, passage), speech) && (mastico || ReadPitch(scene.voice, passage) == editedPitch)) return;
                switch (passage)
                {
                    case 0: scene.dialogues.intro1 = speech; if (!mastico) scene.voice.pitch1 = editedPitch; break;
                    case 1: scene.dialogues.intro2 = speech; if (!mastico) scene.voice.pitch2 = editedPitch; break;
                    case 2: scene.dialogues.intro3 = speech; if (!mastico) scene.voice.pitch3 = editedPitch; break;
                    case 3: scene.dialogues.obj = speech; break;
                    case 4: scene.dialogues.ngp = speech; break;
                    case 5: scene.dialogues.fsw = speech; break;
                }
                StoryRoot.Session.NotifyEdited();
                StoryRoot.FlushWorkspace();
            }, () => { voiceOpen = false; Refresh(); });
    }

    static SpeechDocument ReadSpeech(SceneData scene, int passage) => passage switch
    {
        0 => scene.dialogues.intro1,
        1 => scene.dialogues.intro2,
        2 => scene.dialogues.intro3,
        3 => scene.dialogues.obj,
        4 => scene.dialogues.ngp,
        _ => scene.dialogues.fsw
    };

    static bool IsMastico(VoiceSettings voice, int passage) => passage == 0 ? voice.isMastico1
        : passage == 1 ? voice.isMastico2 : voice.isMastico3;
    static float ReadPitch(VoiceSettings voice, int passage) => passage == 0 ? voice.pitch1
        : passage == 1 ? voice.pitch2 : voice.pitch3;

    static bool SameSpeech(SpeechDocument left, SpeechDocument right)
    {
        if (left.sourceText != right.sourceText || left.words.Count != right.words.Count) return false;
        for (int i = 0; i < left.words.Count; i++)
        {
            var a = left.words[i];
            var b = right.words[i];
            if (a.start != b.start || a.length != b.length || a.score != b.score || a.corrected != b.corrected) return false;
        }
        return true;
    }

    void OnDestroy()
    {
        ((IDisposable)composition).Dispose();
        ((IDisposable)measurement).Dispose();
    }
}
