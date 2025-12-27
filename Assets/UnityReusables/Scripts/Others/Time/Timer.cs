using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.Utils.Timing
{
    public class Timer : MonoBehaviour
    {
        public FloatVariable timer;
        public float endTime;
        public BoolVariable isRunning;
        public BetterEvent onFinish;

        private float _startTime;
        private bool _isAscending;

        private void Start()
        {
            _startTime = timer.v;
            _isAscending = timer.v < endTime;
        }

        [Button]
        public void ResetTimer()
        {
            timer.v = _startTime;
        }

        void Update()
        {
            if (!isRunning.v) return;
            timer.v = _isAscending ? timer.v + UnityEngine.Time.deltaTime : timer.v - UnityEngine.Time.deltaTime;

            if (_isAscending && timer.v > endTime || !_isAscending && timer.v < endTime)
                onFinish.Invoke();
        }
    }
}