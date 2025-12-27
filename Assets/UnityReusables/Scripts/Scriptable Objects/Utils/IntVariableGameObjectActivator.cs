using System;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

public class IntVariableGameObjectActivator : MonoBehaviour
{
    public GameObject[] gameObjects;
    public IntVariable variable;

    private void OnEnable() => variable.AddOnChangeCallback(UpdateActives);
    private void Start() => UpdateActives();
    private void OnDisable() => variable.RemoveOnChangeCallback(UpdateActives);

    public void UpdateActives()
    {
        for (int i = 0; i < gameObjects.Length; i++)
        {
            gameObjects[i].SetActive(i < variable.v); 
        }
    }
}
