using UnityEngine;

public class IdleState : State
{
    public string idleAnimTransitionBool;
    public override void OnEnter()
    {
        npcRoot.animator.SetBool(idleAnimTransitionBool, true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
