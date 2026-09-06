using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class RM_ButtonTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] string tooltipText;
	RM_TooltipDisplay tooltip;

	void Start() => tooltip = FindAnyObjectByType<RM_TooltipDisplay>();

	public void OnPointerEnter(PointerEventData eventData)
	{
		Selectable selectable = GetComponent<Selectable>();
		if (tooltip == null || selectable == null || !selectable.IsInteractable())
			return;

		tooltip.Show(tooltipText, transform as RectTransform);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (tooltip != null)
			tooltip.Hide();
	}

	void OnDisable()
	{
		if (tooltip != null)
			tooltip.Hide();
	}
}