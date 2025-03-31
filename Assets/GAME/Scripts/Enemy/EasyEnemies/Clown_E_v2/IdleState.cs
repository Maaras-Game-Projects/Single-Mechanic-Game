using UnityEngine;

public class IdleState : State
{
    [SerializeField] private float playerDetectionRadius = 2f;
    [SerializeField] private float playerDetectionDistance = 2.5f;
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private State chaseState;

    public string idleAnimTransitionBool;
    public override void OnEnter()
    {
        npcRoot.animator.SetBool(idleAnimTransitionBool, true);
        
    }

    public override void TickLogic()
    {
        // Check if the player is within detection range
        CheckIfPlayerInRange();
    }

    private void CheckIfPlayerInRange()
    {
        Vector3 startPoint = npcRoot.transform.position;
        Vector3 endPoint = startPoint+ npcRoot.transform.forward * playerDetectionDistance;
        Collider[] hitColliders = Physics.OverlapCapsule(startPoint,endPoint,playerDetectionRadius, playerLayerMask);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                // Player is in range, transition to chase state
                Debug.Log("Player in range");
                //npcRoot.statemachine.SwitchState(chaseState);
                //Debug.Log("current state: " + npcRoot.statemachine.currentState);
                return;
            }
        }
        
    }
}
