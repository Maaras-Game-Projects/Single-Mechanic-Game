using UnityEngine;

public class DisableDogeRollChain : StateMachineBehaviour
{
    public string dodgeRollTriggerBoolString ;
    public bool dodgeRollTriggerBoolStatus = false;
    PlayerCombat playerCombat;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        playerCombat = animator.GetComponent<PlayerCombat>();
        playerCombat.DisableInvincibility();
        animator.SetBool(dodgeRollTriggerBoolString, dodgeRollTriggerBoolStatus);
    }
}
