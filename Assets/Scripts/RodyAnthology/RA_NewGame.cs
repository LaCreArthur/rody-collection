using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using SFB;

public class RA_NewGame : MonoBehaviour {

	public InputField titleInput;
	public InputField imgInput;
	public GameObject newGamePanel;

	public Button buttonAccept, buttonCancel;
	public RA_ScrollView sv;
	[SerializeField] RA_FeedbackPanel feedbackPanel;
	Sprite coverImgSprite;

	void Start() {
		titleInput.characterValidation = InputField.CharacterValidation.Name;
	}

	void Update() {
		buttonAccept.interactable = titleInput.text.Length > 0;
	}

	public void NG_OnAcceptClick() {
		string title = titleInput.text;
		if (string.IsNullOrEmpty(title))
		{
			newGamePanel.SetActive(false);
			feedbackPanel.ShowMessage("Entre un titre pour ton histoire, Rody!");
			return;
		}

		Debug.Log($"[RA_NewGame] Creating new story: {title}");
		WorkingStory.CreateNew(title);

		if (imgInput.text.Length > 0 && coverImgSprite != null)
			WorkingStory.SaveSprite("cover.png", coverImgSprite.texture);

		PlayerPrefs.SetString("gamePath", $"memory:{WorkingStory.Id}");
		PlayerPrefs.SetInt("scenesCount", WorkingStory.SceneCount);

		newGamePanel.SetActive(false);
		feedbackPanel.ShowMessage(
			$"L'histoire \"{title}\" a été créée!\nRendez-vous dans Rody Maker pour l'éditer.",
			() => SceneManager.LoadScene(6));
	}

	public void NG_OnCancelClick() => newGamePanel.SetActive(false);

	public void NG_ImgClick()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		WebGLFileBrowser.Instance.OpenImageAsBase64("image/png,image/jpeg", OnWebGLCoverImported);
#else
		var extensions = new[] { new ExtensionFilter("Images", "png", "jpg", "jpeg") };
		string[] files = StandaloneFileBrowser.OpenFilePanel("Choix de l'image de couverture", "", extensions, false);
		if (files.Length == 0) return;

		imgInput.text = files[0];
		coverImgSprite = RM_SaveLoad.LoadSprite(files[0], 0, 340, 480);
#endif
	}

#if UNITY_WEBGL && !UNITY_EDITOR
	void OnWebGLCoverImported(string dataUrl)
	{
		if (string.IsNullOrEmpty(dataUrl)) return;
		var tex = WebGLFileBrowser.DataUrlToTexture(dataUrl);
		if (tex == null) return;
		RM_TextureScale.Point(tex, 320, 200);
		coverImgSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
		imgInput.text = "cover.png";
	}
#endif

	/// <summary>
	/// Imports a .rody.json story file into memory for play/edit.
	/// Shows confirmation if a story is already loaded.
	/// </summary>
	public void OnImportClick()
	{
		if (WorkingStory.IsLoaded)
		{
			feedbackPanel.ShowConfirm(
				"Une histoire est déjà chargée.\nVoulez-vous la remplacer?",
				"oui",
				DoImport);
			return;
		}
		DoImport();
	}

	void DoImport()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		WebGLFileBrowser.Instance.OpenFileAsText(".json,application/json", OnWebGLImportComplete);
#else
		var extensions = new[] { new ExtensionFilter("Rody Story", "rody.json", "json") };
		string[] files = StandaloneFileBrowser.OpenFilePanel("Importer une histoire", "", extensions, false);

		if (files.Length == 0) return;

		string filePath = files[0];
		Debug.Log($"[RA_NewGame] Importing story from: {filePath}");

		string json;
		try
		{
			json = File.ReadAllText(filePath);
		}
		catch (System.Exception e)
		{
			Debug.LogError($"[RA_NewGame] Failed to read file: {e.Message}");
			feedbackPanel.ShowMessage($"Impossible de lire le fichier!\n{e.Message}");
			return;
		}

		WorkingStory.LoadFromJson(json, filePath);
		if (!WorkingStory.IsLoaded)
		{
			feedbackPanel.ShowMessage("Le fichier n'est pas une histoire valide!");
			return;
		}

		PlayerPrefs.SetString("gamePath", $"memory:{WorkingStory.Id}");
		PlayerPrefs.SetInt("scenesCount", WorkingStory.SceneCount);

		sv.ResetAndSelectWorkingStory();
		feedbackPanel.ShowMessage($"Histoire «{WorkingStory.Title}» importée avec succès!");
		Debug.Log($"[RA_NewGame] Story imported: {WorkingStory.Title}");
