using UnityEngine;

public class MyInputManager : MonoBehaviour
{
    MyInputActions myInputActions;

    [SerializeField] PlayerAnimationManager playerAnimationManager;
    [SerializeField] float moveAmount;
    public Vector2 movementInput;

    public float verticalMovementInput;
    public float horizontalMovementInput;
    
    private void OnEnable()
    {
        if (myInputActions == null)
        {
            myInputActions = new MyInputActions();
        }

        myInputActions.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();

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
        playerAnimationManager.UpdateAnimatorValuesForMovement(0, moveAmount);
    }
}
