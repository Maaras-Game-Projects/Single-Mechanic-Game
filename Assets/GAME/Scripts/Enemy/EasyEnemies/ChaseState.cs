using UnityEngine;
using UnityEngine.AI;

public class ChaseState : State
{
    [SerializeField] public NavMeshAgent navMeshAgent;
    [SerializeField]Transform target;

    [SerializeField] public float chaseRadius = 2f;
    [SerializeField] public float chaseDetectionDistance = 2.5f;


    [SerializeField] private float chaseSpeed = 1.5f;

    [SerializeField] IdleState idleState;  
    [SerializeField] CombatState combatState;  

   
   //[SerializeField] State attackState;  

    
    public override void SetCurrentSubState()
    {
        //subStatemachine.currentState = patrolSTate;
    }

   public override void OnEnter()
   {
      navMeshAgent.isStopped = false;
      navMeshAgent.speed = chaseSpeed;
      //navMeshAgent.updatePosition = false;
   }

    public override void TickLogic()
    {

        if(combatState.CheckIfInCombatRange())
        {
            combatState.inCombatRadius = true;
            npcRoot.statemachine.SwitchState(combatState);
            return;
        }

        

        Vector3 startPoint = npcRoot.transform.position;
        Vector3 endPoint = startPoint+ npcRoot.transform.forward * chaseDetectionDistance;
        if(npcRoot.IsPlayerInRange_Capsule(startPoint, endPoint,chaseRadius))
        {
            npcRoot.animator.SetBool( idleState.idleAnimTransitionBool, false);
            navMeshAgent.SetDestination(target.position);
            npcRoot.SetMovementAnimatorValues(navMeshAgent.velocity);

        }
        else
        {
            npcRoot.statemachine.SwitchState(idleState);
        }
    }



    public override void OnExit()
    {
        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
        npcRoot.ResetMovementAnimatorValues();
        //navMeshAgent.speed = chaseSpeed;
    }
    
}
