using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RM_ObjLayout : RM_Layout {

	public InputField objInputField;
	public GameObject zoneHelper;
	public Sprite nearSprite, objSprite, validSprite;
	public bool isObj = false;
	public Button returnBtn, phonemsBtn, zoneBtn, textBtn;
	[HideInInspector]
	public int activeObj = 1,  drawState = 0, activeZone = 0;
	[HideInInspector]
	public string phonems;
	[HideInInspector]
	public List<GameObject> zonesNear, zones;
	private GameObject zoneNear, zone;
	private RectTransform btransform;
	private Vector2 startpos, nearPos, nearSize;
	private bool drawing = false;


	public void RM_ReturnClick(){
		Debug.Log("ObjReturn button clicked");
		SetLayouts(gm.objectsLayout,gm.objTextObj, gm.introTextObj);
		// TODO: unset near zones
		UnsetLayouts(gm.objLayout, gm.objTextObj);
		RM_SaveLoad.SetActiveZones(zonesNear, zones, false);

		// Write back phonems and text to GameManager
		switch (activeObj)
        {
            case 1:
                gm.objDial = phonems;
                gm.objText = objInputField.text;
                Debug.Log("Saved objText: " + gm.objText);
                break;
            case 2:
                gm.ngpDial = phonems;
                gm.ngpText = objInputField.text;
                Debug.Log("Saved ngpText: " + gm.ngpText);
                break;
            case 3:
                gm.fswDial = phonems;
                gm.fswText = objInputField.text;
                Debug.Log("Saved fswText: " + gm.fswText);
                break;
            default: break;
        }

		isObj = false;
	}

	public void TextOnClick() {
		if (objInputField.interactable == false) {
			Debug.Log("inputFields are now interactable");
			objInputField.interactable = true;
			returnBtn.interactable = false;
			phonemsBtn.interactable = false;
			zoneBtn.interactable = false;
		}
		else {
			objInputField.interactable = false;
			returnBtn.interactable = true;
			phonemsBtn.interactable = true;
			zoneBtn.interactable = true;
			Debug.Log("inputField are now not interactable");
		}
	}

	// TODO: track which zone is edited
	public void ZoneOnClick() {
		Debug.Log("ZoneOnClick called, activeZone = " + activeZone + ", zones.Count = " + zones.Count);
		// start drawing first near zone
		zoneNear = zonesNear[activeZone];
		zone = zones[activeZone];

		if (activeZone == 0 && zones.Count > 1) {
			// remove other zones when start drawing or it is impossible to remove them
			switch (activeObj)
			{
				case 1:
					foreach(GameObject o in gm.objNear.GetRange(1, zonesNear.Count-1))
						Destroy(o);
					foreach(GameObject o in gm.obj.GetRange(1, zonesNear.Count-1))
						Destroy(o);
					gm.objNear.RemoveRange(1, zonesNear.Count-1);
					gm.obj.RemoveRange(1, zonesNear.Count-1);
					gm.objNear.Add(zoneNear);
					gm.objNear.Add(zone);
					break;
				case 2:
					foreach(GameObject o in gm.ngpNear.GetRange(1, zonesNear.Count-1))
						Destroy(o);
					foreach(GameObject o in gm.ngp.GetRange(1, zonesNear.Count-1))
						Destroy(o);
					gm.ngpNear.RemoveRange(1, zonesNear.Count-1);
					gm.ngp.RemoveRange(1, zonesNear.Count-1);
					gm.ngpNear.Add(zoneNear);
					gm.ngpNear.Add(zone);
					break;
				case 3:
					foreach(GameObject o in gm.fswNear.GetRange(1, zonesNear.Count-1))
						Destroy(o);
					foreach(GameObject o in gm.fsw.GetRange(1, zonesNear.Count-1))
						Destroy(o);
					gm.fswNear.RemoveRange(1, zonesNear.Count-1);
					gm.fsw.RemoveRange(1, zonesNear.Count-1);
					gm.fswNear.Add(zoneNear);
					gm.fswNear.Add(zone);
					break;
				default: break;
			}
			
			zonesNear.RemoveRange(1, zonesNear.Count-1);
			zones.RemoveRange(1, zones.Count-1);
		}

		if (drawState == 0) {
			// display instruction
			zoneHelper.SetActive(true);
			zoneHelper.GetComponent<Text>().text = "Clique et fait glisser pour dessiner la zone proche (jaune) de l'objet.";

			UnsetLayouts(objInputField.gameObject, gm.title);
			
			btransform = zoneNear.GetComponent<RectTransform>();
			btransform.sizeDelta = new Vector2(0,0);
			
			zoneNear.SetActive(true);
			zone.SetActive(false);
			
			Debug.Log("near zone activated");
			drawState = 1; // near rect zone is updated

			zoneBtn.GetComponent<Image>().sprite = objSprite;
			returnBtn.interactable 	= false;
			phonemsBtn.interactable = false;
			textBtn.interactable 	= false;
			zoneBtn.interactable 	= false;
		}
		// start drawing obj zone
		else if (drawState == 3) { 
			zoneHelper.GetComponent<Text>().text = "Dessine la zone de l'objet (verte).\n Elle doit être à l'intérieur de la zone proche (jaune)";
			nearPos = btransform.localPosition;
			nearSize = btransform.sizeDelta;
			btransform = zone.GetComponent<RectTransform>();
			btransform.sizeDelta = new Vector2(0,0);
			zone.SetActive(true);
			zoneBtn.GetComponent<Image>().sprite = validSprite;
			zoneBtn.interactable = false;
			Debug.Log("obj zone activated");
			drawState = 4; // obj rect zone is updated
			
		}
		// stop drawing
		else {
			drawState = 0; 
			returnBtn.interactable = true;
			phonemsBtn.interactable = true;
			textBtn.interactable = true;
			zoneBtn.GetComponent<Image>().sprite = nearSprite;
			Debug.Log("Zone Near are no more drawable");
			// hide instruction
			SetLayouts(objInputField.gameObject, gm.title);
			activeZone = 0;
			zoneHelper.SetActive(false);
		}
	}

	private void Update()
    {
        if (isInScene(mouseCorrected(Input.mousePosition))) {
			if ((drawState == 1 || drawState == 4)  && Input.GetMouseButtonDown(0) && !drawing)
			{
				if (drawState==4 && !isInNearZone(mouseCorrected(Input.mousePosition))) return;
				drawing = true;
				// Debug.Log("Begin draw zone");
				// Debug.Log("update souris en : "+ mouseCorrected(Input.mousePosition));
				startpos = mouseCorrected(Input.mousePosition);
				drawState = (drawState == 1)? 2:5;
			}
			else if ((drawState == 2 || drawState == 5) && Input.GetMouseButtonUp(0) && drawing)
			{
				if (drawState==5 && !isInNearZone(mouseCorrected(Input.mousePosition))) return;
				if (drawState == 2) 
					zoneHelper.GetComponent<Text>().text = "Reclique pour recommencer ou clique sur \nle bouton du menu pour dessiner la zone de l'objet (verte)";
				else {
					string newZoneStr = ((5-activeZone)>0)?"Tu peux rajouter encore " + (5-activeZone) + " zones avec le clique droit ou cliquer" : "Tu ne peux plus ajouter de zone, clique";
					zoneHelper.GetComponent<Text>().text = newZoneStr + " sur le bouton du menu pour terminer";
				}
				drawing = false;
				Debug.Log("a rectangle is drawn");
				zoneBtn.interactable = true;
				drawState = (drawState == 2)? 3:6;
				// Debug.Log("update souris en : "+ mouseCorrected(Input.mousePosition));
				// Debug.Log("button localPos  : "+ btransform.localPosition);
				// Debug.Log("button position  : "+ btransform.position);
			}

			if((drawState == 3 || drawState == 6) && Input.GetMouseButtonDown(0)) {
				Debug.Log("redraw");
				if (drawState == 3) 
					zoneHelper.GetComponent<Text>().text = "Clique et fait glisser pour dessiner la zone proche (jaune) de l'objet.";
				else zoneHelper.GetComponent<Text>().text = "Dessine la zone de l'objet (verte).\n Elle doit être à l'intérieur de la zone proche (jaune)";
				
				btransform.sizeDelta = new Vector2(0,0);
				drawState = (drawState == 3)? 1:4;
				zoneBtn.interactable = false;
			}

			// draw a new zone by pressing right click after one is drawn
			if(drawState == 6 && Input.GetMouseButtonDown(1)) {
				if (activeZone < 5) {
					Debug.Log("new zone !");
					drawState = 0;
					zoneBtn.interactable = false;
					activeZone++;
					Debug.Log("zones.Count : "+zones.Count+", activeZone : " + activeZone);
					if (activeZone < zones.Count) {
						zoneNear = zonesNear[activeZone];
						zone = zones[activeZone];
					}
					else {
						// clone first zone
						zoneNear = GameObject.Instantiate(zonesNear[0],GameObject.Find("Objects").GetComponent<RectTransform>());
						zone = zoneNear.transform.GetChild(0).gameObject; // should be instantiate by zoneNear
						zonesNear.Add(zoneNear);
						zones.Add(zone);
					}
				}
				ZoneOnClick();
			}

			if (drawState == 2 && drawing)
				UpdateNear(mouseCorrected(Input.mousePosition));
			if (drawState == 5 && drawing && isInNearZone(mouseCorrected(Input.mousePosition)))
				UpdateObj(mouseCorrected(Input.mousePosition));
		}

    }

	private Vector2 mouseCorrected(Vector2 mp) {
		Vector2 newPos = new Vector2(mp.x,mp.y);
		float ratioW = Screen.width / 320;
		float ratioH = Screen.height / 200;
		newPos.x /= ratioW;
		newPos.y /= ratioH;
		return newPos;
	}

	private bool isInScene(Vector2 pos) {
		return(pos.x <= 320 && pos.y <= 130);
	}
	
	private bool isInNearZone(Vector2 pos) {
		pos.x -= 160;
		pos.y -= 65;
		
		return(	pos.x < (nearPos.x + (nearSize.x/2)) && 
				pos.y < (nearPos.y + (nearSize.y/2)) &&
				pos.x > (nearPos.x - (nearSize.x/2)) &&
				pos.y > (nearPos.y - (nearSize.y/2)));
	}

	private void UpdateNear(Vector2 mpos)
    {
        btransform.sizeDelta = Vector2.Max(startpos, mpos) - Vector2.Min(startpos, mpos);
        btransform.localPosition = Vector2.Min(
								  (Vector2.Min(startpos, mpos) + (btransform.sizeDelta * 0.5f) - new Vector2(160,65)),
									new Vector2(320,130));
    }

	private void UpdateObj(Vector2 mpos)
    {
		// Debug.Log("good mouse : " + (mpos.x - 160) + " , " + (mpos.y - 65));
        btransform.sizeDelta = Vector2.Max(startpos, mpos) - Vector2.Min(startpos, mpos);
        btransform.localPosition = (Vector2.Min(startpos, mpos) + (btransform.sizeDelta * 0.5f)) - nearPos - new Vector2(160,65);
    }

	public void RM_PhonemesClick(){
		Debug.Log("phonemes button clicked");
		SceneManager.LoadScene(7, LoadSceneMode.Additive);
	}
}
