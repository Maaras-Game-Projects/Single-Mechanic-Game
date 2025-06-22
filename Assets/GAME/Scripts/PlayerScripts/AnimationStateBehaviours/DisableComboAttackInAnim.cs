using UnityEngine;



public class DisableComboAttackInAnim : StateMachineBehaviour
{

    public string comboAttackBoolString;
    public bool comboAttackBoolStatus = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        animator.SetBool(comboAttackBoolString, comboAttackBoolStatus);
    }
    

    
}
