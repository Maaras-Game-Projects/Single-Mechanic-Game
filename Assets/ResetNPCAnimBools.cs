using UnityEngine;

public class ResetNPCAnimBools : StateMachineBehaviour
{

    [SerializeField] string isInteractingBoolString;
    [SerializeField] bool isInteractingStatus;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        animator.SetBool(isInteractingBoolString, isInteractingStatus);

    }
}
