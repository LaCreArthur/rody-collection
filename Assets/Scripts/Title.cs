using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour {

	public AudioSource titleMusic;
	public GameObject[] blackPanels;
	public Image titleImage;
	private int click = 0;
	private bool isTitle = true; // because same script is use for credits
	int konamiIndex = 0;
	KeyCode[] konamiCode = new KeyCode[] { KeyCode.UpArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.B, KeyCode.A };

	void Start()
	{
		Screen.SetResolution(1280, 800, false);
		isTitle = (SceneManager.GetActiveScene().buildIndex == 1);

		if (!WorkingStory.IsLoaded)
		{
			Debug.LogError("[Title] WorkingStory not loaded - returning to story selection");
			SceneManager.LoadScene(0);
			return;
		}

		InitFromWorkingStory();
		StartCoroutine(music());
		StartCoroutine(appear());
	}

	/// <summary>
	/// Initialize from WorkingStory (in-memory story data).
	/// </summary>
	void InitFromWorkingStory()
	{
		if (isTitle)
		{
			Debug.Log($"[Title] Loading story: {WorkingStory.Title}");

			PlayerPrefs.SetInt("scenesCount", WorkingStory.SceneCount);
			Debug.Log($"[Title] Story has {WorkingStory.SceneCount} scenes");

			// Load title sprite from WorkingStory
			var titleSprite = WorkingStory.LoadSprite("0.png", 320, 200);
			if (titleSprite != null && titleImage != null)
			{
				titleImage.sprite = titleSprite;
			}
			else
			{
				Debug.LogWarning("[Title] Failed to load title sprite");
			}

			Cursor.visible = false;
		}
		else
		{
			Cursor.visible = true;
			Debug.Log("[Title] CREDITS ROLL");

			// Load credits from WorkingStory
			string creditsText = WorkingStory.GetCredits();

			var titleTextComponent = GameObject.Find("Title")?.GetComponent<Text>();
			var creditsTextComponent = GameObject.Find("Credits")?.GetComponent<Text>();

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
			}
			else {
				konamiIndex = 0;
			}
		}
		if (konamiIndex == konamiCode.Length) skipCredit();
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
