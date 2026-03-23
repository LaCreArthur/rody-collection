using System;
using UnityEngine;
using UnityEngine.UI;

public class RA_FeedbackPanel : MonoBehaviour
{
	[SerializeField] Text messageText;
	[SerializeField] Button actionButton;
	[SerializeField] Text actionButtonLabel;
	[SerializeField] Button cancelButton;
	[SerializeField] RA_SoundManager soundManager;

	Action pendingAction;

	void OnEnable()
	{
		actionButton.onClick.AddListener(OnActionClicked);
		cancelButton.onClick.AddListener(OnCancelClicked);
		soundManager.OnFeedbackEnabled();
	}

	void OnDisable()
	{
		actionButton.onClick.RemoveAllListeners();
		cancelButton.onClick.RemoveAllListeners();
		soundManager.OnFeedbackDisabled();
	}

	/// <summary>
	/// Single "ok" button. Optional callback on dismiss.
	/// </summary>
	public void ShowMessage(string message, Action onDismiss = null)
	{
		messageText.text = message;
		actionButtonLabel.text = "ok";
		actionButton.gameObject.SetActive(true);
		cancelButton.gameObject.SetActive(false);
		pendingAction = onDismiss;
		gameObject.SetActive(true);
	}

	/// <summary>
	/// Action button (custom label) + cancel button.
	/// </summary>
	public void ShowConfirm(string message, string confirmLabel, Action onConfirm)
	{
		messageText.text = message;
		actionButtonLabel.text = confirmLabel;
		actionButton.gameObject.SetActive(true);
		cancelButton.gameObject.SetActive(true);
		pendingAction = onConfirm;
		gameObject.SetActive(true);
	}

	public void Hide()
	{
		pendingAction = null;
		gameObject.SetActive(false);
	}

	void OnActionClicked()
	{
		var action = pendingAction;
		Hide();
		action?.Invoke();
	}

	void OnCancelClicked() => Hide();
}
