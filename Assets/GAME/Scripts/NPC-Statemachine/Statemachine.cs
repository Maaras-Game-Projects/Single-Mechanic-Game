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
            Debug.Log("<color=yellow>Old State = </color>" + currentState);
            SetCurrentState(newState);
            newState.OnEnter();
            Debug.Log(">color=cyan>New State = </color>" + currentState);
            currentState.npcRoot.SetDebugStateText(currentState.name); // debugging
            return;
        }
        else
        {
            currentState.OnExit();
            Debug.Log("<color=yellow>Old State = </color>" + currentState);
            currentState = newState;
            currentState.OnEnter();
            Debug.Log("<color=cyan>New State = </color>" + currentState);
            currentState.npcRoot.SetDebugStateText(currentState.name); // debugging
        }
            
            
        
    }
}
