using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

///<summary>
/// Script to control the camera and the post-process attached to it
/// Follow the player and change the color through time
///</summary>

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public float smooth;
    // speed of the color variation
    public float colorSpeed;

    public float rotationX;
    public Vector2 sceneSize;
    public Volume postProcess;

    Vector3 _offset;
    ColorAdjustments _colorAdjustments;

    void Start()
    {
        _offset = transform.position - player.transform.position;

        // Cache the ColorAdjustments component from the Volume profile
        if (postProcess != null && postProcess.profile.TryGet(out _colorAdjustments))
        {
            _colorAdjustments.hueShift.overrideState = true;
        }
    }

    // LateUpdate is called once per frame after object calculation
    void LateUpdate()
    {
        // Follow the player with an offset
        Vector3 newPos = new Vector3(player.transform.position.x / 3.0f, player.transform.position.y, player.transform.position.z);
        transform.position = newPos + _offset;

        Quaternion newRot = Quaternion.Euler(rotationX, 0.0f, player.transform.position.x);
        transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * smooth);

        // Update the post processing hueShift
        if (_colorAdjustments != null)
        {
            _colorAdjustments.hueShift.value += Time.deltaTime * colorSpeed;
        }
    }
}
