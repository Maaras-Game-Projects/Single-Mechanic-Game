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

        playerAnimationManager.PlayAnyInteractiveAnimation("Hit_Front", true,true);

        if (currentHealth <= 0)
        {
           
            playerAnimationManager.PlayAnyInteractiveAnimation("Dying_1", true, true);
            playerCombat.playerSword.EnableDisableSwordCollider(false);
            playerCombat.playerSword.SetSwordRotationValueAtPlayerDeath();
            isPlayerDead = true;
        }
    }
}
