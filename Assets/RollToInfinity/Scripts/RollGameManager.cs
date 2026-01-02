using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

///<summary>
/// Script to handle game states, UI, HUD, score and level
/// Inits the game, lauches a level, handles pause and replay, plays levelup and game over SFX
///</summary>

public class RollGameManager : MonoBehaviour {

	public GameObject player;
	public GameObject menuCanvas;
	public GameObject HUD;
	public GameObject segController;
	public Slider sliderLvl;
	public Slider sliderSensitivity;
	
	// two scrollbar to represent the current level progression of the player
	public Scrollbar sb_fillingBar;
	public Scrollbar sb_movingPoint;

	// hearts are represented by a scrollbar using a heart sprite in tiled mode
	public Scrollbar sb_healthPoints;
	public Button startButton;
	public Text scoreText, lvlText, speedText, title, finalScore, currentLevel, sensitivityText, instructionText;
	public int score;
	public AudioClip clip_levelup, clip_gameOver;
	public Camera rollCamera;
	
	[HideInInspector] public static bool isStarted = false;
	
	int level = 1;
	int Level {
		get { return level;}

		set { 
			Debug.Log("level is set to " + value);
			
			// Logarithmically increase speed through levels
			pc.speed = pc.startingSpeed + Mathf.Log(value) * pc.startingSpeed / 2;
			Debug.Log("player speed is set to " + pc.speed);
			
			// keep the level min speed 
			pc.levelMinSpeed = pc.speed;
			
			// offset of the player at the new level for next level computation
			levelPlayerPos = player.transform.position.z;
			// Debug.Log("levelplayerpos is set to " + levelPlayerPos);
			
			// distance to the next level
			distanceToNextLvl = pc.startingSpeed * 500 * value + levelPlayerPos * 100;
			Debug.Log("Distance to next level : " + distanceToNextLvl / 100);

			lvlText.text = "LEVEL " + value;
			level = value;
		}
	}
	
	float levelPlayerPos = 0.0f;
	public Volume postProcess;
	WhiteBalance _whiteBalance;
	PlayerController pc;
	float distanceToNextLvl = 1.0f;
	AudioSource audioSource;

	void Awake() {
		// auto adjust some UI element size regarding the screen size
		sb_fillingBar.GetComponent<RectTransform>().sizeDelta = new Vector2(0.75f * Screen.width, 20f);
		sb_movingPoint.GetComponent<RectTransform>().sizeDelta = new Vector2(0.75f * Screen.width, 20f);
		menuCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(0.9f * Screen.width, 0.9f * Screen.height);
		audioSource = GetComponent<AudioSource>();
	}

	void Start() {
		pc = player.GetComponent<PlayerController>();

		// Cache the WhiteBalance component from the Volume profile
		if (postProcess != null)
			postProcess.profile.TryGet(out _whiteBalance);

		// display the starting menu at the beginning
		menuCanvas.SetActive(true);
		// Menu color fade
		FadeColor(-10f);

		// setup the start button properties for a first lauch since the menu is dynamic
		startButton.GetComponent<Button>().onClick.AddListener(LaunchGame);
		startButton.transform.GetChild(0).GetComponent<Text>().text = "START";

		// player can choose the level of difficulty
		sliderLvl.interactable = true;
	}

	void Update () {
		
		// Print currentSpeed
		float currentSpeed = Mathf.Round(player.GetComponent<Rigidbody>().linearVelocity.magnitude);
		speedText.text = currentSpeed.ToString() + " KMH";

		// update scrollbars
		if (isStarted){ // prevent level up at start menu
			sb_movingPoint.value = ((player.transform.position.z - levelPlayerPos) / distanceToNextLvl) * 100;
			sb_fillingBar.size = ((player.transform.position.z - levelPlayerPos) / distanceToNextLvl) * 100;
			
			if (sb_fillingBar.size >= 1) { // End of the Level
				++Level;
				StartCoroutine(FadeInOutText(1f, lvlText));
				audioSource.Play();
			}
			
			if (Input.GetButton("Cancel"))
				OnClickMenu();
		}
	}

