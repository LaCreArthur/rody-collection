using TMPro;
using UnityEngine;
using UnityReusables.Utils.Extensions;

namespace UnityReusables.Utils.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class TMPRandomTextAndColor : MonoBehaviour
    {
        public string[] queries;
        public Color[] colors;

        private TMP_Text text;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
            SetRandomTextAndColor();
        }

        public void SetRandomTextAndColor()
        {
            text.text = queries.GetRandom();
            text.color = colors.GetRandom();
        }
    }
}