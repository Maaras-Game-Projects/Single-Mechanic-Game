using UnityEngine;

public class EasyEnemy : NPC_Root
{
    void Awake()
    {
        statemachine = new Statemachine();
        statemachine.SetCurrentState(states[0]);
    }

    void Update()
    {
        if(statemachine.currentState != null)
        {
            statemachine.currentState.TickLogic();
            
        }
    }

    void FixedUpdate()
    {
        if(statemachine.currentState != null)
        {
            statemachine.currentState.TickPhysics();
            
        }
        
    }

}
    