using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityReusables.Utils.Extensions;

public class PrefabSpawner : MonoBehaviour
{
    public bool useList;
    [ShowIf("useList")] public List<GameObject> prefabs = new List<GameObject>();
    [HideIf("useList")] public GameObject prefab;
    
    public Transform parent;
    public Transform target;
    public bool useTargetPosition;
    public bool useTargetRotation;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    
    public bool onStart;
    public bool usePoolingSystem;
    [ShowIf("onStart")] 
    [Tooltip("-1 for infinite instantiation until stoppped by StopInstantiate()")]
    public int startCount;
    [ShowIf("onStart")]
    [Tooltip("Duration between two instantiations")]
    public float startRate;
    
    public bool instanceSelfDestruct;
    [ShowIf("instanceSelfDestruct")] public float selfDestructDelay = 1f;
    
    private bool stopped;
    private int counter;

    private void Start()
    {
        if (onStart) StartSpawning(startCount, startRate);
    }

    [Button]
    public void StartSpawning(int count, float rate) => StartCoroutine(MultipleSpawn(count, rate));

    public void StopSpawning()
    {
        stopped = true;
        StopAllCoroutines();
    }

    [Button]
    public void SingleSpawn()
    {
        GameObject instance;
        var pref = useList ? prefabs.GetRandom() : prefab;

        Vector3 pos = positionOffset;
        if (useTargetPosition) pos += target.position;

        Quaternion rot = Quaternion.Euler(rotationOffset);
        if (useTargetRotation) rot *= target.rotation;

        if (usePoolingSystem)
        {
            instance = PrefabPoolingSystem.Spawn(pref, pos, rot);
            instance.transform.parent = parent;
        }
        else
            instance = Instantiate(pref, pos, rot, parent);

        if (instanceSelfDestruct) StartCoroutine(SelfDestruction(instance));
    }

    IEnumerator MultipleSpawn(int count, float rate)
    {
        counter = 0;
        if (count == -1) count = Int32.MaxValue;
        while (!stopped && counter < count)
        {
            SingleSpawn();
            counter++;
            yield return new WaitForSeconds(rate);
        }
    }

    IEnumerator SelfDestruction(GameObject instance)
    {
        yield return new WaitForSeconds(selfDestructDelay);
        if (usePoolingSystem) 
            PrefabPoolingSystem.Despawn(instance);
        else 
            Destroy(instance);
    }
}
