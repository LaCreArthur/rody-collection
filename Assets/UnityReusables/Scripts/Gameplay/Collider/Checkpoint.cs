using UnityEngine;
using UnityReusables.ScriptableObjects.Events;
using UnityReusables.Utils.Extensions;

public class Checkpoint : MonoBehaviour
{
   public LayerMask playerLayer;
   public SimpleEventSO checkpointReached;

   private void OnTriggerEnter(Collider other)
   {
      if (playerLayer.MatchWith(other.gameObject.layer))
      {
         checkpointReached.Raise();
         gameObject.SetActive(false);
      }
   }
}
