using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    [SerializeField] Animator playerAnimator;
    int horizontal;
    int vertical;

    private void Awake()
    {
        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }

    public void UpdateAnimatorValuesForMovement(float horizontalMovement,float verticalMovement)
    {
        playerAnimator.SetFloat(horizontal, horizontalMovement, 0.1f, Time.deltaTime);
        playerAnimator.SetFloat(vertical, verticalMovement, 0.1f, Time.deltaTime);
    }
}
