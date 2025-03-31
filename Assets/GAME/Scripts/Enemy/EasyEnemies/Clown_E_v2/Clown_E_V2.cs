using System.Collections;
using UnityEngine;

public class Clown_E_V2 : NPC_Root
{

    void Awake()
    {
        statemachine = new Statemachine();
        statemachine.SetCurrentState(states[0]);
        SetAllStates();
        InitAllSubStatemachines();
    }

    void Start()
    {
        statemachine.currentState?.OnEnter();
    }

    void Update()
    {
        if(statemachine.currentState != null)
        {
            statemachine.currentState?.TickLogic_All();

        }
    }

    void FixedUpdate()
    {
        if(statemachine.currentState != null)
        {
            statemachine.currentState?.TickPhysics_All();

        }

    }

    IEnumerator DisableEnemyColliderAFterDelay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        DisableCOllider();
    }
}
