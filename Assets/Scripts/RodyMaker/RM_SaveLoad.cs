using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// Static class for saving and loading story data via WorkingStory.
/// All story operations now go through WorkingStory (no folder-based fallbacks).
/// </summary>
public static class RM_SaveLoad {

    /// <summary>
    /// Loads the title sprite (0.png) from WorkingStory.
    /// </summary>
    public static Sprite LoadTitleSprite()
    {
        if (!WorkingStory.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] WorkingStory not loaded");
            return null;
        }
        return WorkingStory.LoadSprite("0.png", 320, 200);
    }

    /// <summary>
    /// Loads a scene thumbnail (first frame) from WorkingStory.
    /// </summary>
    public static Sprite LoadSceneThumbnail(int sceneIndex)
    {
        if (!WorkingStory.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] WorkingStory not loaded");
            return null;
        }
        return WorkingStory.LoadSprite($"{sceneIndex}.1.png", 61, 25);
    }

    #region Conversion Helpers

    /// <summary>
    /// Converts RM_GameManager state to SceneData for saving.
    /// </summary>
    private static SceneData GameManagerToSceneData(RM_GameManager gm)
    {
        return new SceneData
        {
            dialogues = new PhonemeDialogues
            {
                intro1 = gm.introDial1 ?? ".",
                intro2 = gm.introDial2 ?? ".",
                intro3 = gm.introDial3 ?? ".",
                obj = gm.objDial ?? ".",
                ngp = gm.ngpDial ?? ".",
                fsw = gm.fswDial ?? "."
            },
            texts = new DisplayTexts
            {
                title = gm.titleText ?? "glitch title",
                intro1 = gm.introText1 ?? "",
                intro2 = gm.introText2 ?? "",
                intro3 = gm.introText3 ?? "",
                obj = gm.objText ?? ".",
                ngp = gm.ngpText ?? ".",
                fsw = gm.fswText ?? "."
            },
            music = new MusicSettings
            {
                introMusic = gm.musicIntro ?? "i1",
                sceneMusic = gm.musicLoop ?? "l1"
            },
            voice = new VoiceSettings
            {
                pitch1 = gm.pitch1,
                pitch2 = gm.pitch2,
                pitch3 = gm.pitch3,
                isMastico1 = gm.isMastico1,
                isMastico2 = gm.isMastico2,
                isMastico3 = gm.isMastico3,
                isZambla = gm.isZambla
            },
            objects = new ObjectZones
            {
                obj = GameObjectsToObjectZone(gm.obj, gm.objNear),
                ngp = GameObjectsToObjectZone(gm.ngp, gm.ngpNear),
                fsw = GameObjectsToObjectZone(gm.fsw, gm.fswNear)
            }
        };
    }

    /// <summary>
    /// Converts a list of GameObjects to typed ObjectZone floats.
    /// Only uses the first object (index 0) since each zone is now single.
    /// </summary>
    private static ObjectZone GameObjectsToObjectZone(List<GameObject> target, List<GameObject> near)
    {
        var zone = new ObjectZone();

        if (target != null && target.Count > 0)
        {
            var rect = target[0].GetComponent<RectTransform>();
            zone.x = rect.localPosition.x;
            zone.y = rect.localPosition.y;
            zone.width = rect.sizeDelta.x;
            zone.height = rect.sizeDelta.y;
        }

        if (near != null && near.Count > 0)
        {
            var rect = near[0].GetComponent<RectTransform>();
            zone.nearX = rect.localPosition.x;
            zone.nearY = rect.localPosition.y;
            zone.nearWidth = rect.sizeDelta.x;
            zone.nearHeight = rect.sizeDelta.y;
        }

        return zone;
    }

    /// <summary>
    /// Formats intro texts to legacy format: "Dialog1" "Dialog2" "Dialog3"
    /// </summary>
    private static string FormatIntroTexts(string intro1, string intro2, string intro3)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (!string.IsNullOrEmpty(intro1)) parts.Add($"\"{intro1}\"");
        if (!string.IsNullOrEmpty(intro2)) parts.Add($"\"{intro2}\"");
        if (!string.IsNullOrEmpty(intro3)) parts.Add($"\"{intro3}\"");
        return string.Join(" ", parts);
    }

    /// <summary>
    /// Formats ObjectZone position to raw string "(x, y);".
    /// </summary>
    private static string ObjectZoneToPositionString(ObjectZone zone)
    {
        if (zone == null) return "";
        return $"({zone.x}, {zone.y});";
    }

    /// <summary>
    /// Formats ObjectZone size to raw string "(width, height);".
    /// </summary>
    private static string ObjectZoneToSizeString(ObjectZone zone)
    {
        if (zone == null) return "";
        return $"({zone.width}, {zone.height});";
    }

    /// <summary>
    /// Formats ObjectZone near position to raw string "(x, y);".
    /// </summary>
    private static string ObjectZoneToNearPositionString(ObjectZone zone)
    {
        if (zone == null) return "";
        return $"({zone.nearX}, {zone.nearY});";
    }

    /// <summary>
    /// Formats ObjectZone near size to raw string "(width, height);".
    /// </summary>
    private static string ObjectZoneToNearSizeString(ObjectZone zone)
    {
        if (zone == null) return "";
        return $"({zone.nearWidth}, {zone.nearHeight});";
    }

    /// <summary>
    /// Creates a readable copy of a texture (required for EncodeToPNG on WebGL).
    /// </summary>
    private static Texture2D MakeTextureReadable(Texture2D source)
    {
        RenderTexture tmp = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(source, tmp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;

        Texture2D readableTexture = new Texture2D(source.width, source.height);
        readableTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);

        return readableTexture;
    }

    /// <summary>
    /// Converts SceneData back to the string array format used by the game.
    /// </summary>
    private static string[] SceneDataToStringArray(SceneData data)
    {
        string[] arr = new string[26];

        // Dialogues (phonemes)
        arr[0] = data.dialogues?.intro1 ?? "";
        arr[1] = data.dialogues?.intro2 ?? "";
        arr[2] = data.dialogues?.intro3 ?? "";
        arr[3] = data.dialogues?.obj ?? "";
        arr[4] = data.dialogues?.ngp ?? "";
        arr[5] = data.dialogues?.fsw ?? "";

        // Display texts
        arr[6] = data.texts?.title ?? "";
        // Combine intro1/2/3 into legacy format: "Dialog1" "Dialog2" "Dialog3"
        string intro1 = data.texts?.intro1 ?? "";
        string intro2 = data.texts?.intro2 ?? "";
        string intro3 = data.texts?.intro3 ?? "";
        arr[7] = FormatIntroTexts(intro1, intro2, intro3);
        arr[8] = data.texts?.obj ?? "";
        arr[9] = data.texts?.ngp ?? "";
        arr[10] = data.texts?.fsw ?? "";

        // Music
        arr[11] = $"{data.music?.introMusic ?? "i1"},{data.music?.sceneMusic ?? "l1"}";

        // Voice settings
        arr[12] = $"{data.voice?.pitch1 ?? 1f},{data.voice?.pitch2 ?? 1f},{data.voice?.pitch3 ?? 1f}";
        arr[13] = $"{(data.voice?.isMastico1 == true ? "1" : "0")},{(data.voice?.isMastico2 == true ? "1" : "0")},{(data.voice?.isMastico3 == true ? "1" : "0")},{(data.voice?.isZambla == true ? "1" : "0")}";

        // Object zones - format typed floats back to raw strings
        arr[14] = ObjectZoneToPositionString(data.objects?.obj);
        arr[15] = ObjectZoneToSizeString(data.objects?.obj);
        arr[16] = ObjectZoneToNearPositionString(data.objects?.obj);
        arr[17] = ObjectZoneToNearSizeString(data.objects?.obj);
        arr[18] = ObjectZoneToPositionString(data.objects?.ngp);
        arr[19] = ObjectZoneToSizeString(data.objects?.ngp);
        arr[20] = ObjectZoneToNearPositionString(data.objects?.ngp);
        arr[21] = ObjectZoneToNearSizeString(data.objects?.ngp);
        arr[22] = ObjectZoneToPositionString(data.objects?.fsw);
        arr[23] = ObjectZoneToSizeString(data.objects?.fsw);
        arr[24] = ObjectZoneToNearPositionString(data.objects?.fsw);
        arr[25] = ObjectZoneToNearSizeString(data.objects?.fsw);

        return arr;
    }

    #endregion

    #region Save

    /// <summary>
    /// Saves the current scene to WorkingStory (in-memory).
    /// </summary>
    private static void SaveSceneToWorkingStory(RM_GameManager gm, int scene)
    {
        // Scene 0 is just the cover image
        if (scene == 0)
        {
            Texture2D tex = gm.scenePanel.GetComponent<SpriteRenderer>().sprite.texture;
            Texture2D resized = MakeTextureReadable(tex);
            RM_TextureScale.Point(resized, 320, 240);
            WorkingStory.SaveSprite("0.png", resized);
            if (resized != tex) UnityEngine.Object.Destroy(resized);
            Debug.Log("[RM_SaveLoad] WorkingStory: Cover saved");
            return;
        }

        // Convert game state to SceneData and save
        SceneData sceneData = GameManagerToSceneData(gm);
        WorkingStory.SaveScene(scene, sceneData);

        // Save the main scene sprite (frames 1-4, same image)
        Texture2D sceneTex = gm.scenePanel.GetComponent<SpriteRenderer>().sprite.texture;
        Texture2D resizedScene = MakeTextureReadable(sceneTex);
        RM_TextureScale.Point(resizedScene, 320, 130);

        for (int i = 1; i <= 4; i++)
        {
            WorkingStory.SaveSprite($"{scene}.{i}.png", resizedScene);
        }
        if (resizedScene != sceneTex) UnityEngine.Object.Destroy(resizedScene);

        // Save animation frames if any
        int framesCount = RM_ImgAnimLayout.frames.Count;
        for (int j = 0; j < framesCount; j++)
        {
            Sprite frame = RM_ImgAnimLayout.frames[j];
            if (frame != null)
            {
                Texture2D frameTex = MakeTextureReadable(frame.texture);
                RM_TextureScale.Point(frameTex, 320, 130);
                WorkingStory.SaveSprite($"{scene}.{j + 2}.png", frameTex);
                if (frameTex != frame.texture) UnityEngine.Object.Destroy(frameTex);
            }
        }

        Debug.Log($"[RM_SaveLoad] WorkingStory: Scene {scene} saved (dirty={WorkingStory.IsDirty})");
    }

    /// <summary>
    /// Creates a new scene in WorkingStory, using the previous scene as a template.
    /// </summary>
    public static void CreateNewScene(int sceneIndex)
    {
        if (!WorkingStory.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] Cannot create scene - WorkingStory not loaded");
            return;
        }

        WorkingStory.CreateNewScene(sceneIndex);
        Debug.Log($"[RM_SaveLoad] Created scene {sceneIndex}");
    }

    #endregion

    /// <summary>
    /// Saves the current scene to WorkingStory.
    /// </summary>
    public static void SaveGame(RM_GameManager gm)
    {
        if (!WorkingStory.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] Cannot save - WorkingStory not loaded");
            return;
        }

        int scene = gm.currentScene;
        SaveSceneToWorkingStory(gm, scene);
        Debug.Log("Save done!");
    }

    /// <summary>
    /// Loads scene data as a structured SceneData object.
    /// </summary>
    public static SceneData LoadSceneData(int scene)
    {
        if (!WorkingStory.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] WorkingStory not loaded");
            return null;
        }
        return WorkingStory.LoadScene(scene);
    }

    /// <summary>
    /// Loads scene data as a string array (legacy format).
    /// Used by GameManager for runtime.
    /// </summary>
    public static string[] LoadSceneTxt(int scene)
    {
        if (!WorkingStory.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] WorkingStory not loaded");
            return null;
        }
        SceneData sceneData = WorkingStory.LoadScene(scene);
        return SceneDataToStringArray(sceneData);
    }

    /// <summary>
    /// Returns the scene count from WorkingStory.
    /// </summary>
    public static int CountScenesTxt()
    {
        if (!WorkingStory.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] WorkingStory not loaded");
            return 0;
        }
        return WorkingStory.SceneCount;
    }

    /// <summary>
    /// Loads all sprite frames for a scene from WorkingStory.
    /// </summary>
    public static List<Sprite> LoadSceneSprites(int scene)
    {
        if (!WorkingStory.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] WorkingStory not loaded");
            return new List<Sprite>();
        }
        return WorkingStory.LoadSceneSprites(scene);
    }

    /// <summary>
    /// Deletes a scene from WorkingStory.
    /// </summary>
    public static void DeleteScene(int scene)
    {
        if (!WorkingStory.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] Cannot delete - WorkingStory not loaded");
            return;
        }

        Debug.Log($"[RM_SaveLoad] Deleting scene {scene}");
        WorkingStory.DeleteScene(scene);
        Debug.Log($"[RM_SaveLoad] Scene deleted, count now: {WorkingStory.SceneCount}");
    }

    public static void SetActiveZones(List<GameObject> zonesNear, List<GameObject> zones, bool activate = true) {
		foreach (GameObject near in zonesNear)
		{
			near.SetActive(activate);
		}
		foreach (GameObject zone in zones)
		{
			zone.SetActive(activate);
		}
	}

    /// <summary>
    /// Loads story credits from WorkingStory.
    /// </summary>
    public static void LoadCredits(Text title, Text credits)
    {
        if (!WorkingStory.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] WorkingStory not loaded");
            return;
        }

        string creditsText = WorkingStory.GetCredits();
        string[] lines = creditsText.Split('\n');
        title.text = lines.Length > 0 ? lines[0] : "";
        credits.text = lines.Length > 1 ? string.Join("\n", lines, 1, lines.Length - 1) : "";
        Debug.Log("[RM_SaveLoad] Credits loaded");
    }

    /// <summary>
    /// Loads a sprite from a file path on disk (for file picker use).
    /// </summary>
    /// <param name="filePath">Full path to image file</param>
    /// <param name="ignored">Ignored parameter (legacy compatibility)</param>
    /// <param name="width">Target width</param>
    /// <param name="height">Target height</param>
    public static Sprite LoadSprite(string filePath, int ignored, int width, int height)
    {
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogError($"[RM_SaveLoad] File not found: {filePath}");
            return null;
        }

        try
        {
            byte[] bytes = System.IO.File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(width, height);
            tex.LoadImage(bytes);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new UnityEngine.Vector2(0.5f, 0.5f));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RM_SaveLoad] Failed to load sprite: {e.Message}");
            return null;
        }
    }
}
