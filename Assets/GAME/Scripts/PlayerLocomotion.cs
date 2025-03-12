
using System.Collections;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [SerializeField] private MyInputManager myInputManager;
    [SerializeField] private PlayerAnimationManager playerAnimationManager;
    [SerializeField] private PlayerCombat playerCombat;

    [SerializeField] public Rigidbody playerRigidBody;
    [SerializeField] public Camera mainCamera;

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
    [SerializeField] private float gravityIntensity;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpForce;

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

        Vector3 rollDirection = mainCamera.transform.forward * myInputManager.verticalMovementInput;
        rollDirection = rollDirection + mainCamera.transform.right * myInputManager.horizontalMovementInput;
        rollDirection.Normalize();
        rollDirection.y = 0;

        playerAnimationManager.PlayAnyInteractiveAnimation("Fast Roll", true,true);
    }

    // void OnDrawGizmos()
    // {
    //     // Define the start position and direction
    //     Vector3 start = transform.position;
    //     start.y = start.y + groundRaycastOffset;
    //     Vector3 direction = -Vector3.up;
    //     float radius = 0.2f;
    //     float maxDistance = maxGroundCheckDistance;

    //     // Set Gizmo color
    //     Gizmos.color = Color.cyan;

    //     // Draw the initial sphere at the raycast start point
    //     Gizmos.DrawWireSphere(start, radius);

    //     // If SphereCast hits something, draw the hit point and full cast path
    //     if (Physics.SphereCast(start, radius, direction, out RaycastHit hit, maxDistance, groundLayer))
    //     {
    //         Gizmos.color = Color.blue;
    //         // Draw a line from start to hit point
    //         Gizmos.DrawLine(start, hit.point);

    //         // Draw sphere at hit point
    //         Gizmos.color = Color.yellow;
    //         Gizmos.DrawWireSphere(hit.point, radius);
    //     }
    //     else
    //     {
    //         // Draw the full cast length if nothing was hit (limited to avoid infinite line)
    //         Gizmos.DrawRay(start, direction * 5f); // Adjust 5f as needed
    //     }
    // }
}
