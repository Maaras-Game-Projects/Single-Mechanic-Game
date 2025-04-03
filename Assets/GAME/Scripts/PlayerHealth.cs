using System.Collections;
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
        healthBarSize.x = totalHealth * .1f;
        healthBarIMG.rectTransform.sizeDelta = healthBarSize;
    }

    private void UpdateHealthBar()
    {
        healthBarIMG.fillAmount = currentHealth / totalHealth;
    }

    private void UpdateHealthWithAnimation()
    {
        float targetHealth = currentHealth / totalHealth;

        StartCoroutine(AnimateHealthBarReduce(targetHealth));
    }

    IEnumerator AnimateHealthBarReduce(float targetHealth)
    {
        while(Mathf.Abs(healthBarIMG.fillAmount - targetHealth) > 0.01f)
        {
            healthBarIMG.fillAmount = Mathf.Lerp(healthBarIMG.fillAmount, targetHealth, Time.deltaTime * healthReduceSpeed);
            yield return null;
        }

        healthBarIMG.fillAmount = targetHealth;
    }

    public void TakeDamage(float DamageVal,bool enemyParryWindow,BaseEnemy enemy)
    {
       
        if (isPlayerDead) return;

        playerCombat.DisableHitDetectionInDelay(.1f);

        if (playerCombat.isInvincible) return;
       
        //Debug.Log("hit detection disabled");

        if(playerCombat.isBlocking)
        {
            float damagePercentAfterBlockReduction = 100 - playerCombat.blockDamageREductionValPercent;
            DamageVal = DamageVal * (damagePercentAfterBlockReduction / 100);
            if(!playerCombat.isParrying)
            {
                // playerAnimationManager.playerAnimator.SetBool("inBlocking", false);
                // playerCombat.KnockBackOnBlockDelayed(1f);

                //add camera shake instead of knockback
            }
            
        }

        if(playerCombat.isBlocking && enemyParryWindow && playerCombat.isParrying)
        {
           Debug.Log("BLock Parried");

           playerCombat.OnCloseUpParrySuccess(enemy);
           DamageVal = 0;
        }

        currentHealth -= DamageVal;

        if(!playerCombat.isBlocking)
        {
            playerAnimationManager.PlayAnyInteractiveAnimation("Hit_Front", true,true);
        }

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
