using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using SFB;

public class RA_NewGame : MonoBehaviour {

	public InputField titleInput;
	public InputField imgInput;
	public GameObject errorPanel;
	public GameObject feedbackPanel, newGamePanel, buttonYeap, buttonNop;

	public Button buttonAccept, buttonCancel;
	public Text yeapTxt;
	public RA_ScrollView sv;
	string basePath;
	Sprite coverImgSprite;
	Text feedbackTxt;
	// Use this for initialization
	void Start () {
		basePath = Path.Combine(Application.streamingAssetsPath, "Rody0");
		Debug.Log("basePath : " + basePath);
		titleInput.characterValidation = InputField.CharacterValidation.Name;
		feedbackTxt = feedbackPanel.GetComponentInChildren<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		buttonAccept.interactable = titleInput.text.Length == 0  ? false : true;
	}

	public void NG_OnAcceptClick() {
#if UNITY_WEBGL && !UNITY_EDITOR
		// File system operations not available on WebGL
		feedbackTxt.text = "Cette fonctionnalité n'est pas disponible dans la version web, Rody!";
		buttonNop.SetActive(true);
		newGamePanel.SetActive(false);
		feedbackPanel.SetActive(true);
#else
		// check if the name is OK (characteres)
		string path = Path.Combine(Application.streamingAssetsPath, titleInput.text);
		Debug.Log("[RA] Path of the new game : " + path);
		try
		{
			if (Directory.Exists(path)) {
				// mastico apparait et dit nop
				Debug.Log("[RA] Folder already exist");
				feedbackTxt.text = "Un dossier de jeu porte déjâ ce nom, Rody!";
				buttonNop.SetActive(true);
				newGamePanel.SetActive(false);
				feedbackPanel.SetActive(true);
				return;
			}

			// create folder with this name
			Directory.CreateDirectory(path);
			CopyRodyBaseFolder(path);
			Debug.Log("[RA] New game folder created");
			// if there is a custom cover image specified
			if (imgInput.text.Length > 0) {
				CopyCoverImage(path);
			}
		}
		catch (System.Exception e)
		{
			Debug.Log(e.ToString());
			// mastico apparait et dit nop
			feedbackTxt.text = "Le dossier du jeu n'a pas été créé, Rody! \nQuelque chose n'allait pas...\n("+e.ToString()+")";
			buttonNop.SetActive(true);
		}

		// Load the base game into the Rody Maker menu
		Debug.Log("[RA] Set game path : " + path); // load rody 0
		PlayerPrefs.SetString("gamePath", path);
		// folder creation succeded
		feedbackTxt.text = "Le dossier du jeu a été créé, Rody! \nRendez-vous dans Rody Maker pour écrire son histoire !";
		yeapTxt.text = "ok";
		buttonYeap.SetActive(true);
		buttonYeap.GetComponent<Button>().onClick.RemoveAllListeners();
		buttonYeap.GetComponent<Button>().onClick.AddListener(delegate {UnsetfeedbackPanel(2);});
		newGamePanel.SetActive(false);
		feedbackPanel.SetActive(true);
#endif
	}

	public void NG_OnCancelClick() {
		newGamePanel.SetActive(false);
	}

	public void NG_ImgClick()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("[RA_NewGame] File browser not available on WebGL");
        return;
#else
        Debug.Log("Import Img clicked");
        var extensions = new[] {new ExtensionFilter("Images", "png", "jpg", "jpeg" ),};
        string imgPath = null;
        string[] files = StandaloneFileBrowser.OpenFilePanel("Choix de l'image de couverture", "", extensions, false);
        if (files.Length != 0)
            imgPath = files[0];
        else
            return;

		imgInput.text = imgPath;

