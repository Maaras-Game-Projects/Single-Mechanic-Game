using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicComboAttackState : State
{
    [SerializeField] CombatAdvanced_State combatAdvanced_State;
    [SerializeField] IdleState idleState;

    [SerializeField] int maxComboCount = 2;
    [SerializeField] float staminaCost = 30f;
    [SerializeField] float damageModifier = 0.6f;
    [SerializeField] int attacksIndex = 0;

    [SerializeField] List<Attack> availableComboAttacks = new List<Attack>();
    [SerializeField] List<Attack> finalComboAttacks = new List<Attack>();
    [SerializeField] bool isAttacking = false;

    private Coroutine attackWaitCoroutine;

   
    public override void OnEnter()
    {
        
        if(npcRoot.staminaSystem.CurrentStamina < staminaCost)
        {
            //Roll for All combat Strat, or Roll for Defensive Strat based on defensive weight if low on stamina
            Debug.Log("<color=red>Strategy failed= </color>");
            npcRoot.statemachine.SwitchState(combatAdvanced_State);
            
        }

        // Cap max combo attack count

        CapMaxComboCount();

        //Get list for available Combo Attacks

        PopulateAvailableComboAttacks();

        //Get list for final Combo Attacks

        PopulateFinalComboAttacks();

        if(finalComboAttacks.Count == 0)
        {
            //no combo attacks available, so switch statess
            npcRoot.statemachine.SwitchState(combatAdvanced_State);
        }

        npcRoot.staminaSystem.DepleteStamina(staminaCost);

    }

    public override void OnExit()
    {
        finalComboAttacks.Clear();
        availableComboAttacks.Clear();
        attacksIndex = 0;

        //Disable all attack's inStrategyBool                           (FOR NOW inStrategy BOOL IS REDUNDANT)
        //combatAdvanced_State.DisableInStrategyStatusForAttacks();

    }

    public override void TickLogic()
    {
        //npcRoot.LookAtPlayer(1.5f);
        if(isAttacking) 
        {

            npcRoot.RotateOnAttack(npcRoot.lookRotationSpeed);
            //HandleMovementWhileAttacking();
            
            //Debug.Log("ROT");
            return;
        }
        
        isAttacking = true;
        Attack attackToPerform =  finalComboAttacks[attacksIndex];
        string attackAnimName = attackToPerform.attackAnimClip.name;

        npcRoot.PlayAnyActionAnimation(attackAnimName,true);
        npcRoot.currentDamageToDeal = attackToPerform.damage *damageModifier;

        float waitTime = attackToPerform.attackAnimClip.length;
        attackWaitCoroutine = StartCoroutine(OnAttackStrategyComplete(waitTime));
        
        
    }

    IEnumerator OnAttackStrategyComplete(float waitTime)
    {
        
        yield return new WaitForSeconds(waitTime);
        isAttacking = false;

        attacksIndex++;
        if(attacksIndex >= maxComboCount)
        {
            //All attacks in combos are completed, switch state
            npcRoot.statemachine.SwitchState(combatAdvanced_State);
        }
        
    }

    private void PopulateFinalComboAttacks()
    {
        if(availableComboAttacks.Count == 0) return;

        for (int i = 0; i < maxComboCount; i++)
        {
            int randomValue = UnityEngine.Random.Range(0, availableComboAttacks.Count);
            finalComboAttacks.Add(availableComboAttacks[randomValue]);
            availableComboAttacks.RemoveAt(randomValue);
        }
    }

    private void PopulateAvailableComboAttacks()
    {
        foreach (Attack attack in combatAdvanced_State.attacks)
        {
            if (attack.canAddedInCombo && !attack.inStrategy)
            {
                availableComboAttacks.Add(attack);
            }
        }
    }

    private void CapMaxComboCount()
    {
        int attackCount = 0;

        foreach (Attack attack in combatAdvanced_State.attacks)
        {
            if (attack.canAddedInCombo && !attack.inStrategy)
                attackCount++;
        }

        if (maxComboCount > attackCount)
            maxComboCount = attackCount;
    }

    

}
