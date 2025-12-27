using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class TweenSequencer : MonoBehaviour
{
   public TweenData[] tweenDataArray;
   public bool playBackwardsAfterTriggered;
   private Sequence mySquence;
   [ReadOnly]
   [SerializeField] private bool sequenceTriggered;

   private void Awake()
   {
      mySquence = DOTween.Sequence();
      BuildSequence();
   }

   private void BuildSequence()
   {
      mySquence.Pause();
      mySquence.SetAutoKill(false);
      for (int i = 0; i < tweenDataArray.Length; i++)
      {
         var tweenData = tweenDataArray[i];
         if (tweenData.join)
         {
            mySquence.Join(tweenData.GetTween());
         }
         else
         {
            mySquence.Append((tweenData.GetTween()));
         }
      }
   }

   [Button]
   public void StartSequence()
   {
      //Debug.Log($"Start sequence on {gameObject.name}", this);
      if (!playBackwardsAfterTriggered || !sequenceTriggered)
      {
         mySquence.PlayForward();
      }
      else
      {
         mySquence.PlayBackwards();
      }

      sequenceTriggered = !sequenceTriggered;
   }

   public void PlayForward()
   {
      mySquence.PlayForward();
      sequenceTriggered = !sequenceTriggered;
   }

   public void PlayBackward()
   {
      mySquence.PlayBackwards();
      sequenceTriggered = !sequenceTriggered;
   }
}
