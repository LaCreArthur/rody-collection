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
		// Always use provider-based initialization for official stories
		StartCoroutine(InitWithProvider());
	}

	/// <summary>
	/// Unified initialization - waits for provider, then loads stories.
	/// Official stories come from Resources via provider.
	/// On desktop: also loads user stories and shows import/newGame slots.
	/// </summary>
	IEnumerator InitWithProvider()
	{
		Debug.Log("[RA_ScrollView] InitWithProvider() started");
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
		Debug.Log("[RA_ScrollView] Loading stories from provider...");
		InitFromProvider();
		Debug.Log("[RA_ScrollView] InitWithProvider() complete");
	}

	/// <summary>
	/// Initialize from StoryProvider.
	/// Official stories come from Resources via provider.
	/// On desktop: also loads user stories and shows import/newGame slots.
	/// </summary>
	void InitFromProvider()
	{
		PlayerPrefs.SetInt("rodyMakerFirstTime", 1);
		slots = new List<GameObject>();
		slotTitles = new List<GameObject>();
		userStorySlotIndices.Clear();
		int slotIndex = 0;

		// Load official stories from provider
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

			// Load cover from provider
			LoadCover(slot, story.id);

			slots.Add(slot);
			slotIndex++;
		}

		// On desktop: also load user stories and show import/newGame slots
		bool hasFileSystem = Bootstrap.HasFileSystem;
		if (hasFileSystem)
		{
			LoadUserStories(ref slotIndex);

			// SLOT IMPORT (import .rody.json) - reuses the old "Load Game" prefab
			GameObject importSlot = Instantiate(slotLoadGamePrefab, content.transform).gameObject;
			importSlot.name = "importStory";
			slots.Add(importSlot);
			slotIndex++;
		}

		FinalizeSlots(slotIndex, hasFileSystem);
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

	void LoadCover(GameObject slot, string storyId)
	{
		var provider = StoryProviderManager.Provider;
		if (provider != null)
		{
			var sprite = provider.LoadSprite(storyId, "cover.png", 320, 240);
			if (slot != null && sprite != null)
			{
				var img = slot.transform.GetChild(0).GetComponent<Image>();
				if (img != null) img.sprite = sprite;
			}
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

	/// <summary>
	/// Loads user stories from UserStoriesPath (.rody.json files only).
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

		// Load .rody.json files only
		string[] jsonFiles = Directory.GetFiles(userStoriesPath, "*.rody.json");
		Debug.Log($"[RA_ScrollView] Found {jsonFiles.Length} .rody.json files");

		foreach (string jsonPath in jsonFiles)
		{
			try
			{
				// Read JSON and parse metadata
				string json = File.ReadAllText(jsonPath);
				var story = Newtonsoft.Json.JsonConvert.DeserializeObject<StoryExporter.ExportedStory>(json);

				if (story?.story != null)
				{
					// Instantiate a slot
					GameObject slot = Instantiate(slotPrefab, content.transform).gameObject;

					// Try to load cover from the JSON sprites
					if (story.sprites != null && story.sprites.ContainsKey("cover.png"))
					{
						try
						{
							string base64 = story.sprites["cover.png"];
							if (base64.StartsWith("data:"))
							{
								int comma = base64.IndexOf(',');
								if (comma > 0) base64 = base64.Substring(comma + 1);
							}
							byte[] bytes = System.Convert.FromBase64String(base64);
							var tex = new Texture2D(100, 137, TextureFormat.RGBA32, false);
							tex.filterMode = FilterMode.Point;
							tex.LoadImage(bytes);
							var coverSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
							slot.transform.GetChild(0).GetComponent<Image>().sprite = coverSprite;
						}
						catch (System.Exception e)
						{
							Debug.LogWarning($"[RA_ScrollView] Failed to load cover for {jsonPath}: {e.Message}");
						}
					}

					slot.name = "json:" + jsonPath; // Full path for JSON files
					slot.GetComponentInChildren<Text>().text = story.story.title;

					slots.Add(slot);
					userStorySlotIndices.Add(slots.Count - 1);

					slotIndex++;
					Debug.Log($"[RA_ScrollView] Added JSON story: {story.story.title}");
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


	public void Reset() {
		foreach (GameObject slot in slots) {
			GameObject.Destroy(slot);
		}
		slots.Clear();
		slotTitles.Clear();
		slotImages.Clear();
		slotButtons.Clear();
		userStorySlotIndices.Clear();
		InitFromProvider();
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

		// new empty game (desktop only)
		if (slotName == "newGame") {
			newGamePanel.SetActive(true);
		}
		// import a .rody.json file (desktop only)
		else if (slotName == "importStory") {
			ngScript.OnImportClick();
		}
		// Load the selected game (official or user)
		else {
			// Animate pixelation transition before loading
			yield return StartCoroutine(menu.AnimateExitTransition());

			// Load story into WorkingStory BEFORE scene transition
			if (slotName.StartsWith("json:"))
			{
				// JSON user story - load from file
				string jsonPath = slotName.Substring(5);
				try
				{
					string json = File.ReadAllText(jsonPath);
					WorkingStory.LoadFromJson(json, jsonPath);
					PlayerPrefs.SetInt("scenesCount", WorkingStory.SceneCount);
					Debug.Log($"[RA] Loaded JSON story: {WorkingStory.Title}");
				}
				catch (System.Exception e)
				{
					Debug.LogError($"[RA] Failed to load JSON story: {e.Message}");
					yield break;
				}
			}
			else
			{
				// Official story - load from Resources via WorkingStory
				WorkingStory.LoadOfficial(slotName);
				PlayerPrefs.SetInt("scenesCount", WorkingStory.SceneCount);
				Debug.Log($"[RA] Loaded official story: {WorkingStory.Title}");
			}

			string gamePath = slotName; // Just use slot name directly
			Debug.Log("[RA] Set game path : " + gamePath);
			PlayerPrefs.SetString("gamePath", gamePath);
			SceneManager.LoadScene(1); // load the intro scene
		}

		yield return null;
	}


	public void OnSuppr(int index) {
		string gameName = content.transform.GetChild(index).name;
		bool isDeletable = false;

		// JSON user stories are deletable
		if (gameName.StartsWith("json:"))
		{
			string jsonPath = gameName.Substring(5); // Remove "json:" prefix
			PlayerPrefs.SetString("gameToDelete", jsonPath);
			PlayerPrefs.SetString("gameToDeleteType", "json");
			isDeletable = true;
		}
		// Official stories and special slots are not deletable

		ngScript.SG_onDelete(isDeletable);
	}

	/// <summary>
	/// Gets the currently selected slot's JSON path for export.
	/// Returns null if not a JSON user story.
	/// </summary>
	public string GetSelectedUserStoryPath()
	{
		if (selectedButton < 0 || selectedButton >= slots.Count)
			return null;

		string slotName = content.transform.GetChild(selectedButton).name;

		// Only JSON user stories have exportable paths
		if (slotName.StartsWith("json:"))
			return slotName.Substring(5); // Return path without prefix

		return null;
	}

	/// <summary>
	/// Gets the currently selected slot index.
	/// </summary>
	public int GetSelectedIndex()
	{
		return selectedButton;
	}
	
}
