
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLocomotion : MonoBehaviour
{
    [SerializeField] private MyInputManager myInputManager;
    [SerializeField] private PlayerAnimationManager playerAnimationManager;
    [SerializeField] private PlayerCombat playerCombat;

    [SerializeField] public Rigidbody playerRigidBody;
    [SerializeField] public Camera mainCamera;
    [SerializeField] public CinemachineCamera mainCinemachineCamera;
    [SerializeField] public CinemachineCamera lockOnCamera;
    [SerializeField] public BaseEnemy lockOnTarget;
    [SerializeField] public BaseEnemy lockOnTarget_Left;
    [SerializeField] public BaseEnemy lockOnTarget_Right;
    [SerializeField] public float maxLockOnDistance = 10f;
    [SerializeField] public Image lockOnImage;
    [SerializeField]float playerFOV = 90f;

    [SerializeField] private float lockONDetectionRadius = 3f;
    [SerializeField] List<BaseEnemy> enemiesWithinFOV = new List<BaseEnemy>();


    [SerializeField] Vector3 moveDirection;
    [SerializeField] private Vector3 playerVelocity;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Vector3 targetDirection = Vector3.zero;
    [SerializeField] private Quaternion targetRotation;
    [SerializeField] private Quaternion playerRotation;
    [SerializeField] public bool isWalking = false;
    [SerializeField] public bool canMove = true;
    [SerializeField] public bool canRotate = true;

     [SerializeField] public bool isDodging = true;

    [Space]
    [Header("Falling and Landing Variables")]
    [Space]
    [SerializeField] public bool isGrounded;
    [SerializeField] float inAirTimer = 0f;
    [SerializeField] float leapingVelocity;
    [SerializeField] float fallingVelocity;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundRaycastOffset = 0.5f;
    [SerializeField] float maxGroundCheckDistance = 2.5f;

    [Space]
    [Header("Jump Variables")]
    [Space]
   
    [SerializeField] public bool isJumping = false;
    [SerializeField] public bool isLockedOnTarget = false;
    [SerializeField] private float gravityIntensity;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpForce;
   

    void Update()
    {
        
    }

    void LateUpdate()
    {
        if(isLockedOnTarget && lockOnTarget != null)
        {
            EnableLockOnImage();
        }
        else
        {
            DisableLockOnImage();
        }
    }

    public void HandleAllMovement()
    {
        HandleFallingAndLanding();

        if (playerAnimationManager.inAnimActionStatus)
            return;

        HandleMovement();
        HandleRotation();
        //HandleJump();

    }

    private void HandleMovement()
    {
        
        if (!canMove) return;

        
        moveDirection = mainCamera.transform.forward * myInputManager.verticalMovementInput;
        moveDirection = moveDirection + mainCamera.transform.right * myInputManager.horizontalMovementInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

        if(isWalking) //if is walking, move in walkspeed or else in default speed
        {
            playerVelocity = moveDirection * walkSpeed;
        }
        else
        {
            playerVelocity = moveDirection * movementSpeed;
        }

        playerRigidBody.linearVelocity = playerVelocity;
    }

    private void HandleRotation()
    {
        

        if(isLockedOnTarget)
        {
            if(isDodging)
            {
               
                Debug.Log("rotation on dodge");
                targetDirection = mainCamera.transform.forward * myInputManager.verticalMovementInput;
                targetDirection = targetDirection + mainCamera.transform.right * myInputManager.horizontalMovementInput;
                targetDirection.Normalize();
                targetDirection.y = 0;

                if(targetDirection == Vector3.zero)
                {
                    targetDirection = transform.forward;
                }

                targetRotation = Quaternion.LookRotation(targetDirection);
                playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                transform.rotation = playerRotation;
                 
                
            }
            else
            {
                if(lockOnTarget==null) return;

                Vector3 targetDirection = lockOnTarget.transform.position - transform.position;
                targetDirection.y = 0;
                targetDirection.Normalize();

                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            }
            
        }
        else
        {
            if (!canRotate) return;

            targetDirection = Vector3.zero;

            targetDirection = mainCamera.transform.forward * myInputManager.verticalMovementInput;
            targetDirection = targetDirection + mainCamera.transform.right * myInputManager.horizontalMovementInput;
            targetDirection.Normalize();
            targetDirection.y = 0;

            if(targetDirection == Vector3.zero)
            {
                targetDirection = transform.forward;
            }

            targetRotation = Quaternion.LookRotation(targetDirection);
            playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            transform.rotation = playerRotation;

        }
        
    }

    public void HandleJump()
    {
        if (playerAnimationManager.inAnimActionStatus) return;

        // if (isGrounded)
        // {
        //     playerAnimationManager.playerAnimator.SetBool("isJumping", true);
        //     playerAnimationManager.PlayAnyInteractiveAnimation("Jump 1", false);

        //     jumpForce = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
        //     Vector3 jumpVelocity = moveDirection;
        //     //playerVelocity = moveDirection;
        //     jumpVelocity.y = jumpForce;
        //     Debug.Log("jumpForce = " + jumpForce);
        //     Debug.Log("jumpVelocity = " + jumpVelocity);
        //     playerRigidBody.linearVelocity = jumpVelocity;
        //     Debug.Log("linearVelocity of player = " + playerRigidBody.linearVelocity);

        // }

        if (isGrounded)
        {
            // Trigger the jump animation
            playerAnimationManager.playerAnimator.SetBool("isJumping", true);
            playerAnimationManager.PlayAnyInteractiveAnimation("Jump 1", false);

            // Calculate the jump force required
            jumpForce = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);

            // Apply the jump force using AddForce
            Vector3 jumpDirection = moveDirection; // Keep existing horizontal movement
            jumpDirection.y = jumpForce; // Add vertical force for jumping

            Debug.Log("jumpForce = " + jumpForce);
            Debug.Log("jumpDirection before force = " + jumpDirection);

            // Apply the force to the Rigidbody
            playerRigidBody.AddForce(jumpDirection, ForceMode.VelocityChange);

            Debug.Log("linearVelocity of player after force = " + playerRigidBody.linearVelocity);

            

        }

    }

    IEnumerator FallAfterJump(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        inAirTimer += Time.deltaTime;
        playerRigidBody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
    }

    public void FallAfterJump()
    {
       
        /*inAirTimer += Time.deltaTime;
        playerRigidBody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
        Debug.Log("Fall After Jump");*/
    }

    private void HandleFallingAndLanding()
    {
        RaycastHit hit;
        Vector3 raycastOrigin = transform.position;
        raycastOrigin.y = raycastOrigin.y + groundRaycastOffset;

        Vector3 playerTargetPosition = transform.position;

        //Debug.DrawLine(raycastOrigin, raycastOrigin + Vector3.down * groundRaycastOffset, Color.cyan);
       // Debug.DrawRay(raycastOrigin, Vector3.down * groundRaycastOffset, Color.red);

        if (!isGrounded)
        {
            if (!isJumping)
            {
                if (!playerAnimationManager.inAnimActionStatus)
                {
                    playerAnimationManager.PlayAnyInteractiveAnimation("Fall", true);
                }

                playerAnimationManager.playerAnimator.SetBool("isUsingRootMotion", false);
                inAirTimer += Time.deltaTime;

                playerRigidBody.AddForce(transform.forward * leapingVelocity);
                playerRigidBody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
            }

        }


        if (Physics.SphereCast(raycastOrigin, 0.2f, -Vector3.up, out hit, maxGroundCheckDistance, groundLayer))
        {
            if (!isGrounded && playerAnimationManager.inAnimActionStatus)
            {
                //Debug.Log("ground spherecast check to anim");
                playerAnimationManager.PlayAnyInteractiveAnimation("Fall To Landing", true);
            }

            Vector3 rayHitPoint = hit.point;
            playerTargetPosition.y = rayHitPoint.y;
            //Debug.Log("Ground hit: " + hit.collider.name);
            inAirTimer = 0;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        if (isGrounded && !isJumping)
        {
            if (playerAnimationManager.inAnimActionStatus || myInputManager.moveAmount > 0)
            {
                transform.position = Vector3.Lerp(transform.position, playerTargetPosition, Time.deltaTime/0.1f);
            }
            else
            {
                transform.position = playerTargetPosition;
            }
        }
    }

    public void HandleRolling()
    {
        if (isJumping) return;
        if (isDodging) return;

        isDodging = true;
        //if(isLockedOnTarget)  DisableLockON();

        Vector3 rollDirection = mainCamera.transform.forward * myInputManager.verticalMovementInput;
        rollDirection = rollDirection + mainCamera.transform.right * myInputManager.horizontalMovementInput;
        rollDirection.Normalize();
        rollDirection.y = 0;

        playerAnimationManager.PlayAnyInteractiveAnimation("OS_Roll_F", false,true);
    }

    public void HandleTargetLockON()
    {
        if(isLockedOnTarget)
        {
            DisableLockON();
            return;
        }
        else
        {
            isLockedOnTarget = true;
        }

        isLockedOnTarget = true;

        if(enemiesWithinFOV.Count > 0)
            enemiesWithinFOV.Clear();

        Vector3 capusleEndPoint = transform.forward * maxLockOnDistance;

        Collider[] enemyColliders = Physics.OverlapCapsule(transform.position, capusleEndPoint, lockONDetectionRadius,
            playerCombat.enemyLayerMask);
        
        

        if(enemyColliders.Length > 0)
        {
            
            float dotProductThreshold = Mathf.Cos(playerFOV * 0.5f * Mathf.Deg2Rad);

            foreach (var enemyCollider in enemyColliders)
            {
                Vector3 enemyDirection = (enemyCollider.transform.position - transform.position).normalized;
                float dotProduct = Vector3.Dot(transform.forward, enemyDirection);

                if(dotProduct > dotProductThreshold)
                {
                    BaseEnemy enemy = enemyCollider.GetComponent<BaseEnemy>();
                    if(enemy != null)
                    {
                        if(enemy.isDead) continue;
                        enemiesWithinFOV.Add(enemy);
                    }
                    
                }

            }
        }
        else
        {
            isLockedOnTarget = false;
            return;
        }

        if(enemiesWithinFOV.Count > 0)
        {
            BaseEnemy nearestEnemy = null;
            float shortestDistance = Mathf.Infinity;

            foreach (var enemy in enemiesWithinFOV)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if(distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            lockOnTarget = nearestEnemy;
        }
        else
        {
            isLockedOnTarget = false;
            return;
        }


        if(lockOnTarget == null)
        {
            isLockedOnTarget = false;
            return;
        }
        else
        {

            lockOnCamera.LookAt = lockOnTarget.lockOnTransform_Self;
            mainCinemachineCamera.gameObject.SetActive(false);
            lockOnCamera.gameObject.SetActive(true);

            Debug.Log("Locked on");

            //lockOnTarget.EnableEnemyCanvas();
            //EnableLockOnImage();
        }

        //clear enemies within fov list after testing implementation
        //enemiesWithinFOV.Clear();
    }

    private void DisableLockON()
    {
        isLockedOnTarget = false;
        mainCinemachineCamera.gameObject.SetActive(true);
        lockOnCamera.gameObject.SetActive(false);
        DisableLockOnImage();

        lockOnTarget.DisableEnemyCanvas();
        lockOnTarget = null;

        if (enemiesWithinFOV.Count > 0)
            enemiesWithinFOV.Clear();
    }

    void EnableLockOnImage()
    {
        Vector2 lastScreenPos = lockOnImage.transform.position;
        lockOnImage.gameObject.SetActive(true);
        Vector2 targetPos = mainCamera.WorldToScreenPoint(lockOnTarget.lockOnTransform_Self.transform.position);

        if(Vector3.Distance(targetPos,lastScreenPos) > .25f)
        {
            //lockOnImage.transform.position = targetPos;
            lockOnImage.transform.position = 
                            Vector2.Lerp(lockOnImage.transform.position, targetPos, Time.deltaTime * 3f);
            lastScreenPos = targetPos;
        }

        // lockOnImage.transform.position = 
        //         Vector2.Lerp(lockOnImage.transform.position, screenPos, Time.deltaTime * 10);

        //lockOnImage.transform.position = screenPos;
    }

    void DisableLockOnImage()
    {
        lockOnImage.gameObject.SetActive(false);
       
    }

    public void HandleSwitchLeftTarget()
    {
        if(!isLockedOnTarget) return;

        if(lockOnTarget == null) return;

        if(enemiesWithinFOV.Count > 0)
            enemiesWithinFOV.Clear();

        Vector3 capusleEndPoint = transform.forward * maxLockOnDistance;

        Collider[] enemyColliders = Physics.OverlapCapsule(transform.position, capusleEndPoint, lockONDetectionRadius,
            playerCombat.enemyLayerMask);
        
        

        if(enemyColliders.Length > 0)
        {
            
            float dotProductThreshold = Mathf.Cos(playerFOV * 0.5f * Mathf.Deg2Rad);

            foreach (var enemyCollider in enemyColliders)
            {
                Vector3 enemyDirection = (enemyCollider.transform.position - transform.position).normalized;
                float dotProduct = Vector3.Dot(transform.forward, enemyDirection);

                if(dotProduct > dotProductThreshold)
                {
                    BaseEnemy enemy = enemyCollider.GetComponent<BaseEnemy>();
                    if(enemy != null)
                    {
                        if(enemy.isDead) continue;
                        enemiesWithinFOV.Add(enemy);
                    }
                    
                }

            }
        }  

        if(enemiesWithinFOV.Count == 0) return;

 
        float bestLeftScore = Mathf.NegativeInfinity;
        float shortestDistanceFromCurrentTarget = Mathf.Infinity;

        foreach (BaseEnemy potentialTarget in enemiesWithinFOV)
        {
            if(potentialTarget == lockOnTarget) continue;

            Vector3 directionToEnemy = (potentialTarget.transform.position - transform.position).normalized;
           

            float leftScore = Vector3.Dot(transform.right, directionToEnemy); // Negative means left
            float distance = Vector3.Distance(lockOnTarget.transform.position, potentialTarget.transform.position);

            // Select the closest left target
            if (leftScore < 0 && (leftScore > bestLeftScore || (leftScore == bestLeftScore && distance < shortestDistanceFromCurrentTarget)))
            {
                lockOnTarget_Left = potentialTarget;
                bestLeftScore = leftScore;
                shortestDistanceFromCurrentTarget = distance;
            }
        }

        if(lockOnTarget_Left != null)
        {
            lockOnTarget.DisableEnemyCanvas();
            lockOnTarget = lockOnTarget_Left;
            Debug.Log($"<color=green>enter switch</color>");
            lockOnCamera.LookAt = lockOnTarget_Left.lockOnTransform_Self;
            Debug.Log($"<color=green>Left Look at Target {lockOnCamera.LookAt.parent.name}</color>");

            //EnableLockOnImage();
            //lockOnTarget.EnableEnemyCanvas();
        }
       
        
    }


    public void HandleSwitchRightTarget()
    {
        if(!isLockedOnTarget) return;

        if(lockOnTarget == null) return;

        if(enemiesWithinFOV.Count > 0)
            enemiesWithinFOV.Clear();

        Vector3 capusleEndPoint = transform.forward * maxLockOnDistance;

        Collider[] enemyColliders = Physics.OverlapCapsule(transform.position, capusleEndPoint, lockONDetectionRadius,
            playerCombat.enemyLayerMask);
        
        

        if(enemyColliders.Length > 0)
        {
            
            float dotProductThreshold = Mathf.Cos(playerFOV * 0.5f * Mathf.Deg2Rad);

            foreach (var enemyCollider in enemyColliders)
            {
                Vector3 enemyDirection = (enemyCollider.transform.position - transform.position).normalized;
                float dotProduct = Vector3.Dot(transform.forward, enemyDirection);

                if(dotProduct > dotProductThreshold)
                {
                    BaseEnemy enemy = enemyCollider.GetComponent<BaseEnemy>();
                    if(enemy != null)
                    {
                        if(enemy.isDead) continue;
                        enemiesWithinFOV.Add(enemy);
                    }
                    
                }

            }
        }  

        if(enemiesWithinFOV.Count == 0) return;

 
        float bestRightScore = Mathf.Infinity;
        float shortestDistanceFromCurrentTarget = Mathf.Infinity;

        foreach (BaseEnemy potentialTarget in enemiesWithinFOV)
        {
            if(potentialTarget == lockOnTarget) continue;

            Vector3 directionToEnemy = (potentialTarget.transform.position - transform.position).normalized;
           

            float rightScore = Vector3.Dot(transform.right, directionToEnemy); // Positive means right
            float distance = Vector3.Distance(lockOnTarget.transform.position, potentialTarget.transform.position);

            // Select the closest left target
            if (rightScore > 0 && (rightScore < bestRightScore || (rightScore == bestRightScore && distance < shortestDistanceFromCurrentTarget)))
            {
                lockOnTarget_Right = potentialTarget;
                bestRightScore = rightScore;
                shortestDistanceFromCurrentTarget = distance;
            }
        }

        if(lockOnTarget_Right != null)
        {
            lockOnTarget.DisableEnemyCanvas();
            lockOnTarget = lockOnTarget_Right;
            //Debug.Log($"<color=green>enter switch</color>");
            lockOnCamera.LookAt = lockOnTarget_Right.lockOnTransform_Self;
            //Debug.Log($"<color=green>Left Look at Target {lockOnCamera.LookAt.parent.name}</color>");

            //EnableLockOnImage();
            //lockOnTarget.EnableEnemyCanvas();
        }
       
        
    }

    #region DEBUG

    // void OnDrawGizmos()
    // {
    //     // // Define the start position and direction
    //     // Vector3 start = transform.position;
    //     // start.y = start.y + groundRaycastOffset;
    //     // Vector3 direction = -Vector3.up;
    //     // float radius = 0.2f;
    //     // float maxDistance = maxGroundCheckDistance;

    //     // // Set Gizmo color
    //     // Gizmos.color = Color.cyan;

    //     // // Draw the initial sphere at the raycast start point
    //     // Gizmos.DrawWireSphere(start, radius);

    //     // // If SphereCast hits something, draw the hit point and full cast path
    //     // if (Physics.SphereCast(start, radius, direction, out RaycastHit hit, maxDistance, groundLayer))
    //     // {
    //     //     Gizmos.color = Color.blue;
    //     //     // Draw a line from start to hit point
    //     //     Gizmos.DrawLine(start, hit.point);

    //     //     // Draw sphere at hit point
    //     //     Gizmos.color = Color.yellow;
    //     //     Gizmos.DrawWireSphere(hit.point, radius);
    //     // }
    //     // else
    //     // {
    //     //     // Draw the full cast length if nothing was hit (limited to avoid infinite line)
    //     //     Gizmos.DrawRay(start, direction * 5f); // Adjust 5f as needed
    //     // }

    //     VisualiseFOV();
    //     VisualiseLockOnCapsule();

    // }

    private void VisualiseLockOnCapsule()
    {
        Vector3 capsuleStart = transform.position;
        Vector3 capsuleEnd = transform.position + transform.forward * maxLockOnDistance;

        Gizmos.color = Color.yellow;

        DrawCapsule(capsuleStart, capsuleEnd, lockONDetectionRadius);
    }

    private void DrawCapsule(Vector3 start, Vector3 end, float radius)
    {
        Gizmos.DrawWireSphere(start,radius);
        Gizmos.DrawWireSphere(end,radius);
        Gizmos.DrawLine(start + Vector3.up * radius,end + Vector3.up * radius);
        Gizmos.DrawLine(start + Vector3.down * radius,end + Vector3.down * radius);
        Gizmos.DrawLine(start + Vector3.right * radius,end + Vector3.right * radius);
        Gizmos.DrawLine(start + Vector3.left * radius,end + Vector3.left * radius);
    }

    private void VisualiseFOV()
    {
        float halfFOV = playerFOV * 0.5f;

        Quaternion leftRayRotation = Quaternion.Euler(0, -halfFOV, 0);
        Quaternion rightRayRotation = Quaternion.Euler(0, halfFOV, 0);

        Vector3 leftRayDirection = leftRayRotation * transform.forward * maxLockOnDistance;
        Vector3 rightRayDirection = rightRayRotation * transform.forward * maxLockOnDistance;
        Vector3 centerRayDirection = transform.forward * maxLockOnDistance;

        Gizmos.color = Color.red;

        Gizmos.DrawRay(transform.position, leftRayDirection);
        Gizmos.DrawRay(transform.position, rightRayDirection);
        Gizmos.DrawRay(transform.position, centerRayDirection);
    }

    #endregion DEBUG
}
