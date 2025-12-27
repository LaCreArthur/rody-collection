using TMPro;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.Utils.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class TMPIntOverIntDisplayer : MonoBehaviour
    {
        public IntVariable value;
        public IntVariable total;
        public bool displayAsPercent;
        public bool autoUpdateOnChange;

        public int valueOffset = 0;
        public int TotalOffset = 0;
        
        private TMP_Text _text;

        private void Start()
        {
            _text = GetComponent<TMP_Text>();
            if (autoUpdateOnChange)
            {
                value.AddOnChangeCallback(DisplayScore);
                total.AddOnChangeCallback(DisplayScore);
            }
            DisplayScore();
        }

        public void DisplayScore()
        {
            int v = value.v + valueOffset;
            int t = total.v + TotalOffset;
            _text.text = displayAsPercent ? $"{v / t * 100} %" : $"{v}/{t}";
        }

        private void OnDestroy()
        {
            if (autoUpdateOnChange)
            {
                value.RemoveOnChangeCallback(DisplayScore);
                total.RemoveOnChangeCallback(DisplayScore);
            }
        }
    }
}