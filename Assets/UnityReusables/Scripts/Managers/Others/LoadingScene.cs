using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnityReusables.Managers
{
    public class LoadingScene : MonoBehaviour
    {
        public GameObject loadingScreen;
        public int mainSceneIndex;
        public Text progressText;
        public Slider slider;

        private void Start()
        {
            loadingScreen.SetActive(true);
            StartCoroutine(LoadAsync(mainSceneIndex));
        }

        private IEnumerator LoadAsync(int sceneIndex)
        {
            var operation = SceneManager.LoadSceneAsync(sceneIndex);

            while (!operation.isDone)
            {
                var progress = Mathf.Clamp01(operation.progress / .9f);
                slider.value = progress;
                progressText.text = progress.ToString("P"); // "P" for percent format
                yield return null;
            }
        }
    }
}