		// load the sprite
		coverImgSprite = RM_SaveLoad.LoadSprite(imgPath,0,340,480);
#endif
    }

	// copy a game folder
	// should be verified before the copy
	// Safe copy : only the game file (protection against import custom folder)
	void CopyGameFolder(string oldGamePath, string newGamePath) {
		//Create the Sprites sub-directory
		string spritesPath = newGamePath+"\\Sprites\\";
		Directory.CreateDirectory(spritesPath);

		//Copy all the sprites
		foreach (string sprite in Directory.GetFiles(oldGamePath+"\\Sprites\\", "*.png", SearchOption.TopDirectoryOnly))
			File.Copy(sprite, sprite.Replace(oldGamePath+"\\Sprites\\", spritesPath), true);

		try {
			File.Copy(oldGamePath + "/Sprites/cover.png", newGamePath + "/Sprites/cover.png");
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
			// copy cover from base path
			Debug.Log("load cover : " + basePath + "/Sprites/cover.png");
			File.Copy(basePath + "/Sprites/cover.png", newGamePath + "/Sprites/cover.png");
		}

		File.Copy(oldGamePath + "/credits.txt", newGamePath + "/credits.txt");
		File.Copy(oldGamePath + "/levels.rody", newGamePath + "/levels.rody");
	}
	
	void CopyRodyBaseFolder(string newGamePath) {
		CopyGameFolder(basePath, newGamePath);
	}

	void CopyCoverImage(string newGamePath){
		// if there is a path, it should be a valid file since the field is read only
		Texture2D tex = coverImgSprite.texture;
		RM_TextureScale.Point(tex, 340, 480);
		// encode the sprite into a new png (in case img was in another format)
		var bytes = tex.EncodeToPNG();
		// create the file in Sprites folder
		FileStream png = new FileStream(newGamePath + "/Sprites/cover.png", FileMode.Create);
		var binary = new BinaryWriter(png);
		binary.Write(bytes);
		png.Close();
	}

	/// <summary>
	/// Imports a .rody.json story file by copying it directly to UserStories.
	/// </summary>
	public void OnImportClick()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		feedbackTxt.text = "L'import de fichiers n'est pas disponible dans la version web, Rody!";
		buttonNop.SetActive(true);
		feedbackPanel.SetActive(true);
#else
		var extensions = new[] { new ExtensionFilter("Rody Story", "rody.json", "json") };
		string[] files = StandaloneFileBrowser.OpenFilePanel("Importer une histoire", "", extensions, false);

		if (files.Length == 0)
		{
			feedbackTxt.text = "Aucun fichier n'a été sélectionné!";
			buttonNop.SetActive(true);
			feedbackPanel.SetActive(true);
			return;
		}

		string filePath = files[0];
		Debug.Log($"[RA_NewGame] Importing story from: {filePath}");

		// Validate before importing
		string json = File.ReadAllText(filePath);
		var (isValid, title, sceneCount, error) = StoryImporter.ValidateJson(json);

		if (!isValid)
		{
			feedbackTxt.text = $"Le fichier n'est pas valide!\n{error}";
			buttonNop.SetActive(true);
			feedbackPanel.SetActive(true);
			return;
		}

		// Simply copy the JSON file to UserStories directory
		string destPath = PathManager.GetUniqueJsonStoryPath(title);

		try
		{
			File.Copy(filePath, destPath, overwrite: false);
			Debug.Log($"[RA_NewGame] Copied JSON to: {destPath}");
		}
		catch (System.Exception e)
		{
			Debug.LogError($"[RA_NewGame] Failed to copy JSON: {e.Message}");
			feedbackTxt.text = $"L'import a échoué!\n{e.Message}";
			buttonNop.SetActive(true);
			feedbackPanel.SetActive(true);
			return;
		}

		// Success
		feedbackTxt.text = $"L'histoire \"{title}\" a été importée!\n({sceneCount} scènes)";
		yeapTxt.text = "ok";
		buttonYeap.SetActive(true);
		buttonYeap.GetComponent<Button>().onClick.RemoveAllListeners();
		buttonYeap.GetComponent<Button>().onClick.AddListener(delegate { UnsetfeedbackPanel(1); }); // Reload menu
		feedbackPanel.SetActive(true);

		Debug.Log($"[RA_NewGame] Story imported to: {destPath}");
#endif
	}

	/// <summary>
	/// Exports the currently selected user story to a .rody.json file.
	/// </summary>
	public void OnExportClick()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		feedbackTxt.text = "L'export n'est pas disponible dans la version web, Rody!";
		buttonNop.SetActive(true);
		feedbackPanel.SetActive(true);
