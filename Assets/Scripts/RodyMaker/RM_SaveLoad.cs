using SFB;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;

public static class RM_SaveLoad {

    public static bool CustomFolder = false;

    // Static provider for JSON story loading
    private static JsonStoryProvider jsonProvider;
    private static string jsonProviderPath;

    /// <summary>
    /// Gets or creates a JsonStoryProvider for the current JSON story.
    /// Returns null if the current story is not a JSON file.
    /// </summary>
    private static JsonStoryProvider GetJsonProvider()
    {
        if (!PathManager.IsJsonStory)
            return null;

        // Create new provider if path changed or not initialized
        if (jsonProvider == null || jsonProviderPath != PathManager.GamePath)
        {
            jsonProvider?.ClearSpriteCache();
            jsonProvider = new JsonStoryProvider(PathManager.GamePath);
            jsonProviderPath = PathManager.GamePath;
        }

        return jsonProvider;
    }

    /// <summary>
    /// Clears the cached JSON provider (call when switching stories).
    /// </summary>
    public static void ClearJsonProvider()
    {
        jsonProvider?.ClearSpriteCache();
        jsonProvider = null;
        jsonProviderPath = null;
    }

    /// <summary>
    /// Loads the title sprite (0.png) for JSON stories.
    /// </summary>
    public static Sprite LoadTitleSprite()
    {
        var provider = GetJsonProvider();
        if (provider != null)
        {
            return provider.LoadSprite(null, "0.png", 320, 200);
        }
        return null;
    }

