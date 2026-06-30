using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
		StoryRoot.Session.CreateNew(title);

		if (imgInput.text.Length > 0 && coverImgSprite != null)
			StoryRoot.Session.SaveSprite("cover.png", coverImgSprite.texture);

		newGamePanel.SetActive(false);

		// Persist the new story (best-effort), then open the editor.
		StoryRoot.Store.SaveUser(StoryRoot.Session.Current, _ =>
			feedbackPanel.ShowMessage(
				$"L'histoire \"{title}\" a été créée!\nRendez-vous dans Rody Maker pour l'éditer.",
				() => SceneManager.LoadScene(AppScenes.Editor)));
	}

	public void NG_OnCancelClick() => newGamePanel.SetActive(false);

	public void NG_ImgClick()
	{
		WebGLFileBrowser.Instance.OpenImageAsBase64("image/png,image/jpeg", OnCoverImported);
	}

	void OnCoverImported(string dataUrl)
	{
		if (string.IsNullOrEmpty(dataUrl)) return;
		var tex = WebGLFileBrowser.DataUrlToTexture(dataUrl);
		if (tex == null) return;

		RM_TextureScale.Point(tex, 320, 200);
		coverImgSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
		imgInput.text = "cover.png";
	}

	/// <summary>Imports a .rody.json story, persists it, and selects it in the menu.</summary>
	public void OnImportClick()
	{
		if (StoryRoot.Session.IsLoaded)
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
		WebGLFileBrowser.Instance.OpenFileAsText(".json,application/json", OnImportComplete);
	}

	void OnImportComplete(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			Debug.Log("[RA_NewGame] Import cancelled or failed");
			return;
		}

		StoryRoot.Session.LoadFromJson(json, null);
		if (!StoryRoot.Session.IsLoaded)
		{
			feedbackPanel.ShowMessage("Le fichier n'est pas une histoire valide!");
			return;
		}

		// Auto-persist so the import survives a reload, then show it in the menu.
		var story = StoryRoot.Session.Current;
		StoryRoot.Store.SaveUser(story, _ =>
		{
			sv.ResetAndSelectStory(story.story.id);
			feedbackPanel.ShowMessage($"Histoire «{StoryRoot.Session.Title}» importée avec succès!");
		});
	}

	/// <summary>Exports the current session story as a downloadable .rody.json (sharing only).</summary>
	public void OnExportClick()
	{
		if (!StoryRoot.Session.IsLoaded)
		{
			feedbackPanel.ShowMessage("Aucune histoire n'est chargée pour l'export!");
			return;
		}

		string json = StoryRoot.Session.ExportToJson();
		if (string.IsNullOrEmpty(json))
		{
			feedbackPanel.ShowMessage("L'export a échoué!");
			return;
		}

		string filename = StoryRoot.Session.Id + ".rody.json";
		WebGLFileBrowser.Instance.DownloadTextFile(filename, json, () =>
			feedbackPanel.ShowMessage($"L'histoire a été téléchargée!\n{filename}"));
	}

	/// <summary>Confirms and deletes a persisted user story by id (null = not deletable).</summary>
	public void SG_onDelete(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			feedbackPanel.ShowMessage("Tu ne peux pas supprimer ce jeu !");
			return;
		}

		feedbackPanel.ShowConfirm(
			"Es-tu sûr de vouloir définitivement supprimer ce jeu ?",
			"oui",
			() => StoryRoot.Store.DeleteUser(id, _ => sv.Reset()));
	}
}
