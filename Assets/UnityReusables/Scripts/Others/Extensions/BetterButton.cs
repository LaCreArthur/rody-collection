
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[AddComponentMenu("UI/BetterButton", 51)]
public class BetterButton : Button {
    
#if UNITY_EDITOR
        [MenuItem("GameObject/UI/BetterButton", false, 40)]
        private static void CreateComponent(MenuCommand menuCommand) { CreateUIButton(menuCommand.context as GameObject); }
        
        public static BetterButton CreateUIButton(GameObject parent)
        {
            var go = new GameObject("BetterButton", typeof(RectTransform), typeof(Image), typeof(BetterButton));
            GameObjectUtility.SetParentAndAlign(go, parent);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name); //undo option
            var uiButton = go.GetComponent<BetterButton>();
            uiButton.targetGraphic = go.GetComponent<Image>();
            go.GetComponent<RectTransform>().pivot = new Vector2(0.5f,0.5f);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(270f,100f);
            
            CreateTextMeshProLabel(uiButton);

            Selection.activeObject = go; //select the newly created gameObject
            return uiButton;
        }

        /// <summary> [Editor Only] Creates a TextMeshPro label </summary>
        public static void CreateTextMeshProLabel(BetterButton button)
        {
            var label = new GameObject("Label (TMP)", typeof(RectTransform));
            GameObjectUtility.SetParentAndAlign(label, button.gameObject);
            RectTransform rt = label.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            
            var textMeshProLabel = label.AddComponent<TextMeshProUGUI>();
            textMeshProLabel.text = "BetterButton TMP";
            textMeshProLabel.color = Color.black;
            textMeshProLabel.alignment = TextAlignmentOptions.Center;
            
            rt.sizeDelta = Vector2.zero;
        }

        /// <summary> [Editor Only] Creates a Text label </summary>
        public static void CreateTextLabel(BetterButton button)
        {
            var label = new GameObject("Label", typeof(RectTransform));
            GameObjectUtility.SetParentAndAlign(label, button.gameObject);

            var textLabel = label.AddComponent<Text>();
            textLabel.text = "Better Button text";
            textLabel.resizeTextForBestFit = false;
            textLabel.resizeTextMinSize = 12;
            textLabel.resizeTextMaxSize = 20;
            textLabel.alignment = TextAnchor.MiddleCenter;
            textLabel.alignByGeometry = true;
            textLabel.supportRichText = true;
        }
#endif
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        //call the base event
        base.OnPointerClick(eventData);

        //Invoke better events
        _onClick.Invoke();
    }
    
    [SerializeField]
    private BetterEvent _onClick = default;

    public BetterEvent OnClick
    {
        get => _onClick;
        set => _onClick = value;
    }
}