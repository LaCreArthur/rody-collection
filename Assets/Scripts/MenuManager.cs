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
		StartCoroutine(init());
	}
	
	IEnumerator init() {
		string spritePath = System.IO.Path.Combine(gamePath, "Sprites") + System.IO.Path.DirectorySeparatorChar;

		for (int i = 0; i<16; i++) { //TODO if scrollable menu : change 16 to dynamic number
			GameObject image = scenes[i].transform.GetChild(0).gameObject;
			//Debug.Log("load miniature : " + spritePath + (i+1) + ".1.png");
			image.GetComponent<Image>().sprite = 
			RM_SaveLoad.LoadSprite(spritePath + (i+1) , 1, 61, 25);
            //Debug.Log(spritePath+i+".1.png");
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

	public void OnNext() {
		switch(actionToLoad) {
			case 0: // Bouton scene
				PlayerPrefs.SetInt("currentScene", sceneToLoad);
				SceneManager.LoadScene(3);
				break;
			case 1: // Bouton Draw
				PlayerPrefs.SetInt("currentScene",0);
				SceneManager.LoadScene(6);
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
