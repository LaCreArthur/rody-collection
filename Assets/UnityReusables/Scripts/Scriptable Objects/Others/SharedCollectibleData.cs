using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityReusables.ScriptableObjects.Events;
using UnityReusables.ScriptableObjects.Variables;

[CreateAssetMenu(menuName = "Scriptable Objects/Others/SharedCollectibleData")]
public class SharedCollectibleData : SerializedScriptableObject
{
    public IntVariable collectibleCount;
    [FormerlySerializedAs("worldPositionEvent")] public Vector3PEventSO onWorldCollectEvent;
    [FormerlySerializedAs("onCollectEvent")] [FormerlySerializedAs("UIPositionGainEvent")] public Vector3PEventSO onUICollectEvent;
    public SimpleEventSO onBuyEvent;
    public float rewardDelay;
}
