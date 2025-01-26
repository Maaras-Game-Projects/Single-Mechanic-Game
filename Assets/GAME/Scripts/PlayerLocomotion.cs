
using System.Collections;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [SerializeField] private MyInputManager myInputManager;
    [SerializeField] private PlayerAnimationManager playerAnimationManager;

    [SerializeField] Rigidbody playerRigidBody;
    [SerializeField] Transform mainCamera;

    [SerializeField] Vector3 moveDirection;
    [SerializeField] private Vector3 playerVelocity;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Vector3 targetDirection = Vector3.zero;
    [SerializeField] private Quaternion targetRotation;
    [SerializeField] private Quaternion playerRotation;
    [SerializeField] public bool isWalking = false;

    [Space]
    [Header("Falling and Landing Variables")]
    [Space]
    [SerializeField] public bool isGrounded;
    [SerializeField] float inAirTimer = 0f;
    [SerializeField] float leapingVelocity;
    [SerializeField] float fallingVelocity;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundRaycastOffset = 0.5f;

    [Space]
    [Header("Jump Variables")]
    [Space]
   
    [SerializeField] public bool isJumping = false;
    [SerializeField] private int gravityIntensity;
    [SerializeField] private int jumpHeight;
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
        moveDirection = mainCamera.forward * myInputManager.verticalMovementInput;
        moveDirection = moveDirection + mainCamera.right * myInputManager.horizontalMovementInput;
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
        targetDirection = Vector3.zero;

        targetDirection = mainCamera.forward * myInputManager.verticalMovementInput;
        targetDirection = targetDirection + mainCamera.right * myInputManager.horizontalMovementInput;
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
        /*if (isGrounded)
        {
            playerAnimationManager.playerAnimator.SetBool("isJumping", true);
            playerAnimationManager.PlayAnyInteractiveAnimation("Jump 1", false);

            jumpForce = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
            Vector3 jumpVelocity = moveDirection;
            //playerVelocity = moveDirection;
            jumpVelocity.y = jumpForce;
            Debug.Log("jumpForce = " + jumpForce);
            Debug.Log("jumpVelocity = " + jumpVelocity);
            playerRigidBody.linearVelocity = jumpVelocity;
            Debug.Log("linearVelocity of player = " + playerRigidBody.linearVelocity);

        }*/

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

                inAirTimer += Time.deltaTime;

                playerRigidBody.AddForce(transform.forward * leapingVelocity);
                playerRigidBody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
            }

        }

       

        if (Physics.SphereCast(raycastOrigin,.2f, -Vector3.up, out hit,groundLayer))
        {
            //Debug.DrawRay(hit.point, hit.normal, Color.blue); // Shows the hit point and its normal
            //Debug.Log($"SphereCast hit: {hit.collider.name}");

            //Debug.Log("ground spherecast check");

            if (!isGrounded && playerAnimationManager.inAnimActionStatus)
            {
                //Debug.Log("ground spherecast check to anim");
                playerAnimationManager.PlayAnyInteractiveAnimation("Fall To Landing", true);
            }

            Vector3 rayHitPoint = hit.point;
            playerTargetPosition.y = rayHitPoint.y;
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
}
