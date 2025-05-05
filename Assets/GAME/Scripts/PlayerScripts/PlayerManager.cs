using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] PlayerAnimationManager playerAnimationManager;
    [SerializeField] MyInputManager myInputManager;
    [SerializeField] PlayerHealth playerHealth;

    [SerializeField] bool hideCursor = true;

    void Start()
    {
        // need to shidt this logic to scenemanager or game manager
        if(!hideCursor) return;
        Cursor.visible = false; // Hide the cursor
    }


    private void Update()
    {
        if (playerHealth.isPlayerDead) return;
        //Debug.Log("Is Interacting VAl = " + playerAnimationManager.inAnimActionStatus);
        myInputManager.HandleAllInput();
    }

    private void FixedUpdate()
    {
        //if (playerHealth.isPlayerDead) return;
        playerLocomotion.HandleAllMovement();
    }

    private void LateUpdate()
    {
        if(myInputManager.walkInput && myInputManager.moveAmount > 0f)
        {
            playerLocomotion.isWalking = true;
        }
        else
        {
            playerLocomotion.isWalking = false;
        }

        
        playerAnimationManager.inAnimActionStatus = playerAnimationManager.playerAnimator.GetBool("InAnimAction");

        playerAnimationManager.rootMotionUseStatus = playerAnimationManager.playerAnimator.GetBool("isUsingRootMotion");
        
       

        playerLocomotion.isJumping = playerAnimationManager.playerAnimator.GetBool("isJumping");
        playerAnimationManager.playerAnimator.SetBool("isGrounded", playerLocomotion.isGrounded);
        //myInputManager.ResetJumpInput();
        myInputManager.jumpInput = false;

    }
}
