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
		switch (activeDial)
		{
			case 1:
				textInputField.text = gm.introText1 ?? "";
				break;
			case 2:
				textInputField.text = gm.introText2 ?? "";
				break;
			case 3:
				textInputField.text = gm.introText3 ?? "";
				break;
			default:
				textInputField.text = "";
				break;
		}
	}

	/// <summary>
	/// Writes back the dialog display text from textInputField to gm.introText1/2/3.
	/// </summary>
	private void SetDialText() {
		switch (activeDial)
		{
			case 1:
				gm.introText1 = textInputField.text;
				break;
			case 2:
				gm.introText2 = textInputField.text;
				break;
			case 3:
				gm.introText3 = textInputField.text;
				break;
		}
		Debug.Log($"Saved introText{activeDial}: {textInputField.text}");
	}
}
