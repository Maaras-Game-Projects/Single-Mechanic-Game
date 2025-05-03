using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class RootEnemy : NPC_Root,IDamagable
{
    [Header("Clown E V2 Variables")]
    [SerializeField] private AnimationClip damageClip;
    [SerializeField]private AnimationClip deathAnimClip;
    [SerializeField]private UnityEvent onDamageTaken;

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

    void FixedUpdate()
    {
        if (healthSystem.IsDead) return;
        if(playerHealth.isPlayerDead) return;

        if(statemachine.currentState != null)
        {
            statemachine.currentState?.TickPhysics_All();

        }

    }

    public void TakeDamage(float damageAmount,float criticalDamage)
    {
        if (healthSystem.IsDead) return;

        
        if(IsStunned)
        {
            if(shieldSystem.ActiveShieldCount == 0)
            {
                healthSystem.DepleteHealth(criticalDamage);
                shieldSystem.BreakSheild();
                healthSystem.DisplayDamageTaken(damageAmount);
            }
            else
            {
                shieldSystem.BreakSheild();
            }
            
        }
        else
        {
            if(shieldSystem.ActiveShieldCount == 0)
            {
                healthSystem.DepleteHealth(damageAmount);
                shieldSystem.BreakSheild();
                healthSystem.DisplayDamageTaken(damageAmount);
            }
            else
            {
                shieldSystem.BreakSheild();
            }
        }
        
        
        onDamageTaken?.Invoke();

        //dependent on string need to refactor
        animator.Play("Empty State",1); // to cancel ongoing animations in these two layers
        animator.Play("Empty State",2);

        PlayAnyActionAnimation(damageClip.name,true);

        if(healthSystem.CheckForDeath())
        {
            DisableEnemyCanvas();
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

    void OnDrawGizmos()
    {

        // stare radius
        //VisualiseDetectionCapsule(6f, 2f);


        // chase radius
        //VisualiseDetectionCapsule(12, 10f);

        // // combat radius
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireSphere(transform.position, 7.5f);

        // // combat offset radius
        // Gizmos.color = Color.green;
        // Gizmos.DrawWireSphere(transform.position, 6.5f);

        // //closeRange radius
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireSphere(transform.position, 1.5f);

        // //BackoffRange radius
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireSphere(transform.position, 3f);

        // //MidRange radius
        // Gizmos.color = Color.blue;
        // Gizmos.DrawWireSphere(transform.position, 5.5f);

        // //LongRange radius
        // Gizmos.color = Color.green;
        // Gizmos.DrawWireSphere(transform.position, 7f);

        // if (navMeshAgent != null)
        // {
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawSphere(transform.position, .5f); // GameObject position

        //     Gizmos.color = Color.green;
        //     Gizmos.DrawSphere(navMeshAgent.nextPosition, .5f); // Agent position
        // }
    
    }

    public void TakeDamage(float damageAmount)
    {
        if (healthSystem.IsDead) return;

        
        if(shieldSystem.ActiveShieldCount == 0)
        {
            healthSystem.DepleteHealth(damageAmount);
            shieldSystem.BreakSheild();
            healthSystem.DisplayDamageTaken(damageAmount);
        }
        else
        {
            shieldSystem.BreakSheild();
        }
        
        
        onDamageTaken?.Invoke();

        //dependent on string need to refactor
        animator.Play("Empty State",1); // to cancel ongoing animations in these two layers
        animator.Play("Empty State",2);

        PlayAnyActionAnimation(damageClip.name,true);

        if(healthSystem.CheckForDeath())
        {
            DisableEnemyCanvas();
            PlayAnyActionAnimation(deathAnimClip.name,true);
            float animLength = deathAnimClip.length;
            StartCoroutine(DisableEnemyColliderAFterDelay(animLength));
        }
    }
}

