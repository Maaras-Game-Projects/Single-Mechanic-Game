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
    [SerializeField] private bool isRechargin_Anim =false;

    [Space]
    [Header("Debug Variables")]
    [Space]

    [SerializeField] Image staminaImage_BG;
    [SerializeField] Image staminaImage_Front;
    [SerializeField]private float staminaBarAnimSpeed;
    //[SerializeField]private float rechargeSpeed_FillBar;
    [SerializeField]private bool isStaminaBarAnimating = false;
    [SerializeField] private Camera mainCamera;

    public float CurrentStamina => currentStamina;

    void Start()
    {
        currentStamina = totalStamina;
        //rechargeSpeed_FillBar = rechargeSpeed/totalStamina;
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Tab))
        // {
        //     DepleteStamina(20f);
        // }

        if (!isStaminaBarAnimating)
        {
            RechargeStamina();
        }

        //RotateStaminaBarTowardsPlayer();
    }

    private void RotateStaminaBarTowardsPlayer()
    {
        Vector3 cameraDir = mainCamera.transform.position - staminaImage_BG.transform.position;

        Quaternion lookRotation = Quaternion.LookRotation(cameraDir);

        staminaImage_BG.transform.rotation = lookRotation;
    }

    public void RechargeStamina()
    {
        if(currentStamina >= totalStamina) return;

        if(!isRechargin_Anim)
        {
            animateCoroutine_recharge = StartCoroutine(AnimateStaminaRecharge());
        }

        if(currentStamina > totalStamina)
            currentStamina = totalStamina;
    }

    public void DepleteStamina(float depletionAmount)
    {
        if(currentStamina <= 0)
        {
            isStaminaBarAnimating = false;
            return;
        } 

        float absolute_DepletionAmount = Mathf.Abs(depletionAmount);

        currentStamina -= absolute_DepletionAmount;
        
        //debug
        float targetAmount = currentStamina/totalStamina;
        if(targetAmount < 0)
            targetAmount = 0;

        if(staminaImage_BG != null && staminaImage_Front != null)
        {
            staminaImage_Front.fillAmount = targetAmount;
            if(animateCoroutine!= null)
            {
                StopCoroutine(animateCoroutine);
                //isStaminaBarAnimating = false; //
            }
            animateCoroutine = StartCoroutine(AnimateStaminaUpdate(targetAmount));
           
        }
        //debug

        if(currentStamina < 0)
        {
            //isStaminaBarAnimating = false; //
            currentStamina = 0;
        }
        
    }

  

    IEnumerator AnimateStaminaUpdate(float targetAmount)
    {   
        if(animateCoroutine_recharge != null)
        {
            StopCoroutine(animateCoroutine_recharge);
            isRechargin_Anim = false;
           

        }
        
        isStaminaBarAnimating = true;
        
        yield return new WaitForSeconds(0.35f);

       

        while( Mathf.Abs(staminaImage_BG.fillAmount - targetAmount) > 0.01) // animate until difference is close enough to 0
        {
            
            staminaImage_BG.fillAmount = Mathf.MoveTowards(staminaImage_BG.fillAmount,targetAmount,
                Time.deltaTime * staminaBarAnimSpeed);
            
            yield return null;
        }

        staminaImage_BG.fillAmount = targetAmount;
        isStaminaBarAnimating = false;

    }

    IEnumerator AnimateStaminaRecharge()
    {   
        
        isRechargin_Anim = true;
        
        while(currentStamina < totalStamina)
        {
            currentStamina += rechargeSpeed * Time.deltaTime;

            float targetAmount = currentStamina/totalStamina;

            staminaImage_Front.fillAmount = Mathf.MoveTowards(staminaImage_Front.fillAmount,targetAmount,
                        rechargeSpeed * Time.deltaTime);
            
            staminaImage_BG.fillAmount = Mathf.MoveTowards(staminaImage_BG.fillAmount,targetAmount,
                        rechargeSpeed * Time.deltaTime);

            yield return null;
                        
        }
        currentStamina = totalStamina;

        isRechargin_Anim  = false;

    }

}
