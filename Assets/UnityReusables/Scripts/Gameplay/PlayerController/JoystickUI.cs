using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.PlayerController
{
    public class JoystickUI : MonoBehaviour
    {
        public bool showJoystickUI;
        [ShowIf("showJoystickUI")] public GameObject outline;
        [ShowIf("showJoystickUI")] public GameObject center;
        //TODO should have a scriptable cam variable to share  
        public bool useCustomCamera;
        [ShowIf("useCustomCamera")]
        public Camera cam;
        public Vector2Variable direction;
        public BoolVariable isTouching;
        public Vector2Variable currentPos; // starting point of the joystick
        
        private RectTransform outlineRect;
        private RectTransform centerRect;
        
        void Awake()
        {
            outlineRect = outline.GetComponent<RectTransform>();
            centerRect = center.GetComponent<RectTransform>();
            if (cam == null)
                cam = Camera.main;
        }

        void OnEnable() { if (showJoystickUI) isTouching.AddOnChangeCallback(OnTouchingChange); }
        void OnDisable() { if (showJoystickUI) isTouching.RemoveOnChangeCallback(OnTouchingChange); }

        void OnTouchingChange()
        {
            if (isTouching.v) {
                currentPos.v = cam.ScreenToViewportPoint(Input.mousePosition);
                outline.SetActive(true);
                center.SetActive(true);
            }
            else
            {
                outline.SetActive(false);
                center.SetActive(false);
            }
        }

        void Update()
        {
            if (showJoystickUI && isTouching.v)
            {
                outlineRect.anchoredPosition = cam.ViewportToScreenPoint(currentPos.v) - new Vector3(128,128);
                centerRect.anchoredPosition = cam.ViewportToScreenPoint(currentPos.v) - new Vector3(32,32) + new Vector3(direction.v.x * 128f, direction.v.y * 128f);
            }
        }
    }
}