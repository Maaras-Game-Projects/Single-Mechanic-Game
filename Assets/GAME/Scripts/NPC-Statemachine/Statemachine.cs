using UnityEngine;

public class Statemachine
{
    public State currentState;

    public void SetCurrentState(State state)
    {
        currentState = state;
    }

    public void SwitchState(State newState)
    {
        if(currentState == null)
        {
            SetCurrentState(newState);
            newState.OnEnter();
            currentState.npcRoot.SetDebugStateText(currentState.name); // debugging
            return;
        }
        else
        {
            currentState.OnExit();
            currentState = newState;
            currentState.OnEnter();

            currentState.npcRoot.SetDebugStateText(currentState.name); // debugging
        }
            
            
        
    }
}
