using UnityEngine;
using UnityEngine.AI;

public class ChaseState : State
{
   [SerializeField] NavMeshAgent navMeshAgent;
   [SerializeField]Transform target;

   [SerializeField] public float chaseSpeed = 1.5f;

   [SerializeField] IdleState idleState;  

   
   //[SerializeField] State attackState;  

    
    public override void SetCurrentSubState()
    {
        //subStatemachine.currentState = patrolSTate;
    }

   public override void OnEnter()
   {
      navMeshAgent.enabled = true;
      navMeshAgent.speed = chaseSpeed;
      //navMeshAgent.updatePosition = false;
   }

    public override void TickLogic()
    {
       if(idleState.IsPlayerInRange())
       {
           navMeshAgent.SetDestination(target.position);
           Debug.Log("<color=red>NAVMESH Velocity = </color> " + navMeshAgent.velocity);
           Debug.Log("<color=green>Rigidbody Velocity = </color> " + npcRoot.rigidBody.linearVelocity);

            npcRoot.SetMovementAnimatorValues(navMeshAgent.velocity);

       }
       else
       {
           npcRoot.statemachine.SwitchState(idleState);
       }
    }

    public override void OnExit()
    {
        navMeshAgent.enabled = false;
        //navMeshAgent.speed = chaseSpeed;
    }
    
}
