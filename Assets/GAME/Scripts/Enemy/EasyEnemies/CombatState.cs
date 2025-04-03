using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatState : State
{
    public bool canCirculate = false;
    public bool instantAttack = false;

    public List<Attack> longRangeAttacks = new List<Attack>();
    public List<Attack> closeRangeAttacks = new List<Attack>();

    [Tooltip("Distance to attack player in Long range, the value must be within combat radius")]
    public float min_longRangeAttackDistance = 3f;

    [Tooltip("Distance to attack player in Short range, the value must be within combat radius")]
    public float max_shortRangeAttackDistance = 1.5f;


    public float combatRadius = 5f;

    [SerializeField] private ChaseState chaseState;

    public override void OnEnter()
    {
       //chaseState.navMeshAgent.enabled = false;
    }
    public override void OnExit()
    {
        
    }

    public override void TickLogic()
    {
        npcRoot.LookAtPlayer(); //look at player

        if(npcRoot.npc_RootMotionUseStatus) return; //if root motion is being used, do not perform any other actions

        float distanceToTarget = Vector3.Distance(npcRoot.transform.position, npcRoot.targetTransform.position);

        //If long range attack is availabe, check if player is in long range attack distance and perform long range attack
        if(longRangeAttacks.Count > 0)
        {
            
            if(distanceToTarget >= min_longRangeAttackDistance)
            {
                string attackAnimationToPlay = RollAndGetAttacks(longRangeAttacks);
                if(attackAnimationToPlay != null)
                {
                    npcRoot.PlayAnyActionAnimation(attackAnimationToPlay, true);
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
                string attackAnimationToPlay = RollAndGetAttacks(closeRangeAttacks);
                if(attackAnimationToPlay != null)
                {
                    npcRoot.PlayAnyActionAnimation(attackAnimationToPlay, true);
                    // enable attack delay if present
                    // trigger strafe state if present or go to idle state
                    return;
                }

            }
        }

        //If no attack is available, or no attack is possible, trigger strafe state if present or
    }

   
    private string RollAndGetAttacks(List<Attack> attackList)
    {
        string attackAnimationToPlay = null;
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
                attackAnimationToPlay = attack.attackAnimation.name;
                return attackAnimationToPlay;
            }

            randomValue -= attack.attackChance;
        }

        return attackAnimationToPlay;
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

    


}

[Serializable]

public class Attack
{
    public AnimationClip attackAnimation;
    public float attackChance;
}