#else
		// Get the selected user story path
		string storyPath = sv.GetSelectedUserStoryPath();

		if (string.IsNullOrEmpty(storyPath))
		{
			feedbackTxt.text = "Sélectionne une de tes histoires pour l'exporter!";
			buttonNop.SetActive(true);
			feedbackPanel.SetActive(true);
			return;
		}

		// Get suggested filename
		string suggestedName = StoryExporter.GetExportFileName(storyPath);

		// Open save dialog
		string savePath = StandaloneFileBrowser.SaveFilePanel("Exporter l'histoire", "", suggestedName, "rody.json");

		if (string.IsNullOrEmpty(savePath))
		{
			// User cancelled
			return;
		}

		// Export the story
		bool success = StoryExporter.ExportToFile(storyPath, savePath);

		if (success)
		{
			feedbackTxt.text = $"L'histoire a été exportée!\n{Path.GetFileName(savePath)}";
			yeapTxt.text = "ok";
			buttonYeap.SetActive(true);
			buttonYeap.GetComponent<Button>().onClick.RemoveAllListeners();
			buttonYeap.GetComponent<Button>().onClick.AddListener(delegate { UnsetfeedbackPanel(0); });
		}
		else
		{
			feedbackTxt.text = "L'export a échoué!";
			buttonNop.SetActive(true);
		}
		feedbackPanel.SetActive(true);
#endif
	}

	public void SG_onDelete(bool isDeletable) {
		if (isDeletable) {
			feedbackTxt.text = "Es-tu sur de vouloir définivement supprimer ce jeu ?";
			yeapTxt.text = "oui";
			buttonYeap.GetComponent<Button>().onClick.AddListener(delegate{UnsetfeedbackPanel(3);});
			buttonYeap.SetActive(true);
		}
		else {
			feedbackTxt.text = "Tu ne peux pas supprimer ce jeu !";
		}
		buttonNop.SetActive(true);
		feedbackPanel.SetActive(true);
	}

	public void OnEscape(){
		yeapTxt.text = "oui";
		buttonYeap.SetActive(true);
		feedbackTxt.text = "Veux-tu quitter le jeu ?";
		buttonNop.SetActive(true);
		feedbackPanel.SetActive(true);
		buttonYeap.GetComponent<Button>().onClick.AddListener(delegate{UnsetfeedbackPanel(4);}); 
	}

	public void UnsetErrorPanel() {
		errorPanel.SetActive(false);
	}
	public void UnsetfeedbackPanel(int state) {
		// state is 
		// 0 = button Nop : if import or create game failed or cancel deletion of game
		// 1 if import game succed
		// 2 if create game succed
		// 3 if delete game
		// 4 if quit the game
		feedbackPanel.SetActive(false);
		buttonYeap.SetActive(false);
		buttonNop.SetActive(false);
		switch (state)
		{
			case 1:
				sv.Reset(); // reload the menu
				break;
			case 2: 
				SceneManager.LoadScene(6); // load Rody Maker menu
				break;
			case 3:
				try
				{
					string gamePath = PlayerPrefs.GetString("gameToDelete");
					string deleteType = PlayerPrefs.GetString("gameToDeleteType", "folder");
					Debug.Log($"[RA_NewGame] Deleting ({deleteType}): {gamePath}");

					if (!string.IsNullOrEmpty(gamePath))
					{
						if (deleteType == "json")
						{
							// JSON file deletion
							if (File.Exists(gamePath))
							{
								File.Delete(gamePath);
								Debug.Log($"[RA_NewGame] Deleted JSON file: {gamePath}");
							}
						}
						else if (Path.IsPathRooted(gamePath) && Directory.Exists(gamePath))
						{
							// User story folder - gamePath is already the full path
							Directory.Delete(gamePath, true);
						}
						else
						{
							// Legacy - relative path in StreamingAssets
							Directory.Delete(Path.Combine(Application.streamingAssetsPath, gamePath), true);
						}
					}
					PlayerPrefs.SetString("gameToDelete", "");
					PlayerPrefs.SetString("gameToDeleteType", "");
					sv.Reset(); // reload the menu
				}
				catch (System.Exception e)
				{
					Debug.Log(e);
					feedbackTxt.text = "Impossible de supprimer ! \n" + e.ToString() ;
					buttonNop.SetActive(true);
					feedbackPanel.SetActive(true);
				}
				break;
			case 4:
				Application.Quit();
				break;
			default: break;
		}
		// reset button yeap to default behaviour
		buttonYeap.GetComponent<Button>().onClick.AddListener(delegate{UnsetfeedbackPanel(1);}); 
	}
}
