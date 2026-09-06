using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class BtnPhoneme : MonoBehaviour {

	public SoundManager sm;
	public InputField input;
	public string p;
	public void OnClick() {
		// play the clicked phoneme
		sm.Speak(p);
		// add to the list
		if (input.text.Length == 0) input.text = p;
		else input.text += "_" + p;
	}
}
