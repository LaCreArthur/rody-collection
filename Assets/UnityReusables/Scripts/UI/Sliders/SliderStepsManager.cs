using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;


/*
 * Activate and setup the slider steps for the current stepTotal value, update them when this value change
 * Put this on a StepContainer inside an UI Canvas
 */
namespace UnityReusables.Progression
{
    public class SliderStepsManager : MonoBehaviour
    {
        public IntVariable stepTotal;
        public IntVariable stepCurrent;
        public FloatVariable stepProgress;
        public FloatVariable sliderValue;
        public GameObject stepPrefab;

        private List<GameObject> _stepInstances;
        private float _activationThreshold;

        private void OnEnable()
        {
            _stepInstances = new List<GameObject>();
            stepTotal.AddOnChangeCallback(InitSliderSteps);
            stepProgress.AddOnChangeCallback(OnStepProgress);
        }

        private void OnStepProgress()
        {
            // one step can be one level, game logic should update progress from 0 to 1
            // if more than one step, compute slider value by adding a progress ratio
            sliderValue.v = 1f / stepTotal.v * stepCurrent.v // progress already achieved
                            + stepProgress.v / stepTotal.v; // current progress ratio
            if (stepProgress.v >= 1f)
            {
                var anims = _stepInstances[stepCurrent.v].GetComponentsInChildren<DOTweenAnimation>();
                foreach (var anim in anims)
                {
                    anim.DOPlay();
                }
            }
        }

        private void InitSliderSteps()
        {
            Debug.Log($"init step slider: stepTotal.v={stepTotal.v}");
            _stepInstances.ForEach(Destroy);
            _stepInstances.Clear();
            // start at 1 because 1-step-level doesn't have a sliderStep
            for (int i = 1; i < stepTotal.v; i++)
            {
                _stepInstances.Add(Instantiate(stepPrefab, transform));
            }
        }

        private void OnDestroy()
        {
            stepTotal.RemoveOnChangeCallback(InitSliderSteps);
            stepProgress.RemoveOnChangeCallback(OnStepProgress);
        }
    }
}