/// <summary>
/// Named build-scene indices, replacing bare integer literals scattered across
/// the gameplay and editor flow. Order matches Build Settings.
/// </summary>
public static class AppScenes
{
    public const int Selection = 0; // 0_MenuCollection (story selection)
    public const int Title = 1;     // 1_TitleScene
    public const int Menu = 2;      // 2_MenuScene
    public const int Game = 3;      // 3_GameScene
    public const int Credits = 4;   // 4_CreditsScene
    public const int Win = 5;       // 5_WinScene
    public const int Editor = 6;    // RM_Main (level editor)
    public const int Phonemes = 7;  // additive phoneme editor
}
