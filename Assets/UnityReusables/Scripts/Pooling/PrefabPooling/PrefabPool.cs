using System.Collections.Generic;
using UnityEngine;

public struct PoolableInstances {
    public GameObject instance;
    public IPoolableComponent[] poolableComponents;
}

public class PrefabPool
{
    Dictionary<GameObject,PoolableInstances> _activeList = new Dictionary<GameObject,PoolableInstances>();
    Queue<PoolableInstances> _inactiveList = new Queue<PoolableInstances>();
    private bool useRectTransform;

    public bool UseRectTransform
    {
        get => useRectTransform;
        set => useRectTransform = value;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        PoolableInstances data;

        // if there is an inactive go available, respawn it
        if (_inactiveList.Count > 0)
        {
            data = _inactiveList.Dequeue();
        }
        else
        {
            // instantiate a new go and add it to the list
            GameObject newGO = Object.Instantiate(prefab);
            data = new PoolableInstances
            {
                instance = newGO, poolableComponents = newGO.GetComponents<IPoolableComponent>()
            };
        }

        var spawnedGO = data.instance;
        spawnedGO.SetActive(true);
        if (useRectTransform)
        {
            spawnedGO.GetComponent<RectTransform>().anchoredPosition = position;
        }
        else spawnedGO.transform.position = position;
        spawnedGO.transform.rotation = rotation;

        foreach (var pc in data.poolableComponents)
        {
            pc.Spawned();
        }
        _activeList.Add(spawnedGO, data);
        return spawnedGO;
    }

    public bool Despawn(GameObject objToDespawn)
    {
        if (!_activeList.ContainsKey(objToDespawn))
        {
            Debug.LogError("This object is not managed by this object pool!", objToDespawn);
            return false;
        }

        PoolableInstances data = _activeList[objToDespawn];

        foreach (var pc in data.poolableComponents)
        {
            pc.Despawned();
        }

        data.instance.SetActive(false);
        _activeList.Remove(objToDespawn);
        _inactiveList.Enqueue(data);
        return true;
    }

    public void AddInstances(List<GameObject> instances)
    {
        foreach (var instance in instances)
        {
            var data = new PoolableInstances
            {
                instance = instance, poolableComponents = instance.GetComponents<IPoolableComponent>()
            };
            data.instance.SetActive(false);
            _activeList.Remove(instance);
            _inactiveList.Enqueue(data);
        }

        //Debug.Log($"added {instances.Count} instances to the inactive list");
    }
}