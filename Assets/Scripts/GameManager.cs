using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public SceneLoading loading;
	public Intro intro;
	public Scene scene;
	public AudioSource source;
	public SoundManager sm;
	public Animator MasticoAnimator;
	public GameObject p_layout;
	public GameObject p_title;
	public GameObject p_text;
	public GameObject b_next;
	public GameObject b_draw;
	public GameObject b_repeat;
	public GameObject b_ngp;
	public GameObject b_fsw;
	public GameObject b_mastico;
	public GameObject objNearTemplate, objTemplate;
	public SceneAnimator sceneAnimator;
	[HideInInspector]
	public List<GameObject> obj, objNear, ngp, ngpNear, fsw, fswNear;
	[HideInInspector]
	public bool introOver,objOver,ngpOver,fswOver,interObj,clickIntro = false,clickObj = false;
	[HideInInspector]
	public AudioClip[] currentFx;
	[HideInInspector]
	public List<Sprite> sceneSprites;
	[HideInInspector]
	public string currentDial,currentText,introDial1,introDial2,introDial3,objDial,ngpDial,fswDial,titleText,introText,objText,ngpText,fswText;
	[HideInInspector]
	public int currentScene;

	void Start()
	{
		currentScene = WorkingStory.CurrentSceneIndex;

		if (!WorkingStory.IsLoaded)
		{
			Debug.LogError("[GameManager] WorkingStory not loaded - returning to menu");
			SceneManager.LoadScene(0);
			return;
		}

		if (currentScene > WorkingStory.SceneCount)
		{
			Debug.Log("[GameManager] Scene index exceeds story length - going to win scene");
			SceneManager.LoadScene(5);
			return;
		}

		StartCoroutine(InitFromWorkingStory(currentScene));
	}

	/// <summary>
	/// Initializes scene from WorkingStory (in-memory story data).
	/// </summary>
	IEnumerator InitFromWorkingStory(int sceneIndex)
	{
		yield return null; // Let UI update

		// Load scene data from WorkingStory
		SceneData sceneData = WorkingStory.LoadScene(sceneIndex);
		List<Sprite> loadedSprites = WorkingStory.LoadSceneSprites(sceneIndex);

		if (sceneData == null || loadedSprites == null || loadedSprites.Count == 0)
		{
			Debug.LogError($"[GameManager] Failed to load scene {sceneIndex} from WorkingStory");
			SceneManager.LoadScene(2);
			yield break;
		}

		// Apply scene data
		ApplySceneDataFromModel(sceneData);
		sceneSprites = loadedSprites;

		Debug.Log($"[GameManager] Loaded scene {sceneIndex} from WorkingStory ({sceneSprites.Count} sprites)");

		sceneAnimator.baseFrame = sceneSprites[0];
		sceneAnimator.frames = new List<Sprite>(sceneSprites);
		sceneAnimator.frames.RemoveAt(0);

		// Calculate dial counts
		if (!sm.isMastico1) sceneAnimator.sumDial++;
		if (getDial(2).Count > 0 && !sm.isMastico2) sceneAnimator.sumDial++;
		if (getDial(6).Count > 0 && !sm.isMastico3) sceneAnimator.sumDial++;
		sceneAnimator.firstDial = (!sm.isMastico1) ? 1 : (!sm.isMastico2) ? 2 : (!sm.isMastico3) ? 3 : -1;

		// init variables
		introOver = false;
		objOver = false;

		// start the sequence of events
		StartCoroutine(Play());
	}

	/// <summary>
	/// Applies scene data from SceneData model (used by WebGL path).
	/// </summary>
	void ApplySceneDataFromModel(SceneData data)
	{
		// Dialogues
		introDial1 = data.dialogues?.intro1 ?? ".";
		introDial2 = data.dialogues?.intro2 ?? ".";
		introDial3 = data.dialogues?.intro3 ?? ".";
		objDial = data.dialogues?.obj ?? ".";
		ngpDial = data.dialogues?.ngp ?? ".";
		fswDial = data.dialogues?.fsw ?? ".";

		// Texts
		titleText = data.texts?.title ?? "";
		// Combine intro texts for gameplay display
		introText = CombineIntroTexts(
			data.texts?.intro1 ?? "",
			data.texts?.intro2 ?? "",
			data.texts?.intro3 ?? ""
		);
		objText = data.texts?.obj ?? "";
		ngpText = data.texts?.ngp ?? "";
		fswText = data.texts?.fsw ?? "";

		// Music - set audio clips directly
		source.clip = sm.getMusic(data.music?.introMusic ?? "i1");
		intro.sceneMusic.clip = sm.getMusic(data.music?.sceneMusic ?? "l1");

		// Voice
		sm.pitch1 = data.voice?.pitch1 ?? 1f;
		sm.pitch2 = data.voice?.pitch2 ?? 1f;
		sm.pitch3 = data.voice?.pitch3 ?? 1f;
		sm.isMastico1 = data.voice?.isMastico1 ?? false;
		sm.isMastico2 = data.voice?.isMastico2 ?? false;
		sm.isMastico3 = data.voice?.isMastico3 ?? false;
		sm.isZambla = data.voice?.isZambla ?? false;

		// Objects - create clickable zones from typed SceneData
		if (data.objects != null)
		{
			// Create near zones first (they are parents for the target zones)
			objNear = CreateZoneList("objNear", data.objects.obj, true);
			ngpNear = CreateZoneList("ngpNear", data.objects.ngp, true);
			fswNear = CreateZoneList("fswNear", data.objects.fsw, true);

			// Create target zones
			obj = CreateZoneList("obj", data.objects.obj, false);
			ngp = CreateZoneList("ngp", data.objects.ngp, false);
			fsw = CreateZoneList("fsw", data.objects.fsw, false);

			// Hide all zones initially
			List<List<GameObject>> zonesList = new List<List<GameObject>> { objNear, ngpNear, fswNear };
			foreach (List<GameObject> zones in zonesList)
				foreach (GameObject zone in zones)
					zone.SetActive(false);

			objNearTemplate.SetActive(false);
			objTemplate.SetActive(false);
		}
		else
		{
			// Initialize empty lists if no object data
			objNear = new List<GameObject>();
			ngpNear = new List<GameObject>();
			fswNear = new List<GameObject>();
			obj = new List<GameObject>();
			ngp = new List<GameObject>();
			fsw = new List<GameObject>();
		}
	}

	IEnumerator Play() {
		// load the scene assets with lag
		loading.Play();
		//Debug.Log(loading.isPlaying);
		while (loading.isPlaying) {
			yield return null;
		}
		Debug.Log("loading done!");
		//Debug.Log(loading.isPlaying);
		// play intro music and dials
		intro.Play ();
		while (intro.isPlaying) {
			yield return null;
		}
		Debug.Log("intro done!");
		// wait for click in intro

		while (!introOver) {
			yield return null;
		}
		// active the object interactable zone
		// obj.SetActive(true);
		objNear[0].SetActive(true);
		obj[0].SetActive(true);
		Debug.Log("isInit set to false");
		scene.isInit = false;
		scene.Play(1);
		// wait for click on ngp or next
		while (!objOver) {
			scene.initStep(1);
			yield return null;
		}
		// ngp.SetActive(true);
		ngpNear[0].SetActive(true);
		ngp[0].SetActive(true);
		Debug.Log("isInit set to false");
		scene.isInit = false;
		scene.Play(2);
		while (!ngpOver) {
			scene.initStep(2);
			yield return null;
		}
		// fsw.SetActive(true);
		fswNear[0].SetActive(true);
		fsw[0].SetActive(true);
		Debug.Log("isInit set to false");
		scene.isInit = false;
		scene.Play(3);
		while (!fswOver) {
			scene.initStep(3);
			yield return null;
		}
	}

	public List<int> getDial(int dial) {
		switch(dial) {
			case 1: return sm.StringToPhonemes(introDial1);
			case 2: return sm.StringToPhonemes(introDial2);
			case 3: return sm.StringToPhonemes(objDial);
			case 4: return sm.StringToPhonemes(ngpDial);
			case 5: return sm.StringToPhonemes(fswDial);
			case 6: return sm.StringToPhonemes(introDial3);
			default: return sm.StringToPhonemes("g_et_t_d_i_a_l g_l_i_t_ch");
		}
	}

	void Update() {
		if (Input.GetKeyUp(KeyCode.Escape)){
			SceneManager.LoadScene(2);
		}
	}

	/// <summary>
	/// Combines intro texts for gameplay display.
	/// </summary>
	string CombineIntroTexts(string intro1, string intro2, string intro3)
	{
		var parts = new List<string>();
		if (!string.IsNullOrEmpty(intro1)) parts.Add(intro1);
		if (!string.IsNullOrEmpty(intro2)) parts.Add(intro2);
		if (!string.IsNullOrEmpty(intro3)) parts.Add(intro3);
		return string.Join("\n", parts);
	}

	/// <summary>
	/// Creates a list of zone GameObjects from typed ObjectZone data.
	/// Each object has exactly one target zone and one near zone.
	/// </summary>
	/// <param name="name">Base name for the zone (e.g., "obj", "ngp", "fsw")</param>
	/// <param name="zone">The typed ObjectZone data with position/size floats</param>
	/// <param name="isNear">True to create the near zone, false for target zone</param>
	/// <returns>List containing the single zone GameObject</returns>
	List<GameObject> CreateZoneList(string name, ObjectZone zone, bool isNear)
	{
		var result = new List<GameObject>();

		if (zone == null)
			return result;

		GameObject parent, obj;
		float x, y, width, height;

		if (isNear)
		{
			// Near zone - parent is "Objects", clone from objNearTemplate
			parent = GameObject.Find("Objects");
			obj = Instantiate(objNearTemplate, parent.GetComponent<RectTransform>());
			x = zone.nearX;
			y = zone.nearY;
			width = zone.nearWidth;
			height = zone.nearHeight;
		}
		else
		{
			// Target zone - parent is the corresponding near zone
			parent = GameObject.Find(name + "Near0");
			obj = Instantiate(objTemplate, parent.GetComponent<RectTransform>());
			x = zone.x;
			y = zone.y;
			width = zone.width;
			height = zone.height;
		}

		obj.name = name + "0";

		// Apply position and size
		RectTransform rectTransform = obj.GetComponent<RectTransform>();
		rectTransform.localPosition = new Vector3(x, y, 0f);
		rectTransform.sizeDelta = new Vector2(width, height);

		result.Add(obj);
		return result;
	}

}
