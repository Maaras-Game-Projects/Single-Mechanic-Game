using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAdvanced_State : State
{
    [SerializeField] private bool canBackStep = false; // Can the enemy backstep?
    [SerializeField]public bool chaseToAttackAtStart = true; // Can the enemy must chase and attack after seeing enemy or wait and DecideStrategy in combat radius

    // [Tooltip("Distance to attack player in Long range, the value must be within combat radius")]
    // public float min_longRangeAttackDistance = 3f;

    // [Tooltip("Distance to attack player in Short range, the value must be within combat radius")]
    // public float max_shortRangeAttackDistance = 1.5f;

    [SerializeField]public float combatRadius = 8f;
    public bool inStrafing = false;
    [SerializeField]private float combatRadius_Modified;
    [SerializeField] public bool inCombatRadius = false;

    [SerializeField] public float strafeChance = 40f;
    [SerializeField] private bool canStrafe = false;
    [SerializeField] private bool enteredCombat = false;

    public bool EnteredCombat => enteredCombat;

    [SerializeField] private ChaseState chaseState;
    [SerializeField] private IdleState idleState;
    [SerializeField] private StrafeState strafeState;
    [SerializeField]private float combatRadius_Offset = 0.5f;

    [Space]
    [Header("Combat Zone Variables")]
    [Space]

    [SerializeField] float closeRange_Radius = 1.5f; // same as MaxCloseRange attack radius
    [SerializeField] float backoff_Range_Radius = 3f;
    [SerializeField] float midRange_Radius = 5.5f;
    [SerializeField] float longRange_Radius = 8f; // same as combatRadius

    [Space]
    [Header("Debug Variables")]
    [Space]

    [SerializeField] CombatZone currentCombatZone = CombatZone.Outof_Range;

    public CombatZone CurrentCombatZone => currentCombatZone;

    void Awake()
    {
        combatRadius = longRange_Radius;
        combatRadius_Modified = combatRadius - combatRadius_Offset; // set the modified combat radius to be slightly smaller than the original combat radius

    }

    public override void OnEnter()
    {
        enteredCombat = true;
    }

    public override void OnExit()
    {
        
    }

    public override void TickLogic()
    {
        npcRoot.LookAtPlayer();
        
        // Example usage of strafe
        if(canStrafe)
            npcRoot.statemachine.SwitchState(strafeState);
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
    CircleStrafe, //(rare use)strafe around the player (Clockwise/Anticlockwise) to avoid damage for certain amount of time
    Idle, // idle for a certain amount of time
}

public enum CombatZone
{
    Close_Range, Backoff_Range, Mid_Range, Long_Range, Outof_Range
}
