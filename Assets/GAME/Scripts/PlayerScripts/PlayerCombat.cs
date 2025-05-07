using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] PlayerAnimationManager playerAnimationManager;
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] MyInputManager myInputManager;
    [SerializeField] bool canCombo = false;

    [SerializeField] string comboTriggerBool;
    [SerializeField] public AnimationClip startingAttackClip; // combo start clip is enpugh i guess check whie refactoring
    [SerializeField] AnimationClip blockAnimClip;
    [SerializeField] AnimationClip riposteAnimClip;
    [SerializeField] AnimationClip parryAnimClip;
    [SerializeField] private bool isAttacking = false;
    [SerializeField] private Coroutine rotationCoroutine;
    [SerializeField] private Coroutine disableAttackCoroutine;
    [SerializeField] public bool canDetectHit = false;
    [SerializeField] public SwordDamage playerSword;

    [SerializeField] public bool isBlocking = false;
    [SerializeField] public bool isCountering = false;
    [SerializeField] public bool isParrying = false;
    
    [Space] //Currently being used for parry
    [SerializeField] public bool isParrying_Solo = false;

    //Currently being used for parry
    [SerializeField]private bool parrySolo_Begin;
    public bool ParrySolo_Begin => parrySolo_Begin;
    [Space]

    [SerializeField] public bool isInvincible = false;
    [SerializeField]private bool canParry = true;
    [SerializeField] public bool canCounter = true;
    [SerializeField] private bool canRiposte = true;

    public bool CanRiposte => canRiposte;
    [SerializeField] public Image riposteCrossHairImage;

    [SerializeField] private float addedParryTime = .25f;
    [SerializeField] private float maxHitDetectionDuration = .25f;
    [SerializeField] private float riposteDuration = 2.5f;
    [SerializeField] private float block_KnockBack_Force = 2f;

    [SerializeField] public float blockDamageREductionValPercent = 75f;
    [SerializeField] public LayerMask enemyLayerMask;

    private Vector3 centerOfScreen;
    private Ray riposteRay;

    [Space]
    [Header("Stamina Cost Variables")]
    [Space]

    [SerializeField] StaminaSystem_Player staminaSystem_Player;
    [SerializeField] private float attackStaminaCost = 15f;
    [SerializeField] private float parryStaminaCost = 25f;
    [SerializeField] private float riposteStaminaCost = 18f;
    [SerializeField] private float blockHitStaminaCost = 30f;
    public float BlockHitStaminaCost => blockHitStaminaCost;
    public bool IsAttacking => isAttacking;


    [Space]

    [Header("EVENTS")]
    [Space]

    //[SerializeField] private UnityEvent onCloseUpParrySuccess;
    [SerializeField] private UnityEvent onCloseUpSoloParrySuccess;
    [SerializeField]private bool isStunned;
    [SerializeField] private AnimationClip stunStartAnimationClip;
    [SerializeField] private AnimationClip stunAnimationClip;

    [SerializeField]private bool isKnockedDown;
    [SerializeField] private AnimationClip knockDownStartAnimationClip;
    [SerializeField] private AnimationClip knockDownEndAnimationClip;

    public bool IsStunned => isStunned;

    

    //[SerializeField] private UnityEvent onCounterSuccess;


    void Start()
    {
        centerOfScreen = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    }

    void Update()
    {
        
        if(canRiposte)
        {
            riposteRay = playerLocomotion.mainCamera.ScreenPointToRay(centerOfScreen);
            RaycastHit raycastHit;

            if(Physics.Raycast(riposteRay, out raycastHit, 5f, enemyLayerMask))
            {
                riposteCrossHairImage.gameObject.SetActive(true);
            }
            else
            {
                riposteCrossHairImage.gameObject.SetActive(false);
            }

        }
        else
        {
            riposteCrossHairImage.gameObject.SetActive(false);
        }

        if(isStunned || isKnockedDown) // setting these values forcibly to prevent player from moving or rotating since it resetting somewhere and conflicting
        {
            playerLocomotion.canMove = false;
            playerLocomotion.canRotate = false;
            playerAnimationManager.playerAnimator.SetBool("isUsingRootMotion", true);
            playerAnimationManager.playerAnimator.SetBool("InAnimAction", true);
        }
      
    }
    void LateUpdate()
    {
        if(isCountering)
        { 
            isCountering = false;
        }
    }

    public void StartToAttack()
    {
        if(staminaSystem_Player.CurrentStamina < attackStaminaCost) return; // not enough stamina
        if( playerLocomotion.isDodging) return; // cant attack if dodging
        if( !playerLocomotion.isGrounded) return; // cant attack if jumping or falling
        if(isBlocking) return;
        if(playerAnimationManager.inAnimActionStatus) return;
        if(playerAnimationManager.playerAnimator.IsInTransition(1)
            || playerAnimationManager.playerAnimator.IsInTransition(2)) return;  // checking if block animation to empty state transition is happening
        playerAnimationManager.playerAnimator.Play("Empty State",1);
        playerAnimationManager.playerAnimator.SetLayerWeight(1,0);
        if(canCombo)
        {
            playerLocomotion.canRotate = true;
            // Vector3 targetDirection_1 = Vector3.zero;
            // Quaternion targetRotation_1 = Quaternion.identity;
            // Quaternion playerRotation_1 = Quaternion.identity;

                    
            // targetDirection_1 = playerLocomotion.mainCamera.transform.forward * myInputManager.verticalMovementInput;
            // targetDirection_1 = targetDirection_1 + playerLocomotion.mainCamera.transform.right * myInputManager.horizontalMovementInput;
            // targetDirection_1.Normalize();
            // targetDirection_1.y = 0;

            // if(targetDirection_1 == Vector3.zero)
            // {
            //     targetDirection_1 = transform.forward;
            // }

            // targetRotation_1 = Quaternion.LookRotation(targetDirection_1);
            // //playerRotation_1 = Quaternion.Slerp(transform.rotation, targetRotation_1, 1.5f * Time.deltaTime);

            // transform.rotation = playerRotation_1;
            // Debug.Log("<color=red>PLAYER rOTAION = </color> + " + playerRotation_1);
            // Debug.Log("<color=red>tRANSFORM rOTAION = </color> + " + transform.rotation);

            playerAnimationManager.playerAnimator.SetBool(comboTriggerBool, true);
            staminaSystem_Player.DepleteStamina(attackStaminaCost);
            return;
        }

        if (isAttacking) return; // cant attack if already attacking
        if( playerAnimationManager.inAnimActionStatus) return; // cant attack if in animation

        // Vector3 targetDirection = Vector3.zero;
        // Quaternion targetRotation = Quaternion.identity;
        // Quaternion playerRotation = Quaternion.identity;

               
        // targetDirection = playerLocomotion.mainCamera.transform.forward * myInputManager.verticalMovementInput;
        // targetDirection = targetDirection + playerLocomotion.mainCamera.transform.right * myInputManager.horizontalMovementInput;
        // targetDirection.Normalize();
        // targetDirection.y = 0;

        // if(targetDirection == Vector3.zero)
        // {
        //     targetDirection = transform.forward;
        // }

        // targetRotation = Quaternion.LookRotation(targetDirection);
        // playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, 1.5f * Time.deltaTime);

        // transform.rotation = playerRotation;
        //playerLocomotion.canRotate = true;
        isAttacking = true;
        
        bool isRiposteSuccess = HandleRiposte();

        if (isRiposteSuccess)
            return;
        
        
        playerAnimationManager.PlayAnyInteractiveAnimation(startingAttackClip.name, false, true,false,true);
        //playerLocomotion.canRotate = true;

        float currentAttackClipDuration = startingAttackClip.length;
        //disableAttackCoroutine = StartCoroutine(DisableIsAttacking(currentAttackClipDuration));
 
        staminaSystem_Player.DepleteStamina(attackStaminaCost);



    }

    public void DisableIsAttacking()
    {
        isAttacking = false;
    }

    public void EnableCanCombo()
    {
        canCombo = true;
    }
    public void DisableCanCombo()
    {
        canCombo = false;
    }

    private bool HandleRiposte()
    {
        if (!canRiposte) return false;
        if(staminaSystem_Player.CurrentStamina < 5f) return false; // not enough stamina

        //need to aim at enemy

        RaycastHit riposteHit;

        // Debug visualization
        // Debug.DrawRay(riposteRay.origin, riposteRay.direction * 5f, Color.cyan, 12f);
        // Debug.DrawRay(riposteRay.origin, riposteRay.direction * 8f, Color.yellow, 7.5f);

        if (Physics.SphereCast(riposteRay.origin, 2f, riposteRay.direction, out riposteHit, 20f, enemyLayerMask))
        {

            if (riposteHit.collider != null)
            {

                //BaseEnemy enemy = riposteHit.collider.GetComponent<BaseEnemy>();
                NPC_Root enemy = riposteHit.collider.GetComponent<NPC_Root>();
                if (enemy != null)
                {
                    if (enemy.IsStunned)
                    {

                        TurnTowardsEnemyAndRiposte(enemy.transform);
                        return true;

                    }
                    else
                    {
                        canRiposte = false;
                       
                    }

                }

            }

        }
        else
        {
            canRiposte = false;
        }

        return false;
    }

    private void TurnTowardsEnemyAndRiposte(Transform enemyTransform)
    {
        if(rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }

        StartCoroutine(RotateTowards(enemyTransform.position, .15f, () =>
        {

            playerAnimationManager.PlayAnyInteractiveAnimation(riposteAnimClip.name, false, true);
            float riposteClipDuration = riposteAnimClip.length;
            StartCoroutine(DisableIsAttacking(riposteClipDuration));
            staminaSystem_Player.DepleteStamina(riposteStaminaCost);
            canRiposte = false;

        }));
    }

    IEnumerator RotateTowards(Vector3 targetPosition,float duration,Action OnRotationComplete)
    {
        Quaternion startRotation = transform.rotation;
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        float elapsedTime = 0f;

        while(elapsedTime < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;

        OnRotationComplete?.Invoke();
        
    }

    public void EnableHitDetection()
    {
        canDetectHit = true;
        //Debug.Log("<color=red>hit detection enabled</color>");
    }
    public void DisableHitDetection()
    {
        canDetectHit = false;
        //Debug.Log("<color=green>hit detection disabled</color>");
    }

    public void EnableInvinciblity()
    {
        isInvincible = true;

    }
    public void DisableInvinciblity()
    {
        isInvincible = false;
        
    }

    public void DisableInvinciblityInDelay(float duration)
    {
        StartCoroutine(DisableInvinciblityAfterDelay(duration));
    }

    public void DisableHitDetectionInDelay(float duration)
    {
        StartCoroutine(DisableHitDetectionAfterDelay(duration));
    }

    IEnumerator DisableInvinciblityAfterDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        isInvincible = false;
        //Debug.Log("<color=yellow>hit detection disabled</color>");
    }


    IEnumerator DisableHitDetectionAfterDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        canDetectHit = false;
        //Debug.Log("<color=yellow>hit detection disabled</color>");
    }

    public void BlockAttack()
    {
        if(isBlocking) return;
        if( playerLocomotion.isJumping) return; 
        if( playerLocomotion.isDodging) return; 
        if( isAttacking) return; 
        if( !playerLocomotion.isGrounded) return; 
        
        
        DisableHitDetection();
        //CounterAttack();

        //AttemptCloseUpParry();
        isBlocking = true;
        //myInputManager.walkInput = true;
        //playerLocomotion.canMove = false;
        //playerLocomotion.canRotate = false;
        playerAnimationManager.playerAnimator.SetBool("isUsingRootMotion", true);
        playerAnimationManager.playerAnimator.SetBool("Block_test", true);
        //playerAnimationManager.playerAnimator.SetBool("inBlocking", true);

        //playerAnimationManager.PlayAnyInteractiveAnimation("swordBlock_1", false, true);

    }

    
    public void KnockBackOnBlock()
    {    
        playerLocomotion.playerRigidBody.linearVelocity = Vector3.zero;
        playerLocomotion.playerRigidBody.AddForce(-transform.forward * block_KnockBack_Force, ForceMode.Impulse);
    }

    public void KnockBackOnBlockDelayed(float duration)
    {
        StartCoroutine(KnockBackAfterDuration(duration));
    }

    IEnumerator KnockBackAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        KnockBackOnBlock();
       
    }

    public void GetStunned()
    {
        if (isStunned) return; // already stunned

        isStunned = true;
        playerAnimationManager.PlayAnyInteractiveAnimation(stunStartAnimationClip.name, true, true);
        float waitTime = stunStartAnimationClip.length + stunAnimationClip.length; // Adjust the duration as needed
        StartCoroutine(DelayStun(waitTime - .75f));
    }

    IEnumerator DelayStun(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        isStunned = false;
        playerAnimationManager.playerAnimator.SetBool("isUsingRootMotion", false);
        playerAnimationManager.playerAnimator.SetBool("InAnimAction", false);
        playerLocomotion.canMove = true;
        playerLocomotion.canRotate = true;
       
    }

    // in future we can add a knockback force to this animation
    public void GetKnockedDown()
    {
        if (isKnockedDown) return; 
        isKnockedDown = true;
        playerAnimationManager.PlayAnyInteractiveAnimation(knockDownStartAnimationClip.name, true, true);
        float waitTime = knockDownStartAnimationClip.length + knockDownEndAnimationClip.length; // Adjust the duration as needed
        StartCoroutine(OnKnockBackComplete(waitTime - .25f));
    }

    IEnumerator OnKnockBackComplete(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        isKnockedDown = false;
        playerAnimationManager.playerAnimator.SetBool("isUsingRootMotion", false);
        playerAnimationManager.playerAnimator.SetBool("InAnimAction", false);
        playerLocomotion.canMove = true;
        playerLocomotion.canRotate = true;
       
    }

    public void Parry()
    {
        if (isAttacking) return; // cant attack if already attacking
        if( playerLocomotion.isDodging) return; // cant attack if dodging
        if( !playerLocomotion.isGrounded) return; // cant attack if jumping or falling
        if( playerAnimationManager.inAnimActionStatus) return; // cant attack if in animation

        if(staminaSystem_Player.CurrentStamina < parryStaminaCost) return; // not enough stamina

        if(isParrying_Solo) return;

        isParrying_Solo = true;
        playerAnimationManager.PlayAnyInteractiveAnimation(parryAnimClip.name, true, true);
        float waitTime = parryAnimClip.length;
        StartCoroutine(DisableIsParrying_Solo_Delayed(waitTime));

        staminaSystem_Player.DepleteStamina(parryStaminaCost);
    }

    IEnumerator DisableIsParrying_Solo_Delayed(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        isParrying_Solo = false;
    }

    public void BeginParry_Solo()
    {
        parrySolo_Begin = true;
    }

    public void EndParry_Solo()
    {
        parrySolo_Begin = false;
    }

    public void OnCloseUpSoloParrySuccess(NPC_Root enemy)
    {
        if(!IsEnemyInsidePlayerFOV(enemy.transform))
            return;
       
        canRiposte = true;
        StartCoroutine(DisableRiposteAfterDelay(riposteDuration));
        StartCoroutine(DisableSloMoAfterDelay(.65f));
        enemy.OnParried();
       
        Time.timeScale = 0.5f; // Slow down time for a brief moment
        onCloseUpSoloParrySuccess.Invoke();
    }

    IEnumerator DisableSloMoAfterDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        Time.timeScale = 1f; 
       
    }

    // private void AttemptCloseUpParry()
    // {

    //     if(!canParry) return;
    //     if(isParrying) return;

    //     isParrying = true;
    //     canParry = false;
    //     float blockAnimClipDuration = blockAnimClip.length;
    //     StartCoroutine(DisableParryAfterDelay(blockAnimClipDuration + addedParryTime));
    // }

    // public void OnCloseUpParrySuccess(BaseEnemy enemy)
    // {
    //     if(!IsEnemyInsidePlayerFOV(enemy))
    //         return;
       
    //     canRiposte = true;
    //     StartCoroutine(DisableRiposteAfterDelay(riposteDuration));
    //     enemy.OnParried();
    //     playerAnimationManager.playerAnimator.SetBool("isParrying", true);
    //     onCloseUpParrySuccess.Invoke();
    // }

    private bool IsEnemyInsidePlayerFOV(Transform enemyTransform)
    {
        Vector3 directionToEnemy = enemyTransform.position - transform.position;

        float dotProduct = Vector3.Dot(transform.forward, directionToEnemy);
        float fovThreshold = Mathf.Cos(playerLocomotion.playerFOV * 0.5f + 0.1f);

        if(dotProduct < fovThreshold)
        {
            Debug.Log("<color=red> Enemy Outside FOV</color>");
            return false;
        }
        else
        {
            Debug.Log("<color=green> Enemy Inside FOV</color>");
            return true;
        }
    }

    public void EnableCounter()
    {
        canCounter = true;
    }

     public void AllowParry()
    {
        canParry = true;
    }

    public void DisableParry()
    {
        isParrying = false;
    }

    IEnumerator DisableParryAfterDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        isParrying = false;
       
    }

    IEnumerator DisableRiposteAfterDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        canRiposte = false;
       
    }


    // public void CounterAttack()
    // {
    //     if(!canCounter) return;
    //     if(isCountering) return;

    //     isCountering = true;

    //     Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5f, enemyLayerMask);
    //     BaseEnemy closestEnemy = null;
    //     float closestDistance = Mathf.Infinity; // Start with a very large value

    //     if (hitColliders.Length > 0)
    //     {
    //         foreach (var hitCollider in hitColliders)
    //         {
    //             BaseEnemy enemy = hitCollider.GetComponent<BaseEnemy>();
    //             if (enemy != null && enemy.parryable && !enemy.isDead)
    //             {
    //                 float distance = Vector3.Distance(transform.position, enemy.transform.position);

    //                 if (distance < closestDistance) // Check if this enemy is the closest
    //                 {
    //                     closestDistance = distance;
    //                     closestEnemy = enemy;
    //                 }
    //             }
    //         }

    //         if (closestEnemy != null)
    //         {
    //             OnCounterSuccess(closestEnemy);
    //             //closestEnemy.OnParried();
    //             //playerAnimationManager.playerAnimator.SetBool("isParrying", true);
    //             Debug.Log("<color=green>Parried enemy: </color>" + closestEnemy.gameObject.name);
               
    //         }
    //     }
    //     else
    //     {
    //         Debug.Log("No parryable enemies in range.");
    //     }

    //      canCounter = false;
    // }

    // IEnumerator RotateOnCounter(BaseEnemy baseEnemy,float duration)
    // {
    //     Quaternion startRotation = transform.rotation;
    //     Vector3 direction = baseEnemy.transform.position - transform.position;
    //     direction.y = 0;
    //     Quaternion targetRotation = Quaternion.LookRotation(direction);

    //     float elapsedTime = 0f;

    //     while(elapsedTime < duration)
    //     {
    //         transform.rotation = Quaternion.Slerp(startRotation, targetRotation, (elapsedTime / duration));
    //         elapsedTime += Time.deltaTime;
    //         yield return null;
    //     }

    //     transform.rotation = targetRotation;

    // }

    // IEnumerator ReachEnemyOnCounter(BaseEnemy baseEnemy,float duration,float stoppingDistance)
    // {
    //     Vector3 enemyDirection = baseEnemy.transform.position - transform.position;
    //     enemyDirection.Normalize();

    //     Vector3 destination = baseEnemy.transform.position - (enemyDirection * stoppingDistance);

    //     float elapsedTime = 0f;

    //     while(elapsedTime < duration)
    //     {
    //         transform.position = Vector3.Lerp(transform.position,destination,elapsedTime/duration);
    //         elapsedTime += Time.deltaTime;
    //         yield return null;
    //     }

    //     transform.position = destination;

    // }

    // public void OnCounterSuccess(BaseEnemy enemy)
    // {
       
    //     StartCoroutine(RotateOnCounter(enemy,.15f));
    //     StartCoroutine(ReachEnemyOnCounter(enemy,0.15f,1));
       
    //     //canRiposte = true;
    //     //StartCoroutine(DisableRiposteAfterDelay(riposteDuration));
    //     enemy.OnParried();
    //     playerAnimationManager.playerAnimator.SetBool("isParrying", true);
    //     onCounterSuccess.Invoke();
    // }

    IEnumerator DisableIsAttacking(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        isAttacking = false;
    }

    IEnumerator EnableComboAttackWindow(float comboDelayTime)
    { 
        yield return new WaitForSeconds(comboDelayTime);
        canCombo = false;       
    }


    void OnDrawGizmos()
    {
        // Vector3 origin = transform.position;
        // Vector3 direction = transform.forward;
        // float radius = 5f;
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireSphere(origin,radius);
    }
}

public class PlayerAttack
{
    public string attackName;
    public AnimationClip attackAnimClip;
    public float attackStaminaCost;
    //public float attackDamage;
    // public float attackStaminaCost;
    // public float attackSpeed;
    // public float attackRange;
    // public bool isParryable;
}
