using DG.Tweening;
#if MOREMOUNTAINS_NICEVIBRATIONS
using MoreMountains.NiceVibrations;
#elif NICEVIBRATIONS_INSTALLED
using Lofelt.NiceVibrations;
#endif
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityReusables.Utils.Extensions;
using Random = UnityEngine.Random;

public class CollectibleIMGPoolingController : MonoBehaviour
{
    [SerializeField] GameObject _collectiblePrefab = default;
    [SerializeField] private Camera mainCam = default;
    [SerializeField] private GameObject target;
    public float moveTime;
    [SerializeField] private Ease moveEase = default;
    public bool randomSpawnMove;
    [HideIf("randomSpawnMove")] 
    public Vector2 spawnMoveOffset;
    [ShowIf("randomSpawnMove")] 
    public Vector2 randomSpawnMoveOffset;
    
    private RectTransform targetRectTrans;

    void Start() => targetRectTrans = target.GetComponent<RectTransform>();

    public void SpawnFromWorldPos(Vector3 pos) => SpawnAtScreenPos(mainCam.WorldToScreenPoint(pos));

    public void SpawnFromUIPos(Vector3 pos) => SpawnAtScreenPos(pos);

    private void SpawnAtScreenPos(Vector3 pos)
    {
        #if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.Haptic(HapticTypes.RigidImpact);
        #elif NICEVIBRATIONS_INSTALLED
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.RigidImpact);
        #endif

        // spawn a collectible at worldPos
        var collectible = PrefabPoolingSystem.Spawn(_collectiblePrefab, pos, Quaternion.identity);
        
        // if new gameobject, set its parent to this rect transform, or it will not be visible
        if (collectible.transform.parent == null) 
            collectible.transform.SetParent(this.transform, false);
        
        // set a scale tween and a fade in tween
        var ColRectTrans = collectible.GetComponent<RectTransform>();
        ColRectTrans.DOScale(Vector2.one * 1.5f, moveTime).From().SetEase(moveEase);
        collectible.GetComponent<Image>().DOFade(0, moveTime/2).From().SetEase(moveEase);

        var moveOffset = randomSpawnMove ? 
            new Vector2(randomSpawnMoveOffset.RandomInside(), randomSpawnMoveOffset.RandomInside()) : 
            spawnMoveOffset + new Vector2(Random.Range(-10,10), Random.Range(-10,10));
        
        ColRectTrans.DOAnchorPos(ColRectTrans.anchoredPosition + moveOffset, moveTime / 2)
            .OnComplete(() =>
                ColRectTrans.DOAnchorPos(targetRectTrans.anchoredPosition, moveTime / 2).SetEase(moveEase)
                .OnComplete(() =>
                {
                    PrefabPoolingSystem.Despawn(collectible);
                    targetRectTrans.DOScale(Vector3.one * 1.3f, 0.3f).SetEase(Ease.OutBounce)
                        .OnComplete(() => targetRectTrans.DOScale(Vector3.one, 0.3f).SetEase(Ease.InBounce));
                }
            )
        );
    }

    public void SpawnToScreenPos()
    {
#if MOREMOUNTAINS_NICEVIBRATIONS
        MMVibrationManager.Haptic(HapticTypes.RigidImpact);
#elif NICEVIBRATIONS_INSTALLED
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.RigidImpact);
#endif
        
        // spawn at zero, need to get the rect transform to set position right
        var collectible = PrefabPoolingSystem.Spawn(_collectiblePrefab);
        
        // if new gameobject, set its parent to this rect transform, or it will not be visible
        if (collectible.transform.parent == null) 
            collectible.transform.SetParent(this.transform, false);
        
        // set a scale tween and a fade in tween
        var ColRectTrans = collectible.GetComponent<RectTransform>();
        // set starting pos to Collectible pos
        ColRectTrans.position = targetRectTrans.position;
        ColRectTrans.anchoredPosition = targetRectTrans.anchoredPosition;
        
        ColRectTrans.DOScale(Vector2.one * 1.5f, moveTime).From().SetEase(moveEase);
        collectible.GetComponent<Image>().DOFade(0, moveTime).From().SetEase(moveEase);

        var moveOffset =
            spawnMoveOffset + new Vector2(Random.Range(-50,50), Random.Range(-50,50));
        
        targetRectTrans.DOScale(Vector3.one * 1.3f, 0.3f).SetEase(Ease.OutBounce)
            .OnComplete(() => targetRectTrans.DOScale(Vector3.one, 0.3f).SetEase(Ease.InBounce));
        // random burst move
        ColRectTrans.DOAnchorPos(targetRectTrans.anchoredPosition + moveOffset, moveTime / 2).OnComplete(() =>
            ColRectTrans.DOScale(Vector3.zero, moveTime/2).SetEase(moveEase)).OnComplete (() =>
        {
            ColRectTrans.localScale = Vector3.one;
            PrefabPoolingSystem.Despawn(collectible);
        });
    }

    public void ChangeTarget(GameObject newTarget)
    {
        target = newTarget;
        targetRectTrans = target.GetComponent<RectTransform>();
    }
}