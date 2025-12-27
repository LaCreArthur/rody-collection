using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.Utils.Extensions;

public class ObjectGridInstantiator : MonoBehaviour
{
    public GameObject[] prefabs;
    public float spawnPosNoise;
    public int row, column, height;
    public bool onStart;
    public bool randomRotation = true;
    public bool useTransformStartPos;
    [HideIf("useTransformStartPos")]
    public Vector3 startPos; 
    public Vector3 gridSize;

    List<GameObject> instances = new List<GameObject>();
    
    void Start()
    {
        if (useTransformStartPos) startPos = transform.position;
        if (onStart) Instantiate();
    }

    [Button]
    public void Instantiate()
    {
        for (int i = 0; i < column; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < row; k++)
                {
                    var pos = startPos + new Vector3(i * gridSize.x, j * gridSize.y, k * gridSize.z);
                    //pos += Random.insideUnitSphere * spawnPosNoise;
                    var obj = Instantiate(prefabs.GetRandom(), pos, randomRotation ? Random.rotation : Quaternion.identity);
                    instances.Add(obj);
                }
            }
        }
    }

    bool isQuitting;

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    void OnDestroy()
    {
        if (isQuitting) return;
        foreach (var instance in instances) Destroy(instance);
        instances.Clear();
    }
}
