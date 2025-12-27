using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
 
public class SelectGameObjectsWithMissingScripts : Editor
{
    private static List<Object> s_objectsWithDeadLinks;

    [MenuItem("Tools/My Utilities/Active GameObject Children With Missing Scripts")]
    static void SelectActiveGameObjects()
    {
        s_objectsWithDeadLinks = new List<Object>();
        CheckNullComponent(Selection.activeGameObject);
        if (s_objectsWithDeadLinks.Count > 0)
        {
            //Set the selection in the editor
            Selection.objects = s_objectsWithDeadLinks.ToArray();
        }
    }
    [MenuItem("Tools/My Utilities/Scene GameObjects With Missing Scripts")]
    static void SelectScebeGameObjects()
    {
        //Get the current scene and all top-level GameObjects in the scene hierarchy
        UnityEngine.SceneManagement.Scene currentScene = SceneManager.GetActiveScene();
        s_objectsWithDeadLinks = new List<Object>();
 
        GameObject[] currentObjects = currentScene.GetRootGameObjects();
        foreach (var currentObject in currentObjects)
        {
            CheckNullComponent(currentObject);
        }
        if (s_objectsWithDeadLinks.Count > 0)
        {
            //Set the selection in the editor
            Selection.objects = s_objectsWithDeadLinks.ToArray();
        }
        else
        {
            Debug.Log("No GameObjects in '" + currentScene.name + "' have missing scripts! Yay!");
        }
    }

    private static void CheckNullComponent(GameObject g)
    {
        //Get all components on the GameObject, then loop through them 
        Transform[] transforms = g.GetComponentsInChildren<Transform>(includeInactive:true);
        for (int i = 0; i < transforms.Length; i++)
        {
            GameObject currentGameObject = transforms[i].gameObject;
            foreach (var component in currentGameObject.GetComponents<Component>())
            {
                //If the component is null, that means it's a missing script!
                if (component == null)
                {
                    //Add the sinner to our naughty-list
                    s_objectsWithDeadLinks.Add(currentGameObject);
                    Debug.Log(currentGameObject + " has a missing script!");
                    break;
                }
            }
        }
    }
}
