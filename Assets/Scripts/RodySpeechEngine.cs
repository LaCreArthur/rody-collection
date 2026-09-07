using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

// Port of tools/original-extraction/preprocess.py and render_all.py.
// All samples are unsigned 8-bit PCM; Unity converts them only at the playback boundary.
public sealed class RodySpeechEngine
{
    public const int SampleRate = 13000;
    const int ClipTable = 0x2560;
    const int AudioStart = 0x2f70;
    const int TableStart = 0x4aee;
    const int NativeSpeed = -1;
    const int DefaultDelay = 16 - 2 * NativeSpeed;
    readonly byte[] bank;
    readonly byte[] tables;
    readonly int[] clips = new int[644];
    public int OriginalDialogueCount { get; }

    public RodySpeechEngine(byte[] bank, byte[] tables)
    {
        this.bank = bank;
        this.tables = tables;
        while (OriginalDialogueCount < 147 && BankWord(2 * OriginalDialogueCount + 2) > BankWord(2 * OriginalDialogueCount))
            OriginalDialogueCount++;
        for (int i = 0; i < clips.Length; i++)
        {
            int p = ClipTable + 4 * i;
            clips[i] = bank[p] << 24 | bank[p + 1] << 16 | bank[p + 2] << 8 | bank[p + 3];
        }
    }

    int Table(int address) => tables[address - TableStart];

    // Remake notation -> native record tokens. Base values come from
    // catalog/phoneme_table.tsv + data/rep_tokens.json; see docs/SPEECH_ENGINE.md.
    static readonly Dictionary<string, ushort> Phonemes = new Dictionary<string, ushort>
    {
        { "i", 0x1200 }, { "et", 0x1201 }, { "ai", 0x1202 }, { "a", 0x1203 },
        { "oh", 0x1205 }, { "o", 0x1206 }, { "ou", 0x1207 }, { "u", 0x1208 },
        { "e", 0x120a }, { "eu", 0x120a }, { "in", 0x120b }, { "un", 0x120b },
        { "an", 0x120c }, { "on", 0x420d }, { "p", 0x3b16 }, { "b", 0x2b17 },
        { "m", 0x2b18 }, { "f", 0x2b59 }, { "v", 0x1b1a }, { "t", 0x3a1b },
        { "d", 0x1b1c }, { "n", 0x1b1d }, { "s", 0x2b1e }, { "z", 0x1a1f },
        { "l", 0x1a20 }, { "ch", 0x2b21 }, { "j", 0x1b22 }, { "c", 0x3b23 },
        { "g", 0x2124 }, { "r", 0x1025 }, { "y", 0x202d },
        // Same native grain, longer duration; these are authored variants, not extra syllables.
        { "ouu", 0x2207 }, { "ee", 0x3209 }
    };

    // Each descriptor has one lossless spelling. Unlabelled original variants
    // retain their descriptor number; do not invent a French identity for them.
    static readonly string[] NativeNames =
    {
        "i", "et", "ai", "a", "son04", "oh", "o", "ou",
        "u", "e2", "e", "in", "an", "on", "son14", "son15",
        "son16", "son17", "son18", "son19", "son20", "son21", "p", "b",
        "m", "f", "v", "t", "d", "n", "s", "z",
        "l", "ch", "j", "c", "g", "r", "son38", "son39",
        "son40", "son41", "son42", "son43", "son44", "y", "son46", "son47",
        "son48", "son49", "son50", "son51", "son52", "son53", "son54", "son55",
        "son56", "son57", "son58", "son59", ",", ".", "fin", "son63"
    };

    int BankWord(int offset) => bank[offset] << 8 | bank[offset + 1];

    public string OriginalDialogue(int index)
    {
        if (index < 0 || index >= OriginalDialogueCount) throw new ArgumentOutOfRangeException(nameof(index));
        int start = BankWord(index * 2), end = BankWord(index * 2 + 2);
        var tokens = new ushort[(end - start) / 2];
        for (int i = 0; i < tokens.Length; i++) tokens[i] = (ushort)BankWord(0x12c + start + i * 2);
        return FormatDialogue(tokens);
    }

