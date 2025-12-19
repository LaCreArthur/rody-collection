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
			File.Copy(oldGamePath + "/cover.png", newGamePath + "/cover.png");
		}
		catch (System.Exception e)
		{ 
			Debug.Log(e);
			// copy cover from base path
			Debug.Log("load cover : " + basePath + "/cover.png");
			File.Copy(basePath + "/cover.png", newGamePath + "/cover.png");
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
		// create the file
		FileStream png = new FileStream(newGamePath + "/cover.png", FileMode.Create);
		var binary = new BinaryWriter(png);
		binary.Write(bytes);
		png.Close();
	}

	public void IG_OnUploadClick(){
#if UNITY_WEBGL && !UNITY_EDITOR
		// File system operations not available on WebGL
		feedbackTxt.text = "L'import de dossiers n'est pas disponible dans la version web, Rody!";
		buttonNop.SetActive(true);
		feedbackPanel.SetActive(true);
#else
		// choix du dossier
		string[] folderPath = StandaloneFileBrowser.OpenFolderPanel("Dossier de jeu a importer", Application.dataPath, false);

		feedbackTxt.text = "";

		if (folderPath.Length == 0){
			buttonNop.SetActive(true);
            feedbackTxt.text += "Aucun dossier n'a été sélectionné!";
        }
		else {
			int errorCount = 0;
			string[] errorsTxt = new string[3];

			if(!File.Exists(folderPath[0] + "/credits.txt")){
				errorsTxt[errorCount] = "le fichier 'credits.txt'";
				errorCount++;
			}
			if(!File.Exists(folderPath[0] + "/levels.rody")){
				errorsTxt[errorCount] = "le fichier 'levels.rody'";
				errorCount++;
			}
			if(!Directory.Exists(folderPath[0] + "/Sprites")){
				errorsTxt[errorCount] = "le sous-dossier 'Sprites'";
				errorCount++;
			}

			// if the folder is valid
			if(errorCount == 0) {
				// get the name of the game
				string gameName = Path.GetFileName(folderPath[0]);
				// copy the folder
				CopyGameFolder(folderPath[0], Path.Combine(Application.streamingAssetsPath, gameName));
				// set the Ok button
				yeapTxt.text = "ok";
				buttonYeap.SetActive(true);
				feedbackTxt.text += "Le dossier "+gameName+" a bien été importé !\n";
				if(!File.Exists(folderPath[0] + "/cover.png")){
					feedbackTxt.text += "l'image de jacquette (cover.png) est manquante !";

				}
			}
			else {
				buttonNop.SetActive(true);
				feedbackTxt.text += "Le dossier ne peut pas être importé !\n";
				if (errorCount == 1)
					feedbackTxt.text += errorsTxt[0] + " est introuvable !";
				else {
					for (int i = 0; i < (errorCount-2); i++) {
						feedbackTxt.text += errorsTxt[i] + ", ";
					}
					feedbackTxt.text += errorsTxt[errorCount-2] + " et " +  errorsTxt[errorCount-1] + " sont introuvables !";
				}
			}
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
					Debug.Log(gamePath);
					if (gamePath.Length > 0)
						Directory.Delete(System.IO.Path.Combine(Application.streamingAssetsPath,PlayerPrefs.GetString("gameToDelete")), true);	
					PlayerPrefs.SetString("gameToDelete", "");
					sv.Reset(); // reload the menu
				}
				catch (System.Exception e)
				{
					Debug.Log(e);
					feedbackTxt.text = "Impossible de supprimer le dossier ! \n" + e.ToString() ;
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
