using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [SerializeField] private MyInputManager myInputManager;
    [SerializeField] Rigidbody playerRigidBody;
    [SerializeField] Transform mainCamera;

    [SerializeField] Vector3 moveDirection;
    [SerializeField] private Vector3 playerVelocity;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Vector3 targetDirection = Vector3.zero;
    [SerializeField] private Quaternion targetRotation;
    [SerializeField] private Quaternion playerRotation;

    public void HandleAllMovement()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        moveDirection = mainCamera.forward * myInputManager.verticalMovementInput;
        moveDirection = moveDirection + mainCamera.right * myInputManager.horizontalMovementInput;
        moveDirection.Normalize();
        moveDirection.y = 0;


        playerVelocity = moveDirection * movementSpeed;

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
}