    /// <summary>
    /// Loads a scene thumbnail (first frame) for JSON stories.
    /// </summary>
    public static Sprite LoadSceneThumbnail(int sceneIndex)
    {
        var provider = GetJsonProvider();
        if (provider != null)
        {
            return provider.LoadSprite(null, $"{sceneIndex}.1.png", 61, 25);
        }
        return null;
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
                intro = gm.introText ?? "glitch intro",
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
                obj = new ObjectZone
                {
                    positionRaw = objListToString(gm.obj),
                    sizeRaw = objListToString(gm.obj, true),
                    nearPositionRaw = objListToString(gm.objNear),
                    nearSizeRaw = objListToString(gm.objNear, true)
                },
                ngp = new ObjectZone
                {
                    positionRaw = objListToString(gm.ngp),
                    sizeRaw = objListToString(gm.ngp, true),
                    nearPositionRaw = objListToString(gm.ngpNear),
                    nearSizeRaw = objListToString(gm.ngpNear, true)
                },
                fsw = new ObjectZone
                {
                    positionRaw = objListToString(gm.fsw),
                    sizeRaw = objListToString(gm.fsw, true),
                    nearPositionRaw = objListToString(gm.fswNear),
                    nearSizeRaw = objListToString(gm.fswNear, true)
                }
            }
        };
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
        arr[7] = data.texts?.intro ?? "";
        arr[8] = data.texts?.obj ?? "";
        arr[9] = data.texts?.ngp ?? "";
        arr[10] = data.texts?.fsw ?? "";

        // Music
        arr[11] = $"{data.music?.introMusic ?? "i1"},{data.music?.sceneMusic ?? "l1"}";

        // Voice settings
        arr[12] = $"{data.voice?.pitch1 ?? 1f},{data.voice?.pitch2 ?? 1f},{data.voice?.pitch3 ?? 1f}";
        arr[13] = $"{(data.voice?.isMastico1 == true ? "1" : "0")},{(data.voice?.isMastico2 == true ? "1" : "0")},{(data.voice?.isMastico3 == true ? "1" : "0")},{(data.voice?.isZambla == true ? "1" : "0")}";

        // Object zones
        arr[14] = data.objects?.obj?.positionRaw ?? "";
        arr[15] = data.objects?.obj?.sizeRaw ?? "";
        arr[16] = data.objects?.obj?.nearPositionRaw ?? "";
        arr[17] = data.objects?.obj?.nearSizeRaw ?? "";
        arr[18] = data.objects?.ngp?.positionRaw ?? "";
        arr[19] = data.objects?.ngp?.sizeRaw ?? "";
        arr[20] = data.objects?.ngp?.nearPositionRaw ?? "";
        arr[21] = data.objects?.ngp?.nearSizeRaw ?? "";
        arr[22] = data.objects?.fsw?.positionRaw ?? "";
        arr[23] = data.objects?.fsw?.sizeRaw ?? "";
        arr[24] = data.objects?.fsw?.nearPositionRaw ?? "";
        arr[25] = data.objects?.fsw?.nearSizeRaw ?? "";

        return arr;
    }

    #endregion

    #region JSON Save

    /// <summary>
    /// Saves the current scene to a JSON story file.
    /// </summary>
    private static void SaveGameToJson(RM_GameManager gm, int scene)
    {
        var provider = GetJsonProvider();
        if (provider == null)
        {
            Debug.LogError("[RM_SaveLoad] Cannot save - no JSON provider available");
            return;
        }

        // Scene 0 is just the cover image
        if (scene == 0)
        {
            Texture2D tex = gm.scenePanel.GetComponent<SpriteRenderer>().sprite.texture;
            Texture2D resized = MakeTextureReadable(tex);
            RM_TextureScale.Point(resized, 320, 240);
            provider.SaveSprite("0.png", resized);
            if (resized != tex) UnityEngine.Object.Destroy(resized);
            provider.WriteToFile();
            Debug.Log("[RM_SaveLoad] JSON: Cover saved");
            return;
        }

        // Convert game state to SceneData
        SceneData sceneData = GameManagerToSceneData(gm);
        provider.SaveScene(scene, sceneData);

        // Save the main scene sprite (frames 1-4, same image)
        Texture2D sceneTex = gm.scenePanel.GetComponent<SpriteRenderer>().sprite.texture;
        Texture2D resizedScene = MakeTextureReadable(sceneTex);
        RM_TextureScale.Point(resizedScene, 320, 130);

        for (int i = 1; i <= 4; i++)
        {
            provider.SaveSprite($"{scene}.{i}.png", resizedScene);
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
                provider.SaveSprite($"{scene}.{j + 2}.png", frameTex);
                if (frameTex != frame.texture) UnityEngine.Object.Destroy(frameTex);
            }
        }

        // Update scene count and write to file
        provider.UpdateSceneCount(PlayerPrefs.GetInt("scenesCount"));
        provider.WriteToFile();

        Debug.Log($"[RM_SaveLoad] JSON: Scene {scene} saved");
    }

    /// <summary>
    /// Creates a new scene in the JSON story, using the previous scene as a template.
    /// </summary>
    public static void CreateNewScene(int sceneIndex)
    {
        if (!PathManager.IsJsonStory)
        {
            Debug.Log("[RM_SaveLoad] CreateNewScene: Not a JSON story, skipping");
            return;
        }

        var provider = GetJsonProvider();
        if (provider == null)
        {
            Debug.LogError("[RM_SaveLoad] Cannot create scene - no JSON provider available");
            return;
        }

        provider.CreateNewScene(sceneIndex);

        // Write to disk immediately so the scene persists
        provider.WriteToFile();
    }

    /// <summary>
    /// Deletes a scene from a JSON story file.
    /// </summary>
    private static void DeleteSceneFromJson(int scene)
    {
        var provider = GetJsonProvider();
        if (provider == null)
        {
            Debug.LogError("[RM_SaveLoad] Cannot delete - no JSON provider available");
            return;
        }

        var cachedStory = provider.GetCachedStory();
        if (cachedStory?.scenes == null)
        {
            Debug.LogError("[RM_SaveLoad] Cannot delete - no scenes in story");
            return;
        }

        // Remove the scene data
        cachedStory.scenes.RemoveAll(s => s.index == scene);

        // Remove associated sprites (scene.1.png, scene.2.png, etc.)
        if (cachedStory.sprites != null)
        {
            var spritesToRemove = new List<string>();
            foreach (var key in cachedStory.sprites.Keys)
            {
                if (key.StartsWith($"{scene}.") && key.EndsWith(".png"))
                {
                    spritesToRemove.Add(key);
                }
            }
            foreach (var key in spritesToRemove)
            {
                cachedStory.sprites.Remove(key);
            }
            Debug.Log($"[RM_SaveLoad] JSON: Removed {spritesToRemove.Count} sprites for scene {scene}");
        }

        // Reindex subsequent scenes
        foreach (var s in cachedStory.scenes)
        {
            if (s.index > scene)
            {
                // Also rename sprites for this scene
                if (cachedStory.sprites != null)
                {
                    int oldIndex = s.index;
                    int newIndex = oldIndex - 1;
                    var spritesToRename = new List<string>();
                    foreach (var key in cachedStory.sprites.Keys)
                    {
                        if (key.StartsWith($"{oldIndex}.") && key.EndsWith(".png"))
                        {
                            spritesToRename.Add(key);
                        }
                    }
                    foreach (var oldKey in spritesToRename)
                    {
                        string newKey = oldKey.Replace($"{oldIndex}.", $"{newIndex}.");
                        cachedStory.sprites[newKey] = cachedStory.sprites[oldKey];
                        cachedStory.sprites.Remove(oldKey);
                    }
                }
                s.index--;
            }
        }

        // Update scene count
        int newCount = PlayerPrefs.GetInt("scenesCount") - 1;
        PlayerPrefs.SetInt("scenesCount", newCount);
        provider.UpdateSceneCount(newCount);

        // Write to file
        provider.WriteToFile();
        Debug.Log($"[RM_SaveLoad] JSON: Scene {scene} deleted, scenes count now: {newCount}");
    }

    #endregion

	public static void SaveGame (RM_GameManager gm)
    {
        int scene = gm.currentScene;

        // increment the counter only if scene is saved, avoid create a second new scene without saving the first one
        if (PlayerPrefs.GetInt("scenesCount") + 1 == scene) {
			Debug.Log("Adding a new scene to the counter...");
			PlayerPrefs.SetInt("scenesCount", scene);
        }

        // JSON story save path
        if (PathManager.IsJsonStory)
        {
            SaveGameToJson(gm, scene);
            return;
        }

        // Legacy folder-based save
        string path = PathManager.GamePath + Path.DirectorySeparatorChar;
        string newPath = path;
        
        /***
        // chose the destination of the game folder in which the scene will be saved
        // removed with rody collection to be more convenient for users
        if (CustomFolder == false) {
            string[] newPaths = StandaloneFileBrowser.OpenFolderPanel("Dossier de ton jeu...", Application.dataPath + "/StreamingAssets", false);
            if (newPaths.Length == 0){
                Debug.Log("save canceled");
                return;
            }
            else {
                newPath = newPaths[0] + "\\";
                CustomFolder = true;
                PlayerPrefs.SetString("gamePath", newPath);
                Debug.Log("SaveGame : new gamePath : " + newPath);
            }
        }
        else {
            newPath = path;
        }
        ***/
        
        newPath = path;

        if (scene == 0) {
            SaveSprite(gm.scenePanel.GetComponent<SpriteRenderer>().sprite.texture, newPath, "0", 320, 240);
            return; // save only the image
        }
        
        
        for (int i = 1; i<5; i++){
            SaveSprite(gm.scenePanel.GetComponent<SpriteRenderer>().sprite.texture, newPath, scene+"."+i);
        }
        
        // save the animation frames if there are set
        int framesCount = RM_ImgAnimLayout.frames.Count;
        if (framesCount > 0) 
            for (int j = 0; j < framesCount; j++)
            {
                Sprite frame = RM_ImgAnimLayout.frames[j];
                if (frame != null)
                    SaveSprite(frame.texture, newPath, scene+"."+(j+2));
            }

        string oldTxtPath = PathManager.LevelsFile;
        string newTxtPath = newPath + "levels.rody";
        
        // cancel chosing file 
        if (newPath == null) 
            return;
        
        string[] lines = File.ReadAllLines(oldTxtPath);
        
        // create the file if it is a new file
        if (!File.Exists(newTxtPath)) {
            File.Copy(oldTxtPath, newTxtPath);
            Debug.Log("creation of " + newTxtPath);
        }
        
        try
        {   // Open the text file using a stream writer.
            using (TextWriter sw = new StreamWriter(newTxtPath))
            {
                WriteToTxt(sw, scene, lines, gm);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The file could not be write:");
            Console.WriteLine(e.Message);
        }
        Debug.Log("Save done !");
    }

	private static void SaveSprite(Texture2D tex, string path, string scene, int width = 320, int height = 130) {
        RM_TextureScale.Point(tex, width, height);
        var bytes = tex.EncodeToPNG();
        string spritePath = Path.Combine(path, "Sprites") + Path.DirectorySeparatorChar;
        if (!Directory.Exists(spritePath))
            Directory.CreateDirectory(spritePath);
    
        FileStream png = new FileStream(spritePath + scene + ".png", FileMode.Create);
    
        var binary = new BinaryWriter(png);
        binary.Write(bytes);
        png.Close();
    }

    private static void WriteToTxt(TextWriter sw, int scene, string[] lines, RM_GameManager gm){
        int currentScene=0;
        int currentLine=0;

        // preceding scene rewriting
        for (int i = 1; i<scene; i++) {
            for (int j = currentLine; lines[j] != "~"; j++) {
                sw.WriteLine(lines[j]);
                currentLine++;
            }
            sw.WriteLine("~");
            currentLine++;
            currentScene++;
        }

        // current scene writting
        sw.WriteLine("#######################\n###### scene "+scene+" ########\n#######################");
        sw.WriteLine("## phonems\n" + gm.introDial1 + "\n" + gm.introDial2 + "\n" + gm.introDial3 + "\n" + gm.objDial + "\n" + gm.ngpDial + "\n" + gm.fswDial);
        sw.WriteLine("## texts [string]\n" + gm.titleText + "\n" + gm.introText + "\n" + gm.objText + "\n" + gm.ngpText + "\n" + gm.fswText);
        sw.WriteLine("## musics [i1..i3 | l1..l15]\n" + gm.musicIntro + "," + gm.musicLoop);
        sw.WriteLine("## pitch [float]\n" + gm.pitch1 + "," + gm.pitch2 + "," + gm.pitch3);
        sw.WriteLine("## is mastico speaking [booleans] ?\n" + BoolToString(gm.isMastico1) + "," + BoolToString(gm.isMastico2) + "," + BoolToString(gm.isMastico3) + "," + BoolToString(gm.isZambla));
        sw.WriteLine("## obj position\n"     + objListToString(gm.obj));
        sw.WriteLine("## obj size\n"         + objListToString(gm.obj, true));
        sw.WriteLine("## objNear position\n" + objListToString(gm.objNear));
        sw.WriteLine("## objNear size\n"     + objListToString(gm.objNear, true));
        sw.WriteLine("## NGP position\n"     + objListToString(gm.ngp));
        sw.WriteLine("## NGP size\n"         + objListToString(gm.ngp, true));
        sw.WriteLine("## NgpNear position\n" + objListToString(gm.ngpNear));
        sw.WriteLine("## NgpNear size\n"     + objListToString(gm.ngpNear, true));
        sw.WriteLine("## FSW position\n"     + objListToString(gm.fsw));
        sw.WriteLine("## FSW size\n"         + objListToString(gm.fsw, true));
        sw.WriteLine("## fswNear position\n" + objListToString(gm.fswNear));
        sw.WriteLine("## fswNear size\n"     + objListToString(gm.fswNear, true));
        sw.WriteLine("~");
        // increment the scene counter
        currentScene++; 
        currentLine+=47; // one scene is 47 lines
        
        for (int i = currentScene; i < PlayerPrefs.GetInt("scenesCount") + 1; i++) { //TODO verify this condition
            // following scene rewriting
            for (int j=currentLine; lines[j] != "~"; j++) {
                    sw.WriteLine(lines[j]);
                currentLine++;
            }
            sw.WriteLine("~");
            currentLine++;
            currentScene++;
        }
        sw.Close();
    }

    private static string objListToString(List<GameObject> objList, bool size = false) {
        string objStr = "";
        foreach (GameObject obj in objList)
        {
            if (size)
                objStr += (Vector2) obj.GetComponent<RectTransform>().sizeDelta + ";";
            else 
                objStr += (Vector2) obj.GetComponent<RectTransform>().localPosition + ";";
        }
        Debug.Log(objStr);
        return objStr;
    }
    private static string BoolToString(bool b) {
        return (b)?"1":"0";
    }

    /// <summary>
    /// Loads scene data as a structured SceneData object.
    /// Preferred over LoadSceneTxt for new code.
    /// </summary>
    public static SceneData LoadSceneData(int scene)
    {
        string[] raw = LoadSceneTxt(scene);
        return SceneDataParser.Parse(raw);
    }

    public static string[] LoadSceneTxt(int scene)
    {
        // Check if this is a JSON story
        var provider = GetJsonProvider();
        if (provider != null)
        {
            return LoadSceneTxtFromJson(provider, scene);
        }

        // Legacy folder-based loading
        // Debug.Log("LoadSceneTxt gamePath : "+PathManager.GamePath);
        string[] sceneStr = new string[26];
        try
        {   // Open the text file using a stream reader.
            using (StreamReader sr = new StreamReader(PathManager.LevelsFile))
            {
                string line;

                for (int j=1; j<scene; j++) {
                   while ((line = sr.ReadLine()) != "~") ; // jump to the selected scene in the txt
                }

                int i = 0;
                while ((line = sr.ReadLine()) != "~") {
                    if (line == "") {
                        sceneStr[i] = "";
                        i++;
                    }
                    else
                        if (line[0] == '#') // does not count as a line
                            continue;
                        else
                        {
                            if (line[0] == '.') // empty but count as a line
                                sceneStr[i] = "";
                            else
                                sceneStr[i] = readLine(line);
                            //Debug.Log("line "+i+ " : " + sceneStr[i]);
                            i++;
                        }
                }
                sr.Close();
            }

			return sceneStr;
        }
        catch (Exception e)
        {
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }
		return null;
    }

    /// <summary>
    /// Loads scene text data from a JSON story provider.
    /// </summary>
    private static string[] LoadSceneTxtFromJson(JsonStoryProvider provider, int scene)
    {
        SceneData sceneData = provider.LoadScene(null, scene);
        return SceneDataToStringArray(sceneData);
    }

    public static int CountScenesTxt() {
        // Check if this is a JSON story
        var provider = GetJsonProvider();
        if (provider != null)
        {
            return provider.GetSceneCount(null);
        }

        // Legacy folder-based counting
        int count = 0;
        try
        {
            using (StreamReader sr = new StreamReader(PathManager.LevelsFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                    if (line == "~")
                        count++;
                sr.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }
		return count;
    }

    private static string readLine(string line) 
    {
        string newLine = (line[0] == '\\')? "" : line[0].ToString();
        for (int i=1; i<line.Length; ++i)
        {
            if (line[i] == 'n' && line[i-1] == '\\')
                newLine = String.Concat(newLine, "\n");
            else if (line[i] != '\\')
                newLine = String.Concat(newLine, line[i]);
        }
        return newLine;
    }

    public static List<Sprite> LoadSceneSprites(int scene)
    {
        // Check if this is a JSON story
        var provider = GetJsonProvider();
        if (provider != null)
        {
            return provider.LoadSceneSprites(null, scene);
        }

        // Legacy folder-based loading
        List<Sprite> sceneSprites = new List<Sprite>();

        string spritesPath = PathManager.GetSceneSpritesBasePath(scene);

        Debug.Log("(LoadSceneSprites) path : " + spritesPath);

        // search the number of frames
		DirectoryInfo dir = new DirectoryInfo(PathManager.SpritesPath);
        var files = dir.GetFiles(scene + ".*.png");

        for (int i = 1; i < files.Length + 1; i++) // frames index start at 1 not 0
        {
            sceneSprites.Add(LoadSprite(spritesPath, i, 320, 130));
        }

        return sceneSprites;
    }

    public static Sprite LoadSprite(string spritePath, int index, int width, int height) {
        Texture2D tex = null;
        byte[] fileData;
        // index == 0 means import a new sprite else load an existing one
        string file = index == 0 ? spritePath : spritePath + "." + index.ToString() + ".png";

        //Debug.Log("[LoadSprite] spritePath : " +  spritePath + ", index : " + index + ", file : " + file);

        if (!File.Exists(file)) // sprite missing or adding a new scene
        {
            if (index > 1 && index < 5) {
                string baseFrame = spritePath + ".1.png";
                // if frame is one of the 3 first & not exist but base do, copy base and load it
                if (File.Exists(baseFrame))
                    File.Copy(baseFrame, file);
                // else load the default sprite
                else {
                    Debug.Log("(LoadSprite) file does not exist : " + file + ", loading base sprite instead");
                    File.Copy(Path.Combine(PlayerPrefs.GetString("gamePath"), "Sprites", "base.png"), file);
                }
            }
            else {
                File.Copy(Path.Combine(PlayerPrefs.GetString("gamePath"), "Sprites", "base.png"), file);
            }
        }

        //Debug.Log("(LoadSprite) load : " + file);
        fileData = File.ReadAllBytes(file);

        tex = new Texture2D(2, 2);
        tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        tex.filterMode = FilterMode.Point;
        RM_TextureScale.Point(tex,width,height); // resize the texture dimensions to 320*130

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
        return sprite;
    }

    public static List<GameObject> ReadObjects(string name, string posStr, string sizeStr, bool isNear = false) {
        
        if (posStr[posStr.Length - 1] == ';')
            posStr = posStr.Substring(0, posStr.Length - 1);

        if (sizeStr[sizeStr.Length - 1] == ';')
            sizeStr = sizeStr.Substring(0, sizeStr.Length - 1);
        
        string[] posList  =  posStr.Split(';');
        string[] sizeList = sizeStr.Split(';');

        int zonesCount = posList.Length;
        // Debug.Log("(readObjects) posList length is : " + zonesCount);
        
        if (posList.Length != sizeList.Length) {
            // Debug.Log("(readObjects) posList not same length as sizeList !");
            return null;
        }

        // if clone is an object zone parent should be it nearzone or if it is a Near object zone, parent is Objects

        List<GameObject> objects = new List<GameObject>();

        for (int i=0; i < zonesCount; ++i) {
            
            GameObject parent, obj;
            if (isNear) {
                parent = GameObject.Find("Objects");
                obj = GameObject.Instantiate(GameObject.Find("ObjNear"), parent.GetComponent<RectTransform>());
            }
            else {
                parent = GameObject.Find(name + "Near" + i);
                obj = GameObject.Instantiate(GameObject.Find("Obj"), parent.GetComponent<RectTransform>());
            }
            
            obj.name = name+i;
             
            // Debug.Log("(readObjects) obj name : " + obj.name);
            // Debug.Log("(readObjects) parent name : " + parent.name);

            objects.Add(LoadObject(obj, posList[i], sizeList[i]));
        }
        return objects;
    }
    public static GameObject LoadObject(GameObject obj, string pos, string size) {
		
		//Debug.Log("set position and size for : " + name);
		
        
		RectTransform rectTransform = obj.GetComponent<RectTransform>();

		//Debug.Log("## " + name + " position\n"+rectTransform.localPosition.x+","+rectTransform.localPosition.y);
		//Debug.Log("## " + name + " size\n"+rectTransform.sizeDelta.x+","+rectTransform.sizeDelta.y);

		string[] positions = pos.Split(',');
		string[] sizes     = size.Split(',');
		
		rectTransform.localPosition = new Vector3(
			float.Parse(positions[0].Replace("(","")),
			float.Parse(positions[1].Replace(")","")),0f);

		rectTransform.sizeDelta = new Vector3(
			float.Parse(sizes[0].Replace("(","")),
			float.Parse(sizes[1].Replace(")","")),0f);

		// Debug.Log(name + "pos : " + rectTransform.localPosition + "\n" 
		// 		+ name + "size : " + rectTransform.sizeDelta);
        return obj;
    }

    public static void DeleteScene(int scene) {
        Debug.Log("(DeleteScene) Erase level " + scene + ", scenes count : " + PlayerPrefs.GetInt("scenesCount"));

        // JSON story delete
        if (PathManager.IsJsonStory)
        {
            DeleteSceneFromJson(scene);
            return;
        }

        // Legacy folder-based delete
        // Debug.Log("(DeleteScene) Deleting scene " + scene + " sprites ...");
        // for (int i=0; i < 5; ++i){
        //     File.Delete(path + "Sprites\\" + scene + "." + i + ".png" );
        // }

        Debug.Log("(DeleteScene) Deleting text ...");
        int currentScene=0;
        int currentLine=0;
        string[] lines = File.ReadAllLines(PathManager.LevelsFile);

        try
        {   // Open the text file using a stream writer.
            using (TextWriter sw = new StreamWriter(PathManager.LevelsFile))
            {
                for (int i = 1; i<scene; i++) {
                    for (int j = currentLine; lines[j] != "~"; j++) {
                        sw.WriteLine(lines[j]);
                        currentLine++;
                    }
                    sw.WriteLine("~");
                    currentLine++;
                    currentScene++;
                }
                // skip current scene
                currentScene++;
                currentLine+=47; // move the lines offset to the next scene
                for (int i = currentScene; i < PlayerPrefs.GetInt("scenesCount") + 1; i++) {
                    // following scene rewriting
                    for (int j=currentLine; lines[j] != "~"; j++) {
                            sw.WriteLine(lines[j]);
                        currentLine++;
                    }
                    sw.WriteLine("~");
                    currentLine++;
                    currentScene++;
                }
                sw.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The file could not be write:");
            Console.WriteLine(e.Message);
        }
        PlayerPrefs.SetInt("scenesCount", PlayerPrefs.GetInt("scenesCount") - 1);
        Debug.Log("Erase level done !, scenes count now : " + PlayerPrefs.GetInt("scenesCount"));
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

    public static void LoadCredits(Text title, Text credits)
    {
        // Check if this is a JSON story
        var provider = GetJsonProvider();
        if (provider != null)
        {
            string creditsText = provider.LoadCredits(null);
            string[] lines = creditsText.Split('\n');
            title.text = lines.Length > 0 ? lines[0] : "";
            credits.text = lines.Length > 1 ? string.Join("\n", lines, 1, lines.Length - 1) : "";
            Debug.Log("Read Credits from JSON: Done!");
            return;
        }

        // Legacy folder-based loading
        string path = PathManager.CreditsFile;
        Debug.Log("Read Credits at " + path);
        // Debug.Log("LoadSceneTxt gamePath : "+path);
        try
        {   // Open the text file using a stream reader.
            using (StreamReader sr = new StreamReader(path))
            {
                title.text = sr.ReadLine();

                string creditsStr = "";
                string line;
                while ((line = sr.ReadLine()) != null) {
                    creditsStr += line + "\n";
                }
                sr.Close();
                credits.text = creditsStr;
                Debug.Log("Read Credits : Done!");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }
    }
}
