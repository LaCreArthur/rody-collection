using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityReusables.Utils;
using UnityReusables.Utils.Extensions;

namespace UnityReusables.Scripts.Editor
{
    [CustomPropertyDrawer(typeof(MonoScriptAttribute), false)]
    public class MonoScriptPropertyDrawer : PropertyDrawer
    {
        static Dictionary<string, MonoScript> s_scriptCache;

        static MonoScriptPropertyDrawer()
        {
            s_scriptCache = new Dictionary<string, MonoScript>();
            var scripts = Resources.FindObjectsOfTypeAll<MonoScript>();
            for (int i = 0; i < scripts.Length; i++)
            {
                var type = scripts[i].GetClass();
                if (type != null && !s_scriptCache.ContainsKey(type.FullName))
                {
                    s_scriptCache.Add(type.FullName, scripts[i]);
                }
            }
        }

        bool _viewString;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                Rect r = EditorGUI.PrefixLabel(position, label);
                Rect labelRect = position;
                labelRect.xMax = r.xMin;
                position = r;
                _viewString = GUI.Toggle(labelRect, _viewString, "", "label");
                if (_viewString)
                {
                    property.stringValue = EditorGUI.TextField(position, property.stringValue);
                    return;
                }

                MonoScript script = null;
                string typeName = property.stringValue;
                if (!string.IsNullOrEmpty(typeName))
                {
                    s_scriptCache.TryGetValue(typeName, out script);
                    if (script == null)
                        GUI.color = Color.red;
                }

                script = (MonoScript) EditorGUI.ObjectField(position, script, typeof(MonoScript), false);
                if (GUI.changed)
                {
                    if (script != null)
                    {
                        var type = script.GetClass();
                        MonoScriptAttribute attr = (MonoScriptAttribute) attribute;
                        if (attr.type != null && !attr.type.IsAssignableFrom(type))
                            type = null;
                        if (type != null)
                            property.stringValue = script.GetClass().Name;
                        else
                            Debug.LogWarning("The script file " + script.name + " doesn't contain an assignable class");
                    }
                    else
                        property.stringValue = "";
                }
            }
            else
            {
                GUI.Label(position, "The MonoScript attribute can only be used on string variables");
            }
        }
    }
}