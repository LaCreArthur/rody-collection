using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickHandler : MonoBehaviour {

	public GameManager gm;
	public void NextClick() {

		int nextScene = StoryRoot.Session.CurrentSceneIndex + 1;

		if (gm.clickIntro) {
			Debug.Log ("next clicked in intro");

			// Final scenes can still have an object phase. Only skip straight to credits
			// when there is no primary object target to enter after the intro.
			if (nextScene > StoryRoot.Session.SceneCount && !HasPrimaryObjectPhase()) {
				gm.intro.sceneMusic.Stop();
				gm.clickIntro = false;
				SceneManager.LoadScene(AppScenes.Win);
				return;
			}

			gm.intro.sceneMusic.Stop();
			gm.introOver = true;
			gm.clickIntro = false;
		}
		else if (gm.clickObj) {
			Debug.Log ("next clicked in obj/ngp/fsw, next = " + nextScene);

            StoryRoot.Session.CurrentSceneIndex = nextScene;
			SceneManager.LoadScene(AppScenes.Game);
        }
	}

	bool HasPrimaryObjectPhase()
	{
		return gm.obj != null && gm.obj.Count > 0 && gm.objNear != null && gm.objNear.Count > 0;
	}

	public void LaunchCredit() {
		SceneManager.LoadScene(AppScenes.Win);
	}

	public void RepeatClick() {
		if (gm.clickIntro) {
			gm.clickIntro = false;
			Debug.Log("repeat intro dialog");
			gm.intro.sceneMusic.Stop();
			StartCoroutine(gm.intro.Dialog());
		}
		else if (gm.clickObj) {
			gm.clickObj = false;
			Debug.Log("repeat obj/ngp/fsw dialog");
			StartCoroutine(gm.scene.dialog());
		}
	}

	public void DrawClick() {
		//PlayerPrefs.SetInt("scene", SceneManager.GetActiveScene().buildIndex);
		SceneManager.LoadScene(AppScenes.Editor);
	}

	public void ngpClick() {
		gm.objOver = true;
		gm.clickObj = false;
	}
	public void fswClick() {
		gm.ngpOver = true;
		gm.clickObj = false;
	}
}
