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

#if UNITY_WEBGL && !UNITY_EDITOR
		StartCoroutine(InitWebGL());
#else
		InitDesktop();
#endif

		StartCoroutine(music());
		StartCoroutine(appear());
	}

	void InitDesktop()
	{
		if (isTitle) {
			int customGame = PlayerPrefs.GetInt("customGame");
			string gamePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Rody7");
			if (customGame == 1) {
				string customGamePath = PlayerPrefs.GetString("gamePath");
				// For JSON stories, check if file exists. For folders, check if directory exists.
				if (!string.IsNullOrEmpty(customGamePath) &&
				    (File.Exists(customGamePath) || Directory.Exists(customGamePath))){
					gamePath = customGamePath;
					Debug.Log("Launching Custom Game");
				}
				PlayerPrefs.SetInt("customGame",0);
			}

			PlayerPrefs.SetString("gamePath", gamePath);
			Debug.Log("Title set gamePath as : " + gamePath);

			PlayerPrefs.SetInt("scenesCount", RM_SaveLoad.CountScenesTxt());
			Debug.Log("scenes in this game folder : " + PlayerPrefs.GetInt("scenesCount"));

			// Load title sprite - use JSON provider for JSON stories
			if (PathManager.IsJsonStory)
			{
				titleImage.sprite = RM_SaveLoad.LoadTitleSprite();
			}
			else
			{
				titleImage.sprite = RM_SaveLoad.LoadSprite(PathManager.GetSpritePath("0.png"),0,320,200);
			}
			Cursor.visible = false;
		}
		else {
			Cursor.visible = true;
			Debug.Log("CREDITS ROLL");
			RM_SaveLoad.LoadCredits(GameObject.Find("Title").GetComponent<Text>(),GameObject.Find("Credits").GetComponent<Text>());
		}
	}

	IEnumerator InitWebGL()
	{
		yield return null; // Yield once to let UI update

		if (isTitle) {
			// gamePath is already set by RA_ScrollView (it's the story ID)
			string storyId = PlayerPrefs.GetString("gamePath");
			Debug.Log($"[Title] WebGL: Loading story '{storyId}'");

			// Get scene count from provider
			var provider = StoryProviderManager.Provider;
			var stories = provider.GetStories();
			var story = stories.Find(s => s.id == storyId);

			if (story != null)
			{
				PlayerPrefs.SetInt("scenesCount", story.sceneCount);
				Debug.Log($"[Title] WebGL: Story has {story.sceneCount} scenes");
			}
			else
			{
				Debug.LogWarning($"[Title] WebGL: Story '{storyId}' not found, using default scene count");
				PlayerPrefs.SetInt("scenesCount", 10);
			}

			// Load title image synchronously from provider
			var titleSprite = provider.LoadSprite(storyId, "0.png", 320, 200);
			if (titleSprite != null && titleImage != null)
			{
				titleImage.sprite = titleSprite;
			}
			else
			{
				Debug.LogWarning($"[Title] WebGL: Failed to load title sprite for '{storyId}'");
			}

			Cursor.visible = false;
		}
		else {
			Cursor.visible = true;
			Debug.Log("CREDITS ROLL (WebGL)");

			// Load credits synchronously from provider
			string storyId = PlayerPrefs.GetString("gamePath");
			var provider = StoryProviderManager.Provider;
			string creditsText = provider.LoadCredits(storyId);

			var titleTextComponent = GameObject.Find("Title")?.GetComponent<UnityEngine.UI.Text>();
			var creditsTextComponent = GameObject.Find("Credits")?.GetComponent<UnityEngine.UI.Text>();

			if (!string.IsNullOrEmpty(creditsText))
			{
				string[] lines = creditsText.Split('\n');
				if (titleTextComponent != null && lines.Length > 0)
					titleTextComponent.text = lines[0];
				if (creditsTextComponent != null && lines.Length > 1)
					creditsTextComponent.text = string.Join("\n", lines, 1, lines.Length - 1);
			}
		}
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
