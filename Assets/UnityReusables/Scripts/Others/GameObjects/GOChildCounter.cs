using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;
using UnityReusables.Utils.Extensions;
using UnityReusables.Utils.LayerTagDropdown;

namespace UnityReusables.Utils
{
    public class GOChildCounter : MonoBehaviour
    {
        public IntVariable childCount;

        [DisableIf("isComponentFiltered")] public bool isTagFiltered;

        [TagDropdown] [ShowIf("isTagFiltered")] [HideIf("isComponentFiltered")]
        public string tagFilter;

        [DisableIf("isTagFiltered")] public bool isComponentFiltered;

        [ShowIf("isComponentFiltered")] [HideIf("isTagFiltered")] [MonoScript]
        public string componentTypeName;

        void Start()
        {
            childCount.v = isTagFiltered ? transform.GetChildrenByTag(tagFilter).Count :
                isComponentFiltered ? transform.GetChildrenByComponent(componentTypeName).Count :
                transform.childCount;
        }
    }
}