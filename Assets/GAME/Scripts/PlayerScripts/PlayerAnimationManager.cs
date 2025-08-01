
using UnityEngine;

namespace EternalKeep
{
    public class PlayerAnimationManager : MonoBehaviour
    {
        [SerializeField] public Animator playerAnimator;
        [SerializeField] public PlayerLocomotion playerLocomotion;
        [SerializeField] private PlayerCombat playerCombat;
        [SerializeField] private bool canOverrideAnimation = false;

        [SerializeField] AnimationClip playerIdleAnimationClip;
        [SerializeField] AnimationClip sitIdleAnimationClip;
        [SerializeField] AnimationClip idleToSitAnimationClip;

        public bool CanOverrideAnimation
        {
            get { return canOverrideAnimation; }
        }
        int horizontal;
        int vertical;

        public bool inAnimActionStatus;
        public bool rootMotionUseStatus;
        [SerializeField] private AnimationClip lastClip;
        //[SerializeField]private HashSet<AnimationClip> attackAnimClipsHashSet = new HashSet<AnimationClip>();
        private int lastTransitionHash;

        private void Awake()
        {
            horizontal = Animator.StringToHash("Horizontal");
            vertical = Animator.StringToHash("Vertical");

            //attackAnimClipsHashSet = new HashSet<AnimationClip>(playerCombat.attackAnimClips);
        }

        public void PlayAnyInteractiveAnimation(string animationName, bool isInteracting, bool isUsingRootMotion = false,
            bool canMove = false, bool canRotate = false)
        {
            playerAnimator.SetBool("InAnimAction", isInteracting);
            playerAnimator.SetBool("isUsingRootMotion", isUsingRootMotion);

            playerAnimator.CrossFade(animationName, 0.1f);

            playerLocomotion.canMove = canMove;
            playerLocomotion.canRotate = canRotate;
        }

        public void UpdateAnimatorValuesForMovement(float horizontalMovement, float verticalMovement, bool isWalking)
        {
            if (isWalking)
            {
                //horizontalMovement = 0.5f; //0,5f -> sets to walking animation in locomotion blend tree
                verticalMovement = .5f;
            }

            playerAnimator.SetFloat(horizontal, horizontalMovement, 0.1f, Time.deltaTime);
            playerAnimator.SetFloat(vertical, verticalMovement, 0.1f, Time.deltaTime);
        }

        public void SetAllLayersToDefaultState_ExceptDamageState()
        {
            playerAnimator.Play("Empty State", 1); // to cancel ongoing animations in these two layers
            playerAnimator.Play("Empty State", 2);
        }

        public void EnableAnimationOverride()
        {
            canOverrideAnimation = true;
        }

        public void DisableAnimationOverride()
        {
            canOverrideAnimation = false;
        }

        public void PlayRestAnimation()
        {
            PlayAnyInteractiveAnimation(idleToSitAnimationClip.name, true, true);
        }

        public void PlaySitToIdleAnimation()
        {
            PlayAnyInteractiveAnimation(sitIdleAnimationClip.name, true, true);
        }

        private void OnAnimatorMove()
        {

            if (playerAnimator == null) return;

            HandleRootMotionUsage();

            //HandleHitDetectionOnTransitions();
        }

        private void HandleHitDetectionOnTransitions()
        {

            if (!playerAnimator.IsInTransition(1)) return; // Exit if not in transition

            AnimatorTransitionInfo transitionInfo = playerAnimator.GetAnimatorTransitionInfo(1);
            int currentTransitionHash = transitionInfo.fullPathHash;

            // Get the current animation clip playing on the animation layer (layer 1)
            var currentClipInfo = playerAnimator.GetCurrentAnimatorClipInfo(1);
            if (currentClipInfo.Length == 0) return; // If there's no animation playing, exit early

            AnimationClip currentClip = currentClipInfo[0].clip; // Get the active clip

            // If a new transition started from an attack animation
            // if (lastClip != null && attackAnimClipsHashSet.Contains(lastClip) && currentTransitionHash != lastTransitionHash)
            // {
            //     playerCombat.DisableHitDetection(); // Disable hit detection at the start of transition

            // }

            // If the last animation was an attack animation and now a different animation is playing
            // if (lastClip != null && attackAnimClipsHashSet.Contains(lastClip) && currentClip != lastClip)
            // {
            //     playerCombat.DisableHitDetection();  // Disable hit detection at the end of transition

            // }

            // Store values for next check
            lastClip = currentClip;
            lastTransitionHash = currentTransitionHash;
        }

        public void ResetPlayerAnimationLogics()
        {
            playerAnimator.SetBool("InAnimAction", false);
            playerAnimator.SetBool("isUsingRootMotion", false);
            playerAnimator.SetBool("isGrounded", true);
            playerAnimator.SetBool("isJumping", false);
            playerAnimator.SetBool("isMoving", false);
            playerAnimator.Play("Empty State", 3);
            SetAllLayersToDefaultState_ExceptDamageState();
            playerAnimator.Play(playerIdleAnimationClip.name, 0); // Reset to idle animation
        }

        private void HandleRootMotionUsage()
        {
            //if(playerCombat.IsStunned) return; // Disable root motion if player is kinematic
            //if(playerLocomotion.isJumping) return; // Disable root motion if player is jumping
            if (rootMotionUseStatus)
            {
                playerLocomotion.playerRigidBody.linearDamping = 0;
                Vector3 animDeltaPosition = playerAnimator.deltaPosition;
                animDeltaPosition.y = 0;
                Vector3 animTargetVelocity = animDeltaPosition / Time.deltaTime; // vel = changeinPos/ChangeinTime
                //animTargetVelocity.y = 0;
                playerLocomotion.playerRigidBody.linearVelocity = animTargetVelocity;

                // if(playerAnimator.GetBool("Block_test"))
                // {
                //     playerLocomotion.HandleRotation();
                // }
            }
        }
    }

}
