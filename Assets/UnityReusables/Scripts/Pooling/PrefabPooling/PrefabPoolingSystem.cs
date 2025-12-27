using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

public static class PrefabPoolingSystem {
    static Dictionary<GameObject,PrefabPool> s_prefabToPoolMap = new Dictionary<GameObject,PrefabPool>();
    static Dictionary<GameObject,PrefabPool> s_goToPoolMap = new Dictionary<GameObject,PrefabPool>();

    /// <summary>
    /// Use this method when loading a new scene, because all refs to pooled instances will be null
    /// </summary>
    public static void Reset() {
        s_prefabToPoolMap.Clear ();
        s_goToPoolMap.Clear ();
    }
    
    /// <summary>
    /// Use this method to prespawn numToSpawn instances and avoid instantiation on framerate critical gamreplay phases  
    /// </summary>
    /// <param name="prefab">prefab to prespawn</param>
    /// <param name="numToSpawn">number of instances to spawn</param>
    public static void Prespawn(GameObject prefab, int numToSpawn)
    {
        var spawnedObjects = new List<GameObject>();
        for (int i = 0; i < numToSpawn; i++)
        {
            spawnedObjects.Add(Spawn(prefab));
        }

        for (int i = 0; i < numToSpawn; i++)
        {
            Despawn(spawnedObjects[i]);
        }
        
        spawnedObjects.Clear();
    }
    public static void PopulateWithInstances(GameObject prefab, GameObject root)
    {
        var pool = GetOrCreatePool(prefab);
        var poolableComponent = prefab.GetComponent<IPoolableComponent>();
        var childrenComponents = root.GetComponentsInChildren(poolableComponent.GetType(), true);
        
        //Debug.Log($"found {childrenComponents.Length} children with a IPoolableComponent");
        
        List<GameObject> childrenGO = new List<GameObject>();
        childrenComponents.ForEach((component) => childrenGO.Add(component.gameObject));
        pool.AddInstances(childrenGO);
    }
    
    
    /// <summary>
    /// Spawn an instance of the prefab at position and rotation and returns it
    /// </summary>
    /// <returns>the spawned instance</returns>
    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var pool = GetOrCreatePool(prefab);

        GameObject go = pool.Spawn(prefab, position, rotation);
        s_goToPoolMap.Add(go, pool);
        return go;
    }

    private static PrefabPool GetOrCreatePool(GameObject prefab)
    {
        if (!s_prefabToPoolMap.ContainsKey(prefab))
        {
            s_prefabToPoolMap.Add(prefab, new PrefabPool());
        }

        return s_prefabToPoolMap[prefab];
    }

    /// <summary>
    /// Spawn an instance of the prefab at 0,0,0 and returns it 
    /// </summary>
    /// <returns></returns>
    public static GameObject Spawn(GameObject prefab) {
        return Spawn (prefab, Vector3.zero, Quaternion.identity);
    }
    
    /// <summary>
    /// Despawn obj if it belongs to a pool 
    /// </summary>
    /// <param name="obj">the instance to despawn</param>
    /// <returns>returns true if the object was successfully despawned</returns>
    public static bool Despawn(GameObject obj) {
        if (!s_goToPoolMap.ContainsKey(obj)) {
            Debug.LogError ($"Object {obj.name} not managed by pool system!");
            return false;
        }
        PrefabPool pool = s_goToPoolMap[obj];
        
        if (pool.Despawn (obj)) {
            s_goToPoolMap.Remove (obj);
            return true;
        }
        return false;
    }
}