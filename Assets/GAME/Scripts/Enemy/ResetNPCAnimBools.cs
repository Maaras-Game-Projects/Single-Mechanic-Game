using UnityEngine;

public class ResetNPCAnimBools : StateMachineBehaviour
{

    [SerializeField] string isInteractingBoolString;
    [SerializeField] bool isInteractingStatus;

    [SerializeField] string isStunnedBoolString;
    [SerializeField] bool isStunnedStatus;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        animator.SetBool(isInteractingBoolString, isInteractingStatus);
        animator.SetBool(isStunnedBoolString, isStunnedStatus);

    }
}
