using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] PlayerAnimationManager playerAnimationManager;
    [SerializeField] MyInputManager myInputManager;

    private void Update()
    {
        Debug.Log("Is Interacting VAl = " + playerAnimationManager.inAnimActionStatus);
        myInputManager.HandleAllInput();
    }

    private void FixedUpdate()
    {
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

        playerLocomotion.isJumping = playerAnimationManager.playerAnimator.GetBool("isJumping");
        playerAnimationManager.playerAnimator.SetBool("isGrounded", playerLocomotion.isGrounded);
        //myInputManager.ResetJumpInput();

    }
}
