using TMPro;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.ScriptableObjects.Utils
{
    public class GameStateTMPTextListener : MonoBehaviour
    {
        public GameStateVariable gameState;

        public string prefix;
        public string suffix;

        private TMP_Text m_text;

        public bool isTimer;
        private float stateTime;
        private string currentState;

        private void Start()
        {
            m_text = GetComponent<TMP_Text>();
            gameState.AddOnChangeCallback(SetText);
            SetText();
        }

        private void SetText()
        {
            currentState = gameState.v.ToString();

            if (isTimer)
                stateTime = 0;
            else
                m_text.text = $"{prefix}{currentState}{suffix}";
        }

        private void Update()
        {
            if (!isTimer)
                return;
            stateTime += Time.deltaTime;
            m_text.text = $"{prefix}{currentState} ({stateTime:0.0}){suffix}";
        }

        private void OnDestroy()
        {
            gameState.RemoveOnChangeCallback(SetText);
        }
    }
}