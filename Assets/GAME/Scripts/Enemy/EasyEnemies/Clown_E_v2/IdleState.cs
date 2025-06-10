using UnityEngine;

public class IdleState : State
{

    [SerializeField] private float stareRadius = 2f;
    [SerializeField] private float StareDistance = 2.5f;
    [SerializeField] private bool starePlayerBeforeChase = false;
    [SerializeField] private string idleAnimTransitionBool;
    [SerializeField] private ChaseState chaseState;

    public override void OnEnter()
    {
        GoToIdleAnimation();

    }

    public void GoToIdleAnimation()
    {
        npcRoot.animator.SetBool(idleAnimTransitionBool, true);
    }

    public void GoToLocomotionAnimation()
    {
        npcRoot.animator.SetBool(idleAnimTransitionBool, false);
    }

    public override void OnExit()
    {
        //npcRoot.animator.SetBool(idleAnimTransitionBool, false);

    }

    ///******** NEED TO ADD LOOK AT PLAYER FUNCTIONALITY IF IN RANGE **********///

    public override void TickLogic()
    {
        if (starePlayerBeforeChase)
        {
            //stare logic
        }
        else
        {
            //npcRoot.statemachine.SwitchState(chaseState);
            // Check if the player is within detection range
            ChaseWhenPlayerInRange();
        }

    }

    private void ChaseWhenPlayerInRange()
    {
        Vector3 startPoint = npcRoot.transform.position;
        Vector3 endPoint = startPoint + npcRoot.transform.forward * chaseState.chaseDetectionDistance;
        if (npcRoot.IsPlayerInRange_Capsule(startPoint, endPoint, chaseState.chaseRadius))
        {
            npcRoot.statemachine.SwitchState(chaseState);
        }

    }


    // public bool IsPlayerInRange()
    // {
    //     Vector3 startPoint = npcRoot.transform.position;
    //     Vector3 endPoint = startPoint+ npcRoot.transform.forward * playerDetectionDistance;
    //     Collider[] hitColliders = Physics.OverlapCapsule(startPoint,endPoint,playerDetectionRadius, npcRoot.playerLayerMask);

    //     foreach (Collider hitCollider in hitColliders)
    //     {
    //         if (hitCollider.CompareTag("Player"))
    //         {
    //             return true;
    //         }
    //     }

    //     return false;

    // }
}
