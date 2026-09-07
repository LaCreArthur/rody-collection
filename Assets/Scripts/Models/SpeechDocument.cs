using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// Authored French and its one lossless speech score. Word fragments own the
/// score; the complete notation is a projection, never another stored version.
/// </summary>
[Serializable]
public class SpeechDocument
{
    public string sourceText = "";
    public List<SpeechWord> words = new List<SpeechWord>();

    [JsonIgnore]
    public string Notation => string.Join("_", words.Where(word => !string.IsNullOrEmpty(word.score)).Select(word => word.score));

    public static SpeechDocument FromNotation(string notation) => new SpeechDocument
    {
        words = new List<SpeechWord> { new SpeechWord { score = notation ?? "" } }
    };

    public SpeechDocument Clone() => new SpeechDocument
    {
        sourceText = sourceText,
        words = words.Select(word => new SpeechWord
        {
            start = word.start,
            length = word.length,
            score = word.score,
            corrected = word.corrected
        }).ToList()
    };

    /// <summary>Character range in Notation for [firstWord, endWord).</summary>
    public (int start, int end) NotationRange(int firstWord, int endWord)
    {
        if (firstWord < 0 || endWord < firstWord || endWord > words.Count)
            throw new ArgumentOutOfRangeException(nameof(firstWord));
        int offset = 0, start = -1, end = 0;
        bool hasScore = false;
        for (int i = 0; i < words.Count; i++)
        {
            string score = words[i].score;
            if (string.IsNullOrEmpty(score)) continue;
            if (hasScore) offset++;
            if (i >= firstWord && i < endWord)
            {
                if (start < 0) start = offset;
                end = offset + score.Length;
            }
            offset += score.Length;
            hasScore = true;
        }
        return start < 0 ? (0, 0) : (start, end);
    }
}

[Serializable]
public class SpeechWord
{
    public int start;
    public int length;
    public string score = "";
    public bool corrected;
}
