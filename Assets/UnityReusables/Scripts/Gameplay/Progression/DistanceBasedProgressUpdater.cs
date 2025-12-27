using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

// Update a FloatVariable progress based on this transform distance to arrival
// Should be attached to the player or object that needs to reach the arrival
namespace UnityReusables.Progression
{
    public class DistanceBasedProgressUpdater : MonoBehaviour
    {
        public bool autoStartInGame;
        [ShowIf("autoStartInGame")] public GameStateVariable gameState;

        public Vector3Variable arrival;
        public bool autoStartOnArrivalChange;
        
        public FloatVariable progress;

        // number of frames between each update
        public int refreshRate = 3;

        public bool only1D;
        [ShowIf("only1D")] public bool xOnly;
        [ShowIf("only1D")] public bool yOnly;
        [ShowIf("only1D")] public bool zOnly;


        private Vector3 _arrivalPos;
        private float _startDist;
        private float _currDist;
        private bool _isStarted;

        private void OnEnable()
        {
            if (autoStartInGame)
                gameState.AddOnChangeCallback(StartDistanceProgress);
            if (autoStartOnArrivalChange)
                arrival.AddOnChangeCallback(StartDistanceProgress);
        }
        
        private void OnDisable()
        {
            if (autoStartInGame)
                gameState.RemoveOnChangeCallback(StartDistanceProgress);
            if (autoStartOnArrivalChange)
                arrival.RemoveOnChangeCallback(StartDistanceProgress);
        }

        // to be call for each new level (or step)
        [Button]
        public void StartDistanceProgress()
        {
            var tmpArrival = arrival.v;
            _arrivalPos = only1D
                ? new Vector3(xOnly ? tmpArrival.x : 0, yOnly ? tmpArrival.y : 0, zOnly ? tmpArrival.z : 0)
                : arrival.v;
            _startDist = Vector3.SqrMagnitude(_arrivalPos - GetCurrentDist());
            _currDist = _startDist;
            
            _isStarted = true;
        }

        private Vector3 GetCurrentDist()
        {
            var tmpPos = transform.position;
            return only1D
                ? new Vector3(xOnly ? tmpPos.x : 0, yOnly ? tmpPos.y : 0, zOnly ? tmpPos.z : 0)
                : tmpPos;
        }

        private void Update()
        {
            if (!_isStarted) return;
            // limit refresh timer to save perf
            if (Time.frameCount % refreshRate != 0) return;

            progress.v = 1 - _currDist / _startDist;
            _currDist = Vector3.SqrMagnitude(_arrivalPos - GetCurrentDist());
        }
    }
}