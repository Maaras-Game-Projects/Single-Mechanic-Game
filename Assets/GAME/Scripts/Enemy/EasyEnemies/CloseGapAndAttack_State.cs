using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A combat strategy state to chase and attack
public class CloseGapAndAttack_State : State
{
    //set it false will not set isPerformingAttackStrategy 
    // to false on complete to link it to another combat strategy
    [SerializeField] bool linkStrategyToCombo = false; 
    [SerializeField] float addedStaminaCost = 5f; 

    [SerializeField] CombatAdvanced_State combatAdvanced_State;
    [SerializeField]private IdleState idleState;

    // if list is empty and this list will be populated with attacks from Combat state
    // that has weights for this strat
    [SerializeField] List<Attack> closeGapAndAttack_Attacks = new List<Attack>();
    [SerializeField] Attack endAttack;
    private Coroutine attackWaitCoroutine;

    void Awake()
    {
        AddAvailableAttacks();
    }

    private void AddAvailableAttacks()
    {
        if (closeGapAndAttack_Attacks.Count == 0)
        {
            foreach (Attack attack in combatAdvanced_State.attacks)
            {
                if (attack.canAddedInCloseGap)
                {
                    closeGapAndAttack_Attacks.Add(attack);
                }
            }
        }
    }

    public override void OnEnter()
    {
       npcRoot.isPerformingAttackStrategy = true;
       npcRoot.isChasingTarget = true;

       endAttack = RollAndGetAttack();

    }

    public override void OnExit()
    {
        if(!linkStrategyToCombo)
        {
            npcRoot.isPerformingAttackStrategy = false;
        }
        
        npcRoot.isChasingTarget = false;
    }

    public override void TickLogic()
    {
        float totalStaminaCost = endAttack.staminaCost + addedStaminaCost;
        if(npcRoot.staminaSystem.CurrentStamina < totalStaminaCost)
        {
            //Roll for All combat Strat, or Roll for Defensive Strat based on defensive weight if low on stamina
            Debug.Log("<color=red>Strategy failed= </color>");
            npcRoot.statemachine.SwitchState(combatAdvanced_State);
            
        }
        else if(!combatAdvanced_State.IsPlayerInCloseRange()) // chase until close to player
        {
            idleState.GoToLocomotionAnimation();
            if(npcRoot.isPlayerInLineOfSight())
            {
                npcRoot.TurnCharacter();
                npcRoot.LookAtPlayer();  
            }
           
            
            //npcRoot.TurnCharacter();
            npcRoot.SetNavMeshAgentDestination(npcRoot.targetTransform.position);
            npcRoot.SetStrafeAnimatorValues_Run();
        }
        else if(linkStrategyToCombo)
        {
            //switch to comboAttack State
        }
        else // perform close range attack
        {
            npcRoot.isPerformingAttackStrategy = true;
            npcRoot.staminaSystem.DepleteStamina(totalStaminaCost);
            npcRoot.currentDamageToDeal = endAttack.damage;
            npcRoot.PlayAnyActionAnimation(endAttack.attackAnimClip.name,true);

            float waitTime = endAttack.attackAnimClip.length;
            attackWaitCoroutine = StartCoroutine(OnAttackStrategyComplete(waitTime));
        }

        
        
    }

    IEnumerator OnAttackStrategyComplete(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        npcRoot.statemachine.SwitchState(combatAdvanced_State);

    }

    private Attack RollAndGetAttack()
    {
        if(closeGapAndAttack_Attacks.Count == 0) return null;

        float totalAttackChance = 0f;
        
        foreach (Attack attack in closeGapAndAttack_Attacks)
        {
            totalAttackChance += attack.weightsByCombatZone.closeRange_Weight;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalAttackChance);

        

        foreach (Attack attack in closeGapAndAttack_Attacks)
        {
            if (randomValue <= attack.weightsByCombatZone.closeRange_Weight)
            {
                return attack;
            }

            randomValue -= attack.weightsByCombatZone.closeRange_Weight;
        }

        return null;
    }
}
