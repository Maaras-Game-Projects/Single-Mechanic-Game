using System.Collections;
using UnityEngine;

public class Clown_E_V2 : NPC_Root,IDamagable
{
    [Header("Clown E V2 Variables")]
    [SerializeField] private AnimationClip damageClip;
    [SerializeField]private AnimationClip deathAnimClip;

    protected override void Awake()
    {
        base.Awake();
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
        if (healthSystem.IsDead) return;
        if(playerHealth.isPlayerDead) return;

        
        if(statemachine.currentState != null)
        {
            statemachine.currentState?.TickLogic_All();

        }
        
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
       
    } 

    protected override void FixedUpdate()
    {
        if (healthSystem.IsDead) return;
        if(playerHealth.isPlayerDead) return;

        if(statemachine.currentState != null)
        {
            statemachine.currentState?.TickPhysics_All();

        }

    }

    public void TakeDamage(float damageAmount)
    {
        if (healthSystem.IsDead) return;

        healthSystem.DepleteHealth(damageAmount);

        PlayAnyActionAnimation(damageClip.name,true);

        if(healthSystem.CheckForDeath())
        {
            PlayAnyActionAnimation(deathAnimClip.name,true);
            float animLength = deathAnimClip.length;
            StartCoroutine(DisableEnemyColliderAFterDelay(animLength));
        }
    }

    
    
    IEnumerator DisableEnemyColliderAFterDelay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        DisableCOllider();
    }

    public void TakeDamage(float damageAmount, float criticalDamage)
    {
        throw new System.NotImplementedException();
    }

    // void OnDrawGizmos()
    // {

    //     // stare radius
    //     //VisualiseDetectionCapsule(6f, 2f);


    //     // chase radius
    //     VisualiseDetectionCapsule(6f, 3f);

    //     // combat radius
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(transform.position, 1.3f);
    // }


}
