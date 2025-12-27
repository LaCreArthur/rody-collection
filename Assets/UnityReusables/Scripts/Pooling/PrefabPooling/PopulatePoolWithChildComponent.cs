using Sirenix.OdinInspector;
using UnityEngine;

public class PopulatePoolWithChildComponent : MonoBehaviour
{
    [SerializeField] GameObject _prefab = default;
    void Awake()
    {
        PrefabPoolingSystem.PopulateWithInstances(_prefab, gameObject);
    }

    [Button]
    public void TestSpawn()
    {
        if (Application.isPlaying) 
            PrefabPoolingSystem.Spawn(_prefab);
        else 
            Debug.Log("Application must be playing to spawn prefab");
    }
}
