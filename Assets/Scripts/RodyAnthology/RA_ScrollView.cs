using System.Collections;
using System.Collections.Generic;
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
	public RA_ActionPanel actionPanel;
	[SerializeField] RA_FeedbackPanel feedbackPanel;

	[Header("WebGL")]
	public GameObject loadingUI;

	static ScrollRect scrollRect;
	static float t = 0.0f;

	List<GameObject> slots;
	List<GameObject> slotTitles;
	List<Image> slotImages;
	List<Button> slotButtons;
	List<StoryCard> cards;            // parallel to slots
	readonly SpriteCache _covers = new SpriteCache();

	float step;
	float newPos;
	float oldPos;
	bool isLerping = false;
	int selectedButton, middleSlot;
	public RA_NewGame ngScript;
	bool isScrollViewDisabled = true;

	void Start () {
		// First-launch editor hint: set once, never re-set on later menu visits.
		if (!PlayerPrefs.HasKey("rodyMakerFirstTime"))
			PlayerPrefs.SetInt("rodyMakerFirstTime", 1);

		if (loadingUI != null) loadingUI.SetActive(false);
		BuildSlots();
	}

	void OnEnable()
	{
		RA_ActionPanel.OnEditClicked += HandleEditClicked;
		RA_ActionPanel.OnExportClicked += HandleExportClicked;
		RA_ActionPanel.OnImportClicked += HandleImportClicked;
		RA_ActionPanel.OnNewClicked += HandleNewClicked;
	}

	void OnDisable()
	{
		RA_ActionPanel.OnEditClicked -= HandleEditClicked;
		RA_ActionPanel.OnExportClicked -= HandleExportClicked;
		RA_ActionPanel.OnImportClicked -= HandleImportClicked;
		RA_ActionPanel.OnNewClicked -= HandleNewClicked;
	}

	/// <summary>
	/// Builds one carousel slot per catalog card (built-in then user, in catalog order).
	/// </summary>
	void BuildSlots()
	{
		slots = new List<GameObject>();
		slotTitles = new List<GameObject>();
		cards = StoryRoot.Catalog.Cards();

		foreach (var card in cards)
		{
			GameObject slot = Instantiate(slotPrefab, content.transform).gameObject;
			slot.name = card.id;
			slot.GetComponentInChildren<Text>().text = card.title;
			PaintCover(slot, card);
			slots.Add(slot);
		}

		FinalizeSlots(slots.Count);
	}

	void PaintCover(GameObject slot, StoryCard card)
	{
		var img = slot.transform.GetChild(0).GetComponent<Image>();
		if (img == null) return;
		var sprite = _covers.Get(card.id, card.cover, 320, 200);
		if (sprite != null) img.sprite = sprite;
	}

	void FinalizeSlots(int slotCount)
	{
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
		scrollRect = GetComponent<ScrollRect>();
		scrollRect.onValueChanged.AddListener(OnValueChanged);
		middleSlot = slots.Count / 2;
		selectedButton = 0; // select the first game of the list on launch
		scrollRect.horizontalNormalizedPosition = (selectedButton * step) + (middleSlot - selectedButton) * 2f / 100f;
		slotImages[selectedButton].GetComponent<Image>().sprite = selected;
		slotTitles[selectedButton].SetActive(true);
		UpdateActionPanel();
	}

	StoryCard SelectedCard()
	{
		if (cards == null || selectedButton < 0 || selectedButton >= cards.Count)
			return null;
		return cards[selectedButton];
	}

	public void Reset() {
		foreach (GameObject slot in slots) {
			GameObject.Destroy(slot);
		}
		slots.Clear();
		slotTitles.Clear();
		slotImages.Clear();
		slotButtons.Clear();
		BuildSlots();
	}

	/// <summary>Rebuilds the carousel and scrolls to the given story id.</summary>
	public void ResetAndSelectStory(string id)
	{
		Reset();
		StartCoroutine(ScrollToSlotByName(id));
	}

	IEnumerator ScrollToSlotByName(string slotName)
	{
		yield return null;
		Canvas.ForceUpdateCanvases();

		int index = -1;
		for (int i = 0; i < slots.Count; i++)
		{
			if (content.transform.GetChild(i).name == slotName)
			{
				index = i;
				break;
			}
		}

		if (index < 0)
			yield break;

		if (selectedButton == index)
		{
			float targetPos = (index * step) + (middleSlot - index) * 2f / 100f;
			scrollRect.horizontalNormalizedPosition = targetPos;
			updateSlotSprites(index);
			yield break;
		}

		SetMoveToValues(index);
	}

	// Update is called once per frame
	void Update () {
		isScrollViewDisabled = feedbackPanel.gameObject.activeSelf || newGamePanel.activeSelf || sm.isRollPlaying;
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
				updateSlotSprites(selectedButton);
			}
		}
		// move to next or previous slot when axis moved, if not in movement
		if (!isLerping && scrollRect.horizontal == true) {
			float value = Input.GetAxisRaw ("Horizontal");
			if (value < 0 && selectedButton > 0) { // move left
				SetMoveToValues(selectedButton - 1);
			}
			else if  (value > 0 && selectedButton < slots.Count-1) { // move right
				SetMoveToValues(selectedButton + 1);
			}

			// Delete the selected story (user stories only).
			if (Input.GetKeyUp(KeyCode.Delete)) {
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

		index =  index <= 0 ? 0 :
						index >= slotImages.Count ? slotImages.Count - 1 :
						index; // index starts at 0

		if (index != selectedButton) {
			updateSlotSprites(index);
			if (!isLerping) {
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
			}
			else {
				slotImages[i].GetComponent<Image>().sprite = notSelected;
				slotButtons[i].image.rectTransform.sizeDelta = new Vector2(54,72);
				slotTitles[i].SetActive(false);
			}
		}
		UpdateActionPanel();
	}

	void UpdateActionPanel()
	{
		var card = SelectedCard();
		if (card != null)
			actionPanel.Show(card.source == StorySource.User);
		else
			actionPanel.Hide();
	}

	void HandleEditClicked()
	{
		if (isScrollViewDisabled) return;
		var card = SelectedCard();
		if (card == null) return;

		StoryRoot.Session.Load(StoryRoot.Catalog.Resolve(card.id), card.source);
		// Editing a built-in transparently produces an editable user copy.
		StoryRoot.Session.ForkForEditing();
		StoryRoot.Session.CurrentSceneIndex = 0;
		StartCoroutine(TransitionToEditor());
	}

	IEnumerator TransitionToEditor()
	{
		yield return StartCoroutine(menu.AnimateExitTransition());
		SceneManager.LoadScene(AppScenes.Editor);
	}

	void HandleExportClicked()
	{
		if (isScrollViewDisabled) return;
		var card = SelectedCard();
		if (card == null || card.source != StorySource.User) return;

		StoryRoot.Session.Load(StoryRoot.Catalog.Resolve(card.id), card.source);
		ngScript.OnExportClick();
	}

	void HandleImportClicked()
	{
		if (isScrollViewDisabled) return;
		ngScript.OnImportClick();
	}

	void HandleNewClicked()
	{
		if (isScrollViewDisabled) return;
		newGamePanel.SetActive(true);
	}

	void OnClick() {
		if (isScrollViewDisabled)
			return; // don't do anything if scroll view is disabled
		Button me = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
		int index = slotButtons.FindIndex(x => x == me);

		if (selectedButton == index) { // the button is focused and clicked
			StartCoroutine(PlaySelected(index));
		}
		else // the button is not focused
			SetMoveToValues(index);
	}

	void SetMoveToValues(int destIndex) {
		newPos = (destIndex * step) + (middleSlot - destIndex) * 2f / 100f;
		oldPos = scrollRect.horizontalNormalizedPosition;
		selectedButton = destIndex;
		sm.OnSlotSelection();
		isLerping = true;
	}

	/// <summary>Loads the selected card into the session and starts play (title scene).</summary>
	IEnumerator PlaySelected(int index) {
		if (index < 0 || index >= cards.Count) yield break;

		var card = cards[index];
		yield return StartCoroutine(menu.AnimateExitTransition());

		StoryRoot.Session.Load(StoryRoot.Catalog.Resolve(card.id), card.source);
		if (!StoryRoot.Session.IsLoaded) yield break;

		SceneManager.LoadScene(AppScenes.Title);
	}

	/// <summary>Delete the selected story (user stories only).</summary>
	public void OnSuppr(int index) {
		if (index < 0 || index >= cards.Count) return;
		var card = cards[index];
		bool isDeletable = card.source == StorySource.User;
		if (isDeletable)
			ngScript.SG_onDelete(card.id);
		else
			ngScript.SG_onDelete(null);
	}
}
