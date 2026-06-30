using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RA_ActionPanel : MonoBehaviour {
	const string EditLabel = "Éditer";
	const string ExportLabel = "Exporter";
	const string ForkLabel = "Dupliquer";

	public static event Action OnEditClicked;
	public static event Action OnExportClicked;
	public static event Action OnImportClicked;
	public static event Action OnNewClicked;

	[SerializeField] Button editButton;
	[SerializeField] Button exportButton;
	[SerializeField] Button importButton;
	[SerializeField] Button newButton;

	[SerializeField] TMP_Text editLabel;
	[SerializeField] TMP_Text exportLabel;
	[SerializeField] TMP_Text importLabel;
	[SerializeField] TMP_Text newLabel;

	[SerializeField] Color enabledTextColor = Color.white;
	[SerializeField] Color disabledTextColor = new Color(0.25f, 0.25f, 0.25f, 1f);

	void OnEnable()
	{
		editButton.onClick.AddListener(() => OnEditClicked?.Invoke());
		exportButton.onClick.AddListener(() => OnExportClicked?.Invoke());
		importButton.onClick.AddListener(() => OnImportClicked?.Invoke());
		newButton.onClick.AddListener(() => OnNewClicked?.Invoke());
	}

	void OnDisable()
	{
		editButton.onClick.RemoveAllListeners();
		exportButton.onClick.RemoveAllListeners();
		importButton.onClick.RemoveAllListeners();
		newButton.onClick.RemoveAllListeners();
	}

	/// <summary>
	/// Reflects the selected slot. The one place provenance is consulted: Export is
	/// offered only for user stories, and Edit reads "Dupliquer" on a built-in.
	/// </summary>
	public void Show(bool isUser)
	{
		gameObject.SetActive(true);

		SetButton(editButton, editLabel, true);       // always: fork (built-in) or edit (user)
		SetButton(exportButton, exportLabel, isUser); // share only a user story
		SetButton(importButton, importLabel, true);
		SetButton(newButton, newLabel, true);

		editLabel.text = isUser ? EditLabel : ForkLabel;
		exportLabel.text = ExportLabel;
	}

	public void Hide() => gameObject.SetActive(false);

	void SetButton(Button button, TMP_Text label, bool enabled)
	{
		button.interactable = enabled;
		label.color = enabled ? enabledTextColor : disabledTextColor;
	}
}
