using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SFB;

public class MenuManager : MonoBehaviour {

	public GameObject[] buttons;
	public GameObject[] scenes;

	public int sceneToLoad = 1;
	public int actionToLoad = 0;

	private string gamePath;

	public void ClickButton(GameObject button) {
		Debug.Log(button.name);
	}
	
	public void ClickScene(GameObject scene) {
		Debug.Log(scene.name);
	}
	
	void Start () {
		Cursor.visible = false;
		gamePath = PlayerPrefs.GetString("gamePath");
		Debug.Log("Menu : gamePath : " + gamePath);
		PlayerPrefs.SetInt("currentScene", 1);
#if UNITY_WEBGL && !UNITY_EDITOR
		StartCoroutine(InitWebGL());
#else
		StartCoroutine(init());
#endif
	}

	IEnumerator init() {
		// Load scene thumbnails
		for (int i = 0; i < 16; i++) {
			GameObject image = scenes[i].transform.GetChild(0).gameObject;
			int sceneIndex = i + 1;

			// Use JSON provider for JSON stories, folder-based for others
			if (PathManager.IsJsonStory)
			{
				string spriteName = $"{sceneIndex}.1.png";
				image.GetComponent<Image>().sprite = RM_SaveLoad.LoadSceneThumbnail(sceneIndex);
			}
			else
			{
				string spritePath = PathManager.SpritesPath + System.IO.Path.DirectorySeparatorChar;
				image.GetComponent<Image>().sprite = RM_SaveLoad.LoadSprite(spritePath + sceneIndex, 1, 61, 25);
			}
		}

		foreach (GameObject button in buttons) {
			yield return new WaitForSeconds(0.2f);
			button.SetActive (true);
		}

		for (int i = 3; i<16 ; i+=4) {
			GameObject scene = scenes[i];
			yield return new WaitForSeconds(0.2f);
			scene.SetActive (true);
		}
		for (int i = 14; i>0 ; i-=4) {
			GameObject scene = scenes[i];
			yield return new WaitForSeconds(0.2f);
			scene.SetActive (true);
		}
		for (int i = 1; i<14 ; i+=4) {
			GameObject scene = scenes[i];
			yield return new WaitForSeconds(0.2f);
			scene.SetActive (true);
		}
		for (int i = 12; i>=0 ; i-=4) {
			GameObject scene = scenes[i];
			yield return new WaitForSeconds(0.2f);
			scene.SetActive (true);
		}
		Cursor.visible = true;
	}

	IEnumerator InitWebGL() {
		// Load scene thumbnails asynchronously from Firebase
		int loadedCount = 0;
		int totalToLoad = 16;

		for (int i = 0; i < 16; i++) {
			int sceneIndex = i + 1;
			GameObject image = scenes[i].transform.GetChild(0).gameObject;
			string spriteName = $"{sceneIndex}.1.png";

			RM_SaveLoad.LoadSpriteAsync(spriteName,
				sprite => {
					if (sprite != null && image != null)
						image.GetComponent<Image>().sprite = sprite;
					loadedCount++;
				},
				error => {
					Debug.LogWarning($"[MenuManager] Failed to load thumbnail {spriteName}: {error}");
					loadedCount++;
				}
			);
		}

		// Wait for all thumbnails to load (or fail)
		while (loadedCount < totalToLoad)
			yield return null;

		// Animate buttons appearing
		foreach (GameObject button in buttons) {
			yield return new WaitForSeconds(0.2f);
			button.SetActive(true);
		}

		// Animate scenes appearing (same pattern as desktop)
		for (int i = 3; i < 16; i += 4) {
			yield return new WaitForSeconds(0.2f);
			scenes[i].SetActive(true);
		}
		for (int i = 14; i > 0; i -= 4) {
			yield return new WaitForSeconds(0.2f);
			scenes[i].SetActive(true);
		}
		for (int i = 1; i < 14; i += 4) {
			yield return new WaitForSeconds(0.2f);
			scenes[i].SetActive(true);
		}
		for (int i = 12; i >= 0; i -= 4) {
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
				//LoadGame();
				//Debug.Log(gamePath);
				//Debug.Log(PlayerPrefs.GetString("gamePath"));
				PlayerPrefs.SetInt("currentScene",0);
				SceneManager.LoadScene(0);
				break;
			default: break;
		}
	}

	/// <summary>
	/// Forks official stories to user space before editing.
	/// User stories (JSON or folder) are edited in place.
	/// </summary>
	private void ForkAndEdit() {
#if UNITY_WEBGL && !UNITY_EDITOR
		// WebGL: Edit not supported for now (needs IndexedDB implementation)
		Debug.LogWarning("[MenuManager] Edit not yet supported on WebGL");
		return;
#else
		string currentPath = PlayerPrefs.GetString("gamePath");

		// User stories can be edited directly (both JSON and folder)
		if (PathManager.IsUserStory)
		{
			Debug.Log("[MenuManager] Editing user story in place: " + currentPath);
			SceneManager.LoadScene(6);
			return;
		}

		// Official stories need to be forked first
		Debug.Log("[MenuManager] Forking official story to user space...");

		string forkedPath;
		if (PathManager.IsJsonStory)
		{
			// Fork JSON story
			forkedPath = PathManager.ForkJsonStory(currentPath);
		}
		else
		{
			// Fork folder-based story
			forkedPath = UserStoryProvider.ForkStory(currentPath);
		}

		if (string.IsNullOrEmpty(forkedPath))
		{
			Debug.LogError("[MenuManager] Fork failed, cannot edit");
			return;
		}

		// Update gamePath to the forked copy
		PlayerPrefs.SetString("gamePath", forkedPath);
		PlayerPrefs.SetInt("scenesCount", RM_SaveLoad.CountScenesTxt());
		Debug.Log("[MenuManager] Now editing forked story: " + forkedPath);

		SceneManager.LoadScene(6);
#endif
	}

	private void LoadGame(){
		string[] folderPath = StandaloneFileBrowser.OpenFolderPanel("Selectionne le dossier de ton jeu...", Application.dataPath + "/StreamingAssets", false);
		if (folderPath.Length == 0)
        {
            return;
        }
        gamePath = folderPath[0];
        PlayerPrefs.SetInt("customGame", 1);
		PlayerPrefs.SetString("gamePath", gamePath);
		Debug.Log("LoadGame (gamePath): " + gamePath);
		SceneManager.LoadScene(1);
	}
}
