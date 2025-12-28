using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RM_IntroLayout : RM_Layout {

	public InputField titleInputField;
	public Button returnBtn, musicBtn, dialoguesBtn;

	public void TextClick() {
		if (titleInputField.interactable) {
			titleInputField.interactable = false;
			Debug.Log("inputFiel is now not interactable");
			returnBtn.interactable = 
			musicBtn.interactable = 
			dialoguesBtn.interactable = true;
		}
		else {
			titleInputField.interactable = true;
			Debug.Log("inputField is now interactable");
			returnBtn.interactable =
			musicBtn.interactable = 
			dialoguesBtn.interactable = false;
		}
	}

	public void RM_ReturnClick(){
		Debug.Log("IntroReturn button clicked");

		// Write back title to GameManager
		gm.titleText = titleInputField.text;
		Debug.Log("Saved titleText: " + gm.titleText);

		SetLayouts(gm.mainLayout);
		UnsetLayouts(gm.introTextObj, gm.title, gm.introLayout);
	}

	public void RM_MusicClick(){
		Debug.Log("Music button clicked");
		// set music by strings
		SetLayouts(gm.introTextObj, gm.title, gm.musicLayout);
		UnsetLayouts(gm.introLayout);
        gm.musicLayout.GetComponent<RM_MusicLayout>().SetMusic();
	}

	public void RM_DialoguesClick(){
		Debug.Log("IntroDial button clicked");
		SetLayouts(gm.dialoguesLayout, gm.introTextObj, gm.title);
		UnsetLayouts(gm.introLayout);
		gm.dialoguesLayout.GetComponent<RM_DialoguesLayout>().SetDialButtons();
	}
}
