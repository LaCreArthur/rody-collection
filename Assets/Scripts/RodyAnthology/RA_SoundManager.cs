using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RA_SoundManager : MonoBehaviour {

	public AudioClip feedbackEnableClip, feedbackDisableClip, slotSelectedClip, joyLeftClip, joyLeftUnlock, joyRightClip;
	public AudioSource audioSource;
	int joyLeftClicks = 0;
	public bool isRollPlaying = false;
	void Awake () {
		audioSource = gameObject.GetComponent<AudioSource>();
	}

	void Update() {

	}

	public void OnFeedbackEnabled(){
		ResetDefaultValues();
		audioSource.clip = feedbackEnableClip;
		audioSource.Play();
	}

	public void OnFeedbackDisabled(){
		ResetDefaultValues();
		audioSource.clip = feedbackDisableClip;
		audioSource.Play();
	}

	public void OnSlotSelection() {
		audioSource.clip = slotSelectedClip;
		audioSource.pitch = Random.Range(0.7f, 1.3f);
		audioSource.volume = 0.2f;
		audioSource.Play();
	}

	public void OnJoyLeftClick() {
		if (joyLeftClicks == 0) audioSource.pitch = 0.7f;
		if (joyLeftClicks == 4) {
			audioSource.clip = joyLeftUnlock;
			SceneManager.LoadScene("RollToInfinity");
			isRollPlaying = true;
		}
		else 
			audioSource.clip = joyLeftClip;
		joyLeftClicks++;
		audioSource.pitch += 0.1f;
		audioSource.volume = 0.2f;
		audioSource.Play();
	}

	public void OnJoyRightClick() {
		ResetDefaultValues();
		audioSource.clip = joyRightClip;
		audioSource.Play();
		try
		{
			SceneManager.UnloadSceneAsync("RollToInfinity");
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
		}
		if (isRollPlaying) {
			SceneManager.LoadScene(0);
		}
	}

	void ResetDefaultValues() {
		audioSource.pitch = 1f;
		audioSource.volume = 0.33f;
	}
}
