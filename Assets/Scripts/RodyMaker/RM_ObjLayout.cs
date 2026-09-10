using UnityEngine;
using UnityEngine.UI;

public class RM_ObjLayout : RM_Layout
{
    public InputField objInputField;
    public GameObject zoneHelper;
    public Sprite nearSprite, objSprite, validSprite;
    public Button returnBtn, phonemsBtn, zoneBtn, textBtn;

    int activeObj = 1;
    int drawState;
    Vector2 dragStart;
    RectTransform nearView, targetView;

    ObjectZone Zone => activeObj == 1 ? gm.CurrentScene.objects.obj
        : activeObj == 2 ? gm.CurrentScene.objects.ngp : gm.CurrentScene.objects.fsw;
    SpeechDocument Dialogue => activeObj == 1 ? gm.CurrentScene.dialogues.obj
        : activeObj == 2 ? gm.CurrentScene.dialogues.ngp : gm.CurrentScene.dialogues.fsw;
    string DisplayText => activeObj == 1 ? gm.CurrentScene.texts.obj
        : activeObj == 2 ? gm.CurrentScene.texts.ngp : gm.CurrentScene.texts.fsw;

    protected override void Awake()
    {
        base.Awake();
        objInputField.interactable = false;
        objInputField.onValueChanged.AddListener(SetText);
        objInputField.onEndEdit.AddListener(_ => StoryRoot.FlushWorkspace());
    }

    public void Bind(int objective)
    {
        activeObj = objective;
        objInputField.SetTextWithoutNotify(DisplayText);
        if (nearView == null)
        {
            nearView = Instantiate(gm.objNearTemplate, gm.objNearTemplate.transform.parent).GetComponent<RectTransform>();
            targetView = Instantiate(gm.objTemplate, nearView).GetComponent<RectTransform>();
        }
        RefreshZones();
        StopDrawing();
    }

    void RefreshZones()
    {
        var zone = Zone;
        nearView.localPosition = new Vector3(zone.nearX, zone.nearY, 0);
        nearView.sizeDelta = new Vector2(zone.nearWidth, zone.nearHeight);
        targetView.localPosition = new Vector3(zone.x, zone.y, 0);
        targetView.sizeDelta = new Vector2(zone.width, zone.height);
        nearView.gameObject.SetActive(true);
        targetView.gameObject.SetActive(true);
    }

    void SetText(string text)
    {
        if (!gm.CanEdit || DisplayText == text) return;
        var texts = gm.CurrentScene.texts;
        if (activeObj == 1) texts.obj = text;
        else if (activeObj == 2) texts.ngp = text;
        else texts.fsw = text;
        StoryRoot.Session.NotifyEdited();
        gm.RefreshText();
    }

    public void RM_ReturnClick()
    {
        if (!gm.CanEdit) return;
        SetLayouts(gm.objectsLayout, gm.introTextObj);
        UnsetLayouts(gm.objLayout, gm.objTextObj);
        StoryRoot.FlushWorkspace();
    }

    public void TextOnClick()
    {
        if (!gm.CanEdit) return;
        objInputField.interactable = !objInputField.interactable;
        returnBtn.interactable = phonemsBtn.interactable = zoneBtn.interactable = !objInputField.interactable;
    }

    public void ZoneOnClick()
    {
        if (!gm.CanEdit) return;
        if (drawState == 0)
        {
            drawState = 1;
            zoneHelper.SetActive(true);
            zoneHelper.GetComponent<Text>().text = "Clique et fais glisser pour dessiner la zone proche (jaune).";
            UnsetLayouts(objInputField.gameObject, gm.title);
            returnBtn.interactable = phonemsBtn.interactable = textBtn.interactable = zoneBtn.interactable = false;
            zoneBtn.GetComponent<Image>().sprite = objSprite;
        }
        else if (drawState == 3)
        {
            drawState = 4;
            zoneHelper.GetComponent<Text>().text = "Dessine la cible (verte) à l’intérieur de la zone proche (jaune).";
            targetView.gameObject.SetActive(true);
            zoneBtn.GetComponent<Image>().sprite = validSprite;
            zoneBtn.interactable = false;
        }
        else if (drawState == 6)
        {
            StopDrawing();
            StoryRoot.FlushWorkspace();
        }
    }

