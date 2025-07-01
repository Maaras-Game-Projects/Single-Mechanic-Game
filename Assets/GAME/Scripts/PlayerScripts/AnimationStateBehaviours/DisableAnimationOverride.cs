using UnityEngine;

namespace EternalKeep
{
    public class DisableAnimationOverride : StateMachineBehaviour
    {
        PlayerAnimationManager playerAnimationManager;
        //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            playerAnimationManager = animator.GetComponent<PlayerAnimationManager>();
            playerAnimationManager.DisableAnimationOverride();
        }


    }

}


