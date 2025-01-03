using UnityEngine;

public class MyInputManager : MonoBehaviour
{
    MyInputActions myInputActions;

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
    }
}
