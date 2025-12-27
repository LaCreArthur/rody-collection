using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityReusables.ScriptableObjects.Variables;

[RequireComponent(typeof(Button))]
public class ButtonTogglableComponent : MonoBehaviour
{
    public Image imageToSwap;
    public bool changeColor;
    [ShowIf("changeColor")]
    public Image imageToChangeColor;
    public BoolVariable value;

    [Header("On")]
    public Sprite onSprite;
    public BetterEvent onEvents;
    [ColorPalette("$PaletteName")][ShowIf("changeColor")]
    public Color onColor;
    
    [Header("Off")]
    public Sprite offSprite;
    public BetterEvent offEvents;
    [ColorPalette("$PaletteName")][ShowIf("changeColor")]
    public Color offColor;
    
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnClick()
    {
        value.v = !value.v;
    }
    
    private void OnValueChange()
    {
        SetSprite();
        SetColor();
        var events = value.v ? onEvents : offEvents;
        events.Invoke();
    }

    private void SetSprite()
    {
        imageToSwap.sprite = value.v ? onSprite : offSprite;
    }
    
    private void SetColor()
    {
        if (changeColor)
            imageToChangeColor.color = value.v ? onColor : offColor;
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClick);
        value.AddOnChangeCallback(OnValueChange);
        SetSprite();
        SetColor();
    }

    private void OnDisable()
    {
        value.RemoveOnChangeCallback(OnValueChange);
        _button.onClick.RemoveListener(OnClick);
    }
}
