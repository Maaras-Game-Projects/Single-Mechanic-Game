using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CombatState : State
{
    public bool strafeAround = false;
    public bool instantAttack = false;

    [SerializeField] private bool inAttackDelay = false;

    public List<Attack> longRangeAttacks = new List<Attack>();
    public List<Attack> closeRangeAttacks = new List<Attack>();

    [Tooltip("Distance to attack player in Long range, the value must be within combat radius")]
    public float min_longRangeAttackDistance = 3f;

    [Tooltip("Distance to attack player in Short range, the value must be within combat radius")]
    public float max_shortRangeAttackDistance = 1.5f;


    public float combatRadius = 5f;
    public float combatRadius_Modified;
    [SerializeField] public bool inCombatRadius = false;

    [SerializeField] private ChaseState chaseState;
    [SerializeField] private IdleState idleState;

    private Coroutine attackDelayCoroutine = null;
    private Coroutine actionAfterAttackCoroutine = null;
    private Coroutine rollForStrafeCoroutine = null;

    void Awake()
    {
        combatRadius_Modified = combatRadius - .2f; // set the modified combat radius to be slightly smaller than the original combat radius
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public override void OnEnter()
    {
       //chaseState.navMeshAgent.enabled = false;
    }
    public override void OnExit()
    {
        chaseState.navMeshAgent.isStopped = true;
        chaseState.navMeshAgent.velocity = Vector3.zero;
        npcRoot.rigidBody.linearVelocity = Vector3.zero;
    }

    public override void TickLogic()
    {
        npcRoot.LookAtPlayer(); //look at player
        
        if(npcRoot.npc_RootMotionUseStatus) return; //if root motion is being used, do not perform any other actions

        if(inAttackDelay) return; //if in attack delay, do not perform any other actions
        
        //Add Perform Backstep logic if present

        float distanceToTarget = Vector3.Distance(npcRoot.transform.position, npcRoot.targetTransform.position);

        //If long range attack is availabe, check if player is in long range attack distance and perform long range attack
        if(longRangeAttacks.Count > 0)
        {
            
            if(distanceToTarget >= min_longRangeAttackDistance)
            {
                Attack attackToPerform = RollAndGetAttacks(longRangeAttacks);
                if(attackToPerform != null)
                {
                    npcRoot.PlayAnyActionAnimation(attackToPerform.attackAnimation.name, true);
                    // enable attack delay if present
                    // trigger strafe state if present or go to idle state
                    return;
                }

            }
        }

        //If short range attack is availabe, check if player is in short range attack distance and perform short range attack
        if(closeRangeAttacks.Count > 0)
        {
            
            if(distanceToTarget <= max_shortRangeAttackDistance)
            {
                Attack attackToPerform = RollAndGetAttacks(closeRangeAttacks);
                if(attackToPerform != null)
                {
                    npcRoot.PlayAnyActionAnimation(attackToPerform.attackAnimation.name, true);

                    // enable attack delay if present
                    float waitTime = attackToPerform.attackAnimation.length + attackToPerform.attackDelay;
                    
                    //add logic for chain/combo attacks if present

                    attackDelayCoroutine = StartCoroutine(DisableAttackDelay(waitTime));
                    
                    if(attackToPerform.attackDelay == 0f) //roll and trigger strafe state if present or go to idle state after attack complete
                    {
                        rollForStrafeCoroutine = StartCoroutine(RollForStrafeAfterDelay(attackToPerform.attackAnimation.length));
                    }
                    else if(attackToPerform.attackDelay > 0f) //roll and trigger strafe state if present or go to idle state after delay
                    {
                        actionAfterAttackCoroutine = StartCoroutine(
                            SwitchToIdleAfterAttackAndRollForNextAction(attackToPerform.attackAnimation.length, attackToPerform.attackDelay));
                    }

                    return;
                }

            }
        }

        //If no attack is available, or no attack is possible, trigger strafe state if present or go to idle state
        RollForStrafeAround();
        Debug.Log("<color=red>no attack available, set to Idle</color>");

    }

   
    private Attack RollAndGetAttacks(List<Attack> attackList)
    {
        //AnimationClip attackAnimationToPlay = null;
        float totalAttackChance = 0f;
        float randomValue = UnityEngine.Random.Range(0f, 100f);

        foreach (Attack attack in attackList)
        {
            totalAttackChance += attack.attackChance;
        }

        foreach (Attack attack in attackList)
        {
            if (randomValue <= attack.attackChance)
            {
                return attack;
            }

            randomValue -= attack.attackChance;
        }

        return null;
    }

    public bool CheckIfInCombatRange()
    {
        Vector3 startPoint = npcRoot.transform.position;
        if(npcRoot.IsPlayerInRange_Sphere(startPoint, combatRadius_Modified))
        {
            return true;
        }

        return false;
        
    }

    IEnumerator DisableAttackDelay(float delayTime)
    {
        inAttackDelay = true;
        yield return new WaitForSeconds(delayTime);
        Debug.Log("<color=yellow>attack delay disabled</color>");
        inAttackDelay = false;
    }

    IEnumerator RollForStrafeAfterDelay(float delayTime)
    {

        yield return new WaitForSeconds(delayTime);
        Debug.Log("<color=red>roll strafe delay</color>");
        RollForStrafeAround();
    }

    private void RollForStrafeAround()
    {
        if (strafeAround)
        {
            //roll for strafe state if present
            //trigger strafe state or go to idle state based on roll

        }
        else
        {
            //go to idle state
            npcRoot.statemachine.SwitchState(idleState);
        }
    }

    IEnumerator SwitchToIdleAfterAttackAndRollForNextAction(float animCOmpleteTime, float delayTime)
    {
        yield return new WaitForSeconds(animCOmpleteTime);
         Debug.Log("<color=white>next action roll idle switch</color>");
        npcRoot.statemachine.SwitchState(idleState);

        yield return new WaitForSeconds(delayTime);
        Debug.Log("<color=green>next action roll delay</color>");
        RollForStrafeAround();
    }

}


    




[Serializable]

public class Attack
{
    public AnimationClip attackAnimation;
    public float attackChance;

    public float attackDelay;

    //public bool instantBackstep = false;    
    //public float uniqueDamagePerHit = 0f;    

}
