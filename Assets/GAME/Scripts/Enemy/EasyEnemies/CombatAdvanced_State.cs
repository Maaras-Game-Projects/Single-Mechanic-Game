using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace EternalKeep
{
    public class CombatAdvanced_State : State, IEnemyStateReset
    {

        [SerializeField] public bool chaseToAttackAtStart = true; // Can the enemy must chase and attack after seeing enemy or wait and DecideStrategy in combat radius

        // [Tooltip("Distance to attack player in Long range, the value must be within combat radius")]
        // public float min_longRangeAttackDistance = 3f;

        // [Tooltip("Distance to attack player in Short range, the value must be within combat radius")]
        // public float max_shortRangeAttackDistance = 1.5f;

        [SerializeField] public float combatRadius = 8f;
        [SerializeField] private float combatRadius_Modified;
        [SerializeField] public bool inCombatRadius = false;

        [SerializeField] private bool enteredCombat = false;

        public bool EnteredCombat => enteredCombat;

        [SerializeField] private ChaseState chaseState;
        [SerializeField] private IdleState idleState;
        [SerializeField] private StrafeState strafeState;
        [SerializeField] private BackOffState backOffState;
        [SerializeField] private CloseGapAndAttack_State closeGapAndAttack_State;
        [SerializeField] private CloseGapBlendAndAttack closeGapBlendAndAttack_State;
        [SerializeField] private DynamicComboAttackState dynamicComboAttackState;
        [SerializeField] private LeapAttackState leapAttackState;
        [SerializeField] private float combatRadius_Offset = 0.5f;
        [SerializeField] private float decisionInterval = 3f;
        [SerializeField] private float forceDecideDelay = .25f;
        [SerializeField] private float idleDuration = 2f;
        [SerializeField] private float healthDifferenceReference_MaxValue = 0f;
        [SerializeField] private float healthDifferenceValue = 20f;
        [SerializeField] private float staminaValueToGoDefense = 5f;
        [SerializeField] private bool canCheckHealthDifference = false;
        [SerializeField] private bool forceDecide = false;
        [SerializeField] private bool forceRollForDefense = false;
        [SerializeField] private bool isAttacking = false;
        [SerializeField] private bool isIdling = false;
        [SerializeField] private MidCombatMovement midCombatMovement;

        [Space]
        [Header("CombatStrategy Variables")]
        [Space]

        [SerializeField] CombatStrategyWeights combatStrategyWeights_CloseRange;
        [Space]

        [SerializeField] CombatStrategyWeights combatStrategyWeights_BackOffRange;
        [Space]

        [SerializeField] CombatStrategyWeights combatStrategyWeights_MidRange;
        [Space]

        [SerializeField] CombatStrategyWeights combatStrategyWeights_LongRange;
        [Space]


        [Space]
        [Header("Combat Zone Variables")]
        [Space]

        [SerializeField] float closeRange_Radius = 1.5f; // same as MaxCloseRange attack radius
        [SerializeField] float backoff_Range_Radius = 3f;
        [SerializeField] float midRange_Radius = 5.5f;
        [SerializeField] float longRange_Radius = 8f; // same as combatRadius

        [Space]
        [Header("Combat Zone Weight Variables")]
        [Space]

        [Range(0, 100)]
        [SerializeField] float chanceToGoDefensive = 50f;

        [Space]
        [Header("Attack Variables")]
        [Space]

        [SerializeField] public List<Attack> attacks = new List<Attack>();

        [Space]
        [Header("Debug Variables")]
        [Space]

        [SerializeField] CombatZone currentCombatZone = CombatZone.Outof_Range;
        [SerializeField] CommonCombatStrategies currentCombatStrategy = CommonCombatStrategies.Idle;
        [SerializeField] float elapsedDecisionTime = 0f;

        [Space]

        [SerializeField] List<Attack> CloseRangeAttacks = new List<Attack>();
        [SerializeField] List<Attack> backOffRangeAttacks = new List<Attack>();
        [SerializeField] List<Attack> midRangeAttacks = new List<Attack>();
        [SerializeField] List<Attack> LongRangeAttacks = new List<Attack>();

        [SerializeField] Attack attackToPerform;

        private Coroutine attackStrategyWaitCoroutine = null;
        private Coroutine idleStrategyWaitCoroutine = null;

        public CombatZone CurrentCombatZone => currentCombatZone;
        public CommonCombatStrategies CurrentCombatStrategy => currentCombatStrategy;


        void OnDisable()
        {
            StopAllCoroutines();
        }

        void Awake()
        {
            InitCombatState();

        }

        private void InitCombatState()
        {
            combatRadius = longRange_Radius;
            combatRadius_Modified = combatRadius - combatRadius_Offset; // set the modified combat radius to be slightly smaller than the original combat radius

            AddCloseRangeAttacks();
            AddBackOffRangeAttacks();
            AddMidRangeAttacks();
            AddLongRangeAttacks();
        }

        void Start()
        {
            healthDifferenceReference_MaxValue = npcRoot.healthSystem.MaxHealth;
        }

        private void AddCloseRangeAttacks()
        {
            foreach (Attack attack in attacks)
            {
                if (attack.weightsByCombatZone.closeRange_Weight > 0)
                {
                    CloseRangeAttacks.Add(attack);
                }
            }
        }

        private void AddLongRangeAttacks()
        {
            foreach (Attack attack in attacks)
            {
                if (attack.weightsByCombatZone.longRange_Weight > 0)
                {
                    LongRangeAttacks.Add(attack);
                }
            }
        }

        private void AddMidRangeAttacks()
        {
            foreach (Attack attack in attacks)
            {
                if (attack.weightsByCombatZone.midRange_Weight > 0)
                {
                    midRangeAttacks.Add(attack);
                }
            }
        }

        private void AddBackOffRangeAttacks()
        {
            foreach (Attack attack in attacks)
            {
                if (attack.weightsByCombatZone.backOffRange_Weight > 0)
                {
                    backOffRangeAttacks.Add(attack);
                }
            }
        }

        public override void OnEnter()
        {
            //forceDecide = true;
            StartCoroutine(EnableForceDecideInDelay(forceDecideDelay));
            if (forceDecide)
            {
                elapsedDecisionTime = decisionInterval;
            }
            else
            {
                elapsedDecisionTime = 0f;
            }


            enteredCombat = true;

            //npcRoot.animator.CrossFade("Empty State",0.05f, 1);


        }

        IEnumerator EnableForceDecideInDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            forceDecide = true;
        }

        public override void OnExit()
        {

        }

        public override void TickLogic()
        {
            if (npcRoot.isAnotherEnemyCloseToTargetBlockingLOS())
            {
                int randomValue = UnityEngine.Random.Range(0, 2);

                if (randomValue == 0)
                    strafeState.currentStrafeDirection = direction.left;
                else
                    strafeState.currentStrafeDirection = direction.right;

                Debug.Log($"<color=green>LOS blocked by enemy -- strafing");
                npcRoot.statemachine.SwitchState(strafeState);
                return;
            }

            //idleState.FallBackToDefaultStateOnPlayerDeath();

            if (npcRoot.isPlayerInLineOfSight())
            {
                HandleTurnAndRotation();
                //Debug.Log($"<color=red>out of LOS, turning</color>");
            }
            else
            {
                npcRoot.statemachine.SwitchState(chaseState);

                return;
            }


            elapsedDecisionTime += Time.deltaTime;

            HandleMidCombatMovementAnimation();
            npcRoot.UpdateMoveDirection();



            if (isAttacking || isIdling || npcRoot.isInteracting || npcRoot.IsStunned) return;



            if (elapsedDecisionTime >= decisionInterval || forceDecide)
            {
                CommonCombatStrategies strategyToPerform = CommonCombatStrategies.Idle;

                if (npcRoot.staminaSystem.CurrentStamina < staminaValueToGoDefense) //check if stamina is low enough to go defensive
                {
                    strategyToPerform = DetermineDefensiveStrategy();
                    PerformStrategy(strategyToPerform);
                    elapsedDecisionTime = 0f;
                    forceDecide = false;
                    //canCheckHealthDifference = false;

                    return;
                }

                if (forceRollForDefense)
                {
                    strategyToPerform = RollForDefensiveStrategy();
                    PerformStrategy(strategyToPerform);
                    elapsedDecisionTime = 0f;
                    forceDecide = false;
                    forceRollForDefense = false;
                    //canCheckHealthDifference = false;


                    return;
                }

                if (canCheckHealthDifference)
                {
                    if (healthDifferenceReference_MaxValue - npcRoot.healthSystem.CurrentHealth >= healthDifferenceValue) //check if health difference is signicant enough to roll for defensive
                    {
                        healthDifferenceReference_MaxValue = npcRoot.healthSystem.CurrentHealth;

                        //Roll for Defensive Strategy
                        strategyToPerform = RollForDefensiveStrategy();
                        canCheckHealthDifference = false;
                    }
                    else
                    {
                        canCheckHealthDifference = false;
                        return;
                    }

                }
                else
                {
                    strategyToPerform = DetermineCombatStrategy();
                }

                //Debug.Log($"<color=cyan>Strategy = {strategyToPerform}");
                PerformStrategy(strategyToPerform);
                elapsedDecisionTime = 0f;
                forceDecide = false;

            }
        }

        public void EnableHealthDifferenceCheck()
        {
            canCheckHealthDifference = true;
        }

        public void EnableForceDecide()
        {
            forceDecide = true;
        }


        public void EnableRollForDefense()
        {
            forceRollForDefense = true;
        }

        private void RollForDefensiveStrategyAndPerform()
        {
            CommonCombatStrategies strategyToPerform = RollForDefensiveStrategy();
            PerformStrategy(strategyToPerform);

        }

        private CommonCombatStrategies RollForDefensiveStrategy()
        {
            CommonCombatStrategies strategyToPerform;
            float randomValue = UnityEngine.Random.Range(0.1f, 100f);

            if (randomValue <= chanceToGoDefensive)
            {
                strategyToPerform = DetermineDefensiveStrategy();
            }
            else
            {
                strategyToPerform = DetermineCombatStrategy();
            }

            return strategyToPerform;
        }

        public void DisableInStrategyStatusForAttacks()
        {
            foreach (Attack attack in attacks)
            {
                attack.inStrategy = false;
            }
        }

        private void HandleTurnAndRotation()
        {
            if (!isAttacking) //&& !npcRoot.IsPerformingComboAttacks && !npcRoot.isInteracting)
            {
                npcRoot.TurnCharacter();
                //Debug.Log($"<color=cyan>Turning Character</color>");
                npcRoot.LookAtPlayer(npcRoot.lookRotationSpeed);
            }
            else
            {
                npcRoot.RotateOnAttack(npcRoot.lookRotationSpeed);
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

        private CommonCombatStrategies DetermineCombatStrategy()
        {
            CommonCombatStrategies strategyToPerform = CommonCombatStrategies.Idle;



            UpdateCurrentCombatZone();

            if (currentCombatZone == CombatZone.Outof_Range || !npcRoot.isPlayerInLineOfSight())
            {
                npcRoot.statemachine.SwitchState(chaseState);
            }
            else if (currentCombatZone == CombatZone.Close_Range)
            {
                strategyToPerform = RollAndGetCombatStrategies_CloseRange();
            }
            else if (currentCombatZone == CombatZone.Backoff_Range)
            {
                strategyToPerform = RollAndGetCombatStrategies_BackOffRange();
            }
            else if (currentCombatZone == CombatZone.Mid_Range)
            {
                strategyToPerform = RollAndGetCombatStrategies_MidRange();
            }
            else if (currentCombatZone == CombatZone.Long_Range)
            {
                strategyToPerform = RollAndGetCombatStrategies_LongRange();
            }

            return strategyToPerform;


        }

        private void PerformStrategy(CommonCombatStrategies strategyToPerform)
        {

            if (strategyToPerform == CommonCombatStrategies.Strafe)
            {
                currentCombatStrategy = strategyToPerform;
                npcRoot.statemachine.SwitchState(strafeState);
            }
            else if (strategyToPerform == CommonCombatStrategies.BackOff)
            {
                currentCombatStrategy = strategyToPerform;
                npcRoot.statemachine.SwitchState(backOffState);
            }
            else if (strategyToPerform == CommonCombatStrategies.Idle)
            {
                currentCombatStrategy = strategyToPerform;
                PerformIdle();
            }
            else if (strategyToPerform == CommonCombatStrategies.CloseRange_Attack)
            {
                currentCombatStrategy = strategyToPerform;
                PerformCloseRangeAttackStrategy();
            }
            else if (strategyToPerform == CommonCombatStrategies.LongRange_Attack)
            {
                currentCombatStrategy = strategyToPerform;
                PerformLongRangeAttackStrategy();
            }
            else if (strategyToPerform == CommonCombatStrategies.CloseGapAndAttack)
            {
                currentCombatStrategy = strategyToPerform;
                npcRoot.statemachine.SwitchState(closeGapAndAttack_State);
            }
            else if (strategyToPerform == CommonCombatStrategies.CloseGapAndAttack_Combo)
            {
                currentCombatStrategy = strategyToPerform;
                npcRoot.statemachine.SwitchState(closeGapAndAttack_State);
            }
            else if (strategyToPerform == CommonCombatStrategies.CloseGapBlend_And_Attack)
            {
                currentCombatStrategy = strategyToPerform;
                npcRoot.statemachine.SwitchState(closeGapBlendAndAttack_State);
            }
            else if (strategyToPerform == CommonCombatStrategies.CloseGapBlend_And_AttackWithCombo)
            {
                currentCombatStrategy = strategyToPerform;
                npcRoot.statemachine.SwitchState(closeGapBlendAndAttack_State);
            }
            else if (strategyToPerform == CommonCombatStrategies.ComboAttack_CloseRange)
            {
                currentCombatStrategy = strategyToPerform;
                npcRoot.statemachine.SwitchState(dynamicComboAttackState);
            }
            else if (strategyToPerform == CommonCombatStrategies.LeapAttack)
            {
                currentCombatStrategy = strategyToPerform;
                npcRoot.statemachine.SwitchState(leapAttackState);
            }
        }

        private void PerformIdle()
        {
            isIdling = true;
            idleState.GoToIdleAnimation();
            idleStrategyWaitCoroutine = StartCoroutine(OnIdleStrategyComplete(idleDuration));
        }

        private void PerformCloseRangeAttackStrategy()
        {
            attackToPerform = RollAndGetCloseRangeStrategyAttack();

            if (attackToPerform == null || npcRoot.staminaSystem.CurrentStamina < attackToPerform.staminaCost)
            {
                //Roll for All combat Strat, or Roll for Defensive Strat based on defensive weight
                Debug.Log("<color=red>Strategy failed= </color>" + currentCombatStrategy);
                EnableRollForDefense();
                EnableForceDecide();
                //forceDecide = true;
            }
            else
            {

                isAttacking = true;
                npcRoot.staminaSystem.DepleteStamina(attackToPerform.staminaCost);
                npcRoot.currentDamageToDeal = attackToPerform.damage;
                npcRoot.canAttackKnockback = attackToPerform.canAttackKnockback;
                // Debug.Log($"<color=yellow>Attack name = {attackToPerform.attackAnimClip.name }</color>");
                // Debug.Log($"<color=yellow>Attack damage = {attackToPerform.damage }</color>");
                // Debug.Log($"<color=yellow>npcRoot damage = {npcRoot.currentDamageToDeal }</color>");
                // Debug.Log($"<color=yellow>Attack KnockBack Val = {attackToPerform.canAttackKnockback }</color>");
                // Debug.Log($"<color=yellow>npcRoot knockback val = {      npcRoot.canAttackKnockback }</color>");
                npcRoot.PlayAnyActionAnimation(attackToPerform.attackAnimClip.name, true);

                float waitTime = attackToPerform.attackAnimClip.length;
                attackStrategyWaitCoroutine = StartCoroutine(OnAttackStrategyComplete(waitTime));

            }


        }

        private void PerformLongRangeAttackStrategy()
        {
            attackToPerform = RollAndGetLongRangeStrategyAttack();

            if (attackToPerform == null || npcRoot.staminaSystem.CurrentStamina < attackToPerform.staminaCost)
            {
                //Roll for All combat Strat, or Roll for Defensive Strat based on defensive weight
                Debug.Log("<color=red>Strategy failed= </color>" + currentCombatStrategy);
                EnableRollForDefense();
                EnableForceDecide();
            }
            else
            {

                isAttacking = true;
                npcRoot.staminaSystem.DepleteStamina(attackToPerform.staminaCost);
                npcRoot.currentDamageToDeal = attackToPerform.damage;
                npcRoot.canAttackKnockback = attackToPerform.canAttackKnockback;
                // Debug.Log($"<color=cyan>Attack name = {attackToPerform.attackAnimClip.name }</color>");
                // Debug.Log($"<color=cyan>Attack damage = {attackToPerform.damage }</color>");
                // Debug.Log($"<color=cyan>npcRoot damage = {npcRoot.currentDamageToDeal }</color>");
                // Debug.Log($"<color=cyan>Attack KnockBack Val = {attackToPerform.canAttackKnockback }</color>");
                // Debug.Log($"<color=cyan>npcRoot knockback val = {      npcRoot.canAttackKnockback }</color>");
                npcRoot.PlayAnyActionAnimation(attackToPerform.attackAnimClip.name, true);

                float waitTime = attackToPerform.attackAnimClip.length;
                attackStrategyWaitCoroutine = StartCoroutine(OnAttackStrategyComplete(waitTime));

            }


        }

        IEnumerator OnAttackStrategyComplete(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            attackToPerform.onAttackEnd?.Invoke();
            isAttacking = false;
            npcRoot.DisableCanKnockBackOnAttack();

        }

        IEnumerator OnIdleStrategyComplete(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            forceDecide = true;
            isIdling = false;
            if (npcRoot.statemachine.currentState != this)
            {
                npcRoot.statemachine.SwitchState(this);
            }

        }

        private Attack RollAndGetCloseRangeStrategyAttack()
        {

            if (currentCombatZone == CombatZone.Close_Range)
            {
                return RollAndGetCloseRangeAttacks_CloseRangeZone();
            }
            else if (currentCombatZone == CombatZone.Backoff_Range)
            {
                return RollAndGetBackOffRangeAttacks();
            }
            else
            {
                return null;
            }

        }

        private Attack RollAndGetBackOffRangeAttacks()
        {
            if (backOffRangeAttacks.Count == 0) return null;

            float totalAttackChance = 0f;

            foreach (Attack attack in backOffRangeAttacks)
            {
                totalAttackChance += attack.weightsByCombatZone.backOffRange_Weight;
            }

            float randomValue = UnityEngine.Random.Range(0.1f, totalAttackChance);



            foreach (Attack attack in backOffRangeAttacks)
            {
                if (randomValue <= attack.weightsByCombatZone.backOffRange_Weight)
                {
                    return attack;
                }

                randomValue -= attack.weightsByCombatZone.backOffRange_Weight;
            }

            return null;
        }



        private Attack RollAndGetCloseRangeAttacks_CloseRangeZone()
        {
            if (CloseRangeAttacks.Count == 0) return null;

            float totalAttackChance = 0f;
            foreach (Attack attack in CloseRangeAttacks)
            {
                totalAttackChance += attack.weightsByCombatZone.closeRange_Weight;
            }

            float randomValue = UnityEngine.Random.Range(0.1f, totalAttackChance);



            foreach (Attack attack in CloseRangeAttacks)
            {
                if (randomValue <= attack.weightsByCombatZone.closeRange_Weight)
                {
                    return attack;
                }

                randomValue -= attack.weightsByCombatZone.closeRange_Weight;
            }

            return null;
        }

        private Attack RollAndGetMidRangeAttacks()
        {
            if (midRangeAttacks.Count == 0) return null;

            float totalAttackChance = 0f;

            foreach (Attack attack in midRangeAttacks)
            {
                totalAttackChance += attack.weightsByCombatZone.midRange_Weight;
            }

            float randomValue = UnityEngine.Random.Range(0.1f, totalAttackChance);



            foreach (Attack attack in midRangeAttacks)
            {
                if (randomValue <= attack.weightsByCombatZone.midRange_Weight)
                {
                    return attack;
                }

                randomValue -= attack.weightsByCombatZone.midRange_Weight;
            }

            return null;
        }

        private Attack RollAndGetLongRangeAttacks_LongRangeZone()
        {
            if (LongRangeAttacks.Count == 0) return null;

            float totalAttackChance = 0f;

            foreach (Attack attack in LongRangeAttacks)
            {
                totalAttackChance += attack.weightsByCombatZone.longRange_Weight;
            }

            float randomValue = UnityEngine.Random.Range(0.1f, totalAttackChance);



            foreach (Attack attack in LongRangeAttacks)
            {
                if (randomValue <= attack.weightsByCombatZone.longRange_Weight)
                {
                    return attack;
                }

                randomValue -= attack.weightsByCombatZone.longRange_Weight;
            }

            return null;
        }

        private Attack RollAndGetLongRangeStrategyAttack()
        {
            if (currentCombatZone == CombatZone.Backoff_Range)
            {
                return RollAndGetBackOffRangeAttacks();
            }
            else if (currentCombatZone == CombatZone.Mid_Range)
            {
                return RollAndGetMidRangeAttacks();
            }
            else if (currentCombatZone == CombatZone.Long_Range)
            {
                return RollAndGetLongRangeAttacks_LongRangeZone();
            }
            else
            {
                return null;
            }

        }

        public void UpdateCurrentCombatZone()
        {
            if (IsPlayerInCloseRange()) return;
            if (isPlayerInBackoffRange()) return;
            if (isPlayerInMidRange()) return;
            if (isPlayerInLongRange()) return;

            currentCombatZone = CombatZone.Outof_Range;
            //Debug.Log("<color=yellow>Current Zone = </color>" + currentCombatZone);
        }

        private CommonCombatStrategies DetermineDefensiveStrategy()
        {
            CommonCombatStrategies strategyToPerform = CommonCombatStrategies.Idle;



            UpdateCurrentCombatZone();

            if (currentCombatZone == CombatZone.Outof_Range || !npcRoot.isPlayerInLineOfSight())
            {
                npcRoot.statemachine.SwitchState(chaseState);
            }
            else if (currentCombatZone == CombatZone.Close_Range)
            {
                strategyToPerform = RollAndGetCombatStrategies_Defensive_CloseRange();
            }
            else if (currentCombatZone == CombatZone.Backoff_Range)
            {
                strategyToPerform = RollAndGetCombatStrategies_Defensive_BackOffRange();
            }
            else if (currentCombatZone == CombatZone.Mid_Range)
            {
                strategyToPerform = RollAndGetCombatStrategies_Defensive_MidRange();
            }
            else if (currentCombatZone == CombatZone.Long_Range)
            {
                strategyToPerform = RollAndGetCombatStrategies_Defensive_LongRange();
            }

            return strategyToPerform;


        }

        private CommonCombatStrategies RollAndGetCombatStrategies_Defensive_CloseRange()
        {
            Dictionary<CommonCombatStrategies, float> combatStrategiesWeightPairDefensive_CloseRange =
            new Dictionary<CommonCombatStrategies, float>
            {
                {CommonCombatStrategies.BackOff,combatStrategyWeights_CloseRange.BackOff},
                {CommonCombatStrategies.Strafe,combatStrategyWeights_CloseRange.Strafe},
                {CommonCombatStrategies.Idle,combatStrategyWeights_CloseRange.Idle},

            };

            float totalChance = combatStrategiesWeightPairDefensive_CloseRange.Values.Sum();

            float randomValue = UnityEngine.Random.Range(0.1f, totalChance);

            foreach (var pair in combatStrategiesWeightPairDefensive_CloseRange)
            {
                CommonCombatStrategies strategy = pair.Key;
                float weight = pair.Value;
                if (randomValue <= weight)
                {
                    return strategy;
                }

                randomValue -= weight;
            }

            return CommonCombatStrategies.Idle;
        }

        private CommonCombatStrategies RollAndGetCombatStrategies_Defensive_BackOffRange()
        {
            Dictionary<CommonCombatStrategies, float> combatStrategiesWeightPairDefensive_BackoffRange =
            new Dictionary<CommonCombatStrategies, float>
            {
                {CommonCombatStrategies.BackOff,combatStrategyWeights_BackOffRange.BackOff},
                {CommonCombatStrategies.Strafe,combatStrategyWeights_BackOffRange.Strafe},
                {CommonCombatStrategies.Idle,combatStrategyWeights_BackOffRange.Idle},

            };

            float totalChance = combatStrategiesWeightPairDefensive_BackoffRange.Values.Sum();

            float randomValue = UnityEngine.Random.Range(0.1f, totalChance);

            foreach (var pair in combatStrategiesWeightPairDefensive_BackoffRange)
            {
                CommonCombatStrategies strategy = pair.Key;
                float weight = pair.Value;
                if (randomValue <= weight)
                {
                    return strategy;
                }

                randomValue -= weight;
            }

            return CommonCombatStrategies.Idle;
        }

        private CommonCombatStrategies RollAndGetCombatStrategies_Defensive_MidRange()
        {
            Dictionary<CommonCombatStrategies, float> combatStrategiesWeightPairDefensive_MidRange =
            new Dictionary<CommonCombatStrategies, float>
            {
                {CommonCombatStrategies.BackOff,combatStrategyWeights_MidRange.BackOff},
                {CommonCombatStrategies.Strafe,combatStrategyWeights_MidRange.Strafe},
                {CommonCombatStrategies.Idle,combatStrategyWeights_MidRange.Idle},

            };

            float totalChance = combatStrategiesWeightPairDefensive_MidRange.Values.Sum();

            float randomValue = UnityEngine.Random.Range(0.1f, totalChance);

            foreach (var pair in combatStrategiesWeightPairDefensive_MidRange)
            {
                CommonCombatStrategies strategy = pair.Key;
                float weight = pair.Value;
                if (randomValue <= weight)
                {
                    return strategy;
                }

                randomValue -= weight;
            }

            return CommonCombatStrategies.Idle;
        }

        private CommonCombatStrategies RollAndGetCombatStrategies_Defensive_LongRange()
        {
            Dictionary<CommonCombatStrategies, float> combatStrategiesWeightPairDefensive_LongRange =
            new Dictionary<CommonCombatStrategies, float>
            {
                {CommonCombatStrategies.BackOff,combatStrategyWeights_LongRange.BackOff},
                {CommonCombatStrategies.Strafe,combatStrategyWeights_LongRange.Strafe},
                {CommonCombatStrategies.Idle,combatStrategyWeights_LongRange.Idle},

            };

            float totalChance = combatStrategiesWeightPairDefensive_LongRange.Values.Sum();

            float randomValue = UnityEngine.Random.Range(0.1f, totalChance);

            foreach (var pair in combatStrategiesWeightPairDefensive_LongRange)
            {
                CommonCombatStrategies strategy = pair.Key;
                float weight = pair.Value;
                if (randomValue <= weight)
                {
                    return strategy;
                }

                randomValue -= weight;
            }

            return CommonCombatStrategies.Idle;
        }

        private CommonCombatStrategies RollAndGetCombatStrategies_CloseRange()
        {
            Dictionary<CommonCombatStrategies, float> combatStrategiesWeightPair_CloseRange =
            new Dictionary<CommonCombatStrategies, float>
            {
                {CommonCombatStrategies.CloseRange_Attack,combatStrategyWeights_CloseRange.closeRange_Attack},
                {CommonCombatStrategies.LongRange_Attack,combatStrategyWeights_CloseRange.LongRange_Attack},
                {CommonCombatStrategies.ComboAttack_CloseRange,combatStrategyWeights_CloseRange.ComboAttack_CloseRange},
                {CommonCombatStrategies.CloseGapAndAttack,combatStrategyWeights_CloseRange.CloseGapAndAttack},
                {CommonCombatStrategies.CloseGapAndAttack_Combo,combatStrategyWeights_CloseRange.CloseGapAndAttack_Combo},
                {CommonCombatStrategies.CloseGapBlend_And_Attack,combatStrategyWeights_CloseRange.CloseGapBlend_And_Attack},
                {CommonCombatStrategies.CloseGapBlend_And_AttackWithCombo,combatStrategyWeights_CloseRange.CloseGapBlend_And_AttackWithCombo},
                {CommonCombatStrategies.LeapAttack,combatStrategyWeights_CloseRange.leap_Attack_Strat},
                {CommonCombatStrategies.BackOff,combatStrategyWeights_CloseRange.BackOff},
                {CommonCombatStrategies.Strafe,combatStrategyWeights_CloseRange.Strafe},
                {CommonCombatStrategies.Idle,combatStrategyWeights_CloseRange.Idle},

            };

            float totalChance = combatStrategiesWeightPair_CloseRange.Values.Sum();

            float randomValue = UnityEngine.Random.Range(0.1f, totalChance);

            foreach (var pair in combatStrategiesWeightPair_CloseRange)
            {
                CommonCombatStrategies strategy = pair.Key;
                float weight = pair.Value;
                if (randomValue <= weight)
                {
                    return strategy;
                }

                randomValue -= weight;
            }

            return CommonCombatStrategies.Idle;
        }

        private CommonCombatStrategies RollAndGetCombatStrategies_BackOffRange()
        {
            Dictionary<CommonCombatStrategies, float> combatStrategiesWeightPair_BackOffRange =
            new Dictionary<CommonCombatStrategies, float>
            {
                {CommonCombatStrategies.CloseRange_Attack,combatStrategyWeights_BackOffRange.closeRange_Attack},
                {CommonCombatStrategies.LongRange_Attack,combatStrategyWeights_BackOffRange.LongRange_Attack},
                {CommonCombatStrategies.ComboAttack_CloseRange,combatStrategyWeights_BackOffRange.ComboAttack_CloseRange},
                {CommonCombatStrategies.CloseGapAndAttack,combatStrategyWeights_BackOffRange.CloseGapAndAttack},
                {CommonCombatStrategies.CloseGapAndAttack_Combo,combatStrategyWeights_BackOffRange.CloseGapAndAttack_Combo},
                {CommonCombatStrategies.CloseGapBlend_And_Attack,combatStrategyWeights_BackOffRange.CloseGapBlend_And_Attack},
                {CommonCombatStrategies.CloseGapBlend_And_AttackWithCombo,combatStrategyWeights_BackOffRange.CloseGapBlend_And_AttackWithCombo},
                {CommonCombatStrategies.LeapAttack,combatStrategyWeights_BackOffRange.leap_Attack_Strat},
                { CommonCombatStrategies.BackOff,combatStrategyWeights_BackOffRange.BackOff},
                {CommonCombatStrategies.Strafe,combatStrategyWeights_BackOffRange.Strafe},
                {CommonCombatStrategies.Idle,combatStrategyWeights_BackOffRange.Idle},

            };

            float totalChance = combatStrategiesWeightPair_BackOffRange.Values.Sum();

            float randomValue = UnityEngine.Random.Range(0.1f, totalChance);

            foreach (var pair in combatStrategiesWeightPair_BackOffRange)
            {
                CommonCombatStrategies strategy = pair.Key;
                float weight = pair.Value;
                if (randomValue <= weight)
                {
                    return strategy;
                }

                randomValue -= weight;
            }

            return CommonCombatStrategies.Idle;
        }

        private CommonCombatStrategies RollAndGetCombatStrategies_MidRange()
        {
            Dictionary<CommonCombatStrategies, float> combatStrategiesWeightPair_MidRange =
            new Dictionary<CommonCombatStrategies, float>
            {
                {CommonCombatStrategies.CloseRange_Attack,combatStrategyWeights_MidRange.closeRange_Attack},
                {CommonCombatStrategies.LongRange_Attack,combatStrategyWeights_MidRange.LongRange_Attack},
                {CommonCombatStrategies.ComboAttack_CloseRange,combatStrategyWeights_MidRange.ComboAttack_CloseRange},
                {CommonCombatStrategies.CloseGapAndAttack,combatStrategyWeights_MidRange.CloseGapAndAttack},
                {CommonCombatStrategies.CloseGapAndAttack_Combo,combatStrategyWeights_MidRange.CloseGapAndAttack_Combo},
                {CommonCombatStrategies.CloseGapBlend_And_Attack,combatStrategyWeights_MidRange.CloseGapBlend_And_Attack},
                {CommonCombatStrategies.CloseGapBlend_And_AttackWithCombo,combatStrategyWeights_MidRange.CloseGapBlend_And_AttackWithCombo},
                {CommonCombatStrategies.LeapAttack,combatStrategyWeights_MidRange.leap_Attack_Strat},
                { CommonCombatStrategies.BackOff,combatStrategyWeights_MidRange.BackOff},
                {CommonCombatStrategies.Strafe,combatStrategyWeights_MidRange.Strafe},
                {CommonCombatStrategies.Idle,combatStrategyWeights_MidRange.Idle},

            };

            float totalChance = combatStrategiesWeightPair_MidRange.Values.Sum();

            float randomValue = UnityEngine.Random.Range(0.1f, totalChance);

            foreach (var pair in combatStrategiesWeightPair_MidRange)
            {
                CommonCombatStrategies strategy = pair.Key;
                float weight = pair.Value;
                if (randomValue <= weight)
                {
                    return strategy;
                }

                randomValue -= weight;
            }

            return CommonCombatStrategies.Idle;
        }

        private CommonCombatStrategies RollAndGetCombatStrategies_LongRange()
        {
            Dictionary<CommonCombatStrategies, float> combatStrategiesWeightPair_LongRange =
            new Dictionary<CommonCombatStrategies, float>
            {
                {CommonCombatStrategies.CloseRange_Attack,combatStrategyWeights_LongRange.closeRange_Attack},
                {CommonCombatStrategies.LongRange_Attack,combatStrategyWeights_LongRange.LongRange_Attack},
                {CommonCombatStrategies.ComboAttack_CloseRange,combatStrategyWeights_LongRange.ComboAttack_CloseRange},
                {CommonCombatStrategies.CloseGapAndAttack,combatStrategyWeights_LongRange.CloseGapAndAttack},
                {CommonCombatStrategies.CloseGapAndAttack_Combo,combatStrategyWeights_LongRange.CloseGapAndAttack_Combo},
                {CommonCombatStrategies.CloseGapBlend_And_Attack,combatStrategyWeights_LongRange.CloseGapBlend_And_Attack},
                {CommonCombatStrategies.CloseGapBlend_And_AttackWithCombo,combatStrategyWeights_LongRange.CloseGapBlend_And_AttackWithCombo},
                {CommonCombatStrategies.LeapAttack,combatStrategyWeights_LongRange.leap_Attack_Strat},
                { CommonCombatStrategies.BackOff,combatStrategyWeights_LongRange.BackOff},
                {CommonCombatStrategies.Strafe,combatStrategyWeights_LongRange.Strafe},
                {CommonCombatStrategies.Idle,combatStrategyWeights_LongRange.Idle},

            };

            float totalChance = combatStrategiesWeightPair_LongRange.Values.Sum();

            float randomValue = UnityEngine.Random.Range(0.1f, totalChance);

            foreach (var pair in combatStrategiesWeightPair_LongRange)
            {
                CommonCombatStrategies strategy = pair.Key;
                float weight = pair.Value;
                if (randomValue <= weight)
                {
                    return strategy;
                }

                randomValue -= weight;
            }

            return CommonCombatStrategies.Idle;
        }



        public bool CheckIfInCombatModified_Range()
        {
            Vector3 startPoint = npcRoot.transform.position;
            if (npcRoot.IsPlayerInRange_Sphere(startPoint, combatRadius_Modified))
            {
                return true;
            }

            return false;

        }

        public bool CheckIfInCombatRange()
        {
            Vector3 startPoint = npcRoot.transform.position;
            if (npcRoot.IsPlayerInRange_Sphere(startPoint, combatRadius))
            {
                return true;
            }

            return false;

        }


        public bool IsPlayerInCloseRange()
        {
            Vector3 startPoint = npcRoot.transform.position;
            if (npcRoot.IsPlayerInRange_Sphere(startPoint, closeRange_Radius))
            {
                currentCombatZone = CombatZone.Close_Range;
                //Debug.Log("<color=cyan>Current Zone = </color>" + currentCombatZone);
                return true;
            }

            return false;
        }

        public bool isPlayerInBackoffRange()
        {
            Vector3 startPoint = npcRoot.transform.position;
            if (npcRoot.IsPlayerInRange_Sphere(startPoint, backoff_Range_Radius))
            {
                currentCombatZone = CombatZone.Backoff_Range;
                //Debug.Log("<color=cyan>Current Zone = </color>" + currentCombatZone);
                return true;
            }

            return false;
        }

        public bool isPlayerInMidRange()
        {
            Vector3 startPoint = npcRoot.transform.position;
            if (npcRoot.IsPlayerInRange_Sphere(startPoint, midRange_Radius))
            {
                currentCombatZone = CombatZone.Mid_Range;
                //Debug.Log("<color=cyan>Current Zone = </color>" + currentCombatZone);
                return true;
            }

            return false;
        }

        public bool isPlayerInLongRange()
        {
            Vector3 startPoint = npcRoot.transform.position;
            if (npcRoot.IsPlayerInRange_Sphere(startPoint, longRange_Radius))
            {
                currentCombatZone = CombatZone.Long_Range;
                //Debug.Log("<color=cyan>Current Zone = </color>" + currentCombatZone);
                return true;
            }

            return false;
        }


        public void ResetEnemyState()
        {
            inCombatRadius = false;
            enteredCombat = false;
            forceDecide = false;
            forceRollForDefense = false;
            canCheckHealthDifference = false;
            isIdling = false;
            isAttacking = false;
            currentCombatStrategy = CommonCombatStrategies.Idle;
            currentCombatZone = CombatZone.Outof_Range;
            elapsedDecisionTime = 0f;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // if(combatStrategyWeights_CloseRange.LongRange_Attack > 0)
            // {
            //     Debug.LogWarning("Setting Long Range Strategy Weight in Close Range is redundant," +
            //         "Modify Combat Zone Weights of attacks to perform longrange attacks in closerange Zone ");
            // }


            // if(combatStrategyWeights_MidRange.closeRange_Attack > 0 || combatStrategyWeights_LongRange.closeRange_Attack > 0)
            // {
            //     Debug.LogWarning("Setting Close Range Strategy Weight in Mid or Long Range is redundant," +
            //         "Modify Combat Zone Weights of attacks to perform close range attacks in long or midrange Zone ");
            // }
        }



        void OnDrawGizmos()
        {
            //Combat/Long Range Radius
            VisualiseSphereRange(longRange_Radius, Color.green);

            //mid range Radius
            VisualiseSphereRange(midRange_Radius, Color.yellow);

            //backOffRange Radius
            VisualiseSphereRange(backoff_Range_Radius, Color.blue);

            //close range Radius
            VisualiseSphereRange(closeRange_Radius, Color.red);

        }

        void VisualiseSphereRange(float radius, Color color)
        {

            Gizmos.color = color;
            Gizmos.DrawWireSphere(transform.position, radius);
        }



#endif

    }

    public enum CommonCombatStrategies
    {
        CloseRange_Attack,  //Standard single attack when in close range
        LongRange_Attack,  //Standard single attack when in long range
        ComboAttack_CloseRange, //Combo attacks when in close range
        CloseGapAndAttack, //Close the gap by running or dashing to the player and attack
        CloseGapAndAttack_Combo, //Close the gap by running or dashing to the player and attack with a combo
        CloseGapBlend_And_Attack, //Play windup animation and then close the gap and complete the attack
        CloseGapBlend_And_AttackWithCombo, //Play windup animation and then close the gap and complete the attack and then combo attack
        LeapAttack, //Leap towards the player and attack
        BackOff, //strafe back or Backstep to avoid damage
        Strafe, // strafe left or right (mostly) or front and back (rarely) to avoid damage for certain amount of time
        Idle, // idle for a certain amount of time
    }

    public enum CombatZone
    {
        Close_Range, Backoff_Range, Mid_Range, Long_Range, Outof_Range
    }

    public enum MidCombatMovement
    {
        Run, Walk, Idle
    }

    [System.Serializable]
    public class WeightsByCombatZone
    {
        public float closeRange_Weight = 10f;
        public float backOffRange_Weight = 10f;
        public float midRange_Weight = 10f;
        public float longRange_Weight = 10f;
    }

    [System.Serializable]
    public class CombatStrategyWeights
    {
        public float closeRange_Attack = 10f;
        public float LongRange_Attack = 10f;
        public float ComboAttack_CloseRange = 10f;
        public float CloseGapAndAttack = 10f;
        public float CloseGapAndAttack_Combo = 10f;
        public float CloseGapBlend_And_Attack = 10f;
        public float CloseGapBlend_And_AttackWithCombo = 10f;
        public float leap_Attack_Strat = 0f;
        public float BackOff = 10f;
        public float Strafe = 10f;
        public float Idle = 10f;
    }

    [Serializable]

    public class Attack
    {
        public AnimationClip attackAnimClip;
        public float damage;
        public float staminaCost = 10f;
        public bool canAttackKnockback = false;

        public bool canAddedInCombo = false;
        public bool canAddedInCloseGap = false;
        public bool inStrategy = false; // can be used to check if it is already being used in an strategy, b4 adding it to other strategy
        public WeightsByCombatZone weightsByCombatZone;

        public float comboTransitionTime = .1f; //for comboChaining
        public float comboEntryNormalizedTransitionTime = .1f; //for comboChaining

        public UnityEvent onAttackBegin;
        public UnityEvent onAttackEnd;


    }

    [Serializable]

    public class CloseGapBlendAttack
    {
        public AnimationClip attackWindUpAnimClip;
        public AnimationClip endAttackClip;

        public bool canAttackKnockback = false;
        public float damage;
        public float staminaCost = 10f;
        public float weight = 10f;
        public string attackTransitionBoolName;

        //public bool inStrategy = false; // can be used to check if it is already being used in an strategy, b4 adding it to other strategy



    }

}








