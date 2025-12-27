using UnityEngine;
using UnityReusables.ScriptableObjects.Events;

/// <summary>
/// lets you raise a vector3PEvent with the position of the gameobject it is attached to
/// </summary>
public class Vector3PEventPositionRaiser : MonoBehaviour
{
    [SerializeField] private Vector3PEventSO _event;

    public void Raise()
    {
        _event.Raise(transform.position);
    }
}
