using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Rendering;


// This class contains all common properties and methods for all NPC classes
public class NPC_Root : MonoBehaviour
{
    //[SerializeField] public float health = 150f; //
    [SerializeField] public float currentDamageToDeal = 50f; //
    [SerializeField] public bool canAttackKnockback = false; //

    public bool CanAttackKnockback => canAttackKnockback; //
    //[SerializeField] public bool isDead = false; //

    [SerializeField] public Animator animator; // 
    [SerializeField] public Rigidbody rigidBody; //

    [SerializeField] private CapsuleCollider npcCollider;

    [SerializeField] public bool isInteracting = false; //
    
    [SerializeField] public bool canRotateWhileAttack = false; //

    [SerializeField] public Transform lockOnTransform_Self; //
    [SerializeField] public float lookRotationSpeed; //
    [SerializeField] public Transform targetTransform; //

    [SerializeField] public List<State> states = new List<State>(); //
    [SerializeField] public Statemachine statemachine; //[SerializeField] public State currentState => statemachine.currentState; //

    public bool canDetectHit = false; ////////
    public bool parryable = false; //////// might create seperate hit detection module with parryable logic

    [SerializeField] public StaminaSystem staminaSystem;
    [SerializeField] public HealthSystem healthSystem;
    [SerializeField] public ShieldSystem shieldSystem;
    [SerializeField] public ProjectileSpawner projectileSpawner;

    [SerializeField] private CanvasGroup enemyCanvasGroup;

    [SerializeField] public LayerMask obstacleLayerMask;

    [Space]
    [Header("NavMesh and Strafe Variables")]
    [Space]

    [SerializeField] NavMeshAgent navMeshAgent; 
    [SerializeField] public bool isChasingTarget = false; //bb
    [SerializeField] public bool isStrafing = false; //bb

    [Space]
    [Header("Stun Variables")]
    [Space]
    [SerializeField] private bool isStunned;

    [SerializeField] private AnimationClip stunAnimationClip;

    public bool IsStunned => isStunned; //
    [SerializeField] public float chaseSpeed = 1f; //
    [SerializeField] public float strafeSpeed = 1f; //
    

    [Space]
    [Header("Player Variables")]
    [Space]
    public PlayerHealth playerHealth;
    public LayerMask playerLayerMask;

    [Space]
    [Header("Turn Animation Variables")]
    [Space]
    [SerializeField] private AnimationClip turnAnimRight_90;
    [SerializeField] private AnimationClip turnAnimLeft_90;
    [SerializeField] private AnimationClip turnAnimRight_180;
    [SerializeField] private AnimationClip turnAnimLeft_180;
    [SerializeField] public bool isTurning = false; //

    private Quaternion targetRotationAtEndOfTurn;


    [Space]
    [Header("Debugging")]
    [Space]
    public bool debug = false;
    [SerializeField] private TextMeshPro debugStateText;
    
    [Space]
    [Header("Grounding & Falling Variables")]
    [Space]

    [SerializeField] bool canFallAndLand = true;

    [SerializeField] private float groundRaycastOffset;
    [SerializeField] private float maxGroundCheckDistance;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isJumping;

    [SerializeField] private float inAirTimer;
    [SerializeField] private float leapingVelocity;
    [SerializeField] private float fallingVelocity;
    [SerializeField]private AnimationClip landAnimClip;
    [SerializeField] private AnimationClip fallAnimClip;


    private CapsuleCollider capsuleCollider;
    private Vector3 capsuleColliderCenter_Default;
    private float capsuleHeight_Default;
    [SerializeField]private Vector3 targetPosition_OnStairs;


    [Space]
    [Header("Events")]
    [Space]

    [SerializeField]private UnityEvent onHitDetectionEnd;
    

    void Start()
    {
        
        
    }



    // might need to add collider for hit detection


    public virtual void SetAllStates()
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

    protected virtual void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleColliderCenter_Default = capsuleCollider.center;
        capsuleHeight_Default = capsuleCollider.height;
        //Debug.Log("awake " + capsuleCollider.name);
        DisableNavMeshMovement();
    }


    protected virtual void FixedUpdate()
    {
        if(healthSystem.IsDead) return;
        if(!canFallAndLand) return;
        HandleFallingAndLanding();
    }
   

    protected virtual void LateUpdate()
    {
        if(healthSystem.IsDead) return;

        isInteracting = animator.GetBool("isInteracting");

        if(canFallAndLand)
        {
            animator.SetBool("isGrounded", isGrounded);
        }

        if(navMeshAgent != null  && !navMeshAgent.updatePosition)
        {
            navMeshAgent.nextPosition = transform.position;
            navMeshAgent.transform.rotation = transform.rotation;
        }
        
    }

    public void LookAtPlayer(float rotationSpeed)
    {
        
        // Get direction to target (ignore Y-axis to prevent tilting)
        Vector3 direction = (targetTransform.position - transform.position).normalized;
        direction.y = 0; // Prevent vertical tilting

        // Smoothly rotate towards the player
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

    }

    public void RotateOnAttack(float rotationSpeed)
    {
        if(!canRotateWhileAttack) return;

        LookAtPlayer(rotationSpeed);
        //Debug.Log("asdadadadad");
    }

