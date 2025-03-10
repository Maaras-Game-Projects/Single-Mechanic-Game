using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] PlayerAnimationManager playerAnimationManager;
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] bool canCombo = false;
    [SerializeField] float attackComboDelay = 1f;
    [SerializeField] AnimationClip[] attackAnimClips;
    [SerializeField] AnimationClip blockAnimClip;
    [SerializeField] AnimationClip riposteAnimClip;
    [SerializeField] int currentAttackComboAnimIndex = 0;
    [SerializeField] private bool isAttacking = false;
    [SerializeField] private Coroutine comboCoroutine;
    [SerializeField] public bool canDetectHit = false;
    [SerializeField] public SwordDamage playerSword;

    [SerializeField] public bool isBlocking = false;
    [SerializeField] public bool isCountering = false;
    [SerializeField] public bool isParrying = false;
    [SerializeField]private bool canParry = true;
    [SerializeField] public bool canCounter = true;
    [SerializeField] public bool canRiposte = true;
    [SerializeField] public Image riposteCrossHairImage;

    [SerializeField] private float addedParryTime = .25f;
    [SerializeField] private float block_KnockBack_Force = 2f;

    [SerializeField] public float blockDamageREductionValPercent = 75f;
    [SerializeField] public LayerMask enemyLayerMask;

    private Vector3 centerOfScreen;
    private Ray riposteRay;


    [Space]

    [Header("EVENTS")]
    [Space]

    [SerializeField] private UnityEvent onCloseUpParrySuccess;
    

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

            if(Physics.Raycast(riposteRay, out raycastHit, 100f, enemyLayerMask))
            {
                riposteCrossHairImage.gameObject.SetActive(true);
                riposteCrossHairImage.color = Color.red;
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
        if (isAttacking) return; // cant attack if already attacking

        isAttacking = true;

        if(canRiposte)
        {
            //need to aim at enemy

            RaycastHit riposteHit;

            // Debug visualization
            Debug.DrawRay(riposteRay.origin, riposteRay.direction * 20f, Color.cyan, 10f);

            if(Physics.SphereCast(riposteRay.origin, 2f, riposteRay.direction, out riposteHit, 20f, enemyLayerMask))
            {
          
                if(riposteHit.collider != null)
                {

                    BaseEnemy enemy = riposteHit.collider.GetComponent<BaseEnemy>();
                    if(enemy != null)
                    {
                        if(enemy.isStunned)
                        {
                            
                            TurnTowardsEnemyForRiposte(enemy);
                            playerAnimationManager.PlayAnyInteractiveAnimation(riposteAnimClip.name, false, true);
                            float riposteClipDuration = riposteAnimClip.length;
                            StartCoroutine(DisableIsAttacking(riposteClipDuration));
                            canRiposte = false;
                            return;

                        }
                        
                    }
                    
                }
               
            }

           
            
        }

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

    private void TurnTowardsEnemyForRiposte(BaseEnemy enemy)
    {
        Debug.Log("Turning towards enemy for riposte");
        Vector3 enemyDirection = enemy.transform.position - transform.position;
        enemyDirection.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(enemyDirection);

        //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2.5f);
        transform.rotation = lookRotation;
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

        //AlignWithCamera();

        AttemptCloseUpParry();
        isBlocking = true;
        playerLocomotion.canMove = false;
        playerLocomotion.canRotate = false;
        playerAnimationManager.playerAnimator.SetBool("isUsingRootMotion", true);
        playerAnimationManager.playerAnimator.SetBool("inBlocking", true);

        //playerAnimationManager.PlayAnyInteractiveAnimation("swordBlock_1", false, true);

    }

    public void KnockBackOnBlock()
    {    
        playerLocomotion.playerRigidBody.AddForce(-transform.forward * block_KnockBack_Force, ForceMode.Impulse);
    }

    private void AttemptCloseUpParry()
    {

        if(!canParry) return;
        if(isParrying) return;

        isParrying = true;
        canParry = false;
        float blockAnimClipDuration = blockAnimClip.length;
        StartCoroutine(DisableParryAfterDelay(blockAnimClipDuration + addedParryTime));
    }

    public void OnCloseUpParrySuccess(BaseEnemy enemy)
    {
        canRiposte = true;
        enemy.OnParried();
        playerAnimationManager.playerAnimator.SetBool("isParrying", true);
        onCloseUpParrySuccess.Invoke();
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


    void OnDrawGizmos()
    {
        Vector3 origin = playerLocomotion.mainCamera.transform.position;
        Vector3 direction = transform.forward;
        float radius = 2f;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin,radius);
    }
}
