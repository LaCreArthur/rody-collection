using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RM_GameManager : MonoBehaviour {

	public GameObject mainLayout;
	public GameObject imagesLayout;
	public GameObject imgAnimLayout;
	public GameObject introLayout;
	public GameObject dialoguesLayout;
	public GameObject dialLayout;
	public GameObject objectsLayout; 
	public GameObject objLayout; 
	public GameObject musicLayout;
	public GameObject warningLayout;
	public GameObject welcomePanel;
	public GameObject introTextObj;
	public GameObject objTextObj;
	public GameObject ngpTextObj;
	public GameObject fswTextObj;
	public GameObject title;
	public GameObject scenePanel;
	public GameObject objNearTemplate, objTemplate;
	public List<GameObject> objNear, obj, ngpNear, ngp, fswNear, fsw;
	public SoundManager sm;
	[HideInInspector] 
	public int currentScene = 0;
	public int  framesCount = 0;
	[HideInInspector] 
	public float pitch1, pitch2, pitch3;
	[HideInInspector] 
	public bool isMastico1, isMastico2, isMastico3, isZambla, modified = false;
	[HideInInspector]
	public string currentDial,currentText,introDial1,introDial2,introDial3,objDial,ngpDial,fswDial,titleText,introText1,introText2,introText3,objText,ngpText,fswText, musicIntro, musicLoop;

	void Start() {

		// Load current scene from WorkingStory
		currentScene = WorkingStory.CurrentSceneIndex;

		Debug.Log($"[RM_GameManager] scenes count: {WorkingStory.SceneCount}");
		
		if (PlayerPrefs.GetInt("rodyMakerFirstTime") == 1) {
			welcomePanel.SetActive(true);
		}
		// set rodyMakerFirstTime to false at rody maker start
		PlayerPrefs.SetInt("rodyMakerFirstTime", 0);

		mainLayout.SetActive(true);
		
		introLayout.SetActive(false);
		warningLayout.SetActive(false);
		dialoguesLayout.SetActive(false);
		dialLayout.SetActive(false);
		objectsLayout.SetActive(false); 
		objLayout.SetActive(false); 
		musicLayout.SetActive(false);
		introTextObj.SetActive(false);
		title.SetActive(false);

		Reset();
	}

	public void Reset() {

		// Variables for the txt file
		introDial1 = ".";
		introDial2 = ".";
		introDial3 = ".";
		objDial    = ".";
		ngpDial    = ".";
		fswDial    = ".";
		titleText  = "glitch title";
		introText1 = "";
		introText2 = "";
		introText3 = "";
		objText    = ".";
		ngpText    = ".";
		fswText    = ".";
		pitch1 = 1f;
		pitch2 = 1f;
		pitch3 = 1f;
		// Variables for the interface
		introTextObj.GetComponent<Text>().text = "Dialogues de la scène";
		title.GetComponent<Text>().text 	   = "Titre de la scène";
		objTextObj.GetComponent<Text>().text   = "Indice du premier objet";
		ngpTextObj.GetComponent<Text>().text   = "Indice de l'objet New Game Plus";
		fswTextObj.GetComponent<Text>().text   = "Indice de l'objet FromSoftware";

		introLayout.GetComponent<RM_IntroLayout>().titleInputField.text = null;
		objLayout.GetComponent<RM_ObjLayout>().objInputField.text = null;

		// Reset thumbnails
		mainLayout.GetComponent<RM_MainLayout>().LoadSprites();
		mainLayout.GetComponent<RM_MainLayout>().UpdateActiveThumbnail();
		mainLayout.GetComponent<RM_MainLayout>().UpdateButtonStates();
		// objLayout.GetComponent<RM_ObjLayout>().zoneNear.SetActive(false);


		if (currentScene != 0) {
			ReadSceneStr();
		}
	}

	/// <summary>
	/// Loads scene data from WorkingStory and populates editor fields.
	/// </summary>
	public void ReadSceneStr()
	{
		int sceneToLoad = currentScene;

		// If creating a new scene, load from the previous scene as template
		if (WorkingStory.SceneCount + 1 == currentScene)
		{
			Debug.Log("[RM_GameManager] Load previous scene as template...");
			sceneToLoad = currentScene - 1;
		}

		// Load scene data from WorkingStory
		SceneData data = WorkingStory.LoadScene(sceneToLoad);
		if (data == null)
		{
			Debug.LogError($"[RM_GameManager] Failed to load scene {sceneToLoad}");
			return;
		}

		// Music
		musicIntro = data.music?.introMusic ?? "i1";
		musicLoop = data.music?.sceneMusic ?? "l1";

		// Voice settings
		pitch1 = data.voice?.pitch1 ?? 1f;
		pitch2 = data.voice?.pitch2 ?? 1f;
		pitch3 = data.voice?.pitch3 ?? 1f;
		isMastico1 = data.voice?.isMastico1 ?? false;
		isMastico2 = data.voice?.isMastico2 ?? false;
		isMastico3 = data.voice?.isMastico3 ?? false;
		isZambla = data.voice?.isZambla ?? false;

		// Dialogues (phonemes)
		introDial1 = data.dialogues?.intro1 ?? ".";
		introDial2 = data.dialogues?.intro2 ?? ".";
		introDial3 = data.dialogues?.intro3 ?? ".";
		objDial = data.dialogues?.obj ?? ".";
		ngpDial = data.dialogues?.ngp ?? ".";
		fswDial = data.dialogues?.fsw ?? ".";

		// Display texts
		titleText = data.texts?.title ?? "glitch title";
		title.GetComponent<Text>().text = titleText;
		introText1 = data.texts?.intro1 ?? "";
		introText2 = data.texts?.intro2 ?? "";
		introText3 = data.texts?.intro3 ?? "";
		// Update UI to show first intro text (or combined for display)
		introTextObj.GetComponent<Text>().text = !string.IsNullOrEmpty(introText1) ? introText1 : "Dialogues de la scène";
		objText = data.texts?.obj ?? ".";
		ngpText = data.texts?.ngp ?? ".";
		fswText = data.texts?.fsw ?? ".";

		// Object zones
		objNearTemplate.SetActive(true);
		objTemplate.SetActive(true);

		objNear = CreateZoneList("objNear", data.objects?.obj, true);
		ngpNear = CreateZoneList("ngpNear", data.objects?.ngp, true);
		fswNear = CreateZoneList("fswNear", data.objects?.fsw, true);

		obj = CreateZoneList("obj", data.objects?.obj, false);
		ngp = CreateZoneList("ngp", data.objects?.ngp, false);
		fsw = CreateZoneList("fsw", data.objects?.fsw, false);

		List<List<GameObject>> zonesList = new List<List<GameObject>>{objNear, ngpNear, fswNear};
		foreach(List<GameObject> zones in zonesList)
			foreach (GameObject zone in zones)
				zone.SetActive(false);

		objNearTemplate.SetActive(false);
		objTemplate.SetActive(false);
	}

	/// <summary>
	/// Creates a list of zone GameObjects from typed ObjectZone data.
	/// </summary>
	private List<GameObject> CreateZoneList(string name, ObjectZone zone, bool isNear)
	{
		var result = new List<GameObject>();
		if (zone == null) return result;

		GameObject parent, zoneObj;
		float x, y, width, height;

		if (isNear)
		{
			parent = GameObject.Find("Objects");
			zoneObj = Instantiate(objNearTemplate, parent.GetComponent<RectTransform>());
			x = zone.nearX;
			y = zone.nearY;
			width = zone.nearWidth;
			height = zone.nearHeight;
		}
		else
		{
			parent = GameObject.Find(name + "Near0");
			zoneObj = Instantiate(objTemplate, parent.GetComponent<RectTransform>());
			x = zone.x;
			y = zone.y;
			width = zone.width;
			height = zone.height;
		}

		zoneObj.name = name + "0";

		RectTransform rectTransform = zoneObj.GetComponent<RectTransform>();
		rectTransform.localPosition = new Vector3(x, y, 0f);
		rectTransform.sizeDelta = new Vector2(width, height);

		result.Add(zoneObj);
		return result;
	}

	void Update() {
		if (Input.GetKeyUp(KeyCode.Escape)){
			SceneManager.LoadScene(2);
		}
	}

	void OnApplicationQuit()
    {
        // delete non-saved new level images
		if(currentScene > WorkingStory.SceneCount) {
			RM_SaveLoad.DeleteScene(currentScene);
		}
    }

	public void OnWelcomePanelExit() {
		welcomePanel.SetActive(false);
	}

	public void OnWelcomePanelWebsite() {
		Debug.Log("go on website !");
		Application.OpenURL("https://lacrearthur.itch.io/rody-maker");
	}

	public void OnWelcomePanelYoutube() {
		Debug.Log("go on youtube !");
		Application.OpenURL("https://youtu.be/1vx8D2irVLI?t=2m17s");
	}
}
