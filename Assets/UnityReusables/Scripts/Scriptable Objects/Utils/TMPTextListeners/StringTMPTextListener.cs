using TMPro;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.ScriptableObjects.Utils
{
    [RequireComponent(typeof(TMP_Text))]
    public class StringTMPTextListener : MonoBehaviour
    {
        public StringVariable variable;
        public string prefix;
        public string suffix;
        public bool autoUpdateOnChange = true;
        private TMP_Text m_text;

        private void Start()
        {
            m_text = GetComponent<TMP_Text>();
            if (autoUpdateOnChange) variable.AddOnChangeCallback(SetText);
            SetText();
        }

        public void SetText()
        {
            m_text.text = $"{prefix}{variable.v}{suffix}";
        }

        private void OnDestroy()
        {
            if (autoUpdateOnChange) variable.RemoveOnChangeCallback(SetText);
        }
    }
}