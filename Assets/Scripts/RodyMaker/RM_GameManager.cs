using System.Collections;
using System.Collections.Generic;
using System.IO;
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
	public string currentDial,currentText,introDial1,introDial2,introDial3,objDial,ngpDial,fswDial,titleText,introText,objText,ngpText,fswText, musicIntro, musicLoop;

	void Start() { 
		
		// if gm is load from ingame
		currentScene = PlayerPrefs.GetInt("currentScene");
		
		// Debug only
		// PlayerPrefs.SetInt("currentScene", 1);
		// currentScene = 1;
		PlayerPrefs.SetInt("scenesCount", RM_SaveLoad.CountScenesTxt());
		Debug.Log("scenes count : " + PlayerPrefs.GetInt("scenesCount"));
		
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
		introText  = "glitch intro";
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

	 public void ReadSceneStr(){
        
        string[] sceneStr = new string[26];

		if (PlayerPrefs.GetInt("scenesCount") + 1 == currentScene) {
			Debug.Log("Load last scene text...");
			sceneStr = RM_SaveLoad.LoadSceneTxt(currentScene-1);
		}
		else
			sceneStr = RM_SaveLoad.LoadSceneTxt(currentScene);

		// get music string
		musicIntro = sceneStr[11].Split(',')[0];
		musicLoop  = sceneStr[11].Split(',')[1];			

		string[] pitchs = sceneStr[12].Split(',');
		pitch1     = float.Parse(pitchs[0]);
		pitch2     = float.Parse(pitchs[1]);
		pitch3     = float.Parse(pitchs[2]);

		string[] booleans = sceneStr[13].Split(',');
		isMastico1 = booleans[0] == "1";
		isMastico2 = booleans[1] == "1";
		isMastico3 = booleans[2] == "1";
		isZambla   = booleans[3] == "1";
		
		// wow such refactoring
		introDial1 = sceneStr[0];
		introDial2 = sceneStr[1];
		introDial3 = sceneStr[2];
		objDial    = sceneStr[3];
		ngpDial    = sceneStr[4];
		fswDial    = sceneStr[5];
		titleText  = sceneStr[6];
		title.GetComponent<Text>().text = titleText;
		introText  = sceneStr[7];
		introTextObj.GetComponent<Text>().text = introText;
		objText    = sceneStr[8];
		ngpText    = sceneStr[9];
		fswText    = sceneStr[10];

		objNearTemplate.SetActive(true);
		objTemplate.SetActive(true);
		
        objNear = RM_SaveLoad.ReadObjects("objNear", sceneStr[16], sceneStr[17], true);
        ngpNear = RM_SaveLoad.ReadObjects("ngpNear", sceneStr[20], sceneStr[21], true);
        fswNear = RM_SaveLoad.ReadObjects("fswNear", sceneStr[24], sceneStr[25], true);
        
        obj = RM_SaveLoad.ReadObjects("obj" , sceneStr[14], sceneStr[15]);
		ngp = RM_SaveLoad.ReadObjects("ngp" , sceneStr[18], sceneStr[19]);
		fsw = RM_SaveLoad.ReadObjects("fsw" , sceneStr[22], sceneStr[23]);

		List<List<GameObject>> zonesList = new List<List<GameObject>>{objNear, ngpNear, fswNear};
		foreach(List<GameObject> zones in zonesList)
			foreach (GameObject zone in zones)
				zone.SetActive(false);
		
		objNearTemplate.SetActive(false);
		objTemplate.SetActive(false);
    }

	void Update() {
		if (Input.GetKeyUp(KeyCode.Escape)){
			SceneManager.LoadScene(2);
		}
	}

	void OnApplicationQuit()
    {
        // delete non-saved new level images
		if(currentScene > PlayerPrefs.GetInt("scenesCount")) {
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
