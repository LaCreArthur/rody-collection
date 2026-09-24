using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Fix a mispronounced word: click it in the line, write how it is said in French,
// listen, validate. A fix applies to that word everywhere in the story.
public class RM_VoiceLayout : MonoBehaviour
{
    public Button listenLine, validate, cancel, listenWord;
    public GameObject correction;
    public Text prompt;
    public InputField respelling;

    static readonly Color Picked = new Color(1f, .91f, .58f), Hovered = new Color(1f, .91f, .58f, .45f),
        Fixed = new Color(.5f, 0f, .125f);

    readonly List<Image> marks = new List<Image>();
    readonly List<UICharInfo> characters = new List<UICharInfo>();
    readonly List<UILineInfo> lines = new List<UILineInfo>();
    RM_GameManager gm;
    SceneData scene;
    int passage, word = -1, previewRequest;
    float panelWidth;
    bool previewing;
    Action afterPreview;
    Dictionary<string, string> respellings; // the story's fixes with this session's changes
    SpeechDocument preview;                 // the line as it sounds with them
    Image pick, hover;

    Text Line => gm.workspace.PassageField(passage).textComponent;
    float Pitch => RM_DialLayout.SpokenPitch(scene, passage);
    // Pauses and bare quotes are not words.
    bool IsWord(int index) => FrenchSpeechDraft.PauseAt(preview.sourceText, preview.words[index].start) == '\0'
        && Core(index).Length > 0;
    string Core(int index) => FrenchSpeechDraft.WordCore(preview.sourceText, preview.words[index]);
    string Key(int index) => FrenchSpeechDraft.WordKey(preview.sourceText, preview.words[index]);

    public void Initialize(RM_GameManager manager)
    {
        gm = manager;
        listenLine.onClick.AddListener(PlayLine);
        listenWord.onClick.AddListener(PlayWord);
        validate.onClick.AddListener(Validate);
        cancel.onClick.AddListener(Cancel);
        respelling.onValueChanged.AddListener(Respell);
        respelling.onSubmit.AddListener(_ => PlayWord());
        pick = Mark(Picked);
        hover = Mark(Hovered);
        panelWidth = ((RectTransform)correction.transform).sizeDelta.x;
    }

    public void Open(SceneData line, int index)
    {
        scene = line;
        passage = index;
        respellings = new Dictionary<string, string>(StoryRoot.Session.Draft.respellings);
        preview = RM_DialLayout.ReadSpeech(scene, passage);
        word = -1;
        validate.interactable = true;
        correction.SetActive(false);
        gm.ShowPanel(RM_Panel.Voice);
        PlayLine();
        gm.tooltip.ShowBrief("Clique sur un mot mal prononcé.", (RectTransform)gm.workspace.transform);
    }

    public void Cancel()
    {
        if (gm.CanEdit) Close();
    }

    void Validate()
    {
        if (!gm.CanEdit) return;
        var story = StoryRoot.Session.Draft;
        var changed = new HashSet<string>(respellings.Keys.Union(story.respellings.Keys).Where(key =>
            !respellings.TryGetValue(key, out string now) || !story.respellings.TryGetValue(key, out string before) || now != before));
        if (changed.Count > 0)
        {
            story.respellings = respellings;
            StoryRoot.Session.NotifyEdited();
            foreach (var entry in story.scenes)
                for (int i = 0; i < 6; i++)
                {
                    string text = RM_DialLayout.ReadText(entry.data, i);
                    if (!RM_Speech.IsOriginal(RM_DialLayout.ReadSpeech(entry.data, i)) && FrenchSpeechDraft.SourceWords(text)
                            .Any(w => changed.Contains(FrenchSpeechDraft.WordKey(text, w))))
                        gm.workspace.SyncSpeech(entry.data, i);
                }
        }
        Close();
    }

    void Close()
    {
        gm.sm.StopSpeech();
        gm.tooltip.HideFor((RectTransform)gm.workspace.transform);
        previewRequest++;
        previewing = false;
        afterPreview = null;
        gm.ShowPanel(RM_Panel.Text);
    }

    void Pick(int index)
    {
        gm.tooltip.HideFor((RectTransform)gm.workspace.transform);
        word = index;
        prompt.text = "ÉCRIS « " + Core(index) + " » COMME TU LE DIS";
        // A long word widens the panel instead of spilling past its border.
        var panel = (RectTransform)correction.transform;
        panel.sizeDelta = new Vector2(Mathf.Max(panelWidth, prompt.preferredWidth + 8), panel.sizeDelta.y);
        correction.SetActive(true);
        respelling.SetTextWithoutNotify(respellings.TryGetValue(Key(index), out string value) ? value : Core(index));
        respelling.ActivateInputField();
        PlayWord();
    }

