using UnityEngine;

public class MyInputManager : MonoBehaviour
{
    MyInputActions myInputActions;

    [SerializeField] PlayerAnimationManager playerAnimationManager;
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] public float moveAmount;
    public Vector2 movementInput;

    public float verticalMovementInput;
    public float horizontalMovementInput;

    public bool walkInput = false;
    
    private void OnEnable()
    {
        if (myInputActions == null)
        {
            myInputActions = new MyInputActions();
        }

        myInputActions.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
        myInputActions.PlayerMovement.Walk.performed += i => walkInput = true;
        myInputActions.PlayerMovement.Walk.canceled += i => walkInput = false;

        myInputActions.Enable();
    }


    private void OnDisable()
    {
        myInputActions.Disable();
    }

    public void HandleAllInput()
    {
        HandleMovementInput();
    }

    private void HandleMovementInput()
    {
        verticalMovementInput = movementInput.y;
        horizontalMovementInput = movementInput.x;

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalMovementInput) + Mathf.Abs(verticalMovementInput));
        playerAnimationManager.UpdateAnimatorValuesForMovement(0, moveAmount,playerLocomotion.isWalking);
    }
}