using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float maxhealth = 150f;
    [SerializeField] private bool isDead = false;
    [SerializeField] private bool isBoss = false;
    [SerializeField] private float currentHealth;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Image HealthBarImage_BG;
    [SerializeField] private Image HealthBarImage_Front;
    [SerializeField] private TextMeshPro damageTextField_TMP;

    [SerializeField] private bool isHealing_AnimPlaying;
    [SerializeField] private bool isHealthBarUpdating;
    [SerializeField] private float healthBarAnimSpeed;
    [SerializeField] private float healSpeed;
    [SerializeField] private float healDuration;

    public UnityEvent onDeath;

    [Space]
    [Header("Boss Canvas")]
    [Space]

    [SerializeField] CanvasGroup bossCanvasGroup;

    [SerializeField] string bossName = "Boss";
    [SerializeField] TextMeshProUGUI bossNameTextField_TMP;

    [SerializeField] private TextMeshProUGUI damageTextField_TMP_Boss;

    [SerializeField] private Image HealthBarImage_BG_Boss;
    [SerializeField] private Image HealthBarImage_Front_Boss;


    private Coroutine animateCoroutine_heal;

    private Coroutine depleteCoroutine;
    private Coroutine damageTextCoroutine;


    #region Properties

    public float MaxHealth => maxhealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (isDead) return;
        currentHealth = maxhealth;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKey(KeyCode.Tab)) // debug
            //FullHeal();

        //RotateHealthBarTowardsPlayer();
    }

    private void RotateHealthBarTowardsPlayer()
    {
        Vector3 cameraDir = mainCamera.transform.position - HealthBarImage_BG.transform.position;

        Quaternion lookRotation = Quaternion.LookRotation(cameraDir);

        HealthBarImage_BG.transform.rotation = lookRotation;
    }

    public void FullHeal()
    {
        if (currentHealth >= maxhealth) return;

        if (!isHealing_AnimPlaying)
        {
            //animateCoroutine_heal = StartCoroutine(AnimateHealthBarHeal_TimeBased(healDuration,maxhealth));
            animateCoroutine_heal = StartCoroutine(AnimateHealthBarHeal_SpeedBased(maxhealth));
        }

        if (currentHealth > maxhealth)
            currentHealth = maxhealth;
    }

    public void DepleteHealth(float depletionAmount)
    {
        float absolute_DepletionAmount = Mathf.Abs(depletionAmount);

        currentHealth -= absolute_DepletionAmount;


        float targetAmount = currentHealth / maxhealth;

        if (isBoss)
        {
            if (HealthBarImage_BG_Boss != null && HealthBarImage_Front_Boss != null)
            {
                HealthBarImage_Front_Boss.fillAmount = targetAmount;
                if (depleteCoroutine != null)
                {
                    StopCoroutine(depleteCoroutine);
                }
                depleteCoroutine = StartCoroutine(AnimateHealthBarUpdate(targetAmount));

            }
        }
        else
        {
            if (HealthBarImage_BG != null && HealthBarImage_Front != null)
            {
                HealthBarImage_Front.fillAmount = targetAmount;
                if (depleteCoroutine != null)
                {
                    StopCoroutine(depleteCoroutine);
                }
                depleteCoroutine = StartCoroutine(AnimateHealthBarUpdate(targetAmount));

            }
        }



        if (currentHealth < 0)
            currentHealth = 0;
    }


    public void SetEnemyDead()
    {
        currentHealth = 0;
        HealthBarImage_BG.fillAmount = 0;
        HealthBarImage_Front.fillAmount = 0;
        isDead = true;
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

        if (isBoss)
        {
            while (Mathf.Abs(HealthBarImage_BG_Boss.fillAmount - targetAmount) > 0.01) // animate until difference is close enough to 0
            {
                HealthBarImage_BG_Boss.fillAmount = Mathf.MoveTowards(HealthBarImage_BG_Boss.fillAmount, targetAmount,
                    Time.deltaTime * healthBarAnimSpeed);

                yield return null;
            }
            HealthBarImage_BG_Boss.fillAmount = targetAmount;
        }
        else
        {
            while (Mathf.Abs(HealthBarImage_BG.fillAmount - targetAmount) > 0.01) // animate until difference is close enough to 0
            {
                HealthBarImage_BG.fillAmount = Mathf.MoveTowards(HealthBarImage_BG.fillAmount, targetAmount,
                    Time.deltaTime * healthBarAnimSpeed);

                yield return null;
            }

            HealthBarImage_BG.fillAmount = targetAmount;
        }


        isHealthBarUpdating = false;

    }

    IEnumerator AnimateHealthBarHeal_SpeedBased(float targetAmount)
    {

        isHealing_AnimPlaying = true;

        float absolute_targetAmount = Mathf.Clamp(targetAmount, 0f, maxhealth - currentHealth);
        float endValue = currentHealth + absolute_targetAmount;

        if (isBoss)
        {
            while (currentHealth < endValue)
            {
                currentHealth += healSpeed * Time.deltaTime;

                float targetFillAmount = currentHealth / maxhealth;

                HealthBarImage_Front_Boss.fillAmount = Mathf.MoveTowards(HealthBarImage_Front_Boss.fillAmount, targetFillAmount,
                            healSpeed * Time.deltaTime);

                HealthBarImage_BG_Boss.fillAmount = Mathf.MoveTowards(HealthBarImage_BG_Boss.fillAmount, targetFillAmount,
                            healSpeed * Time.deltaTime);

                yield return null;

            }
            currentHealth = endValue;
        }
        else
        {
            while (currentHealth < endValue)
            {
                currentHealth += healSpeed * Time.deltaTime;

                float targetFillAmount = currentHealth / maxhealth;

                HealthBarImage_Front.fillAmount = Mathf.MoveTowards(HealthBarImage_Front.fillAmount, targetFillAmount,
                            healSpeed * Time.deltaTime);

                HealthBarImage_BG.fillAmount = Mathf.MoveTowards(HealthBarImage_BG.fillAmount, targetFillAmount,
                            healSpeed * Time.deltaTime);

                yield return null;

            }
            currentHealth = endValue;
        }


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
        if (isBoss)
        {
            while (elapsedTime < duration)
            {
                float lerpTime = elapsedTime / duration;
                currentHealth = Mathf.Lerp(currentHealthBeforeHeal, absoluteTargetAmount, lerpTime);

                float targetFillAmount = currentHealth / maxhealth;

                // HealthBarImage_Front.fillAmount = Mathf.Lerp(HealthBarImage_Front.fillAmount,targetFillAmount,
                //             lerpTime);
                HealthBarImage_Front_Boss.fillAmount = targetFillAmount;

                // HealthBarImage_BG.fillAmount = Mathf.Lerp(HealthBarImage_BG.fillAmount,targetFillAmount,
                //             lerpTime);
                HealthBarImage_BG_Boss.fillAmount = targetFillAmount;

                elapsedTime += Time.deltaTime;

                yield return null;

            }
            currentHealth = absoluteTargetAmount;
        }
        else
        {
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
            currentHealth = absoluteTargetAmount;
        }


        isHealing_AnimPlaying = false;

    }

    public bool CheckForDeath()
    {
        if (currentHealth <= 0)
        {
            onDeath?.Invoke();

            if (isBoss)
                bossCanvasGroup.alpha = 0;

            isDead = true;
            return true;
        }
        else
            return false;
    }

    public void DisplayDamageTaken(float damageAmount)
    {
        if (isBoss)
        {
            if (damageTextField_TMP_Boss.gameObject.activeSelf == true)
            {
                float lastDamageTaken = float.Parse(damageTextField_TMP_Boss.text);
                damageAmount += lastDamageTaken;
            }

            damageTextField_TMP_Boss.gameObject.SetActive(true);
            damageTextField_TMP_Boss.text = damageAmount.ToString();

            if (damageTextCoroutine != null)
            {
                StopCoroutine(damageTextCoroutine);
            }
            damageTextCoroutine = StartCoroutine(DisableDamageTextField(2.5f));
        }
        else
        {
            if (damageTextField_TMP.gameObject.activeSelf == true)
            {
                float lastDamageTaken = float.Parse(damageTextField_TMP.text);
                damageAmount += lastDamageTaken;
            }

            damageTextField_TMP.gameObject.SetActive(true);
            damageTextField_TMP.text = damageAmount.ToString();

            if (damageTextCoroutine != null)
            {
                StopCoroutine(damageTextCoroutine);
            }
            damageTextCoroutine = StartCoroutine(DisableDamageTextField(2.5f));
        }

    }

    IEnumerator DisableDamageTextField(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (isBoss)
        {
            damageTextField_TMP_Boss.gameObject.SetActive(false);
        }
        else
        {
            damageTextField_TMP.gameObject.SetActive(false);
        }

    }

    public void EnableBossCanvas()
    {
        if (!isBoss) return;
        bossNameTextField_TMP.text = bossName;
        if (!bossCanvasGroup.gameObject.activeSelf)
            bossCanvasGroup.gameObject.SetActive(true);
        bossCanvasGroup.alpha = 1;

    }

    public void DisableBossCanvas()
    {
        if (!isBoss) return;
       
        if (!bossCanvasGroup.gameObject.activeSelf)
            bossCanvasGroup.gameObject.SetActive(false);
        bossCanvasGroup.alpha = 0;

    }


    public void ResetHealthSystem()
    {
        currentHealth = maxhealth;
        isDead = false;

        if (isBoss)
        {
            if (bossCanvasGroup != null)
            {
                bossCanvasGroup.alpha = 0;
            }
            if (HealthBarImage_BG_Boss != null && HealthBarImage_Front_Boss != null)
            {
                HealthBarImage_BG_Boss.fillAmount = 1f;
                HealthBarImage_Front_Boss.fillAmount = 1f;
            }

            DisableBossCanvas();
        }
        else
        {
            if (HealthBarImage_BG != null && HealthBarImage_Front != null)
            {
                HealthBarImage_BG.fillAmount = 1f;
                HealthBarImage_Front.fillAmount = 1f;
            }
        }

        if (damageTextCoroutine != null)
        {
            StopCoroutine(damageTextCoroutine);
        }

        damageTextField_TMP?.gameObject.SetActive(false);
        damageTextField_TMP_Boss?.gameObject.SetActive(false);
    }
    

    
}
