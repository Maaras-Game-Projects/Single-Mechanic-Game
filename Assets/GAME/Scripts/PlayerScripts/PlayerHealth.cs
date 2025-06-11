using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{

    public bool isPlayerDead = false;

    //

    [SerializeField] private float maxhealth = 150f;
    //[SerializeField] private bool isDead = false;
    [SerializeField] private float currentHealth;
    [SerializeField] private int healthPotionCount = 5;

    public int HealthPotionCount
    {
        get => healthPotionCount;
    }

    [SerializeField] private Image HealthBarImage_BG;
    [SerializeField] private Image HealthBarImage_Front;

    [SerializeField] private bool isHealing_AnimPlaying;
    [SerializeField] private bool isHealthBarUpdating;
    [SerializeField] private float healthBarAnimSpeed;
    [SerializeField] private float healSpeed;
    [SerializeField] private float healDuration;
    [SerializeField] private bool heal_TimeBased = true;


    private Coroutine animateCoroutine_heal;

    private Coroutine depleteCoroutine;


    [SerializeField] PlayerCombat playerCombat;
    [SerializeField] PlayerAnimationManager playerAnimationManager;
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] StaminaSystem_Player staminaSystem_Player;

    [SerializeField] HandleHealthUI handleHealthUI;


    [Space]
    [Header("Animation Clip Variables")]
    [Space]

    [SerializeField] private AnimationClip hitAnimationClip;
    [SerializeField] private AnimationClip deathAnimationClip;
    [SerializeField] private AnimationClip healAnimationClip;


    #region Events

    public UnityEvent OnPlayerTakeDamage;
    public UnityEvent OnPlayerDead;
    public UnityEvent onPlayerFullHeal;

    #endregion

    #region Properties

    public float MaxHealth => maxhealth;
    public float CurrentHealth => currentHealth;
    public bool IsPlayerDead => isPlayerDead;

    #endregion

    void OnEnable()
    {
        onPlayerFullHeal.AddListener(() => handleHealthUI.UpdateHealthPotionCount(healthPotionCount));
        onPlayerFullHeal.AddListener(() => handleHealthUI.UpdateHealthPotionUI(healthPotionCount));

        handleHealthUI.UpdateHealthPotionCount(healthPotionCount);
        handleHealthUI.UpdateHealthPotionUI(healthPotionCount);
    }

    void OnDisable()
    {
        onPlayerFullHeal.RemoveListener(()=> handleHealthUI.UpdateHealthPotionCount(healthPotionCount));
        onPlayerFullHeal.RemoveListener(()=> handleHealthUI.UpdateHealthPotionUI(healthPotionCount));
    }

    // private void Start()
    // {
    //     currentHealth = totalHealth;
    //     SetHealthBarSizeToTotalHealth();

    // }

    // private void SetHealthBarSizeToTotalHealth()
    // {
    //     Vector2 healthBarSize = healthBarIMG.rectTransform.sizeDelta;
    //     healthBarSize.x = totalHealth * .1f;
    //     healthBarIMG.rectTransform.sizeDelta = healthBarSize;
    // }

    // private void UpdateHealthBar()
    // {
    //     healthBarIMG.fillAmount = currentHealth / totalHealth;
    // }

    // private void UpdateHealthWithAnimation()
    // {
    //     float targetHealth = currentHealth / totalHealth;

    //     StartCoroutine(AnimateHealthBarReduce(targetHealth));
    // }

    // IEnumerator AnimateHealthBarReduce(float targetHealth)
    // {
    //     while(Mathf.Abs(healthBarIMG.fillAmount - targetHealth) > 0.01f)
    //     {
    //         healthBarIMG.fillAmount = Mathf.Lerp(healthBarIMG.fillAmount, targetHealth, Time.deltaTime * healthReduceSpeed);
    //         yield return null;
    //     }

    //     healthBarIMG.fillAmount = targetHealth;
    // }





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxhealth;

        //Need to Load current health and potion count from save data here
    }

    // Update is called once per frame
    void Update()
    {
        // if(Input.GetKey(KeyCode.R)) // debug
        //     PlayHealAnimation();

        //RotateHealthBarTowardsPlayer();
    }

    // private void RotateHealthBarTowardsPlayer()
    // {
    //     Vector3 cameraDir = mainCamera.transform.position - HealthBarImage_BG.transform.position;

    //     Quaternion lookRotation = Quaternion.LookRotation(cameraDir);

    //     HealthBarImage_BG.transform.rotation = lookRotation;
    // }

    public void IncrementHealthPotionCount(int amount)
    {
        if (amount <= 0) return;
        healthPotionCount += amount;


    }

    public void DecrementHealthPotionCount()
    {
        healthPotionCount--;
        if (healthPotionCount < 0)
            healthPotionCount = 0;
    }

    public void FullHeal()
    {
        if (currentHealth >= maxhealth) return;

        if (!isHealing_AnimPlaying)
        {
            //animateCoroutine_heal = StartCoroutine(AnimateHealthBarHeal_TimeBased(healDuration,maxhealth));
            if (heal_TimeBased)
                animateCoroutine_heal = StartCoroutine(AnimateHealthBarHeal_TimeBased(healDuration, maxhealth));
            else
                animateCoroutine_heal = StartCoroutine(AnimateHealthBarHeal_SpeedBased(maxhealth));

            playerAnimationManager.PlayAnyInteractiveAnimation(healAnimationClip.name, true, true);
        }

        if (currentHealth > maxhealth)
            currentHealth = maxhealth;

        DecrementHealthPotionCount();
        // TODO: Add some Sound cue to indicate health potion used and healed
        //Add UI feedback for health potion used

        Debug.Log($"<color=red> Healed");

        onPlayerFullHeal?.Invoke();
    }

    public void PlayHealAnimation()
    {
        if (currentHealth >= maxhealth || healthPotionCount <= 0)
        {
            //Add some Sound cue to indicate no health potion left
            return;
        }

        if (playerAnimationManager.inAnimActionStatus) return;
        if (!isHealing_AnimPlaying)
        {
            playerAnimationManager.PlayAnyInteractiveAnimation(healAnimationClip.name, true, true);
        }

    }

    public void DepleteHealth(float depletionAmount)
    {
        float absolute_DepletionAmount = Mathf.Abs(depletionAmount);

        currentHealth -= absolute_DepletionAmount;


        float targetAmount = currentHealth / maxhealth;
        if (HealthBarImage_BG != null && HealthBarImage_Front != null)
        {
            HealthBarImage_Front.fillAmount = targetAmount;

            if (depleteCoroutine != null)
            {
                StopCoroutine(depleteCoroutine);
            }
            depleteCoroutine = StartCoroutine(AnimateHealthBarUpdate(targetAmount));

        }


        if (currentHealth < 0)
            currentHealth = 0;
    }



    IEnumerator AnimateHealthBarUpdate(float targetAmount)
    {
        if (animateCoroutine_heal != null)
        {
            StopCoroutine(animateCoroutine_heal);
            isHealing_AnimPlaying = false;
        }

        isHealthBarUpdating = true;

        yield return new WaitForSeconds(0.35f);


        while (Mathf.Abs(HealthBarImage_BG.fillAmount - targetAmount) > 0.01) // animate until difference is close enough to 0
        {
            HealthBarImage_BG.fillAmount = Mathf.MoveTowards(HealthBarImage_BG.fillAmount, targetAmount,
                Time.deltaTime * healthBarAnimSpeed);

            yield return null;
        }

        HealthBarImage_BG.fillAmount = targetAmount;

        isHealthBarUpdating = false;

    }

    IEnumerator AnimateHealthBarHeal_SpeedBased(float targetAmount)
    {

        isHealing_AnimPlaying = true;

        float absolute_targetAmount = Mathf.Clamp(targetAmount, 0f, maxhealth - currentHealth);
        float endValue = currentHealth + absolute_targetAmount;

        while (currentHealth < endValue)
        {
            currentHealth += healSpeed * Time.deltaTime;

            float targetFillAmount = currentHealth / maxhealth;
            Debug.Log("targetFillAmount: " + targetFillAmount);

            HealthBarImage_Front.fillAmount = Mathf.MoveTowards(HealthBarImage_Front.fillAmount, targetFillAmount,
                        healSpeed * Time.deltaTime);

            HealthBarImage_BG.fillAmount = Mathf.MoveTowards(HealthBarImage_BG.fillAmount, targetFillAmount,
                        healSpeed * Time.deltaTime);

            yield return null;

        }
        HealthBarImage_BG.fillAmount = 1f;
        HealthBarImage_Front.fillAmount = 1f;
        currentHealth = endValue;

        isHealing_AnimPlaying = false;

    }

    IEnumerator AnimateHealthBarHeal_TimeBased(float duration, float targetAmount)
    {

        isHealing_AnimPlaying = true;

        float absoluteTargetAmount = Mathf.Abs(targetAmount);
        if (absoluteTargetAmount > maxhealth)
            absoluteTargetAmount = maxhealth;

        float currentHealthBeforeHeal = currentHealth;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float lerpTime = elapsedTime / duration;
            currentHealth = Mathf.Lerp(currentHealthBeforeHeal, absoluteTargetAmount, lerpTime);

            float targetFillAmount = currentHealth / maxhealth;

            // HealthBarImage_Front.fillAmount = Mathf.Lerp(HealthBarImage_Front.fillAmount,targetFillAmount,
            //             lerpTime);
            HealthBarImage_Front.fillAmount = targetFillAmount;

            // HealthBarImage_BG.fillAmount = Mathf.Lerp(HealthBarImage_BG.fillAmount,targetFillAmount,
            //             lerpTime);
            HealthBarImage_BG.fillAmount = targetFillAmount;

            elapsedTime += Time.deltaTime;

            yield return null;

        }
        HealthBarImage_BG.fillAmount = 1f;
        HealthBarImage_Front.fillAmount = 1f;
        currentHealth = absoluteTargetAmount;

        isHealing_AnimPlaying = false;

    }

    public void TakeDamage(float DamageVal)
    {
        if (isPlayerDead) return;

        playerCombat.DisableHitDetectionInDelay(.1f);
        playerCombat.DisableCanComboDelayed(.1f);
        playerCombat.DisableCanCombo();


        if (playerCombat.isInvincible) return;

        currentHealth -= DamageVal;
        playerAnimationManager.SetAllLayersToDefaultState_ExceptDamageState();

        if (playerCombat.isInvincible)
            playerCombat.isInvincible = false;

        playerAnimationManager.PlayAnyInteractiveAnimation(hitAnimationClip.name, true, true);
        playerCombat.DisableInvinciblityInDelay(.1f);

        DepleteHealth(DamageVal);

        OnPlayerTakeDamage?.Invoke();

        if (currentHealth <= 0)
        {

            playerAnimationManager.PlayAnyInteractiveAnimation(deathAnimationClip.name, true, true);
            playerCombat.playerSword.EnableDisableSwordCollider(false);
            playerCombat.playerSword.SetSwordRotationValueAtPlayerDeath();
            playerLocomotion.SetVelocityToZero();
            isPlayerDead = true;

            OnPlayerDead?.Invoke();
        }
    }


    public void TakeDamage(float DamageVal, bool enemyParryWindow, NPC_Root enemy)
    {

        if (isPlayerDead) return;

        playerCombat.DisableHitDetectionInDelay(.1f);
        playerCombat.DisableCanComboDelayed(.1f);
        playerCombat.DisableCanCombo();



        if (playerCombat.isInvincible) return;

        //Debug.Log("hit detection disabled");

        if (enemyParryWindow && playerCombat.ParrySolo_Begin)
        {


            playerCombat.OnCloseUpSoloParrySuccess(enemy);
            playerCombat.EndParry_Solo();
            DamageVal = 0;
            return;
        }


        if (playerCombat.isBlocking)
        {
            staminaSystem_Player.DepleteStamina(playerCombat.BlockHitStaminaCost);
            if (staminaSystem_Player.CurrentStamina < 1)
            {
                //play Stun animation
                playerCombat.GetStunned();

                return;
            }
            else
            {
                float damagePercentAfterBlockReduction = 100 - playerCombat.blockDamageREductionValPercent;
                DamageVal = DamageVal * (damagePercentAfterBlockReduction / 100);
                playerLocomotion.PeformCameraShake(0.5f, 3f);
            }



        }

        // if(playerCombat.isBlocking && enemyParryWindow && playerCombat.isParrying)
        // {
        //    Debug.Log("BLock Parried");

        //    //playerCombat.OnCloseUpParrySuccess(enemy);
        //    DamageVal = 0;
        // }



        currentHealth -= DamageVal;

        if (!playerCombat.isBlocking)
        {
            playerAnimationManager.SetAllLayersToDefaultState_ExceptDamageState();

            if (enemy.CanAttackKnockback)
            {
                if (playerCombat.isInvincible)
                    playerCombat.isInvincible = false;
                playerCombat.GetKnockedDown();
            }
            else
            {
                if (playerCombat.isInvincible)
                    playerCombat.isInvincible = false;
                playerAnimationManager.PlayAnyInteractiveAnimation(hitAnimationClip.name, true, true);

            }

            playerCombat.DisableInvinciblityInDelay(.1f);

        }

        // if(animateHealthBar)
        // {
        //     UpdateHealthWithAnimation();
        // }
        // else
        // {
        //     UpdateHealthBar();
        // }

        DepleteHealth(DamageVal);

        OnPlayerTakeDamage?.Invoke();

        if (currentHealth <= 0)
        {

            playerAnimationManager.PlayAnyInteractiveAnimation(deathAnimationClip.name, true, true);
            playerCombat.playerSword.EnableDisableSwordCollider(false);
            playerCombat.playerSword.SetSwordRotationValueAtPlayerDeath();
            playerLocomotion.SetVelocityToZero();
            playerLocomotion.DisableLockON();

            isPlayerDead = true;

            playerLocomotion.playerRigidBody.constraints = RigidbodyConstraints.FreezeAll;


            OnPlayerDead?.Invoke();
        }
    }

    public void ResetPlayerHealth()
    {
        isPlayerDead = false;
        currentHealth = maxhealth;

        if (HealthBarImage_BG != null && HealthBarImage_Front != null)
        {
            HealthBarImage_BG.fillAmount = 1f;
            HealthBarImage_Front.fillAmount = 1f;
        }

        if (animateCoroutine_heal != null)
        {
            StopCoroutine(animateCoroutine_heal);
            isHealing_AnimPlaying = false;
        }

        if (depleteCoroutine != null)
        {
            StopCoroutine(depleteCoroutine);
            isHealthBarUpdating = false;
        }
    }
}
