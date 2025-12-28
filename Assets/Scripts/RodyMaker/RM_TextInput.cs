using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RM_TextInput : MonoBehaviour {

	private InputField inputField;
	public RM_GameManager gm;
	void Awake() {
		inputField = gameObject.GetComponent<InputField>();
		inputField.interactable = false;
	}

	public void OnEnd(string input) {
		if (input == "title"){
			gm.titleText = inputField.text;
			Debug.Log("titleText is now : " + gm.titleText);
			gm.title.GetComponent<Text>().text = inputField.text;
		}
		if (input == "introText"){
			// Store directly to the appropriate intro text field
			int activeDial = gm.dialLayout.GetComponent<RM_DialLayout>().activeDial;
			switch(activeDial){
				case 1:
					gm.introText1 = inputField.text;
					Debug.Log($"introText1 is now: {gm.introText1}");
					break;
				case 2:
					gm.introText2 = inputField.text;
					Debug.Log($"introText2 is now: {gm.introText2}");
					break;
				case 3:
					gm.introText3 = inputField.text;
					Debug.Log($"introText3 is now: {gm.introText3}");
					break;
				default: break;
			}

			// Update display to show first intro text
			gm.introTextObj.GetComponent<Text>().text = !string.IsNullOrEmpty(gm.introText1) ? gm.introText1 : "Dialogues de la scène";
		}
		if (input == "objText"){

			RM_ObjLayout objLayoutScript = gm.objLayout.GetComponent(typeof (RM_ObjLayout)) as RM_ObjLayout;
			// Debug.Log(objLayoutScript.activeObj);

			switch (objLayoutScript.activeObj){
				case 1: gm.objText = inputField.text; 
						gm.objTextObj.GetComponent<Text>().text = inputField.text;
						Debug.Log("objText is now : " + gm.objText);
						break;
				case 2: gm.ngpText = inputField.text; 
						gm.ngpTextObj.GetComponent<Text>().text = inputField.text;
						Debug.Log("ngpText is now : " + gm.ngpText);
						break;
				case 3: gm.fswText = inputField.text; 
						gm.fswTextObj.GetComponent<Text>().text = inputField.text;
						Debug.Log("fswText is now : " + gm.fswText);
						break;
				default : break;
			}
		}
	}

	void Update() {
	}

}
