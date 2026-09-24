using System;
using System.Linq;
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
    static readonly Color SelectorHighlight = new Color(.25f, .12f, .04f, 1f), SelectorHighlightText = new Color(1f, .91f, .58f, 1f);
    // The character button cycles Mastico, then these voices from low to high.
    static readonly float[] CharacterPitches = { .8f, .9f, 1.1f, 1.2f, 1.3f };
    readonly TextGenerator composition = new TextGenerator();
    readonly TextGenerator measurement = new TextGenerator();
    readonly int[] starts = new int[3];
    readonly Color[] selectorColors = new Color[6];
    Color selectorTextColor;
    RM_GameManager gm;
    RectTransform workspaceRect;
    float left;

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
            selectorTextColor = passageButtons[i].GetComponentInChildren<Text>().color;
            passageButtons[i].transition = Selectable.Transition.None;
            passageButtons[i].onClick.AddListener(() =>
            {
                bool empty = ReadText(gm.CurrentScene, passage).Length == 0;
                gm.SelectPassage(passage, empty);
                if (empty && passage < 3 && !FitsBody(ComposeIntro(passage, "A")))
                    gm.tooltip.ShowBrief("Plus de place dans le cadre.", (RectTransform)passageButtons[passage].transform);
            });
        }
        voiceButton.onClick.AddListener(OpenVoice);
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
                if (field.readOnly) gm.tooltip.Show("Modifier ce texte", workspaceRect);
            }
            else
            {
                gm.pointer.Clear(field);
                gm.tooltip.HideFor(workspaceRect);
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
        bool canSelect = hasScene && gm.CanEdit && gm.Panel != RM_Panel.Zones && gm.Panel != RM_Panel.Voice;
        SetFieldState(titleInput, canSelect, gm.Panel == RM_Panel.Title, true);
        voiceButton.gameObject.SetActive(hasScene && gm.Panel != RM_Panel.Title);
        speakerButton.gameObject.SetActive(hasScene && gm.Panel != RM_Panel.Title);
        if (!hasScene) return;

        for (int i = 0; i < passageButtons.Length; i++)
        {
            passageButtons[i].interactable = canSelect;
            bool selected = i == (int)gm.Passage;
            passageButtons[i].image.color = selected ? SelectorHighlight : selectorColors[i];
            passageButtons[i].GetComponentInChildren<Text>().color = selected ? SelectorHighlightText : selectorTextColor;
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
        var scene = gm.CurrentScene;
        int passage = (int)gm.Passage;
        bool mastico = gm.IsObjective || IsMastico(scene.voice, passage);
        voiceButton.interactable = gm.CanEdit && ReadText(scene, passage).Length > 0;
        speakerButton.interactable = gm.CanEdit;
        speakerButton.image.sprite = gm.IsObjective ? targetSprite : mastico ? masticoUnmute : masticoMute;
        speakerButton.GetComponent<RM_ButtonTooltip>().SetText(gm.IsObjective ? "Dessiner la cible"
            : mastico ? "Mastico parle · clic : voix d’un personnage"
            : "Personnage, voix " + VoiceName(ReadPitch(scene.voice, passage)) + " · clic : voix suivante");
        voiceButton.GetComponent<RM_ButtonTooltip>().SetText(RM_Speech.IsOriginal(ReadSpeech(scene, passage))
            ? "Écouter la voix de 1988" : "Écouter et corriger la prononciation");
    }

    static string VoiceName(float pitch) => pitch < .85f ? "très grave" : pitch < .95f ? "grave" : pitch < 1.05f ? "normale"
        : pitch < 1.15f ? "aiguë" : pitch < 1.25f ? "très aiguë" : "suraiguë";

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
            SetFieldState(field, canSelect, gm.Panel == RM_Panel.Text && selected,
                selected || (gm.Panel != RM_Panel.Text && gm.Panel != RM_Panel.Voice));
            if (value.Length == 0)
            {
                // An empty passage takes no space in the game frame. Put its caret
                // where its first line would start: just below the composed text.
                float lineHeight = measurement.GetPreferredHeight("A", settings) / reference.pixelsPerUnit;
                float caretY = combined.Length == 0 ? 0 : (composition.lines[composition.lineCount - 1].topY
                    - composition.lines[composition.lineCount - 1].height) / reference.pixelsPerUnit - lineHeight * .5f;
                SetRect((RectTransform)field.transform, new Vector2(0, caretY), new Vector2(width, selected ? lineHeight : 0));
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

    public RM_TextInputField PassageField(int passage) => passage < 3 ? introInputs[passage] : objectiveInput;

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
        if (passage >= 0) SyncSpeech(gm.CurrentScene, passage);
        gm.RefreshText();
    }

    // The voice follows the text: each edit reconverts the line with the story's
    // respellings. Only lines never edited keep their 1988 score.
    public void SyncSpeech(SceneData scene, int passage, Action synced = null)
    {
        string text = ReadText(scene, passage);
        var story = StoryRoot.Session.Draft;
        gm.Speech.Convert(text, story.respellings, (speech, error) =>
        {
            if (StoryRoot.Session.Draft != story || ReadText(scene, passage) != text) return;
            if (speech == null)
            {
                Debug.LogWarning("[Maker] " + error);
                if (synced != null) gm.tooltip.ShowBrief(error, workspaceRect);
                return;
            }
            WriteSpeech(scene, passage, speech);
            StoryRoot.Session.NotifyEdited();
            synced?.Invoke();
        });
    }

    public static string ReadText(SceneData scene, int passage) => (passage switch
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
        var scene = gm.CurrentScene;
        int passage = (int)gm.Passage;
        float pitch = IsMastico(scene.voice, passage) ? CharacterPitches[0]
            : CharacterPitches.FirstOrDefault(p => p > ReadPitch(scene.voice, passage) + .001f);
        bool mastico = pitch == 0;
        switch (passage)
        {
            case 0: scene.voice.isMastico1 = mastico; if (!mastico) scene.voice.pitch1 = pitch; break;
            case 1: scene.voice.isMastico2 = mastico; if (!mastico) scene.voice.pitch2 = pitch; break;
            case 2: scene.voice.isMastico3 = mastico; if (!mastico) scene.voice.pitch3 = pitch; break;
        }
        StoryRoot.Session.NotifyEdited();
        StoryRoot.FlushWorkspace();
        Refresh();
        gm.sm.Speak(ReadSpeech(scene, passage), SpokenPitch(scene, passage));
    }

    void OpenVoice()
    {
        if (!gm.CanEdit || gm.CurrentScene == null) return;
        FinishFocus();
        var scene = gm.CurrentScene;
        int passage = (int)gm.Passage;
        var speech = ReadSpeech(scene, passage);
        if (RM_Speech.IsOriginal(speech))
        {
            gm.sm.Speak(speech, SpokenPitch(scene, passage));
            gm.tooltip.ShowBrief("Voix originale de 1988. Modifie le texte pour la refaire.", workspaceRect);
            return;
        }
        if (speech.sourceText == ReadText(scene, passage)) { gm.voice.Open(scene, passage); return; }
        SyncSpeech(scene, passage, () =>
        {
            if (gm.Panel == RM_Panel.Text && gm.CurrentScene == scene && (int)gm.Passage == passage) gm.voice.Open(scene, passage);
        });
    }

    public static SpeechDocument ReadSpeech(SceneData scene, int passage) => passage switch
    {
        0 => scene.dialogues.intro1,
        1 => scene.dialogues.intro2,
        2 => scene.dialogues.intro3,
        3 => scene.dialogues.obj,
        4 => scene.dialogues.ngp,
        _ => scene.dialogues.fsw
    };

    static void WriteSpeech(SceneData scene, int passage, SpeechDocument speech)
    {
        switch (passage)
        {
            case 0: scene.dialogues.intro1 = speech; break;
            case 1: scene.dialogues.intro2 = speech; break;
            case 2: scene.dialogues.intro3 = speech; break;
            case 3: scene.dialogues.obj = speech; break;
            case 4: scene.dialogues.ngp = speech; break;
            case 5: scene.dialogues.fsw = speech; break;
        }
    }

    /// <summary>The pitch the game plays this line with.</summary>
    public static float SpokenPitch(SceneData scene, int passage) => passage >= 3 || IsMastico(scene.voice, passage)
        ? scene.voice.isZambla ? .9f : 1f : ReadPitch(scene.voice, passage);

    static bool IsMastico(VoiceSettings voice, int passage) => passage == 0 ? voice.isMastico1
        : passage == 1 ? voice.isMastico2 : voice.isMastico3;
    static float ReadPitch(VoiceSettings voice, int passage) => passage == 0 ? voice.pitch1
        : passage == 1 ? voice.pitch2 : voice.pitch3;

    void OnDestroy()
    {
        ((IDisposable)composition).Dispose();
        ((IDisposable)measurement).Dispose();
    }
}
