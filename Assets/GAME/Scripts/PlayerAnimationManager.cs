using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    [SerializeField] public Animator playerAnimator;
    int horizontal;
    int vertical;

    public bool inAnimActionStatus;

    private void Awake()
    {
        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }

    public void PlayAnyInteractiveAnimation(string animationName, bool isInteracting)
    {
        playerAnimator.SetBool("InAnimAction", isInteracting);

        playerAnimator.CrossFade(animationName, 0.1f);
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
}
