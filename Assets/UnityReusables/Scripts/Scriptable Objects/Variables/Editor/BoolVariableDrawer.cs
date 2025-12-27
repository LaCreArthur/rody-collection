using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

namespace UnityReusables.Scripts.Editor
{
    public class BoolVariableDrawer : BaseVariableDrawer<BoolVariable>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            base.DrawPropertyLayout(label);
            if (value != null)
            {
                GUIHelper.PushLabelWidth(40);
                value.SetValue(EditorGUI.Toggle(rect.AlignCenter(rect.width * 0.2f).AddX(rect.width * 0.15f), "Value",
                    value.v));
                value.InitialValue =
                    EditorGUI.Toggle(rect.AlignRight(rect.width * 0.2f), "Initial", value.InitialValue);
                GUIHelper.PopLabelWidth();
            }
            this.ValueEntry.SmartValue = value;
        }
    }
}