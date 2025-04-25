using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StrafeState : State
{

    [Space]
    [Header("Strafe Variables")]
    [Space]

    
    [SerializeField] int minimum_StrafeDuration = 2;
    [SerializeField] int maximum_StrafeDuration = 6;

    [SerializeField] private float strafe_duration = 10f;
    [SerializeField] private float elapsedStrafeTime = 0f;
    [SerializeField] private bool rolledForStrafeResume = false;

    [Range(0,100)]
    [SerializeField] private float weight_ForContinueStrafing = 50f;
    [SerializeField] public direction currenStrafeDirection; 

    [Space(20)]
    [Header("STRAFE DIRECTION CHANCES IN COMBAT ZONES")]
    [Space(20)]


    [Space]
    [Header("Close Range Chance Variables")]
    [Space]

    [SerializeField] StrafeDirectionWeights strafeDirectionWeights_CloseRange;

    [Space]
    [Header("Backoff Range Chance Variables")]
    [Space]

    [SerializeField] StrafeDirectionWeights strafeDirectionWeights_BackOffRange;
    
    [Space]
    [Header("Mid Range Chance Variables")]
    [Space]

    [SerializeField] StrafeDirectionWeights strafeDirectionWeights_MidRange;
    
    [Space]
    [Header("Long Range Chance Variables")]
    [Space]

    [SerializeField] StrafeDirectionWeights strafeDirectionWeights_LongRange;

    

    
    [SerializeField] IdleState idleState;
    [SerializeField] CombatAdvanced_State combatAdvanced_State;

    public override void OnEnter()
    {
        npcRoot.isStrafing = true;
        elapsedStrafeTime = 0f;
        
        rolledForStrafeResume = false;
        idleState.GoToLocomotionAnimation();

    
        strafe_duration = RollForMaximumStrafeDuration();
        DetermineStrafeDirection_ByCombatZone(combatAdvanced_State.CurrentCombatZone);
        currenStrafeDirection = CheckForObstacleInCurrentOrOppositeDirection(currenStrafeDirection);
        npcRoot.SetStrafeAnimatorValues(currenStrafeDirection);
    }

    public override void OnExit()
    {
        rolledForStrafeResume = false;
        npcRoot.isStrafing = false;
    }

    public override void TickLogic()
    {
        //npcRoot.statemachine.SwitchState(idleState);
        Strafe(strafe_duration,currenStrafeDirection);
    }

    private void Strafe(float duration, direction direction)
    {
        if(npcRoot.isPlayerInLineOfSight())
        {
            npcRoot.LookAtPlayer(1.5f);  
            //npcRoot.TurnCharacter();
           
        }
        else
        {
            npcRoot.statemachine.SwitchState(combatAdvanced_State);
        }

        npcRoot.SetStrafeAnimatorValues(direction);

        elapsedStrafeTime += Time.deltaTime;

        CheckForObstacleInCurrentOrOppositeDirection(direction);

        if(elapsedStrafeTime < maximum_StrafeDuration)
        {
            if (elapsedStrafeTime >= minimum_StrafeDuration)
            {
                if(!rolledForStrafeResume) //if strafed for minimum duration, roll to decide whether to continue strafing or go to combat
                {
                    float randomWeight = UnityEngine.Random.Range(0f,100f);

                    if(randomWeight > weight_ForContinueStrafing) // if greater than continueStrafe weight, roll for offensive combat strats or continue strafing 
                    {
                        //roll for offensive strats in combat state
                        //if picked attack doesnt have higher stamina cost than current stamina, continue strafing or go to combat stat
                        Debug.Log("<color=red>Strafe Interrupted</color>");
                        npcRoot.statemachine.SwitchState(combatAdvanced_State);
                    }
                    else
                    {
                        Debug.Log("<color=green>Strafe Resumed</color>");
                    }

                    rolledForStrafeResume = true;

                }
                

                
            }
        }
        else
        {
            npcRoot.statemachine.SwitchState(combatAdvanced_State);
        }

        // if (elapsedStrafeTime >= strafe_duration)
        // {
        //     // Go To Idle Animation after circling or Call DecideStrategy() to decide next action
        //     npcRoot.statemachine.SwitchState(combatAdvanced_State); // Go to idle animation after circling 
        // }


    }

    private int RollForMaximumStrafeDuration()
    {
        int min_Value = minimum_StrafeDuration + 1;
        int max_Value = maximum_StrafeDuration + 1;

        int randomMaxStrafeDuration = UnityEngine.Random.Range(min_Value,max_Value);

        return randomMaxStrafeDuration;
    }

    private void DetermineStrafeDirection_ByCombatZone(CombatZone combatZone)
    {
        if(combatZone == CombatZone.Close_Range)
        {
            currenStrafeDirection = RollAndGetStrafeDirection_CloseRange();
        }
        else if(combatZone == CombatZone.Backoff_Range)
        {
            currenStrafeDirection = RollAndGetStrafeDirection_BackOffRange();
        }
        else if(combatZone == CombatZone.Mid_Range)
        {
            currenStrafeDirection = RollAndGetStrafeDirection_MidRange();
        }
        else if(combatZone == CombatZone.Long_Range)
        {
            currenStrafeDirection = RollAndGetStrafeDirection_LongRange();
        }

    }

    private direction RollAndGetStrafeDirection_CloseRange()
    {
        Dictionary<direction,float> strafeDirectionWeightPair_CloseRange = new Dictionary<direction, float>
        {
            {direction.left,strafeDirectionWeights_CloseRange.left},
            {direction.right,strafeDirectionWeights_CloseRange.right},
            {direction.front,strafeDirectionWeights_CloseRange.forward},
            {direction.back,strafeDirectionWeights_CloseRange.backward},
        };

        float totalChance =  strafeDirectionWeightPair_CloseRange.Values.Sum();

        float randomValue = UnityEngine.Random.Range(0.1f,totalChance);

        foreach (var  pair in strafeDirectionWeightPair_CloseRange)
        {
            direction direction = pair.Key;
            float weight = pair.Value;
            if (randomValue <= weight)
            {
                return direction;
            }

            randomValue -= weight;
        }

        return direction.left;
    }

    private direction RollAndGetStrafeDirection_BackOffRange()
    {
        Dictionary<direction,float> strafeDirectionWeightPair_BackOffRange = new Dictionary<direction, float>
        {
            {direction.left,strafeDirectionWeights_BackOffRange.left},
            {direction.right,strafeDirectionWeights_BackOffRange.right},
            {direction.front,strafeDirectionWeights_BackOffRange.forward},
            {direction.back,strafeDirectionWeights_BackOffRange.backward},
        };

        float totalChance =  strafeDirectionWeightPair_BackOffRange.Values.Sum();

        float randomValue = UnityEngine.Random.Range(0.1f,totalChance);

        foreach (var  pair in strafeDirectionWeightPair_BackOffRange)
        {
            direction direction = pair.Key;
            float weight = pair.Value;
            if (randomValue <= weight)
            {
                return direction;
            }

            randomValue -= weight;
        }

        return direction.left;
    }

    private direction RollAndGetStrafeDirection_MidRange()
    {
        Dictionary<direction,float> strafeDirectionWeightPair_MidRange = new Dictionary<direction, float>
        {
            {direction.left,strafeDirectionWeights_MidRange.left},
            {direction.right,strafeDirectionWeights_MidRange.right},
            {direction.front,strafeDirectionWeights_MidRange.forward},
            {direction.back,strafeDirectionWeights_MidRange.backward},
        };

        float totalChance =  strafeDirectionWeightPair_MidRange.Values.Sum();

        float randomValue = UnityEngine.Random.Range(0.1f,totalChance);

        foreach (var  pair in strafeDirectionWeightPair_MidRange)
        {
            direction direction = pair.Key;
            float weight = pair.Value;
            if (randomValue <= weight)
            {
                return direction;
            }

            randomValue -= weight;
        }

        return direction.left;
    }

    private direction RollAndGetStrafeDirection_LongRange()
    {
        Dictionary<direction,float> strafeDirectionWeightPair_LongRange = new Dictionary<direction, float>
        {
            {direction.left,strafeDirectionWeights_LongRange.left},
            {direction.right,strafeDirectionWeights_LongRange.right},
            {direction.front,strafeDirectionWeights_LongRange.forward},
            {direction.back,strafeDirectionWeights_LongRange.backward},
        };

        float totalChance =  strafeDirectionWeightPair_LongRange.Values.Sum();

        float randomValue = UnityEngine.Random.Range(0.1f,totalChance);

        foreach (var  pair in strafeDirectionWeightPair_LongRange)
        {
            direction direction = pair.Key;
            float weight = pair.Value;
            if (randomValue <= weight)
            {
                return direction;
            }

            randomValue -= weight;
        }

        return direction.left;
    }



    private direction CheckForObstacleInCurrentOrOppositeDirection(direction direction)
    {
        Vector3 rayDirection = -npcRoot.transform.right;
        rayDirection = Determine_StrafeObstacleDetection_RaycastDirection(direction, rayDirection);

        Ray ray = new Ray(npcRoot.transform.position, rayDirection);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2f, npcRoot.obstacleLayerMask))
        {
            
            //Obstacle detected,so change strafe direction to opposite direction
            direction = Determine_StrafeOppositeDirection(direction);

            currenStrafeDirection = direction; // Update the current strafe direction

            //check obstacle in opposite direction
            Ray ray2 = new Ray(npcRoot.transform.position, rayDirection * -1f);

            RaycastHit hit2;
            if (Physics.Raycast(ray2, out hit2, 2f, npcRoot.obstacleLayerMask))
            {
                
                //Obstacle detected in opposite direction, so stop circling
                npcRoot.isStrafing = false;
                npcRoot.statemachine.SwitchState(combatAdvanced_State); // Go to idle animation after circling 
                //idleState.GoToIdleAnimation(); // Go to idle animation after circling
            }
            else
            {
                npcRoot.SetStrafeAnimatorValues(direction);
            }
        }

        return direction;
    }

   


    private  direction Determine_StrafeOppositeDirection(direction direction)
    {
        if (direction == direction.front)
        {
            direction = direction.back;
        }
        else if (direction == direction.back)
        {
            direction = direction.front;
        }
        else if (direction == direction.left)
        {
            direction = direction.right;
        }
        else if (direction == direction.right)
        {
            direction = direction.left;
        }

        return direction;
    }

    private Vector3 Determine_StrafeObstacleDetection_RaycastDirection(direction direction, Vector3 rayDirection)
    {
        if (direction == direction.front)
        {
            rayDirection = npcRoot.transform.forward;
        }
        else if (direction == direction.back)
        {
            rayDirection = -npcRoot.transform.forward;
        }
        else if (direction == direction.left)
        {
            rayDirection = -npcRoot.transform.right;
        }
        else if (direction == direction.right)
        {
            rayDirection = npcRoot.transform.right;
        }

        return rayDirection;
    }
}


[Serializable]
public class StrafeDirectionWeights
{
    public float left = 50f;
    public float right = 10f;
    public float forward = 10f;
    public float backward = 10f;
}
