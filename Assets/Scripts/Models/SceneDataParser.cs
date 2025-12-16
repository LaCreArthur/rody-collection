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
        data.texts.intro = raw[IDX_INTRO_TEXT] ?? "";
        data.texts.obj = raw[IDX_OBJ_TEXT] ?? "";
        data.texts.ngp = raw[IDX_NGP_TEXT] ?? "";
        data.texts.fsw = raw[IDX_FSW_TEXT] ?? "";
        
        // Parse music (comma-separated: intro,scene)
        ParseMusic(raw[IDX_MUSIC], data.music);
        
        // Parse voice settings
        ParseVoice(raw[IDX_PITCHES], raw[IDX_SPEAKERS], data.voice);
        
        // Parse object zones
        data.objects.obj.positionRaw = raw[IDX_OBJ_POS] ?? "";
        data.objects.obj.sizeRaw = raw[IDX_OBJ_SIZE] ?? "";
        data.objects.obj.nearPositionRaw = raw[IDX_OBJ_NEAR_POS] ?? "";
        data.objects.obj.nearSizeRaw = raw[IDX_OBJ_NEAR_SIZE] ?? "";
        
        data.objects.ngp.positionRaw = raw[IDX_NGP_POS] ?? "";
        data.objects.ngp.sizeRaw = raw[IDX_NGP_SIZE] ?? "";
        data.objects.ngp.nearPositionRaw = raw[IDX_NGP_NEAR_POS] ?? "";
        data.objects.ngp.nearSizeRaw = raw[IDX_NGP_NEAR_SIZE] ?? "";
        
        data.objects.fsw.positionRaw = raw[IDX_FSW_POS] ?? "";
        data.objects.fsw.sizeRaw = raw[IDX_FSW_SIZE] ?? "";
        data.objects.fsw.nearPositionRaw = raw[IDX_FSW_NEAR_POS] ?? "";
        data.objects.fsw.nearSizeRaw = raw[IDX_FSW_NEAR_SIZE] ?? "";
        
        return data;
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
    /// Creates a fallback scene with glitch data, used when loading fails.
    /// </summary>
    public static SceneData CreateGlitchScene()
    {
        var data = new SceneData();
        data.dialogues = PhonemeDialogues.Glitch;
        data.texts = DisplayTexts.Glitch;
        return data;
    }
}
