using DG.Tweening;
using UnityEngine;

namespace UnityReusables.Managers
{
    public class InOutAnimatedComponent : MonoBehaviour
    {
        [SerializeField] protected Vector3 defaultPos;
        [SerializeField] protected BetterEvent onAnimInCompleted, onAnimOutCompleted;

        [Header("Animations In")] 
        [SerializeField] protected Vector3 animInPosOffset;
        [SerializeField] protected int animInDuration;
        [SerializeField] protected Ease animInEase;

        [Header("Animations Out")] 
        [SerializeField] protected Vector3 animOutPosOffset;
        [SerializeField] protected int animOutDuration;
        [SerializeField] protected Ease animOutEase;

        private GameObject _currentInstance;
        protected GameObject CurrentInstance
        {
            get => _currentInstance;
            set
            {
                _lastInstance = _currentInstance;
                _currentInstance = value;
            }
        }

        private GameObject _lastInstance;

        protected void AnimInCurrent()
        {
            CurrentInstance.transform.position = animInPosOffset;
            CurrentInstance.transform.DOMove(defaultPos, animInDuration).SetEase(animInEase)
                .OnComplete(() => onAnimInCompleted.Invoke());
        }

        protected void AnimOutLast()
        {
            // if no last instance, nothing to animate out
            if (_lastInstance == null) return;
            
            _lastInstance.transform.DOMove(animOutPosOffset, animOutDuration).SetEase(animOutEase)
                .OnComplete(() => onAnimOutCompleted.Invoke());
        }
    }
}