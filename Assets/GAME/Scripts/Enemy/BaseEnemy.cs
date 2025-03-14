using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class BaseEnemy : MonoBehaviour,IDamagable
{
    [SerializeField] public float health = 150f;
    [SerializeField] public bool isDead = false;
    [SerializeField] public bool canLookAtPlayer = true;
    [SerializeField] private bool canRunTowardsPlayer;
    [SerializeField] private bool canAttackPlayer = false;

    [SerializeField] private bool enableAttackBehaviour = true;
    private Animator animator;
    [SerializeField] private AnimationClip getHitClip;

    private Rigidbody enemyRigidBody;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] public float moveSpeed = 3f;
    [SerializeField] public float chaseRadius = 20f;
    [SerializeField] public float attackRadius = 1.5f;
    [SerializeField] public Vector3 runDirectionTowardsPlayer;
    [SerializeField] public Vector3 moveVelocity;
    [SerializeField] public Transform playerTransform;
    [SerializeField] private float rotationSpeed = 2;
    [SerializeField] private float attackDelayDuration = 1.5f;
    [SerializeField] private float stunDamageMultiplier = 3f;
    [SerializeField] private float stunDuration = 3f;
    [SerializeField] private float baseKnockBackVal = 3f;

    Coroutine attackDelayCoroutine = null;
    [SerializeField] AnimationClip primaryAttackClip;
    [SerializeField] List<AnimationClip> attackAnimClips = new List<AnimationClip>();

    public bool isAttacking = false;
    public bool inAttackDelay = false;
    public bool canDetectHit = false;
    public bool parryable = false;
    public bool isStunned = false;
    [SerializeField] private bool enemy_RootMotionUseStatus = false;
    private HashSet<AnimationClip> attackAnimClipsHashSet = new HashSet<AnimationClip>();
     [SerializeField] private AnimationClip lastClip;
    public Transform lockOnTransform_Self;
    private int lastTransitionHash;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyRigidBody = GetComponent<Rigidbody>();

        attackAnimClipsHashSet = new HashSet<AnimationClip>(attackAnimClips);

    }

    private void OnDisable()
    {
        if(attackDelayCoroutine  != null)
        {
            StopCoroutine(attackDelayCoroutine);
        }
    }

    private void Update()
    {
        if(enableAttackBehaviour)
        {
            CheckIfPlayerInChaseRadius();
        }
       

        LookAtPlayer();

    }

    void LateUpdate()
    {
        enemy_RootMotionUseStatus = animator.GetBool("isUsingRootMotion_Enemy");
    }


    private void CheckIfPlayerInChaseRadius()
    {
        if(isDead) return;
        if(isStunned) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if(distance <= chaseRadius && distance >= 1f)
        {
            canLookAtPlayer = true;
            canRunTowardsPlayer = true;
        }
        else
        {
            canLookAtPlayer = false;
            canRunTowardsPlayer = false;
            StopMoving();
        }

        if(distance <= attackRadius)
        {
            canAttackPlayer = true;
            AttackPlayer();
        }
        else
        {
            canAttackPlayer = false;
        }

    }

    private void LookAtPlayer()
    {
        if (isDead) return;
        if(isStunned) return;

        if (!canLookAtPlayer)
            return;

        // Get direction to player (ignore Y-axis to prevent tilting)
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0; // Prevent vertical tilting

        // Smoothly rotate towards the player
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        SetRunDirectionTowardsPlayer();
    }

    private void SetRunDirectionTowardsPlayer()
    {
        if (isDead) return;
        if(isStunned) return;

        if (!canRunTowardsPlayer)
            return;

        // set Run direction towards player
        runDirectionTowardsPlayer = transform.forward;
    }

    private void FixedUpdate()
    {
        RunTowardsPlayer();
    }

    private void RunTowardsPlayer()
    {
        if (isDead) return;
        if(isStunned) return;

        if (!canRunTowardsPlayer)
            return;
        Run(runDirectionTowardsPlayer);

        float distanceBetweenPlayerAndSelf = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceBetweenPlayerAndSelf <= 1.5)
        {
            StopMoving();
            AttackPlayer();
        }
    }


    public void Run(Vector3 direction)
    {
        if (isDead) return;
        if(isStunned) return;

        moveVelocity = direction * moveSpeed;
        enemyRigidBody.linearVelocity = moveVelocity;

        SetMovementAnimatorValues(moveVelocity);

    }

    private void StopMoving()
    {
        if (isDead) return;

        canRunTowardsPlayer = false;
        enemyRigidBody.linearVelocity = Vector3.zero;
        //SetMovementAnimatorValues(enemyRigidBody.linearVelocity);
        ResetMovementAnimatorValues();
    }

    IEnumerator DisableAttackDelayAfterDelay(float delayTime)
    {

        yield return new WaitForSeconds(delayTime);
        inAttackDelay = false;
    }

    private void AttackPlayer()
    {
        if (isDead) return;
        if(isStunned) return;
        if (isAttacking) return;
        if (inAttackDelay) return;
        if (playerHealth.isPlayerDead) return;
        if (!canAttackPlayer) return;

        isAttacking = true;
        EnableDisableAttackBehaviour(false);

        PlayAnyActionAnimation("Enemy_Attack_1C",false);

        StartCoroutine(EnableAttackBehaviourAfterDuration(primaryAttackClip.length));

        inAttackDelay = true;

        float attackDelayWaitTime = primaryAttackClip.length + attackDelayDuration;

        attackDelayCoroutine = StartCoroutine(DisableAttackDelayAfterDelay(attackDelayWaitTime));
    }

    private void ResetMovementAnimatorValues()
    {
        animator.SetFloat("X_Velocity", 0f);
        animator.SetFloat("Z_Velocity", 0f);
    }

    private void SetMovementAnimatorValues(Vector3 Velocity)
    {
        float x_velocityVal = Mathf.Clamp01(Mathf.Abs(Velocity.x));
        float z_velocityVal = Mathf.Clamp01(Mathf.Abs(Velocity.z));

        animator.SetFloat("X_Velocity", x_velocityVal, 0.1f, Time.deltaTime);
        animator.SetFloat("Z_Velocity", z_velocityVal, 0.1f, Time.deltaTime);
    }

    public void KnockBack()
    {   
        Debug.Log("<color=blue>Z Val B4 Push = </color>" + transform.position.z);
        enemyRigidBody.linearVelocity = Vector3.zero;

        enemyRigidBody.AddForce(-transform.forward * baseKnockBackVal, ForceMode.Impulse);
        Debug.Log("<color=yellow>Z Val AF Push = </color>" + transform.position.z);
        Debug.Log("<color=red>KnockBack</color>");
    }

    private void EnableDisableAttackBehaviour(bool status)
    {
        enableAttackBehaviour = status;
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


    public void OnParried()
    {
        GetStunned();
    }

    private void GetStunned()
    {
        DisableHitDetectionInDelay(0.15f);
        isStunned = true;
        animator.SetBool("isStunned", true);
        PlayAnyActionAnimation("Subtle_Stun",true);
        StartCoroutine(DisableStunAfterDuration(stunDuration));
    }

    IEnumerator DisableStunAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        animator.SetBool("isStunned", false);
        isStunned = false;
    }

    IEnumerator EnableAttackBehaviourAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        EnableDisableAttackBehaviour(true);
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;
        
        EnableDisableAttackBehaviour(false);
        DisableHitDetectionInDelay(0.75f);
        DisableHitDetectionInDelay(0.15f);
        DisableHitDetectionInDelay(0.1f);
        
       

        if(isStunned)
        {
            damageAmount *= stunDamageMultiplier;
        }

        health -= damageAmount;

        //animator.Play("Hit_left");
        PlayAnyActionAnimation("Hit_left");

        float animLength = getHitClip.length;
        StartCoroutine(EnableAttackBehaviourAfterDuration(animLength));
        KnockBack();

        if (health <= 0)
        {
            Debug.Log("Dead");

            PlayAnyActionAnimation("Falling_Back_Death");
            isDead = true;
        }
    }

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

    private void HandleHitDetectionOnTransitions()
    {

        if (!animator.IsInTransition(1)) return; // Exit if not in transition

        AnimatorTransitionInfo transitionInfo = animator.GetAnimatorTransitionInfo(1);
        int currentTransitionHash = transitionInfo.fullPathHash;

        // Get the current animation clip playing on the animation layer (layer 1)
        var currentClipInfo = animator.GetCurrentAnimatorClipInfo(1);
        if (currentClipInfo.Length == 0) return; // If there's no animation playing, exit early

        AnimationClip currentClip = currentClipInfo[0].clip; // Get the active clip

        // If a new transition started from an attack animation
        if (lastClip != null && attackAnimClipsHashSet.Contains(lastClip) && currentTransitionHash != lastTransitionHash)
        {
            DisableHitDetection(); // Disable hit detection at the start of transition
            Debug.Log("Disable Hit Detection - Transition Started");
        }

        // If the last animation was an attack animation and now a different animation is playing
        if (lastClip != null && attackAnimClipsHashSet.Contains(lastClip) && currentClip != lastClip)
        {
            DisableHitDetection(); // Disable hit detection at the end of transition
            Debug.Log("Disable Hit Detection - Transition Ended");
        }

        // Store values for next check
        lastClip = currentClip;
        lastTransitionHash = currentTransitionHash;
        
    }


    private void HandleRootMotionUsage()
    {
        if (enemy_RootMotionUseStatus)
        {
            enemyRigidBody.linearDamping = 0;
            Vector3 animDeltaPosition = animator.deltaPosition;
            animDeltaPosition.y = 0;
            Vector3 animTargetVelocity = animDeltaPosition / Time.deltaTime; // vel = changeinPos/ChangeinTime
            //animTargetVelocity.y = 0;
            enemyRigidBody.linearVelocity = animTargetVelocity;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("<color=green>Collided Enemy</color>");

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("<color=cyan>Collided with Player</color>");
            enemyRigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        Debug.Log("<color=green>Collided exit Enemy</color>");

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("<color=cyan>Collided with exit Player</color>");
            enemyRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }
}