    void StopDrawing()
    {
        drawState = 0;
        zoneHelper.SetActive(false);
        zoneBtn.GetComponent<Image>().sprite = nearSprite;
        returnBtn.interactable = phonemsBtn.interactable = textBtn.interactable = zoneBtn.interactable = true;
        SetLayouts(objInputField.gameObject, gm.title);
    }

    void Update()
    {
        if (!gm.CanEdit || drawState == 0) return;
        Vector2 point = new Vector2(Input.mousePosition.x * 320f / Screen.width,
            Input.mousePosition.y * 200f / Screen.height) - new Vector2(160, 65);
        bool drawing = drawState == 2 || drawState == 5;
        bool near = drawState <= 3;
        Rect bounds = near ? new Rect(-160, -65, 320, 130)
            : new Rect(nearView.localPosition.x - nearView.sizeDelta.x * .5f,
                nearView.localPosition.y - nearView.sizeDelta.y * .5f, nearView.sizeDelta.x, nearView.sizeDelta.y);

        if (!drawing && Input.GetMouseButtonDown(0) && bounds.Contains(point))
        {
            dragStart = point;
            drawState = near ? 2 : 5;
            drawing = true;
            zoneBtn.interactable = false;
            if (near) targetView.gameObject.SetActive(false);
        }
        if (!drawing) return;

        point = new Vector2(Mathf.Clamp(point.x, bounds.xMin, bounds.xMax), Mathf.Clamp(point.y, bounds.yMin, bounds.yMax));
        RectTransform view = near ? nearView : targetView;
        view.sizeDelta = Vector2.Max(dragStart, point) - Vector2.Min(dragStart, point);
        Vector2 center = (dragStart + point) * .5f;
        view.localPosition = near ? (Vector3)center : (Vector3)(center - (Vector2)nearView.localPosition);
        if (!Input.GetMouseButtonUp(0)) return;
        if (view.sizeDelta.x == 0 || view.sizeDelta.y == 0)
        {
            drawState = near ? 1 : 4;
            return;
        }
        if (!near) CommitZones();
        drawState = near ? 3 : 6;
        zoneBtn.interactable = true;
        zoneHelper.GetComponent<Text>().text = near
            ? "Redessine la zone proche ou clique sur le bouton pour placer la cible."
            : "Redessine la cible ou clique sur le bouton pour terminer.";
    }

    // Near and target form one accepted edit. Leaving midway keeps the previous pair.
    void CommitZones()
    {
        var zone = Zone;
        var nearPosition = nearView.localPosition;
        var nearSize = nearView.sizeDelta;
        var position = targetView.localPosition;
        var size = targetView.sizeDelta;
        if (zone.nearX == nearPosition.x && zone.nearY == nearPosition.y &&
            zone.nearWidth == nearSize.x && zone.nearHeight == nearSize.y &&
            zone.x == position.x && zone.y == position.y && zone.width == size.x && zone.height == size.y) return;
        zone.nearX = nearPosition.x;
        zone.nearY = nearPosition.y;
        zone.nearWidth = nearSize.x;
        zone.nearHeight = nearSize.y;
        zone.x = position.x;
        zone.y = position.y;
        zone.width = size.x;
        zone.height = size.y;
        StoryRoot.Session.NotifyEdited();
        StoryRoot.FlushWorkspace();
    }

    public void RM_PhonemesClick()
    {
        if (!gm.CanEdit) return;
        phonemsBtn.interactable = false;
        SynthManager.Open(Dialogue, gm.CurrentScene.voice.isZambla ? 0.9f : 1f, false, "MASTICO · CONSIGNE",
            (speech, _) =>
            {
                if (SameSpeech(Dialogue, speech)) return;
                var dialogues = gm.CurrentScene.dialogues;
                if (activeObj == 1) dialogues.obj = speech;
                else if (activeObj == 2) dialogues.ngp = speech;
                else dialogues.fsw = speech;
                StoryRoot.Session.NotifyEdited();
                StoryRoot.FlushWorkspace();
            }, () => phonemsBtn.interactable = true);
    }

    void OnDisable()
    {
        if (nearView != null) nearView.gameObject.SetActive(false);
        drawState = 0;
    }
}
