using System;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace UnityReusables.Utils.LayerTagDropdown
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LayerDropdownAttribute : Attribute
    {
    }

#if UNITY_EDITOR
    
    public sealed class LayerDropdownAttributeDrawer : OdinAttributeDrawer<LayerDropdownAttribute, int>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ValueEntry.SmartValue = EditorGUILayout.LayerField(label ?? new GUIContent(""), ValueEntry.SmartValue);
        }
    }

#endif
}