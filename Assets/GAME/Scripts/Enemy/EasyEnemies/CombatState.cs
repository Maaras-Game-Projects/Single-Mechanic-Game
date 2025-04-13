using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
    public bool inStrafing = false;
    public float combatRadius_Modified;
    [SerializeField] public bool inCombatRadius = false;

    [SerializeField] private ChaseState chaseState;
    [SerializeField] private IdleState idleState;

    [Space]
    [Header("Strafe Variables")]
    [Space]

    [SerializeField] private float strafeChance = 40f;
    [SerializeField] private float maxStrafeDuration_Left = 1f;
    [SerializeField] private float maxStrafeDuration_Right = 1f;
    [SerializeField] private float maxStrafeDuration_Back = 1f;
    [SerializeField] private float maxStrafeDuration_Front = 1f;
    [SerializeField] private float maxTotalStrafe_Duration = 3f;

    [SerializeField] private float strafeSpeed = .5f;
    [SerializeField] private bool attackWhileStrafe_LongRange = false;
    [SerializeField] private bool attackWhileStrafe_CloseRange = false;

    private Coroutine attackDelayCoroutine = null;
    private Coroutine actionAfterAttackCoroutine = null;
    private Coroutine rollForStrafeCoroutine = null;
    private Coroutine strafeAroundCoroutine = null;

    [Space]
    [Header("Debugging")]
    [Space]

    [SerializeField] private string strafeDirectionStatus = "none"; // debug var
    [SerializeField] private direction currentStrafeDirection ; // debug var
    [SerializeField] private float currentStrafeDuration; // debug var

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
        // chaseState.navMeshAgent.isStopped = true;
        // chaseState.navMeshAgent.velocity = Vector3.zero;
        // npcRoot.rigidBody.linearVelocity = Vector3.zero;
    }

    public override void TickLogic()
    {
        npcRoot.LookAtPlayer(); //look at player
        
        if(npcRoot.isInteracting) return; //if root motion is being used, do not perform any other actions

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
        //Debug.Log("<color=red>no attack available, set to Idle</color>");

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
            //roll for strafe chance

            //if(inStrafing) return; //if already strafing, do not perform any other actions
            
            //strafeAroundCoroutine = StartCoroutine(StrafeAround());

        }
        else
        {
            //go to idle state
            npcRoot.statemachine.SwitchState(idleState);
        }
    }

    // IEnumerator StrafeAround()
    // {
    //     inStrafing = true; // set the inStrafing flag to true
    //     npcRoot.isStrafing = inStrafing;
    //     Debug.Log("<color=green>Strafe Begin</color>");
    //     float elapsedTime = 0f;
        
    //     var navAgentAngSpeed = chaseState.navMeshAgent.angularSpeed; // store the original angular speed
                
    //     chaseState.navMeshAgent.speed = strafeSpeed; // set the nav mesh agent speed to the strafe speed
    //     while (elapsedTime < maxTotalStrafe_Duration)
    //     {
    //         //Get a random direction to strafe in
    //         Vector3 strafeDirection = GetRandomStrafeDirection();

            

    //         //Get a random duration for the strafe action based on the max duration provided.
    //         float strafeDuration = GetRandomStrafeDuration(strafeDirection);

    //         strafeDuration = ModifyStrafeDurationBasedOnRemainingTotalDuration(elapsedTime, strafeDuration);

    //         //debug
    //         currentStrafeDuration = strafeDuration; 

    //         float elapsedStrafeTime = 0f;
    //         float strafeDistance = strafeSpeed * strafeDuration; 

    //         Vector3 strafeTargetPosition;
    //         Ray ray = new Ray(npcRoot.transform.position + Vector3.up * .25f, strafeDirection);

    //         RaycastHit hit;

    //         if (Physics.Raycast(ray, out hit, strafeDistance, npcRoot.obstacleLayerMask))
    //         {
    //             strafeTargetPosition = hit.point - strafeDirection.normalized * 1f; // set the target position to be the hit point minus the strafe direction
    //         }
    //         else
    //         {
    //             strafeTargetPosition = npcRoot.transform.position + strafeDirection * strafeDistance; // set the target position to be the hit point plus the strafe direction
    //         }



    //         NavMeshHit navMeshHit;
    //         if(NavMesh.SamplePosition(strafeTargetPosition, out navMeshHit, 1f, NavMesh.AllAreas))
    //         {
    //             strafeTargetPosition = navMeshHit.position;
    //             chaseState.navMeshAgent.isStopped = false;
    //             chaseState.navMeshAgent.angularSpeed = 0f; // set the angular speed to 0 to prevent rotation during strafe
    //             chaseState.navMeshAgent.SetDestination(strafeTargetPosition); // set the destination to be the target position
    //             //npcRoot.SetStrafeAnimatorValues(currentStrafeDirection); // set the strafe animator values to be the strafe direction
    //         }
    //         else
    //         {
    //             yield return null; // if the target position is not valid, wait for the next frame
    //             continue; // continue to the next iteration of the loop
    //         }
        
    //         while (elapsedStrafeTime < strafeDuration)
    //         {
    //             npcRoot.SetStrafeAnimatorValues(currentStrafeDirection);
    //             elapsedStrafeTime += Time.deltaTime; // increment the elapsed strafe time
    //             elapsedTime += Time.deltaTime; // increment the elapsed time

    //             yield return null;
    //             Debug.Log("<color=blue>Strafing</color>");

    //         }

    //         yield return null; // wait for the next frame
    //     }

    //     chaseState.navMeshAgent.isStopped = true; // stop the nav mesh agent after the strafe duration is complete
    //     chaseState.navMeshAgent.velocity = Vector3.zero; // set the velocity to zero after the strafe duration is complete
    //     chaseState.navMeshAgent.angularSpeed = navAgentAngSpeed; // reset the angular speed to the original value
    //     Debug.Log("<color=cyan>Strafe Stopped</color>");

    //     inStrafing = false; // set the inStrafing flag to false
    //     npcRoot.isStrafing = inStrafing;


    //     //alternate code

    //     // Debug.Log("<color=green>Strafe Begin</color>");

    //     // inStrafing = true; // set the inStrafing flag to true

       
    //     // float elapsedTime = 0f;

    //     // while (elapsedTime < maxTotalStrafe_Duration)
    //     // {
    //     //     Vector3 strafeDirection = GetRandomStrafeDirection();
    //     //     float strafeDuration = GetRandomStrafeDuration(strafeDirection);
    //     //     strafeDuration = ModifyStrafeDurationBasedOnRemainingTotalDuration(elapsedTime, strafeDuration);

    //     //     float elapsedStrafeTime = 0f;
    //     //     float strafeDistance = strafeSpeed * strafeDuration;

    //     //     Vector3 origin = npcRoot.transform.position + Vector3.up * 0.25f;
    //     //     Ray ray = new Ray(origin, strafeDirection);
    //     //     RaycastHit hit;
    //     //     Vector3 strafeTargetPosition;

    //     //     if (Physics.Raycast(ray, out hit, strafeDistance, npcRoot.obstacleLayerMask))
    //     //     {
    //     //         strafeTargetPosition = hit.point - strafeDirection.normalized * 1f;
    //     //         Debug.Log("<color=red>Obstacle detected. Adjusting target.</color>");
    //     //     }
    //     //     else
    //     //     {
    //     //         strafeTargetPosition = npcRoot.transform.position + strafeDirection * strafeDistance;
    //     //     }

    //     //     if (NavMesh.SamplePosition(strafeTargetPosition, out NavMeshHit navMeshHit, 1f, NavMesh.AllAreas))
    //     //     {
    //     //         strafeTargetPosition = navMeshHit.position;
    //     //         chaseState.navMeshAgent.isStopped = false;
    //     //         chaseState.navMeshAgent.speed = strafeSpeed; // ← important!
    //     //         chaseState.navMeshAgent.SetDestination(strafeTargetPosition);

    //     //         while (elapsedStrafeTime < strafeDuration)
    //     //         {
    //     //             // if (!chaseState.navMeshAgent.pathPending &&
    //     //             //     chaseState.navMeshAgent.remainingDistance <= chaseState.navMeshAgent.stoppingDistance)
    //     //             // {
    //     //             //     Debug.Log("<color=yellow>Reached strafe point early.</color>");
    //     //             //     break;
    //     //             // }

    //     //             elapsedStrafeTime += Time.deltaTime;
    //     //             elapsedTime += Time.deltaTime;

    //     //             Debug.Log("<color=blue>Strafing</color>");
    //     //             yield return null;
    //     //         }
    //     //     }
    //     //     else
    //     //     {
    //     //         Debug.Log("<color=orange>Invalid target on NavMesh. Skipping.</color>");
    //     //         yield return null;
    //     //         continue;
    //     //     }

    //     //     yield return null;
    //     // }

    //     // chaseState.navMeshAgent.isStopped = true;
    //     // chaseState.navMeshAgent.velocity = Vector3.zero;
    //     // Debug.Log("<color=cyan>Strafe Stopped</color>");

    //     // inStrafing = false; // set the inStrafing flag to false
    // }

    private float ModifyStrafeDurationBasedOnRemainingTotalDuration(float elapsedTime, float strafeDuration)
    {
        float remainingTotalStrafeDuration = maxTotalStrafe_Duration - elapsedTime; // calculate the remaining strafe duration
        if (strafeDuration > remainingTotalStrafeDuration)
        {
            if(remainingTotalStrafeDuration < 0.1f)
            {
                return 0f; // if the remaining strafe duration is less than 0.1f, set it to 0f
            }
            
            strafeDuration = remainingTotalStrafeDuration;
            
        }

        float clampedStrafeDuration = Mathf.Clamp(strafeDuration, 0.1f, remainingTotalStrafeDuration); // clamp the strafe duration to be within 0.1f and max total strafe duration
        return clampedStrafeDuration;
    }

    private Vector3 GetRandomStrafeDirection()
    {
        float randomValue = UnityEngine.Random.Range(0, 4);

        switch (randomValue)
        {
            case 0:
                //debug
                strafeDirectionStatus = "right"; 
                currentStrafeDirection = direction.right;
                return npcRoot.transform.right; // strafe right
            case 1:
                //debug
                strafeDirectionStatus = "left"; 
                currentStrafeDirection = direction.left;
                return -npcRoot.transform.right; // strafe left
            case 2:
                //debug
                strafeDirectionStatus = "front"; 
                currentStrafeDirection = direction.front;
                return npcRoot.transform.forward; // strafe forward
            case 3:
                //debug
                strafeDirectionStatus = "back"; 
                currentStrafeDirection = direction.back;
                return -npcRoot.transform.forward; // strafe backward
            default:
                //debug
                strafeDirectionStatus = "back";
                currentStrafeDirection = direction.back; 
                return -npcRoot.transform.forward; // default to backward
        }
    }

    private float GetRandomStrafeDuration(Vector3 direction)
    {

        if(direction == npcRoot.transform.right) // strafe right
        {
            return GetRandomDuration(maxStrafeDuration_Right);
        }
        else if (direction == -npcRoot.transform.right) // strafe left
        {
            return GetRandomDuration(maxStrafeDuration_Left);
        }
        else if (direction == npcRoot.transform.forward) // strafe forward
        {
            return GetRandomDuration(maxStrafeDuration_Front);
        }
        else if (direction == -npcRoot.transform.forward) // strafe backward
        {
            return GetRandomDuration(maxStrafeDuration_Back);
        }

        return 0f; // default case, should not happen
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

    /// <summary>
    /// Get a random duration for the strafe action based on the max duration provided.
    /// The duration is randomly selected between 0,0.25,0.5,... and to the max duration provided.
    /// </summary>
    /// <param name="maxDuration">max duration is capped at 5</param>

    private float GetRandomDuration(float maxDuration) 
    {
        if(maxDuration > 10) maxDuration = 10f; // set the max duration to be within 5f
        if(maxDuration < 3) maxDuration = 3f; 

        // float[] durations = {0,0.25f,0.5f,0.75f,1f,1.25f,1.5f,1.75f,2f,2.25f,2.5f,2.75f,3f,3.25f,
        //                         3.5f,3.75f,4f,4.25f,4.5f,4.75f,5f};

        float[] durations = {3f,4f,5f,6f,7f,8f,9f,10f};
        
        List<float> filteredDurations = new List<float>();

        for(int i = 0; i < durations.Length; i++)
        {
            if(durations[i] <= maxDuration)
            {
                filteredDurations.Add(durations[i]);
            }
        }

        float randomDuration = filteredDurations[UnityEngine.Random.Range(0, filteredDurations.Count)];

        return randomDuration; // return the random duration selected from the filtered list

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

public enum direction{
    left,
    right,
    front,
    back
}
