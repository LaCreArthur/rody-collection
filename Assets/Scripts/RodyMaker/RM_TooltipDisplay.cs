using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Owns and drives the shared tooltip panel.
///     Attach to the scene root. Wire tooltipPanel and tooltipPanelText in the Inspector.
/// </summary>
public class RM_TooltipDisplay : MonoBehaviour
{
    const float HorizontalPadding = 10f;
    const float VerticalPadding = 6f;
    const float MaxInnerWidth = 260f;

    public GameObject tooltipPanel;
    public Text tooltipPanelText;

    Canvas rootCanvas;
    Camera uiCamera;
    RectTransform textRect;

    void Start()
    {
        if (tooltipPanel == null || tooltipPanelText == null)
        {
            Debug.LogWarning("[RM_TooltipDisplay] Tooltip panel references are not wired");
            return;
        }

        Canvas buttonCanvas = tooltipPanel.GetComponentInParent<Canvas>();
        if (buttonCanvas == null)
        {
            Debug.LogWarning("[RM_TooltipDisplay] Cannot initialize tooltip without parent Canvas");
            return;
        }

        rootCanvas = buttonCanvas.rootCanvas != null ? buttonCanvas.rootCanvas : buttonCanvas;
        uiCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

        CanvasGroup tooltipGroup = tooltipPanel.GetComponent<CanvasGroup>();
        if (tooltipGroup != null)
        {
            tooltipGroup.blocksRaycasts = false;
            tooltipGroup.interactable = false;
        }

        Image background = tooltipPanel.GetComponent<Image>();
        if (background != null)
            background.raycastTarget = false;

        tooltipPanelText.raycastTarget = false;
        textRect = tooltipPanelText.rectTransform;
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(0f, 1f);
        textRect.pivot = new Vector2(0f, 1f);
        tooltipPanel.SetActive(false);
    }

    public void Show(string message, RectTransform target)
    {
        if (tooltipPanel == null || tooltipPanelText == null || target == null)
            return;

        tooltipPanelText.text = message;
        UpdateSize();
        UpdatePosition(target);
        tooltipPanel.SetActive(true);
    }

    public void Hide()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    void UpdatePosition(RectTransform target)
    {
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 bottomCenterWorld = (corners[0] + corners[3]) * 0.5f;
        Vector3 topCenterWorld = (corners[1] + corners[2]) * 0.5f;

        RectTransform canvasRect = rootCanvas.transform as RectTransform;
        RectTransform panelRect = tooltipPanel.transform as RectTransform;
        if (canvasRect == null || panelRect == null)
            return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);

        Vector2 belowPosition = ScreenToCanvasPoint(canvasRect, bottomCenterWorld) + new Vector2(0f, -8f);
        Vector2 abovePosition = ScreenToCanvasPoint(canvasRect, topCenterWorld) + new Vector2(0f, 8f);
        Vector2 tooltipSize = panelRect.rect.size;
        Rect canvasBounds = canvasRect.rect;

        panelRect.pivot = new Vector2(0.5f, 1f);
        Vector2 finalPosition = belowPosition;

        float belowBottom = finalPosition.y - tooltipSize.y;
        if (belowBottom < canvasBounds.yMin)
        {
            panelRect.pivot = new Vector2(0.5f, 0f);
            finalPosition = abovePosition;
        }

        float halfWidth = tooltipSize.x * 0.5f;
        finalPosition.x = Mathf.Clamp(finalPosition.x, canvasBounds.xMin + halfWidth, canvasBounds.xMax - halfWidth);

        if (panelRect.pivot.y > 0.5f)
            finalPosition.y = Mathf.Clamp(finalPosition.y, canvasBounds.yMin + tooltipSize.y, canvasBounds.yMax);
        else
            finalPosition.y = Mathf.Clamp(finalPosition.y, canvasBounds.yMin, canvasBounds.yMax - tooltipSize.y);

        panelRect.anchoredPosition = finalPosition;
    }

    void UpdateSize()
    {
        RectTransform panelRect = tooltipPanel.transform as RectTransform;
        if (panelRect == null || textRect == null)
            return;

        TextGenerationSettings settings = tooltipPanelText.GetGenerationSettings(new Vector2(MaxInnerWidth, 0f));
        float preferredWidth = tooltipPanelText.cachedTextGeneratorForLayout.GetPreferredWidth(tooltipPanelText.text, settings) / tooltipPanelText.pixelsPerUnit;
        float innerWidth = Mathf.Min(preferredWidth, MaxInnerWidth);

        settings = tooltipPanelText.GetGenerationSettings(new Vector2(innerWidth, 0f));
        float innerHeight = tooltipPanelText.cachedTextGeneratorForLayout.GetPreferredHeight(tooltipPanelText.text, settings) / tooltipPanelText.pixelsPerUnit;
        innerHeight = Mathf.Max(innerHeight, tooltipPanelText.fontSize);

        textRect.sizeDelta = new Vector2(innerWidth, innerHeight);
        textRect.anchoredPosition = new Vector2(HorizontalPadding, -VerticalPadding);
        panelRect.sizeDelta = new Vector2(innerWidth + (HorizontalPadding * 2f), innerHeight + (VerticalPadding * 2f));

        LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
    }

    Vector2 ScreenToCanvasPoint(RectTransform canvasRect, Vector3 worldPoint)
    {
        Vector2 anchoredPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(uiCamera, worldPoint),
            uiCamera,
            out anchoredPosition);
        return anchoredPosition;
    }
}
