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
	public Transform slotLoadGamePrefab;

	[Header("WebGL")]
	public GameObject loadingUI;

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
		if (loadingUI != null) loadingUI.SetActive(true);

		// Wait for provider to be ready
		if (!StoryProviderManager.IsReady)
		{
			StoryProviderManager.Initialize();
			while (!StoryProviderManager.IsReady)
			{
				yield return null;
			}
		}

		if (loadingUI != null) loadingUI.SetActive(false);
		InitFromProvider();
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
			// SLOT LOAD GAME
			GameObject loadGameSlot = Instantiate(slotLoadGamePrefab, content.transform).gameObject;
			loadGameSlot.name = "loadGame";
			slots.Add(loadGameSlot);
			// SLOT NEW GAME
			GameObject newGameSlot = Instantiate(slotNewGamePrefab, content.transform).gameObject;
			newGameSlot.name = "newGame";
			slots.Add(newGameSlot);
			slotCount += 2;
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

		step = 1.0f / (slots.Count - 1);
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
		int slotIndex = 0;
		string[] gameFolders = Directory.GetDirectories(Application.streamingAssetsPath);
		string[] orderedGameFolders = OrderGameFolder(gameFolders);

		// SLOT OF THE GAMES IN streamingAssetsPath
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
				// load the cover
				slot.transform.GetChild(0).GetComponent<Image>().sprite = RM_SaveLoad.LoadSprite(folder + "/cover.png", 0, 100, 137);
				slot.name = lastFolderName;
				// game's title
				slot.GetComponentInChildren<Text>().text = lastFolderName;
				
				// add the slot to slots
				slots.Add(slot);
				
				slotIndex++;
			}
		}

		// SLOT LOAD GAME
		GameObject loadGameSlot = Instantiate(slotLoadGamePrefab, content.transform).gameObject;
		loadGameSlot.name = "loadGame"; // simplify the access;
		slots.Add(loadGameSlot);
		// SLOT NEW GAME
		GameObject newGameSlot = Instantiate(slotNewGamePrefab, content.transform).gameObject;
		newGameSlot.name = "newGame"; // simplify the access;
		slots.Add(newGameSlot);
		
		// set the size of the content relative to the slot count to have a fluid scroll
		content.GetComponent<RectTransform>().sizeDelta = new Vector2((slotIndex+2) * 100, 100);
		// populate the lists
		slotImages  = new List<Image>();
		slotButtons = new List<Button>();
		for(int i = 0; i < slots.Count; i++) {
			slotImages.Add(slots[i].GetComponent<Image>());
			slots[i].GetComponent<Button>().onClick.AddListener(OnClick);
			slotButtons.Add(slots[i].GetComponent<Button>());
			GameObject title = slots[i].transform.Find("Title").gameObject;
			slotTitles.Add(title);
			title.SetActive(false);
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
		slotTitles[selectedButton].SetActive(true);
	}

	public void Reset() {
		foreach (GameObject slot in slots) {
			GameObject.Destroy(slot);
		}
		slots.Clear();
		slotTitles.Clear();
		slotImages.Clear();
		slotButtons.Clear();
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

#if UNITY_WEBGL && !UNITY_EDITOR
		// On WebGL, no loadGame/newGame slots - all slots are stories
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
		if (index == content.transform.childCount - 1) {
			newGamePanel.SetActive(true);
		}
		// load a game folder
		else if (index == content.transform.childCount - 2) {
			ngScript.IG_OnUploadClick();
		}
		// Load the selected game
		else {
			while(menu.pix.BlockCount > 32) {
				menu.pix.enabled = true;
				menu.pix.BlockCount -= (menu.pixAcceleration * menu.pix.BlockCount / 100);
				Debug.Log(menu.pix.BlockCount);
				yield return new WaitForEndOfFrame();
			}
			Debug.Log("[RA] Set game path : " + getGamePath(index));
			PlayerPrefs.SetString("gamePath", getGamePath(index));
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
		return (File.Exists(folderPath + "/cover.png")
			&& File.Exists(folderPath + "/credits.txt")
			&& File.Exists(folderPath + "/levels.rody")
			&& Directory.Exists(folderPath + "/Sprites"));
	}

	public void OnSuppr(int index) {
		string gameName = content.transform.GetChild(index).name;
		bool isDeletable = false;
		switch (gameName)
		{
			case "Rody Et Mastico" :
			case "Rody Et Mastico II" :
			case "Rody Et Mastico III" :
			case "Rody Noël" :
			case "Rody Et Mastico V" :
			case "Rody Et Mastico VI" :
			case "Rody Et Mastico A Ibiza" :
			case "loadGame" :
			case "newGame" :
				isDeletable = false;
				break;
			default:
				PlayerPrefs.SetString("gameToDelete", gameName);
				isDeletable = true;
				break;
		}
		ngScript.SG_onDelete(isDeletable);
	}
	
}
