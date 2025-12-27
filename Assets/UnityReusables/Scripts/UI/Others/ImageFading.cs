using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UnityReusables.Utils
{
    [RequireComponent(typeof(Image))]
    public class ImageFading : MonoBehaviour
    {
        public Ease easeIn = Ease.InOutSine;
        public Ease easeOut = Ease.InOutSine;

        private Image _image;

        private void Start() => _image = GetComponent<Image>();
        public void SetColor(Color color) => _image.color = color;
        public void FadeIn(float duration) => _image.DOFade(1.0f, duration).SetEase(easeIn);
        public void FadeOut(float duration) => _image.DOFade(0.0f, duration).SetEase(easeOut);
    }
}