    void Respell(string value)
    {
        if (word < 0) return;
        if (value.Length == 0 || string.Equals(value, Core(word), StringComparison.OrdinalIgnoreCase)) respellings.Remove(Key(word));
        else respellings[Key(word)] = value;
        int request = ++previewRequest;
        previewing = true;
        validate.interactable = false; // a fix is kept only once it converts
        gm.Speech.Convert(preview.sourceText, respellings, (speech, error) =>
        {
            if (request != previewRequest) return;
            previewing = false;
            validate.interactable = speech != null;
            if (speech == null) gm.tooltip.ShowBrief(error, (RectTransform)correction.transform);
            else preview = speech;
            var next = afterPreview;
            afterPreview = null;
            next?.Invoke();
        });
    }

    void PlayLine()
    {
        if (previewing) { afterPreview = PlayLine; return; }
        gm.sm.Speak(preview, Pitch);
    }

    void PlayWord()
    {
        if (word < 0) return;
        if (previewing) { afterPreview = PlayWord; return; }
        var range = preview.NotationRange(word, word + 1);
        gm.sm.Speak(preview, Pitch, range.start, range.end);
    }

    void LateUpdate()
    {
        var label = Line;
        bool shown = label.text == preview.sourceText;
        label.cachedTextGenerator.GetCharacters(characters);
        label.cachedTextGenerator.GetLines(lines);
        int hovered = shown ? WordUnderPointer(label) : -1;
        if (hovered >= 0 && gm.CanEdit && Input.GetMouseButtonDown(0)) Pick(hovered);
        Place(pick, label, shown ? word : -1, false);
        Place(hover, label, hovered == word ? -1 : hovered, false);
        int count = 0;
        for (int i = 0; shown && i < preview.words.Count; i++)
        {
            if (!IsWord(i) || !respellings.ContainsKey(Key(i))) continue;
            if (count == marks.Count) marks.Add(Mark(Fixed));
            Place(marks[count++], label, i, true);
        }
        for (int i = count; i < marks.Count; i++) marks[i].gameObject.SetActive(false);
    }

    void OnDisable()
    {
        if (pick == null) return;
        pick.gameObject.SetActive(false);
        hover.gameObject.SetActive(false);
        foreach (var mark in marks) mark.gameObject.SetActive(false);
    }

    int WordUnderPointer(Text label)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(label.rectTransform, Input.mousePosition,
                label.canvas.rootCanvas.worldCamera, out var point)) return -1;
        for (int i = 0; i < preview.words.Count; i++)
        {
            // One art pixel of slack around each word keeps small words easy to hit.
            var rect = WordRect(label, i);
            if (IsWord(i) && rect.width > 0 && new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2).Contains(point))
                return i;
        }
        return -1;
    }

    // In the label's local units, from the positions of the rendered text.
    Rect WordRect(Text label, int index)
    {
        var spoken = preview.words[index];
        int last = spoken.start + spoken.length - 1;
        if (spoken.length == 0 || last >= characters.Count || lines.Count == 0) return Rect.zero;
        int line = 0;
        while (line + 1 < lines.Count && lines[line + 1].startCharIdx <= spoken.start) line++;
        float scale = 1f / label.pixelsPerUnit;
        return Rect.MinMaxRect(characters[spoken.start].cursorPos.x * scale, (lines[line].topY - lines[line].height) * scale,
            (characters[last].cursorPos.x + characters[last].charWidth) * scale, lines[line].topY * scale);
    }

    void Place(Image mark, Text label, int index, bool underline)
    {
        var rect = index < 0 ? Rect.zero : WordRect(label, index);
        mark.gameObject.SetActive(rect.width > 0);
        if (rect.width <= 0) return;
        if (underline) rect = new Rect(rect.x, rect.y, rect.width, 1);
        var body = gm.workspace.body;
        Vector2 min = body.InverseTransformPoint(label.rectTransform.TransformPoint(rect.min));
        Vector2 max = body.InverseTransformPoint(label.rectTransform.TransformPoint(rect.max));
        var t = (RectTransform)mark.transform;
        t.anchoredPosition = (min + max) * .5f;
        t.sizeDelta = max - min;
    }

    // Marks sit behind the line's text, in the workspace body that holds it.
    Image Mark(Color color)
    {
        var mark = new GameObject("Word mark", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        var t = (RectTransform)mark.transform;
        t.SetParent(gm.workspace.body, false);
        t.SetAsFirstSibling();
        t.anchorMin = t.anchorMax = t.pivot = new Vector2(.5f, .5f);
        mark.color = color;
        mark.raycastTarget = false;
        mark.gameObject.SetActive(false);
        return mark;
    }
}
