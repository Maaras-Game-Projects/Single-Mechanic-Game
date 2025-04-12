using UnityEngine;

public class StrafeState : State
{

    [Space]
    [Header("Strafe Variables")]
    [Space]

    [SerializeField] private float strafeChance = 40f;
    [SerializeField] private float maxStrafeDuration_Left = 2f;
    [SerializeField] private float maxStrafeDuration_Right = 2f;
    [SerializeField] private float maxStrafeDuration_Back = 2f;
    [SerializeField] private float maxStrafeDuration_Front = 2f;
    [SerializeField] private float strafe_duration = 10f;
    [SerializeField] private float elapsedStrafeTime = 0f;
    [SerializeField] public direction currenStrafeDirection; 


    
    [SerializeField] IdleState idleState;
    [SerializeField] CombatAdvanced_State combatAdvanced_State;

    public override void OnEnter()
    {
        npcRoot.isStrafing = true;
        elapsedStrafeTime = 0f;
        
        idleState.GoToLocomotionAnimation();

        //Need to add method here to determine random strafe direction
        //Need to add method here to determine random strafe duration 

        currenStrafeDirection = CheckForObstacleInCurrentOrOppositeDirection(currenStrafeDirection);
        npcRoot.SetStrafeAnimatorValues(currenStrafeDirection);
    }

    public override void OnExit()
    {
        npcRoot.isStrafing = false;
    }

    public override void TickLogic()
    {
        Strafe(strafe_duration,currenStrafeDirection);
    }

    public void Strafe(float duration, direction direction)
    {
        npcRoot.LookAtPlayer();

        elapsedStrafeTime += Time.deltaTime;

        CheckForObstacleInCurrentOrOppositeDirection(direction);

        if (elapsedStrafeTime >= strafe_duration)
        {
            // Go To Idle Animation after circling or Call DecideStrategy() to decide next action
            npcRoot.statemachine.SwitchState(idleState); // Go to idle animation after circling 
        }


    }

    private direction CheckForObstacleInCurrentOrOppositeDirection(direction direction)
    {
        Vector3 rayDirection = -npcRoot.transform.right;
        rayDirection = Determine_StrafeObstacleDetection_RaycastDirection(direction, rayDirection);

        Ray ray = new Ray(npcRoot.transform.position, rayDirection);

        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 2f, Color.green); // Visualize the ray in the scene view

        if (Physics.Raycast(ray, out hit, 2f, npcRoot.obstacleLayerMask))
        {
            Debug.Log("<color=red>Obstacle detected in strafe direction: </color>" + hit.collider.gameObject.name);

            //Obstacle detected,so change strafe direction to opposite direction
            direction = Determine_StrafeOppositeDirection(direction);

            currenStrafeDirection = direction; // Update the current strafe direction

            Debug.Log("<color=red>Changing direction to </color>" + direction.ToString());
            Debug.DrawRay(ray.origin, ray.direction * -2f, Color.yellow); // Visualize the ray in the scene view

            //check obstacle in opposite direction
            Ray ray2 = new Ray(npcRoot.transform.position, rayDirection * -1f);

            RaycastHit hit2;
            if (Physics.Raycast(ray2, out hit2, 2f, npcRoot.obstacleLayerMask))
            {
                Debug.Log("<color=yellow>Obstacle detected in opposite direction: </color>" + hit.collider.gameObject.name);
                //Obstacle detected in opposite direction, so stop circling
                npcRoot.isStrafing = false;
                npcRoot.statemachine.SwitchState(idleState); // Go to idle animation after circling 
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
