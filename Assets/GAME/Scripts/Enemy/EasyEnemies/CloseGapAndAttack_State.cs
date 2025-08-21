using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EternalKeep
{
    // A combat strategy state to chase and attack
    public class CloseGapAndAttack_State : State, IEnemyStateReset
    {
        //set it false will not set isPerformingAttackStrategy 
        // to false on complete to link it to another combat strategy
        [SerializeField] bool linkStrategyToCombo = false;
        [SerializeField] float addedStaminaCost = 5f;
        [SerializeField] private float totalStaminaCost = 5f;
        [SerializeField] bool isAttacking = false;

        [SerializeField] CombatAdvanced_State combatAdvanced_State;
        [SerializeField] private IdleState idleState;
        [SerializeField] private DynamicComboAttackState dynamicComboAttackState;

        // if list is empty and this list will be populated with attacks from Combat state
        // that has weights for this strat
        [SerializeField] List<Attack> closeGapAndAttack_Attacks = new List<Attack>();
        [SerializeField] Attack endAttack;
        private Coroutine attackWaitCoroutine;
        private bool canSwitchToCombatAdvancedState;
        [SerializeField] private MidCombatMovement midCombatMovement;

        public float AddedStaminaCost => addedStaminaCost;


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
            endAttack = RollAndGetAttack();

            if (combatAdvanced_State.CurrentCombatStrategy == CommonCombatStrategies.CloseGapBlend_And_AttackWithCombo)
            {
                linkStrategyToCombo = true;
                totalStaminaCost = dynamicComboAttackState.LinkStratstaminaCost + addedStaminaCost;
            }
            else
            {
                totalStaminaCost = endAttack.staminaCost + addedStaminaCost;
            }

            if (npcRoot.staminaSystem.CurrentStamina < totalStaminaCost)
            {
                //Roll for All combat Strat, or Roll for Defensive Strat based on defensive weight if low on stamina
                Debug.Log("<color=red>Strategy failed= </color>");
                canSwitchToCombatAdvancedState = true;
                StartCoroutine(SwitchToCombatState_Delayed(0.1f));

            }

            npcRoot.isChasingTarget = true;

            if (combatAdvanced_State.CurrentCombatStrategy == CommonCombatStrategies.CloseGapAndAttack_Combo)
            {
                linkStrategyToCombo = true;
            }

        }

        public override void OnExit()
        {

            npcRoot.isChasingTarget = false;
            linkStrategyToCombo = false;
            canSwitchToCombatAdvancedState = false;
            npcRoot.DisableCanKnockBackOnAttack();
        }

        public override void TickLogic()
        {
            if (canSwitchToCombatAdvancedState) return;


            if (isAttacking)
            {

                npcRoot.RotateOnAttack(npcRoot.lookRotationSpeed);
                HandleMidCombatMovementAnimation();

                //Debug.Log("ROT");
                return;
            }

            if (!combatAdvanced_State.IsPlayerInCloseRange()) // chase until close to player
            {
                idleState.GoToLocomotionAnimation();
                if (npcRoot.isPlayerInLineOfSight())
                {
                    //npcRoot.TurnCharacter();
                    npcRoot.LookAtPlayer(npcRoot.lookRotationSpeed);
                }


                //npcRoot.TurnCharacter();
                npcRoot.SetNavMeshAgentDestination(npcRoot.targetTransform.position);
                npcRoot.SetStrafeAnimatorValues_Run();
                npcRoot.UpdateMoveDirection();
            }
            else if (linkStrategyToCombo)
            {
                //switch to comboAttack State
                npcRoot.statemachine.SwitchState(dynamicComboAttackState);
            }
            else // perform close range attack
            {
                isAttacking = true;

                npcRoot.staminaSystem.DepleteStamina(totalStaminaCost);
                npcRoot.currentDamageToDeal = endAttack.damage;
                npcRoot.canAttackKnockback = endAttack.canAttackKnockback;
                npcRoot.PlayAnyActionAnimation(endAttack.attackAnimClip.name, true);

                float waitTime = endAttack.attackAnimClip.length;
                attackWaitCoroutine = StartCoroutine(OnAttackStrategyComplete(waitTime));
            }



        }

        IEnumerator SwitchToCombatState_Delayed(float waitTime)
        {

            yield return new WaitForSeconds(waitTime);
            combatAdvanced_State.EnableRollForDefense();
            npcRoot.statemachine.SwitchState(combatAdvanced_State);

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

        IEnumerator OnAttackStrategyComplete(float waitTime)
        {

            yield return new WaitForSeconds(waitTime);
            isAttacking = false;

            npcRoot.statemachine.SwitchState(combatAdvanced_State);

        }

        private Attack RollAndGetAttack()
        {
            if (closeGapAndAttack_Attacks.Count == 0) return null;

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

        public void ResetEnemyState()
        {
            isAttacking = false;
            linkStrategyToCombo = false;
            canSwitchToCombatAdvancedState = false;
        }
    }

}


