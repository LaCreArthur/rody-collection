using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace RollToInfinity
{
    ///<summary>
    /// Not my script ! Replace selected objects by a prefab, was usefull for the level design
    /// From https://github.com/redbluegames/rb-unity-tools/blob/master/Assets/RedBlueGames/RBTools/Editor%20Tools/Editor/ReplaceWithPrefab.cs
    ///</summary>
    public class ReplaceWithPrefab : EditorWindow
    {
        [SerializeField] private GameObject prefab;

        [MenuItem("Tools/RollToInfinity/Replace With Prefab")]
        static void CreateReplaceWithPrefab()
        {
            EditorWindow.GetWindow<ReplaceWithPrefab>();
        }

        private void OnGUI()
        {
            prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);

            if (GUILayout.Button("Replace"))
            {
                var selection = Selection.gameObjects;

                for (var i = selection.Length - 1; i >= 0; --i)
                {
                    var selected = selection[i];
                    var prefabType = PrefabUtility.GetPrefabAssetType(prefab);
                    GameObject newObject;

                    if (prefabType != PrefabAssetType.NotAPrefab)
                    {
                        newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    }
                    else
                    {
                        newObject = Instantiate(prefab);
                        newObject.name = prefab.name;
                    }

                    if (newObject == null)
                    {
                        Debug.LogError("Error instantiating prefab");
                        break;
                    }

                    Undo.RegisterCreatedObjectUndo(newObject, "Replace With Prefabs");
                    newObject.transform.parent = selected.transform.parent;
                    newObject.transform.localPosition = selected.transform.localPosition;
                    newObject.transform.localRotation = selected.transform.localRotation;
                    newObject.transform.localScale = selected.transform.localScale;
                    newObject.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());
                    Undo.DestroyObjectImmediate(selected);
                }
            }

            GUI.enabled = false;
            EditorGUILayout.LabelField("Selection count: " + Selection.objects.Length);
        }
    }
}
#endif