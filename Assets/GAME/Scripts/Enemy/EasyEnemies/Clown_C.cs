using UnityEngine;
using System.Collections;

public class Clown_C : NPC_Root,IDamagable
{
    [Header("Clown E V2 Variables")]
    [SerializeField] private AnimationClip damageClip;
    [SerializeField]private AnimationClip deathAnimClip;

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
        if (isDead) return;
        if(playerHealth.isPlayerDead) return;

        if(statemachine.currentState != null)
        {
            statemachine.currentState?.TickPhysics_All();

        }

    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        health -= damageAmount;

        //animator.Play("Hit_left");
        PlayAnyActionAnimation(damageClip.name,true);

        if (health <= 0)
        {
           // Debug.Log("Dead");

            PlayAnyActionAnimation(deathAnimClip.name,true);
            isDead = true;

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

    
}

