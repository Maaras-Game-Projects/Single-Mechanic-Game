using System;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class BaseEnemy : MonoBehaviour,IDamagable
{
    [SerializeField] public float health = 150f;
    [SerializeField] public bool isDead = false;
    [SerializeField] public bool canLookAtPlayer = true;
    [SerializeField] private bool canRunTowardsPlayer;
    private Animator animator;

    private Rigidbody enemyRigidBody;
    [SerializeField] public float moveSpeed = 3f;
    [SerializeField] public float chaseRadius = 20f;
    [SerializeField] public Vector3 runDirectionTowardsPlayer;
    [SerializeField] public Vector3 moveVelocity;
    [SerializeField] public Transform playerTransform;
    [SerializeField] private float rotationSpeed = 2;

    public bool isAttacking = false;
    public bool canDetectHit = false;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyRigidBody = GetComponent<Rigidbody>();

    }

    private void Start()
    {
        
    }

    private void Update()
    {
        CheckIfPlayerInChaseRadius();

        LookAtPlayer();

    }


    private void CheckIfPlayerInChaseRadius()
    {
        if(isDead) return;

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

    }

    private void LookAtPlayer()
    {
        if (isDead) return;

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


    private void AttackPlayer()
    {
        if (isDead) return;
        if(isAttacking) return;

        isAttacking = true;

        PlayAnyActionAnimation("Sword_Attack_1");
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

  
    public void EnableHitDetection()
    {
        canDetectHit = true;
    }
    
    public void DisableHitDetection()
    {  
        canDetectHit = false;
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        health -= damageAmount;

        //animator.Play("Hit_left");
        PlayAnyActionAnimation("Hit_left");

        if (health <= 0)
        {
            Debug.Log("Dead");

            PlayAnyActionAnimation("Falling_Back_Death");
            isDead = true;
        }
    }

    public void PlayAnyActionAnimation(string animationName,bool isUsingRootMotion = false)
    {
        //animator.SetBool("isUsingRootMotion_Enemy", isUsingRootMotion);
        animator.CrossFade(animationName, 0.1f);
  
    }
}
