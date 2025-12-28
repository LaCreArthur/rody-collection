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
	Sprite coverImgSprite;
	Text feedbackTxt;

	void Start () {
		titleInput.characterValidation = InputField.CharacterValidation.Name;
		feedbackTxt = feedbackPanel.GetComponentInChildren<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		buttonAccept.interactable = titleInput.text.Length == 0  ? false : true;
	}

	public void NG_OnAcceptClick() {
		string title = titleInput.text;
		if (string.IsNullOrEmpty(title))
		{
			feedbackTxt.text = "Entre un titre pour ton histoire, Rody!";
			buttonNop.SetActive(true);
			newGamePanel.SetActive(false);
			feedbackPanel.SetActive(true);
			return;
		}

		// Create new story in memory using WorkingStory
		Debug.Log($"[RA_NewGame] Creating new story: {title}");
		WorkingStory.CreateNew(title);

		// Handle cover image if specified
		if (imgInput.text.Length > 0 && coverImgSprite != null)
		{
			// Save cover sprite to WorkingStory
			WorkingStory.SaveSprite("cover.png", coverImgSprite.texture);
		}

		// Set PlayerPrefs for backward compatibility during migration
		// Use a special prefix to indicate in-memory story
		PlayerPrefs.SetString("gamePath", $"memory:{WorkingStory.Id}");
		PlayerPrefs.SetInt("scenesCount", WorkingStory.SceneCount);

		// Success feedback
		feedbackTxt.text = $"L'histoire \"{title}\" a été créée!\nRendez-vous dans Rody Maker pour l'éditer.";
		yeapTxt.text = "ok";
		buttonYeap.SetActive(true);
		buttonYeap.GetComponent<Button>().onClick.RemoveAllListeners();
		buttonYeap.GetComponent<Button>().onClick.AddListener(delegate {UnsetfeedbackPanel(2);});
		newGamePanel.SetActive(false);
		feedbackPanel.SetActive(true);
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

	/// <summary>
	/// Imports a .rody.json story file into memory for play/edit.
	/// </summary>
	public void OnImportClick()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		// WebGL: Use browser file picker via jslib
		WebGLFileBrowser.Instance.OpenFileAsText(".json,application/json", OnWebGLImportComplete);
#else
		var extensions = new[] { new ExtensionFilter("Rody Story", "rody.json", "json") };
		string[] files = StandaloneFileBrowser.OpenFilePanel("Importer une histoire", "", extensions, false);

		if (files.Length == 0)
		{
			return; // User cancelled
		}

		string filePath = files[0];
		Debug.Log($"[RA_NewGame] Importing story from: {filePath}");

		// Read and load into WorkingStory
		string json;
		try
		{
			json = File.ReadAllText(filePath);
		}
		catch (System.Exception e)
		{
			Debug.LogError($"[RA_NewGame] Failed to read file: {e.Message}");
			feedbackTxt.text = $"Impossible de lire le fichier!\n{e.Message}";
			buttonNop.SetActive(true);
			feedbackPanel.SetActive(true);
			return;
		}

		// Load into WorkingStory (remembers file path for quick save)
		WorkingStory.LoadFromJson(json, filePath);

		if (!WorkingStory.IsLoaded)
		{
			feedbackTxt.text = "Le fichier n'est pas valide!";
			buttonNop.SetActive(true);
			feedbackPanel.SetActive(true);
			return;
		}

		// Set PlayerPrefs for backward compatibility during migration
		PlayerPrefs.SetString("gamePath", $"memory:{WorkingStory.Id}");
		PlayerPrefs.SetInt("scenesCount", WorkingStory.SceneCount);

		// Success - offer to play or edit
		feedbackTxt.text = $"L'histoire \"{WorkingStory.Title}\" a été importée!\n({WorkingStory.SceneCount} scènes)";
		yeapTxt.text = "Jouer";
		buttonYeap.SetActive(true);
		buttonYeap.GetComponent<Button>().onClick.RemoveAllListeners();
		buttonYeap.GetComponent<Button>().onClick.AddListener(delegate {
			feedbackPanel.SetActive(false);
			buttonYeap.SetActive(false);
			buttonNop.SetActive(false);
			SceneManager.LoadScene(1); // Load title scene
		});
		feedbackPanel.SetActive(true);

		Debug.Log($"[RA_NewGame] Story imported: {WorkingStory.Title}");
#endif
	}

