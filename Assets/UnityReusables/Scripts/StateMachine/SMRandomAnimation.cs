using UnityEngine;

namespace UnityReusables.StateMachine
{
    public class SMRandomAnimation : StateMachineBehaviour
    {
        public string parameterName;
        public int randomCount;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //Debug.Log($"Enter {stateInfo.ToString()}");
            animator.SetInteger(parameterName, Random.Range(0, randomCount));
        }
    }
}