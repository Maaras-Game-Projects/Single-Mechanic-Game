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
    [SerializeField] public Vector3 runDirectionTowardsPlayer;
    [SerializeField] public Vector3 moveVelocity;
    [SerializeField] public Transform playerTransform;
    [SerializeField] private float rotationSpeed = 2;


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

        LookAtPlayer();

    }

    private void LookAtPlayer()
    {
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
        if (!canRunTowardsPlayer)
            return;
        Run(runDirectionTowardsPlayer);
    }


    public void Run(Vector3 direction)
    {

       /* transform.LookAt(playerTransform.position);

        moveDirection = transform.forward;*/
        moveVelocity = direction * moveSpeed;
        enemyRigidBody.linearVelocity = moveVelocity;

        SetMovementAnimatorValues();

    }

    private void SetMovementAnimatorValues()
    {
        float x_velocityVal = Mathf.Clamp01(Mathf.Abs(moveVelocity.x));
        float z_velocityVal = Mathf.Clamp01(Mathf.Abs(moveVelocity.z));

        animator.SetFloat("X_Velocity", x_velocityVal, 0.1f, Time.deltaTime);
        animator.SetFloat("Z_Velocity", z_velocityVal, 0.1f, Time.deltaTime);
    }

    public void ScanForPlayer()
    {

    }

    public void AttackPlayer()
    {

    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        health -= damageAmount;

        animator.Play("Hit_left");

        if (health <= 0)
        {
            Debug.Log("Dead");
            animator.Play("Falling_Back_Death");
        }
    }
}
