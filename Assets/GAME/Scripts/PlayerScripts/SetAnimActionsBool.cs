using UnityEngine;

public class SetAnimActionsBool : StateMachineBehaviour
{
    [SerializeField] string AnimActionBoolString;
    [SerializeField] bool AnimActionBoolStatus;

    [SerializeField] string rootMotionUseBoolString;
    [SerializeField] bool rootMotionUseBoolStatus;

    [SerializeField] string parryingBoolString;
    [SerializeField] bool parryingBoolStatus;

    [SerializeField]private string dodgeRollChainTriggerName;
    [SerializeField]private string attackComboTriggerName;

   

    PlayerLocomotion playerLocomotion;
    PlayerAnimationManager playerAnimationManager;
    PlayerCombat playerCombat;
        

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playerLocomotion == null)
        {
            playerLocomotion = animator.GetComponent<PlayerLocomotion>();
            playerAnimationManager = animator.GetComponent<PlayerAnimationManager>();
            playerCombat = animator.GetComponent<PlayerCombat>();
        }

        animator.SetBool(AnimActionBoolString, AnimActionBoolStatus);
        animator.SetBool(rootMotionUseBoolString, rootMotionUseBoolStatus);
        animator.SetBool(parryingBoolString, parryingBoolStatus);

        playerLocomotion.canMove = true;
        playerLocomotion.canRotate = true;
        playerLocomotion.isDodging = false;
        playerLocomotion.ResetColliderHeightAndCenter();
        playerLocomotion.DisableDodgeRollChain();
        playerAnimationManager.playerAnimator.SetLayerWeight(1, 1);
        playerCombat.DisableIsAttacking();
        playerCombat.DisableCanCombo();
        playerCombat.DisableHitDetection();

        playerAnimationManager.playerAnimator.SetBool(attackComboTriggerName, false);
        playerAnimationManager.playerAnimator.SetBool(dodgeRollChainTriggerName, false);
        

    }


}
