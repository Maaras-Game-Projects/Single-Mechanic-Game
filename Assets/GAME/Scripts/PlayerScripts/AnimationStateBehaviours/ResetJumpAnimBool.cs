using UnityEngine;

namespace EternalKeep
{
    public class ResetJumpAnimBool : StateMachineBehaviour
    {
        private PlayerLocomotion playerLocomotion;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //// Get the PlayerLocomotion component dynamically
            //if (playerLocomotion == null)
            //{
            //    playerLocomotion = animator.GetComponent<PlayerLocomotion>();
            //}
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool("isJumping", false);

            //// Call the method on PlayerLocomotion
            //if (playerLocomotion != null)
            //{
            //    playerLocomotion.FallAfterJump();
            //}
            //else
            //{
            //    Debug.LogWarning("PlayerLocomotion component not found!");
            //}
        }


    }


}

