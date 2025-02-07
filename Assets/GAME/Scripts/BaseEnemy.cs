using UnityEngine;

public class BaseEnemy : MonoBehaviour,IDamagable
{
    [SerializeField] public float health = 150f;
    [SerializeField] public bool isDead = false;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
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
