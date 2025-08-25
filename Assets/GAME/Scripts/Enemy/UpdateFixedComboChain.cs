using UnityEngine;

namespace EternalKeep
{
    public class UpdateFixedComboChain : StateMachineBehaviour
    {
        NPC_Root npcRoot;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            npcRoot = animator.GetComponent<NPC_Root>();

            if (npcRoot != null)
            {
                //npcRoot.DisableStun();
                //npcRoot.SetPerformingComboAttacksStatus(false);
                npcRoot.UpdateFixedComboChainAttacks();
                //Debug.Log("<color=green>Fixed Combo Attack Updated</color>");
            }




        }
        
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            npcRoot = animator.GetComponent<NPC_Root>();

            if (npcRoot != null)
            {
                //This is a fallback to disable canCombo
                npcRoot.UpdateFixedComboCanCombo();
            }
            



        }
    }
}
