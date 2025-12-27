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

	public bool isRevertMode = false;

	[FormerlySerializedAs("warningText")]
	public Text messageText;

	public void OnCancelClick(){
		Debug.Log("Warning dialog cancelled");
		ResetFlags();
		CloseDialog();
	}

	public void OnConfirmClick(){
		Debug.Log($"[RM_WarningLayout] OnConfirmClick - targetScene: {targetScene}, currentScene: {gm.currentScene}, scenesCount: {PlayerPrefs.GetInt("scenesCount")}");
		Debug.Log($"[RM_WarningLayout] Modes - Test: {isTestMode}, Delete: {isDeleteMode}, Revert: {isRevertMode}");

		if (isTestMode){
			// Test mode - load play scene
			ResetFlags();
			if (targetScene == 0)
				SceneManager.LoadScene(1);  // Title screen test
			else
				SceneManager.LoadScene(3);  // Game scene test
			return;
		}
		else if (isRevertMode) {
			// Revert mode - reload current scene without saving
			Debug.Log("Reverting scene " + gm.currentScene);
			ResetFlags();
			gm.Reset();  // Reload from saved data
			CloseDialog();
			return;
		}
		else if (isDeleteMode) {
			// Delete current scene (only from thumbnail click on scenes >= 18)
			// Verify that the clicked scene matches current scene (should always be true)
			if (targetScene != gm.currentScene) {
				Debug.LogWarning($"[RM_WarningLayout] BUG: targetScene ({targetScene}) != currentScene ({gm.currentScene})! Possible Inspector wiring issue.");
			}
			Debug.Log($"[RM_WarningLayout] Deleting scene {gm.currentScene}");
			ResetFlags();
			if (gm.currentScene <= PlayerPrefs.GetInt("scenesCount"))
				RM_SaveLoad.DeleteScene(gm.currentScene);
			else
				Debug.Log("[RM_WarningLayout] Cancelled new scene creation (scene > scenesCount)");
			// Navigate to the preceding scene
			targetScene = gm.currentScene - 1;
			if (targetScene < 1) targetScene = 1;
			Debug.Log($"[RM_WarningLayout] After delete, navigating to scene {targetScene}");
		}
		else if (gm.currentScene == targetScene) {
			// Clicking on current scene (shouldn't happen normally)
			Debug.Log($"[RM_WarningLayout] currentScene == targetScene ({targetScene}), just closing dialog");
			CloseDialog();
			return;
		}

		// Navigate to the target scene
		Debug.Log($"[RM_WarningLayout] Navigating from scene {gm.currentScene} to scene {targetScene}");

		// If navigating to a new scene, update scenesCount immediately
		int currentScenesCount = PlayerPrefs.GetInt("scenesCount");
		if (targetScene > currentScenesCount) {
			PlayerPrefs.SetInt("scenesCount", targetScene);
			Debug.Log($"[RM_WarningLayout] New scene created, scenesCount updated to {targetScene}");
		}

		PlayerPrefs.SetInt("currentScene", targetScene);
		gm.currentScene = targetScene;
		gm.Reset();
		CloseDialog();
	}

	private void ResetFlags(){
		isTestMode = false;
		isDeleteMode = false;
		isRevertMode = false;
	}

	private void CloseDialog(){
		SetLayouts(gm.mainLayout);
		UnsetLayouts(gm.warningLayout);
	}
}
