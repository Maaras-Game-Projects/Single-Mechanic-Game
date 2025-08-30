using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EternalKeep
{
    public class LeapAttackState : State, IEnemyStateReset
    {
        [SerializeField] CombatAdvanced_State combatAdvanced_State;
        [SerializeField] IdleState idleState;
        [SerializeField] List<LeapAttack> leapAttacks = new List<LeapAttack>();
        [SerializeField] bool isAttacking = false;

        [SerializeField] LeapAttack chosenAttack;

        private Coroutine attackWaitCoroutine;
        private bool canSwitchToCombatState = false;



        public override void OnEnter()
        {
            npcRoot.inLeapAttack = true;
            chosenAttack = RollAndGetAttack();

            if (chosenAttack == null)
            {
                Debug.Log("No Leap Attack available, switching to combat state.");
                StartCoroutine(SwitchToCombatState_Delayed(0.1f));
                return;
            }

            float endStaminaCost = chosenAttack.staminaCost;

            if (npcRoot.staminaSystem.CurrentStamina < endStaminaCost)
            {
                //Roll for All combat Strat, or Roll for Defensive Strat based on defensive weight if low on stamina
                Debug.Log("<color=red>Strategy failed= </color>");
                canSwitchToCombatState = true;
                StartCoroutine(SwitchToCombatState_Delayed(0.1f));
                return;

            }

            npcRoot.staminaSystem.DepleteStamina(endStaminaCost);

        }

        public override void OnExit()
        {
            isAttacking = false;
            canSwitchToCombatState = false;
            npcRoot.inLeapAttack = false;
            npcRoot.DisableCanKnockBackOnAttack();

        }

        public override void TickLogic()
        {
            if (canSwitchToCombatState) return;

            //idleState.FallBackToDefaultStateOnPlayerDeath();
            //npcRoot.LookAtPlayer(1.5f);
            if (isAttacking)
            {

                npcRoot.RotateOnAttack(npcRoot.lookRotationSpeed);
                //HandleMovementWhileAttacking();

                //Debug.Log("ROT");
                return;
            }

            isAttacking = true;


            npcRoot.PlayAnyActionAnimation(chosenAttack.fullAttackClip.name, true);
            npcRoot.currentDamageToDeal = chosenAttack.damage;

            //need to add logic for knockback to attackToPerform and parrayable only on last attack in combo
            npcRoot.canAttackKnockback = chosenAttack.canAttackKnockback;
            SetModifyableLeapingSpeeds();

            float waitTime = 1f;
            if (chosenAttack.useAttackDurationManually)
            {
                waitTime = chosenAttack.durationOfAttack;
            }
            else if (chosenAttack.fullAttackClip != null)
            {
                waitTime = chosenAttack.fullAttackClip.length;
            }

            attackWaitCoroutine = StartCoroutine(OnAttackStrategyComplete(waitTime));


        }

        private void SetModifyableLeapingSpeeds()
        {
            if (combatAdvanced_State.CurrentCombatZone == CombatZone.Backoff_Range)
            {
                npcRoot.mod_verticalLeapingSpeed = chosenAttack.verticalLeapSpeed;
                npcRoot.mod_forwardLeapingSpeed = chosenAttack.forwardLeapSpeed_BackOffRange;
            }
            else if (combatAdvanced_State.CurrentCombatZone == CombatZone.Mid_Range)
            {
                npcRoot.mod_verticalLeapingSpeed = chosenAttack.verticalLeapSpeed;
                npcRoot.mod_forwardLeapingSpeed = chosenAttack.forwardLeapSpeed_MidRange;
            }
            else if (combatAdvanced_State.CurrentCombatZone == CombatZone.Long_Range)
            {
                npcRoot.mod_verticalLeapingSpeed = chosenAttack.verticalLeapSpeed;
                npcRoot.mod_forwardLeapingSpeed = chosenAttack.forwardLeapSpeed_LongRange;
            }
        }

        IEnumerator OnAttackStrategyComplete(float waitTime)
        {

            yield return new WaitForSeconds(waitTime);
            isAttacking = false;
            npcRoot.statemachine.SwitchState(combatAdvanced_State);

        }

        IEnumerator SwitchToCombatState_Delayed(float waitTime)
        {

            yield return new WaitForSeconds(waitTime);
            combatAdvanced_State.EnableRollForDefense();
            npcRoot.statemachine.SwitchState(combatAdvanced_State);

        }

        private LeapAttack RollAndGetAttack()
        {
            if (leapAttacks.Count == 0) return null;

            float totalAttackChance = 0f;

            foreach (LeapAttack attack in leapAttacks)
            {
                totalAttackChance += attack.weight;
            }

            float randomValue = UnityEngine.Random.Range(0.1f, totalAttackChance);



            foreach (LeapAttack attack in leapAttacks)
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
            canSwitchToCombatState = false;
        }
    }

    [Serializable]
    public class LeapAttack
    {
        [Tooltip("Drop FullAttackANimCLip if not use start anim clip")]
        public AnimationClip fullAttackClip;

        [Tooltip("Use this to manually set the attack duration instead of using the animation clip length.")]
        public bool useAttackDurationManually = false;

        [Tooltip("This duration will be used if useAttackDurationManually is true, otherwise the duration will be taken from the animation clip length.")]
        public float durationOfAttack = 1f;
        public bool canAttackKnockback = false;
        public float damage;
        public float staminaCost = 10f;
        public float weight = 10f;
        public float verticalLeapSpeed = 1f;
        public float forwardLeapSpeed_BackOffRange = 1f;
        public float forwardLeapSpeed_MidRange = 1f;
        public float forwardLeapSpeed_LongRange = 1f;


    }


}



