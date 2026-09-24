using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SynthManager : MonoBehaviour
{
    public SoundManager sm;
    public SpeechInputField french, input;
    public Slider pitchSlider;
    public Dropdown sounds;
    public Text status, guidance, pronunciationLabel, pickerHelp, pitchValue, playLabel;
    public Button play, passage, paste, pastePhonemes, automatic, insert, audition, reset, apply, close;

    // Author-facing examples; synthesis and token validity belong to RodySpeechEngine.
    static readonly (string token, string example)[] SoundExamples =
    {
        ("a", "chat"), ("i", "ici"), ("u", "tu"), ("ou", "loup"),
        ("o", "beau"), ("oh", "porte"), ("et", "été"), ("ai", "mer"),
        ("e", "le / bleu"), ("eu", "peur"), ("ee", "meuh — son tenu"),
        ("an", "vent"), ("on", "bon"), ("in", "lapin"), ("un", "brun"),
        ("oi", "moi"), ("ui", "nuit"), ("y", "soleil"),
        ("p", "pas"), ("b", "bas"), ("t", "toi"), ("ti", "tu — autre T"),
        ("d", "dos"), ("c", "car"), ("g", "gare"), ("m", "maman"),
        ("n", "non"), ("gn", "montagne"), ("l", "lit"), ("r", "rue"),
        ("s", "sac"), ("z", "zéro"), ("f", "feu"), ("v", "vie"),
        ("ch", "chat"), ("j", "joue"), ("ouu", "oui ! — son tenu"),
        (",", "pause courte"), (".", "pause longue"),
        ("-", "bruit blanc"), ("cuicui", "oiseau"), ("pop", "pop")
    };

    // The opening is split across these three calls in the original game.
    // Templates are formatted from the bank when selected, never stored twice.
    static readonly int[] OriginalExamples = { 0, 1, 3 };

    string SelectedText => sounds.value < SoundExamples.Length ? SoundExamples[sounds.value].token :
        SoundManager.OriginalDialogue(OriginalExamples[sounds.value - SoundExamples.Length]);

    SpeechDocument document;
    int anchor, focus, frenchAnchor, frenchFocus, selectedWord;
    int conversionId;
    float convertAfter;
    bool scheduled, waiting, playWhenReady, closing, valid;
    string conversionError;
    SpeechInputField pasteTarget;
    int pasteStart, pasteEnd;

    bool HasFrench => !string.IsNullOrWhiteSpace(french.text);
    bool Pasting => pasteTarget != null;
    string Score => document.Notation;
    string EditedScore => string.Join("_", document.words.Select((word, i) => i == selectedWord ? input.text : word.score)
        .Where(score => !string.IsNullOrEmpty(score)));

    void Awake()
    {
        gameObject.name = "SpeechWorkbench-" + GetEntityId();
        sounds.ClearOptions();
        sounds.AddOptions(SoundExamples.Select(s => $"{s.token}  ·  {s.example}").ToList());
        sounds.AddOptions(OriginalExamples.Select((_, i) => $"Rody 1 · ouverture {i + 1}/3 · expression originale").ToList());
        sounds.onValueChanged.AddListener(_ => Refresh());
        play.onClick.AddListener(PlayAll);
        passage.onClick.AddListener(PlayPassage);
        paste.onClick.AddListener(() => Paste(french, frenchAnchor, frenchFocus));
        pastePhonemes.onClick.AddListener(() => Paste(input, anchor, focus));
        automatic.onClick.AddListener(ResetPronunciation);
        insert.onClick.AddListener(InsertSound);
        audition.onClick.AddListener(() => sm.Speak(SelectedText, pitchSlider.value));
        reset.onClick.AddListener(Restore);
        apply.onClick.AddListener(Copy);
        close.onClick.AddListener(Close);
        french.characterLimit = FrenchPhonemizer.MaxCharacters;
        french.onValueChanged.AddListener(_ => SourceChanged());
        input.onValueChanged.AddListener(_ => Refresh());
        input.onEndEdit.AddListener(_ => CommitPronunciation());
        pitchSlider.onValueChanged.AddListener(value =>
        {
            pitchSlider.SetValueWithoutNotify(Mathf.Round(value * 100) / 100);
            Refresh();
        });
        Restore();
    }

    void SetDocument(SpeechDocument value)
    {
        conversionId++;
        scheduled = waiting = playWhenReady = false;
        conversionError = null;
        document = value;
        french.SetTextWithoutNotify(document.sourceText);
        frenchAnchor = frenchFocus = 0;
        selectedWord = -1;
        input.SetTextWithoutNotify("");
        SelectWord(document.words.Count == 0 ? -1 : 0);
        Refresh();
    }

    void SourceChanged()
    {
        CommitPronunciation();
        sm.StopSpeech();
        conversionId++;
        conversionError = null;
        playWhenReady = false;
        if (!HasFrench)
        {
            document = SpeechDocument.FromNotation("");
            scheduled = waiting = false;
            selectedWord = -1;
            SelectWord(0);
        }
        else
        {
            scheduled = waiting = true;
            convertAfter = Time.unscaledTime + 0.3f;
        }
        Refresh();
    }

    void ConvertNow()
    {
        scheduled = false;
        waiting = true;
        conversionError = null;
        FrenchPhonemizer.Request(french.text, conversionId, gameObject.name, Converted);
    }

    public void RodyFrenchConverted(string json) => Converted(JsonUtility.FromJson<FrenchPhonemizer.Result>(json));

    void Converted(FrenchPhonemizer.Result result)
    {
        if (closing || result.requestId != conversionId) return;
        waiting = false;
        try
        {
            document = FrenchSpeechDraft.Convert(french.text, result, document);
            selectedWord = -1;
            SelectWord(FrenchSpeechDraft.WordAt(document, frenchFocus));
        }
        catch (ArgumentException exception)
        {
            conversionError = exception.Message;
        }
        Refresh();
        if (playWhenReady && valid) sm.Speak(document, pitchSlider.value);
        playWhenReady = false;
    }

    void SelectWord(int index)
    {
        if (index == selectedWord) return;
        if (selectedWord >= 0 && !CommitPronunciation()) return;
        selectedWord = index;
        input.SetTextWithoutNotify(index < 0 ? "" : document.words[index].score);
        anchor = focus = input.text.Length;
        Refresh();
    }

    bool CommitPronunciation()
    {
        if (selectedWord < 0) return true;
        if (waiting || Pasting) return false;
        string text = input.text;
        if (text.Split((char[])null).SelectMany(word => word.Split('_')).Any(token => RodySpeechEngine.TokenError(token) != null))
            return false;
        if (document.words[selectedWord].score == text) return true;
        sm.StopSpeech();
        var word = document.words[selectedWord];
        if (HasFrench && !FrenchSpeechDraft.SamePronunciation(word.score, text)) word.corrected = true;
        word.score = text;
        Refresh();
        return true;
    }

    void ResetPronunciation()
    {
        if (selectedWord < 0 || !HasFrench || Pasting) return;
        input.SetTextWithoutNotify(document.words[selectedWord].score);
        document.words[selectedWord].corrected = false;
        conversionId++;
        ConvertNow();
        Refresh();
    }

    void Refresh()
    {
        if (document == null) return;
        string score = EditedScore;
        string error = score.Split((char[])null).SelectMany(word => word.Split('_'))
            .Select(RodySpeechEngine.TokenError).FirstOrDefault(message => message != null);
        valid = !waiting && conversionError == null && error == null;
        bool hasScore = !string.IsNullOrWhiteSpace(score);
        play.interactable = sm.isPlaying || (!Pasting && (HasFrench || hasScore) && (waiting || conversionError != null || valid));
        passage.interactable = valid && hasScore && !Pasting;
        apply.interactable = valid && !Pasting;
        input.readOnly = Pasting || waiting || selectedWord < 0;
        french.readOnly = Pasting;
        paste.interactable = !Pasting;
        pastePhonemes.interactable = insert.interactable = !Pasting && !waiting && selectedWord >= 0;
        automatic.interactable = valid && !Pasting && HasFrench && selectedWord >= 0 && document.words[selectedWord].corrected;
        pitchValue.text = pitchSlider.value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "×";
        status.color = valid || waiting ? new Color32(81, 94, 88, 255) : new Color32(171, 49, 42, 255);
        status.text = waiting ? "Préparation de la prononciation…" : conversionError ?? error ??
            (input.text.Contains("[") ? "Les champs [forme, volume, vitesse] conservent l’expression du son." :
                "Les sons sont séparés par _ · Corrige seulement ce qui sonne faux.");
        guidance.text = HasFrench ? ", = pause courte · . = pause longue · Clique un mot pour corriger sa prononciation." :
            hasScore ? "Cette partition garde son expression. Écrire du français ci-dessus crée une nouvelle réplique." :
                ", = pause courte · . = pause longue · Les espaces séparent les mots, sans pause.";
        pronunciationLabel.text = !HasFrench ? "PARTITION · SAISIE DIRECTE DES SONS" : selectedWord < 0 ? "PRONONCIATION" :
            "PRONONCIATION · " + document.sourceText.Substring(document.words[selectedWord].start, document.words[selectedWord].length) +
                (document.words[selectedWord].corrected ? " · corrigée" : "");
        bool originalSelected = sounds.value >= SoundExamples.Length;
        pickerHelp.text = originalSelected ? "UN ORIGINAL REMPLACE LA RÉPLIQUE ET GARDE SON EXPRESSION" : "UN SON À ESSAYER OU À INSÉRER";
        insert.GetComponentInChildren<Text>().text = originalSelected ? "REMPLACER" : "INSÉRER";
        reset.interactable = !Pasting && (waiting || HasFrench || hasScore || !Mathf.Approximately(pitchSlider.value, 1f));
    }

    void Update()
    {
        if (french.isFocused)
        {
            frenchAnchor = french.selectionAnchorPosition;
            frenchFocus = french.selectionFocusPosition;
            if (!waiting && !Pasting && HasFrench && conversionError == null)
                SelectWord(FrenchSpeechDraft.WordAt(document, Math.Min(frenchAnchor, frenchFocus)));
        }
        if (input.isFocused)
        {
            anchor = input.selectionAnchorPosition;
            focus = input.selectionFocusPosition;
        }
        if (scheduled && Time.unscaledTime >= convertAfter) ConvertNow();
        playLabel.text = sm.isPlaying ? "ARRÊTER" : "TOUT ÉCOUTER";
        if (Input.GetKeyUp(KeyCode.Escape)) Close();
        if (Input.GetKeyDown(KeyCode.Return) &&
            (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
             Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand))) PlayAll();
    }

    void PlayAll()
    {
        if (sm.isPlaying) { sm.StopSpeech(); return; }
        if (Pasting) return;
        if (HasFrench && (waiting || conversionError != null))
        {
            playWhenReady = true;
            if (scheduled || !waiting) ConvertNow();
        }
        else if (valid && CommitPronunciation()) sm.Speak(document, pitchSlider.value);
    }

    (int start, int end) Selection(bool passageMode)
    {
        string text = input.text;
        int start = Mathf.Clamp(Math.Min(anchor, focus), 0, text.Length);
        int end = Mathf.Clamp(Math.Max(anchor, focus), 0, text.Length);
        bool wholeWord = passageMode && start == end;
        bool IsBoundary(char c) => char.IsWhiteSpace(c) || (!wholeWord && c == '_');
        if (start != end || passageMode ||
            (start > 0 && start < text.Length && !IsBoundary(text[start - 1]) && !IsBoundary(text[start])))
        {
            while (start > 0 && !IsBoundary(text[start - 1])) start--;
            while (end < text.Length && !IsBoundary(text[end])) end++;
        }
        return (start, end);
    }

    void PlayPassage()
    {
        if (!valid || selectedWord < 0 || Pasting || !CommitPronunciation()) return;
        if (HasFrench)
        {
            int first = selectedWord, end = first + 1;
            int sourceEnd = Math.Max(frenchAnchor, frenchFocus);
            if (frenchAnchor != frenchFocus)
                while (end < document.words.Count && document.words[end].start < sourceEnd) end++;
            var range = document.NotationRange(first, end);
            sm.Speak(document, pitchSlider.value, range.start, range.end);
        }
        else
        {
            var range = Selection(true);
            sm.Speak(document, pitchSlider.value, range.start, range.end);
        }
    }

    void InsertSound()
    {
        if (Pasting || waiting) return;
        if (sounds.value >= SoundExamples.Length)
        {
            sm.StopSpeech();
            SetDocument(SpeechDocument.FromNotation(SelectedText));
            return;
        }
        var range = Selection(false);
        string token = SelectedText;
        string text = input.text;
        string replaced = text.Substring(range.start, range.end - range.start);
        int bracket = replaced.IndexOf('[');
        // Replacing one native sound changes its identity, not its authored controls.
        if (bracket >= 0 && replaced.IndexOf('_') < 0 && replaced.EndsWith("]", StringComparison.Ordinal) &&
            RodySpeechEngine.TokenError(token + replaced.Substring(bracket)) == null)
            token += replaced.Substring(bracket);
        bool Separator(char c) => char.IsWhiteSpace(c) || c == '_';
        string inserted = token;
        if (range.start > 0 && !Separator(text[range.start - 1])) inserted = "_" + inserted;
        if (range.end < text.Length && !Separator(text[range.end])) inserted += "_";
        input.text = text.Substring(0, range.start) + inserted + text.Substring(range.end);
        anchor = focus = range.start + inserted.Length;
        CommitPronunciation();
        input.FocusAt(focus);
    }

    void Restore()
    {
        sm.StopSpeech();
        pitchSlider.SetValueWithoutNotify(1f);
        SetDocument(SpeechDocument.FromNotation(""));
    }

    void Close()
    {
        if (closing) return;
        closing = true;
        sm.StopSpeech();
        SceneManager.LoadScene(AppScenes.Selection);
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void RodyCopyText(string receiver, string text);
    [DllImport("__Internal")] static extern void RodyPasteText(string receiver);
#endif

    void Copy()
    {
        if (!valid || !CommitPronunciation()) return;
#if UNITY_WEBGL && !UNITY_EDITOR
        RodyCopyText(gameObject.name, Score);
#else
        GUIUtility.systemCopyBuffer = Score;
        ClipboardCopied("");
#endif
    }

    void Paste(SpeechInputField target, int start, int end)
    {
        if (Pasting || target.readOnly) return;
        pasteTarget = target;
        pasteStart = Mathf.Clamp(Math.Min(start, end), 0, target.text.Length);
        pasteEnd = Mathf.Clamp(Math.Max(start, end), 0, target.text.Length);
        Refresh();
#if UNITY_WEBGL && !UNITY_EDITOR
        RodyPasteText(gameObject.name);
#else
        ClipboardPasted(GUIUtility.systemCopyBuffer);
#endif
    }

    public void ClipboardCopied(string _) => status.text = "Phonèmes copiés.";
    public void ClipboardCopyFailed(string _) => status.text = "Le navigateur n’a pas autorisé la copie.";

    public void ClipboardFailed(string _)
    {
        pasteTarget = null;
        Refresh();
        status.text = "Le navigateur n’a pas autorisé le presse-papiers.";
    }

    public void ClipboardPasted(string text)
    {
        if (!Pasting) return;
        var target = pasteTarget;
        pasteTarget = null;
        target.text = target.text.Substring(0, pasteStart) + text + target.text.Substring(pasteEnd);
        if (target == input) CommitPronunciation();
        int caret = Mathf.Min(pasteStart + text.Length, target.text.Length);
        if (target == french) frenchAnchor = frenchFocus = caret;
        else anchor = focus = caret;
        Refresh();
        target.FocusAt(caret);
    }
}
