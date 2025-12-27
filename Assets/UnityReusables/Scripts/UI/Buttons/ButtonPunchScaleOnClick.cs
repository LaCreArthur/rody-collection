using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonPunchScaleOnClick : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    [Button]
    public void Play()
    {
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 6, 0.6f).SetEase(Ease.OutQuad)
            .OnComplete(() => transform.localScale = Vector3.one);
    }

    private void OnEnable() => _button.onClick.AddListener(Play);

    private void OnDisable() => _button.onClick.RemoveListener(Play);
}