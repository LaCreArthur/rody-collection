using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;
using UnityReusables.Utils.Extensions;

namespace UnityReusables.ScriptableObjects.Utils
{
    public class IntComparator : MonoBehaviour
    {
        public enum Comparator
        {
            Greater,
            GreaterOrEqual,
            Lower,
            LowerOrEqual,
            Equal,
            NotEqual
        }

        public bool isLhsScriptable;
        public bool isRhsScriptable;

        [ShowIf("isLhsScriptable"), HorizontalGroup("Group 1", LabelWidth = 20)]
        public IntVariable lhsScriptable;

        [HideIf("isLhsScriptable"), HorizontalGroup("Group 1", LabelWidth = 20)]
        public int lhsInt;

        [HorizontalGroup("Group 1", LabelWidth = 20), HideLabel]
        public Comparator comparator;

        [ShowIf("isRhsScriptable"), HorizontalGroup("Group 1", LabelWidth = 20)]
        public IntVariable rhsScriptable;

        [HideIf("isRhsScriptable"), HorizontalGroup("Group 1", LabelWidth = 20)]
        public int rhsInt;

        [Space] public bool isResultScriptable;
        [ShowIf("isResultScriptable")] public BoolVariable resultScriptable;
        [HideIf("isResultScriptable")] public BetterEvent onTrue;
        [HideIf("isResultScriptable")] public BetterEvent onFalse;

        private int lhs, rhs;
        private bool currentResult;
        private bool hasChanged;

        private void Start()
        {
            lhsScriptable.Ref()?.AddOnChangeCallback(SetLhs);
            rhsScriptable.Ref()?.AddOnChangeCallback(SetRhs);
            SetLhs();
            SetRhs();
        }

        private void SetLhs()
        {
            lhs = isLhsScriptable ? lhsScriptable.v : lhsInt;
            OnValueChanged();
        }

        private void SetRhs()
        {
            rhs = isRhsScriptable ? rhsScriptable.v : rhsInt;
            OnValueChanged();
        }

        private bool Compare()
        {
            switch (comparator)
            {
                case Comparator.Greater:
                    return IsGreater();
                case Comparator.GreaterOrEqual:
                    return IsGreater() || IsEqual();
                case Comparator.Lower:
                    return IsLower();
                case Comparator.LowerOrEqual:
                    return IsLower() || IsEqual();
                case Comparator.Equal:
                    return IsEqual();
                case Comparator.NotEqual:
                    return !IsEqual();
                default:
                    throw new ArgumentOutOfRangeException(nameof(comparator), comparator, null);
            }
        }

        private void OnValueChanged()
        {
            bool newResult = Compare();
            hasChanged = newResult != currentResult;
            currentResult = newResult;

            if (!hasChanged) return;
            if (isResultScriptable)
            {
                resultScriptable.v = newResult;
            }
            else
            {
                if (newResult) onTrue.Invoke();
                else onFalse.Invoke();
            }
        }

        private bool IsEqual()
        {
            return lhs == rhs;
        }

        private bool IsGreater()
        {
            return lhs > rhs;
        }

        private bool IsLower()
        {
            return lhs < rhs;
        }

        private void OnDestroy()
        {
            lhsScriptable.Ref()?.RemoveOnChangeCallback(SetLhs);
            rhsScriptable.Ref()?.RemoveOnChangeCallback(SetRhs);
            lhsScriptable.Ref()?.RemoveOnChangeCallback(OnValueChanged);
            rhsScriptable.Ref()?.RemoveOnChangeCallback(OnValueChanged);
        }
    }
}