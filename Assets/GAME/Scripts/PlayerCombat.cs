using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] PlayerAnimationManager playerAnimationManager;
    [SerializeField] bool canCombo = false;
    [SerializeField] float attackComboDelay = 1f;
    [SerializeField] AnimationClip[] attackAnimClips;
    [SerializeField] int currentAttackComboAnimIndex = 0;
    [SerializeField] private bool isAttacking = false;
    [SerializeField] private Coroutine comboCoroutine;

    

    public void StartToAttack()
    {
        /*if (canCombo)
        {
            currentAttackComboAnimIndex++;
            playerAnimationManager.PlayAnyInteractiveAnimation(attackAnimationCombos[currentAttackComboAnimIndex], false, true);

            if(currentAttackComboAnimIndex ==  attackAnimationCombos.Length - 1)
            {
                currentAttackComboAnimIndex = 0;
                canCombo = false;
            }
        }
        else
        {
            playerAnimationManager.PlayAnyInteractiveAnimation(attackAnimationCombos[currentAttackComboAnimIndex], false, true);
            StartCoroutine(EnableComboAttacks(attackComboDelay));
        }*/


        /*if (isAttacking && canCombo)
        {
            currentAttackComboAnimIndex++;
            if (currentAttackComboAnimIndex >= attackAnimCLips.Length)
            {
                currentAttackComboAnimIndex = 0;
            }
        }
        else
        {
            currentAttackComboAnimIndex = 0;
        }

        playerAnimationManager.PlayAnyInteractiveAnimation(attackAnimCLips[currentAttackComboAnimIndex].name, false, true);
        StartCoroutine(EnableComboAttackWindow(attackComboDelay));
        isAttacking = true;*/

        if (isAttacking) return;

        isAttacking = true;

        if (canCombo)
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

        Debug.Log("attack clip id = " + currentAttackComboAnimIndex);
        playerAnimationManager.PlayAnyInteractiveAnimation(attackAnimClips[currentAttackComboAnimIndex].name, false, true);
        
        canCombo = true;
        Debug.Log("Set Combo true");
        float currentAttackClipDuration = attackAnimClips[currentAttackComboAnimIndex].length;
        StartCoroutine(DisableIsAttacking(currentAttackClipDuration));
        comboCoroutine = StartCoroutine(EnableComboAttackWindow(currentAttackClipDuration + attackComboDelay));
   
        

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
        Debug.Log("Set Combo false");
        //currentAttackComboAnimIndex = 0;
    }
}
