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

    // public override void SetAllStates()
    // {
    //     base.SetAllStates();
    //     foreach (State state in states)
    //     {
    //         state.AssignRootTOState(this);
    //     }
    // }

    void Start()
    {
        statemachine.currentState?.OnEnter();
    }

    void Update()
    {
        if (isDead) return;

        if(statemachine.currentState != null)
        {
            statemachine.currentState?.TickLogic_All();

        }
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
       
    } 

    void FixedUpdate()
    {
        if (isDead) return;

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

    // void OnDrawGizmos()
    // {
    //     VisualiseDetectionCapsule(6f, 2f);
    // }
}
