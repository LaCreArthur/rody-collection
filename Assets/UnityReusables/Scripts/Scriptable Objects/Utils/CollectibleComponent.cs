using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;
using UnityReusables.Utils.LayerTagDropdown;

[RequireComponent(typeof(Collider))][SelectionBaseAttribute]
public class CollectibleComponent : MonoBehaviour
{
    public SharedCollectibleData sharedCollectibleData;
    public bool isRewardScriptable;
    [HideIf("isRewardScriptable")]
    public int reward = 1;
    [ShowIf("isRewardScriptable")]
    public IntVariable rewardSO;
    
    public BetterEvent onCollectFeedback;

    [SerializeField, TagDropdown] protected string otherTag = "";
    [SerializeField] protected bool onlyOnce;
    public bool useCollectiblePosition;
    [ShowIf("onlyOnce")]


    protected bool asBeenCollected;

    private void OnTriggerEnter(Collider other)
    {
        if (onlyOnce && asBeenCollected) return;
        if (other.CompareTag(otherTag))
        {
            StartCoroutine(RaisePosition(useCollectiblePosition ? gameObject : other.gameObject));
            onCollectFeedback.Invoke();
            asBeenCollected = true;
        }
    }

    IEnumerator RaisePosition(GameObject other)
    {
        int r = isRewardScriptable ? rewardSO.v : reward;
        for (int i = 0; i < r; i++)
        {
            sharedCollectibleData.onWorldCollectEvent.Raise(other.transform.position);
            StartCoroutine(AddReward());
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator AddReward()
    {
        yield return new WaitForSeconds(sharedCollectibleData.rewardDelay);
        sharedCollectibleData.collectibleCount.Add(1);
    }
}
