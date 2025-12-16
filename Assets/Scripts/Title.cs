using SFB;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour {

	public AudioSource titleMusic;
	public GameObject[] blackPanels;
	public Image titleImage;
	private int click = 0;
	private bool isTitle = true; // because same script is use for credits 
	//TODO : improve this
	int konamiIndex = 0;
	KeyCode[] konamiCode = new KeyCode[] { KeyCode.UpArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.B, KeyCode.A };
	
	// Use this for initialization
	void Start () {
		Screen.SetResolution(1280, 800, false);
		isTitle = (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 1)? true:false;
		if (isTitle) {
			int customGame = PlayerPrefs.GetInt("customGame");
			string gamePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Rody7");
			if (customGame == 1) {
				string customGamePath = PlayerPrefs.GetString("gamePath");
				if (customGamePath != "" || Directory.Exists(customGamePath)){
					gamePath = customGamePath;
					Debug.Log("Launching Custom Game");
				}
				PlayerPrefs.SetInt("customGame",0);
			}

			PlayerPrefs.SetString("gamePath", gamePath);
			Debug.Log("Title set gamePath as : " + gamePath);

			PlayerPrefs.SetInt("scenesCount", RM_SaveLoad.CountScenesTxt());
			Debug.Log("scenes in this game folder : " + PlayerPrefs.GetInt("scenesCount"));
		
			titleImage.sprite = RM_SaveLoad.LoadSprite(PathManager.GetSpritePath("0.png"),0,320,200);
			Cursor.visible = false;
		}
		else {
			Cursor.visible = true;
			Debug.Log("CREDITS ROLL");
			RM_SaveLoad.LoadCredits(GameObject.Find("Title").GetComponent<Text>(),GameObject.Find("Credits").GetComponent<Text>());
		}
	
		StartCoroutine(music());
		StartCoroutine(appear());
	}

	void Update() {
        if (Input.GetMouseButtonDown(0))
        {
			click++;
			if (click == 5) {
				skipCredit();
			}
        }
		
		if (Input.anyKeyDown) {

			if (Input.GetKeyDown(KeyCode.Escape)) {
				Cursor.visible = true;
				SceneManager.LoadScene(0);
			}
			else if (Input.GetKeyDown(KeyCode.Return)) {
				skipCredit();
			}
			else if (Input.GetKeyDown(konamiCode[konamiIndex])) {
				konamiIndex++;
				Debug.Log("konami : " + konamiIndex);
			}
			else {
				konamiIndex = 0;
				Debug.Log("konami : " + konamiIndex);
			}
		}
		if (konamiIndex == 10) skipCredit();
	}
	IEnumerator music() {
		yield return new WaitForSeconds(0.1f);
		while(titleMusic.isPlaying)
			yield return null;
		Debug.Log("title music ended");
		Cursor.visible = true;
		if (isTitle)
			skipCredit();
	}

	IEnumerator appear() {
		if (!isTitle) {
			yield return null;
		}
		else {
			yield return new WaitForSeconds(0.5f);
			foreach (GameObject panel in blackPanels) {
				yield return new WaitForSeconds(0.2f);
				panel.SetActive (false);
			}
		}
	}

	public void skipCredit(){
		SceneManager.LoadScene(2);
	}
}
