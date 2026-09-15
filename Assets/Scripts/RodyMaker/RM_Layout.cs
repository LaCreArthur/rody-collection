using UnityEngine;

public abstract class RM_Layout : MonoBehaviour
{
    protected RM_GameManager gm;

    protected virtual void Awake() => gm = GameObject.Find("GameManager").GetComponent<RM_GameManager>();

}
