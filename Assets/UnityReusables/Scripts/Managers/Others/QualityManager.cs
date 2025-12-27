using UnityEngine;
#if UNITY_PIPELINE_URP
using UnityEngine.Rendering.Universal;
#endif
using UnityReusables.ScriptableObjects.Variables;

public class QualityManager : MonoBehaviour
{
    public Camera mainCam;
    public IntVariable averageFPS;
    public int targetFPS = 55;
    public int tolerance = 15;
    public int intervalOfMesure = 5;
    public int maxSuccesiveFPS = 15;
    
    private int successiveLowFPS;
    private int successiveHighFPS;
    private int currentQualityLevel;
    private bool isPostProcess;

    private void Awake()
    {
        if (mainCam == null) mainCam = Camera.main;
    }

    private void Start()
    {
        currentQualityLevel = QualitySettings.GetQualityLevel();
#if UNITY_PIPELINE_URP
        isPostProcess = mainCam.GetUniversalAdditionalCameraData().renderPostProcessing;
#else
        isPostProcess = true; // Assume post-processing is enabled when not using URP
#endif
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        // every 5 frames, check
        if (Time.frameCount % intervalOfMesure == 0)
        {
            // if low FPS
            if (averageFPS.v < targetFPS - tolerance)
            {
                // reset high successive count
                successiveHighFPS = 0;
                // increment successive low FPS
                successiveLowFPS++;
                // if 5 time low FPS, decrease quality
                if (successiveLowFPS >= maxSuccesiveFPS)
                {
                    successiveLowFPS = 0;
                    // if already min quality, disable post process if enable
                    if (currentQualityLevel <= 0)
                    {
#if UNITY_PIPELINE_URP
                        mainCam.GetUniversalAdditionalCameraData().renderPostProcessing = false;
#endif
                        isPostProcess = false;
                        return;
                    }
                    // else reduce quality
                    currentQualityLevel--;
                    QualitySettings.SetQualityLevel(currentQualityLevel, true);
                    Debug.Log($"Quality decrease to {QualitySettings.names[currentQualityLevel]}");
                }
            }
            // if high FPS
            else
            {
                // reset low successive count
                successiveLowFPS = 0;
                // increment successive high FPS
                successiveHighFPS++;
                // if already max quality, do nothing
                if (currentQualityLevel >= QualitySettings.names.Length - 1) return;

                // if 5 time high FPS, increase quality
                if (successiveHighFPS >= maxSuccesiveFPS)
                {
                    successiveHighFPS = 0;
                    // if no post process, enable it first
                    if (!isPostProcess)
                    {
#if UNITY_PIPELINE_URP
                        mainCam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
#endif
                        isPostProcess = true;
                        return;
                    }
                    // else increase quality
                    currentQualityLevel++;
                    QualitySettings.SetQualityLevel(currentQualityLevel, true);
                    Debug.Log($"Quality increase to {QualitySettings.names[currentQualityLevel]}");
                }
            }
        }
    }
}
