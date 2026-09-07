using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SynthManager : MonoBehaviour
{
    public SoundManager sm;
    public SpeechInputField input;
    public Slider pitchSlider;
    public Dropdown sounds;
    public Text context, status, pitchValue, playLabel, applyLabel, closeLabel;
    public Button play, passage, copy, paste, insert, audition, reset, apply, close;
    public Camera workbenchCamera;
    public AudioListener workbenchListener;
    public EventSystem workbenchEvents;

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
        (",", "pause courte"), (".", "pause longue"), (" ", "respiration"),
        ("-", "bruit blanc"), ("cuicui", "oiseau"), ("pop", "pop")
    };

    // The opening is split across these three calls in the original game.
    // Templates are formatted from the bank when selected, never stored twice.
    static readonly int[] OriginalExamples = { 0, 1, 3 };

    string SelectedText => sounds.value < SoundExamples.Length ? SoundExamples[sounds.value].token :
        SoundManager.OriginalDialogue(OriginalExamples[sounds.value - SoundExamples.Length]);

    Action<string, float> onApply;
    Action onClose;
    string originalText;
    float originalPitch;
    int anchor, focus;
    bool closing;
    bool valid;
    bool pasting;
    int pasteStart, pasteEnd;

    public static void Open(string text, float pitch, bool editablePitch, string heading,
        Action<string, float> applyChanges, Action closed)
    {
        var operation = SceneManager.LoadSceneAsync(AppScenes.Phonemes, LoadSceneMode.Additive);
        operation.completed += _ =>
        {
            var scene = SceneManager.GetSceneByBuildIndex(AppScenes.Phonemes);
            var workbench = scene.GetRootGameObjects().Select(go => go.GetComponent<SynthManager>()).Single(s => s != null);
            workbench.onApply = applyChanges;
            workbench.onClose = closed;
            workbench.workbenchCamera.enabled = false;
            workbench.workbenchListener.enabled = false;
            workbench.workbenchEvents.gameObject.SetActive(false);
            SceneManager.SetActiveScene(scene);
            workbench.Begin(text, pitch, editablePitch, heading);
        };
    }

    void Awake()
    {
        gameObject.name = "SpeechWorkbench-" + GetEntityId();
        sounds.ClearOptions();
        sounds.AddOptions(SoundExamples.Select(s => $"{(s.token == " " ? "espace" : s.token)}  ·  {s.example}").ToList());
        sounds.AddOptions(OriginalExamples.Select((_, i) => $"Rody 1 · ouverture {i + 1}/3 · expression originale").ToList());
        play.onClick.AddListener(PlayAll);
        passage.onClick.AddListener(PlayPassage);
        copy.onClick.AddListener(Copy);
        paste.onClick.AddListener(Paste);
        insert.onClick.AddListener(InsertSound);
        audition.onClick.AddListener(() => sm.Speak(SelectedText, pitchSlider.value));
        reset.onClick.AddListener(Restore);
        apply.onClick.AddListener(Apply);
        close.onClick.AddListener(Close);
        input.onValueChanged.AddListener(_ => Refresh());
        pitchSlider.onValueChanged.AddListener(value =>
        {
            pitchSlider.SetValueWithoutNotify(Mathf.Round(value * 100) / 100);
            Refresh();
        });
        Begin("b_r_a_v_o r_o_d_i", 1f, true, "Une voix de 1988. Tes propres répliques.");
    }

    void Begin(string text, float pitch, bool editablePitch, string heading)
    {
        originalText = text ?? "";
        originalPitch = pitch;
        context.text = heading;
        pitchSlider.interactable = editablePitch;
        input.SetTextWithoutNotify(originalText);
        pitchSlider.SetValueWithoutNotify(pitch);
        anchor = focus = input.text.Length;
        copy.gameObject.SetActive(onApply != null);
        applyLabel.text = onApply != null ? "UTILISER CE DIALOGUE" : "COPIER LES PHONÈMES";
        closeLabel.text = onApply != null ? "ANNULER" : "RETOUR";
        Refresh();
    }

    void Refresh()
    {
        string error = input.text.Split((char[])null).SelectMany(word => word.Split('_'))
            .Select(RodySpeechEngine.TokenError).FirstOrDefault(message => message != null);
        valid = error == null;
        bool hasText = !string.IsNullOrWhiteSpace(input.text);
        play.interactable = sm.isPlaying || (valid && hasText);
        passage.interactable = valid && hasText;
        apply.interactable = valid && !pasting;
        input.readOnly = pasting;
        paste.interactable = insert.interactable = !pasting;
        copy.interactable = hasText;
        pitchValue.text = pitchSlider.value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "×";
        status.color = valid ? new Color32(81, 94, 88, 255) : new Color32(171, 49, 42, 255);
        status.text = !valid ? error : input.text.Contains("[") ?
            "[forme 0–15, volume 0–7, vitesse 0–7] · 0 garde volume/vitesse.\nLa forme choisit une partie du son : un nombre plus grand ne l’allonge pas forcément." :
            "Un espace = une respiration. Deux _ = une petite pause.";
        reset.interactable = !pasting && (input.text != originalText || !Mathf.Approximately(pitchSlider.value, originalPitch));
    }

    void Update()
    {
        if (input.isFocused)
        {
            anchor = input.selectionAnchorPosition;
            focus = input.selectionFocusPosition;
        }
        playLabel.text = sm.isPlaying ? "ARRÊTER" : "TOUT ÉCOUTER";
        play.interactable = sm.isPlaying || (valid && !string.IsNullOrWhiteSpace(input.text));
        if (Input.GetKeyUp(KeyCode.Escape)) Close();
        if (Input.GetKeyDown(KeyCode.Return) &&
            (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
             Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand))) PlayAll();
    }

    void PlayAll()
    {
        if (sm.isPlaying) sm.StopSpeech();
        else if (valid) sm.Speak(input.text, pitchSlider.value);
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
        if (!valid) return;
        var range = Selection(true);
        sm.Speak(input.text, pitchSlider.value, range.start, range.end);
    }

    void InsertSound()
    {
        var range = Selection(false);
        string token = SelectedText;
        string text = input.text;
        bool Separator(char c) => char.IsWhiteSpace(c) || c == '_';
        string inserted = token;
        if (token != " ")
        {
            if (range.start > 0 && !Separator(text[range.start - 1])) inserted = "_" + inserted;
            if (range.end < text.Length && !Separator(text[range.end])) inserted += "_";
        }
        input.text = text.Substring(0, range.start) + inserted + text.Substring(range.end);
        anchor = focus = range.start + inserted.Length;
        FocusInput();
    }

    void FocusInput()
    {
        input.FocusAt(focus);
    }

    void Restore()
    {
        sm.StopSpeech();
        input.SetTextWithoutNotify(originalText);
        pitchSlider.SetValueWithoutNotify(originalPitch);
        anchor = focus = input.text.Length;
        Refresh();
        FocusInput();
    }

    void Apply()
    {
        if (!valid) return;
        if (onApply == null) Copy();
        else
        {
            onApply(input.text, pitchSlider.value);
            Close();
        }
    }

    void Close()
    {
        if (closing) return;
        closing = true;
        sm.StopSpeech();
        if (onApply != null)
        {
            var operation = SceneManager.UnloadSceneAsync(gameObject.scene);
            var closed = onClose;
            operation.completed += _ => closed();
        }
        else SceneManager.LoadScene(AppScenes.Selection);
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void RodyCopyText(string receiver, string text);
    [DllImport("__Internal")] static extern void RodyPasteText(string receiver);
#endif

    void Copy()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        RodyCopyText(gameObject.name, input.text);
#else
        GUIUtility.systemCopyBuffer = input.text;
        ClipboardCopied("");
#endif
    }

    void Paste()
    {
        if (pasting) return;
        pasteStart = Mathf.Clamp(Math.Min(anchor, focus), 0, input.text.Length);
        pasteEnd = Mathf.Clamp(Math.Max(anchor, focus), 0, input.text.Length);
        pasting = true;
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
        pasting = false;
        Refresh();
        status.text = "Le navigateur n’a pas autorisé le presse-papiers.";
    }
    public void ClipboardPasted(string text)
    {
        if (!pasting) return;
        pasting = false;
        input.text = input.text.Substring(0, pasteStart) + text + input.text.Substring(pasteEnd);
        anchor = focus = pasteStart + text.Length;
        Refresh();
        FocusInput();
    }
}
