using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Source spelling and the accepted score are different facts. Conversion edits
// the document; playing/loading it never runs the converter again.
public static class FrenchSpeechDraft
{
    // Authored names take precedence over the converter's language guesses.
    // Per-dialogue corrections are applied afterwards and remain authoritative.
    static readonly Dictionary<string, string> Pronunciations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Rody", "r_o_d_i" }
    };

    static readonly Dictionary<string, string> Sounds = new Dictionary<string, string>
    {
        { "a", "a" }, { "ɑ", "a" }, { "ɐ", "a" }, { "æ", "a" }, { "ʌ", "a" },
        { "i", "i" }, { "ɪ", "i" }, { "y", "u" }, { "ʏ", "u" },
        { "u", "ou" }, { "ʊ", "ou" }, { "w", "ou" }, { "ɥ", "u" },
        { "o", "o" }, { "ɔ", "oh" }, { "e", "et" }, { "ɛ", "ai" },
        { "ə", "e" }, { "ø", "e" }, { "œ", "eu" }, { "ɜ", "eu" },
        { "ɚ", "e_r" }, { "ɝ", "eu_r" },
        { "ɑ̃", "an" }, { "ã", "an" }, { "ɔ̃", "on[1,1,0]" }, { "õ", "on[1,1,0]" },
        { "ɛ̃", "in" }, { "œ̃", "un" },
        { "p", "p" }, { "b", "b" }, { "t", "t" }, { "d", "d" },
        { "k", "c" }, { "ɡ", "g" }, { "g", "g" }, { "m", "m" }, { "n", "n" },
        { "ɲ", "gn" }, { "ŋ", "gn" }, { "l", "l" }, { "ɫ", "l" },
        { "ʁ", "r" }, { "r", "r" }, { "ɹ", "r" }, { "ɾ", "r" }, { "ʀ", "r" },
        { "s", "s" }, { "z", "z" }, { "f", "f" }, { "v", "v" },
        { "ʃ", "ch" }, { "ʒ", "j" }, { "j", "y" }, { "θ", "s" }, { "ð", "z" },
        { "x", "r" }, { "χ", "r" }, { "h", "" }, { "ʔ", "" }
    };

    public static SpeechDocument Convert(string text, FrenchPhonemizer.Result result, SpeechDocument previous)
    {
        if (!string.IsNullOrEmpty(result.error)) throw new ArgumentException(result.error);
        var document = new SpeechDocument { sourceText = text, words = SourceWords(text) };
        var ipaByWord = document.words.Select(_ => new StringBuilder()).ToArray();
        int[] sourceOffsets = CodePointOffsets(text), ipaOffsets = CodePointOffsets(result.ipa);
        for (int i = 0; i < result.sourceMap.Length; i++)
        {
            var point = result.sourceMap[i];
            int start = Offset(sourceOffsets, point.source);
            int end = i + 1 < result.sourceMap.Length ? Offset(sourceOffsets, result.sourceMap[i + 1].source) : text.Length;
            int ipaStart = Offset(ipaOffsets, point.ipa);
            int ipaEnd = i + 1 < result.sourceMap.Length ? Offset(ipaOffsets, result.sourceMap[i + 1].ipa) : result.ipa.Length;
            if (end < start || ipaEnd < ipaStart) throw new ArgumentException("La conversion a renvoyé un alignement invalide.");
            int word = MappedWord(document, start, end);
            if (word >= 0) ipaByWord[word].Append(result.ipa, ipaStart, ipaEnd - ipaStart);
        }

        for (int i = 0; i < document.words.Count; i++)
        {
            var word = document.words[i];
            char pause = PauseAt(text, word.start);
            // The native short pause is only 151 samples. Its native repetition
            // field gives a useful ~116 ms comma; a period is ~323 ms. Existing
            // authored scores keep their own exact pause instructions.
            string spelling = text.Substring(word.start, word.length).Trim('"', '«', '»', '“', '”', '(', ')');
            word.score = pause == ',' ? ",[9,0,0]" : pause == '.' ? ".[0,0,0]" :
                Pronunciations.TryGetValue(spelling, out string pronunciation) ? pronunciation : ToScore(ipaByWord[i].ToString());
        }
        PreserveEdits(previous, document);
        return document;
    }

    public static List<SpeechWord> SourceWords(string text)
    {
        var words = new List<SpeechWord>();
        for (int i = 0; i < text.Length;)
        {
            if (char.IsWhiteSpace(text[i])) { i++; continue; }
            int start = i++;
            if (PauseAt(text, start) == '\0')
                while (i < text.Length && !char.IsWhiteSpace(text[i]) && PauseAt(text, i) == '\0') i++;
            words.Add(new SpeechWord { start = start, length = i - start });
        }
        return words;
    }

    public static char PauseAt(string text, int index)
    {
        if (index < 0 || index >= text.Length) return '\0';
        char c = text[index];
        if ((c == ',' || c == '.') && index > 0 && index + 1 < text.Length &&
            char.IsDigit(text[index - 1]) && char.IsDigit(text[index + 1])) return '\0';
        if (c == ',' || c == ';' || c == ':') return ',';
        if (c == '.' || c == '?' || c == '!' || c == '…') return '.';
        return '\0';
    }

    // A converter word may expand a number, or start in preceding whitespace.
    // Assign its actual IPA fragment to the corresponding written word; never
    // convert selected words independently (that would discard liaison context).
    static int MappedWord(SpeechDocument document, int start, int end)
    {
        int previous = -1;
        for (int i = 0; i < document.words.Count; i++)
        {
            var word = document.words[i];
            if (PauseAt(document.sourceText, word.start) != '\0') continue;
            if (word.start <= start && start < word.start + word.length) return i;
            if (word.start >= start && word.start < end) return i;
            if (word.start < start) previous = i;
        }
        return previous;
    }

    public static string ToScore(string ipa)
    {
        var score = new List<string>();
        ipa = ipa.Normalize(NormalizationForm.FormD);
        for (int i = 0; i < ipa.Length; i++)
        {
            string sound = ipa[i].ToString();
            if (i + 1 < ipa.Length && ipa[i + 1] == '\u0303' && Sounds.ContainsKey(sound + "\u0303"))
            {
                sound += ipa[++i];
            }
            if (Sounds.TryGetValue(sound, out string token))
            {
                if (token.Length != 0) score.Add(token);
                continue;
            }
            // IPA stress, length, liaison hyphens, separators and diacritics are
            // not Rody sound effects or source punctuation. The source owns pauses.
            if (char.IsWhiteSpace(ipa[i]) || "ˈˌːˑ-.,!?;:|‖".IndexOf(ipa[i]) >= 0 ||
                char.GetUnicodeCategory(ipa[i]) == System.Globalization.UnicodeCategory.NonSpacingMark) continue;
            throw new ArgumentException("Le son « " + sound + " » n’a pas encore d’équivalent dans la voix de Rody.");
        }
        return string.Join("_", score);
    }

    static void PreserveEdits(SpeechDocument previous, SpeechDocument next)
    {
        if (previous == null || previous.sourceText.Length == 0) return;
        var oldWords = previous.words.Select(w => previous.sourceText.Substring(w.start, w.length)).ToArray();
        var newWords = next.words.Select(w => next.sourceText.Substring(w.start, w.length)).ToArray();
        int[] matches = Match(oldWords, newWords);
        for (int i = 0; i < next.words.Count; i++)
        {
            if (matches[i] < 0) continue;
            var old = previous.words[matches[i]];
            var word = next.words[i];
            word.score = old.corrected ? old.score : PreserveExpression(old.score, word.score);
            word.corrected = old.corrected;
        }
    }

    // Sequence alignment handles several unrelated edits/pasted replacements,
    // including unchanged corrected words in the middle of the edited text.
    static int[] Match(string[] before, string[] after)
    {
        var lengths = new int[before.Length + 1, after.Length + 1];
        for (int i = before.Length - 1; i >= 0; i--)
            for (int j = after.Length - 1; j >= 0; j--)
                lengths[i, j] = before[i] == after[j] ? lengths[i + 1, j + 1] + 1 :
                    Math.Max(lengths[i + 1, j], lengths[i, j + 1]);
        var matched = Enumerable.Repeat(-1, after.Length).ToArray();
        for (int i = 0, j = 0; i < before.Length && j < after.Length;)
        {
            if (before[i] == after[j]) { matched[j++] = i++; }
            else if (lengths[i + 1, j] >= lengths[i, j + 1]) i++;
            else j++;
        }
        return matched;
    }

    static List<(int sound, string score)> SoundGroups(string score)
    {
        var groups = new List<(int sound, string score)>();
        foreach (var phrase in SoundManager.Engine.ParseDialogue(score, token =>
            throw new ArgumentException(RodySpeechEngine.TokenError(token))))
        {
            if (phrase.Effect != RodySpeechEffect.None)
            {
                groups.Add((-(int)phrase.Effect, phrase.Effect == RodySpeechEffect.Noise ? "-" :
                    phrase.Effect == RodySpeechEffect.Bird ? "cuicui" : "pop"));
                continue;
            }
            for (int i = 0; i < phrase.Tokens.Length;)
            {
                int first = i, sound = phrase.Tokens[i++] & 63;
                while (i < phrase.Tokens.Length && (phrase.Tokens[i] & 63) == sound) i++;
                var envelope = new ushort[i - first];
                Array.Copy(phrase.Tokens, first, envelope, 0, envelope.Length);
                groups.Add((sound, RodySpeechEngine.FormatDialogue(envelope)));
            }
        }
        return groups;
    }

    static bool IsSound(int descriptor) => descriptor >= 0 && descriptor < 60;

    public static bool SamePronunciation(string before, string after) =>
        SoundGroups(before).Where(g => IsSound(g.sound)).Select(g => g.sound)
            .SequenceEqual(SoundGroups(after).Where(g => IsSound(g.sound)).Select(g => g.sound));

    static string PreserveExpression(string before, string after)
    {
        if (before == after) return before;
        if (before.Length == 0) return after;
        // No pronunciation change means no score rewrite. Besides preserving
        // exact authored envelopes, this keeps context-sensitive aliases live
        // until the full utterance is rendered.
        if (SamePronunciation(before, after)) return before;
        var oldGroups = SoundGroups(before);
        var newGroups = SoundGroups(after);
        var oldSounds = oldGroups.Select((g, i) => i).Where(i => IsSound(oldGroups[i].sound)).ToArray();
        var newSounds = newGroups.Where(g => IsSound(g.sound)).ToArray();
        if (newSounds.Length == 0) return oldSounds.Length == 0 ? before : after;
        int[] matches = Match(oldSounds.Select(i => oldGroups[i].sound.ToString()).ToArray(),
            newSounds.Select(g => g.sound.ToString()).ToArray());
        var beforeSound = Enumerable.Range(0, newSounds.Length + 1).Select(_ => new List<string>()).ToArray();
        for (int i = 0; i < oldGroups.Count; i++)
        {
            if (IsSound(oldGroups[i].sound)) continue;
            int next = Array.FindIndex(matches, match => match >= 0 && oldSounds[match] > i);
            beforeSound[next < 0 ? newSounds.Length : next].Add(oldGroups[i].score);
        }
        var score = new List<string>();
        for (int i = 0; i < newSounds.Length; i++)
        {
            score.AddRange(beforeSound[i]);
            // Preserve every native instruction in a surviving sound's envelope.
            score.Add(matches[i] < 0 ? newSounds[i].score : oldGroups[oldSounds[matches[i]]].score);
        }
        score.AddRange(beforeSound[newSounds.Length]);
        return string.Join("_", score);
    }

    public static int WordAt(SpeechDocument document, int caret)
    {
        int previous = -1;
        for (int i = 0; i < document.words.Count; i++)
        {
            if (PauseAt(document.sourceText, document.words[i].start) != '\0') continue;
            if (caret < document.words[i].start && previous >= 0) return previous;
            if (caret < document.words[i].start + document.words[i].length) return i;
            previous = i;
        }
        return previous;
    }

    static int[] CodePointOffsets(string text)
    {
        var offsets = new List<int>();
        for (int i = 0; i < text.Length; i++)
        {
            offsets.Add(i);
            if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1])) i++;
        }
        offsets.Add(text.Length);
        return offsets.ToArray();
    }

    static int Offset(int[] offsets, int point)
    {
        if (point < 0 || point >= offsets.Length) throw new ArgumentException("La conversion a renvoyé une position invalide.");
        return offsets[point];
    }
}
