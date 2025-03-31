using UnityEngine;
using UnityEngine.AI;

public class ChaseState : State
{
   [SerializeField] NavMeshAgent navMeshAgent;
   [SerializeField]Transform target;

   [SerializeField] State patrolSTate;  

    
    public override void SetCurrentSubState()
    {
        subStatemachine.currentState = patrolSTate;
    }

   public override void OnEnter()
   {
      
   }

    public override void TickLogic()
    {
       
    }
    
}
