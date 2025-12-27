using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.Scripts.Editor
{
    public abstract class BaseVariableDrawer<T> : OdinValueDrawer<T> where T : RegistrableScriptableObject
    {
        private bool isCreatingAsset;
        private string name;
        protected Rect rect;
        protected T value;
        
        protected override void DrawPropertyLayout(GUIContent label)
        {
            rect = EditorGUILayout.GetControlRect();

            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            value = this.ValueEntry.SmartValue;
            
            value = EditorGUI.ObjectField(rect.AlignLeft(rect.width * 0.5f), "", value, typeof(T), false) as T;
            
            if (value == null)
            {
                if (!isCreatingAsset)
                {
                    if (GUI.Button(rect.AlignRight(rect.width * 0.5f), "Create new"))
                    {
                        isCreatingAsset = true;
                    }
                }
                else
                {
                    SirenixEditorGUI.BeginShakeableGroup();
                    name = SirenixEditorFields.TextField(label: "Name? \t Asset/Scriptable Objects/", name, GUILayout.Height(20));
                    if (Event.current.keyCode == KeyCode.Return)
                    {
                        isCreatingAsset = false;
                        Debug.Log($"Try to create {name}...");
                        if (name.Length > 0)
                        {
                            this.ValueEntry.SmartValue = CreateMyAsset(name);
                            Debug.Log($"Asset {name} created ({typeof(T)})");
                        }
                        else
                        {
                            SirenixEditorGUI.StartShakingGroup();
                        }
                    }
                    SirenixEditorGUI.EndShakeableGroup();
                }
            }
        }
        
        public static T CreateMyAsset(string name)
        {
            T asset = ScriptableObject.CreateInstance<T>();
            System.IO.Directory.CreateDirectory($"{Application.dataPath}/Scriptable Objects/");
            AssetDatabase.CreateAsset(asset, $"Assets/Scriptable Objects/{name}.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            return asset;
        }
    }
}