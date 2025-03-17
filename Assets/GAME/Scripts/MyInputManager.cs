using UnityEngine;

public class MyInputManager : MonoBehaviour
{
    MyInputActions myInputActions;

    [SerializeField] PlayerAnimationManager playerAnimationManager;
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] PlayerCombat playerCombat;
    [SerializeField] public float moveAmount;
    public Vector2 movementInput;

    public float verticalMovementInput;
    public float horizontalMovementInput;

    public bool walkInput = false;
    public bool jumpInput = false;
    public bool rollInput = false;

    public bool attackInput = false;
    public bool blockInput = false;
    public bool lockOnInput = false;
    public float switchTargetDelta;
    public float switchTargetDeltaThreshold = 50f;
    
    public bool hasSwipedLeft = false;
    public bool hasSwipedRight = false;
    public bool lockOnleftSwitchInput = false;
    public bool lockOnRightSwitchInput = false;
    
    private void OnEnable()
    {
        if (myInputActions == null)
        {
            myInputActions = new MyInputActions();
        }

        myInputActions.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
        myInputActions.PlayerMovement.Walk.performed += i => walkInput = true;
        myInputActions.PlayerMovement.Walk.canceled += i => walkInput = false;

        myInputActions.PlayerMovement.Roll.performed += i => rollInput = true;
        myInputActions.PlayerMovement.Jump.performed += i => jumpInput = true;

        myInputActions.PlayerCombat.Attack.performed += i => attackInput = true;
         myInputActions.PlayerCombat.Block.performed += i => blockInput = true;
        myInputActions.PlayerCombat.Block.canceled += i => {
            blockInput = false;
            playerAnimationManager.playerAnimator.SetBool("inBlocking", false);
            playerCombat.isBlocking = false;
            playerCombat.EnableCounter();
            playerCombat.AllowParry();
            playerCombat.DisableParry();
        };


        myInputActions.PlayerCombat.TargetLockOn.performed += i => lockOnInput = true;
        myInputActions.PlayerCombat.SwitchLeftTarget.performed += i => switchTargetDelta = i.ReadValue<float>();
        myInputActions.PlayerCombat.SwitchRightTarget.performed += i => lockOnRightSwitchInput = true;

        myInputActions.Enable();
    }


    private void OnDisable()
    {
        myInputActions.Disable();
    }

    public void HandleAllInput()
    {
        HandleLockONInput();
        HandleSwitchLockONInput_Left();
        HandleMovementInput();
        HandleJumpInput();
        HandleRollInput();
        HandleAttackInput();
        HandleBlockInput();
    }

    private void HandleJumpInput()
    {
        if(jumpInput)
        {
            jumpInput = false;
            playerLocomotion.HandleJump();
        }
    }

    private void HandleRollInput()
    {
        if (rollInput)
        {
            rollInput = false;
            playerLocomotion.HandleRolling();
        }
    }

    private void HandleAttackInput()
    {
        if (attackInput)
        {
            
            attackInput = false;
            playerCombat.StartToAttack();
        }
    }

    private void HandleLockONInput()
    {
        if (lockOnInput)
        {
            
            lockOnInput = false;
            playerLocomotion.HandleTargetLockON();
        }
    }

    private void HandleSwitchLockONInput_Left()
    {
        if (!playerLocomotion.isLockedOnTarget) return;

        if(switchTargetDelta > switchTargetDeltaThreshold)
        {
            if(!hasSwipedLeft)
            {
                lockOnleftSwitchInput = true;
                hasSwipedLeft = true;
            }
            
        }
        else
        {
            hasSwipedLeft = false;
        }

        if (lockOnleftSwitchInput)
        {
            lockOnleftSwitchInput = false;
            Debug.Log("Switching Left");
            playerLocomotion.HandleSwitchLeftTarget();
        }
        
    }


    private void HandleBlockInput()
    {
        if (blockInput)
        {
            
            playerCombat.BlockAttack();
        }
    }


    private void HandleMovementInput()
    {
        verticalMovementInput = movementInput.y;
        horizontalMovementInput = movementInput.x;

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalMovementInput) + Mathf.Abs(verticalMovementInput));

        if(playerLocomotion.isLockedOnTarget)
        {
            playerAnimationManager.UpdateAnimatorValuesForMovement(horizontalMovementInput, verticalMovementInput,playerLocomotion.isWalking);
        }
        else
        {
             playerAnimationManager.UpdateAnimatorValuesForMovement(0, moveAmount,playerLocomotion.isWalking);
        }
       
    }
}
