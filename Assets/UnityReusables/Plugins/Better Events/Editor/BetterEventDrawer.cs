using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BetterEventDrawer : OdinValueDrawer<BetterEventEntry>
{
    private UnityEngine.Object tmpTarget;

    protected override void DrawPropertyLayout(GUIContent label)
    {
        SirenixEditorGUI.BeginBox();
        {
            SirenixEditorGUI.BeginToolbarBoxHeader();
            {
                var rect = GUILayoutUtility.GetRect(0, 20);
                var unityObjectFieldRect = rect.AlignLeft((rect.width / 5) * 2 + 10);
                var methodSelectorRect = rect.AlignLeft((rect.width / 5) * 2 - 25).AddX((rect.width / 5)*2 + 15);
                var delayRect = rect.AlignRight((rect.width / 6)+15);
                var dInfo = this.GetDelegateInfo();

                EditorGUI.BeginChangeCheck();
                var newTarget = SirenixEditorFields.UnityObjectField(unityObjectFieldRect, dInfo.Target, typeof(UnityEngine.Object), true);
                if (EditorGUI.EndChangeCheck())
                {
                    this.tmpTarget = newTarget;
                }

                EditorGUI.BeginChangeCheck();
                var selectorText = (dInfo.Method == null || this.tmpTarget) ? "Select a method" : dInfo.Method.Name;
                var newMethod = MethodSelector.DrawSelectorDropdown(methodSelectorRect, selectorText, this.CreateSelector);
                if (EditorGUI.EndChangeCheck())
                {
                    this.CreateAndAssignNewDelegate(newMethod.FirstOrDefault());
                    this.tmpTarget = null;
                }
                
                GUIHelper.PushLabelWidth(35);
                var delayProp = Property.Children["delay"];
                var delay = (float)delayProp.ValueEntry.WeakSmartValue;
                delay = SirenixEditorFields.FloatField(delayRect, "Delay", delay);
                delayProp.ValueEntry.WeakSmartValue = delay;
                GUIHelper.PopLabelWidth();

            }
            SirenixEditorGUI.EndToolbarBoxHeader();

            // Draws the rest of the ICustomEvent, and since we've drawn the label, we simply pass along null.
            for (int i = 0; i < this.Property.Children.Count; i++)
            {
                var child = this.Property.Children[i];
                if (child.Name == "Result") continue;
                if (child.Name != "delay")
                    child.Draw();
            }
        }
        SirenixEditorGUI.EndBox();
    }

    private void CreateAndAssignNewDelegate(DelegateInfo delInfo)
    {
        var method = delInfo.Method;
        var target = delInfo.Target;
        var pTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();
        var args = new object[pTypes.Length];

        Type delegateType = null;

        if (method.ReturnType == typeof(void))
        {
            if (args.Length == 0) delegateType = typeof(Action);
            else if (args.Length == 1) delegateType = typeof(Action<>).MakeGenericType(pTypes);
            else if (args.Length == 2) delegateType = typeof(Action<,>).MakeGenericType(pTypes);
            else if (args.Length == 3) delegateType = typeof(Action<,,>).MakeGenericType(pTypes);
            else if (args.Length == 4) delegateType = typeof(Action<,,,>).MakeGenericType(pTypes);
            else if (args.Length == 5) delegateType = typeof(Action<,,,,>).MakeGenericType(pTypes);
        }
        else
        {
            pTypes = pTypes.Append(method.ReturnType).ToArray();
            if (args.Length == 0) delegateType = typeof(Func<>).MakeArrayType();
            else if (args.Length == 1) delegateType = typeof(Func<,>).MakeGenericType(pTypes);
            else if (args.Length == 2) delegateType = typeof(Func<,,>).MakeGenericType(pTypes);
            else if (args.Length == 3) delegateType = typeof(Func<,,,>).MakeGenericType(pTypes);
            else if (args.Length == 4) delegateType = typeof(Func<,,,,>).MakeGenericType(pTypes);
            else if (args.Length == 5) delegateType = typeof(Func<,,,,,>).MakeGenericType(pTypes);
        }
        
        if (delegateType == null)
        {
            Debug.LogError("Unsupported Method Type");
            return;
        }

        var del = Delegate.CreateDelegate(delegateType, target, method);
        this.Property.Tree.DelayActionUntilRepaint(() =>
        {
            this.ValueEntry.WeakSmartValue = new BetterEventEntry(del);
            GUI.changed = true;
            this.Property.RefreshSetup();
        });
    }

    private DelegateInfo GetDelegateInfo()
    {
        var value = this.ValueEntry.SmartValue;
        var del = value.Delegate;
        var methodInfo = del == null ? null : del.Method;

        UnityEngine.Object target = null;
        if (this.tmpTarget)
        {
            target = this.tmpTarget;
        }
        else if (del != null)
        {
            target = del.Target as UnityEngine.Object;
        }

        return new DelegateInfo() { Target = target, Method = methodInfo };
    }

    private OdinSelector<DelegateInfo> CreateSelector(Rect arg)
    {
        arg.xMin -= arg.width;
        var info = this.GetDelegateInfo();
        var sel = new MethodSelector(info.Target);
        sel.SetSelection(info);
        sel.ShowInPopup(arg);
        return sel;
    }
}
