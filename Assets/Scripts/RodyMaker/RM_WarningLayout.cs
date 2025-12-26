using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Handles the warning/confirmation dialog in the editor.
/// Shows warnings before scene changes, deletions, or entering test mode.
/// </summary>
public class RM_WarningLayout : RM_Layout {

	[FormerlySerializedAs("newScene")]
	public int targetScene;

	[FormerlySerializedAs("test")]
	public bool isTestMode = false;

	public bool isDeleteMode = false;

	[FormerlySerializedAs("warningText")]
	public Text messageText;

	public void OnCancelClick(){
		Debug.Log("Warning dialog cancelled");
		isTestMode = false;
		isDeleteMode = false;
		CloseDialog();
	}

	public void OnConfirmClick(){
		Debug.Log("Warning dialog confirmed, target scene: " + targetScene);

		if (isTestMode){
			// Test mode - load play scene
			isTestMode = false;
			if (targetScene == 0)
				SceneManager.LoadScene(1);  // Title screen test
			else
				SceneManager.LoadScene(3);  // Game scene test
			return;
		}
		else if (isDeleteMode) {
			// Delete current scene
			Debug.Log("Deleting scene " + gm.currentScene);
			isDeleteMode = false;
			if (gm.currentScene <= PlayerPrefs.GetInt("scenesCount"))
				RM_SaveLoad.DeleteScene(gm.currentScene);
			else
				Debug.Log("Cancelled new scene creation");
			// Navigate to the preceding scene
			targetScene = gm.currentScene - 1;
			if (targetScene < 1) targetScene = 1;
		}
		else if (gm.currentScene == targetScene) {
			// Clicking on current scene (shouldn't happen normally)
			CloseDialog();
			return;
		}

		// Navigate to the target scene
		PlayerPrefs.SetInt("currentScene", targetScene);
		gm.currentScene = targetScene;
		gm.Reset();
		CloseDialog();
	}

	private void CloseDialog(){
		SetLayouts(gm.mainLayout);
		UnsetLayouts(gm.warningLayout);
	}
}
