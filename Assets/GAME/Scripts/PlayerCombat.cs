using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] PlayerAnimationManager playerAnimationManager;
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] bool canCombo = false;
    [SerializeField] float attackComboDelay = 1f;
    [SerializeField] AnimationClip[] attackAnimClips;
    [SerializeField] AnimationClip blockAnimClip;
    [SerializeField] int currentAttackComboAnimIndex = 0;
    [SerializeField] private bool isAttacking = false;
    [SerializeField] private Coroutine comboCoroutine;
    [SerializeField] public bool canDetectHit = false;
    [SerializeField] public SwordDamage playerSword;

    [SerializeField] public bool isBlocking = false;
    [SerializeField] public bool isCountering = false;
    [SerializeField] public bool isParrying = false;
    [SerializeField] public bool canCounter = true;
    [SerializeField] public float blockDamageREductionValPercent = 75f;
    [SerializeField] public LayerMask enemyLayerMask;



    void LateUpdate()
    {
        if(isCountering)
        { 
            isCountering = false;
        }
    }

    public void StartToAttack()
    {
        if (isAttacking) return; // cant attack if already attacking

        isAttacking = true;

        if (canCombo) // if combo window is enable switch to next attack clip
        {
            canCombo = false;
            if(comboCoroutine != null)
            {
                StopCoroutine(comboCoroutine);
            }
            currentAttackComboAnimIndex++;
            if (currentAttackComboAnimIndex >= attackAnimClips.Length)
            {
                currentAttackComboAnimIndex = 0;
            }
        }
        else
        {
            currentAttackComboAnimIndex = 0;
        }
        playerAnimationManager.PlayAnyInteractiveAnimation(attackAnimClips[currentAttackComboAnimIndex].name, false, true);
        
        canCombo = true;
        float currentAttackClipDuration = attackAnimClips[currentAttackComboAnimIndex].length;
        StartCoroutine(DisableIsAttacking(currentAttackClipDuration));
        comboCoroutine = StartCoroutine(EnableComboAttackWindow(currentAttackClipDuration + attackComboDelay));
   
        

    }

    public void EnableHitDetection()
    {
        canDetectHit = true;
    }
    public void DisableHitDetection()
    {
        canDetectHit = false;
    }

    public void BlockAttack()
    {
        //ParryAttack();
        
        isParrying =  true;
        float blockAnimClipDuration = blockAnimClip.length;
        StartCoroutine(DisableParryAfterDelay(blockAnimClipDuration));

        isBlocking = true;
        playerLocomotion.canMove = false;
        playerLocomotion.canRotate = false;
        playerAnimationManager.playerAnimator.SetBool("isUsingRootMotion", true);
        playerAnimationManager.playerAnimator.SetBool("inBlocking", true);

        //playerAnimationManager.PlayAnyInteractiveAnimation("swordBlock_1", false, true);

    }

    public void EnableCounter()
    {
        canCounter = true;
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

    public void CounterAttack()
    {
        if(!canCounter) return;
        if(isCountering) return;

        isCountering = true;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5f, enemyLayerMask);
        BaseEnemy closestEnemy = null;
        float closestDistance = Mathf.Infinity; // Start with a very large value

        if (hitColliders.Length > 0)
        {
            foreach (var hitCollider in hitColliders)
            {
                BaseEnemy enemy = hitCollider.GetComponent<BaseEnemy>();
                if (enemy != null && enemy.parryable)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);

                    if (distance < closestDistance) // Check if this enemy is the closest
                    {
                        closestDistance = distance;
                        closestEnemy = enemy;
                    }
                }
            }

            if (closestEnemy != null)
            {
                closestEnemy.OnParried();
                playerAnimationManager.playerAnimator.SetBool("isParrying", true);
                Debug.Log("Parried enemy: " + closestEnemy.gameObject.name);
               
            }
        }
        else
        {
            Debug.Log("No parryable enemies in range.");
        }

         canCounter = false;
    }

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


    // void OnDrawGizmos()
    // {
    //     Vector3 origin = transform.position;
    //     Vector3 direction = transform.forward;
    //     float radius = 5f;

    //     Gizmos.DrawWireSphere(origin,radius);
    // }
}
