using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityReusables.ScriptableObjects.Variables;

public class UILevelManager : MonoBehaviour
{
    public IntVariable currentLvl;
    [Range(1,5)][OnValueChanged("InitializeUILevels")]
    public int displayedLevelCount;
    
    public Color pastLvlColor;
    public Color currentLvlColor;
    public Color nextLvlColor;

    [OnValueChanged("InitializeUILevels")]
    public bool isRewardDisplayed;
    [ShowIf("isRewardDisplayed")]
    public GameObject rewardPanel;

    public GameObject[] levelPanels;
    public Image[] levelImages;
    public TMP_Text[] levelTexts;
    public BoolVariable hasReward;
    public IntVariable lastClaimedReward;
    
    void OnEnable() => currentLvl.AddOnChangeCallback(SetUILevels);
    void OnDisable() => currentLvl.RemoveOnChangeCallback(SetUILevels);
    void Start() => InitializeUILevels();

    public void InitializeUILevels()
    {
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100 + displayedLevelCount * 100, rectTransform.sizeDelta.y);
        rewardPanel.SetActive(isRewardDisplayed);
        
        for (int i = 0; i < 5; i++) levelPanels[i].SetActive(i < displayedLevelCount);
        SetUILevels();
    }

    public void SetUILevels()
    {
        // determine the 1-X current segment
        float segmentF = currentLvl.v / (float)displayedLevelCount;
        int segmentI = Mathf.FloorToInt(segmentF);
        int currentLvlInSeg = currentLvl.v % displayedLevelCount;

        if (currentLvlInSeg == 0)
        {
            segmentI--; // last level of the segment (ex: 5 in 1-5)  
            currentLvlInSeg = displayedLevelCount;
        }

        // to match array indexes
        currentLvlInSeg--;
        
        for (int i = 0; i < displayedLevelCount; i++)
        {
            levelTexts[i].text = (segmentI * displayedLevelCount + (i + 1)).ToString(); 
            levelImages[i].color = 
                currentLvlInSeg > i ? pastLvlColor :
                currentLvlInSeg == i ? currentLvlColor :
                nextLvlColor; // currentLvlInSeg < i
        }
    }

    public void OnLevelEnd()
    {
        // wait for current level to be updated
        DOVirtual.DelayedCall(0.1f, () =>
        {
            int currentLvlInSeg = currentLvl.v % displayedLevelCount;
            // if first level in seg and last claimed reward was not the current level,
            // then its a new seg begining, player has a reward 
            if (currentLvlInSeg == 0 && lastClaimedReward.v != currentLvl.v)
            {
                lastClaimedReward.v = currentLvl.v;
                hasReward.v = true;
            }
        });
    }
}
