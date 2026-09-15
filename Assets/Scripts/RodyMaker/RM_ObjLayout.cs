using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class RM_ObjLayout : MonoBehaviour, IPointerDownHandler, IInitializePotentialDragHandler,
    IDragHandler, IPointerUpHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform nearView, targetView;
    public Text paddingLabel;
    public Button plusButton, minusButton, acceptButton, cancelButton;
    public RM_EditorPointer pointer;
    public RM_TooltipDisplay tooltip;

    public Func<bool> CanInteract;
    public Action OnBeginEdit, OnEndEdit;
    public bool IsEditing { get; private set; }

    const int DefaultPadding = 8;
    const float Epsilon = .001f;
    static readonly Rect Bounds = new Rect(-160, -65, 320, 130);

    RectTransform surface;
    ObjectZone model;
    Rect target, near;
    int? padding;
    bool visible, pointerActive, hovered;
    int pointerId;
    Vector2 dragStart;
    Rect gestureTarget, gestureNear;
    int? gesturePadding;

    void Awake()
    {
        surface = GetComponent<RectTransform>();
        plusButton.onClick.AddListener(() => ChangePadding(1));
        minusButton.onClick.AddListener(() => ChangePadding(-1));
        acceptButton.onClick.AddListener(Accept);
        cancelButton.onClick.AddListener(Cancel);
    }

    public void Bind(ObjectZone zone, bool show)
    {
        if (IsEditing && ReferenceEquals(model, zone) && show)
        {
            RefreshViews();
            return;
        }
        Cancel();
        model = zone;
        visible = show && zone != null;
        ReadModel();
        RefreshViews();
    }

    public void BeginEdit()
    {
        if (IsEditing || !visible || CanInteract?.Invoke() != true) return;
        ReadModel();
        IsEditing = true;
        OnBeginEdit?.Invoke();
        RefreshViews();
    }

    public void Accept()
    {
        if (!IsEditing || pointerActive || !HasArea(target) || CanInteract?.Invoke() != true) return;
        Rect originalNear = NearRect(model);
        Rect originalTarget = TargetRect(model);
        if (!near.Equals(originalNear) || !target.Equals(originalTarget))
        {
            Vector2 nearCenter = near.center;
            Vector2 relativeTarget = target.center - nearCenter;
            model.nearX = nearCenter.x;
            model.nearY = nearCenter.y;
            model.nearWidth = near.width;
            model.nearHeight = near.height;
            model.x = relativeTarget.x;
            model.y = relativeTarget.y;
            model.width = target.width;
            model.height = target.height;
            StoryRoot.Session.NotifyEdited();
            StoryRoot.FlushWorkspace();
        }
        IsEditing = false;
        ReadModel();
        RefreshViews();
        OnEndEdit?.Invoke();
    }

    public void Cancel()
    {
        if (!IsEditing) return;
        pointerActive = false;
        IsEditing = false;
        ReadModel();
        RefreshViews();
        OnEndEdit?.Invoke();
    }

    public void ChangePadding(int delta)
    {
        if (pointerActive || CanInteract?.Invoke() != true) return;
        BeginEdit();
        if (!IsEditing) return;
        padding = Mathf.Clamp((padding ?? DefaultPadding) + delta, 0, MaxPadding(target));
        if (HasArea(target)) near = Expand(target, padding.Value);
        RefreshViews();
    }

    public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        if (visible && CanInteract?.Invoke() == true) pointer.Show(this, RM_EditorPointer.Kind.Draw);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        if (!pointerActive) pointer.Clear(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || pointerActive ||
            !visible || CanInteract?.Invoke() != true || !TryPoint(eventData, out Vector2 point)) return;
        BeginEdit();
        if (!IsEditing) return;
        tooltip.Hide();
        pointerActive = true;
        pointerId = eventData.pointerId;
        dragStart = point;
        gestureTarget = target;
        gestureNear = near;
        gesturePadding = padding;
        RefreshViews();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!pointerActive || eventData.pointerId != pointerId) return;
        if (CanInteract?.Invoke() != true)
        {
            CancelGesture();
            return;
        }
        PreviewGesture(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!pointerActive || eventData.pointerId != pointerId) return;
        if (CanInteract?.Invoke() != true)
        {
            CancelGesture();
            return;
        }
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.fingerId != pointerId || touch.phase != TouchPhase.Canceled) continue;
            CancelGesture();
            return;
        }
        PreviewGesture(eventData);
        pointerActive = false;
        RefreshViews();
    }

    // uGUI sends PointerUp before EndDrag on a normal release, even outside the image.
    // An EndDrag without that release is an interrupted gesture, not an accepted preview.
    public void OnEndDrag(PointerEventData eventData)
    {
        if (pointerActive && eventData.pointerId == pointerId) CancelGesture();
    }

    void PreviewGesture(PointerEventData eventData)
    {
        if (!TryPoint(eventData, out Vector2 point))
        {
            CancelGesture();
            return;
        }
        Vector2 min = Vector2.Min(dragStart, point);
        Vector2 max = Vector2.Max(dragStart, point);
        Rect candidate = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        if (!HasArea(candidate))
        {
            RestoreGesture();
        }
        else
        {
            target = candidate;
            padding = Mathf.Min(gesturePadding ?? DefaultPadding, MaxPadding(target));
            near = Expand(target, padding.Value);
        }
        RefreshViews();
    }

    bool TryPoint(PointerEventData eventData, out Vector2 point)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(surface, eventData.position,
                eventData.pressEventCamera, out point)) return false;
        point.x = Mathf.Round(Mathf.Clamp(point.x, Bounds.xMin, Bounds.xMax));
        point.y = Mathf.Round(Mathf.Clamp(point.y, Bounds.yMin, Bounds.yMax));
        return true;
    }

    void CancelGesture()
    {
        if (!pointerActive) return;
        RestoreGesture();
        pointerActive = false;
        RefreshViews();
    }

    void RestoreGesture()
    {
        target = gestureTarget;
        near = gestureNear;
        padding = gesturePadding;
    }

    void ReadModel()
    {
        target = model == null ? default : TargetRect(model);
        near = model == null ? default : NearRect(model);
        padding = ReadPadding(target, near);
    }

    void RefreshViews()
    {
        nearView.localPosition = near.center;
        nearView.sizeDelta = near.size;
        targetView.localPosition = target.center;
        targetView.sizeDelta = target.size;
        nearView.gameObject.SetActive(visible && HasArea(near));
        targetView.gameObject.SetActive(visible && HasArea(target));
        paddingLabel.text = padding.HasValue ? "Proximité : " + padding.Value + " px" : "Proximité : libre";
        if ((hovered || pointerActive) && visible && CanInteract?.Invoke() == true)
            pointer.Show(this, RM_EditorPointer.Kind.Draw);
        else pointer.Clear(this);
        bool editable = IsEditing && !pointerActive && CanInteract?.Invoke() == true;
        int current = padding ?? DefaultPadding;
        plusButton.interactable = editable && current < MaxPadding(target);
        minusButton.interactable = editable && current > 0;
        acceptButton.interactable = editable && HasArea(target);
    }

    static Rect NearRect(ObjectZone zone) => new Rect(
        zone.nearX - zone.nearWidth * .5f, zone.nearY - zone.nearHeight * .5f,
        zone.nearWidth, zone.nearHeight);

    static Rect TargetRect(ObjectZone zone) => new Rect(
        zone.nearX + zone.x - zone.width * .5f, zone.nearY + zone.y - zone.height * .5f,
        zone.width, zone.height);

    static bool HasArea(Rect rect) => rect.width > 0 && rect.height > 0;

    static bool SameRect(Rect a, Rect b) => Mathf.Abs(a.x - b.x) <= Epsilon &&
        Mathf.Abs(a.y - b.y) <= Epsilon && Mathf.Abs(a.width - b.width) <= Epsilon &&
        Mathf.Abs(a.height - b.height) <= Epsilon;

    static Rect Expand(Rect rect, int margin) => Rect.MinMaxRect(
        Mathf.Clamp(rect.xMin - margin, Bounds.xMin, Bounds.xMax),
        Mathf.Clamp(rect.yMin - margin, Bounds.yMin, Bounds.yMax),
        Mathf.Clamp(rect.xMax + margin, Bounds.xMin, Bounds.xMax),
        Mathf.Clamp(rect.yMax + margin, Bounds.yMin, Bounds.yMax));

    static int MaxPadding(Rect rect)
    {
        if (!HasArea(rect)) return (int)Bounds.width;
        float horizontal = Mathf.Max(rect.xMin - Bounds.xMin, Bounds.xMax - rect.xMax);
        float vertical = Mathf.Max(rect.yMin - Bounds.yMin, Bounds.yMax - rect.yMax);
        return Mathf.CeilToInt(Mathf.Max(0, Mathf.Max(horizontal, vertical)));
    }

    static int? ReadPadding(Rect rect, Rect proximity)
    {
        if (!HasArea(rect)) return DefaultPadding;
        float horizontal = Mathf.Max(rect.xMin - proximity.xMin, proximity.xMax - rect.xMax);
        float vertical = Mathf.Max(rect.yMin - proximity.yMin, proximity.yMax - rect.yMax);
        int candidate = Mathf.CeilToInt(Mathf.Max(0, Mathf.Max(horizontal, vertical)) - Epsilon);
        return candidate <= MaxPadding(rect) && SameRect(Expand(rect, candidate), proximity)
            ? candidate : (int?)null;
    }

    void OnApplicationFocus(bool focused)
    {
        if (!focused) CancelGesture();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) CancelGesture();
    }

    void OnDisable()
    {
        hovered = false;
        pointerActive = false;
        IsEditing = false;
        pointer.Clear(this);
    }
}