    // Optional [envelope, amplitude, rate] fields encode all 16 native bits:
    // 4 envelope bits, 3 amplitude bits, 3 rate bits, 6 descriptor bits.
    // Zero amplitude/rate inherits state; envelope is a shape code, not linear time.
    public static string FormatDialogue(IReadOnlyList<ushort> tokens)
    {
        var text = new StringBuilder();
        foreach (ushort token in tokens)
        {
            if (text.Length != 0) text.Append('_');
            text.Append(NativeNames[token & 63]).Append('[')
                .Append((token >> 12).ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(((token >> 9) & 7).ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(((token >> 6) & 7).ToString(CultureInfo.InvariantCulture)).Append(']');
        }
        return text.ToString();
    }

    static bool TryNativeToken(string text, out ushort token, out string error)
    {
        if (Phonemes.TryGetValue(text, out token)) { error = null; return true; }
        error = "Son inconnu : " + text;
        int bracket = text.IndexOf('[');
        if (bracket < 0) return false;
        error = text + " : écris son[forme,volume,vitesse], sans espace.";
        if (!text.EndsWith("]", StringComparison.Ordinal)) return false;
        string name = text.Substring(0, bracket);
        int descriptor = Array.IndexOf(NativeNames, name);
        if (descriptor < 0 && Phonemes.TryGetValue(name, out ushort basic)) descriptor = basic & 63;
        if (descriptor < 0)
        {
            error = name + " : utilise un seul son pour régler son expression.";
            return false;
        }
        string[] fields = text.Substring(bracket + 1, text.Length - bracket - 2).Split(',');
        if (fields.Length != 3) return false;
        int value = descriptor;
        for (int i = 0; i < 3; i++)
        {
            int max = i == 0 ? 15 : 7;
            if (!int.TryParse(fields[i], NumberStyles.None, CultureInfo.InvariantCulture, out int field) || field > max)
            {
                error = name + " : " + (i == 0 ? "forme" : i == 1 ? "volume" : "vitesse") + " doit être entre 0 et " + max + ".";
                return false;
            }
            value |= field << (i == 0 ? 12 : i == 1 ? 9 : 6);
        }
        token = (ushort)value;
        error = null;
        return true;
    }

    public static string TokenError(string token)
    {
        if (TryNativeToken(token, out _, out string error)) return null;
        if (token == "" || token == "," || token == "." || token == "oi" || token == "ui" ||
            token == "gn" || token == "ti" || token == "-" || token == "cuicui" || token == "pop") return null;
        return error;
    }

    public static bool IsKnownToken(string token) => TokenError(token) == null;

    public List<RodySpeechPart> RenderDialogue(string dialogue, Action<string> unknownToken,
        int selectionStart = 0, int selectionEnd = int.MaxValue)
    {
        var output = new List<RodySpeechPart>();
        foreach (var phrase in ParseDialogue(dialogue, unknownToken, selectionStart, selectionEnd))
            output.Add(phrase.Effect == RodySpeechEffect.None ? new RodySpeechPart(RenderSelection(phrase.Tokens, phrase.FirstToken, phrase.EndToken)) :
                new RodySpeechPart(phrase.Effect));
        return output;
    }

    public List<RodySpeechPhrase> ParseDialogue(string dialogue, Action<string> unknownToken,
        int selectionStart = 0, int selectionEnd = int.MaxValue)
    {
        var parts = new List<RodySpeechPhrase>();
        if (string.IsNullOrWhiteSpace(dialogue)) return parts;
        var tokens = new List<ushort>();
        var tiIndices = new List<int>();
        int atomStart = 0, atomEnd = 0, firstToken = -1, endToken = 0;
        bool Selected() => atomStart < selectionEnd && atomEnd > selectionStart;
        void Token(ushort token)
        {
            if (Selected())
            {
                if (firstToken < 0) firstToken = tokens.Count;
                endToken = tokens.Count + 1;
            }
            tokens.Add(token);
        }
        void Flush()
        {
            if (tokens.Count == 0) return;
            foreach (int index in tiIndices)
            {
                // "ti" is the bank-2 t recording. Encode the original bank-toggle
                // control, preserving t's duration and the following phoneme's context.
                int next = index + 1 < tokens.Count ? tokens[index + 1] & 63 : 0x3c;
                int p = next < 0x16 ? next : next - 0x16;
                int type = next >= 0x3c ? 9 : next < 0x16 ? (next < 14 ? 5 : 6) :
                    next <= 0x2f ? Table(0x4b10 + p) : 4;
                int vowel = ConsonantVowel(type, p);
                bool bank6 = Table(0x4b44 + vowel * 26 + 5) != 0;
                tokens[index] = (ushort)(bank6 ? 0x8a1b : 0x3a1b);
            }
            if (firstToken >= 0) parts.Add(new RodySpeechPhrase(tokens.ToArray(), firstToken, endToken));
            tokens.Clear();
            tiIndices.Clear();
            firstToken = -1;
            endToken = 0;
        }
        string[] words = dialogue.Split((char[])null);
        int wordStart = 0;
        for (int wordIndex = 0; wordIndex < words.Length; wordIndex++)
        {
            int phonemeStart = wordStart;
            foreach (string phoneme in words[wordIndex].Split('_'))
            {
                atomStart = Math.Min(phonemeStart, dialogue.Length - 1);
                atomEnd = atomStart + Math.Max(1, phoneme.Length);
                if (TryNativeToken(phoneme, out ushort token, out _)) Token(token);
                else switch (phoneme)
                {
                    case "":
                    case ",": Token(0x3c); break;
                    case ".": Token(0x3d); break;
                    case "oi": Token(Phonemes["ou"]); Token(Phonemes["a"]); break;
                    case "ui": Token(Phonemes["u"]); Token(Phonemes["i"]); break;
                    case "gn": Token(Phonemes["n"]); Token(Phonemes["y"]); break;
                    case "ti": tiIndices.Add(tokens.Count); Token(Phonemes["t"]); break;
                    case "-":
                    case "cuicui":
                    case "pop":
                        Flush();
                        if (Selected()) parts.Add(new RodySpeechPhrase(phoneme == "-" ? RodySpeechEffect.Noise :
                            phoneme == "cuicui" ? RodySpeechEffect.Bird : RodySpeechEffect.Pop));
                        break;
                    default:
                        // Existing imported stories treat unknown tokens as short pauses.
                        // Keep that behavior, but make bad authoring visible to the caller.
                        unknownToken(phoneme);
                        Token(0x3c);
                        break;
                }
                phonemeStart += phoneme.Length + 1;
            }
            atomStart = wordStart + words[wordIndex].Length;
            atomEnd = atomStart + 1;
            if (wordIndex + 1 < words.Length) Token(0x3c);
            wordStart = atomEnd;
        }
        Flush();
        return parts;
    }

    int ConsonantVowel(int type, int p) => type < 2 ? 10 : type == 2 ? 7 : type == 3 ? 8 :
        type == 6 ? Table(0x4b00 + ((p - 14) & 255) * 2) : type > 6 || type == 4 ? 10 : p;

    public byte[] Render(IReadOnlyList<ushort> tokens) => Interpret(Preprocess(tokens));

    // A selected passage is a slice of the full rendering. Trace the native
    // emit boundaries rather than guessing inherited controls from text fields.
    byte[] RenderSelection(ushort[] tokens, int first, int end)
    {
        if (first == 0 && end == tokens.Length) return Render(tokens);
        int commandStart = 0, commandEnd = 0;
        var commands = Preprocess(tokens, (token, offset) =>
        {
            if (token == first) commandStart = offset;
            if (token == end) commandEnd = offset;
        });
        int sampleStart = 0, sampleEnd = 0;
        var pcm = Interpret(commands, (command, offset) =>
        {
            if (command == commandStart) sampleStart = offset;
            if (command == commandEnd) sampleEnd = offset;
        });
        var selection = new byte[sampleEnd - sampleStart];
        Array.Copy(pcm, sampleStart, selection, 0, selection.Length);
        return selection;
    }

    public byte[] Preprocess(IReadOnlyList<ushort> tokens) => Preprocess(tokens, null);

    byte[] Preprocess(IReadOnlyList<ushort> tokens, Action<int, int> boundary)
    {
        var s = new int[18];
        var output = new List<byte>();
        void Add(int value) => output.Add((byte)value);
        void Unit(int op, int p, int variant) { Add(op); Add(p); Add(variant); }
        int Word(int offset) => s[offset] << 8 | s[offset + 1];
        void Controls(int word)
        {
            int lo = word & 255, hi = word >> 8 & 255;
            if (lo != 0) { Add(0x66); Add(lo - 4); }
            if (hi != 0) { Add(0x61); Add(hi - 1); }
        }
        int Vowel(int p) => Table(0x4b00 + ((p - 0xe) & 255) * 2);
        void Onset()
        {
            int next = s[13];
            if (next < 2 || next == 4 || next == 9) return;
            Controls(Word(14));
            int p = next < 5 ? (next == 3 ? 8 : 7) : next == 5 ? s[12] : Vowel(s[12]);
            if (p == 4) p = 3;
            if (Table(0x4b2a + s[6]) != 0) s[6] = (s[6] + 1) & 255;
            int consonant;
            if (s[7] == 4) consonant = Table(0x4aee + (s[6] - 0x1a) * 2 + 1);
            else
            {
                if (s[6] >= 0x11 && s[6] <= 0x12) s[6] = 0x10;
                consonant = s[6];
            }
            Unit(4, p, consonant);
        }
        void LowConsonant()
        {
            Controls(Word(8));
            int next = s[13];
            int p = ConsonantVowel(next, s[12]);
            int op = Table(0x4b44 + ((p * 26 + s[6]) & 65535)) != 0 ? 6 : 2;
            if (s[10] >= 5)
            {
                s[10] = (s[10] - 5) & 255;
                op = op == 2 ? 6 : 2;
            }
            if (s[7] != 0)
            {
                Unit(op, s[6], 3);
                for (int i = 0; i < s[10]; i++) Unit(op, s[6], 4);
            }
            else
            {
                if (s[6] == 0x16 && (s[0] == 10 || s[0] == 14 || s[0] == 16 || s[0] == 21))
                {
                    if (s[0] == 10) Unit(0, 16, 2);
                    s[7] = 9;
                    return;
                }
                for (int i = 0; i < s[10]; i++) Unit(op, s[6], 3);
                Unit(op, s[6], 4);
            }
            if (s[13] == 9) { Unit(op, s[6], 5); return; }
            if (s[6] == 0x16) { s[7] = 9; return; }
            Onset();
        }
        void Consonant()
        {
            if (s[7] < 2) { LowConsonant(); return; }
            if (s[7] == 4)
            {
                Controls(Word(8));
                for (int i = 0; i < s[10]; i++) Unit(2, s[6], 3);
                Unit(2, s[6], 4);
                if (s[13] == 9) { Unit(2, s[6], 5); return; }
                Onset();
                return;
            }
            int p = s[7] == 2 ? 7 : 8;
            if (s[1] >= 5)
            {
                Controls(Word(8));
                if (s[1] == 9) Unit(4, p, 0x16);
            }
            for (int i = 0; i < s[10]; i++) Unit(0, p, 3);
            Controls(Word(14));
            Unit(4, s[13] == 6 ? Vowel(s[12]) : s[12] == 4 ? 3 : s[12], s[6]);
        }
        void Emit()
        {
            if (s[7] == 9)
            {
                if (s[6] != 0x23)
                    for (int i = 0; i <= s[10]; i++) Add(s[6]);
                return;
            }
            if (s[7] < 5) { Consonant(); return; }
            int alternate = s[7] == 6 ? Vowel(s[6]) : 0;
            if (s[1] >= 5)
            {
                Controls(Word(8));
                if (s[1] == 9) Unit(4, alternate != 0 ? alternate : s[6], 0x16);
            }
            int repeat = s[10];
            if (repeat == 0)
            {
                Unit(0, s[6], alternate != 0 ? (s[13] == 9 ? 0 : 1) : (s[13] == 9 ? 2 : 4));
                return;
            }
            if (repeat <= 3)
            {
                for (int i = 0; i < repeat - 1; i++) Unit(0, alternate != 0 ? alternate : s[6], 3);
                Unit(0, s[6], s[13] == 9 ? 0 : 1);
                return;
            }
            if (repeat <= 6)
            {
                for (int i = 0; i < repeat - 4; i++) Unit(0, alternate != 0 ? alternate : s[6], 3);
                Unit(0, s[6], 3);
                return;
            }
            for (int i = 0; i < repeat - 7; i++) Unit(0, alternate != 0 ? alternate : s[6], 3);
            Unit(0, s[6], alternate != 0 ? 0 : 2);
        }
        void Shift()
        {
            Array.Copy(s, 6, s, 0, 12);
            Array.Clear(s, 12, 6);
            s[12] = 0x20;
            s[13] = 9;
        }
        s[6] = s[12] = 0x20;
        s[13] = 9;
        int processed = 0;
        boundary?.Invoke(0, 0);
        foreach (ushort token in tokens)
        {
            Shift();
            int p = token & 0x3f;
            if (p >= 0x3c && p <= 0x3e)
            {
                s[12] = p == 0x3c ? 0x20 : p == 0x3d ? 0x2e : 0x23;
                s[13] = 9;
            }
            else if (p < 0x16)
            {
                s[12] = p;
                s[13] = p < 0xe ? 5 : 6;
            }
            else
            {
                s[12] = p - 0x16;
                s[13] = p <= 0x2f ? Table(0x4b10 + s[12]) : 4;
            }
            s[16] = token >> 12;
            s[15] = token >> 6 & 7;
            s[14] = token >> 9 & 7;
            Emit();
            if (processed > 0) boundary?.Invoke(processed, output.Count);
            processed++;
        }
        Shift();
        Emit();
        boundary?.Invoke(tokens.Count, output.Count);
        Add(0x23);
        return output.ToArray();
    }

    public byte[] Interpret(IReadOnlyList<byte> commands) => Interpret(commands, null);

    byte[] Interpret(IReadOnlyList<byte> commands, Action<int, int> boundary)
    {
        var output = new List<byte>();
        int amplitude = 0;
        int delay = DefaultDelay;
        void Silence(int length)
        {
            for (int k = 0; k < length; k++) output.Add(128);
        }
        void Segment(int start, int end)
        {
            if (start < 0 || end >= clips.Length || clips[end] > bank.Length - AudioStart) return;
            // The original loop reads a sample before checking its end pointer.
            int n = Math.Max(1, clips[end] - clips[start]);
            int count = Math.Max(1, (int)Math.Round(n * (372.0 + 10 * delay) / (372 + 10 * DefaultDelay)));
            for (int k = 0; k < count; k++)
            {
                int sample = bank[AudioStart + clips[start] + Math.Min(n - 1, (int)((long)k * n / count))];
                sample -= 128;
                // Preserve the 68000's arithmetic-shift rounding, including odd samples.
                if (amplitude == 1) sample -= sample >> 1;
                else if (amplitude == 2) sample -= sample >> 2;
                else if (amplitude == 3) sample += sample >> 2;
                else if (amplitude >= 4) sample += sample >> 1;
                sample += 128;
                output.Add((byte)Math.Max(0, Math.Min(255, sample)));
            }
        }
        for (int i = 0; i < commands.Count;)
        {
            boundary?.Invoke(i, output.Count);
            int op = commands[i++];
            switch (op)
            {
                case 0x20: Silence(151); break;
                case 0x2e: Silence(4196); break;
                case 0x23: return output.ToArray();
                case 0x61:
                    amplitude = commands[i++];
                    break;
                case 0x66:
                    delay = Math.Max(0, 16 - unchecked((sbyte)(2 * NativeSpeed + commands[i++])));
                    break;
                case 0:
                case 2:
                case 6:
                    int p = commands[i++], variant = commands[i++];
                    if (variant > 5) throw new ArgumentException($"Unsupported speech variant: {variant}");
                    int offset = (op == 0 ? 0 : op == 2 ? 0x10c / 4 : 0x2b4 / 4) + 3 * p;
                    int start = variant == 2 || variant == 4 ? 1 : variant == 5 ? 2 : 0;
                    int end = variant == 1 || variant == 4 ? 2 : variant == 3 ? 1 : 3;
                    Segment(offset + start, offset + end);
                    break;
                case 4:
                    int vowel = commands[i++], consonant = commands[i++];
                    int index = 0x45c / 4 + vowel + 14 * consonant;
                    Segment(index, index + 1);
                    break;
                default: throw new ArgumentException($"Unsupported speech command: {op}");
            }
        }
        return output.ToArray();
    }
}

public enum RodySpeechEffect { None, Noise, Bird, Pop }

public readonly struct RodySpeechPart
{
    public readonly byte[] Samples;
    public readonly RodySpeechEffect Effect;

    public RodySpeechPart(byte[] samples) { Samples = samples; Effect = RodySpeechEffect.None; }
    public RodySpeechPart(RodySpeechEffect effect) { Samples = null; Effect = effect; }
}

// Parsed score segments are native instructions; rendered parts are derived PCM.
public readonly struct RodySpeechPhrase
{
    public readonly ushort[] Tokens;
    public readonly RodySpeechEffect Effect;

    public readonly int FirstToken, EndToken;

    public RodySpeechPhrase(ushort[] tokens, int first, int end)
    { Tokens = tokens; Effect = RodySpeechEffect.None; FirstToken = first; EndToken = end; }
    public RodySpeechPhrase(RodySpeechEffect effect)
    { Tokens = null; Effect = effect; FirstToken = EndToken = 0; }
}
