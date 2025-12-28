using System;

/// <summary>
/// Parses scene data from the levels.rody text format into SceneData objects.
/// Centralizes all the magic index logic in one place.
/// </summary>
public static class SceneDataParser
{
    // Magic indices from levels.rody file format
    private const int IDX_INTRO_DIAL_1 = 0;
    private const int IDX_INTRO_DIAL_2 = 1;
    private const int IDX_INTRO_DIAL_3 = 2;
    private const int IDX_OBJ_DIAL = 3;
    private const int IDX_NGP_DIAL = 4;
    private const int IDX_FSW_DIAL = 5;
    private const int IDX_TITLE_TEXT = 6;
    private const int IDX_INTRO_TEXT = 7;
    private const int IDX_OBJ_TEXT = 8;
    private const int IDX_NGP_TEXT = 9;
    private const int IDX_FSW_TEXT = 10;
    private const int IDX_MUSIC = 11;
    private const int IDX_PITCHES = 12;
    private const int IDX_SPEAKERS = 13;
    private const int IDX_OBJ_POS = 14;
    private const int IDX_OBJ_SIZE = 15;
    private const int IDX_OBJ_NEAR_POS = 16;
    private const int IDX_OBJ_NEAR_SIZE = 17;
    private const int IDX_NGP_POS = 18;
    private const int IDX_NGP_SIZE = 19;
    private const int IDX_NGP_NEAR_POS = 20;
    private const int IDX_NGP_NEAR_SIZE = 21;
    private const int IDX_FSW_POS = 22;
    private const int IDX_FSW_SIZE = 23;
    private const int IDX_FSW_NEAR_POS = 24;
    private const int IDX_FSW_NEAR_SIZE = 25;
    
    /// <summary>
    /// Parses a raw string array from LoadSceneTxt into a structured SceneData object.
    /// </summary>
    public static SceneData Parse(string[] raw)
    {
        if (raw == null || raw.Length < 26)
        {
            UnityEngine.Debug.LogError("SceneDataParser: Invalid raw data, returning glitch scene");
            return CreateGlitchScene();
        }
        
        var data = new SceneData();
        
        // Parse dialogues
        data.dialogues.intro1 = raw[IDX_INTRO_DIAL_1] ?? "";
        data.dialogues.intro2 = raw[IDX_INTRO_DIAL_2] ?? "";
        data.dialogues.intro3 = raw[IDX_INTRO_DIAL_3] ?? "";
        data.dialogues.obj = raw[IDX_OBJ_DIAL] ?? "";
        data.dialogues.ngp = raw[IDX_NGP_DIAL] ?? "";
        data.dialogues.fsw = raw[IDX_FSW_DIAL] ?? "";
        
        // Parse display texts
        data.texts.title = raw[IDX_TITLE_TEXT] ?? "";
        ParseIntroTexts(raw[IDX_INTRO_TEXT], data.texts);
        data.texts.obj = raw[IDX_OBJ_TEXT] ?? "";
        data.texts.ngp = raw[IDX_NGP_TEXT] ?? "";
        data.texts.fsw = raw[IDX_FSW_TEXT] ?? "";
        
        // Parse music (comma-separated: intro,scene)
        ParseMusic(raw[IDX_MUSIC], data.music);
        
        // Parse voice settings
        ParseVoice(raw[IDX_PITCHES], raw[IDX_SPEAKERS], data.voice);
        
        // Parse object zones into typed floats
        ParseObjectZone(data.objects.obj,
            raw[IDX_OBJ_POS], raw[IDX_OBJ_SIZE],
            raw[IDX_OBJ_NEAR_POS], raw[IDX_OBJ_NEAR_SIZE]);

        ParseObjectZone(data.objects.ngp,
            raw[IDX_NGP_POS], raw[IDX_NGP_SIZE],
            raw[IDX_NGP_NEAR_POS], raw[IDX_NGP_NEAR_SIZE]);

        ParseObjectZone(data.objects.fsw,
            raw[IDX_FSW_POS], raw[IDX_FSW_SIZE],
            raw[IDX_FSW_NEAR_POS], raw[IDX_FSW_NEAR_SIZE]);

        return data;
    }

    /// <summary>
    /// Parses raw position/size strings into typed ObjectZone floats.
    /// Format: "(x, y);" for position, "(width, height);" for size.
    /// </summary>
    private static void ParseObjectZone(ObjectZone zone, string posRaw, string sizeRaw, string nearPosRaw, string nearSizeRaw)
    {
        // Parse main zone
        var pos = ParseVector(posRaw);
        var size = ParseVector(sizeRaw);
        zone.x = pos.x;
        zone.y = pos.y;
        zone.width = size.x;
        zone.height = size.y;

        // Parse near zone
        var nearPos = ParseVector(nearPosRaw);
        var nearSize = ParseVector(nearSizeRaw);
        zone.nearX = nearPos.x;
        zone.nearY = nearPos.y;
        zone.nearWidth = nearSize.x;
        zone.nearHeight = nearSize.y;
    }

