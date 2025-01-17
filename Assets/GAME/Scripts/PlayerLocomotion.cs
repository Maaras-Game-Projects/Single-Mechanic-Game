
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

    [Header("Falling and Landing Variables")]

    [SerializeField] bool isGrounded;
    [SerializeField] float inAirTimer = 0f;
    [SerializeField] float leapingVelocity;
    [SerializeField] float fallingVelocity;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundRaycastOffset = 0.5f;

    public void HandleAllMovement()
    {
        HandleFallingAndLanding();

        if (playerAnimationManager.inAnimActionStatus)
            return;

        HandleMovement();
        HandleRotation();

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

    private void HandleFallingAndLanding()
    {
        RaycastHit hit;
        Vector3 raycastOrigin = transform.position;
        raycastOrigin.y = raycastOrigin.y + groundRaycastOffset;

        //Debug.DrawLine(raycastOrigin, raycastOrigin + Vector3.down * groundRaycastOffset, Color.cyan);
       // Debug.DrawRay(raycastOrigin, Vector3.down * groundRaycastOffset, Color.red);

        if (!isGrounded)
        {
            if(!playerAnimationManager.inAnimActionStatus)
            {
                playerAnimationManager.PlayAnyInteractiveAnimation("Fall", true);
            }

            inAirTimer += Time.deltaTime;

            playerRigidBody.AddForce(transform.forward * leapingVelocity);
            playerRigidBody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
        }

       

        if (Physics.SphereCast(raycastOrigin,.2f, -Vector3.up, out hit,groundLayer))
        {
            //Debug.DrawRay(hit.point, hit.normal, Color.blue); // Shows the hit point and its normal
            //Debug.Log($"SphereCast hit: {hit.collider.name}");

            Debug.Log("ground spherecast check");

            if (!isGrounded && playerAnimationManager.inAnimActionStatus)
            {
                Debug.Log("ground spherecast check to anim");
                playerAnimationManager.PlayAnyInteractiveAnimation("Fall To Landing", true);
            }

            inAirTimer = 0;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

    }
}
