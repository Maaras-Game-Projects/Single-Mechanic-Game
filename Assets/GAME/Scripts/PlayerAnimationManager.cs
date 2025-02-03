using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    [SerializeField] public Animator playerAnimator;
    [SerializeField] public PlayerLocomotion playerLocomotion;
    int horizontal;
    int vertical;

    public bool inAnimActionStatus;
    public bool rootMotionUseStatus;

    private void Awake()
    {
        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }

    public void PlayAnyInteractiveAnimation(string animationName, bool isInteracting,bool isUsingRootMotion = false,
        bool canMove = false, bool canRotate = false)
    {
        playerAnimator.SetBool("InAnimAction", isInteracting);
        playerAnimator.SetBool("isUsingRootMotion", isUsingRootMotion);

        playerAnimator.CrossFade(animationName, 0.1f);

        playerLocomotion.canMove = canMove;
        playerLocomotion.canRotate = canRotate;
    }

    public void UpdateAnimatorValuesForMovement(float horizontalMovement,float verticalMovement,bool isWalking)
    {
        if (isWalking)
        {
            horizontalMovement = 0.5f; //0,5f -> sets to walking animation in locomotion blend tree
            verticalMovement = 0.5f;
        }

        playerAnimator.SetFloat(horizontal, horizontalMovement, 0.1f, Time.deltaTime);
        playerAnimator.SetFloat(vertical, verticalMovement, 0.1f, Time.deltaTime);
    }

    private void OnAnimatorMove()
    {
        HandleRootMotionUsage();
    }

    private void HandleRootMotionUsage()
    {
        if (rootMotionUseStatus)
        {
            playerLocomotion.playerRigidBody.linearDamping = 0;
            Vector3 animDeltaPosition = playerAnimator.deltaPosition;
            animDeltaPosition.y = 0;
            Vector3 animTargetVelocity = animDeltaPosition / Time.deltaTime; // vel = changeinPos/ChangeinTime
            //animTargetVelocity.y = 0;
            playerLocomotion.playerRigidBody.linearVelocity = animTargetVelocity;
        }
    }
}
