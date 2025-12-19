using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SceneLoading : MonoBehaviour {

	public GameManager gm;
	public List<GameObject> blackPanels;

	public bool isPlaying = true;
	public GameObject zambla;

	public void Play() {
		StartCoroutine (Load ());
		isPlaying = true;
	}

	IEnumerator Load()
	{
		yield return new WaitForSeconds(0.2f);
		gm.p_layout.SetActive (true);
		yield return new WaitForSeconds(0.2f);
		gm.p_title.GetComponent<UnityEngine.UI.Text>().text = gm.titleText;
		gm.p_title.SetActive (true);
		
		yield return new WaitForSeconds(0.2f);
		gm.p_text.GetComponent<UnityEngine.UI.Text>().text = gm.introText;
		gm.p_text.SetActive (true);

		foreach (GameObject panel in blackPanels) {
			yield return new WaitForSeconds(0.2f);
			panel.SetActive (false);
		}

		yield return new WaitForSeconds(0.05f);
		gm.b_mastico.SetActive (true);
		yield return new WaitForSeconds(0.05f);
		gm.b_next.SetActive (true);
		yield return new WaitForSeconds(0.05f);
		gm.b_draw.SetActive (true);
		gm.b_draw.GetComponent<Button>().interactable = true;
		yield return new WaitForSeconds(0.05f);
		gm.b_repeat.SetActive (true);
		yield return new WaitForSeconds(0.05f);

		// Show Zambla for the Ibiza story
		string currentGame = PlayerPrefs.GetString("gamePath");
		bool isIbiza = currentGame.Contains("Ibiza") || currentGame == "Rody Et Mastico A Ibiza";
		if (zambla != null && isIbiza) {
			zambla.SetActive(true);
			yield return new WaitForSeconds(0.05f);
		}
		Debug.Log("fin loading");
		isPlaying = false;
	}

}
