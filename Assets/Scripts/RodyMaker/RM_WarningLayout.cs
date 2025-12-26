using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RM_WarningLayout : RM_Layout {

	public int newScene;
	public bool test = false;
	public bool isDeleteMode = false;  // True when deleting a scene
	public Text warningText;

	public void CancelClick(){
		Debug.Log("Cancel button clicked");
		test = false;
		isDeleteMode = false;
		ResetLayout();
	}
	public void AcceptClick(){

		Debug.Log("Accept button clicked, load scene : " + newScene);
		if (test){
			// Test mode - load play scene
			test = false;  // Reset flag
			if (newScene == 0) // test of the title screen
            	SceneManager.LoadScene(1);
        	else
            	SceneManager.LoadScene(3);
			return;
		}
		else if (isDeleteMode) {
			// Delete current scene
			Debug.Log("Deleting scene " + gm.currentScene);
			isDeleteMode = false;  // Reset flag
			if (gm.currentScene <= PlayerPrefs.GetInt("scenesCount"))
				RM_SaveLoad.DeleteScene(gm.currentScene);
			else
				Debug.Log("cancel new scene creation");
			// Load the preceding scene
			newScene = gm.currentScene - 1;
			if (newScene < 1) newScene = 1;
		}
		else if (gm.currentScene == newScene) {
			// Clicking on current scene (shouldn't happen normally, but handle it)
			ResetLayout();
			return;
		}
		// Change to the new scene
		PlayerPrefs.SetInt("currentScene", newScene);
		gm.currentScene = newScene;
		gm.Reset();
		ResetLayout();
	}

	public void ResetLayout(){
		SetLayouts(gm.mainLayout);
		UnsetLayouts(gm.warningLayout);
	}
}
