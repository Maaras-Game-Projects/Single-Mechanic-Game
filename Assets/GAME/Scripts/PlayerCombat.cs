using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] PlayerAnimationManager playerAnimationManager;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] bool canCombo = false;
    [SerializeField] float attackComboDelay = 1f;
    [SerializeField] AnimationClip[] attackAnimClips;
    [SerializeField] int currentAttackComboAnimIndex = 0;
    [SerializeField] private bool isAttacking = false;
    [SerializeField] private Coroutine comboCoroutine;
    [SerializeField] public bool canDetectHit = false;
    [SerializeField] public SwordDamage playerSword;

    [SerializeField] public bool isBlocking = false;
    [SerializeField] public float blockDamageREductionValPercent = 75f;

    

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
        isBlocking = true;

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
}
