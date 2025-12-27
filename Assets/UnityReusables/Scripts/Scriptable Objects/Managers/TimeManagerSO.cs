using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Managers/Time Manager")]
public class TimeManagerSO : ScriptableObject
{
    [SerializeField][ReadOnly]
    public float startingTimeScale;
    [SerializeField][ReadOnly]
    private float currentTimeScale;
    [SerializeField][ReadOnly]
    private float previousTimeScale;

    public float CurrentTimeScale
    {
        get => currentTimeScale;
        set
        {
            previousTimeScale = currentTimeScale;
            currentTimeScale = value;
            Time.timeScale = value;
        }
    }

    private void OnEnable()
    {
        startingTimeScale = currentTimeScale = Time.timeScale;
    }

    [Button(ButtonStyle.Box)]
    public void SetTimeScale(float scale)
    {
        CurrentTimeScale = scale;
    }

    [Button]
    public void ResetTimeScale()
    {
        CurrentTimeScale = startingTimeScale;
    }
    
    [Button]
    public void SetPreviousTimeScale()
    {
        CurrentTimeScale = previousTimeScale;
    }
}