using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EternalKeep
{
    public class CloseGapBlendAndAttack : State, IEnemyStateReset
    {
        [SerializeField] bool linkStrategyToCombo = false;
        [SerializeField] float addedStaminaCost = 5f;
        [SerializeField] float totalStaminaCost = 5f;
        [SerializeField] bool isAttacking = false;
        [SerializeField] bool isWindingUp = false;
        [SerializeField] bool isWindupAnimPlayed = false;
        [SerializeField] MidCombatMovement movementWhileAttacking;
        [SerializeField] CombatAdvanced_State combatAdvanced_State;
        [SerializeField] IdleState idleState;
        [SerializeField] DynamicComboAttackState dynamicComboAttackState;

        [Space]
        [SerializeField] List<CloseGapBlendAttack> blendAttacks = new List<CloseGapBlendAttack>();
        [Space]

        [SerializeField] CloseGapBlendAttack attackToPerform;

        private Coroutine attackWaitCoroutine;
        private Coroutine windUpWaitCoroutine;
        private bool canSwitchToCombatAdvancedState = false;

        public float AddedStaminaCost => addedStaminaCost;


        public override void OnEnter()
        {

            npcRoot.isChasingTarget = true;
            attackToPerform = RollAndGetAttack();

            if (combatAdvanced_State.CurrentCombatStrategy == CommonCombatStrategies.CloseGapBlend_And_AttackWithCombo)
            {
                linkStrategyToCombo = true;
                totalStaminaCost = dynamicComboAttackState.LinkStratstaminaCost + addedStaminaCost;
            }
            else
            {
                totalStaminaCost = attackToPerform.staminaCost + addedStaminaCost;
            }




            if (npcRoot.staminaSystem.CurrentStamina < totalStaminaCost)
            {
                //Roll for All combat Strat, or Roll for Defensive Strat based on defensive weight if low on stamina
                Debug.Log("<color=red>Strategy failed= </color>" + combatAdvanced_State.CurrentCombatStrategy);
                //Debug.Log("<color=blue>Rolled for Def Strat in CGBA State B4 </color>" + npcRoot.statemachine.currentState);
                //combatAdvanced_State.EnableForceDecide();
                //combatAdvanced_State.EnableRollForDefense();
                canSwitchToCombatAdvancedState = true;
                StartCoroutine(SwitchToCombatState_Delayed(0.1f));



            }


        }

        public override void OnExit()
        {

            npcRoot.isChasingTarget = false;
            isWindupAnimPlayed = false;
            linkStrategyToCombo = false;
            canSwitchToCombatAdvancedState = false;
            npcRoot.DisableCanKnockBackOnAttack();
            //npcRoot.animator.SetBool(attackToPerform.attackTransitionBoolName,false);

        }

        public override void TickLogic()
        {
            // if(npcRoot.staminaSystem.CurrentStamina < totalStaminaCost)
            // {
            //     //Roll for All combat Strat, or Roll for Defensive Strat based on defensive weight if low on stamina
            //     //Debug.Log("<color=red>Strategy failed= </color>");
            //      Debug.Log("<color=cyan>Rolled for Def Strat in CGBA State B4 </color>" + npcRoot.statemachine.currentState);
            //     combatAdvanced_State.RollForDefensiveStrategyAndPerform();
            //     Debug.Log("<color=red>Rolled for Def Strat in CGBA State AFTER </color>" + npcRoot.statemachine.currentState);
            //     return;

            // }
            if (canSwitchToCombatAdvancedState) return;

            //idleState.FallBackToDefaultStateOnPlayerDeath();

            if (isAttacking)
            {

                npcRoot.RotateOnAttack(npcRoot.lookRotationSpeed);
                HandleMovementWhileAttacking();
                npcRoot.UpdateMoveDirection();

                //Debug.Log("ROT");
                return;
            }


            if (!combatAdvanced_State.IsPlayerInCloseRange()) // chase until close to player
            {
                //play windup anim might need to ad bool check

                if (isWindingUp) return;

                if (!isWindupAnimPlayed)
                {

                    isWindingUp = true;
                    npcRoot.PlayAnyActionAnimation(attackToPerform.attackWindUpAnimClip.name, false);
                    float waitTime = attackToPerform.attackWindUpAnimClip.length;
                    windUpWaitCoroutine = StartCoroutine(OnWindUpComplete(waitTime));
                }
                // Debug.Log("<color=cyan> CGBA tick </color>" + npcRoot.statemachine.currentState);

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

            else // perform close range attack
            {
                isAttacking = true;
                //attackToPerform.inStrategy = true;

                //npcRoot.animator.SetLayerWeight(2,0);
                //npcRoot.animator.la

                npcRoot.staminaSystem.DepleteStamina(totalStaminaCost);
                npcRoot.currentDamageToDeal = attackToPerform.damage;
                npcRoot.canAttackKnockback = attackToPerform.canAttackKnockback;
                npcRoot.animator.SetBool(attackToPerform.attackTransitionBoolName, true);
                npcRoot.PlayAnyActionAnimation(attackToPerform.endAttackClip.name, true);
                float waitTime = attackToPerform.endAttackClip.length;
                attackWaitCoroutine = StartCoroutine(OnAttackStrategyComplete(waitTime));
            }



        }

        private void HandleMovementWhileAttacking()
        {
            if (movementWhileAttacking == MidCombatMovement.Run)
            {
                npcRoot.SetStrafeAnimatorValues_Run();
            }
            else if (movementWhileAttacking == MidCombatMovement.Walk)
            {
                npcRoot.SetStrafeAnimatorValues(direction.front);
            }
            else if (movementWhileAttacking == MidCombatMovement.Idle)
            {
                idleState.GoToIdleAnimation();
            }
        }

        IEnumerator OnWindUpComplete(float waitTime)
        {

            yield return new WaitForSeconds(waitTime);
            isWindingUp = false;
            //Debug.Log("<color=yellow>Windup CGBA coroutine </color>" + npcRoot.statemachine.currentState);
            isWindupAnimPlayed = true;

        }

        IEnumerator SwitchToCombatState_Delayed(float waitTime)
        {

            yield return new WaitForSeconds(waitTime);
            combatAdvanced_State.EnableRollForDefense();
            npcRoot.statemachine.SwitchState(combatAdvanced_State);

        }

        IEnumerator OnAttackStrategyComplete(float waitTime)
        {

            yield return new WaitForSeconds(waitTime);
            isAttacking = false;

            // //To filter out attacks that already executed in this state, so that no repeated attacks are present in combo state
            // if(!linkStrategyToCombo)
            // {
            //     attackToPerform.inStrategy = false;
            // }
            npcRoot.animator.SetBool(attackToPerform.attackTransitionBoolName, false);

            //npcRoot.animator.SetLayerWeight(2,1);
            if (linkStrategyToCombo)
            {
                //switch to comboAttack State
                npcRoot.statemachine.SwitchState(dynamicComboAttackState);
            }
            else
            {
                npcRoot.statemachine.SwitchState(combatAdvanced_State);
            }



        }

        private CloseGapBlendAttack RollAndGetAttack()
        {
            if (blendAttacks.Count == 0) return null;

            float totalAttackChance = 0f;

            foreach (CloseGapBlendAttack attack in blendAttacks)
            {
                totalAttackChance += attack.weight;
            }

            float randomValue = UnityEngine.Random.Range(0f, totalAttackChance);



            foreach (CloseGapBlendAttack attack in blendAttacks)
            {
                if (randomValue <= attack.weight)
                {
                    return attack;
                }

                randomValue -= attack.weight;
            }

            return null;
        }

        public void ResetEnemyState()
        {
            isAttacking = false;
            isWindingUp = false;
            isWindupAnimPlayed = false;
            linkStrategyToCombo = false;
            canSwitchToCombatAdvancedState = false;
        }
    }

}

