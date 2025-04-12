using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAdvanced_State : State
{
    public bool canBackStep = false; // Can the enemy backstep?
    public bool chaseToAttackAtStart = true; // Can the enemy must chase and attack after seeing enemy or wait and DecideStrategy in combat radius

    [Tooltip("Distance to attack player in Long range, the value must be within combat radius")]
    public float min_longRangeAttackDistance = 3f;

    [Tooltip("Distance to attack player in Short range, the value must be within combat radius")]
    public float max_shortRangeAttackDistance = 1.5f;

    public float combatRadius = 8f;
    public bool inStrafing = false;
    public float combatRadius_Modified;
    [SerializeField] public bool inCombatRadius = false;

    [SerializeField] public float strafeChance = 40f;
    [SerializeField] private bool canStrafe = false;
    [SerializeField] public bool enteredCombat = false;

    [SerializeField] private ChaseState chaseState;
    [SerializeField] private IdleState idleState;
    [SerializeField] private StrafeState strafeState;
    [SerializeField]private float combatRadius_Offset = 0.5f;


    // [Space]
    // [Header("Debug Variables")]
    // [Space]

    void Awake()
    {
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
        float distance = Vector3.Distance(npcRoot.transform.position, npcRoot.targetTransform.position);
        if(distance <= max_shortRangeAttackDistance)
        {
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
