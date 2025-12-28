using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

	public GameObject[] buttons;
	public GameObject[] scenes;

	public int sceneToLoad = 1;
	public int actionToLoad = 0;

	public void ClickButton(GameObject button) {
		Debug.Log(button.name);
	}

	public void ClickScene(GameObject scene) {
		Debug.Log(scene.name);
	}

	void Start()
	{
		Cursor.visible = false;
		PlayerPrefs.SetInt("currentScene", 1);

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
		// Load scene thumbnails from WorkingStory
		for (int i = 0; i < 16; i++)
		{
			int sceneIndex = i + 1;
			GameObject image = scenes[i].transform.GetChild(0).gameObject;

			var sprite = WorkingStory.LoadSprite($"{sceneIndex}.1.png", 320, 130);
			if (sprite != null && image != null)
				image.GetComponent<Image>().sprite = sprite;
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

	public void OnNext() {
		switch(actionToLoad) {
			case 0: // Bouton scene
				PlayerPrefs.SetInt("currentScene", sceneToLoad);
				SceneManager.LoadScene(3);
				break;
			case 1: // Bouton Draw (Edit)
				PlayerPrefs.SetInt("currentScene",0);
				ForkAndEdit();
				break;
			case 2: // Bouton intro
				PlayerPrefs.SetInt("currentScene",0);
				SceneManager.LoadScene(0);
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

		// Update scenesCount from WorkingStory
		PlayerPrefs.SetInt("scenesCount", WorkingStory.SceneCount);

		// Go to editor
		SceneManager.LoadScene(6);
	}
}
