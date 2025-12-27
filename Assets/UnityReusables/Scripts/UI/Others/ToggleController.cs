using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.Utils.UI
{
    public class ToggleController : MonoBehaviour
    {
        public bool isOn;
        public BoolVariable associatedVariable;

        public Color onColorBg;
        public Color offColorBg;

        public Image toggleBgImage;

        public GameObject handle;
        public GameObject onGO;
        public GameObject offGO;

        public float handleOffset;
        public float speed;

        public BetterEvent onOn;
        public BetterEvent onOff;
        
        private RectTransform handleTransform;
        private float handleSize;
        private float onPosX;
        private float offPosX;
        static float t;

        void Awake()
        {
            handleTransform = handle.GetComponent<RectTransform>();
            handleSize = handleTransform.sizeDelta.x;
            float toggleSizeX = GetComponent<RectTransform>().sizeDelta.x;
            onPosX = toggleSizeX / 2 - handleSize / 2 - handleOffset;
            offPosX = onPosX * -1;
            if (associatedVariable != null) isOn = associatedVariable.v;
        }
        
        public void setposition(Vector2 pos) => handleTransform.anchoredPosition = pos;

        void Start()
        {
            if (isOn)
            {
                toggleBgImage.color = onColorBg;
                handleTransform.anchoredPosition = new Vector2(onPosX, 0f);
                Debug.Log(handleTransform.anchoredPosition);
                onGO.SetActive(true);
                offGO.SetActive(false);
            }
            else
            {
                toggleBgImage.color = offColorBg;
                handleTransform.anchoredPosition = new Vector2(offPosX, 0f);
                onGO.SetActive(false);
                offGO.SetActive(true);
            }
        }

        public void InvokeCallback()
        {
            if (isOn) onOn.Invoke();
            else onOff.Invoke();
        }

        public void Switching() => Toggle();


        public void Toggle()
        {
            if (!onGO.activeSelf || !offGO.activeSelf)
            {
                onGO.SetActive(true);
                offGO.SetActive(true);
            }

            StartCoroutine(Anim());
            IEnumerator Anim()
            {
                while (t <= 1.0f)
                {
                    t += Time.deltaTime;
                    if (isOn)
                    {
                        toggleBgImage.color = SmoothColor(onColorBg, offColorBg);
                        Transparency(onGO, 1f, 0f);
                        Transparency(offGO, 0f, 1f);
                        handleTransform.anchoredPosition = SmoothMove(onPosX, offPosX);
                    }
                    else
                    {
                        toggleBgImage.color = SmoothColor(offColorBg, onColorBg);
                        Transparency(onGO, 0f, 1f);
                        Transparency(offGO, 1f, 0f);
                        handleTransform.anchoredPosition = SmoothMove(offPosX, onPosX);
                    }
                    yield return null;
                }
            
                t = 0.0f;
                isOn = !isOn;
                if (associatedVariable != null) associatedVariable.v = isOn; 
                InvokeCallback();
            }
        }


        Vector2 SmoothMove(float startPosX, float endPosX)
        {
            return new Vector2(Mathf.Lerp(startPosX, endPosX, t += speed * Time.deltaTime), 0f);
        }

        Color SmoothColor(Color startCol, Color endCol)
        {
            Color resultCol;
            resultCol = Color.Lerp(startCol, endCol, t += speed * Time.deltaTime);
            return resultCol;
        }

        void Transparency(GameObject alphaObj, float startAlpha, float endAlpha)
        {
            CanvasGroup alphaVal;
            alphaVal = alphaObj.gameObject.GetComponent<CanvasGroup>();
            alphaVal.alpha = Mathf.Lerp(startAlpha, endAlpha, t += speed * Time.deltaTime);
        }
    }
}