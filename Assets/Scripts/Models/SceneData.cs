using System;
using UnityEngine;

/// <summary>
/// Data model for a game scene, replacing magic string array indices.
/// Contains all data needed to play a scene: dialogues, texts, music, voice settings, and objects.
/// </summary>
[Serializable]
public class SceneData
{
    /// <summary>
    /// Phoneme strings for text-to-speech dialogues.
    /// </summary>
    public PhonemeDialogues dialogues;
    
    /// <summary>
    /// Display text shown to the player.
    /// </summary>
    public DisplayTexts texts;
    
    /// <summary>
    /// Music settings for intro and scene.
    /// </summary>
    public MusicSettings music;
    
    /// <summary>
    /// Voice synthesis settings (pitch, speaker assignment).
    /// </summary>
    public VoiceSettings voice;
    
    /// <summary>
    /// Clickable object zones for the three game objects.
    /// </summary>
    public ObjectZones objects;
    
    /// <summary>
    /// Creates an empty SceneData with default values.
    /// </summary>
    public SceneData()
    {
        dialogues = new PhonemeDialogues();
        texts = new DisplayTexts();
        music = new MusicSettings();
        voice = new VoiceSettings();
        objects = new ObjectZones();
    }
}

/// <summary>
/// Phoneme dialogue strings for each scene phase.
/// These are converted to phoneme lists for TTS playback.
/// </summary>
[Serializable]
public class PhonemeDialogues
{
    public string intro1 = "";      // First intro speaker
    public string intro2 = "";      // Second intro speaker (response)
    public string intro3 = "";      // Third intro speaker (if any)
    public string obj = "";         // Object hint dialogue
    public string ngp = "";         // Near game piece dialogue
    public string fsw = "";         // Final scene win dialogue
    
    /// <summary>
    /// Default glitch dialogues used when loading fails.
    /// </summary>
    public static PhonemeDialogues Glitch => new PhonemeDialogues
    {
        intro1 = "g_l_i_t_ch",
        intro2 = "g_l_i_t_ch",
        intro3 = "",
        obj = "g_l_i_t_ch",
        ngp = "g_l_i_t_ch",
        fsw = "g_l_i_t_ch"
    };
}

/// <summary>
/// Display text shown on screen for each scene phase.
/// </summary>
[Serializable]
public class DisplayTexts
{
    public string title = "";       // Scene title
    public string intro = "";       // Introductory text
    public string obj = "";         // Object to find description
    public string ngp = "";         // NGP object description
    public string fsw = "";         // FSW object description
    
    /// <summary>
    /// Default glitch texts used when loading fails.
    /// </summary>
    public static DisplayTexts Glitch => new DisplayTexts
    {
        title = "Glitch",
        intro = "intro glitch",
        obj = "object glitch",
        ngp = "ngp object glitch",
        fsw = "fsw object glitch"
    };
}

/// <summary>
/// Music settings for intro and scene loop.
/// </summary>
[Serializable]
public class MusicSettings
{
    public string introMusic = "i1";    // Music during intro sequence
    public string sceneMusic = "l1";    // Music during gameplay
}

/// <summary>
/// Voice synthesis settings for the three speakers.
/// </summary>
[Serializable]
public class VoiceSettings
{
    // Pitch modifiers for each speaker (1.0 = normal)
    public float pitch1 = 1f;
    public float pitch2 = 1f;
    public float pitch3 = 1f;
    
    // Whether each speaker is Mastico (true) or another character (false)
    public bool isMastico1 = false;
    public bool isMastico2 = false;
    public bool isMastico3 = false;
    
    // Special Zambla mode
    public bool isZambla = false;
}

/// <summary>
/// Clickable zones for the three findable objects in a scene.
/// </summary>
[Serializable]
public class ObjectZones
{
    public ObjectZone obj;      // Primary object
    public ObjectZone ngp;      // Near game piece
    public ObjectZone fsw;      // Final scene win
    
    public ObjectZones()
    {
        obj = new ObjectZone();
        ngp = new ObjectZone();
        fsw = new ObjectZone();
    }
}

/// <summary>
/// A single clickable object zone with position and size.
/// The zone is the exact clickable area, nearZone is for "getting warmer" feedback.
/// </summary>
[Serializable]
public class ObjectZone
{
    // The clickable target zone
    public float x;
    public float y;
    public float width;
    public float height;

    // The "getting warmer" zone (slightly bigger, around the target)
    public float nearX;
    public float nearY;
    public float nearWidth;
    public float nearHeight;
}
