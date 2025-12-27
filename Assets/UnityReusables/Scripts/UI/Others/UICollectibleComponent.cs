using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

public class UICollectibleComponent : MonoBehaviour
{
    [Header("References")]
    public SharedCollectibleData sharedCollectibleData;

    public RectTransform spawnOrigin;
    
    [Header("Reward")]
    public bool isRewardScriptable;
    [HideIf("isRewardScriptable")]
    public int reward = 1;
    [ShowIf("isRewardScriptable")]
    public IntVariable rewardSO;
    public bool isValueOffset;
    [ShowIf("isValueOffset")] public int valueOffset;
        
    public bool isValueMultiplied;
    [ShowIf("isValueMultiplied")] public int multiple;
    
    [Header("Extra")]
    public BetterEvent onCollectFeedback;
    [SerializeField] protected bool onlyOnce;

    protected bool asBeenCollected;

    public void Collect()
    {
        if (onlyOnce && asBeenCollected) return;
        StartCoroutine(RaisePosition());
        onCollectFeedback.Invoke();
        asBeenCollected = true;
    }

    IEnumerator RaisePosition()
    {
        int r = isRewardScriptable ? rewardSO.v : reward;
        
        int total = isValueOffset ? r + valueOffset : 
                isValueMultiplied ? r * multiple : r;

        if (total > 0)
        {
            for (int i = 0; i < total; i++)
            {
                // clamp to 25 max anim event
                if (i < 25)
                {
                    if (sharedCollectibleData == null)
                    {
                        Debug.Log("sharedCollectibleData == null");
                    }
                    if (sharedCollectibleData.onUICollectEvent == null)
                    {
                        Debug.Log("sharedCollectibleData.UIPositionGainEvent == null");
                    }
                    
                    sharedCollectibleData.onUICollectEvent.Raise(spawnOrigin.position);
                    
                }
                StartCoroutine(AddReward());
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            // if negative total, revert the animation (it's a cost)
            for (int i = 0; i > total; i--)
            {
                if (i > -25)
                    sharedCollectibleData.onBuyEvent.Raise();
                sharedCollectibleData.collectibleCount.Add(-1);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    IEnumerator AddReward()
    {
        yield return new WaitForSeconds(sharedCollectibleData.rewardDelay);
        sharedCollectibleData.collectibleCount.Add(1);
    }
}