using UnityEngine;

namespace EternalKeep
{
    /// <summary>
    /// A StateMachineBehaviour to disable the combo attack boolean in the animator.
    /// This is useful for ensuring that the NPC does not remain in a combo attack state after an attack sequence.
    /// </summary>

    public class DisableComboAttackBool : StateMachineBehaviour
    {
        NPC_Root npcRoot;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            npcRoot = animator.GetComponent<NPC_Root>();

            if (npcRoot != null)
            {
                //npcRoot.DisableStun();
                //npcRoot.SetPerformingComboAttacksStatus(false);
                //Debug.Log("<color=red>Combo Attack Disabled</color>");
            }




        }
        
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            npcRoot = animator.GetComponent<NPC_Root>();

            if (npcRoot != null)
            {
                //npcRoot.DisableStun();
                //npcRoot.SetPerformingComboAttacksStatus(false);
            }
            



        }
    }
}
