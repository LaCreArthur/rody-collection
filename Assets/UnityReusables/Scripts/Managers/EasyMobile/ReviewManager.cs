#if EASY_MOBILE
using EasyMobile;
#endif
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

[CreateAssetMenu(menuName = "Scriptable Objects/Managers/Review")]
public class ReviewManager : ScriptableObject
{
    public int minLevelBeforeReview = 4;
    public IntVariable currentLevel;

#if EASY_MOBILE
    int levelSinceLastCheck = 0;
    private void OnEnable() => currentLevel.AddOnChangeCallback(CheckReview);
    private void OnDisable() => currentLevel.RemoveOnChangeCallback(CheckReview);

    public void CheckReview()
    {
        Debug.Log("test log Review Manager");
        levelSinceLastCheck++;
        if (levelSinceLastCheck == minLevelBeforeReview)
        {
            StoreReview.RequestRating();
            levelSinceLastCheck = 0;
        }
    }
    #endif
}
