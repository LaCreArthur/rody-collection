using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource), typeof(CanvasGroup))]
public class RA_FeedbackPanel : MonoBehaviour
{
    [SerializeField] Text messageText;
    [SerializeField] Button actionButton;
    [SerializeField] Text actionButtonLabel;
    [SerializeField] Button secondButton;
    [SerializeField] Text secondButtonLabel;
    [SerializeField] Button cancelButton;
    [SerializeField] AudioClip openingClip, closingClip;

    AudioSource audioSource;
    CanvasGroup canvasGroup;
    bool previousCursorVisible;
    Action pendingAction, pendingSecondAction, pendingCancel;

    public bool IsVisible => canvasGroup != null && canvasGroup.blocksRaycasts;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        canvasGroup = GetComponent<CanvasGroup>();
        actionButton.onClick.AddListener(OnActionClicked);
        secondButton.onClick.AddListener(OnSecondClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        SetVisible(false);
    }

    void OnDestroy()
    {
        actionButton.onClick.RemoveListener(OnActionClicked);
        secondButton.onClick.RemoveListener(OnSecondClicked);
        cancelButton.onClick.RemoveListener(OnCancelClicked);
    }

    public void ShowMessage(string message, Action onDismiss = null)
    {
        Show(message, "OK", onDismiss, null, null, null);
        cancelButton.gameObject.SetActive(false);
    }

    public void ShowConfirm(string message, string confirmLabel, Action onConfirm)
    {
        Show(message, confirmLabel, onConfirm, null, null, null);
        cancelButton.gameObject.SetActive(true);
    }

    // A cancellation callback declares a real cancel outcome. Without one the
    // two choices remain mandatory, as on an unreadable-workspace recovery error.
    public void ShowChoices(string message, string firstLabel, Action first,
        string secondLabel, Action second, Action onCancel = null)
    {
        Show(message, firstLabel, first, secondLabel, second, onCancel);
        cancelButton.gameObject.SetActive(onCancel != null);
    }

    void Show(string message, string firstLabel, Action first,
        string secondLabel, Action second, Action onCancel)
    {
        if (!IsVisible) previousCursorVisible = Cursor.visible;
        Cursor.visible = true;
        messageText.text = message;
        actionButtonLabel.text = firstLabel;
        secondButtonLabel.text = secondLabel;
        secondButton.gameObject.SetActive(secondLabel != null);
        pendingAction = first;
        pendingSecondAction = second;
        pendingCancel = onCancel;
        SetVisible(true);
        audioSource.PlayOneShot(openingClip);
    }

    public void Hide()
    {
        pendingAction = pendingSecondAction = pendingCancel = null;
        if (!IsVisible) return;
        SetVisible(false);
        Cursor.visible = previousCursorVisible;
        // The object stays active so its own AudioSource can finish the close sound.
        audioSource.PlayOneShot(closingClip);
    }

    void SetVisible(bool visible)
    {
        canvasGroup.alpha = visible ? 1 : 0;
        canvasGroup.interactable = canvasGroup.blocksRaycasts = visible;
    }

    void OnActionClicked()
    {
        var action = pendingAction;
        Hide();
        action?.Invoke();
    }

    void OnSecondClicked()
    {
        var action = pendingSecondAction;
        Hide();
        action?.Invoke();
    }

    void OnCancelClicked()
    {
        var action = pendingCancel;
        Hide();
        action?.Invoke();
    }
}
