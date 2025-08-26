using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EternalKeep
{
    public class DynamicComboAttackState : State, IEnemyStateReset
    {
        [SerializeField] CombatAdvanced_State combatAdvanced_State;
        [SerializeField] IdleState idleState;
        [SerializeField] CloseGapAndAttack_State closeGapAndAttack_State;
        [SerializeField] CloseGapBlendAndAttack closeGapBlendAndAttack_State;

        [SerializeField] int maxComboCount = 2;
        [SerializeField] float staminaCost = 30f;
        [SerializeField] float linkStratstaminaCost = 15f;
        [SerializeField] float damageModifier = 0.6f;
        [SerializeField] int attacksIndex = 0;
        [SerializeField] int attackAnimsLayer = 1;
        [SerializeField] float comboTransitionDuration_Common = 0.1f;

        [SerializeField] List<Attack> availableComboAttacks = new List<Attack>();
        [SerializeField] List<Attack> finalComboAttacks = new List<Attack>();
        //[SerializeField] bool isAttacking = false;

        private Coroutine attackWaitCoroutine;
        private bool canSwitchToCombatState = false;
        [SerializeField] private MidCombatMovement midCombatMovement;

        public float LinkStratstaminaCost => linkStratstaminaCost;

        bool dynamicComboStarted = false;

        [Space]
        [Header("Fixed Combat Variables")]
        [Space]

        [SerializeField] bool useFixedCombos = false;
        [SerializeField] string comboIndexString;
        [SerializeField] string canComboBoolString;

        [SerializeField] int comboIndex;
        [SerializeField] int comboLimit;

        [SerializeField] List<FixedComboAttack> fixedComboAttacks = new List<FixedComboAttack>();

        [SerializeField] FixedComboAttack currentFixedComboAttack;


        public override void OnEnter()
        {
            float endStaminaCost;

            if (combatAdvanced_State.CurrentCombatStrategy == CommonCombatStrategies.CloseGapBlend_And_AttackWithCombo)
            {
                endStaminaCost = linkStratstaminaCost + closeGapBlendAndAttack_State.AddedStaminaCost;
            }
            else if (combatAdvanced_State.CurrentCombatStrategy == CommonCombatStrategies.CloseGapAndAttack_Combo)
            {
                endStaminaCost = linkStratstaminaCost + closeGapAndAttack_State.AddedStaminaCost;
            }
            else
            {
                endStaminaCost = staminaCost;
            }

            if (endStaminaCost == staminaCost)
            {
                if (npcRoot.staminaSystem.CurrentStamina < endStaminaCost)
                {
                    //Roll for All combat Strat, or Roll for Defensive Strat based on defensive weight if low on stamina
                    Debug.Log("<color=red>Strategy failed= </color>");
                    canSwitchToCombatState = true;
                    StartCoroutine(SwitchToCombatState_Delayed(0.1f));
                    return;

                }

            }

            HandleMidCombatMovementAnimation();


            if (!useFixedCombos)
            {
                // Cap max combo attack count

                CapMaxComboCount();

                //Get list for available Combo Attacks

                PopulateAvailableComboAttacks();

                //Get list for final Combo Attacks

                PopulateFinalComboAttacks();

                if (finalComboAttacks.Count == 0)
                {
                    //no combo attacks available, so switch statess
                    StartCoroutine(SwitchToCombatState_Delayed(0.1f));
                    return;
                }

                dynamicComboStarted = true;
            }
            else
            {
                SetCanComboTriggerStatus(true);
                currentFixedComboAttack = GetFixedComboAttackByWeight();
            }


            if (currentFixedComboAttack == null)
            {
                StartCoroutine(SwitchToCombatState_Delayed(0.1f));
                return;
            }
            comboLimit = DetermineComboLimit();
            npcRoot.staminaSystem.DepleteStamina(endStaminaCost);


        }

        private int DetermineComboLimit()
        {
            float maxLimitRandom = UnityEngine.Random.Range(0.01f, 100f);

            if (maxLimitRandom <= currentFixedComboAttack.maxChainingChance)
            {
                return currentFixedComboAttack.maxChainCount;
            }
            else
            {
                int minLimitRandom = UnityEngine.Random.Range(currentFixedComboAttack.minChainCount,currentFixedComboAttack.maxChainCount);
                return minLimitRandom;
            }
        }

        private FixedComboAttack GetFixedComboAttackByWeight()
        {
            if (fixedComboAttacks.Count == 0) return null;

            float totalAttackChance = 0f;

            foreach (FixedComboAttack attack in fixedComboAttacks)
            {
                totalAttackChance += attack.attackWeight;
            }

            float randomValue = UnityEngine.Random.Range(0f, totalAttackChance);



            foreach (FixedComboAttack attack in fixedComboAttacks)
            {
                if (randomValue <= attack.attackWeight)
                {
                    return attack;
                }

                randomValue -= attack.attackWeight;
            }

            return null;
        }

        public override void OnExit()
        {
            finalComboAttacks.Clear();
            availableComboAttacks.Clear();
            attacksIndex = 0;
            canSwitchToCombatState = false;
            npcRoot.DisableCanKnockBackOnAttack();
            SetCanComboTriggerStatus(false);
            ResetFixedComboIndex();

            npcRoot.SetPerformingComboAttacksStatus(false);
            npcRoot.DisableComboChaining();

            //Disable all attack's inStrategyBool                           (FOR NOW inStrategy BOOL IS REDUNDANT)
            //combatAdvanced_State.DisableInStrategyStatusForAttacks();

        }

        private void SetCanComboTriggerStatus(bool value)
        {
            if(canComboBoolString == "") return;
            npcRoot.animator.SetBool(canComboBoolString, value);
        }

        public override void TickLogic()
        {
            if (canSwitchToCombatState) return;
            
            idleState.FallBackToDefaultStateOnPlayerDeath();
            Debug.Log($"<color=cyan> Dynamic Combo Attack State Tick</color>");
            //npcRoot.LookAtPlayer(1.5f);
            if (!useFixedCombos)
            {
                HandleDynamicComboAttack();
            }
            else
            {
                HandleFixedComboAttack();
            }


        }

        private void HandleFixedComboAttack()
        {
            if (npcRoot.IsPerformingComboAttacks)
            {

                npcRoot.RotateOnAttack(npcRoot.lookRotationSpeed);
                //HandleMidCombatMovementAnimation();
                npcRoot.UpdateMoveDirection();
                if (!npcRoot.animator.GetBool(canComboBoolString)  && !npcRoot.isInteracting)
                {
                    //Debug.Log("<color=white>Max Combo Complete Switching State</color>");
                    npcRoot.SetPerformingComboAttacksStatus(false);
                    npcRoot.statemachine.SwitchState(combatAdvanced_State);
                }

                //Debug.Log("ROT");
                return;
            }
            npcRoot.SetPerformingComboAttacksStatus(true);
            npcRoot.PlayAnyActionAnimation(currentFixedComboAttack.attackAnimClip.name, true);
            // npcRoot.currentDamageToDeal = currentFixedComboAttack.comboDamageValues[comboIndex] * damageModifier;

            // npcRoot.canAttackKnockback = currentFixedComboAttack.canKnockbackValues[comboIndex];


        }

        //will be called in npcRoot which will be called in animation events of combo attack clips
        public void UpdateFixedComboChain()
        {
            if (!npcRoot.IsPerformingComboAttacks) return;
            if (comboIndex < comboLimit)
            {
                npcRoot.currentDamageToDeal = currentFixedComboAttack.comboDamageValues[comboIndex] * damageModifier;
                npcRoot.canAttackKnockback = currentFixedComboAttack.canKnockbackValues[comboIndex];
                comboIndex++;
                npcRoot.animator.SetInteger(comboIndexString, comboIndex);

            }
            else
            {
                if (comboIndex == comboLimit)
                {
                    npcRoot.currentDamageToDeal = currentFixedComboAttack.comboDamageValues[comboIndex] * damageModifier;
                    npcRoot.canAttackKnockback = currentFixedComboAttack.canKnockbackValues[comboIndex];
                }
                //comboIndex = 0;
                    //npcRoot.animator.SetInteger(comboIndexString, comboIndex);
                SetCanComboTriggerStatus(false);
                //Debug.Log($"<color=green>Max Combo Chain Reached, cannot chain more, comboindex = {comboIndex}</color>");
            }
            
        }

        public void UpdateCanComboOnAnimExit()
        {
            if (!npcRoot.IsPerformingComboAttacks) return;
            if (comboIndex == comboLimit)
            {
                if (comboIndex == 0)
                {
                    SetCanComboTriggerStatus(false);
                    return;
                }
                else
                {
                    comboIndex++;
                }

            }
            else if (comboIndex > comboLimit)
            {

                //comboIndex = 0;
                //npcRoot.animator.SetInteger(comboIndexString, comboIndex);
                SetCanComboTriggerStatus(false);
                //Debug.Log($"<color=yellow>Max Combo Chain Reached on Anim Exit, cannot chain more, comboindex = {comboIndex}</color>");

            }
            
        }

        public void ResetFixedComboIndex()
        {
            if(comboIndexString == "") return;
            comboIndex = 0;
            npcRoot.animator.SetInteger(comboIndexString, comboIndex);
            //Debug.Log("<color=blue>Resetting Fixed Combo Index to 0</color>");
        }

        private void HandleDynamicComboAttack()
        {
            // if (!npcRoot.isInteracting && !npcRoot.IsPerformingComboAttacks && !dynamicComboStarted)
            // {
            //     npcRoot.statemachine.SwitchState(combatAdvanced_State);
            //     return;
            // }
            if (npcRoot.IsPerformingComboAttacks)
                {

                    npcRoot.RotateOnAttack(npcRoot.lookRotationSpeed);
                    //HandleMidCombatMovementAnimation();
                    npcRoot.UpdateMoveDirection();

                    //Debug.Log("ROT");
                    //return;
                }
            if (npcRoot.CanChainCombo)
            {
                npcRoot.DisableComboChaining();
                //npcRoot.SetPerformingComboAttacksStatus(true);
                attacksIndex++;
                if (attacksIndex >= maxComboCount)
                {
                    //All attacks in combos are completed, switch state
                    npcRoot.SetPerformingComboAttacksStatus(false);
                    dynamicComboStarted = false;
                    npcRoot.statemachine.SwitchState(combatAdvanced_State);
                    return;
                }
                Attack attack = finalComboAttacks[attacksIndex];
                string attackName = attack.attackAnimClip.name;
                npcRoot.PlayAnyActionAnimation(attackName, attackAnimsLayer, true, attack.comboTransitionTime, attack.comboEntryNormalizedTransitionTime);
                npcRoot.currentDamageToDeal = attack.damage * damageModifier;

                //need to add logic for knockback to attackToPerform and parrayable only on last attack in combo
                npcRoot.canAttackKnockback = attack.canAttackKnockback;

                // float waitTime = attack.attackAnimClip.length;
                // if(attackWaitCoroutine != null)
                //         StopCoroutine(attackWaitCoroutine);
                // attackWaitCoroutine = StartCoroutine(DisableIsAttackingInDelay(waitTime));
                return;
            }



            // isAttacking = true;
            // Attack attackToPerform = finalComboAttacks[attacksIndex];
            // string attackAnimName = attackToPerform.attackAnimClip.name;

            // npcRoot.PlayAnyActionAnimation(attackAnimName, true);
            // npcRoot.currentDamageToDeal = attackToPerform.damage * damageModifier;

            // //need to add logic for knockback to attackToPerform and parrayable only on last attack in combo
            // npcRoot.canAttackKnockback = attackToPerform.canAttackKnockback;

            // float waitTime = attackToPerform.attackAnimClip.length;
            // attackWaitCoroutine = StartCoroutine(OnAttackStrategyComplete(waitTime));
            if (!npcRoot.IsPerformingComboAttacks)
            {
                npcRoot.SetPerformingComboAttacksStatus(true);
                Attack attackToPerform = finalComboAttacks[attacksIndex];
                string attackAnimName = attackToPerform.attackAnimClip.name;

                if (attacksIndex == 0)
                {
                    npcRoot.PlayAnyActionAnimation(attackAnimName, true);
                    npcRoot.currentDamageToDeal = attackToPerform.damage * damageModifier;

                    //need to add logic for knockback to attackToPerform and parrayable only on last attack in combo
                    npcRoot.canAttackKnockback = attackToPerform.canAttackKnockback;

                    // float waitTime = attackToPerform.attackAnimClip.length;
                    // if(attackWaitCoroutine != null)
                    //     StopCoroutine(attackWaitCoroutine);
                    // attackWaitCoroutine = StartCoroutine(DisableIsAttackingInDelay(waitTime));
                }
            }

        }

        // //fallback to disable isAttack if combo is interupted by other anims
        // IEnumerator DisableIsAttackingInDelay(float waitTime)
        // {
        //     yield return new WaitForSeconds(waitTime);
        //     Debug.Log("<color=green>Disabling COMBO isAttacking</color>");
        //     isAttacking = false;
        // }

        IEnumerator OnAttackStrategyComplete(float waitTime)
        {

            yield return new WaitForSeconds(waitTime);
            //isAttacking = false;

            attacksIndex++;

            combatAdvanced_State.UpdateCurrentCombatZone();
            if (combatAdvanced_State.CurrentCombatZone == CombatZone.Mid_Range || combatAdvanced_State.CurrentCombatZone
            == CombatZone.Long_Range || combatAdvanced_State.CurrentCombatZone == CombatZone.Outof_Range)
            {
                Debug.Log($"<color=yellow>Player in Mid/Long Range/OutOfRange</color>");
                npcRoot.statemachine.SwitchState(combatAdvanced_State);
                yield break;
            }
            if (attacksIndex >= maxComboCount)
            {
                //All attacks in combos are completed, switch state
                npcRoot.statemachine.SwitchState(combatAdvanced_State);
                yield break;
            }

        }

        private void HandleMidCombatMovementAnimation()
        {
            if (midCombatMovement == MidCombatMovement.Walk)
            {
                npcRoot.SetStrafeAnimatorValues(direction.front);
            }
            else if (midCombatMovement == MidCombatMovement.Run)
            {
                npcRoot.SetStrafeAnimatorValues_Run();
            }
            else
            {
                idleState.GoToIdleAnimation();
            }
        }

        IEnumerator SwitchToCombatState_Delayed(float waitTime)
        {

            yield return new WaitForSeconds(waitTime);
            combatAdvanced_State.EnableRollForDefense();
            npcRoot.statemachine.SwitchState(combatAdvanced_State);

        }


        private void PopulateFinalComboAttacks()
        {
            if (availableComboAttacks.Count == 0) return;

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

        public void ResetEnemyState()
        {
            //npcRoot.SetPerformingComboAttacksStatus(false);
            //npcRoot.DisableComboChaining();

            //ResetFixedComboIndex();
            //SetCanComboTriggerStatus(false);

            finalComboAttacks.Clear();
            availableComboAttacks.Clear();
            attacksIndex = 0;
            canSwitchToCombatState = false;
        }
    }

    [Serializable]

    public class FixedComboAttack
    {
        public AnimationClip attackAnimClip;
        public int maxChainCount;
        public int minChainCount;

        [Range(0.01f, 100f)]
        public float maxChainingChance = 100f;
        public float[] comboDamageValues;
        public bool[] canKnockbackValues;
        public float attackWeight;

    }

}