	void LaunchGame() {
		ResetGame();
		// active the HUD
		HUD.SetActive(true);
	
		// listen if the player get a pick Up
		PlayerController.Picked += UpdateScore;
		PlayerController.Damaged += UpdateHealth;
	}

	void ResetGame() {
		// reset player position
		player.transform.position = new Vector3(0.0f, 0.5f, 0.0f);
		// reset Level values and dependancies
		Level = level;
		// reset player HP
		pc.health = 3;
		// reset score
		score = -1;
		UpdateScore(1);
		// reset health sprites
		sb_healthPoints.size = 0.187f; // fixed float depending on the sprite size
		// reinit segments
		segController.GetComponent<SegmentController>().initSeg();
		// game start again
		isStarted = true;
		// Restore color fade
		FadeColor(5f);
		// display the level with fading effect
		StartCoroutine(FadeInOutText(1f, lvlText));
		StartCoroutine(FadeInOutText(1f, instructionText));
		// Set menu for ingame pause
		title.text = "ROLL TO INFINITY";
		finalScore.text = "";
		startButton.transform.GetChild(0).GetComponent<Text>().text = "CONTINUE";
		startButton.GetComponent<Button>().onClick.RemoveAllListeners();
		startButton.GetComponent<Button>().onClick.AddListener(ContinueGame);

		// level is not changeable ingame
		sliderLvl.interactable = false;
		// reset audioclip to levelup clip
		audioSource.clip = clip_levelup;
	}

	void FadeColor(float fade) {
		// Update the post processing tint (URP WhiteBalance)
		if (_whiteBalance != null)
		{
			_whiteBalance.tint.overrideState = true;
			_whiteBalance.tint.value = fade;
		}
	}
	void ContinueGame() {
		player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		FadeColor(5f);
	}

	public void UpdateScore(int s) {
		// pickups
		if (s == 1) {
			score += s;
			scoreText.text = "Score: " + score.ToString();
		}
		// heart
		else {
			sb_healthPoints.size += 0.070f; 
		}
	}

	public void UpdateHealth(int h) {
		// it simply shift the scrollbar to hide a heart
		sb_healthPoints.size -= 0.070f; 
		Debug.Log("HP : " + h);
		if (h == 0) {
			// gg wp
			title.text = "GAME OVER !";
			// play gameover sfx
			audioSource.clip = clip_gameOver;
			audioSource.Play();
			// stop the game
			isStarted = false;
			// propose a replay
			startButton.transform.GetChild(0).GetComponent<Text>().text = "REPLAY";
			// set the replay function to the button
			startButton.GetComponent<Button>().onClick.RemoveListener(ContinueGame);
			startButton.GetComponent<Button>().onClick.AddListener(ResetGame);
			// print the final score
			finalScore.text = "SCORE : " + score;
			// update the slider to the current level
			sliderLvl.value = level;
			currentLevel.text = "LEVEL: " + level;
			// player can change the level
			sliderLvl.interactable = true;
			// open the menu
			OnClickMenu();
		}
	}

	public void OnClickMenu() {
		// Hide the HUD
		HUD.SetActive(false);
		// display the menu
		menuCanvas.SetActive(true);
		// darken the colors
		FadeColor(-10f);
		// freeze the player
		player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
	}

	public void OnClickStart() {
		menuCanvas.SetActive(false);
		HUD.SetActive(true);
	}

	public void OnSliderChange() {
		Level = (int)sliderLvl.value;
		currentLevel.text = "LEVEL: " + level;
	}
	
	public void OnSensitivityChange() {
		pc.sensitivity = sliderSensitivity.value;
		sensitivityText.text = "SENSITIVITY: " + sliderSensitivity.value;
	}

	public void OnClickExit() {
		SceneManager.UnloadSceneAsync("RollToInfinity");
		SceneManager.LoadScene(0);
	}

	IEnumerator FadeInOutText(float t, Text i) {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        // fade in 
		while (i.color.a < 1.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / t));
            yield return null;
        }
		// temporisation
		yield return new WaitForSeconds(1.0f);
		// fade out
        while (i.color.a > 0.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
            yield return null;
        }
    }
}
