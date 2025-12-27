using System;
using UnityEngine;

namespace UnityReusables.Utils.Extensions
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MonoScriptAttribute : PropertyAttribute
    {
        public Type type;
    }
}