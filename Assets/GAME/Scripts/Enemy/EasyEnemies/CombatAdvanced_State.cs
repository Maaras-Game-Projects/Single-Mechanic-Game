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
    [SerializeField] bool canCircleStrafe = false;
    [SerializeField] bool canStrafe = false;

    [SerializeField] private float strafeSpeed = .5f;

    [Space]
    [Header("Debug Variables")]
    [Space]

    [SerializeField] private bool circlestrafe_ClockWise = true;
    [SerializeField] private float circlestrafe_duration = 10f;
    [SerializeField] private float strafe_duration = 10f;
    [SerializeField] public direction currenStrafeDirection; //imp variable not just for debugging

    void Awake()
    {
        combatRadius_Modified = combatRadius - .2f; // set the modified combat radius to be slightly smaller than the original combat radius
    }

    public override void OnEnter()
    {

    }

    public override void OnExit()
    {
        
    }

    public override void TickLogic()
    {
        npcRoot.LookAtPlayer();
        // if(!canCircleStrafe) return;
        // PerformCircleStrafe(circlestrafe_duration, circlestrafe_ClockWise); // Example usage of circle strafe

        if(!canStrafe) return;
        PerformStrafe(strafe_duration,currenStrafeDirection); // Example usage of strafe
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

    public bool IsPlayerInCloseRange()
    {
        float distance = Vector3.Distance(npcRoot.transform.position, npcRoot.targetTransform.position);
        if(distance <= max_shortRangeAttackDistance)
        {
            return true;
        }
        return false;
        
    }

    private void PerformStrafe(float duration, direction direction)
    {
        
        if(npcRoot.isStrafing) return; // Prevent multiple strafing at the same time
        
        idleState.GoToLocomotionAnimation(); // Go to locomotion animation before strafing
        StartCoroutine(Strafe(duration, direction));

    }
    
    private void PerformCircleStrafe(float duration, bool isClockwise)
    {
        
        
        if(npcRoot.isCircleStrafing) return; // Prevent multiple circle strafing at the same time
        
        idleState.GoToLocomotionAnimation(); // Go to locomotion animation before circling
        StartCoroutine(CircleStrafe(duration, isClockwise));

    }

    IEnumerator Strafe(float duration, direction direction)
    {
        
        npcRoot.isStrafing = true; // Set the flag to indicate that the NPC is circling
        
        float elapsedTime = 0f;

        //strafeAnimClip_Back.

        //npcRoot.currentStrafeDirection = direction; // Set the current strafe direction

        while (elapsedTime < duration)
        {
            Vector3 rayDirection = -npcRoot.transform.right;

            if(direction == direction.front)
            {
                rayDirection = npcRoot.transform.forward;
            }
            else if(direction == direction.back)
            {
                rayDirection = -npcRoot.transform.forward;
            }
            else if(direction == direction.left)
            {
                rayDirection = -npcRoot.transform.right;
            }
            else if(direction == direction.right)
            {
                rayDirection = npcRoot.transform.right;
            }

            Ray ray = new Ray(npcRoot.transform.position, rayDirection);

            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * 2f, Color.green); // Visualize the ray in the scene view

            if(Physics.Raycast(ray, out hit, 2f, npcRoot.obstacleLayerMask))
            {
                Debug.Log("<color=red>Obstacle detected in strafe direction: </color>" + hit.collider.gameObject.name);
               
                //Obstacle detected,so change strafe direction to opposite direction
                if(direction == direction.front)
                {
                    direction = direction.back;
                }
                else if(direction == direction.back)
                {
                    direction = direction.front;
                }
                else if(direction == direction.left)
                {
                    direction = direction.right;
                }
                else if(direction == direction.right)
                {
                    direction = direction.left;
                }

                currenStrafeDirection = direction; // Update the current strafe direction
                
                Debug.Log("<color=red>Changing direction to </color>" + direction.ToString());
                Debug.DrawRay(ray.origin, ray.direction * -2f, Color.yellow); // Visualize the ray in the scene view

                //check obstacle in opposite direction
                Ray ray2 = new Ray(npcRoot.transform.position, rayDirection * -1f);

                RaycastHit hit2;
                if(Physics.Raycast(ray2, out hit2, 2f, npcRoot.obstacleLayerMask))
                {
                    Debug.Log("<color=yellow>Obstacle detected in opposite direction: </color>" + hit.collider.gameObject.name);
                    //Obstacle detected in opposite direction, so stop circling
                    npcRoot.isStrafing = false; 
                    //npcRoot.statemachine.SwitchState(idleState); // Go to idle animation after circling 
                    idleState.GoToIdleAnimation(); // Go to idle animation after circling
                    break;
                }
            }
            npcRoot.SetStrafeAnimatorValues(direction);
           
            elapsedTime += Time.deltaTime;
            
            yield return null; // Wait for the next frame
            
        }

         npcRoot.isStrafing = false; 

        // Go To Idle Animation after circling or Call DecideStrategy() to decide next action
        //npcRoot.statemachine.SwitchState(idleState); // Go to idle animation after circling 

        
    }



    //This Strategy is eligible only when the player within Strafe Distance.
    //CircleStrafe around the player for a certain amount of time.
    //If obstacle present in tangent direction, then strafe in the 
    //opposite direction if move space is available until duration runs out,
    //if move space is not available, then stop  circlestrafe.
    //Must Strafe within Strafe Radius
    IEnumerator CircleStrafe(float duration, bool isClockwise)
    {
        
        npcRoot.isCircleStrafing = true; // Set the flag to indicate that the NPC is circling
        
        float elapsedTime = 0f;


        while (elapsedTime < duration)
        {
            Vector3 directionToTarget = npcRoot.targetTransform.position - transform.position;
            directionToTarget.y = 0f; // Ignore vertical distance
            Vector3 tangentDirection_CW = Vector3.Cross(directionToTarget, npcRoot.transform.up).normalized;
            Vector3 tangentDirection_CCW = Vector3.Cross(npcRoot.transform.up, directionToTarget).normalized;
            
            direction strafeDirection = isClockwise ? direction.left : direction.right;

            // if(isClockwise)
            // {
            //     dire
            // }
            npcRoot.SetStrafeAnimatorValues(strafeDirection);
           
            elapsedTime += Time.deltaTime;
            
            yield return null; // Wait for the next frame
            
        }

        npcRoot.isCircleStrafing = false;

        // Go To Idle Animation after circling or Call DecideStrategy() to decide next action
        //npcRoot.statemachine.SwitchState(idleState); // Go to idle animation after circling 

        
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
