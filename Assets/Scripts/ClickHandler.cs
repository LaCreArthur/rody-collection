using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickHandler : MonoBehaviour {

	public GameManager gm;
	public void NextClick() {

		int nextScene = WorkingStory.CurrentSceneIndex + 1;

		if (gm.clickIntro) {
			Debug.Log ("next clicked in intro");
			if (nextScene > WorkingStory.SceneCount) // it was the last scene, load credits, no objects to found
				SceneManager.LoadScene(5);

			gm.intro.sceneMusic.Stop();
			gm.introOver = true;
			gm.clickIntro = false;
		}
		else if (gm.clickObj) {
			Debug.Log ("next clicked in obj/ngp/fsw, next = " + nextScene);

            WorkingStory.CurrentSceneIndex = nextScene;
			SceneManager.LoadScene(3);
        }
	}

	public void LaunchCredit() {
		SceneManager.LoadScene(5);
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
		SceneManager.LoadScene(6);
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
