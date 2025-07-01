using UnityEngine;

namespace EternalKeep
{
    public class DisableDogeAttackbool : StateMachineBehaviour
    {
        public string dodgeAttackBoolString;
        public bool dodgeAttackBoolStatus = false;

        PlayerCombat playerCombat;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            playerCombat = animator.GetComponent<PlayerCombat>();
            playerCombat.DisableInvincibility();
            animator.SetBool(dodgeAttackBoolString, dodgeAttackBoolStatus);
        }
    }

}

