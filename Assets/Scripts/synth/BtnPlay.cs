using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BtnPlay : MonoBehaviour {

	public SynthManager synth;

	private Button btn;

	void Start() {
		btn = this.gameObject.GetComponent<Button>();
	}

	public void OnClick() {
		// play the input with a specified pitch
		synth.sm.Speak(synth.input.text, synth.pitchSlider.value);
		Debug.Log("say : \"" + synth.input.text + "\" with a picth of " + synth.pitchSlider.value );
	}


	void Update() {
		if (synth.sm.isPlaying) btn.interactable = false;
		else btn.interactable = true;
	}
}
