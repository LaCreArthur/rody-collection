using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

// All commands execute the very same engine source that Unity compiles.
var root = new DirectoryInfo(AppContext.BaseDirectory);
while (root != null && !Directory.Exists(Path.Combine(root.FullName, "Assets", "Resources", "Speech"))) root = root.Parent;
if (root == null) throw new Exception("Run this tool from its RodyMaker checkout.");
string RootPath(params string[] path) => Path.Combine(new[] { root.FullName }.Concat(path).ToArray());
var engine = new RodySpeechEngine(File.ReadAllBytes(RootPath("Assets", "Resources", "Speech", "Rody1.bytes")),
    File.ReadAllBytes(RootPath("Assets", "Resources", "Speech", "Tables.bytes")));
void Unknown(string token) => throw new Exception($"Unknown speech token: '{token}'");
if (args.Length >= 2 && args[0] == "verify")
{
    string folder = Path.GetFullPath(args[1]);
    using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "manifest.json")));
    long samples = 0;
    int count = 0;
    foreach (var item in manifest.RootElement.GetProperty("records").EnumerateArray())
    {
        string id = item.GetProperty("id").GetString();
        ushort[] tokens = item.GetProperty("tokens").EnumerateArray().Select(x => x.GetUInt16()).ToArray();
        var commands = engine.Preprocess(tokens);
        var pcm = engine.Render(tokens);
        string notation = engine.OriginalDialogue(count);
        var parsed = engine.ParseDialogue(notation, Unknown);
        if (parsed.Count != 1 || !parsed[0].Tokens.SequenceEqual(tokens))
            throw new Exception($"{id}: editable original lost native controls");
        if (!engine.RenderDialogue(notation, Unknown).SelectMany(p => p.Samples).SequenceEqual(pcm))
            throw new Exception($"{id}: editable original PCM differs");
        if (!commands.SequenceEqual(File.ReadAllBytes(Path.Combine(folder, id + ".commands"))))
            throw new Exception($"{id}: command bytes differ");
        if (!pcm.SequenceEqual(File.ReadAllBytes(Path.Combine(folder, id + ".pcm"))))
            throw new Exception($"{id}: PCM bytes differ");
        samples += pcm.Length;
        count++;
    }
    Console.WriteLine($"{count} reference records: byte-identical commands and PCM ({samples} samples).");
    var allTokens = Enumerable.Range(0, 65536).Select(value => (ushort)value).ToArray();
    var allParsed = engine.ParseDialogue(RodySpeechEngine.FormatDialogue(allTokens), Unknown);
    if (allParsed.Count != 1 || !allParsed[0].Tokens.SequenceEqual(allTokens))
        throw new Exception("Full notation lost a native token bit");
    Console.WriteLine("All 65,536 native token values survive editable notation; all original dialogues retain PCM.");
    foreach (string invalid in new[] { "on[16,1,0]", "on[1,8,0]", "on[1,1,8]", "on[-1,1,0]",
        "on[1,1]", "on[1,1,0]x", "on[1,1,0,0]", "on[x,1,0]", "oi[1,1,0]", "pop[1,1,0]", "son64[1,1,0]" })
        if (RodySpeechEngine.IsKnownToken(invalid)) throw new Exception("Invalid expression accepted: " + invalid);
    // Splitting a full score into its atom selections must partition its PCM,
    // including repeated envelopes, silent markers, aliases and effect boundaries.
    foreach (string score in new[] { engine.OriginalDialogue(0), engine.OriginalDialogue(25),
        "r[1,5,3]_o[4,0,0]_o[1,0,5]", "b_oi_r", "a[1,1,0]_-_i[1,0,5]_pop" })
    {
        var whole = engine.RenderDialogue(score, Unknown);
        var pieces = new List<RodySpeechPart>();
        int position = 0;
        foreach (string atom in score.Split('_'))
        {
            pieces.AddRange(engine.RenderDialogue(score, Unknown, position, position + atom.Length));
            position += atom.Length + 1;
        }
        if (!pieces.SelectMany(p => p.Samples ?? Array.Empty<byte>()).SequenceEqual(
            whole.SelectMany(p => p.Samples ?? Array.Empty<byte>())) ||
            !pieces.Where(p => p.Effect != RodySpeechEffect.None).Select(p => p.Effect).SequenceEqual(
                whole.Where(p => p.Effect != RodySpeechEffect.None).Select(p => p.Effect)))
            throw new Exception("Passage selections changed full-context audio");
    }
    Console.WriteLine("Selected passages partition full-context PCM and preserve effect order.");
    int dialogues = 0;
    foreach (var item in manifest.RootElement.GetProperty("notation").EnumerateArray())
    {
        string text = item.GetProperty("text").GetString();
        int start = item.TryGetProperty("start", out var startValue) ? startValue.GetInt32() : 0;
        int end = item.TryGetProperty("end", out var endValue) ? endValue.GetInt32() : int.MaxValue;
        var parts = engine.RenderDialogue(text, Unknown, start, end);
        string id = item.GetProperty("id").GetString();
        if (item.TryGetProperty("expected", out var expected))
        {
            byte[] pcm = parts.SelectMany(p => p.Samples ?? Array.Empty<byte>()).ToArray();
            if (!pcm.SequenceEqual(File.ReadAllBytes(Path.Combine(folder, expected.GetString()))))
                throw new Exception($"{id}: authored notation PCM differs");
        }
        if (item.TryGetProperty("effects", out var effects))
        {
            var actual = parts.Where(p => p.Effect != RodySpeechEffect.None).Select(p => p.Effect.ToString());
            if (!actual.SequenceEqual(effects.EnumerateArray().Select(e => e.GetString())))
                throw new Exception($"{id}: effect order differs");
        }
        dialogues++;
    }
    Console.WriteLine($"{dialogues} authored dialogue/notation cases rendered with no unknown tokens; specified PCM/effect checks passed.");
}
else if (args.Length >= 2 && args[0] == "validate")
{
    var errors = args[1].Split((char[])null).SelectMany(word => word.Split('_'))
        .Select(RodySpeechEngine.TokenError).Where(error => error != null).Distinct().ToArray();
    foreach (string error in errors) Console.WriteLine(error);
    if (errors.Length != 0) Environment.ExitCode = 1;
    else Console.WriteLine("OK: all tokens valid");
}
else if (args.Length >= 2 && args[0] == "audit-original")
{
    string folder = Path.GetFullPath(args[1]);
    using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "manifest.json")));
    int count = 0;
    foreach (var item in manifest.RootElement.GetProperty("records").EnumerateArray())
    {
        string id = item.GetProperty("id").GetString();
        ushort[] tokens = item.GetProperty("tokens").EnumerateArray().Select(x => x.GetUInt16()).ToArray();
        if (!engine.Preprocess(tokens).SequenceEqual(File.ReadAllBytes(Path.Combine(folder, id + ".commands"))))
            throw new Exception($"{id}: original 68000 command bytes differ");
        count++;
    }
    // A single synthetic 256-byte grain exercises the production interpreter,
    // comparing to samples captured after original 68000 amplitude arithmetic.
    byte[] bank = new byte[0x2f70 + 256];
    bank[0x2560 + 3 * 4 + 2] = 1; // big-endian clip boundary 256
    for (int i = 0; i < 256; i++) bank[0x2f70 + i] = (byte)i;
    var samplesEngine = new RodySpeechEngine(bank, Array.Empty<byte>());
    for (byte level = 0; level < 5; level++)
    {
        var actual = samplesEngine.Interpret(new byte[] { 0x61, level, 0, 0, 0, 0x23 });
        if (!actual.SequenceEqual(File.ReadAllBytes(Path.Combine(folder, $"amplitude-{level}.pcm"))))
            throw new Exception($"Amplitude {level}: original 68000 sample bytes differ");
    }
    Console.WriteLine($"{count} original 68000 command streams match C#; all 1,280 amplitude samples match.");
}
else if (args.Length >= 3 && (args[0] == "render" || args[0] == "render-original"))
{
    double pitch = args.Length > 3 ? double.Parse(args[3], CultureInfo.InvariantCulture) : 1;
    if (pitch <= 0 || pitch > 3) throw new ArgumentOutOfRangeException("pitch", "Use a positive pitch up to 3.");
    string dialogue = args[0] == "render-original" ? engine.OriginalDialogue(int.Parse(args[2], CultureInfo.InvariantCulture)) : args[2];
    var output = new List<short>();
    foreach (var part in engine.RenderDialogue(dialogue, Unknown))
    {
        float[] input;
        int rate;
        if (part.Effect == RodySpeechEffect.None)
        {
            input = part.Samples.Select(x => (x - 128) / 128f).ToArray();
            rate = RodySpeechEngine.SampleRate;
        }
        else
        {
            // The prefab's named effect fields own the references; don't duplicate the asset map here.
            string prefab = File.ReadAllText(RootPath("Assets", "Prefabs", "SoundManager.prefab"));
            string field = part.Effect.ToString().ToLowerInvariant();
            string guid = Regex.Match(prefab, @"(?m)^  " + field + @": .*guid: ([a-f0-9]+)").Groups[1].Value;
            string meta = Directory.EnumerateFiles(RootPath("Assets", "Sounds"), "*.wav.meta", SearchOption.AllDirectories)
                .Single(path => File.ReadAllText(path).Contains("guid: " + guid));
            (input, rate) = ReadWave(meta.Substring(0, meta.Length - 5));
        }
        int count = (int)Math.Round(input.Length * 44100.0 / rate / pitch);
        for (int i = 0; i < count; i++)
        {
            double position = i * (double)rate * pitch / 44100.0;
            int lo = Math.Min(input.Length - 1, (int)position), hi = Math.Min(input.Length - 1, lo + 1);
            double sample = input[lo] + (input[hi] - input[lo]) * (position - lo);
            output.Add((short)Math.Clamp((int)Math.Round(sample * 32768), short.MinValue, short.MaxValue));
        }
    }
    string path = Path.GetFullPath(args[1]);
    Directory.CreateDirectory(Path.GetDirectoryName(path));
    if (args[0] == "render-original") File.WriteAllText(Path.ChangeExtension(path, ".txt"), dialogue);
    using var writer = new BinaryWriter(File.Create(path));
    writer.Write(Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + output.Count * 2);
    writer.Write(Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
    writer.Write((ushort)1); writer.Write((ushort)1); writer.Write(44100); writer.Write(88200);
    writer.Write((ushort)2); writer.Write((ushort)16);
    writer.Write(Encoding.ASCII.GetBytes("data")); writer.Write(output.Count * 2);
    foreach (short sample in output) writer.Write(sample);
    Console.WriteLine($"{path}: {output.Count / 44100.0:F2}s at pitch {pitch}");
}
else throw new ArgumentException("Usage: render out.wav \"phonemes\" [pitch] | validate \"phonemes\" | render-original out.wav record-index [pitch] | verify fixture-directory | audit-original fixture-directory");

static (float[] samples, int rate) ReadWave(string path)
{
    using var reader = new BinaryReader(File.OpenRead(path));
    if (new string(reader.ReadChars(4)) != "RIFF") throw new Exception("Expected RIFF: " + path);
    reader.ReadInt32();
    if (new string(reader.ReadChars(4)) != "WAVE") throw new Exception("Expected WAVE: " + path);
    int channels = 0, rate = 0;
    while (reader.BaseStream.Position + 8 <= reader.BaseStream.Length)
    {
        string chunk = new string(reader.ReadChars(4));
        int length = reader.ReadInt32();
        long next = reader.BaseStream.Position + length + (length & 1);
        if (chunk == "fmt ")
        {
            if (reader.ReadUInt16() != 1) throw new Exception("Expected PCM: " + path);
            channels = reader.ReadUInt16(); rate = reader.ReadInt32();
            reader.ReadInt32(); reader.ReadUInt16();
            if (reader.ReadUInt16() != 16) throw new Exception("Expected 16-bit PCM: " + path);
        }
        else if (chunk == "data")
        {
            if (channels < 1) throw new Exception("Missing format: " + path);
            var samples = new float[length / 2 / channels];
            for (int i = 0; i < samples.Length; i++)
                for (int c = 0; c < channels; c++) samples[i] += reader.ReadInt16() / (32768f * channels);
            return (samples, rate);
        }
        reader.BaseStream.Position = next;
    }
    throw new Exception("Missing PCM: " + path);
}
