using Sirenix.OdinInspector;
using UnityEngine;

public class BillboardComponent : MonoBehaviour
{
    Camera _camera;
    public bool customCam;
    [ShowIf("customCam")]
    public Camera cam;
    public Vector3 rotationOffset;
    
    //Orient the camera after all movement is completed this frame to avoid jittering
    void Start()
    {
        _camera = (customCam && cam != null) ? cam : Camera.main;
    }

    void LateUpdate()
    {
        var rotation = _camera.transform.rotation;
        transform.LookAt(transform.position + rotation * Vector3.forward,
            rotation * Vector3.up);

        transform.rotation *= Quaternion.Euler(rotationOffset);
    }
}