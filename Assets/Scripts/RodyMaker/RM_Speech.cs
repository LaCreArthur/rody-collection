using System;
using System.Collections.Generic;
using UnityEngine;

// The Maker's bridge to the French converter. Browser results arrive by message
// to this object's name, so it lives on its own object, active for the whole scene.
public class RM_Speech : MonoBehaviour
{
    readonly Dictionary<int, Action<FrenchPhonemizer.Result>> requests = new Dictionary<int, Action<FrenchPhonemizer.Result>>();
    int lastRequest;

    public static RM_Speech Create() => new GameObject("RodyMakerSpeech").AddComponent<RM_Speech>();

    /// <summary>A line never edited in the Maker: its 1988 score has no French words.</summary>
    public static bool IsOriginal(SpeechDocument speech) => speech.sourceText.Length == 0 && speech.Notation.Length > 0;

    /// <summary>
    /// Convert a line with the story's respellings. `done` receives the document,
    /// or null and a message for the creator.
    /// </summary>
    public void Convert(string text, IReadOnlyDictionary<string, string> respellings, Action<SpeechDocument, string> done)
    {
        string spoken = FrenchSpeechDraft.Spoken(text, respellings);
        int id = ++lastRequest;
        requests[id] = result =>
        {
            SpeechDocument speech;
            try { speech = FrenchSpeechDraft.Convert(text, spoken, result); }
            catch (ArgumentException exception)
            {
                done(null, exception.Message);
                return;
            }
            done(speech, null);
        };
        FrenchPhonemizer.Request(spoken, id, gameObject.name, Converted);
    }

    public void RodyFrenchConverted(string json) => Converted(JsonUtility.FromJson<FrenchPhonemizer.Result>(json));

    void Converted(FrenchPhonemizer.Result result)
    {
        if (requests.Remove(result.requestId, out var done)) done(result);
    }
}
