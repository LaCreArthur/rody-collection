// #if dUI_MANAGER
// using Doozy.Engine;
// #endif
// using UnityEngine;
// using UnityReusables.ScriptableObjects.Events;
// using UnityReusables.ScriptableObjects.Variables;
//
// namespace UnityReusables.Managers
// { 
//     public class StepManager : InOutAnimatedComponent
//     {
//         [SerializeField] private IntVariable playerStepTotal = default;
//         [SerializeField] private IntVariable playerStepCurrent = default;
//         [SerializeField] private SimpleEvenSO onStepTransitionStart = default;
//         [SerializeField] private SimpleEvenSO onStepTransitionEnd = default;
//         [SerializeField] private SimpleEvenSO onLevelEnd = default;
//
//         private GameObject _currentLevelInstance;
//         private StepComponent[] _steps = default;
//         private StepComponent _currentStepComponent;
//         private StepComponent _previousStepComponent;
//
//         /// <summary>
//         /// Handle a new level
//         /// </summary>
//         /// <param name="msg">it contains the level instance</param>
//         /// <returns></returns>
//         private bool HandleNewLevel(Message msg)
//         {
//             // if (msg is NewLevelMessage castedMsg)
//             // {
//             //     _currentLevelInstance = castedMsg.level;
//             //     _steps = _currentLevelInstance.GetComponentsInChildren<StepComponent>();
//             //     if (_steps.Length == 0)
//             //     {
//             //         Debug.LogError($"HandleNewLevel failed because no StepComponent was found on the new level {_currentLevelInstance.name}");
//             //         return false;
//             //     }
//             //     playerStepTotal.v = _steps.Length;
//             //     playerStepCurrent.v = 0;
//             //     SetCurrentStep();
//             //     return true;
//             // }
//
//             Debug.LogWarning($"HandleNewLevel failed because it receive a wrongly typed message {msg.type}");
//             return false;
//         }
//
//         /// <summary>
//         /// callback for the stepCompleted Event
//         /// Setup next step or raise LevelCompleted and send UI event
//         /// </summary>
//         public void OnStepCompleted()
//         {
//             playerStepCurrent.Add(1);
//             if (playerStepCurrent.v >= playerStepTotal.v)
//             {
//                 onLevelEnd.Raise();
// #if dUI_MANAGER
//                 GameEventMessage.SendEvent("LevelEnd");
// #endif
//             }
//             else
//             {
//                 onStepTransitionStart.Raise();
// #if dUI_MANAGER
//                 GameEventMessage.SendEvent("StepTransitionStart");
// #endif
//                 SetNextStep();
//             }
//         }
//
//         /// <summary>
//         /// Setup the next step of the current Level
//         /// </summary>
//         private void SetNextStep()
//         {
//             SetLastStep();
//             SetCurrentStep();
//         }
//
//         private void SetLastStep()
//         {
//             // if no current step, nothing to do, we should not have call this function
//             if (_currentStepComponent == null)
//             {
//                 Debug.LogWarning("SetLastStep failed because _currentStepComponent is null", this);
//                 return;
//             }
//             
//             _currentStepComponent.OnStepOut();
//             if (_currentStepComponent.isAnimated)
//                 AnimOutLast();
//             
//         }
//
//         private void SetCurrentStep()
//         {
//             _currentStepComponent = _steps[playerStepCurrent.v];
//             _currentStepComponent.OnStepIn();
//
//             // if step is animated, do the animation
//             if (_currentStepComponent.isAnimated)
//             {
//                 var currentStepGO = _currentStepComponent.gameObject;
//                 currentStepGO.transform.position = animInPosOffset;
//                 currentStepGO.SetActive(true);
//                 AnimInCurrent();
//             }
//             else
//             {
//                 OnStepTransitionEnd();
//             }
//         }
//
//         public void OnStepTransitionEnd()
//         {
//             onStepTransitionEnd.Raise();
// #if dUI_MANAGER
//             GameEventMessage.SendEvent("StepTransitionEnd");
// #endif
//         }
//     }
// }