#region NavMesh Utilities

    public void DisableNavMeshMovement()
    {
        if(navMeshAgent== null) return;


        navMeshAgent.isStopped = false;
        navMeshAgent.updateRotation = false;
        navMeshAgent.updatePosition = false;

        
    }


    public void SetNavMeshAgentVelocityToZero()
    {
        if(navMeshAgent== null) return;
        navMeshAgent.velocity = Vector3.zero;
    }

    public void SetNavMeshAgentDestination(Vector3 target)
    {
        if(navMeshAgent== null) return;
        navMeshAgent.SetDestination(target);
    }

#endregion


    public void ResetMovementAnimatorValues()
    {
        animator.SetFloat("X_Velocity", 0f);
        animator.SetFloat("Z_Velocity", 0f);
    }

    public void SetMovementAnimatorValues(Vector3 Velocity)
    {   
        float x_velocityVal;
        float z_velocityVal;

        // if (!isStrafing)
        // {
        //     x_velocityVal = Mathf.Clamp01(Mathf.Abs(Velocity.x));
        //     z_velocityVal = Mathf.Clamp01(Mathf.Abs(Velocity.z));
        // }
        // else
        // {
        //     x_velocityVal = Velocity.x;
        //     z_velocityVal = Velocity.z;
        // }
        
        x_velocityVal = Mathf.Clamp01(Mathf.Abs(Velocity.x));
        z_velocityVal = Mathf.Clamp01(Mathf.Abs(Velocity.z));

        animator.SetFloat("X_Velocity", x_velocityVal, 0.1f, Time.deltaTime);
        animator.SetFloat("Z_Velocity", z_velocityVal, 0.1f, Time.deltaTime);
    }

    public void SetStrafeAnimatorValues(direction direction)
    {
        switch (direction)
        {
            case direction.left:
                SetStrafeAnimatorValues_Left();
                break;
            case direction.right:
                SetStrafeAnimatorValues_Right();
                break;
            case direction.front:
                SetStrafeAnimatorValues_Front();
                break;
            case direction.back:
                SetStrafeAnimatorValues_Back();
                break;
        }
    }

    public void SetStrafeAnimatorValues_Run()
    {
        animator.SetFloat("X_Velocity", 0, 0.1f, Time.deltaTime);
        animator.SetFloat("Z_Velocity", 1, 0.1f, Time.deltaTime);
    }
       

    private void SetStrafeAnimatorValues_Left()
    {
        animator.SetFloat("X_Velocity", -0.5f, 0.25f, Time.deltaTime);
        animator.SetFloat("Z_Velocity", 0, 0.25f, Time.deltaTime);
    }

    private void SetStrafeAnimatorValues_Right()
    {
        animator.SetFloat("X_Velocity", 0.5f, 0.25f, Time.deltaTime);
        animator.SetFloat("Z_Velocity", 0, 0.25f, Time.deltaTime);
    }

    private void SetStrafeAnimatorValues_Front()
    {
        animator.SetFloat("X_Velocity", 0, 0.25f, Time.deltaTime);
        animator.SetFloat("Z_Velocity", 0.5f, 0.25f, Time.deltaTime);
    }

    private void SetStrafeAnimatorValues_Back()
    {
        animator.SetFloat("X_Velocity", 0, 0.25f, Time.deltaTime);
        animator.SetFloat("Z_Velocity", -0.5f, 0.25f, Time.deltaTime);
    }

    public void EnableEnemyCanvas()
    {
        enemyCanvasGroup.alpha = 1;
    }

    public void DisableEnemyCanvas()
    {
        enemyCanvasGroup.alpha = 0;
        enemyCanvasGroup.blocksRaycasts = false;
        enemyCanvasGroup.interactable = false;
    }

    public void EnableHitDetection()
    {
        canDetectHit = true;
    }
    
    public void DisableHitDetection()
    {  
        canDetectHit = false;
        onHitDetectionEnd?.Invoke();
    }


    public void EnableHAttackRotation()
    {
        canRotateWhileAttack = true;
    }
    
    public void DisableHAttackRotation()
    {  
        canRotateWhileAttack = false;
    }

    public void DisableHitDetectionInDelay(float duration)
    {
        StartCoroutine(DisableHitDetectionAfterDelay(duration));
    }

    IEnumerator DisableHitDetectionAfterDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        canDetectHit = false;
        onHitDetectionEnd?.Invoke();
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

    public void SpawnProjectile()
    {
        if(projectileSpawner != null)
        {
            projectileSpawner.SpawnProjectile();
        }
    }

    public void SetDebugStateText(string stateName)
    {
        if (!debug) return; // If debug is not enabled, do not set the text
        if(debugStateText != null)
        {
            debugStateText.text = stateName;
        }
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

    public void PlayAnyActionAnimation(string animationName,bool isInteracting = false)
    {
        animator.SetBool("isInteracting", isInteracting);
        animator.CrossFade(animationName, 0.1f);
  
    }

    void OnAnimatorMove()
    {
        if (animator == null) return;

        //HandleRootMotionUsage();

        //HandleHitDetectionOnTransitions();

        Vector3 animDeltaPosition = animator.deltaPosition;
        Quaternion animDeltaRotation = animator.deltaRotation;
        UpdateTurnRotation();

        if (!isChasingTarget)
        {

            if(isStrafing)
            {
                transform.position += animDeltaPosition * strafeSpeed;
            }
            else if (isInteracting)
            {
                transform.position += animDeltaPosition;
            }
            else
            {
                //animDeltaPosition.y = 0f;
                transform.position += animDeltaPosition;
            }

             
        }
        else if (navMeshAgent != null)
        {
            Vector3 navMeshPosition = navMeshAgent.nextPosition;

            //Vector3 chaseDirection = (navMeshPosition - transform.position).normalized;
            Vector3 chaseDirection = navMeshAgent.desiredVelocity.normalized;

            Vector3 moveDelta = chaseDirection * animDeltaPosition.magnitude;
            //Vector3 moveDelta =   animDeltaPosition;

            float moveSpeed = chaseSpeed;

            transform.position += moveDelta * moveSpeed;

            
            // Smoothly rotate towards the player
            if (chaseDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(chaseDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookRotationSpeed);
            }

        }



    }

    public void UpdateTurnRotation()
    {
        
        if (isInteracting)
        {
            
            if (isTurning)
            {

                Vector3 directionToTarget = targetTransform.position - transform.position;
                directionToTarget.y = 0; // Ignore vertical component

                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookRotationSpeed);
            }

        }
    }

    private void HandleRootMotionUsage()
    {
        if (isInteracting)
        {
            rigidBody.linearDamping = 0;
            Vector3 animDeltaPosition = animator.deltaPosition;
            animDeltaPosition.y = 0;
            Vector3 animTargetVelocity = animDeltaPosition / Time.deltaTime; // vel = changeinPos/ChangeinTime
            //animTargetVelocity.y = 0;
            rigidBody.linearVelocity = animTargetVelocity;
        }
    }

    public bool IsPlayerInRange_Capsule(Vector3 startPoint, Vector3 endPoint, float playerDetectionRadius)
    {
        Collider[] hitColliders = Physics.OverlapCapsule(startPoint,endPoint,playerDetectionRadius, playerLayerMask);

        if (hitColliders.Length == 0) return false;

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
        
    }

    public bool IsPlayerInRange_Sphere(Vector3 startPoint,float playerDetectionRadius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(startPoint, playerDetectionRadius, playerLayerMask);

        if (hitColliders.Length == 0) return false;

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
        
    }

    public bool isPlayerInLineOfSight()
    {
        if(Physics.Linecast(lockOnTransform_Self.position, targetTransform.position + Vector3.up * .5f, obstacleLayerMask))
        {
            return false;
        }

        return true;


    }

    // call this in animEvent at the end of all turning anims
    public void DisableTurnBoolean()
    {
        isTurning = false;

        // Vector3 directionToTarget = targetTransform.position - transform.position;
        // directionToTarget.y = 0; // Ignore vertical component

        // Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        // //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookRotationSpeed);
        // transform.rotation = targetRotation;
        SnapRotationAfterTurning();
    }

    public void TurnCharacter()
    {
        Vector3 directionToTarget = targetTransform.position - transform.position;
        directionToTarget.y = 0; // Ignore vertical component

        float signedAngle = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);

        //Debug.Log("Signed Angle: " + signedAngle);

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        targetRotationAtEndOfTurn = targetRotation;

        if(signedAngle > -45f && signedAngle < 45f)
        {
            // Do nothing, facing forward
            //Debug.Log("Signed Angle: " + signedAngle);
            return;
        }

        isTurning = true;

        if(signedAngle > 135f || signedAngle < -135f)
        {
            if(turnAnimRight_180 != null)
            {
                PlayAnyActionAnimation(turnAnimRight_180.name, true);
            }

        }
        else

        if(signedAngle > 45f)
        {
            if(turnAnimRight_90 != null)
            {
                PlayAnyActionAnimation(turnAnimRight_90.name, true);
            }

        }
        else if(signedAngle > 135f)
        {
            if(turnAnimRight_180 != null)
            {
                PlayAnyActionAnimation(turnAnimRight_180.name, true);
            }

        }
        else if(signedAngle < -45f)
        {
           if(turnAnimLeft_90 != null)
            {
                PlayAnyActionAnimation(turnAnimLeft_90.name, true);
            }

        }
        else if(signedAngle < -135f)
        {
            if(turnAnimLeft_180 != null)
            {
                PlayAnyActionAnimation(turnAnimLeft_180.name, true);
            }

        }

    }

    // call this in animEvent at the end of all turning anims
    public void SnapRotationAfterTurning()
    {
        transform.rotation = targetRotationAtEndOfTurn;
    }

    public void OnParried()
    {
        GetStunned();
    }

    private void GetStunned()
    {
        DisableHitDetection();
        DisableHitDetectionInDelay(0.15f);
        isStunned = true;
        PlayAnyActionAnimation(stunAnimationClip.name, true);
       
    }

    public void DisableCOllider()
    {
        npcCollider.enabled = false;
        rigidBody.useGravity = false;

    }

    private void HandleFallingAndLanding()
    {
        RaycastHit hit;
        Vector3 raycastOrigin = transform.position;
        raycastOrigin.y = raycastOrigin.y + groundRaycastOffset;

        Vector3 targetPosition = transform.position;

        //Debug.DrawLine(raycastOrigin, raycastOrigin + Vector3.down * groundRaycastOffset, Color.cyan);
       // Debug.DrawRay(raycastOrigin, Vector3.down * groundRaycastOffset, Color.red);

        if (!isGrounded)
        {
            if (!isJumping)
            {
                if (!isInteracting)
                {
                    if(fallAnimClip!=null)
                    {
                        PlayAnyActionAnimation(fallAnimClip.name, true);
                    }
                    

                }

               

                //animator.SetBool("isUsingRootMotion", false);
                inAirTimer += Time.deltaTime;

                rigidBody.AddForce(transform.forward * leapingVelocity);
                rigidBody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
            }

        }

        // if(!isGrounded)
        // {
        //     if(Physics.Raycast(transform.position + Vector3.up * 0.5f, moveDirection, out RaycastHit hitInfo, .75f, groundLayer))
        //     {
        //         Vector3 fallVelocity = playerRigidBody.linearVelocity;
        //         fallVelocity.x = 0f;
        //         fallVelocity.z = 0f;
        //         playerRigidBody.linearVelocity = fallVelocity;
        //         Debug.Log($"<color=green> target fall velocity after collision = {playerRigidBody.linearVelocity}</color>");
                
        //     }
        // }
        

        if(isJumping) return;

        //if(inCoyoteTime) return;

        if (Physics.SphereCast(raycastOrigin, 0.2f, -Vector3.up, out hit, maxGroundCheckDistance, groundLayer))
        {
            if (!isGrounded && isInteracting)
            //if (playerAnimationManager.inAnimActionStatus)
            {
                if(landAnimClip != null)
                {
                    PlayAnyActionAnimation(landAnimClip.name, true);
                }
                
                
            }

            Vector3 rayHitPoint = hit.point;
            targetPosition.y = rayHitPoint.y;
            //Debug.Log("Ground hit: " + hit.collider.name);
            inAirTimer = 0;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        

        if (isGrounded && !isJumping)
        {
            if (isInteracting )
            {
                // if(isDodging && onStairs)
                // {
                //     playerTargetPosition.y = playerTargetPosition.y + verticalTargetPositionOffset;
                // }
                

                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime/0.1f);
                
            }
            else
            {
                transform.position = targetPosition;
            }
        }

       
        //Debug.Log($"<color=cyan>velocity on fall = {playerRigidBody.linearVelocity}</color>");
    }

    // private void HandleStairsManeuver()
    // {
    //     RaycastHit hit;
    //     Vector3 raycastOrigin = transform.position;
    //     raycastOrigin.y = raycastOrigin.y + groundRaycastOffset;

    //     targetPosition_OnStairs = transform.position;

        
    //     if (Physics.SphereCast(raycastOrigin, 0.2f, -Vector3.up, out hit, maxGroundCheckDistance, groundLayer))
    //     {
            
    //         Vector3 rayHitPoint = hit.point;
    //         targetPosition_OnStairs.y = rayHitPoint.y;
    //         //y_targetPosition_OnStairs = rayHitPoint.y;
    //         //Debug.Log("Ground hit: " + hit.collider.name);
            
    //     }
    //     // float smoothSpeed = 10f; // you can tweak this
    //     // Vector3 currentPosition = rigidBody.position;
    //     // Vector3 smoothed = new Vector3(
    //     //     currentPosition.x,
    //     //     Mathf.Lerp(currentPosition.y, targetPosition_OnStairs.y, Time.fixedDeltaTime * smoothSpeed),
    //     //     currentPosition.z
    //     // );

    //     // rigidBody.MovePosition(smoothed);
        
    //     if (isInteracting)
    //     {
            
    //         transform.position = Vector3.Lerp(transform.position, targetPosition_OnStairs, Time.deltaTime/0.1f);
    //         //transform.position = targetPosition_OnStairs;
           
            
    //     }
    //     else
    //     {
    //         transform.position = targetPosition_OnStairs;
    //     }

    //    //rigidBody.position = targetPosition_OnStairs;
       
    // }

    void OnCollisionEnter(Collision collision)
    {
       

        if (collision.gameObject.CompareTag("Player"))
        {
            //Debug.Log("<color=cyan>Collided with Player</color>");
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }

        // if(collision.gameObject.tag == stairsTag)
        // {
        //     onStairs = true;
        //     ShrinkCollider();
        // }
        // else
        // {
        //     onStairs = false;
        //     ResizeCollider();
        // }
    }

    void OnCollisionExit(Collision collision)
    {
       

        if (collision.gameObject.CompareTag("Player"))
        {
            // Debug.Log("<color=cyan>Collided with exit Player</color>");
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        

        
    }

    private void ShrinkCollider()
    {
        capsuleCollider.center = new Vector3(capsuleCollider.center.x, capsuleCollider.center.y + .35f, capsuleCollider.center.z);
        capsuleCollider.height = capsuleCollider.height * .75f;
    }

    private void ResizeCollider()
    {
        capsuleCollider.center = capsuleColliderCenter_Default;
        capsuleCollider.height = capsuleHeight_Default;
    }

    // private void OnGUI()
    // {
    //     GUIStyle gUIStyle = new GUIStyle();
    //     gUIStyle.fontSize = 30;
    //     gUIStyle.normal.textColor = Color.white;

    //     if(statemachine!=null && statemachine.currentState != null)
    //     {
    //         GUI.Label(new Rect(1500, 25, 500, 500), "Current State: " + statemachine.currentState.GetType().Name, gUIStyle);
    //     }
    //     else
    //     {
    //         GUI.Label(new Rect(1500, 25, 500, 500), "Current State: None", gUIStyle);
    //     }

    // }

    void OnDrawGizmos()
    {
        //VisualiseGroundCheck();
    }
    public void VisualiseGroundCheck()
    {
        // Define the start position and direction
        Vector3 start = transform.position;
        start.y = start.y + groundRaycastOffset;
        Vector3 direction = -Vector3.up;
        float radius = .25f;
        float maxDistance = maxGroundCheckDistance;

        // Set Gizmo color
        Gizmos.color = Color.cyan;

        // Draw the initial sphere at the raycast start point
        Gizmos.DrawWireSphere(start, radius);

        // If SphereCast hits something, draw the hit point and full cast path
        if (Physics.SphereCast(start, radius, direction, out RaycastHit hit, maxDistance, groundLayer))
        {
            Gizmos.color = Color.yellow;
            // Draw a line from start to hit point
            Gizmos.DrawLine(start, hit.point);

            // Draw sphere at hit point
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(hit.point, radius);
        }
        else
        {
            // Draw the full cast length if nothing was hit (limited to avoid infinite line)
            Gizmos.DrawRay(start, direction * maxDistance); // Adjust 5f as needed
        }
    }

    public void VisualiseDetectionCapsule(float maxDistance, float lockONDetectionRadius)
    
    {
        Vector3 capsuleStart = transform.position;
        Vector3 capsuleEnd = transform.position + transform.forward * maxDistance;

        Gizmos.color = Color.cyan;

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
