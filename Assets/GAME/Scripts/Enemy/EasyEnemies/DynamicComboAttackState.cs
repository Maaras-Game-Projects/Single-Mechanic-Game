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

        [SerializeField] List<Attack> availableComboAttacks = new List<Attack>();
        [SerializeField] List<Attack> finalComboAttacks = new List<Attack>();
        [SerializeField] bool isAttacking = false;

        private Coroutine attackWaitCoroutine;
        private bool canSwitchToCombatState = false;
        [SerializeField] private MidCombatMovement midCombatMovement;

        public float LinkStratstaminaCost => linkStratstaminaCost;


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
            }

            npcRoot.staminaSystem.DepleteStamina(endStaminaCost);

        }

        public override void OnExit()
        {
            finalComboAttacks.Clear();
            availableComboAttacks.Clear();
            attacksIndex = 0;
            canSwitchToCombatState = false;
            npcRoot.DisableCanKnockBackOnAttack();

            //Disable all attack's inStrategyBool                           (FOR NOW inStrategy BOOL IS REDUNDANT)
            //combatAdvanced_State.DisableInStrategyStatusForAttacks();

        }

        public override void TickLogic()
        {
            if (canSwitchToCombatState) return;
            //npcRoot.LookAtPlayer(1.5f);
            if (isAttacking)
            {

                npcRoot.RotateOnAttack(npcRoot.lookRotationSpeed);
                HandleMidCombatMovementAnimation();

                //Debug.Log("ROT");
                return;
            }



            isAttacking = true;
            Attack attackToPerform = finalComboAttacks[attacksIndex];
            string attackAnimName = attackToPerform.attackAnimClip.name;

            npcRoot.PlayAnyActionAnimation(attackAnimName, true);
            npcRoot.currentDamageToDeal = attackToPerform.damage * damageModifier;

            //need to add logic for knockback to attackToPerform and parrayable only on last attack in combo
            npcRoot.canAttackKnockback = attackToPerform.canAttackKnockback;

            float waitTime = attackToPerform.attackAnimClip.length;
            attackWaitCoroutine = StartCoroutine(OnAttackStrategyComplete(waitTime));


        }

        IEnumerator OnAttackStrategyComplete(float waitTime)
        {

            yield return new WaitForSeconds(waitTime);
            isAttacking = false;

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
            isAttacking = false;
            finalComboAttacks.Clear();
            availableComboAttacks.Clear();
            attacksIndex = 0;
            canSwitchToCombatState = false;
        }
    }

}

