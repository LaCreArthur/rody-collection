using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Useful to be used in callbacks & BetterEvent
 */
namespace UnityReusables.Managers
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Managers/Scene Manager")]
    public class SceneManagerSO : ScriptableObject
    {
        public void ChangeScene(int index)
        {
            SceneManager.LoadScene(index);
        }

        public void LoadScene(string name, LoadSceneMode mode)
        {
            Debug.Log($"try load scene {name}");
            if (SceneManager.GetSceneByName(name).isLoaded)
            {
                Debug.Log($"Scene {name} is already loaded");
                return;
            }

            SceneManager.LoadScene(name, mode);
        }

        public void UnloadScene(string name)
        {
            if (!SceneManager.GetSceneByName(name).isLoaded)
            {
                Debug.Log($"Scene {name} is not loaded");
                return;
            }

            SceneManager.UnloadSceneAsync(name);
        }

        public void Restart()
        {
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }
    }
}