using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class RM_ButtonTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	RM_MainLayout tooltipHost;
	string tooltipText;

	public void Configure(RM_MainLayout host, string text)
	{
		tooltipHost = host;
		tooltipText = text;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Selectable selectable = GetComponent<Selectable>();
		if (tooltipHost == null || selectable == null || !selectable.IsInteractable())
			return;

		tooltipHost.ShowTooltip(tooltipText, transform as RectTransform);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (tooltipHost != null)
			tooltipHost.HideTooltip();
	}

	void OnDisable()
	{
		if (tooltipHost != null)
			tooltipHost.HideTooltip();
	}
}