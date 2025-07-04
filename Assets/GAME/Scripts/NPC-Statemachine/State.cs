
using UnityEngine;

namespace EternalKeep
{
    public abstract class State : MonoBehaviour
    {
        public NPC_Root npcRoot;

        public Statemachine subStatemachine;

        public void InitSubStateMachine()
        {
            subStatemachine = new Statemachine();

        }

        public virtual void SetCurrentSubState() { }

        public void AssignRootTOState(NPC_Root npcRoot)
        {
            this.npcRoot = npcRoot;
        }
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void TickLogic() { }
        public virtual void TickPhysics() { }
        public virtual void TickLogic_All()
        {
            TickLogic();
            if (subStatemachine != null)
            {
                // if(subStatemachine.currentState == null)
                // {
                //     Debug.Log("Substate is null");
                // }
                // else
                // {
                //     subStatemachine.currentState.TickLogic_All();
                // }
                subStatemachine.currentState?.TickLogic_All();
            }
        }
        public virtual void TickPhysics_All()
        {
            TickPhysics();
            if (subStatemachine != null)
            {
                subStatemachine.currentState?.TickPhysics_All();
            }
        }

        public interface IEnemyStateReset
        {
        }
    }

}

