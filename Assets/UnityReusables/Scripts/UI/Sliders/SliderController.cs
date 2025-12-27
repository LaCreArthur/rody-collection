using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityReusables.ScriptableObjects.Variables;

/*
 * Class for updating a Slider with a FloatVariable
 */

namespace UnityReusables.Utils.UI
{
    [RequireComponent(typeof(Slider))]
    public class SliderController : MonoBehaviour
    {
        [InfoBox("Slider Value must be clamped to [0,1] by the game progression logic.", InfoMessageType.None)]
        public FloatVariable sliderValue;

        private Slider m_slider;

        private void Start()
        {
            m_slider = GetComponent<Slider>();
            sliderValue.AddOnChangeCallback(UpdateSlider);
            UpdateSlider();
        }

        public void UpdateSlider()
        {
            m_slider.value = sliderValue.v;
        }

        private void OnDestroy()
        {
            sliderValue.RemoveOnChangeCallback(UpdateSlider);
        }
    }
}