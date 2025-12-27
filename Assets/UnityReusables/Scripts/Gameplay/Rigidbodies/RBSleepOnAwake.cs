using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RBSleepOnAwake : MonoBehaviour
{
    void Awake() => GetComponent<Rigidbody>().Sleep();
}