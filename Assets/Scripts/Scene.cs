using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scene : MonoBehaviour {

	public GameManager gm;
    public bool isInit = false;

	private string txt;
	private List<int> dial;
	private AudioClip[] fx;
	private int objectsToFind = 1;
	private int step;

    public void Play(int step) {
		this.step = step;
		initStep(step);
		StartCoroutine(Sequence());
		StartCoroutine(End(step));
	}

// init the scene variables
	public void initStep(int step) {
		if (isInit)
			return;
		switch(step) {
			case 1: 
				txt  = gm.objText;
				dial = gm.getDial(3);
				fx   = gm.sm.sounds_fx_debutObj;
				objectsToFind = gm.obj.Count;
				break;
			case 2:
				txt  = gm.ngpText;
				dial = gm.getDial(4);
				fx   = gm.sm.sounds_fx_debutNgp;
				objectsToFind = gm.ngp.Count;
				break;
			case 3:
				txt  = gm.fswText;
				dial = gm.getDial(5);
				fx   = gm.sm.sounds_non;
				objectsToFind = gm.fsw.Count;
				break;
			default: 
				txt  = "scene switcher glitch";
				dial = new List<int> { P.g, P.l, P.i, P.t, P.ch};
				fx   = gm.sm.sounds_non;
				break;
		}
		Debug.Log("isInit set to true");
		isInit = true;
	}

	IEnumerator Sequence() {
		yield return new WaitForSeconds(0.2f);
		// remove non unsable buttons
		gm.p_text.SetActive(false);
		gm.b_draw.SetActive(false);
		gm.b_next.SetActive(false);
		gm.b_ngp.SetActive(false);
		gm.b_fsw.SetActive(false);
		gm.b_repeat.SetActive(true);

		// Set the object text
		gm.p_text.GetComponent<UnityEngine.UI.Text>().text = txt;

		yield return new WaitForSeconds(0.2f);
		// display it
		gm.p_text.SetActive(true);
		yield return new WaitForSeconds(0.2f);

		// play a start object fx
		gm.sm.RandomSound(fx);
		
		while(gm.sm.soundSource.isPlaying) 
			yield return null;
		
		StartCoroutine(dialog());

	}

	public IEnumerator dialog() {
		// play the obj dialog and an ending fx
		StartCoroutine(gm.sm.MasticoSpeak(dial, true));

		while(gm.MasticoAnimator.GetBool("isSpeaking"))
			yield return null;

		gm.sm.RandomSound(gm.sm.sounds_fx_fin);
		while(gm.sm.soundSource.isPlaying) 
			yield return null;

		yield return new WaitForSeconds(0.2f);

		// Start object research
		gm.clickObj = true;
		gm.interObj = false;
		Debug.Log("TOTAL OBJECT TO FIND : " + objectsToFind);
	}

	IEnumerator End(int step) {
		// wait until object is founded
		while (objectsToFind > 0)
			yield return null;
		gm.b_draw.SetActive(true);
		gm.b_next.SetActive(true);
		gm.b_repeat.SetActive(false);
		switch (step) {
			case 1: // obj
				// Debug.Log(step);
				gm.b_ngp.SetActive(true);
				RM_SaveLoad.SetActiveZones(gm.objNear, gm.obj, false); 
				// gm.obj.SetActive(false);
				break;
			case 2: // ngp
				gm.b_fsw.SetActive(true);
				RM_SaveLoad.SetActiveZones(gm.ngpNear, gm.ngp, false);
				// gm.ngp.SetActive(false);
				break;
			case 3: // fsw
				RM_SaveLoad.SetActiveZones(gm.fswNear, gm.fsw, false);
				// gm.fsw.SetActive(false);
				break;
			default: break;
		}
		gm.clickObj = true;
		gm.interObj = true;
	}


	public void Founded() {
		if (gm.clickObj) {
			gm.clickObj = false;
			Debug.Log("BRAVO !");
			StartCoroutine(oui());
		}
	}

	public void Near() {
		if (gm.clickObj)
			gm.clickObj = false;
			Debug.Log("CEST PRESQUE CA !");
			StartCoroutine(presque());
	}

	public void Miss() {
		if (gm.clickObj && !gm.interObj) {
			gm.clickObj = false;
			Debug.Log("NON RECOMMENCE !");
			StartCoroutine(non());
		}
	}

	void ActivateNextObjectTargets(List<GameObject> nearTargets, List<GameObject> targets, string stepName)
	{
		int next = targets.Count - objectsToFind;
		if (next <= 0 || next >= targets.Count || next >= nearTargets.Count)
		{
			Debug.LogWarning($"[Scene] Ignoring malformed {stepName} target list progression (next={next}, targets={targets.Count}, nearTargets={nearTargets.Count})");
			return;
		}

		nearTargets[next - 1].SetActive(false);
		targets[next - 1].SetActive(false);
		nearTargets[next].SetActive(true);
		targets[next].SetActive(true);
	}

	IEnumerator oui() {
		gm.MasticoAnimator.SetTrigger("Process");
		yield return new WaitForSeconds(gm.MasticoAnimator.GetCurrentAnimatorClipInfo(0).Length);

		gm.MasticoAnimator.SetTrigger("Oui");
		
		gm.sm.RandomSound(gm.sm.sounds_oui);
		while(gm.sm.soundSource.isPlaying) 
			yield return null;
		
		StartCoroutine(gm.sm.MasticoSpeak(gm.sm.RandomOui(), false));
		
		while(gm.MasticoAnimator.GetBool("isSpeaking"))
			yield return null;

		objectsToFind--;
		if (objectsToFind > 0) {
			gm.clickObj = true; // other objects need to be found
			switch (step) {
				case 1: // obj
					ActivateNextObjectTargets(gm.objNear, gm.obj, "obj");
					break;
				case 2: // ngp
					ActivateNextObjectTargets(gm.ngpNear, gm.ngp, "ngp");
					break;
				case 3: // fsw
					ActivateNextObjectTargets(gm.fswNear, gm.fsw, "fsw");
					break;
				default: break;
			}
			Debug.Log("OBJECT TO FIND : " + objectsToFind);
		}
	}

	IEnumerator presque() {
		gm.MasticoAnimator.SetTrigger("Process");
		yield return new WaitForSeconds(gm.MasticoAnimator.GetCurrentAnimatorClipInfo(0).Length);
		
		gm.sm.RandomSound(gm.sm.sounds_presque);
		while(gm.sm.soundSource.isPlaying) 
			yield return null;

		StartCoroutine(gm.sm.MasticoSpeak(gm.sm.RandomPresque(), false));
		yield return new WaitForSeconds(1f);

		gm.clickObj = true;
	}

	IEnumerator non() {
		gm.MasticoAnimator.SetTrigger("Process");
		yield return new WaitForSeconds(gm.MasticoAnimator.GetCurrentAnimatorClipInfo(0).Length);

		gm.MasticoAnimator.SetTrigger("Non");
		
		gm.sm.RandomSound(gm.sm.sounds_non);
		while(gm.sm.soundSource.isPlaying) 
			yield return null;
		
		StartCoroutine(gm.sm.MasticoSpeak(gm.sm.RandomNon(), false));
		yield return new WaitForSeconds(1f);
		gm.clickObj = true;
	}


}