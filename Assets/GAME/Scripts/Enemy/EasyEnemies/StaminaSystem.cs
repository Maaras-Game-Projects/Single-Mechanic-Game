using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StaminaSystem : MonoBehaviour
{
    [SerializeField] private float totalStamina = 30f;
    [SerializeField] private float rechargeSpeed = 1.5f;

    [SerializeField] private float currentStamina;
    [SerializeField] private NPC_Root nPC_Root;

    private Coroutine animateCoroutine;
    private Coroutine animateCoroutine_recharge;

    [Space]
    [Header("Debug Variables")]
    [Space]

    [SerializeField] Image staminaImage_BG;
    [SerializeField] Image staminaImage_Front;
    [SerializeField]private float staminaBarAnimSpeed;
    //[SerializeField]private float rechargeSpeed_FillBar;
    [SerializeField]private bool isStaminaBarAnimating = false;
    [SerializeField]private bool isStaminaBarAnimating_Recharge = false;
    [SerializeField]private bool canAnimate_BG = false;
    [SerializeField] private float elapsedTime = 0f;
    //[SerializeField] private float elapsedTime_Anim = 0f;

    void Start()
    {
        currentStamina = totalStamina;
        //rechargeSpeed_FillBar = rechargeSpeed/totalStamina;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            // if(!isStaminaBarAnimating)
            // {
            //     DepleteStamina(20f);
            // }
            DepleteStamina(20f);
           
        }
        
        elapsedTime += Time.deltaTime;

        if(elapsedTime >= 1f)
        {
            RechargeStamina();
            elapsedTime = 0f;
        }

        // if(canAnimate_BG)
        // {
        //     elapsedTime_Anim += Time.deltaTime;

        //     if(elapsedTime_Anim > 1f)
        //     {
        //         float targetAmount = currentStamina/totalStamina;

        //         if(staminaImage_BG.fillAmount > targetAmount)
        //         {
        //             //staminaImage_BG.fillAmount += targetAmount;
        //             staminaImage_BG.fillAmount = Mathf.MoveTowards(staminaImage_BG.fillAmount,
        //                 targetAmount,Time.deltaTime * staminaBarAnimSpeed);
        //         }
        //         else
        //         {
        //             canAnimate_BG = false;
        //             elapsedTime_Anim = 0f;
        //         }
        //     }

            
        // }
        
    }

    public void RechargeStamina()
    {
        if(currentStamina >= totalStamina) return;

        currentStamina += rechargeSpeed;
        
        //debug
        if(staminaImage_BG != null && staminaImage_Front != null)
        {
            float targetAmount = currentStamina/totalStamina;
            if(animateCoroutine_recharge!= null)
            {
                StopCoroutine(animateCoroutine_recharge);
            }
            animateCoroutine_recharge = StartCoroutine(AnimateStaminaRecharge(targetAmount));
            //AnimateStaminaRecharge();
        }
        //debug

        if(currentStamina > totalStamina)
            currentStamina = totalStamina;
    }

    public void DepleteStamina(float depletionAmount)
    {
        float absolute_DepletionAmount = Mathf.Abs(depletionAmount);

        currentStamina -= absolute_DepletionAmount;
        
        //debug
        float targetAmount = currentStamina/totalStamina;
        if(staminaImage_BG != null && staminaImage_Front != null)
        {
            staminaImage_Front.fillAmount = targetAmount;
            if(animateCoroutine!= null)
            {
                StopCoroutine(animateCoroutine);
            }
            animateCoroutine = StartCoroutine(AnimateStaminaUpdate(targetAmount));
           
        }
        //debug

        if(currentStamina < 0)
            currentStamina = 0;
    }

    private void AnimateStaminaRecharge()
    {
        if(nPC_Root.isInteracting) return;
        //if(isStaminaBarAnimating) return;

        if(staminaImage_BG.fillAmount >= 1 && staminaImage_Front.fillAmount >= 1) return;
        
        float targetFill = currentStamina/totalStamina;

        staminaImage_BG.fillAmount = targetFill;
        staminaImage_Front.fillAmount = targetFill;
    }

    IEnumerator AnimateStaminaUpdate(float targetAmount)
    {   
        
        isStaminaBarAnimating = true;
        
        yield return new WaitForSeconds(0.25f);


        while( Mathf.Abs(staminaImage_BG.fillAmount - targetAmount) > 0.01) // animate until difference is close enough to 0
        {
            staminaImage_BG.fillAmount = Mathf.MoveTowards(staminaImage_BG.fillAmount,targetAmount,
                Time.deltaTime * staminaBarAnimSpeed);
            
            yield return null;
        }

        staminaImage_BG.fillAmount = targetAmount;

        isStaminaBarAnimating = false;

    }

    IEnumerator AnimateStaminaRecharge(float targetAmount)
    {   
        
        isStaminaBarAnimating_Recharge = true;
        
        while( Mathf.Abs(staminaImage_BG.fillAmount - targetAmount) > 0.01
            & (Mathf.Abs(staminaImage_Front.fillAmount - targetAmount) > 0.01)) // animate until difference is close enough to 0
        {
            staminaImage_BG.fillAmount = Mathf.MoveTowards(staminaImage_BG.fillAmount,targetAmount,
                Time.deltaTime * staminaBarAnimSpeed);
            staminaImage_Front.fillAmount = Mathf.MoveTowards(staminaImage_Front.fillAmount,targetAmount,
                Time.deltaTime * staminaBarAnimSpeed);
            
            yield return null;
        }

        staminaImage_BG.fillAmount = targetAmount;
        staminaImage_Front.fillAmount = targetAmount;

        isStaminaBarAnimating_Recharge = false;

    }

}
