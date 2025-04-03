using UnityEngine;

public class ResetNPCAnimBools : StateMachineBehaviour
{

    [SerializeField] string rootMotionUseBoolString;
    [SerializeField] bool rootMotionUseBoolStatus;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        animator.SetBool(rootMotionUseBoolString, rootMotionUseBoolStatus);

    }
}
