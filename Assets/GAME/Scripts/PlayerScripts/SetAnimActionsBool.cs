using UnityEngine;

public class SetAnimActionsBool : StateMachineBehaviour
{
    [SerializeField] string AnimActionBoolString;
    [SerializeField] bool AnimActionBoolStatus;

    [SerializeField] string rootMotionUseBoolString;
    [SerializeField] bool rootMotionUseBoolStatus;

    [SerializeField] string parryingBoolString;
    [SerializeField] bool parryingBoolStatus;

   

    PlayerLocomotion playerLocomotion;
    PlayerAnimationManager playerAnimationManager;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(playerLocomotion == null)
        {
            playerLocomotion = animator.GetComponent<PlayerLocomotion>();
            playerAnimationManager = animator.GetComponent<PlayerAnimationManager>();
        }

        animator.SetBool(AnimActionBoolString, AnimActionBoolStatus);
        animator.SetBool(rootMotionUseBoolString, rootMotionUseBoolStatus);
        animator.SetBool(parryingBoolString, parryingBoolStatus);

        playerLocomotion.canMove = true;
        playerLocomotion.canRotate = true;
        playerLocomotion.isDodging = false;
        playerLocomotion.ResetColliderHeightAndCenter();

        playerAnimationManager.playerAnimator.SetBool("ComboTrigger_1", false);
        

    }


}