#endif
	}

#if UNITY_WEBGL && !UNITY_EDITOR
	void OnWebGLImportComplete(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			Debug.Log("[RA_NewGame] WebGL import cancelled or failed");
			return;
		}

		Debug.Log($"[RA_NewGame] WebGL import received {json.Length} chars");
		WorkingStory.LoadFromJson(json, null);

		if (!WorkingStory.IsLoaded)
		{
			feedbackPanel.ShowMessage("Le fichier n'est pas une histoire valide!");
			return;
		}

		PlayerPrefs.SetString("gamePath", $"memory:{WorkingStory.Id}");
		PlayerPrefs.SetInt("scenesCount", WorkingStory.SceneCount);

		sv.ResetAndSelectWorkingStory();
		feedbackPanel.ShowMessage($"Histoire «{WorkingStory.Title}» importée avec succès!");
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
			feedbackPanel.ShowMessage("Aucune histoire n'est chargée pour l'export!");
			return;
		}

#if UNITY_WEBGL && !UNITY_EDITOR
		string json = WorkingStory.ExportToJson();
		if (string.IsNullOrEmpty(json))
		{
			feedbackPanel.ShowMessage("L'export a échoué!");
			return;
		}

		string filename = WorkingStory.Id + ".rody.json";
		WebGLFileBrowser.Instance.DownloadTextFile(filename, json, () => {
			WorkingStory.MarkSaved("download:" + filename);
			if (sv != null) sv.Reset();
			feedbackPanel.ShowMessage($"L'histoire a été téléchargée!\n{filename}");
		});
#else
		string suggestedName = WorkingStory.Id + ".rody.json";
		string savePath = StandaloneFileBrowser.SaveFilePanel("Exporter l'histoire", "", suggestedName, "rody.json");

		if (string.IsNullOrEmpty(savePath)) return;

		string json = WorkingStory.ExportToJson();
		if (string.IsNullOrEmpty(json))
		{
			feedbackPanel.ShowMessage("L'export a échoué!");
			return;
		}

		try
		{
			File.WriteAllText(savePath, json);
			WorkingStory.MarkSaved(savePath);
			if (sv != null) sv.Reset();
			feedbackPanel.ShowMessage($"L'histoire a été exportée!\n{Path.GetFileName(savePath)}");
		}
		catch (System.Exception e)
		{
			Debug.LogError($"[RA_NewGame] Export failed: {e.Message}");
			feedbackPanel.ShowMessage($"L'export a échoué!\n{e.Message}");
		}
#endif
	}

	public void SG_onDelete(bool isDeletable)
	{
		if (isDeletable)
		{
			string gamePath = PlayerPrefs.GetString("gameToDelete");
			feedbackPanel.ShowConfirm(
				"Es-tu sûr de vouloir définitivement supprimer ce jeu ?",
				"oui",
				() => DeleteStory(gamePath));
		}
		else
		{
			feedbackPanel.ShowMessage("Tu ne peux pas supprimer ce jeu !");
		}
	}

	void DeleteStory(string path)
	{
		try
		{
			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				File.Delete(path);
				Debug.Log($"[RA_NewGame] Deleted: {path}");
			}
			PlayerPrefs.SetString("gameToDelete", "");
			PlayerPrefs.SetString("gameToDeleteType", "");
			sv.Reset();
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
			feedbackPanel.ShowMessage("Impossible de supprimer !\n" + e.Message);
		}
	}
}