    /// <summary>
    /// Parses a raw string like "(x, y);" into a tuple of floats.
    /// </summary>
    private static (float x, float y) ParseVector(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return (0, 0);

        // Remove trailing semicolon and parentheses
        raw = raw.Trim().TrimEnd(';').Trim('(', ')');

        var parts = raw.Split(',');
        if (parts.Length < 2) return (0, 0);

        float.TryParse(parts[0].Trim(), out float x);
        float.TryParse(parts[1].Trim(), out float y);

        return (x, y);
    }
    
    private static void ParseMusic(string raw, MusicSettings music)
    {
        if (string.IsNullOrEmpty(raw)) return;
        
        var parts = raw.Split(',');
        if (parts.Length >= 1) music.introMusic = parts[0].Trim();
        if (parts.Length >= 2) music.sceneMusic = parts[1].Trim();
    }
    
    private static void ParseVoice(string pitchesRaw, string speakersRaw, VoiceSettings voice)
    {
        // Parse pitches
        if (!string.IsNullOrEmpty(pitchesRaw))
        {
            var pitches = pitchesRaw.Split(',');
            if (pitches.Length >= 1) float.TryParse(pitches[0], out voice.pitch1);
            if (pitches.Length >= 2) float.TryParse(pitches[1], out voice.pitch2);
            if (pitches.Length >= 3) float.TryParse(pitches[2], out voice.pitch3);
        }

        // Parse speaker flags (1 = Mastico, 0 = other)
        if (!string.IsNullOrEmpty(speakersRaw))
        {
            var speakers = speakersRaw.Split(',');
            if (speakers.Length >= 1) voice.isMastico1 = speakers[0] == "1";
            if (speakers.Length >= 2) voice.isMastico2 = speakers[1] == "1";
            if (speakers.Length >= 3) voice.isMastico3 = speakers[2] == "1";
            if (speakers.Length >= 4) voice.isZambla = speakers[3] == "1";
        }
    }

    /// <summary>
    /// Parses old combined intro text format into separate intro1/2/3 fields.
    /// Old format: "Dialog 1" "Dialog 2" "Dialog 3" (quotes around each dialog)
    /// </summary>
    private static void ParseIntroTexts(string raw, DisplayTexts texts)
    {
        if (string.IsNullOrEmpty(raw))
        {
            texts.intro1 = "";
            texts.intro2 = "";
            texts.intro3 = "";
            return;
        }

        // Split by quotes - odd indices contain the dialog text
        // Example: '"Dialog1" "Dialog2"' splits to ['', 'Dialog1', ' ', 'Dialog2', '']
        var parts = raw.Split('"');

        texts.intro1 = parts.Length > 1 ? parts[1] : raw; // If no quotes, use whole string
        texts.intro2 = parts.Length > 3 ? parts[3] : "";
        texts.intro3 = parts.Length > 5 ? parts[5] : "";
    }
    
    /// <summary>
    /// Creates a fallback scene with glitch data, used when loading fails.
    /// </summary>
    public static SceneData CreateGlitchScene()
    {
        var data = new SceneData();
        data.dialogues = PhonemeDialogues.Glitch;
        data.texts = DisplayTexts.Glitch;

        // Set default object zones
        data.objects.obj.x = 0; data.objects.obj.y = 0;
        data.objects.obj.width = 10; data.objects.obj.height = 10;
        data.objects.obj.nearX = 0; data.objects.obj.nearY = 0;
        data.objects.obj.nearWidth = 20; data.objects.obj.nearHeight = 20;

        data.objects.ngp.x = 50; data.objects.ngp.y = 0;
        data.objects.ngp.width = 10; data.objects.ngp.height = 10;
        data.objects.ngp.nearX = 50; data.objects.ngp.nearY = 0;
        data.objects.ngp.nearWidth = 20; data.objects.ngp.nearHeight = 20;

        data.objects.fsw.x = 100; data.objects.fsw.y = 0;
        data.objects.fsw.width = 10; data.objects.fsw.height = 10;
        data.objects.fsw.nearX = 100; data.objects.fsw.nearY = 0;
        data.objects.fsw.nearWidth = 20; data.objects.fsw.nearHeight = 20;

        return data;
    }
}
