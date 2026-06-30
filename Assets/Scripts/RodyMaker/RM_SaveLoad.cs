using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// Static class for saving and loading story data via StoryRoot.Session.
/// All story operations now go through the session (no folder-based fallbacks).
/// </summary>
public static class RM_SaveLoad {

    /// <summary>
    /// Loads the title sprite (0.png) from StoryRoot.Session.
    /// </summary>
    public static Sprite LoadTitleSprite()
    {
        if (!StoryRoot.Session.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] no story loaded");
            return null;
        }
        return StoryRoot.Session.LoadSprite("0.png", 320, 200);
    }

    /// <summary>
    /// Loads a scene thumbnail (first frame) from StoryRoot.Session.
    /// </summary>
    public static Sprite LoadSceneThumbnail(int sceneIndex)
    {
        if (!StoryRoot.Session.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] no story loaded");
            return null;
        }
        return StoryRoot.Session.LoadSprite($"{sceneIndex}.1.png", 61, 25);
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

    #endregion

    #region Save

    /// <summary>
    /// Saves the current scene to the session (in-memory).
    /// </summary>
    private static void SaveSceneToSession(RM_GameManager gm, int scene)
    {
        // Scene 0 is just the cover image
        if (scene == 0)
        {
            Texture2D tex = gm.scenePanel.GetComponent<SpriteRenderer>().sprite.texture;
            Texture2D resized = TextureUtils.MakeReadable(tex);
            RM_TextureScale.Point(resized, 320, 240);
            StoryRoot.Session.SaveSprite("0.png", resized);
            if (resized != tex) UnityEngine.Object.Destroy(resized);
            Debug.Log("[RM_SaveLoad] Cover saved");
            return;
        }

        // Convert game state to SceneData and save
        SceneData sceneData = GameManagerToSceneData(gm);
        StoryRoot.Session.SaveScene(scene, sceneData);

        // Save the main scene sprite (frames 1-4, same image)
        Texture2D sceneTex = gm.scenePanel.GetComponent<SpriteRenderer>().sprite.texture;
        Texture2D resizedScene = TextureUtils.MakeReadable(sceneTex);
        RM_TextureScale.Point(resizedScene, 320, 130);

        for (int i = 1; i <= 4; i++)
        {
            StoryRoot.Session.SaveSprite($"{scene}.{i}.png", resizedScene);
        }
        if (resizedScene != sceneTex) UnityEngine.Object.Destroy(resizedScene);

        // Save animation frames if any
        int framesCount = RM_ImgAnimLayout.frames.Count;
        for (int j = 0; j < framesCount; j++)
        {
            Sprite frame = RM_ImgAnimLayout.frames[j];
            if (frame != null)
            {
                Texture2D frameTex = TextureUtils.MakeReadable(frame.texture);
                RM_TextureScale.Point(frameTex, 320, 130);
                StoryRoot.Session.SaveSprite($"{scene}.{j + 2}.png", frameTex);
                if (frameTex != frame.texture) UnityEngine.Object.Destroy(frameTex);
            }
        }

        Debug.Log($"[RM_SaveLoad] Scene {scene} saved (dirty={StoryRoot.Session.IsDirty})");
    }

    /// <summary>
    /// Creates a new scene in the session, using the previous scene as a template.
    /// </summary>
    public static void CreateNewScene(int sceneIndex)
    {
        if (!StoryRoot.Session.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] Cannot create scene - no story loaded");
            return;
        }

        StoryRoot.Session.CreateNewScene(sceneIndex);
        Debug.Log($"[RM_SaveLoad] Created scene {sceneIndex}");
    }

    #endregion

    /// <summary>
    /// Saves the current scene to StoryRoot.Session.
    /// </summary>
    public static void SaveGame(RM_GameManager gm)
    {
        if (!StoryRoot.Session.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] Cannot save - no story loaded");
            return;
        }

        int scene = gm.currentScene;
        SaveSceneToSession(gm, scene);
        Debug.Log("Save done!");
    }

    /// <summary>
    /// Loads scene data as a structured SceneData object.
    /// </summary>
    public static SceneData LoadSceneData(int scene)
    {
        if (!StoryRoot.Session.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] no story loaded");
            return null;
        }
        return StoryRoot.Session.LoadScene(scene);
    }

    /// <summary>
    /// Loads all sprite frames for a scene from the session.
    /// </summary>
    public static List<Sprite> LoadSceneSprites(int scene)
    {
        if (!StoryRoot.Session.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] no story loaded");
            return new List<Sprite>();
        }
        return StoryRoot.Session.LoadSceneSprites(scene);
    }

    /// <summary>
    /// Deletes a scene from StoryRoot.Session.
    /// </summary>
    public static void DeleteScene(int scene)
    {
        if (!StoryRoot.Session.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] Cannot delete - no story loaded");
            return;
        }

        Debug.Log($"[RM_SaveLoad] Deleting scene {scene}");
        StoryRoot.Session.DeleteScene(scene);
        Debug.Log($"[RM_SaveLoad] Scene deleted, count now: {StoryRoot.Session.SceneCount}");
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
    /// Loads story credits from StoryRoot.Session.
    /// </summary>
    public static void LoadCredits(Text title, Text credits)
    {
        if (!StoryRoot.Session.IsLoaded)
        {
            Debug.LogError("[RM_SaveLoad] no story loaded");
            return;
        }

        string creditsText = StoryRoot.Session.GetCredits();
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
