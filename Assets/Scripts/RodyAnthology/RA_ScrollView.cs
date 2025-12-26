using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class RA_ScrollView : MonoBehaviour {

	public Sprite selected;
	public Sprite notSelected;
	public float lerpSpeed = 0.5f;
	public RA_Menu menu;
	public RA_SoundManager sm;
	public GameObject newGamePanel;
	public GameObject content;
	public Transform slotPrefab;
	public Transform slotNewGamePrefab;
	public Transform slotLoadGamePrefab; // Now used for importing .rody.json files

	[Header("WebGL")]
	public GameObject loadingUI;

	// Tracks which slots are user stories (for deletion/export)
	private HashSet<int> userStorySlotIndices = new HashSet<int>();

	static ScrollRect scrollRect;
	static float t = 0.0f;

	List<GameObject> slots;
	List<GameObject> slotTitles;
	List<Image> slotImages;
	List<Button> slotButtons;

	float step;
	float newPos;
	float oldPos;
	bool isLerping = false;
	int selectedButton, middleSlot;
	public RA_NewGame ngScript;
	bool isScrollViewDisabled = true;

	void Start () {
#if UNITY_WEBGL && !UNITY_EDITOR
		StartCoroutine(InitWebGL());
#else
		Init();
#endif
	}

	/// <summary>
	/// WebGL initialization - waits for Firebase, then loads from provider.
	/// </summary>
	IEnumerator InitWebGL()
	{
		Debug.Log("[RA_ScrollView] InitWebGL() started");
		if (loadingUI != null) loadingUI.SetActive(true);

		// Wait for provider to be ready
		if (!StoryProviderManager.IsReady)
		{
			Debug.Log("[RA_ScrollView] StoryProviderManager not ready, calling Initialize()...");
			StoryProviderManager.Initialize();

			int waitFrames = 0;
			while (!StoryProviderManager.IsReady)
			{
				waitFrames++;
				if (waitFrames % 60 == 0) // Log every 60 frames (~1 second)
				{
					Debug.Log($"[RA_ScrollView] Still waiting for provider... (frame {waitFrames})");
				}
				yield return null;
			}
			Debug.Log($"[RA_ScrollView] Provider ready after {waitFrames} frames");
		}
		else
		{
			Debug.Log("[RA_ScrollView] StoryProviderManager already ready");
		}

		if (loadingUI != null) loadingUI.SetActive(false);
		Debug.Log("[RA_ScrollView] Calling InitFromProvider()...");
		InitFromProvider();
		Debug.Log("[RA_ScrollView] InitWebGL() complete");
	}

	/// <summary>
	/// Initialize from StoryProvider (used on WebGL).
	/// No file system access - gets stories from Firebase.
	/// </summary>
	void InitFromProvider()
	{
		PlayerPrefs.SetInt("rodyMakerFirstTime", 1);
		slots = new List<GameObject>();
		slotTitles = new List<GameObject>();
		int slotIndex = 0;

		var stories = StoryProviderManager.Provider.GetStories();
		Debug.Log($"[RA_ScrollView] Loaded {stories.Count} stories from provider");

		// Order stories (same logic as OrderGameFolder but for metadata)
		var orderedStories = OrderStories(stories);

		foreach (var story in orderedStories)
		{
			if (story.id == "Rody0") continue; // Skip base template

			// Instantiate a slot
			GameObject slot = Instantiate(slotPrefab, content.transform).gameObject;
			slot.name = story.id;
			slot.GetComponentInChildren<Text>().text = story.title;

			// Load cover async on WebGL
			LoadCoverAsync(slot, story.id);

			slots.Add(slot);
			slotIndex++;
		}

		// On WebGL, don't show Load Game / New Game (requires file system)
		// Just finalize the UI
		FinalizeSlots(slotIndex, false);
	}

	List<StoryMetadata> OrderStories(List<StoryMetadata> stories)
	{
		var ordered = new List<StoryMetadata>();
		var remaining = new List<StoryMetadata>(stories);

		string[] preferredOrder = {
			"Rody Et Mastico",
			"Rody Et Mastico II",
			"Rody Et Mastico III",
			"Rody Noël",
			"Rody Et Mastico V",
			"Rody Et Mastico VI",
			"Rody Et Mastico A Ibiza"
		};

		foreach (var name in preferredOrder)
		{
			var found = remaining.Find(s => s.id == name);
			if (found != null)
			{
				ordered.Add(found);
				remaining.Remove(found);
			}
		}

		ordered.AddRange(remaining);
		return ordered;
	}

	void LoadCoverAsync(GameObject slot, string storyId)
	{
		var provider = StoryProviderManager.FirebaseProvider;
		if (provider != null)
		{
			provider.LoadSpriteAsync(storyId, "cover.png",
				sprite => {
					if (slot != null && sprite != null)
					{
						var img = slot.transform.GetChild(0).GetComponent<Image>();
						if (img != null) img.sprite = sprite;
					}
				},
				error => Debug.LogWarning($"[RA_ScrollView] Failed to load cover for {storyId}: {error}")
			);
		}
	}

	void FinalizeSlots(int slotCount, bool includeFileSystemSlots)
	{
		if (includeFileSystemSlots)
		{
			// SLOT NEW GAME
			GameObject newGameSlot = Instantiate(slotNewGamePrefab, content.transform).gameObject;
			newGameSlot.name = "newGame";
			slots.Add(newGameSlot);
			slotCount += 1;
		}

		// Handle empty slot list
		if (slots.Count == 0)
		{
			Debug.LogWarning("[RA_ScrollView] No slots to display!");
			content.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
			slotImages = new List<Image>();
			slotButtons = new List<Button>();
			scrollRect = GetComponent<ScrollRect>();
			return;
		}

		content.GetComponent<RectTransform>().sizeDelta = new Vector2(slotCount * 100, 100);

		slotImages = new List<Image>();
		slotButtons = new List<Button>();
		for (int i = 0; i < slots.Count; i++)
		{
			slotImages.Add(slots[i].GetComponent<Image>());
			slots[i].GetComponent<Button>().onClick.AddListener(OnClick);
			slotButtons.Add(slots[i].GetComponent<Button>());
			GameObject title = slots[i].transform.Find("Title").gameObject;
			slotTitles.Add(title);
			title.SetActive(false);
		}

		step = 1.0f / Mathf.Max(1, slots.Count - 1);
		Debug.Log("slots count : " + slots.Count);
		scrollRect = GetComponent<ScrollRect>();
		scrollRect.onValueChanged.AddListener(OnValueChanged);
		selectedButton = middleSlot = slots.Count / 2;
		scrollRect.horizontalNormalizedPosition = selectedButton * step;
		slotImages[selectedButton].GetComponent<Image>().sprite = selected;
		slotTitles[selectedButton].SetActive(true);
	}

	void Init() {
		// set rodyMakerFirstTime to true at application launch
		PlayerPrefs.SetInt("rodyMakerFirstTime", 1);
		slots  = new List<GameObject>();
		slotTitles  = new List<GameObject>();
		userStorySlotIndices.Clear();
		int slotIndex = 0;
		string[] gameFolders = Directory.GetDirectories(Application.streamingAssetsPath);
		string[] orderedGameFolders = OrderGameFolder(gameFolders);

		// OFFICIAL STORIES (from StreamingAssets)
		foreach (string folder in orderedGameFolders) {

			//Debug.Log(Path.GetDirectoryName(folder));

			string lastFolderName = new DirectoryInfo(folder).Name;
			if (lastFolderName[0] == '_') lastFolderName = lastFolderName.Substring(3);
			// check directory content
			//Debug.Log("checking folder : " + folder);
			if (isGameFolder(folder) && lastFolderName != "Rody0") {

				//Debug.Log("\"" + lastFolderName + "\" est un dossier de jeu valide");

				// instantiate a slot
				GameObject slot = Instantiate(slotPrefab, content.transform).gameObject;
				// load the cover from Sprites folder
				slot.transform.GetChild(0).GetComponent<Image>().sprite = RM_SaveLoad.LoadSprite(folder + "/Sprites/cover.png", 0, 100, 137);
				slot.name = lastFolderName;
				// game's title
				slot.GetComponentInChildren<Text>().text = lastFolderName;

				// add the slot to slots
				slots.Add(slot);

				slotIndex++;
			}
		}

		// USER STORIES (from Application.persistentDataPath/UserStories)
		LoadUserStories(ref slotIndex);

		// SLOT IMPORT (import .rody.json) - reuses the old "Load Game" prefab
		GameObject importSlot = Instantiate(slotLoadGamePrefab, content.transform).gameObject;
		importSlot.name = "importStory";
		slots.Add(importSlot);
		slotIndex++;

		// SLOT NEW GAME
		GameObject newGameSlot = Instantiate(slotNewGamePrefab, content.transform).gameObject;
		newGameSlot.name = "newGame"; // simplify the access;
		slots.Add(newGameSlot);

		// set the size of the content relative to the slot count to have a fluid scroll
		content.GetComponent<RectTransform>().sizeDelta = new Vector2((slotIndex+1) * 100, 100);
		// populate the lists
		slotImages  = new List<Image>();
		slotButtons = new List<Button>();
		for(int i = 0; i < slots.Count; i++) {
			slotImages.Add(slots[i].GetComponent<Image>());
			slots[i].GetComponent<Button>().onClick.AddListener(OnClick);
			slotButtons.Add(slots[i].GetComponent<Button>());

			GameObject title = slots[i].transform.Find("Title")?.gameObject;
			slotTitles.Add(title);
			if (title != null) title.SetActive(false);
		}

		// compute the step required to switch the selected slot
		step = 1.0f / (slots.Count-1);
		Debug.Log("slots count : " + slots.Count);
		scrollRect = GetComponent<ScrollRect>();
        scrollRect.onValueChanged.AddListener(OnValueChanged);
		// set the middle slot to be selected at launch
		selectedButton = middleSlot = slots.Count / 2;
		scrollRect.horizontalNormalizedPosition = (selectedButton) * step;
		slotImages[selectedButton].GetComponent<Image>().sprite = selected;
		if (slotTitles[selectedButton] != null) slotTitles[selectedButton].SetActive(true);
	}

	/// <summary>
	/// Loads user stories from UserStoriesPath and adds them as slots.
	/// Supports both folder-based stories and .rody.json files.
	/// </summary>
	void LoadUserStories(ref int slotIndex)
	{
		PathManager.EnsureUserStoriesDirectory();
		string userStoriesPath = PathManager.UserStoriesPath;

		if (!Directory.Exists(userStoriesPath))
		{
			Debug.Log("[RA_ScrollView] No user stories directory");
			return;
		}

		// Load folder-based user stories
		string[] userFolders = Directory.GetDirectories(userStoriesPath);
		Debug.Log($"[RA_ScrollView] Found {userFolders.Length} user story folders");

		foreach (string folder in userFolders)
		{
			if (isGameFolder(folder))
			{
				string folderName = new DirectoryInfo(folder).Name;

				// Instantiate a slot
				GameObject slot = Instantiate(slotPrefab, content.transform).gameObject;

				// Load the cover from Sprites folder
				string coverPath = Path.Combine(folder, "Sprites", "cover.png");
				if (File.Exists(coverPath))
				{
					slot.transform.GetChild(0).GetComponent<Image>().sprite = RM_SaveLoad.LoadSprite(coverPath, 0, 100, 137);
				}

				slot.name = "user:" + folderName; // Prefix to identify as user story
				slot.GetComponentInChildren<Text>().text = folderName;

				slots.Add(slot);
				userStorySlotIndices.Add(slots.Count - 1);

				slotIndex++;
				Debug.Log($"[RA_ScrollView] Added user story folder: {folderName}");
			}
		}

		// Load .rody.json files
		string[] jsonFiles = Directory.GetFiles(userStoriesPath, "*.rody.json");
		Debug.Log($"[RA_ScrollView] Found {jsonFiles.Length} .rody.json files");

		foreach (string jsonPath in jsonFiles)
		{
			try
			{
				// Create a temporary provider to read metadata
				var tempProvider = new JsonStoryProvider(jsonPath);
				var stories = tempProvider.GetStories();

				if (stories.Count > 0)
				{
					var storyMeta = stories[0];

					// Instantiate a slot
					GameObject slot = Instantiate(slotPrefab, content.transform).gameObject;

					// Try to load cover from the JSON
					var coverSprite = tempProvider.LoadSprite(null, "cover.png", 100, 137);
					if (coverSprite != null)
					{
						slot.transform.GetChild(0).GetComponent<Image>().sprite = coverSprite;
					}

					string fileName = Path.GetFileName(jsonPath);
					slot.name = "json:" + jsonPath; // Full path for JSON files
					slot.GetComponentInChildren<Text>().text = storyMeta.title;

					slots.Add(slot);
					userStorySlotIndices.Add(slots.Count - 1);

					slotIndex++;
					Debug.Log($"[RA_ScrollView] Added JSON story: {storyMeta.title}");
				}
			}
			catch (System.Exception e)
			{
				Debug.LogWarning($"[RA_ScrollView] Failed to load JSON story {jsonPath}: {e.Message}");
			}
		}
	}

	/// <summary>
	/// Checks if a slot index is a user story.
	/// </summary>
	public bool IsUserStorySlot(int index)
	{
		return userStorySlotIndices.Contains(index);
	}

	/// <summary>
	/// Gets the path for a slot, handling official stories, user folder stories, and JSON stories.
	/// </summary>
	string GetSlotPath(int index)
	{
		string slotName = content.transform.GetChild(index).name;

		if (slotName.StartsWith("json:"))
		{
			// JSON story - the full path is stored after the prefix
			return slotName.Substring(5); // Remove "json:" prefix
		}
		else if (slotName.StartsWith("user:"))
		{
			// User story folder - get from UserStoriesPath
			string folderName = slotName.Substring(5); // Remove "user:" prefix
			return Path.Combine(PathManager.UserStoriesPath, folderName);
		}
		else
		{
			// Official story - get from StreamingAssets
			return Path.Combine(Application.streamingAssetsPath, slotName);
		}
	}

	public void Reset() {
		foreach (GameObject slot in slots) {
			GameObject.Destroy(slot);
		}
		slots.Clear();
		slotTitles.Clear();
		slotImages.Clear();
		slotButtons.Clear();
		userStorySlotIndices.Clear();
#if UNITY_WEBGL && !UNITY_EDITOR
		InitFromProvider();
#else
		Init();
#endif
	}

	// Update is called once per frame
	void Update () {
		isScrollViewDisabled = ngScript.errorPanel.activeSelf || ngScript.feedbackPanel.activeSelf || ngScript.newGamePanel.activeSelf || sm.isRollPlaying;
		if (isScrollViewDisabled) {
			t = 1.0f; // reset the lerping properly
			scrollRect.horizontal = false; // disable scroll by mouse
		}
		else {
			scrollRect.horizontal = true; // enable scroll 
		}

		// move to the selected slot when clicked
		if (isLerping) {
			scrollRect.horizontalNormalizedPosition = Mathf.Lerp(oldPos, newPos, t);
			t += lerpSpeed * Time.deltaTime;
			if (t > 1.0f) {
				isLerping = false;
				t = 0.0f;
				selectedButton = selectedButton < 0 ? 0 : selectedButton > slots.Count-1 ? slots.Count-1 : selectedButton;
				Debug.Log("lerp stops");
				updateSlotSprites(selectedButton);
			}
		}
		// move to next or previous slot when axis moved,  if not in movement
		if (!isLerping && scrollRect.horizontal == true) {
			float value = Input.GetAxisRaw ("Horizontal"); 
			if (value < 0 && selectedButton > 0) { // move left
			SetMoveToValues(selectedButton - 1);
			}
			else if  (value > 0 && selectedButton < slots.Count-1) { // move right
			SetMoveToValues(selectedButton + 1);
			}

			if (Input.GetKeyUp(KeyCode.Return)) {
				StartCoroutine(LoadFolder(selectedButton));
			}
			if (Input.GetKey(KeyCode.Escape)) {
				ngScript.OnEscape();
			}
			if (Input.GetKey(KeyCode.Delete)) {
				OnSuppr(selectedButton);
			}
		}
	}

	string[] OrderGameFolder(string[] gameFolders) {
		List<string> orderedGameFoldersList = new List<string>();
		List<string> gameFoldersList = new List<string>(gameFolders);

		// for the 7 original stories
		for (int i = 0; i < 7; i++) {
			bool removeElem = false;
			// for all unordered yet
			int j;
			for (j = 0; j < gameFoldersList.Count; j++) {
				// get the folder name
				string folderName = new DirectoryInfo(gameFoldersList[j]).Name;
				// the full path need to be stored
				if 	  ((i == 0 && folderName == "Rody Et Mastico") 
					|| (i == 1 && folderName == "Rody Et Mastico II") 
					|| (i == 2 && folderName == "Rody Et Mastico III") 
					|| (i == 3 && folderName == "Rody Noël") 
					|| (i == 4 && folderName == "Rody Et Mastico V") 
					|| (i == 5 && folderName == "Rody Et Mastico VI") 
					|| (i == 6 && folderName == "Rody Et Mastico A Ibiza")) {
					orderedGameFoldersList.Add(gameFoldersList[j]);
					removeElem = true;
					//Debug.Log("Add to ordered :" + folderName);
					break;
				}
			}
			if (removeElem) {
				gameFoldersList.RemoveAt(j);
				//Debug.Log("removed");
			}
		}
		
		// Add the rest of the unordered list
		orderedGameFoldersList.AddRange(gameFoldersList);
		
		return orderedGameFoldersList.ToArray();
	}

	void OnValueChanged(Vector2 value) {

		float currentPos = slots[selectedButton].GetComponent<RectTransform>().position.x;
		int index = selectedButton;
		if (currentPos > 2f) 
			index--;
		if (currentPos < -2f)
			index++;
		
		//Debug.Log("current pos : " + currentPos +", index : " + index);

		index =  index <= 0 ? 0 : 
						index >= slotImages.Count ? slotImages.Count - 1 : 
						index; // index starts at 0

		if (index != selectedButton) {
			updateSlotSprites(index);
			if (!isLerping) {
				Debug.Log("new selected is : " + selectedButton);
				sm.OnSlotSelection();
			}
		}

	}

	void updateSlotSprites(int index) {
		for(int i = 0; i < slotImages.Count; i++) {
			if (i == index) {
				slotImages[i].GetComponent<Image>().sprite = selected;
				slotButtons[i].image.rectTransform.sizeDelta = new Vector2(58,80);
				slotTitles[i].SetActive(true);
				selectedButton = index;
				//Debug.Log("new selected is : " + index); 
			}
			else {
				slotImages[i].GetComponent<Image>().sprite = notSelected;
				slotButtons[i].image.rectTransform.sizeDelta = new Vector2(54,72);
				slotTitles[i].SetActive(false);
			}
		}
	}

	void OnClick() {
		if (isScrollViewDisabled)
			return; // don't do anything if scroll view is disabled
		Button me = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
		int index = slotButtons.FindIndex(x => x == me);
		//Debug.Log("move to " + index + ", by step : " + index * step);
		
		if (selectedButton == index) { // the button is focus and clicked
			// set the right game folder
			StartCoroutine(LoadFolder(index));
		}
		else // the button is not focus
			SetMoveToValues(index);
	}

	void SetMoveToValues(int destIndex) {
		newPos = (destIndex * step) + (middleSlot - destIndex) * 2f / 100f;
		//Debug.Log("offset : " + (middleSlot - destIndex) * 2f / 100f);
		//Debug.Log("new pos : " + newPos);
		oldPos = scrollRect.horizontalNormalizedPosition;
		selectedButton = destIndex;
		Debug.Log("new selected is : " + destIndex);
		sm.OnSlotSelection();
		isLerping = true;
	}

	IEnumerator LoadFolder(int index) {
		PlayerPrefs.SetInt("customGame",1);

		string slotName = content.transform.GetChild(index).name;

#if UNITY_WEBGL && !UNITY_EDITOR
		// On WebGL, no newGame/import slots - all slots are stories
		while(menu.pix.BlockCount > 32) {
			menu.pix.enabled = true;
			menu.pix.BlockCount -= (menu.pixAcceleration * menu.pix.BlockCount / 100);
			Debug.Log(menu.pix.BlockCount);
			yield return new WaitForEndOfFrame();
		}
		Debug.Log("[RA] Set game path : " + getGamePath(index));
		PlayerPrefs.SetString("gamePath", getGamePath(index));
		SceneManager.LoadScene(1); // load the intro scene
#else
		// new empty game
		if (slotName == "newGame") {
			newGamePanel.SetActive(true);
		}
		// import a .rody.json file
		else if (slotName == "importStory") {
			ngScript.OnImportClick();
		}
		// Load the selected game (official or user)
		else {
			while(menu.pix.BlockCount > 32) {
				menu.pix.enabled = true;
				menu.pix.BlockCount -= (menu.pixAcceleration * menu.pix.BlockCount / 100);
				Debug.Log(menu.pix.BlockCount);
				yield return new WaitForEndOfFrame();
			}
			string gamePath = GetSlotPath(index);
			Debug.Log("[RA] Set game path : " + gamePath);
			PlayerPrefs.SetString("gamePath", gamePath);
			SceneManager.LoadScene(1); // load the intro scene
		}
#endif

		yield return null;
	}

	string getGamePath(int index) {
#if UNITY_WEBGL && !UNITY_EDITOR
		// On WebGL, just return the story ID (slot name) - Firebase provider uses IDs
		return content.transform.GetChild(index).name;
#else
		return System.IO.Path.Combine(Application.streamingAssetsPath, content.transform.GetChild(index).name);
#endif
	}

	bool isGameFolder(string folderPath) {
		return (File.Exists(folderPath + "/Sprites/cover.png")
			&& File.Exists(folderPath + "/credits.txt")
			&& File.Exists(folderPath + "/levels.rody")
			&& Directory.Exists(folderPath + "/Sprites"));
	}

	public void OnSuppr(int index) {
		string gameName = content.transform.GetChild(index).name;
		bool isDeletable = false;

		// JSON stories are deletable (delete the file)
		if (gameName.StartsWith("json:"))
		{
			string jsonPath = gameName.Substring(5); // Remove "json:" prefix
			PlayerPrefs.SetString("gameToDelete", jsonPath);
			PlayerPrefs.SetString("gameToDeleteType", "json");
			isDeletable = true;
		}
		// User folder stories are deletable
		else if (gameName.StartsWith("user:"))
		{
			string folderName = gameName.Substring(5); // Remove "user:" prefix
			string fullPath = Path.Combine(PathManager.UserStoriesPath, folderName);
			PlayerPrefs.SetString("gameToDelete", fullPath);
			PlayerPrefs.SetString("gameToDeleteType", "folder");
			isDeletable = true;
		}
		// Official stories and special slots are not deletable
		else
		{
			switch (gameName)
			{
				case "Rody Et Mastico" :
				case "Rody Et Mastico II" :
				case "Rody Et Mastico III" :
				case "Rody Noël" :
				case "Rody Et Mastico V" :
				case "Rody Et Mastico VI" :
				case "Rody Et Mastico A Ibiza" :
				case "newGame" :
				case "importStory" :
					isDeletable = false;
					break;
				default:
					// Other non-user stories (if any)
					PlayerPrefs.SetString("gameToDelete", gameName);
					PlayerPrefs.SetString("gameToDeleteType", "folder");
					isDeletable = true;
					break;
			}
		}
		ngScript.SG_onDelete(isDeletable);
	}

	/// <summary>
	/// Gets the currently selected slot's path for export.
	/// Returns null if not a user story (folder-based only, not JSON).
	/// JSON stories don't need re-export - they're already in the right format.
	/// </summary>
	public string GetSelectedUserStoryPath()
	{
		if (selectedButton < 0 || selectedButton >= slots.Count)
			return null;

		string slotName = content.transform.GetChild(selectedButton).name;

		// JSON stories can't be re-exported (they're already JSON)
		if (slotName.StartsWith("json:"))
			return null;

		if (!slotName.StartsWith("user:"))
			return null;

		string folderName = slotName.Substring(5);
		return Path.Combine(PathManager.UserStoriesPath, folderName);
	}

	/// <summary>
	/// Gets the currently selected slot index.
	/// </summary>
	public int GetSelectedIndex()
	{
		return selectedButton;
	}
	
}