#if UNITY_WEBGL && !UNITY_EDITOR
	/// <summary>
	/// Callback for WebGL file import - receives JSON content from browser.
	/// </summary>
	private void OnWebGLImportComplete(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			Debug.Log("[RA_NewGame] WebGL import cancelled or failed");
			return; // User cancelled or error
		}

		Debug.Log($"[RA_NewGame] WebGL import received {json.Length} chars");

		// Load into WorkingStory (no file path for WebGL - story exists only in memory)
		WorkingStory.LoadFromJson(json, null);

		if (!WorkingStory.IsLoaded)
		{
			feedbackTxt.text = "Le fichier n'est pas valide!";
			buttonNop.SetActive(true);
			feedbackPanel.SetActive(true);
			return;
		}

		// Set PlayerPrefs for backward compatibility
		PlayerPrefs.SetString("gamePath", $"memory:{WorkingStory.Id}");
		PlayerPrefs.SetInt("scenesCount", WorkingStory.SceneCount);

		// Success - offer to play
		feedbackTxt.text = $"L'histoire \"{WorkingStory.Title}\" a été importée!\n({WorkingStory.SceneCount} scènes)";
		yeapTxt.text = "Jouer";
		buttonYeap.SetActive(true);
		buttonYeap.GetComponent<Button>().onClick.RemoveAllListeners();
		buttonYeap.GetComponent<Button>().onClick.AddListener(delegate {
			feedbackPanel.SetActive(false);
			buttonYeap.SetActive(false);
			buttonNop.SetActive(false);
			SceneManager.LoadScene(1); // Load title scene
		});
		feedbackPanel.SetActive(true);

		Debug.Log($"[RA_NewGame] WebGL story imported: {WorkingStory.Title}");
	}
#endif

	/// <summary>
	/// Exports the currently loaded WorkingStory to a .rody.json file.
	/// </summary>
	public void OnExportClick()
	{
		if (!WorkingStory.IsLoaded)
		{
			feedbackTxt.text = "Aucune histoire n'est chargée pour l'export!";
			buttonNop.SetActive(true);
			feedbackPanel.SetActive(true);
			return;
		}

#if UNITY_WEBGL && !UNITY_EDITOR
		// WebGL: Download via browser
		string json = WorkingStory.ExportToJson();
		if (string.IsNullOrEmpty(json))
		{
			feedbackTxt.text = "L'export a échoué!";
			buttonNop.SetActive(true);
			feedbackPanel.SetActive(true);
			return;
		}

		string filename = WorkingStory.Id + ".rody.json";
		WebGLFileBrowser.Instance.DownloadTextFile(filename, json, () => {
			feedbackTxt.text = $"L'histoire a été téléchargée!\n{filename}";
			yeapTxt.text = "ok";
			buttonYeap.SetActive(true);
			buttonYeap.GetComponent<Button>().onClick.RemoveAllListeners();
			buttonYeap.GetComponent<Button>().onClick.AddListener(delegate { UnsetfeedbackPanel(0); });
			feedbackPanel.SetActive(true);
		});
#else
		// Desktop: Save file dialog
		string suggestedName = WorkingStory.Id + ".rody.json";
		string savePath = StandaloneFileBrowser.SaveFilePanel("Exporter l'histoire", "", suggestedName, "rody.json");

		if (string.IsNullOrEmpty(savePath))
		{
			return; // User cancelled
		}

		// Export to file
		string json = WorkingStory.ExportToJson();
		if (string.IsNullOrEmpty(json))
		{
			feedbackTxt.text = "L'export a échoué!";
			buttonNop.SetActive(true);
			feedbackPanel.SetActive(true);
			return;
		}

		try
		{
			File.WriteAllText(savePath, json);
			WorkingStory.MarkSaved(savePath);

			feedbackTxt.text = $"L'histoire a été exportée!\n{Path.GetFileName(savePath)}";
			yeapTxt.text = "ok";
			buttonYeap.SetActive(true);
			buttonYeap.GetComponent<Button>().onClick.RemoveAllListeners();
			buttonYeap.GetComponent<Button>().onClick.AddListener(delegate { UnsetfeedbackPanel(0); });
		}
		catch (System.Exception e)
		{
			Debug.LogError($"[RA_NewGame] Export failed: {e.Message}");
			feedbackTxt.text = $"L'export a échoué!\n{e.Message}";
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
					Debug.Log($"[RA_NewGame] Deleting: {gamePath}");

					if (!string.IsNullOrEmpty(gamePath) && File.Exists(gamePath))
					{
						File.Delete(gamePath);
						Debug.Log($"[RA_NewGame] Deleted: {gamePath}");
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
