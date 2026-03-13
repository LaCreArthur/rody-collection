using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {
	static readonly Color EnabledPreviewColor = Color.white;
	static readonly Color DisabledPreviewColor = new Color(0.25f, 0.25f, 0.25f, 1f);

	public GameObject[] buttons;
	public GameObject[] scenes;

	public int sceneToLoad = 1;
	public int actionToLoad = 0;

	Sprite[] defaultScenePreviewSprites;

	public void ClickButton(GameObject button) {
		Debug.Log(button.name);
	}

	public void ClickScene(GameObject scene) {
		Debug.Log(scene.name);
	}

	void Start()
	{
		Cursor.visible = false;
		WorkingStory.CurrentSceneIndex = 1;

		if (!WorkingStory.IsLoaded)
		{
			Debug.LogError("[MenuManager] WorkingStory not loaded - returning to story selection");
			SceneManager.LoadScene(0);
			return;
		}

		Debug.Log($"[MenuManager] Story loaded: {WorkingStory.Title}");
		StartCoroutine(InitFromWorkingStory());
	}

	/// <summary>
	/// Initialize scene thumbnails from WorkingStory.
	/// </summary>
	IEnumerator InitFromWorkingStory()
	{
		CacheDefaultScenePreviewSprites();

		// Load scene thumbnails from WorkingStory
		int visibleSceneCount = Mathf.Min(WorkingStory.SceneCount, scenes.Length);
		Toggle firstAvailableToggle = null;
		for (int i = 0; i < scenes.Length; i++)
		{
			int sceneIndex = i + 1;
			GameObject sceneSlot = scenes[i];
			GameObject image = sceneSlot.transform.GetChild(0).gameObject;
			Image previewImage = image != null ? image.GetComponent<Image>() : null;
			Toggle toggle = sceneSlot.GetComponent<Toggle>();
			bool hasScene = sceneIndex <= visibleSceneCount;

			if (previewImage != null)
			{
				Sprite sprite = hasScene ? WorkingStory.LoadSprite($"{sceneIndex}.1.png", 320, 130) : null;
				previewImage.sprite = sprite != null ? sprite : defaultScenePreviewSprites[i];
				previewImage.color = hasScene ? EnabledPreviewColor : DisabledPreviewColor;
			}

			if (toggle != null)
			{
				toggle.interactable = hasScene;
				if (!hasScene)
					toggle.isOn = false;
				else if (firstAvailableToggle == null)
					firstAvailableToggle = toggle;
			}
		}

		if (firstAvailableToggle != null)
		{
			sceneToLoad = 1;
			firstAvailableToggle.isOn = true;
		}

		yield return null; // Allow frame to render

		// Animate buttons appearing
		foreach (GameObject button in buttons)
		{
			yield return new WaitForSeconds(0.2f);
			button.SetActive(true);
		}

		// Animate scenes appearing
		for (int i = 3; i < 16; i += 4)
		{
			yield return new WaitForSeconds(0.2f);
			scenes[i].SetActive(true);
		}
		for (int i = 14; i > 0; i -= 4)
		{
			yield return new WaitForSeconds(0.2f);
			scenes[i].SetActive(true);
		}
		for (int i = 1; i < 14; i += 4)
		{
			yield return new WaitForSeconds(0.2f);
			scenes[i].SetActive(true);
		}
		for (int i = 12; i >= 0; i -= 4)
		{
			yield return new WaitForSeconds(0.2f);
			scenes[i].SetActive(true);
		}

		Cursor.visible = true;
	}

	void CacheDefaultScenePreviewSprites()
	{
		if (defaultScenePreviewSprites != null && defaultScenePreviewSprites.Length == scenes.Length)
			return;

		defaultScenePreviewSprites = new Sprite[scenes.Length];
		for (int i = 0; i < scenes.Length; i++)
		{
			if (scenes[i] == null || scenes[i].transform.childCount == 0)
				continue;

			Image previewImage = scenes[i].transform.GetChild(0).GetComponent<Image>();
			if (previewImage != null)
				defaultScenePreviewSprites[i] = previewImage.sprite;
		}
	}

	public void OnNext() {
		switch(actionToLoad) {
			case 0: // Bouton scene
				WorkingStory.CurrentSceneIndex = sceneToLoad;
				SceneManager.LoadScene(3);
				break;
			case 1: // Bouton Draw (Edit)
				WorkingStory.CurrentSceneIndex = 0;
				ForkAndEdit();
				break;
			case 2: // Bouton intro (return to story selection)
				WorkingStory.CurrentSceneIndex = 0;
				ExportReminder.NavigateToMenuWithCheck();
				break;
			default: break;
		}
	}

	/// <summary>
	/// Forks official stories before editing.
	/// WorkingStory is already loaded at this point.
	/// </summary>
	private void ForkAndEdit()
	{
		if (!WorkingStory.IsLoaded)
		{
			Debug.LogError("[MenuManager] WorkingStory not loaded");
			return;
		}

		// If official story, fork for editing (creates in-memory copy)
		if (WorkingStory.IsOfficial)
		{
			Debug.Log("[MenuManager] Forking official story for editing...");
			WorkingStory.ForkForEditing();
		}
		else
		{
			Debug.Log("[MenuManager] Already a user story, editing in place");
		}

		// Go to editor
		SceneManager.LoadScene(6);
	}
}
