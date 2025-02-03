using UnityEngine;

public class SetAnimActionsBool : StateMachineBehaviour
{
    [SerializeField] string AnimActionBoolString;
    [SerializeField] bool AnimActionBoolStatus;

    [SerializeField] string rootMotionUseBoolString;
    [SerializeField] bool rootMotionUseBoolStatus;

    PlayerLocomotion playerLocomotion;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(playerLocomotion == null)
        {
            playerLocomotion = animator.GetComponent<PlayerLocomotion>();
        }

        animator.SetBool(AnimActionBoolString, AnimActionBoolStatus);
        animator.SetBool(rootMotionUseBoolString, rootMotionUseBoolStatus);

        playerLocomotion.canMove = true;
        playerLocomotion.canRotate = true;

    }


}
