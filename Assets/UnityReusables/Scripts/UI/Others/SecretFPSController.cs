using System;
using System.Collections;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

public class SecretFPSController : MonoBehaviour
{
    public GameObject FPSPanel;
    public BoolVariable isFPSPanelActive;
    private int clickCount;
    
    private void Start()
    {
        clickCount = 0;
        FPSPanel.SetActive(isFPSPanelActive.v);
    }

    public void OnClick()
    {
        clickCount++;
        Debug.Log("clickcount : " + clickCount);
        // stop previous 1s timer
        StopAllCoroutines();
        // start a new 1s timer
        StartCoroutine(ResetCount());
        if (clickCount > 5)
        {
            isFPSPanelActive.v = !isFPSPanelActive.v;
            Debug.Log("set FPS Panel to " + isFPSPanelActive.v);
            // revert active state
            FPSPanel.SetActive(isFPSPanelActive.v);
            clickCount = 0;
        }
    }

    private IEnumerator ResetCount()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("click count resetted");
        clickCount = 0;
    }
}
