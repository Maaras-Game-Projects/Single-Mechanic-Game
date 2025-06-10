using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] PlayerAnimationManager playerAnimationManager;
    [SerializeField] MyInputManager myInputManager;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] PlayerCombat playerCombat;
    [SerializeField] StaminaSystem_Player playerStamina;

    [SerializeField] Transform playersCurrentSpawnPoint; //must set this to last rested bonfires spawn point

    [SerializeField] bool hideCursor = true;

    void Start()
    {
        // need to shidt this logic to scenemanager or game manager
        if (!hideCursor) return;
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
        if (myInputManager.walkInput && myInputManager.moveAmount > 0f)
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

    public void ResetPlayer()
    {

        playerLocomotion.ResetPlayerLocomotion();
        playerCombat.ResetPlayerCombatLogics();
        playerAnimationManager.ResetPlayerAnimationLogics();
        playerHealth.ResetPlayerHealth();       
        playerStamina.ResetPlayerStamina();

        transform.position = playersCurrentSpawnPoint.position;
        transform.rotation = Quaternion.identity; // Reset rotation to identity quaternion
        playerLocomotion.playerRigidBody.rotation = Quaternion.identity; // Reset rotation to identity quaternion
        playerLocomotion.playerRigidBody.position = playersCurrentSpawnPoint.position; // Reset position to spawn point

        Debug.Log($"<color=green>Player has been reset to spawn point: {playersCurrentSpawnPoint.position}</color>");

    }

    public void ResetPlayerDelayed(float delay)
    {
        StartCoroutine(ResetPlayerCoroutine(delay));
    }
    
    IEnumerator ResetPlayerCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetPlayer();
    }
    
}
