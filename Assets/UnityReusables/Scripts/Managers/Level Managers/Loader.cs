using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
    public bool useSlider;
    [ShowIf("useSlider")]
    public Slider slider;

    public bool useImageFilled;
    [ShowIf("useImageFilled")]
    public Image image;
    
    public TMP_Text progressText;

    private void Start()
    {
        StartCoroutine(LoadAsync(1));
    }

    IEnumerator LoadAsync(int sceneIndex)
    {
        yield return new WaitForEndOfFrame();
        var operation = SceneManager.LoadSceneAsync(sceneIndex);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            if (useSlider)
                slider.value = progress;
            if (useImageFilled)
                image.fillAmount = progress;
            
            progressText.text = progress.ToString("P0"); // "P" for percent format
            yield return null;
        }
    }
}