using System.Collections;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

[RequireComponent(typeof(CollectibleIMGPoolingController))]
public class KeyCollectibleIMGController : MonoBehaviour
{
    public IntVariable keyCount;
    public GameObject[] targets;
    public GameObject[] keys;
    
    private CollectibleIMGPoolingController _cipc;

    private void Start()
    {
        _cipc = GetComponent<CollectibleIMGPoolingController>();
    }

    public void SpawnAtScreenPos(Vector3 worldPos)
    {
        int index = keyCount.v;
        if (index >= targets.Length)
        {
            Debug.LogWarning("keyCount is greater than the key slots !");
            keyCount.Add(-1);
            index--;
        }
        _cipc.ChangeTarget(targets[keyCount.v]);
        _cipc.SpawnFromWorldPos(worldPos);
        //StartCoroutine(DisplayKey(index));
    }

    IEnumerator DisplayKey(int index)
    {
        yield return new WaitForSeconds(_cipc.moveTime);
        keys[index].SetActive(true);
    }
}