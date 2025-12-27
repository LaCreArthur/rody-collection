using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.Utils.Timing
{
    [RequireComponent(typeof(TMP_Text))]
    public class TimeDisplayer : MonoBehaviour
    {
        public FloatVariable timer;

        public bool isRefreshed;
        [ShowIf("isRefreshed")] public int refreshInterval;

        private TMP_Text _timeText;
        private float _minutes;
        private float _seconds;

        private void Start()
        {
            _timeText = GetComponent<TMP_Text>();
            DisplayTime();
        }

        private void Update()
        {
            if (!isRefreshed || UnityEngine.Time.frameCount % refreshInterval > 0) return;
            DisplayTime();
        }

        public void DisplayTime()
        {
            _minutes = Mathf.Floor(timer.v / 60);
            _seconds = timer.v % 60;
            if (_seconds > 59) _seconds = 59;
            if (_minutes < 0)
            {
                _minutes = 0;
                _seconds = 0;
            }

            _timeText.text = $"{_minutes:0}:{_seconds:00}";
        }
    }
}