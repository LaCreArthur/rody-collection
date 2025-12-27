using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PoolableParticleComponent : MonoBehaviour, IPoolableComponent
{
    ParticleSystem _ps;

    void Awake() => _ps = GetComponent<ParticleSystem>();
    public void Spawned() => _ps.Play();
    public void Despawned()
    {
        Debug.Log("despawned", this);
        _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void OnParticleSystemStopped()
    {
        Debug.Log("OnParticleSystemStopped", this);
        PrefabPoolingSystem.Despawn(gameObject);
    }
}