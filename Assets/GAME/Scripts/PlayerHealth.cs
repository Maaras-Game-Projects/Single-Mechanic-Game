using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{

    public float totalHealth = 500f;
    public float currentHealth;
    public bool isPlayerDead = false;

    [SerializeField] bool animateHealthBar = true;
    [SerializeField] private float healthReduceSpeed = 5f;

    [SerializeField] Image healthBarIMG;

    [SerializeField] PlayerCombat playerCombat;
    [SerializeField] PlayerAnimationManager playerAnimationManager;

    public UnityEvent OnPlayerTakeDamage;
    public UnityEvent OnPlayerDead;


    private void Start()
    {
        currentHealth = totalHealth;
        SetHealthBarSizeToTotalHealth();
        
    }

    private void SetHealthBarSizeToTotalHealth()
    {
        Vector2 healthBarSize = healthBarIMG.rectTransform.sizeDelta;
        healthBarSize.x = totalHealth;
        healthBarIMG.rectTransform.sizeDelta = healthBarSize;
    }

    private void UpdateHealthBar()
    {
        healthBarIMG.fillAmount = currentHealth / totalHealth;
    }

    private void UpdateHealthWithAnimation()
    {
        float targetHealth = currentHealth / totalHealth;

        healthBarIMG.fillAmount = Mathf.MoveTowards(healthBarIMG.fillAmount, targetHealth, Time.deltaTime * healthReduceSpeed);
    }

    public void TakeDamage(float DamageVal)
    {
       
        if (isPlayerDead) return;

        currentHealth -= DamageVal;

        playerAnimationManager.PlayAnyInteractiveAnimation("Hit_Front", true,true);

        if(animateHealthBar)
        {
            UpdateHealthWithAnimation();
        }
        else
        {
            UpdateHealthBar();
        }

        OnPlayerTakeDamage?.Invoke();

        if (currentHealth <= 0)
        {
           
            playerAnimationManager.PlayAnyInteractiveAnimation("Dying_1", true, true);
            playerCombat.playerSword.EnableDisableSwordCollider(false);
            playerCombat.playerSword.SetSwordRotationValueAtPlayerDeath();
            isPlayerDead = true;

            OnPlayerDead?.Invoke();
        }
    }
}
