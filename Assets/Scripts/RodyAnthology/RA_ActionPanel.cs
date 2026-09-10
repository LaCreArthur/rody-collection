using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RA_ActionPanel : MonoBehaviour {
	const string EditLabel = "Éditer";
	const string SaveLabel = "Enregistrer";
	const string ForkLabel = "Dupliquer";

	public static event Action OnEditClicked;
	public static event Action OnSaveClicked;
	public static event Action OnImportClicked;
	public static event Action OnNewClicked;

	[SerializeField] Button editButton;
	[SerializeField] Button saveButton;
	[SerializeField] Button importButton;
	[SerializeField] Button newButton;
	[SerializeField] Button voicesButton;

	[SerializeField] TMP_Text editLabel;
	[SerializeField] TMP_Text saveLabel;
	[SerializeField] TMP_Text importLabel;
	[SerializeField] TMP_Text newLabel;

	[SerializeField] Color enabledTextColor = Color.white;
	[SerializeField] Color disabledTextColor = new Color(0.25f, 0.25f, 0.25f, 1f);

	void OnEnable()
	{
		editButton.onClick.AddListener(() => OnEditClicked?.Invoke());
		saveButton.onClick.AddListener(() => OnSaveClicked?.Invoke());
		importButton.onClick.AddListener(() => OnImportClicked?.Invoke());
		newButton.onClick.AddListener(() => OnNewClicked?.Invoke());
		voicesButton.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene(AppScenes.Phonemes));
	}

	void OnDisable()
	{
		editButton.onClick.RemoveAllListeners();
		saveButton.onClick.RemoveAllListeners();
		importButton.onClick.RemoveAllListeners();
		newButton.onClick.RemoveAllListeners();
		voicesButton.onClick.RemoveAllListeners();
	}

    public void Show(bool isWorkspace, bool hasWorkspace, bool ready, bool busy)
    {
        gameObject.SetActive(true);
        SetButton(editButton, editLabel, ready && !busy && (!isWorkspace || hasWorkspace));
        SetButton(saveButton, saveLabel, ready && !busy && isWorkspace && hasWorkspace);
        SetButton(importButton, importLabel, ready && !busy);
        SetButton(newButton, newLabel, ready && !busy);
        voicesButton.interactable = !busy;
        editLabel.text = isWorkspace ? EditLabel : ForkLabel;
        saveLabel.text = SaveLabel;
    }

	public void Hide() => gameObject.SetActive(false);

	void SetButton(Button button, TMP_Text label, bool enabled)
	{
		button.interactable = enabled;
		label.color = enabled ? enabledTextColor : disabledTextColor;
	}
}
