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
		RA_ActionPanel.OnSaveClicked += HandleSaveClicked;
		StoryRoot.StateChanged += RefreshWorkspace;
		RA_ActionPanel.OnImportClicked += HandleImportClicked;
		RA_ActionPanel.OnNewClicked += HandleNewClicked;
	}

	void OnDisable()
	{
		RA_ActionPanel.OnEditClicked -= HandleEditClicked;
		RA_ActionPanel.OnSaveClicked -= HandleSaveClicked;
		StoryRoot.StateChanged -= RefreshWorkspace;
		RA_ActionPanel.OnImportClicked -= HandleImportClicked;
		RA_ActionPanel.OnNewClicked -= HandleNewClicked;
	}

	/// <summary>
	/// Builds the immutable originals plus the one personal workspace slot.
	/// </summary>
	void BuildSlots()
	{
		slots = new List<GameObject>();
		slotTitles = new List<GameObject>();
		cards = new List<StoryCard>(StoryRoot.Catalog.Cards());
		cards.Add(WorkspaceCard());

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
		var sprite = _covers.Get(card.source == StorySource.User ? "workspace" : "builtin/" + card.id, card.cover, 320, 200);
		img.sprite = sprite != null ? sprite : slotPrefab.GetChild(0).GetComponent<Image>().sprite;
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

    StoryCard WorkspaceCard()
    {
        var draft = StoryRoot.Session.Draft;
        string cover = null;
        draft?.sprites.TryGetValue(SpriteCache.CoverName, out cover);
        return new StoryCard
        {
            id = "Mon histoire",
            source = StorySource.User,
            title = !StoryRoot.IsReady ? "Mon histoire · récupération…" : draft == null ? "Mon histoire · à créer" :
                "Mon histoire · " + draft.story.title + (StoryRoot.Session.IsDirty ? "\nModifications non enregistrées" : ""),
            cover = cover,
            sceneCount = draft?.scenes.Count ?? 0
        };
    }

    void RefreshWorkspace()
    {
        if (cards == null) return;
        int index = cards.Count - 1;
        var next = WorkspaceCard();
        if (cards[index].cover != next.cover)
        {
            _covers.Evict("workspace");
            PaintCover(slots[index], next);
        }
        cards[index] = next;
        slotTitles[index].GetComponent<Text>().text = next.title;
        UpdateActionPanel();
    }

    void OnDestroy() => _covers.Clear();

	// Update is called once per frame
	void Update () {
		if (scrollRect == null) return;
        bool disabled = StoryRoot.IsBusy || newGamePanel.activeSelf || sm.isRollPlaying;
        if (disabled != isScrollViewDisabled)
        {
            isScrollViewDisabled = disabled;
            UpdateActionPanel();
        }
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
			actionPanel.Show(card.source == StorySource.User, StoryRoot.Session.HasWorkspace, StoryRoot.IsReady, isScrollViewDisabled);
		else
			actionPanel.Hide();
	}

	void HandleEditClicked()
	{
		if (isScrollViewDisabled) return;
		var card = SelectedCard();
		if (card == null) return;

        if (card.source == StorySource.User) StoryRoot.EditWorkspace();
        else
        {
            try { StoryRoot.RequestWorkspace(StorySession.Duplicate(StoryRoot.Catalog.Resolve(card.id))); }
            catch (System.Exception e) { StoryRoot.ShowMessage(e.Message); }
        }
    }

    void HandleSaveClicked()
    {
        if (isScrollViewDisabled || SelectedCard()?.source != StorySource.User) return;
        StoryRoot.SaveWorkspace();
    }

	void HandleImportClicked()
	{
		if (isScrollViewDisabled) return;
		StoryRoot.ImportWorkspace();
	}

	void HandleNewClicked()
	{
		if (isScrollViewDisabled || !StoryRoot.IsReady) return;
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
        if (card.source == StorySource.User)
        {
            if (!StoryRoot.IsReady) yield break;
            if (!StoryRoot.Session.HasWorkspace) { newGamePanel.SetActive(true); yield break; }
            StoryRoot.Session.ActivateWorkspace();
        }
        else
        {
            Story story = null;
            try { story = StoryRoot.Catalog.Resolve(card.id); }
            catch (System.Exception e) { StoryRoot.ShowMessage(e.Message); }
            if (story == null) yield break;
            StoryRoot.Session.LoadBuiltin(story);
        }
        StoryRoot.Session.CurrentSceneIndex = 1;
        yield return StartCoroutine(menu.AnimateExitTransition());
        SceneManager.LoadScene(AppScenes.Title);
    }
}
