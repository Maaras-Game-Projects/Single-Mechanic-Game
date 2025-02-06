using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] PlayerAnimationManager playerAnimationManager;
    [SerializeField] bool canCombo = false;
    [SerializeField] float attackComboDelay = 1f;
    [SerializeField] string[] attackAnimationCombos;
    [SerializeField] int currentAttackComboAnimIndex = 0;

    public void StartToAttack()
    {
        if (canCombo)
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
        }
        
       
    }

    IEnumerator EnableComboAttacks(float comboDelayTime)
    {
        canCombo = true;
        yield return new WaitForSeconds(comboDelayTime);
        canCombo = false;
    }
}
