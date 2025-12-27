using UnityEngine;
using UnityReusables.Utils.Extensions;

[RequireComponent(typeof(Renderer))]
public class RandomMaterialComponent : MonoBehaviour
{
    public Material[] materials;
    public bool onStart;

    void Start()
    {
        if (!onStart) return;
        SetRandomMaterial();
    }

    public void SetRandomMaterial() => GetComponent<Renderer>().sharedMaterial = materials.GetRandom();
}