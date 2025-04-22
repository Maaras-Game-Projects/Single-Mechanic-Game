using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float maxhealth = 150f; 
    [SerializeField] private bool isDead = false;
    [SerializeField] private float currentHealth;
    [SerializeField] private Camera mainCamera;
    [SerializeField]private Image HealthBarImage_BG;
    [SerializeField]private Image HealthBarImage_Front;
    
    [SerializeField]private bool isHealing_AnimPlaying;
    [SerializeField]private bool isHealthBarUpdating;
    [SerializeField]private float healthBarAnimSpeed;
    [SerializeField]private float healSpeed;
    [SerializeField]private float healDuration;

    public UnityEvent onDeath;

    private Coroutine animateCoroutine_heal;
    
    private Coroutine depleteCoroutine;


#region Properties

    public float MaxHealth => maxhealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

#endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxhealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Tab)) // debug
            FullHeal();

        RotateHealthBarTowardsPlayer();
    }

    private void RotateHealthBarTowardsPlayer()
    {
        Vector3 cameraDir = mainCamera.transform.position - HealthBarImage_BG.transform.position;

        Quaternion lookRotation = Quaternion.LookRotation(cameraDir);

        HealthBarImage_BG.transform.rotation = lookRotation;
    }

    public void FullHeal()
    {
        if(currentHealth >= maxhealth) return;

        if(!isHealing_AnimPlaying)
        {
            //animateCoroutine_heal = StartCoroutine(AnimateHealthBarHeal_TimeBased(healDuration,maxhealth));
            animateCoroutine_heal = StartCoroutine(AnimateHealthBarHeal_SpeedBased(maxhealth));
        }

        if(currentHealth > maxhealth)
            currentHealth = maxhealth;
    }

    public void DepleteHealth(float depletionAmount)
    {
        float absolute_DepletionAmount = Mathf.Abs(depletionAmount);

        currentHealth -= absolute_DepletionAmount;
        
        
        float targetAmount = currentHealth/maxhealth;
        if(HealthBarImage_BG != null && HealthBarImage_Front != null)
        {
            HealthBarImage_Front.fillAmount = targetAmount;
            if(depleteCoroutine!= null)
            {
                StopCoroutine(depleteCoroutine);
            }
            depleteCoroutine = StartCoroutine(AnimateHealthBarUpdate(targetAmount));
           
        }
        

        if(currentHealth < 0)
            currentHealth = 0;
    }

  

    IEnumerator AnimateHealthBarUpdate(float targetAmount)
    {   
        if(animateCoroutine_heal != null)
        {
            StopCoroutine(animateCoroutine_heal);
            isHealing_AnimPlaying = false;
        }
        
        isHealthBarUpdating = true;
        
        yield return new WaitForSeconds(0.35f);


        while( Mathf.Abs(HealthBarImage_BG.fillAmount - targetAmount) > 0.01) // animate until difference is close enough to 0
        {
            HealthBarImage_BG.fillAmount = Mathf.MoveTowards(HealthBarImage_BG.fillAmount,targetAmount,
                Time.deltaTime * healthBarAnimSpeed);
            
            yield return null;
        }

        HealthBarImage_BG.fillAmount = targetAmount;

        isHealthBarUpdating = false;

    }

    IEnumerator AnimateHealthBarHeal_SpeedBased(float targetAmount)
    {   
        
        isHealing_AnimPlaying = true;

        float absolute_targetAmount = Mathf.Clamp(targetAmount,0f,maxhealth - currentHealth);
        float endValue = currentHealth + absolute_targetAmount;
        
        while(currentHealth < endValue)
        {
            currentHealth += healSpeed * Time.deltaTime;

            float targetFillAmount = currentHealth/maxhealth;

            HealthBarImage_Front.fillAmount = Mathf.MoveTowards(HealthBarImage_Front.fillAmount,targetFillAmount,
                        healSpeed * Time.deltaTime);
            
            HealthBarImage_BG.fillAmount = Mathf.MoveTowards(HealthBarImage_BG.fillAmount,targetFillAmount,
                        healSpeed * Time.deltaTime);

            yield return null;
                        
        }
        currentHealth = endValue;

        isHealing_AnimPlaying  = false;

    }

    IEnumerator AnimateHealthBarHeal_TimeBased(float duration, float targetAmount)
    {   
        
        isHealing_AnimPlaying = true;

        float absoluteTargetAmount = Mathf.Abs(targetAmount);

        if(absoluteTargetAmount > maxhealth)
            absoluteTargetAmount = maxhealth;

        float currentHealthBeforeHeal = currentHealth;

        float elapsedTime = 0f;
        
        while(elapsedTime < duration)
        {
            float lerpTime = elapsedTime/duration;
            currentHealth = Mathf.Lerp(currentHealthBeforeHeal,absoluteTargetAmount,lerpTime);

            float targetFillAmount = currentHealth/maxhealth;

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

        isHealing_AnimPlaying  = false;

    }

    public bool CheckForDeath()
    {
        if(currentHealth <= 0)
        {
            onDeath?.Invoke();
            isDead = true;
            return true;
        } 
        else
            return false;
    }

    
}
