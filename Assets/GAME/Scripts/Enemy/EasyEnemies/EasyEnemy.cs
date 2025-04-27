using UnityEngine;

public class EasyEnemy : NPC_Root
{
    protected override void Awake()
    {
        statemachine = new Statemachine();
        statemachine.SetCurrentState(states[0]);
        SetAllStates();
        InitAllSubStatemachines();
    }

    void Start()
    {
        // DebugLogic();
        // DebugPhysics();
    }

    private void DebugPhysics()
    {
        if (statemachine.currentState != null)
        {
            statemachine.currentState.TickPhysics_All();

        }
    }

    private void DebugLogic()
    {
        if (statemachine.currentState != null)
        {
            statemachine.currentState.TickLogic_All();

        }
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

}
    