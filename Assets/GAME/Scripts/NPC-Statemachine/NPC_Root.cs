using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This class contains all common properties and methods for all NPC classes
public class NPC_Root : MonoBehaviour
{
    [SerializeField] public float health = 150f; //
    [SerializeField] public bool isDead = false; //

    [SerializeField] public Animator animator; // 
    [SerializeField] public Rigidbody rigidBody; //

    [SerializeField] public bool npc_RootMotionUseStatus = false; //

    [SerializeField] public Transform lockOnTransform_Self; //

    [SerializeField] public List<State> states = new List<State>(); //
    [SerializeField] public Statemachine statemachine; //[SerializeField] public State currentState => statemachine.currentState; //

    public bool canDetectHit = false; ////////
    public bool parryable = false; //////// might create seperate hit detection module with parryable logic

    private Collider npcCollider; // might need to add collider for hit detection

    void Start()
    {
        npcCollider = GetComponent<Collider>();
    }

    public void SetAllStates()
    {
        foreach (State state in states)
        {
            state.AssignRootTOState(this);
        }
    }

    public void InitAllSubStatemachines()
    {
        foreach (State state in states)
        {
            state.InitSubStateMachine();
            state.SetCurrentSubState();
        }
    }

    void LateUpdate()
    {
        npc_RootMotionUseStatus = animator.GetBool("isUsingRootMotion_Enemy");
    }

    private void ResetMovementAnimatorValues()
    {
        animator.SetFloat("X_Velocity", 0f);
        animator.SetFloat("Z_Velocity", 0f);
    }

    public void SetMovementAnimatorValues(Vector3 Velocity)
    {
        float x_velocityVal = Mathf.Clamp01(Mathf.Abs(Velocity.x));
        float z_velocityVal = Mathf.Clamp01(Mathf.Abs(Velocity.z));

        animator.SetFloat("X_Velocity", x_velocityVal, 0.1f, Time.deltaTime);
        animator.SetFloat("Z_Velocity", z_velocityVal, 0.1f, Time.deltaTime);
    }

    public void EnableHitDetection()
    {
        canDetectHit = true;
    }
    
    public void DisableHitDetection()
    {  
        canDetectHit = false;
    }

    public void DisableHitDetectionInDelay(float duration)
    {
        StartCoroutine(DisableHitDetectionAfterDelay(duration));
    }

    IEnumerator DisableHitDetectionAfterDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        canDetectHit = false;
        //Debug.Log("<color=yellow>hit detection disabled</color>");
    }

    public void EnableParryWindow()
    {
        parryable = true;
    }
    
    public void DisableParryWindow()
    {  
        parryable = false;
    }

    // public void TakeDamage(float damageAmount) // might need to seperate with health and damage logic
    // {
    //     if (isDead) return;
        
    //     //EnableDisableAttackBehaviour(false);
    //     DisableHitDetectionInDelay(0.75f);
    //     DisableHitDetectionInDelay(0.15f);
    //     DisableHitDetectionInDelay(0.1f);
        
       

    //     if(isStunned)
    //     {
    //         damageAmount *= stunDamageMultiplier;
    //     }

    //     health -= damageAmount;

    //     //animator.Play("Hit_left");
    //     PlayAnyActionAnimation("Hit_left");

    //     float animLength = getHitClip.length;
    //     StartCoroutine(EnableAttackBehaviourAfterDuration(animLength));
    //     KnockBack();

    //     if (health <= 0)
    //     {
    //         Debug.Log("Dead");

    //         PlayAnyActionAnimation("Falling_Back_Death");
    //         isDead = true;
    //     }
    // }

    public void PlayAnyActionAnimation(string animationName,bool isUsingRootMotion = false)
    {
        animator.SetBool("isUsingRootMotion_Enemy", isUsingRootMotion);
        animator.CrossFade(animationName, 0.1f);
  
    }

    void OnAnimatorMove()
    {
        if (animator == null) return;

        HandleRootMotionUsage();

        //HandleHitDetectionOnTransitions();
    }

     private void HandleRootMotionUsage()
    {
        if (npc_RootMotionUseStatus)
        {
            rigidBody.linearDamping = 0;
            Vector3 animDeltaPosition = animator.deltaPosition;
            animDeltaPosition.y = 0;
            Vector3 animTargetVelocity = animDeltaPosition / Time.deltaTime; // vel = changeinPos/ChangeinTime
            //animTargetVelocity.y = 0;
            rigidBody.linearVelocity = animTargetVelocity;
        }
    }

    public void DisableCOllider()
    {
        npcCollider.enabled = false;
        rigidBody.useGravity = false;

    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("<color=green>Collided Enemy</color>");

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("<color=cyan>Collided with Player</color>");
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        Debug.Log("<color=green>Collided exit Enemy</color>");

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("<color=cyan>Collided with exit Player</color>");
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    


    public void VisualiseDetectionCapsule(float maxDistance, float lockONDetectionRadius)
    
    {
        Vector3 capsuleStart = transform.position;
        Vector3 capsuleEnd = transform.position + transform.forward * maxDistance;

        Gizmos.color = Color.yellow;

        DrawCapsule(capsuleStart, capsuleEnd, lockONDetectionRadius);
    }

    private void DrawCapsule(Vector3 start, Vector3 end, float radius)
    {
        Gizmos.DrawWireSphere(start,radius);
        Gizmos.DrawWireSphere(end,radius);
        Gizmos.DrawLine(start + Vector3.up * radius,end + Vector3.up * radius);
        Gizmos.DrawLine(start + Vector3.down * radius,end + Vector3.down * radius);
        Gizmos.DrawLine(start + Vector3.right * radius,end + Vector3.right * radius);
        Gizmos.DrawLine(start + Vector3.left * radius,end + Vector3.left * radius);
    }


}
