using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class BaseEnemy : MonoBehaviour,IDamagable
{
    [SerializeField] public float health = 150f;
    [SerializeField] public bool isDead = false;
    private Animator animator;

    private Rigidbody enemyRigidBody;
    [SerializeField] public float moveSpeed = 3f;
    [SerializeField] public Vector3 moveDirection;
    [SerializeField] public Vector3 moveVelocity;
    [SerializeField] public Transform playerTransform;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyRigidBody = GetComponent<Rigidbody>();

    }

    private void Start()
    {
        
    }


    private void FixedUpdate()
    {
        Run();
    }

    public void Run()
    {

        transform.LookAt(playerTransform.position);

        moveDirection = transform.forward;
        moveVelocity = moveDirection * moveSpeed;
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
