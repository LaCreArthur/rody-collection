using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityReusables.Managers.Audio_Manager;
using UnityReusables.ScriptableObjects.Events;
using UnityReusables.ScriptableObjects.Variables;

[RequireComponent(typeof(TMP_Text))]
public class TMPCountdown : MonoBehaviour
{
    [DisableIf("useStartFloatVar")]
    public bool useStartIntVar;
    [DisableIf("useStartIntVar")]
    public bool useStartFloatVar;
    [HideIf("@useStartIntVar || useStartFloatVar")]
    public int startVal;
    [ShowIf("useStartIntVar")]
    public IntVariable startValIntVar;
    [ShowIf("useStartFloatVar")]
    public FloatVariable startValFloatVar;
    
    public int finishVal;

    public float stepDuration;
    public string endingText = "GO!";
    public bool clearOnFinish;
    public bool onStart;
    public bool stylisedEnd;
    public Color stylizedColor;
    public int stylizedFontSize;

    public SimpleEventSO onFinished;
    
    private TMP_Text text;
    private bool ascending;
    private int currentVal;

    private Color defaultColor;
    private int defaultFontSize;

    private bool sfxStarted;
    
    void Start()
    {
        text = GetComponent<TMP_Text>();
        defaultColor = text.color;
        defaultFontSize = (int)text.fontSize;
        if (onStart) StartCountdown();
    }

    public void StartCountdown()
    {
        currentVal = useStartIntVar ? startValIntVar.v : useStartFloatVar ? (int)startValFloatVar.v : startVal;
        ascending = currentVal < finishVal;

        //Debug.Log($"Start a coundown from {currentVal} to {finishVal}");
        
        text.text = currentVal.ToString();
        StopAllCoroutines();
        StartCoroutine(Countdown());
    }

    public IEnumerator Countdown()
    {
        sfxStarted = false;
        while (currentVal != finishVal)
        {
            yield return new WaitForSeconds(stepDuration);
            currentVal += ascending ? 1 : -1;
            text.text = currentVal.ToString();
            text.transform.DOPunchScale(Vector3.one, 0.2f);
            if (stylisedEnd)
            {
                int remaining = Mathf.Abs(currentVal - finishVal);
                if (remaining < 4) //TODO: very specific use case could be more generic
                {
                    if (!sfxStarted)
                    {
                        AudioManager.instance.Play("Countdown");
                        sfxStarted = true;
                    }
                    text.color = stylizedColor;
                    text.fontSize = stylizedFontSize;
                }
            }
        }
        yield return new WaitForSeconds(stepDuration);
        text.text = endingText;
        // reset style for ending text
        if (stylisedEnd)
        {
            text.color = defaultColor;
            text.fontSize = defaultFontSize;
        }
        yield return new WaitForSeconds(stepDuration);
        if (clearOnFinish) text.text = "";
        onFinished.Raise();   
    }
}
