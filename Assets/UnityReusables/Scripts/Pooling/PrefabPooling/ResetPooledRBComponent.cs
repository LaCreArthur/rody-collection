using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class ResetPooledRBComponent : MonoBehaviour, IPoolableComponent
{
    private Rigidbody _rb;

    public void Spawned()
    {
    }

    public void Despawned()
    {
        if (_rb == null) 
            _rb = GetComponent<Rigidbody>(); // lazy init
        if (_rb == null) 
            return; // no rb
        _rb.linearVelocity = _rb.angularVelocity = Vector3.zero;
    }
}