using Unity.VisualScripting;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void TickLogic() { }
    public virtual void TickPhysics() { }
    public virtual void TickLogic_All() { }
    public virtual void TickPhysics_All() { }
}
