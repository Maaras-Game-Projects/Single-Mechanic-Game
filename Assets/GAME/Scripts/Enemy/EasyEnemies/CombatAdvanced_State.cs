using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatAdvanced_State : State
{

    [SerializeField]public bool chaseToAttackAtStart = true; // Can the enemy must chase and attack after seeing enemy or wait and DecideStrategy in combat radius

    // [Tooltip("Distance to attack player in Long range, the value must be within combat radius")]
    // public float min_longRangeAttackDistance = 3f;

    // [Tooltip("Distance to attack player in Short range, the value must be within combat radius")]
    // public float max_shortRangeAttackDistance = 1.5f;

    [SerializeField]public float combatRadius = 8f;
    [SerializeField]private float combatRadius_Modified;
    [SerializeField] public bool inCombatRadius = false;

    [SerializeField] private bool enteredCombat = false;

    public bool EnteredCombat => enteredCombat;

    [SerializeField] private ChaseState chaseState;
    [SerializeField] private IdleState idleState;
    [SerializeField] private StrafeState strafeState;
    [SerializeField] private CloseGapAndAttack_State closeGapAndAttack_State;
    [SerializeField] private CloseGapBlendAndAttack closeGapBlendAndAttack_State;
    [SerializeField]private float combatRadius_Offset = 0.5f;
    [SerializeField]private float decisionInterval = 3f;
    [SerializeField]private bool forceDecide = false;
    [SerializeField]private bool isAttacking = false;
    [SerializeField]private bool isIdling = false;
    [SerializeField]private MidCombatMovement midCombatMovement;

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

    [Range(0,100)]
    [SerializeField] float chanceToGoDefensive_OnLowStamina = 50f;

    [SerializeField] WeightsByCombatZone strafeStrategy_Weights;

    [Space]
    [Header("Attack Variables")]
    [Space]

    [SerializeField]public List<Attack> attacks = new List<Attack>();

    [Space]
    [Header("Debug Variables")]
    [Space]

    [SerializeField] CombatZone currentCombatZone = CombatZone.Outof_Range;
    [SerializeField] CommonCombatStrategies currentCombatStrategy = CommonCombatStrategies.Idle;
    [SerializeField] float elapsedDecisionTime = 0f;

    [Space]

    [SerializeField] List<Attack> CloseRangeAttacks = new List<Attack>();

    private Coroutine attackStrategyWaitCoroutine = null;

    public CombatZone CurrentCombatZone => currentCombatZone;


    void OnDisable()
    {
        StopAllCoroutines();
    }

    void Awake()
    {
        combatRadius = longRange_Radius;
        combatRadius_Modified = combatRadius - combatRadius_Offset; // set the modified combat radius to be slightly smaller than the original combat radius

        AddCloseRangeAttacks();

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

    public override void OnEnter()
    {
        elapsedDecisionTime = 0f;
        forceDecide = true;
        enteredCombat = true;
    }

    public override void OnExit()
    {
        
    }

    public override void TickLogic()
    {

        if (npcRoot.isPlayerInLineOfSight())
        {
            HandleTurnAndRotation();
 
        }
        else
        {
            npcRoot.statemachine.SwitchState(chaseState);
        }


        elapsedDecisionTime += Time.deltaTime;
        
        HandleMidCombatMovementAnimation();

        if (isAttacking || isIdling || npcRoot.isInteracting) return;

        if (elapsedDecisionTime >= decisionInterval || forceDecide)
        {

            CommonCombatStrategies strategyToPerform = DetermineCombatStrategy();

            PerformStrategy(strategyToPerform);
            elapsedDecisionTime = 0f;
            forceDecide = false;
        }
    }

    private void HandleTurnAndRotation()
    {
        if (!isAttacking)
        {
            npcRoot.TurnCharacter();
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

       

        UpateCurrentCombatZone();

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
            Debug.Log("<color=green>Current Strategy = </color>" + currentCombatStrategy);
            npcRoot.statemachine.SwitchState(strafeState);
        }
        else if (strategyToPerform == CommonCombatStrategies.CloseRange_Attack)
        {
            currentCombatStrategy = strategyToPerform;
            Debug.Log("<color=red>Current Strategy = </color>" + currentCombatStrategy);
            PerformCloseRangeAttack();
        }
        else if (strategyToPerform == CommonCombatStrategies.CloseGapAndAttack)
        {
            currentCombatStrategy = strategyToPerform;
            Debug.Log("<color=red>Current Strategy = </color>" + currentCombatStrategy);
            npcRoot.statemachine.SwitchState(closeGapAndAttack_State);
        }
        else if (strategyToPerform == CommonCombatStrategies.CloseGapBlend_And_Attack)
        {
            currentCombatStrategy = strategyToPerform;
            Debug.Log("<color=blue>Current Strategy = </color>" + currentCombatStrategy);
            npcRoot.statemachine.SwitchState(closeGapBlendAndAttack_State);
        }
        // else if (strategyToPerform == CommonCombatStrategies.Idle)
        // {
        //     currentCombatStrategy = strategyToPerform;
        //     isIdling = true;
        //     //Need to add Idle Feature
        // }
        else
        {
            currentCombatStrategy = strategyToPerform;
            Debug.Log("<color=red>Current Strategy = </color>" + currentCombatStrategy);
            //npcRoot.staminaSystem.DepleteStamina(20f);
            idleState.GoToIdleAnimation();
        }
    }

    private void PerformCloseRangeAttack()
    {
        Attack attackToPerform = RollAndGetCloseRangeAttack();

        if(npcRoot.staminaSystem.CurrentStamina < attackToPerform.staminaCost)
        {
            //Roll for All combat Strat, or Roll for Defensive Strat based on defensive weight
            Debug.Log("<color=red>Strategy failed= </color>" + currentCombatStrategy);
            forceDecide = true;
        }
        else
        {
           
            isAttacking = true;
            npcRoot.staminaSystem.DepleteStamina(attackToPerform.staminaCost);
            npcRoot.currentDamageToDeal = attackToPerform.damage;
            npcRoot.PlayAnyActionAnimation(attackToPerform.attackAnimClip.name,true);
           
            float waitTime = attackToPerform.attackAnimClip.length;
            attackStrategyWaitCoroutine = StartCoroutine(OnAttackStrategyComplete(waitTime));

        }
        
        
    }

    IEnumerator OnAttackStrategyComplete(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        isAttacking = false;

    }

    private Attack RollAndGetCloseRangeAttack()
    {
        if(CloseRangeAttacks.Count == 0) return null;

        float totalAttackChance = 0f;
        
        foreach (Attack attack in CloseRangeAttacks)
        {
            totalAttackChance += attack.weightsByCombatZone.closeRange_Weight;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalAttackChance);

        

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

    private void UpateCurrentCombatZone()
    {
        if(IsPlayerInCloseRange()) return;
        if(isPlayerInBackoffRange()) return;
        if(isPlayerInMidRange()) return;
        if(isPlayerInLongRange()) return;

        currentCombatZone = CombatZone.Outof_Range;
        //Debug.Log("<color=yellow>Current Zone = </color>" + currentCombatZone);
    }

    private CommonCombatStrategies RollAndGetCombatStrategies_Offensive()
    {
        Dictionary<CommonCombatStrategies,float> combatStrategiesWeightPair_CloseRange = 
        new Dictionary<CommonCombatStrategies, float>
        {
            {CommonCombatStrategies.CloseRange_Attack,combatStrategyWeights_CloseRange.closeRange_Attack},
            {CommonCombatStrategies.LongRange_Attack,combatStrategyWeights_CloseRange.LongRange_Attack},
            {CommonCombatStrategies.ComboAttack_CloseRange,combatStrategyWeights_CloseRange.ComboAttack_CloseRange},
            {CommonCombatStrategies.CloseGapAndAttack,combatStrategyWeights_CloseRange.CloseGapAndAttack},
            {CommonCombatStrategies.CloseGapAndAttack_Combo,combatStrategyWeights_CloseRange.CloseGapAndAttack_Combo},
            {CommonCombatStrategies.CloseGapBlend_And_Attack,combatStrategyWeights_CloseRange.CloseGapBlend_And_Attack},
            {CommonCombatStrategies.CloseGapBlend_And_AttackWithCombo,combatStrategyWeights_CloseRange.CloseGapBlend_And_AttackWithCombo},
            {CommonCombatStrategies.BackOff,combatStrategyWeights_CloseRange.BackOff},
            {CommonCombatStrategies.Strafe,combatStrategyWeights_CloseRange.Strafe},
            {CommonCombatStrategies.Idle,combatStrategyWeights_CloseRange.Idle},
            
        };

        float totalChance =  combatStrategiesWeightPair_CloseRange.Values.Sum();

        float randomValue = UnityEngine.Random.Range(0.1f,totalChance);

        foreach (var  pair in combatStrategiesWeightPair_CloseRange)
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
        Dictionary<CommonCombatStrategies,float> combatStrategiesWeightPair_CloseRange = 
        new Dictionary<CommonCombatStrategies, float>
        {
            {CommonCombatStrategies.CloseRange_Attack,combatStrategyWeights_CloseRange.closeRange_Attack},
            {CommonCombatStrategies.LongRange_Attack,combatStrategyWeights_CloseRange.LongRange_Attack},
            {CommonCombatStrategies.ComboAttack_CloseRange,combatStrategyWeights_CloseRange.ComboAttack_CloseRange},
            {CommonCombatStrategies.CloseGapAndAttack,combatStrategyWeights_CloseRange.CloseGapAndAttack},
            {CommonCombatStrategies.CloseGapAndAttack_Combo,combatStrategyWeights_CloseRange.CloseGapAndAttack_Combo},
            {CommonCombatStrategies.CloseGapBlend_And_Attack,combatStrategyWeights_CloseRange.CloseGapBlend_And_Attack},
            {CommonCombatStrategies.CloseGapBlend_And_AttackWithCombo,combatStrategyWeights_CloseRange.CloseGapBlend_And_AttackWithCombo},
            {CommonCombatStrategies.BackOff,combatStrategyWeights_CloseRange.BackOff},
            {CommonCombatStrategies.Strafe,combatStrategyWeights_CloseRange.Strafe},
            {CommonCombatStrategies.Idle,combatStrategyWeights_CloseRange.Idle},
            
        };

        float totalChance =  combatStrategiesWeightPair_CloseRange.Values.Sum();

        float randomValue = UnityEngine.Random.Range(0.1f,totalChance);

        foreach (var  pair in combatStrategiesWeightPair_CloseRange)
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
        Dictionary<CommonCombatStrategies,float> combatStrategiesWeightPair_BackOffRange = 
        new Dictionary<CommonCombatStrategies, float>
        {
            {CommonCombatStrategies.CloseRange_Attack,combatStrategyWeights_BackOffRange.closeRange_Attack},
            {CommonCombatStrategies.LongRange_Attack,combatStrategyWeights_BackOffRange.LongRange_Attack},
            {CommonCombatStrategies.ComboAttack_CloseRange,combatStrategyWeights_BackOffRange.ComboAttack_CloseRange},
            {CommonCombatStrategies.CloseGapAndAttack,combatStrategyWeights_BackOffRange.CloseGapAndAttack},
            {CommonCombatStrategies.CloseGapAndAttack_Combo,combatStrategyWeights_BackOffRange.CloseGapAndAttack_Combo},
            {CommonCombatStrategies.CloseGapBlend_And_Attack,combatStrategyWeights_BackOffRange.CloseGapBlend_And_Attack},
            {CommonCombatStrategies.CloseGapBlend_And_AttackWithCombo,combatStrategyWeights_BackOffRange.CloseGapBlend_And_AttackWithCombo},
            {CommonCombatStrategies.BackOff,combatStrategyWeights_BackOffRange.BackOff},
            {CommonCombatStrategies.Strafe,combatStrategyWeights_BackOffRange.Strafe},
            {CommonCombatStrategies.Idle,combatStrategyWeights_BackOffRange.Idle},
            
        };

        float totalChance =  combatStrategiesWeightPair_BackOffRange.Values.Sum();

        float randomValue = UnityEngine.Random.Range(0.1f,totalChance);

        foreach (var  pair in combatStrategiesWeightPair_BackOffRange)
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
        Dictionary<CommonCombatStrategies,float> combatStrategiesWeightPair_MidRange = 
        new Dictionary<CommonCombatStrategies, float>
        {
            {CommonCombatStrategies.CloseRange_Attack,combatStrategyWeights_MidRange.closeRange_Attack},
            {CommonCombatStrategies.LongRange_Attack,combatStrategyWeights_MidRange.LongRange_Attack},
            {CommonCombatStrategies.ComboAttack_CloseRange,combatStrategyWeights_MidRange.ComboAttack_CloseRange},
            {CommonCombatStrategies.CloseGapAndAttack,combatStrategyWeights_MidRange.CloseGapAndAttack},
            {CommonCombatStrategies.CloseGapAndAttack_Combo,combatStrategyWeights_MidRange.CloseGapAndAttack_Combo},
            {CommonCombatStrategies.CloseGapBlend_And_Attack,combatStrategyWeights_MidRange.CloseGapBlend_And_Attack},
            {CommonCombatStrategies.CloseGapBlend_And_AttackWithCombo,combatStrategyWeights_MidRange.CloseGapBlend_And_AttackWithCombo},
            {CommonCombatStrategies.BackOff,combatStrategyWeights_MidRange.BackOff},
            {CommonCombatStrategies.Strafe,combatStrategyWeights_MidRange.Strafe},
            {CommonCombatStrategies.Idle,combatStrategyWeights_MidRange.Idle},
            
        };

        float totalChance =  combatStrategiesWeightPair_MidRange.Values.Sum();

        float randomValue = UnityEngine.Random.Range(0.1f,totalChance);

        foreach (var  pair in combatStrategiesWeightPair_MidRange)
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
        Dictionary<CommonCombatStrategies,float> combatStrategiesWeightPair_LongRange = 
        new Dictionary<CommonCombatStrategies, float>
        {
            {CommonCombatStrategies.CloseRange_Attack,combatStrategyWeights_LongRange.closeRange_Attack},
            {CommonCombatStrategies.LongRange_Attack,combatStrategyWeights_LongRange.LongRange_Attack},
            {CommonCombatStrategies.ComboAttack_CloseRange,combatStrategyWeights_LongRange.ComboAttack_CloseRange},
            {CommonCombatStrategies.CloseGapAndAttack,combatStrategyWeights_LongRange.CloseGapAndAttack},
            {CommonCombatStrategies.CloseGapAndAttack_Combo,combatStrategyWeights_LongRange.CloseGapAndAttack_Combo},
            {CommonCombatStrategies.CloseGapBlend_And_Attack,combatStrategyWeights_LongRange.CloseGapBlend_And_Attack},
            {CommonCombatStrategies.CloseGapBlend_And_AttackWithCombo,combatStrategyWeights_LongRange.CloseGapBlend_And_AttackWithCombo},
            {CommonCombatStrategies.BackOff,combatStrategyWeights_LongRange.BackOff},
            {CommonCombatStrategies.Strafe,combatStrategyWeights_LongRange.Strafe},
            {CommonCombatStrategies.Idle,combatStrategyWeights_LongRange.Idle},
            
        };

        float totalChance =  combatStrategiesWeightPair_LongRange.Values.Sum();

        float randomValue = UnityEngine.Random.Range(0.1f,totalChance);

        foreach (var  pair in combatStrategiesWeightPair_LongRange)
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
        if(npcRoot.IsPlayerInRange_Sphere(startPoint, combatRadius_Modified))
        {
            return true;
        }

        return false;
        
    }

    public bool CheckIfInCombatRange()
    {
        Vector3 startPoint = npcRoot.transform.position;
        if(npcRoot.IsPlayerInRange_Sphere(startPoint, combatRadius))
        {
            return true;
        }

        return false;
        
    }


    public bool IsPlayerInCloseRange()
    {
        Vector3 startPoint = npcRoot.transform.position;
        if(npcRoot.IsPlayerInRange_Sphere(startPoint, closeRange_Radius))
        {
            currentCombatZone = CombatZone.Close_Range;
            Debug.Log("<color=cyan>Current Zone = </color>" + currentCombatZone);
            return true;
        }

        return false;
    }

    private bool isPlayerInBackoffRange()
    {
        Vector3 startPoint = npcRoot.transform.position;
        if(npcRoot.IsPlayerInRange_Sphere(startPoint, backoff_Range_Radius))
        {
            currentCombatZone = CombatZone.Backoff_Range;
            Debug.Log("<color=cyan>Current Zone = </color>" + currentCombatZone);
            return true;
        }

        return false;
    }

    private bool isPlayerInMidRange()
    {
        Vector3 startPoint = npcRoot.transform.position;
        if(npcRoot.IsPlayerInRange_Sphere(startPoint, midRange_Radius))
        {
            currentCombatZone = CombatZone.Mid_Range;
            Debug.Log("<color=cyan>Current Zone = </color>" + currentCombatZone);
            return true;
        }

        return false;
    }

    private bool isPlayerInLongRange()
    {
        Vector3 startPoint = npcRoot.transform.position;
        if(npcRoot.IsPlayerInRange_Sphere(startPoint, longRange_Radius))
        {
            currentCombatZone = CombatZone.Long_Range;
            Debug.Log("<color=cyan>Current Zone = </color>" + currentCombatZone);
            return true;
        }

        return false;
    }

    
    
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
    Run, Walk,Idle
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
    public float LongRange_Attack  = 10f;
    public float ComboAttack_CloseRange = 10f;
    public float CloseGapAndAttack = 10f;
    public float CloseGapAndAttack_Combo = 10f;
    public float CloseGapBlend_And_Attack = 10f;
    public float CloseGapBlend_And_AttackWithCombo = 10f;
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

    public bool canAddedInCombo = false;
    public bool canAddedInCloseGap= false;
    public bool inStrategy = false; // can be used to check if it is already being used in an strategy, b4 adding it to other strategy
    public WeightsByCombatZone weightsByCombatZone;
    

}


[Serializable]

public class CloseGapBlendAttack
{
    public AnimationClip attackWindUpAnimClip;
    public AnimationClip endAttackClip;
    public float damage;
    public float staminaCost = 10f;
    public float weight = 10f;
    public string attackTransitionBoolName;

    //public bool inStrategy = false; // can be used to check if it is already being used in an strategy, b4 adding it to other strategy
   
    

}

