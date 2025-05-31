using UnityEngine;

public class ResetNPCAnimBools : StateMachineBehaviour
{

    [SerializeField] string isInteractingBoolString;
    [SerializeField] bool isInteractingStatus;

    [SerializeField] string isStunnedBoolString;
    [SerializeField] bool isStunnedStatus;
    
    NPC_Root npcRoot;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        npcRoot = animator.GetComponent<NPC_Root>();

        if (npcRoot != null)
        {
            npcRoot.DisableStun();
        }
        animator.SetBool(isInteractingBoolString, isInteractingStatus);
        animator.SetBool(isStunnedBoolString, isStunnedStatus);

        

    }
}
