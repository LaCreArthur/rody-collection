using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class RM_DialLayout : RM_Layout {

	public GameObject isMasticoBtn;
	public Button phonemsBtn, returnBtn;
	public InputField textInputField;
	public Sprite masticoUnmute, masticoMute;
	[HideInInspector]
	public int activeDial = 1;
	[HideInInspector]
	public float pitch;
	[HideInInspector]
	public string phonems = "";
	public bool isDial = false, isMastico = false;
	public void RM_ReturnClick(){
		Debug.Log("DialReturn button clicked");
		SetLayouts(gm.dialoguesLayout, gm.introTextObj);
		UnsetLayouts(gm.dialLayout);

		// Write back pitch and phonems
		switch (activeDial)
        {
            case 1:
                gm.pitch1 = pitch;
                gm.introDial1 = phonems;
                break;
            case 2:
                gm.pitch2 = pitch;
                gm.introDial2 = phonems;
                break;
            case 3:
                gm.pitch3 = pitch;
                gm.introDial3 = phonems;
                break;
            default: break;
        }

		// Write back dialog display text to introText
		SetDialText();

		Debug.Log("new pitch is : " + pitch);
		Debug.Log("new introDial is : " + phonems);
		isDial = false;
		gm.dialoguesLayout.GetComponent<RM_DialoguesLayout>().SetDialButtons();
	}

	public void RM_PhonemesClick(){
		Debug.Log("phonemes button clicked");
		SceneManager.LoadScene(7, LoadSceneMode.Additive);
	}

	public void RM_TextClick(){
		Debug.Log("Text button clicked");
		if (textInputField.interactable){
			textInputField.interactable = false;
			Debug.Log("inputField is now not interactable");
			phonemsBtn.interactable = 
			returnBtn.interactable = 
			isMasticoBtn.GetComponent<Button>().interactable = true;
		}
		else {
			textInputField.interactable = true;
			Debug.Log("inputField is now interactable");
			phonemsBtn.interactable = 
			returnBtn.interactable = 
			isMasticoBtn.GetComponent<Button>().interactable = false;
		}
	}

	public void RM_IsMasticoClick(){
		Debug.Log("IsMastico button clicked");
		
		isMastico = (isMastico)? false : true;
		updateMasticoSprite();

		switch (activeDial)
        {
            case 1:
                gm.isMastico1 = isMastico;
                break;
            case 2:
                gm.isMastico2 = isMastico;
                break;
            case 3:
                gm.isMastico3 = isMastico;
                break;
            default: break;
        }
	}

	public void updateMasticoSprite(){
		isMasticoBtn.GetComponent<Image>().sprite = (isMastico)? masticoUnmute : masticoMute;
	}
	
	public void GetDialText() {
		string[] dials = gm.introText.Split('"');
		textInputField.text = "";
		switch (activeDial)
        {
            case 1:
                if (dials.Length > 2) textInputField.text = dials[1];
                break;
            case 2:
                if (dials.Length > 4) textInputField.text = dials[3];
                break;
            case 3:
                if (dials.Length > 6) textInputField.text = dials[5];
                break;
            default: break;
        }
	}

	/// <summary>
	/// Writes back the dialog display text from textInputField to gm.introText.
	/// Format: text"dialog1"text"dialog2"text"dialog3"text
	/// </summary>
	private void SetDialText() {
		string[] dials = gm.introText.Split('"');
		if (dials.Length < 2) return; // No dialogs to update

		int dialIndex = (activeDial * 2) - 1; // 1->1, 2->3, 3->5
		if (dialIndex < dials.Length) {
			dials[dialIndex] = textInputField.text;
			gm.introText = string.Join("\"", dials);
			Debug.Log("Saved introText dialog " + activeDial + ": " + textInputField.text);
		}
	}
}
