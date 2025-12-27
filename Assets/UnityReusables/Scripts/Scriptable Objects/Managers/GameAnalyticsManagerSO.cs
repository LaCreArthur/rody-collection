#if GAMEANALYTICS
using GameAnalyticsSDK;
#endif
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

[CreateAssetMenu(menuName = "Scriptable Objects/Managers/GameAnalytics")]
public class GameAnalyticsManagerSO : ScriptableObject
{
    [SerializeField] private bool useGA = true;
    [SerializeField] private bool debugLog = false;
    
    private void OnEnable()
    {
        #if !TINYSAUCE
        if (!useGA)
            return;

        if (debugLog) Debug.Log("Initializing GameAnalytics");
#if GAMEANALYTICS
        GameAnalytics.Initialize();
#endif
        #endif
    }

    public void InAppPurchasEvent(string currency, int amount, string itemType, string itemId, string cartType, string receipt, string signature = "")
    {
        if (!useGA)
        {
            Debug.Log("InAppPurchasEvent not tracked, GameAnalytics is disable", this);
            return;
        }
        if (debugLog) Debug.Log("New GameAnalytics InAppPurchasEvent");
#if GAMEANALYTICS
        #if UNITY_IOS
            GameAnalytics.NewBusinessEventIOS(currency, amount, itemType, itemId, cartType, receipt);
        #endif
        #if UNITY_ANDROID
            GameAnalytics.NewBusinessEventGooglePlay(currency, amount, itemType, itemId, cartType, receipt, signature);
        #endif
#endif
    }
    
    public void ProgressionEvent(GAProgressionStatus progressionStatus, string world, string level, string phase, int score)
    {
        if (!useGA)
        {
            Debug.Log("ProgressionEvent not tracked, GameAnalytics is disable", this);
            return;
        }
        
        if (debugLog) Debug.Log("New GameAnalytics ProgressionEvent");
        #if TINYSAUCE
        TinySauceEvent(progressionStatus, level, score);
        #endif
#if GAMEANALYTICS
        GameAnalytics.NewProgressionEvent(progressionStatus, world, level, phase, score);
#endif
    }
    
    public void ProgressionEventSO(GAProgressionStatus progressionStatus, string world, IntVariable level, StringVariable phase, IntVariable score)
    {
        ProgressionEvent(progressionStatus, world, level.v.ToString(), phase.v, score.v);
    }
    
    public void CustomEvent(string eventName, float eventValue)
    {
        if (!useGA)
        {
            Debug.Log("CustomEvent not tracked, GameAnalytics is disable", this);
            return;
        }
        if (debugLog) Debug.Log("New GameAnalytics CustomEvent");
#if GAMEANALYTICS
        GameAnalytics.NewDesignEvent (eventName, eventValue);
#endif
    }
    
    public void ErrorEvent(GAErrorSeverity severity, string message)
    {
        if (!useGA)
        {
            Debug.Log("ErrorEvent not tracked, GameAnalytics is disable", this);
            return;
        }
        if (debugLog) Debug.Log("New GameAnalytics ErrorEvent");
#if GAMEANALYTICS
        GameAnalytics.NewErrorEvent (severity, message);
#endif
    }

    private void TinySauceEvent(GAProgressionStatus progressionStatus, string level, int score)
    {
#if GAMEANALYTICS
        switch (progressionStatus)
        {
            case GAProgressionStatus.Start:
                TinySauce.OnGameStarted(level);
                break;
            case GAProgressionStatus.Complete:
                TinySauce.OnGameFinished(true, score, level);
                break;
            case GAProgressionStatus.Fail:
                TinySauce.OnGameFinished(false, score, level);
                break;
            default:
                Debug.Log("TinySauceEvent Unsupported GAProgressionStatus");
                break;
        }
#endif
    }

}

#if !GAMEANALYTICS
public class GAErrorSeverity {}
public enum GAProgressionStatus
{
    //Undefined progression
    Undefined = 0,
    // User started progression
    Start = 1,
    // User succesfully ended a progression
    Complete = 2,
    // User failed a progression
    Fail = 3
}
#endif
