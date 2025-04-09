using System.Collections.Generic;
using UnityEngine;

public class CombatAdvanced_State : State
{
    public bool canBackStep = false; // Can the enemy backstep?

    [Tooltip("Distance to attack player in Long range, the value must be within combat radius")]
    public float min_longRangeAttackDistance = 3f;

    [Tooltip("Distance to attack player in Short range, the value must be within combat radius")]
    public float max_shortRangeAttackDistance = 1.5f;

    public float combatRadius = 8f;
    public bool inStrafing = false;
    public float combatRadius_Modified;
    [SerializeField] public bool inCombatRadius = false;

    [SerializeField] private ChaseState chaseState;
    [SerializeField] private IdleState idleState;

    [Space]
    [Header("Strafe Variables")]
    [Space]

    [SerializeField] private float strafeChance = 40f;
    [SerializeField] private float maxStrafeDuration_Left = 2f;
    [SerializeField] private float maxStrafeDuration_Right = 2f;
    [SerializeField] private float maxStrafeDuration_Back = 2f;
    [SerializeField] private float maxStrafeDuration_Front = 2f;

    [SerializeField] private float strafeSpeed = .5f;

    public override void OnEnter()
    {

    }

    public override void OnExit()
    {
        
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
