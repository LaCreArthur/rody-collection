using System.Collections.Generic;
using UnityEngine;

namespace UnityReusables.ScriptableObjects.Variables
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Collections/List of List of GameObject")]
    public class ListListGameObjectVariable : BaseVariable<List<List<GameObject>>>
    {
    }
}