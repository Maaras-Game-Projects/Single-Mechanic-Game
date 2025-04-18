using UnityEngine;

public class DisableWindUpBools : StateMachineBehaviour
{
    [SerializeField] string windupBoolName;
    [SerializeField] bool windupBoolStatus;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       animator.SetBool(windupBoolName,windupBoolStatus);
    }

    
}
