using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StaminaSystem_Player : MonoBehaviour
{

    [SerializeField] private float totalStamina = 30f;
    [SerializeField] private float rechargeSpeed = 1.5f;
    [SerializeField] private float currentStamina;

    private Coroutine animateCoroutine;
    private Coroutine animateCoroutine_recharge;
    [SerializeField] private bool isRechargin_Anim = false;

    [Space]
    [Header("Player Variables")]
    [Space]
    [SerializeField] private PlayerCombat playerCombat;


    [Space]
    [Header("Debug Variables")]
    [Space]

    [SerializeField] Image staminaImage_BG;
    [SerializeField] Image staminaImage_Front;
    [SerializeField] private float staminaBarAnimSpeed;
    //[SerializeField]private float rechargeSpeed_FillBar;
    [SerializeField] private bool isStaminaBarAnimating = false;
    //[SerializeField] private Camera mainCamera;

    public float CurrentStamina => currentStamina;
    private float initialRechargeSpeed;
    private float blocRechargeSpeed;

    [Range(0.1f, 1f)]
    [SerializeField] private float speedModifierOnBlocking = 0.1f;

    void Start()
    {
        currentStamina = totalStamina;
        initialRechargeSpeed = rechargeSpeed;
        blocRechargeSpeed = rechargeSpeed * speedModifierOnBlocking;
        //rechargeSpeed_FillBar = rechargeSpeed/totalStamina;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            DepleteStamina(20f);
        }

        if (playerCombat.isBlocking)
        {
            //canRecharge = false;
            rechargeSpeed = blocRechargeSpeed;

        }
        else
        {
            //canRecharge = true;
            rechargeSpeed = initialRechargeSpeed;
        }

        if (!isStaminaBarAnimating)
        {
            // if(!canRecharge) 
            // {
            //     if(animateCoroutine_recharge != null)
            //     {
            //         StopCoroutine(animateCoroutine_recharge);
            //         isRechargin_Anim = false;
            //     }
            //     return;
            // }

            RechargeStamina();
        }



        //RotateStaminaBarTowardsPlayer();
    }

    // private void RotateStaminaBarTowardsPlayer()
    // {
    //     Vector3 cameraDir = mainCamera.transform.position - staminaImage_BG.transform.position;

    //     Quaternion lookRotation = Quaternion.LookRotation(cameraDir);

    //     staminaImage_BG.transform.rotation = lookRotation;
    // }

    public void RechargeStamina()
    {
        if (currentStamina >= totalStamina) return;

        if (!isRechargin_Anim)
        {
            animateCoroutine_recharge = StartCoroutine(AnimateStaminaRecharge());
        }

        if (currentStamina > totalStamina)
            currentStamina = totalStamina;
    }

    public void DepleteStamina(float depletionAmount)
    {
        if (currentStamina <= 0)
        {
            isStaminaBarAnimating = false;
            return;
        }

        float absolute_DepletionAmount = Mathf.Abs(depletionAmount);

        currentStamina -= absolute_DepletionAmount;

        //debug
        float targetAmount = currentStamina / totalStamina;
        if (targetAmount < 0)
            targetAmount = 0;

        if (staminaImage_BG != null && staminaImage_Front != null)
        {
            staminaImage_Front.fillAmount = targetAmount;
            if (animateCoroutine != null)
            {
                StopCoroutine(animateCoroutine);
                //isStaminaBarAnimating = false; //
            }
            animateCoroutine = StartCoroutine(AnimateStaminaUpdate(targetAmount));

        }
        //debug

        if (currentStamina < 0)
        {
            //isStaminaBarAnimating = false; //
            currentStamina = 0;
        }

        //Debug.Log($"<color=blue>Stamina Depleted");

    }



    IEnumerator AnimateStaminaUpdate(float targetAmount)
    {
        if (animateCoroutine_recharge != null)
        {
            StopCoroutine(animateCoroutine_recharge);
            isRechargin_Anim = false;


        }

        isStaminaBarAnimating = true;

        yield return new WaitForSeconds(0.35f);



        while (Mathf.Abs(staminaImage_BG.fillAmount - targetAmount) > 0.01) // animate until difference is close enough to 0
        {

            staminaImage_BG.fillAmount = Mathf.MoveTowards(staminaImage_BG.fillAmount, targetAmount,
                Time.deltaTime * staminaBarAnimSpeed);

            yield return null;
        }

        staminaImage_BG.fillAmount = targetAmount;
        isStaminaBarAnimating = false;

    }

    IEnumerator AnimateStaminaRecharge()
    {

        isRechargin_Anim = true;

        while (currentStamina < totalStamina)
        {
            currentStamina += rechargeSpeed * Time.deltaTime;

            float targetAmount = currentStamina / totalStamina;

            staminaImage_Front.fillAmount = Mathf.MoveTowards(staminaImage_Front.fillAmount, targetAmount,
                        rechargeSpeed * Time.deltaTime);

            staminaImage_BG.fillAmount = Mathf.MoveTowards(staminaImage_BG.fillAmount, targetAmount,
                        rechargeSpeed * Time.deltaTime);

            yield return null;

        }
        currentStamina = totalStamina;

        isRechargin_Anim = false;

    }
    
    public void ResetPlayerStamina()
    {
        currentStamina = totalStamina;
        if (staminaImage_BG != null && staminaImage_Front != null)
        {
            staminaImage_BG.fillAmount = 1f;
            staminaImage_Front.fillAmount = 1f;
        }
        if (animateCoroutine != null)
        {
            StopCoroutine(animateCoroutine);
            isStaminaBarAnimating = false;
        }
        if (animateCoroutine_recharge != null)
        {
            StopCoroutine(animateCoroutine_recharge);
            isRechargin_Anim = false;
        }
    }

}
