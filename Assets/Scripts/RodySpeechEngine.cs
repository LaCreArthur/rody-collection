using System;
using System.Collections.Generic;

// Port of tools/original-extraction/preprocess.py and render_all.py.
// All samples are unsigned 8-bit PCM; Unity converts them only at the playback boundary.
public sealed class RodySpeechEngine
{
    public const int SampleRate = 13000;
    const int ClipTable = 0x2560;
    const int AudioStart = 0x2f70;
    const int TableStart = 0x4b00;
    const int DefaultDelay = 18;
    readonly byte[] bank;
    readonly byte[] tables;
    readonly int[] clips = new int[644];

    public RodySpeechEngine(byte[] bank, byte[] tables)
    {
        this.bank = bank;
        this.tables = tables;
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

    public static bool IsKnownToken(string token) => Phonemes.ContainsKey(token) ||
        token == "" || token == "," || token == "." || token == "oi" || token == "ui" ||
        token == "gn" || token == "ti" || token == "-" || token == "cuicui" || token == "pop";

    public List<RodySpeechPart> RenderDialogue(string dialogue, Action<string> unknownToken)
    {
        var parts = new List<RodySpeechPart>();
        if (string.IsNullOrWhiteSpace(dialogue)) return parts;
        var tokens = new List<ushort>();
        var tiIndices = new List<int>();
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
            parts.Add(new RodySpeechPart(Render(tokens)));
            tokens.Clear();
            tiIndices.Clear();
        }
        foreach (string word in dialogue.Split((char[])null))
        {
            foreach (string phoneme in word.Split('_'))
            {
                if (Phonemes.TryGetValue(phoneme, out ushort token)) tokens.Add(token);
                else switch (phoneme)
                {
                    case "":
                    case ",": tokens.Add(0x3c); break;
                    case ".": tokens.Add(0x3d); break;
                    case "oi": tokens.Add(Phonemes["ou"]); tokens.Add(Phonemes["a"]); break;
                    case "ui": tokens.Add(Phonemes["u"]); tokens.Add(Phonemes["i"]); break;
                    case "gn": tokens.Add(Phonemes["n"]); tokens.Add(Phonemes["y"]); break;
                    case "ti": tiIndices.Add(tokens.Count); tokens.Add(Phonemes["t"]); break;
                    case "-":
                    case "cuicui":
                    case "pop":
                        Flush();
                        parts.Add(new RodySpeechPart(phoneme == "-" ? RodySpeechEffect.Noise :
                            phoneme == "cuicui" ? RodySpeechEffect.Bird : RodySpeechEffect.Pop));
                        break;
                    default:
                        // Existing imported stories treat unknown tokens as short pauses.
                        // Keep that behavior, but make bad authoring visible to the caller.
                        unknownToken(phoneme);
                        tokens.Add(0x3c);
                        break;
                }
            }
            tokens.Add(0x3c);
        }
        Flush();
        return parts;
    }

    int ConsonantVowel(int type, int p) => type < 2 ? 10 : type == 2 ? 7 : type == 3 ? 8 :
        type == 6 ? Table(0x4b00 + ((p - 14) & 255) * 2) : type > 6 || type == 4 ? 10 : p;

    public byte[] Render(IReadOnlyList<ushort> tokens) => Interpret(Preprocess(tokens));

    public byte[] Preprocess(IReadOnlyList<ushort> tokens)
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
            if (s[6] >= 0x11 && s[6] <= 0x12) s[6] = 0x10;
            Unit(4, p, s[6]);
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
                    Unit(0, 16, 2);
                    s[7] = 9;
                    return;
                }
                for (int i = 0; i < s[10]; i++) Unit(op, s[6], 3);
                Unit(op, s[6], 4);
            }
            if (s[13] == 9) Unit(op, s[6], 5);
            Onset();
        }
        void Consonant()
        {
            if (s[7] < 2) { LowConsonant(); return; }
            if (s[7] == 4)
            {
                Controls(Word(8));
                for (int i = 0; i < s[10]; i++) Unit(2, s[6], 0xaa);
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
        }
        Shift();
        Emit();
        Add(0x23);
        return output.ToArray();
    }

    public byte[] Interpret(IReadOnlyList<byte> commands)
    {
        var output = new List<byte>();
        double amplitude = 1;
        int delay = DefaultDelay;
        void Silence(int length)
        {
            for (int k = 0; k < length; k++) output.Add(128);
        }
        void Segment(int start, int end)
        {
            if (start < 0 || end >= clips.Length || clips[end] > bank.Length - AudioStart) return;
            int n = clips[end] - clips[start];
            if (n <= 0) return;
            int count = Math.Max(1, (int)Math.Round(n * (372.0 + 10 * delay) / (372 + 10 * DefaultDelay)));
            for (int k = 0; k < count; k++)
            {
                int sample = bank[AudioStart + clips[start] + Math.Min(n - 1, (int)((long)k * n / count))];
                sample = (int)((sample - 128) * amplitude) + 128;
                output.Add((byte)Math.Max(0, Math.Min(255, sample)));
            }
        }
        for (int i = 0; i < commands.Count;)
        {
            int op = commands[i++];
            switch (op)
            {
                case 0x20: Silence(151); break;
                case 0x2e: Silence(4196); break;
                case 0x61:
                    int level = commands[i++];
                    amplitude = level == 0 ? 1 : level == 1 ? 0.5 : level == 2 ? 0.75 : level == 3 ? 1.25 : 1.5;
                    break;
                case 0x66:
                    delay = Math.Max(0, DefaultDelay - (sbyte)commands[i++]);
                    break;
                case 0:
                case 2:
                case 6:
                    int p = commands[i++], variant = commands[i++];
                    if (variant > 5) break;
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
