using UnityEngine;
using UnityReusables.Utils.Extensions;

[RequireComponent(typeof(MeshFilter))]
public class RandomMeshComponent : MonoBehaviour
{
    public Mesh[] meshes;
    public bool onStart;

    void Start()
    {
        if (!onStart) return;
        SetRandomMesh();
    }

    public void SetRandomMesh() => GetComponent<MeshFilter>().sharedMesh = meshes.GetRandom();
}