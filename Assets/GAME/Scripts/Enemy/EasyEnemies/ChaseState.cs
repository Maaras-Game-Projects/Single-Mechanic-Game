using UnityEngine;
using UnityEngine.AI;

public class ChaseState : State
{
    [SerializeField] public NavMeshAgent navMeshAgent;

    [SerializeField]Transform target;

    [SerializeField] public float chaseRadius = 2f;
    [SerializeField] public float chaseDetectionDistance = 2.5f;


    //[SerializeField] private float chaseSpeed = 1.5f;

    [SerializeField] IdleState idleState;  
    [SerializeField] CombatState combatState;  
    [SerializeField] CombatAdvanced_State combatState_Advanced;  

   
   //[SerializeField] State attackState;  

    
    public override void SetCurrentSubState()
    {
        //subStatemachine.currentState = patrolSTate;
    }

   public override void OnEnter()
   {
        navMeshAgent.isStopped = false;
        //navMeshAgent.speed = chaseSpeed;
        //navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
        navMeshAgent.updatePosition = false;


        //npcRoot.TurnCharacter();
        npcRoot.isChasingTarget = true;
   }

    public override void TickLogic()
    {
        if(npcRoot.isInteracting) return;

        if(combatState_Advanced.CheckIfInCombatRange())
        {

            combatState_Advanced.inCombatRadius = true; // debug var
            if(npcRoot.isPlayerInLineOfSight())
            {
                if(!combatState_Advanced.chaseToAttackAtStart)
                {                   
                    npcRoot.statemachine.SwitchState(combatState_Advanced);
                    return;
                }
                else if(combatState_Advanced.IsPlayerInCloseRange())
                {
                    npcRoot.statemachine.SwitchState(combatState_Advanced);
                    return;
                }
                
            }
            
            
        }
        else
        {
            combatState_Advanced.inCombatRadius = false; // debug var
        }

        
        

        Vector3 startPoint = npcRoot.transform.position;
        Vector3 endPoint = startPoint+ npcRoot.transform.forward * chaseDetectionDistance;
        if(npcRoot.IsPlayerInRange_Capsule(startPoint, endPoint,chaseRadius))
        {
            idleState.GoToLocomotionAnimation();
            npcRoot.TurnCharacter();
            npcRoot.LookAtPlayer();
            navMeshAgent.SetDestination(target.position);
            npcRoot.SetStrafeAnimatorValues_Run();

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
        navMeshAgent.updateRotation = true;
        navMeshAgent.updatePosition = true;

        npcRoot.isChasingTarget = false;
        
        npcRoot.ResetMovementAnimatorValues();
        //navMeshAgent.speed = chaseSpeed;
    }
    
}
