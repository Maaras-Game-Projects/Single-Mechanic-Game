using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    public float maxHealth = 500f;
    public float currentHealth;
    public bool isPlayerDead = false;

    [SerializeField] PlayerCombat playerCombat;
    [SerializeField] PlayerAnimationManager playerAnimationManager;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float DamageVal)
    {
       
        if (isPlayerDead) return;

        currentHealth -= DamageVal;

        //playerAnimationManager.PlayAnyInteractiveAnimation()

        //animator.Play("Hit_left");
        //PlayAnyActionAnimation("Hit_left");

        if (currentHealth <= 0)
        {
            //Debug.Log("Dead");

            //PlayAnyActionAnimation("Falling_Back_Death");
            isPlayerDead = true;
        }
    }
}
