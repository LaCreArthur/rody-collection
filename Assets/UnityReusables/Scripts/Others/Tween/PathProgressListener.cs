using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

[RequireComponent(typeof(DOTweenPath))]
public class PathProgressListener : MonoBehaviour
{
    public FloatVariable progression;
    private DOTweenPath DTpath;
    private bool isTracking;

    private void Awake()
    {
        DTpath = GetComponent<DOTweenPath>();
    }

    private void Start()
    {
        if (DTpath.autoPlay) TrackProgression(true);
    }

    private void Update()
    {
        if (!isTracking) return;
        progression.v = DTpath.tween.ElapsedPercentage();
    }

    [Button]
    public void TrackProgression(bool track)
    {
        isTracking = track;
    }
}
