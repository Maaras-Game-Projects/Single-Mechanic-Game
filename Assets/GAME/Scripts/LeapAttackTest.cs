using UnityEngine;

namespace EternalKeep
{
    public class LeapAttackTest : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] float verticalSpeed = 5f;
        [SerializeField] float forwardSpeed = 5f;
        [SerializeField] float animationSlowDownSpeed = 5f;

        [SerializeField] Transform targetTransform;

        [SerializeField] Vector3 targetPosition;
        [SerializeField] private bool isLeaping = false;


        [SerializeField] float verticalSpeed_WindUp = 5f;
        [SerializeField] float forwardSpeed_WindUp = 5f;
        [SerializeField] float verticalSpeed_Leap = 5f;
        [SerializeField] float forwardSpeed_Leap = 5f;
        [SerializeField] float verticalSpeed_Land = 1f;
        [SerializeField] float forwardSpeed_Land = 1f;

        [SerializeField] AnimationClip animationClip;

        [SerializeField] float elapsedTime = 0f;
        [SerializeField] Vector3 leapSTartPos;
        [SerializeField] private float leapSpeedDynamic;

        void OnAnimatorMove()
        {
            if (animator == null) return;

            Vector3 deltaPosition = animator.deltaPosition;

            // if (isLeaping)
            // {

            //     Vector3 currentPosition = transform.position;

            //     //Vector3 flatCurrentPosition = new Vector3(currentPosition.x, 0f, currentPosition.z);
            //     //Vector3 flatTargetPosition = new Vector3(targetPosition.x, 0f, targetPosition.z);

            //     // Calculate the direction to the target position
            //     //Vector3 directionToTarget = (flatTargetPosition - flatCurrentPosition).normalized;
            //     Vector3 directionToTarget = (targetPosition - currentPosition).normalized;

            //     Vector3 moveAmount = directionToTarget * forwardSpeed_Leap * Time.deltaTime;

            //     Vector3 newPosition = currentPosition + moveAmount;
            //     newPosition.y = currentPosition.y + deltaPosition.y * verticalSpeed_Leap;

            //     if(Vector3.Distance(currentPosition, targetPosition) < 0.1f)
            //     {
            //         newPosition = targetPosition; // Snap to the target position
            //         isLeaping = false; // Stop leaping when close enough to the target

            //     }

            //     transform.position = newPosition;


            // }
            // if (isLeaping)
            // {

            //     elapsedTime += Time.deltaTime;

            //     // Calculate the normalized time based on the animation length
            //     float normalizedTime = elapsedTime / animationClip.length;

            //     Vector3 newPosition = Vector3.Lerp(leapSTartPos, targetPosition, normalizedTime);

            //     newPosition.y += deltaPosition.y * verticalSpeed_Leap;

            //     transform.position = newPosition;

            //     if (normalizedTime >= 1f)
            //     {
            //         isLeaping = false; // Stop leaping when the animation is complete
            //         elapsedTime = 0f; // Reset elapsed time
            //     }

            // }
            // else
            if (isLeaping)
            {

                // Apply vertical speed
                deltaPosition.y *= verticalSpeed;

                // Apply forward speed
                deltaPosition.z *= leapSpeedDynamic;

                // Apply the modified deltaPosition to the character's position
                transform.position += deltaPosition;

                if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    transform.position = targetPosition; // Snap to the target position
                    isLeaping = false; // Stop leaping when close enough to the target
                }

            }
            else
            {
                // Apply vertical speed
                deltaPosition.y *= verticalSpeed;

                // Apply forward speed
                deltaPosition.z *= forwardSpeed;

                // Apply the modified deltaPosition to the character's position
                transform.position += deltaPosition;
            }

        }


        public void SetWindUpAnimationMovementSpeed()
        {
            verticalSpeed = verticalSpeed_WindUp;
            forwardSpeed = forwardSpeed_WindUp;
        }

        public void SetLandAnimationMovementSpeed()
        {
            verticalSpeed = verticalSpeed_Land;
            forwardSpeed = forwardSpeed_Land;
        }

        public void SetLeapAnimationMovementSpeed()
        {
            verticalSpeed = verticalSpeed_Leap;
            forwardSpeed = forwardSpeed_Leap;
        }

        public void StartLeapingTowardsTarget()
        {
            targetPosition = DetermineLandPostion();
            isLeaping = true;
            elapsedTime = 0f; // Reset elapsed time
            leapSTartPos = transform.position; // Store the starting position for the leap
            leapSpeedDynamic = Vector3.Distance(transform.position, targetPosition) / animationClip.length;
        }

        public Vector3 DetermineLandPostion()
        {
            if (targetTransform == null) return Vector3.zero;

            return targetTransform.position;
        }

        public void SetAnimationSlowSpeed()
        {
            animator.speed = animationSlowDownSpeed;
        }

        public void ResetAnimationSpeed()
        {
            animator.speed = 1f;
        }
    }

}


