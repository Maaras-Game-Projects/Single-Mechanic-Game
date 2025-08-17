using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EternalKeep
{
    public class PlayerLocomotion : MonoBehaviour
    {
        [SerializeField] private MyInputManager myInputManager;
        [SerializeField] private PlayerAnimationManager playerAnimationManager;
        [SerializeField] private PlayerCombat playerCombat;
        [SerializeField] private PlayerHealth playerHealth;

        [SerializeField] public Rigidbody playerRigidBody;
        [SerializeField] public Camera mainCamera;

        [Space]
        [Header("Lock On Variables")]
        [Space]

        [SerializeField] public CinemachineCamera mainCinemachineCamera;
        [SerializeField] public CinemachineCamera lockOnCamera;

        [SerializeField] CinemachineBasicMultiChannelPerlin mainCinemachineCameraMultiChannelperlin;

        [SerializeField] CinemachineBasicMultiChannelPerlin lockOnCameraMultiChannelperlin;

        [SerializeField] private bool isCameraShaking = false;
        [SerializeField] public NPC_Root lockOnTarget;
        [SerializeField] public NPC_Root lockOnTarget_Left;
        [SerializeField] public NPC_Root lockOnTarget_Right;
        [SerializeField] public float maxLockOnDistance = 10f;
        [SerializeField] public Image lockOnImage;
        [SerializeField] public float playerFOV = 90f;

        [SerializeField] private float lockONDetectionRadius = 3f;
        [SerializeField] List<NPC_Root> enemiesWithinFOV = new List<NPC_Root>();

        [SerializeField] private LayerMask obstacleLayerMask;



        [SerializeField] Vector3 moveDirection;
        [SerializeField] private Vector3 playerVelocity;

        private float defaultRotationSpeed;
        private float defaultMovementSpeed;
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private Vector3 targetDirection = Vector3.zero;
        [SerializeField] private Quaternion targetRotation;

        [SerializeField] private Quaternion playerRotation;
        [SerializeField] public bool isWalking = false;
        [SerializeField] public bool canMove = true;
        [SerializeField] public bool canRotate = true;
        [SerializeField] public bool canRotateWhileAction = false;

        [SerializeField] public bool isDodging = true;
        [SerializeField] private bool canChainDodge = true;
        [SerializeField] private bool canAttackAfterDodge = true;
        public bool CanAttackAfterDodge => canAttackAfterDodge;

        [SerializeField] string dodgeRollChainTrigger;
        [SerializeField] string dodgeAttackTriggerBool;
        public string DodgeAttackTriggerBool => dodgeAttackTriggerBool;

        [Space]
        [Header("Roll on Stairs Variables")]
        [Space]
        [SerializeField] private bool onStairs = false;
        [SerializeField] private string stairsTag;
        [SerializeField] private float verticalTargetPositionOffset = 0.5f;

        [Space]
        [Header("Fall Damage Variables")]
        [Space]

        

        [SerializeField] private bool canInitiateVoidFallDamageDeathCheck = false;
        [SerializeField] private bool canCheckFallDamageDistance = true;
        [SerializeField] private bool shouldDieAtLanding = false;
        [SerializeField] private bool canDoubleCheckFallDamageOnLanding = false;

        [SerializeField] private float maxFallHeight = 8f;
        [SerializeField] private float maxFallHeightCheckDistance = 25f;

        private Vector3 fallDistancerayStart;
        private Vector3 voidfallDistancerayEndPoint;


        [Space]
        [Header("Falling and Landing Variables")]
        [Space]
        [SerializeField] public bool isGrounded;
        [SerializeField] float inAirTimer = 0f;
        [SerializeField] float leapingVelocity;
        [SerializeField] float fallingVelocity;
        [SerializeField] LayerMask groundLayer;
        [SerializeField] float groundRaycastOffset = 0.5f;
        [SerializeField] float maxGroundCheckDistance = 2.5f;

        [SerializeField] private bool canFallStrafe;


        [Space]
        [Header("Jump Variables")]
        [Space]

        [SerializeField] public bool isJumping = false;
        [SerializeField] public bool isLockedOnTarget = false;
        [SerializeField] private float gravityIntensity;
        [SerializeField] private float jumpHeight;
        [SerializeField] private float jumpForce;
        [SerializeField] private float horizontalJumpForce;
        [SerializeField] private float fallControlStrength = 3f;
        [SerializeField] private float fallControlAcceleration = 4f;
        [SerializeField] private float fallTurnSpeed = 3f;

        private float defaultHorizontalJumpForce;


        [Space]
        [Header("Stamina Cost Variables")]
        [Space]

        [SerializeField] StaminaSystem_Player staminaSystem_Player;
        [SerializeField] private float jumpStaminaCost = 10f;
        [SerializeField] private float dodgeStaminaCost = 15f;


        [Space]
        [Header("Event Variables")]
        [Space]

        [SerializeField] UnityEvent onPlayerJump;
        [SerializeField] UnityEvent onPlayerDodge;

        CapsuleCollider capsuleCollider;
        float capsuleHeight_Default;
        Vector3 capsuleCenter_Default;


        void Awake()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
            capsuleHeight_Default = capsuleCollider.height;
            capsuleCenter_Default = capsuleCollider.center;

            jumpForce = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
            defaultHorizontalJumpForce = horizontalJumpForce;

            defaultMovementSpeed = movementSpeed;
            defaultRotationSpeed = rotationSpeed;

            //GameSaveData.Instance.playerLocomotion = this;
        }

        void Update()
        {

        }

        void LateUpdate()
        {
            if (isLockedOnTarget && lockOnTarget != null)
            {
                EnableLockOnImage();

                if (lockOnTarget.healthSystem.IsDead)
                {
                    DisableLockON();
                    return;
                }
            }
            else
            {
                DisableLockOnImage();
            }
        }

        public void HandleAllMovement()
        {
            HandleFallingAndLanding();

            if (playerAnimationManager.inAnimActionStatus)
                return;
            //if(playerCombat.IsStunned) return;
            HandleMovement();
            HandleRotation();
            //HandleJump();

        }

        private void HandleMovement()
        {

            if (!canMove) return;

            Vector3 modifiedForwardCameraTransform = mainCamera.transform.forward;
            modifiedForwardCameraTransform.y = 0;
            modifiedForwardCameraTransform.Normalize();

            Vector3 modifiedRightCameraTransform = mainCamera.transform.right;
            modifiedRightCameraTransform.y = 0;
            modifiedRightCameraTransform.Normalize();


            moveDirection = modifiedForwardCameraTransform * myInputManager.verticalMovementInput;
            moveDirection = moveDirection + modifiedRightCameraTransform * myInputManager.horizontalMovementInput;


            moveDirection.Normalize();
            moveDirection.y = 0;


            if (isWalking) //if is walking, move in walkspeed or else in default speed
            {
                playerVelocity = moveDirection * walkSpeed;
            }
            else
            {
                playerVelocity = moveDirection * movementSpeed;


            }

            playerRigidBody.linearVelocity = playerVelocity;
        }

        public void EnableRotationWhileAction()
        {
            canRotateWhileAction = true;
        }

        public void DisableRotationWhileAction()
        {
            canRotateWhileAction = false;
        }

        public void HandleRotation()
        {


            if (isLockedOnTarget)
            {
                //Debug.Log("<color=yello>In Rotation in LockOn Begin</color>");

                if (isDodging)
                {
                    //Debug.Log("<color=blue>In Rotation AFter Dodge Begin</color>");

                    // targetDirection = Vector3.zero;


                    // targetDirection = mainCamera.transform.forward * myInputManager.verticalMovementInput;
                    // targetDirection = targetDirection + mainCamera.transform.right * myInputManager.horizontalMovementInput;
                    // targetDirection.Normalize();
                    // targetDirection.y = 0;

                    // if (targetDirection == Vector3.zero)
                    // {
                    //     targetDirection = transform.forward;
                    // }

                    // targetRotation = Quaternion.LookRotation(targetDirection);
                    // playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                    // transform.rotation = playerRotation;

                    // //Debug.Log("<color=white>In Rotation AFter Dodge End</color>");

                    if ((playerCombat.IsAttacking || isDodging) && canRotateWhileAction)
                    {

                        HandleRotationWhileLockedOff(2.5f);
                        //Debug.Log($"<color=green>Attack Rot</color>");
                        return;
                    }


                }
                else
                {
                    if (lockOnTarget == null) return;

                    Vector3 targetDirection = lockOnTarget.transform.position - transform.position;
                    targetDirection.y = 0;
                    targetDirection.Normalize();

                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                    //Debug.Log("<color=red>In Locked On Rotation</color>");

                }

            }
            else
            {
                if (!canRotate) return;

                if ((playerCombat.IsAttacking || isDodging) && canRotateWhileAction)
                {

                    HandleRotationWhileLockedOff(2.5f);
                    //Debug.Log($"<color=green>Attack Rot</color>");
                    return;
                }

                if (!playerCombat.IsAttacking && !isDodging)
                {

                    HandleRotationWhileLockedOff(1); // rotate on default speed when not attacking or dodging
                    //Debug.Log($"<color=red>Default Rot</color>");
                    //return;
                }



            }

        }

        private void HandleRotationWhileLockedOff(float rotationSpeedModifier)
        {
            targetDirection = Vector3.zero;

            targetDirection = mainCamera.transform.forward * myInputManager.verticalMovementInput;
            targetDirection = targetDirection + mainCamera.transform.right * myInputManager.horizontalMovementInput;
            targetDirection.Normalize();
            targetDirection.y = 0;

            if (targetDirection == Vector3.zero)
            {
                targetDirection = transform.forward;
            }

            targetRotation = Quaternion.LookRotation(targetDirection);

            playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * rotationSpeedModifier * Time.deltaTime);

            transform.rotation = playerRotation;
        }

        public void HandleJump()
        {


            if (playerAnimationManager.inAnimActionStatus) return;
            if (playerAnimationManager.rootMotionUseStatus) return;
            if (isDodging) return;
            if (playerCombat.isBlocking) return;
            if (playerAnimationManager.playerAnimator.IsInTransition(1)) return; // checking if block animation to empty state transition is happening

            if (staminaSystem_Player.CurrentStamina < jumpStaminaCost) return;

            //SUBTLE JUMP

            if (isGrounded)
            {
                isGrounded = false;


                Vector3 modifiedForwardCameraTransform = mainCamera.transform.forward;
                modifiedForwardCameraTransform.y = 0;
                modifiedForwardCameraTransform.Normalize();

                Vector3 modifiedRightCameraTransform = mainCamera.transform.right;
                modifiedRightCameraTransform.y = 0;
                modifiedRightCameraTransform.Normalize();


                moveDirection = modifiedForwardCameraTransform * myInputManager.verticalMovementInput;
                moveDirection = moveDirection + modifiedRightCameraTransform * myInputManager.horizontalMovementInput;


                moveDirection.Normalize();



                playerAnimationManager.playerAnimator.SetBool("Block_test", false);
                playerAnimationManager.playerAnimator.Play("Empty State", 1);


                // capsuleCollider.height = 1.75f;
                // capsuleCollider.center = new Vector3(capsuleCollider.center.x, 0.75f, capsuleCollider.center.z);


                Vector3 jumpVelocity = moveDirection * horizontalJumpForce;
                //Debug.DrawRay(transform.position, transform.forward * 2f, Color.red, 2f);

                // if(Physics.CapsuleCast(transform.position + Vector3.up * 0.1f,
                //      transform.position + Vector3.up * 1.2f, 1f, moveDirection.normalized, out RaycastHit hit, .25f))
                // {
                //     //Vector3 rayHitPoint = hit.point;
                //     jumpVelocity.z = 0f;
                //     jumpVelocity.x = 0f;
                //     Debug.Log("<color=cyan>movedirection = " + moveDirection + "</color>");


                // }



                //Vector3 jumpVelocity = Vector3.up * jumpForce;

                // jumpVelocity.x = 0;
                // jumpVelocity.z = 0;
                jumpVelocity.y = jumpForce;

                playerRigidBody.linearVelocity = jumpVelocity;

                //StartCoroutine(AddJumpMomentumLater(.08f));

                playerAnimationManager.playerAnimator.SetBool("isJumping", true);

                playerAnimationManager.PlayAnyInteractiveAnimation("OS_Jump_InPlace", false);

                staminaSystem_Player.DepleteStamina(jumpStaminaCost);
                onPlayerJump?.Invoke();

                Debug.Log($"<color=green>velocity on jump = {playerRigidBody.linearVelocity}</color>");

            }

            //FLOATY JUMP

            // if (isGrounded)
            // {
            //     // Trigger the jump animation
            //     playerAnimationManager.playerAnimator.SetBool("isJumping", true);
            //     playerAnimationManager.PlayAnyInteractiveAnimation("OS_Jump_InPlace", false);

            //     // Calculate the jump force required
            //     jumpForce = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);

            //     // Apply the jump force using AddForce
            //     Vector3 jumpDirection = Vector3.up; // Keep existing horizontal movement
            //     jumpDirection.y = jumpForce; // Add vertical force for jumping

            //     Debug.Log("jumpForce = " + jumpForce);
            //     Debug.Log("jumpDirection before force = " + jumpDirection);

            //     // Apply the force to the Rigidbody
            //     playerRigidBody.AddForce(jumpDirection, ForceMode.VelocityChange);
            //     playerRigidBody.AddForce(moveDirection*horizontalJumpForce, ForceMode.VelocityChange);

            //     Debug.Log("linearVelocity of player after force = " + playerRigidBody.linearVelocity);

            // }

            //HYBRID JUMP

            // if (isGrounded)
            // {
            //     playerAnimationManager.playerAnimator.SetBool("isJumping", true);
            //     playerAnimationManager.PlayAnyInteractiveAnimation("OS_Jump_InPlace", false);

            //     jumpForce = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);

            //     Vector3 jumpVelocity = Vector3.zero;
            //     jumpVelocity.y = jumpForce;

            //     Vector3 horizontalVelocity = moveDirection * horizontalJumpForce;
            //     horizontalVelocity.y = 0;

            //     //Vector3 horizontalVelocity = moveDirection * horizontalJumpForce;

            //     Debug.Log("jumpForce = " + jumpForce);
            //     Debug.Log("jumpVelocity = " + jumpVelocity);
            //     playerRigidBody.linearVelocity = jumpVelocity;

            //     playerRigidBody.AddForce(horizontalVelocity,ForceMode.VelocityChange);

            //     Debug.Log("linearVelocity of player = " + playerRigidBody.linearVelocity);

            // }

        }

        IEnumerator AddJumpMomentumLater(float waitTime)
        {
            yield return new WaitForSeconds(waitTime); // or small delay like 0.05s

            Vector3 horizontalVelocity = moveDirection * horizontalJumpForce;
            //playerRigidBody.linearVelocity += horizontalVelocity;

            // if(Physics.CapsuleCast(transform.position,
            //          transform.position + Vector3.up * 0.5f, 0.5f, moveDirection, out RaycastHit hit, .75f, groundLayer))
            // {
            //     //Vector3 rayHitPoint = hit.point;
            //     horizontalVelocity.z = 0f;
            //     horizontalVelocity.x = 0f;
            //     Debug.Log("<color=cyan>movedirection = " + moveDirection + "</color>");
            //     Debug.Log("Jump Velocity = " + horizontalVelocity);

            // }

            playerRigidBody.linearVelocity += horizontalVelocity;

            // Optional wall check again here
            // if (!Physics.CapsuleCast(…)) // avoid adding horizontal velocity if wall is right ahead
            //     playerRigidBody.linearVelocity += horizontalVelocity;
        }

        IEnumerator FallAfterJump(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            inAirTimer += Time.deltaTime;
            playerRigidBody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
        }

        public void FallAfterJump()
        {

            /*inAirTimer += Time.deltaTime;
            playerRigidBody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
            Debug.Log("Fall After Jump");*/
        }

        private void HandleFallingAndLanding()
        {
            RaycastHit hit;
            Vector3 raycastOrigin = transform.position;
            raycastOrigin.y = raycastOrigin.y + groundRaycastOffset;

            Vector3 playerTargetPosition = transform.position;

            //Debug.DrawLine(raycastOrigin, raycastOrigin + Vector3.down * groundRaycastOffset, Color.cyan);
            // Debug.DrawRay(raycastOrigin, Vector3.down * groundRaycastOffset, Color.red);

            if (!isGrounded)
            {
                if (!isJumping)
                {
                    if (!playerAnimationManager.inAnimActionStatus)
                    {
                        playerAnimationManager.PlayAnyInteractiveAnimation("OS_Jump_Fall_Loop", true);

                    }

                    HandleFallStrafe();

                    playerAnimationManager.playerAnimator.SetBool("isUsingRootMotion", false);
                    inAirTimer += Time.deltaTime;

                    playerRigidBody.AddForce(transform.forward * leapingVelocity);
                    playerRigidBody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);

                    //Debug.DrawRay(transform.position, -Vector3.up * 8, Color.red);




                    if (canCheckFallDamageDistance)
                    {
                        fallDistancerayStart = transform.position;
                        canCheckFallDamageDistance = false;
                        RaycastHit hitInfo;

                        if (Physics.Raycast(fallDistancerayStart, -Vector3.up, out hitInfo, maxFallHeightCheckDistance, groundLayer))
                        {

                            if (hitInfo.collider != null)
                            {
                                float fallHeightDifference_ABS = Mathf.Abs(hitInfo.point.y - fallDistancerayStart.y);
                                Debug.Log($"<color=green> Fall Height DIFF Val = {fallHeightDifference_ABS}</color>");
                                if (fallHeightDifference_ABS > maxFallHeight)
                                {
                                    shouldDieAtLanding = true;
                                }
                                else
                                {
                                    Debug.Log($"<color=green>within Fall Height</color>");
                                    canInitiateVoidFallDamageDeathCheck = false;
                                }
                                canDoubleCheckFallDamageOnLanding = true;
                            }
                        }
                        else
                        {
                            canInitiateVoidFallDamageDeathCheck = true;
                            voidfallDistancerayEndPoint = fallDistancerayStart + Vector3.down * maxFallHeight;
                        }
                    }

                    if (canInitiateVoidFallDamageDeathCheck)
                    {
                        float heightDifference_ABS = Mathf.Abs(transform.position.y - voidfallDistancerayEndPoint.y);
                        Debug.Log($"<color=yellow>Fall height diff = {heightDifference_ABS}</color>");
                        if (heightDifference_ABS <= 0.5f)
                        {
                            //kill player and disable
                            Debug.Log($"<color=blue>VOID FALL DEATH</color>");
                            playerHealth.DieByVOIDFallDamage();
                            canInitiateVoidFallDamageDeathCheck = false;
                            canCheckFallDamageDistance = true;
                        }
                    }
                }

            }

            // if(!isGrounded)
            // {
            //     if(Physics.Raycast(transform.position + Vector3.up * 0.5f, moveDirection, out RaycastHit hitInfo, .75f, groundLayer))
            //     {
            //         Vector3 fallVelocity = playerRigidBody.linearVelocity;
            //         fallVelocity.x = 0f;
            //         fallVelocity.z = 0f;
            //         playerRigidBody.linearVelocity = fallVelocity;
            //         Debug.Log($"<color=green> target fall velocity after collision = {playerRigidBody.linearVelocity}</color>");

            //     }
            // }


            if (isJumping) return;

            //if(inCoyoteTime) return;

            if (Physics.SphereCast(raycastOrigin, 0.2f, -Vector3.up, out hit, maxGroundCheckDistance, groundLayer))
            {
                if (!isGrounded && playerAnimationManager.inAnimActionStatus)
                //if (playerAnimationManager.inAnimActionStatus)
                {
                    //Debug.Log("ground spherecast check to anim");
                    // capsuleCollider.height = 1.75f;
                    // capsuleCollider.center = new Vector3(capsuleCollider.center.x, 0.75f, capsuleCollider.center.z);
                    playerAnimationManager.playerAnimator.SetBool("isMoving", true);
                    playerAnimationManager.PlayAnyInteractiveAnimation("OS_Jump_Land", true, true);
                    //playerAnimationManager.playerAnimator.SetBool("isUsingRootMotion", true);
                    //Debug.Log("<color=red>In Land</color>");
                }

                Vector3 rayHitPoint = hit.point;
                playerTargetPosition.y = rayHitPoint.y;
                //Debug.Log("Ground hit: " + hit.collider.name);
                inAirTimer = 0;
                isGrounded = true;

                canCheckFallDamageDistance = true;
                if (shouldDieAtLanding)
                {
                    DoubleCheckFallDamageOnLanding();

                }
                DoubleCheckFallDamageOnLanding();

            }
            else
            {
                isGrounded = false;
            }



            if (isGrounded && !isJumping)
            {
                if (playerAnimationManager.inAnimActionStatus || myInputManager.moveAmount > 0 || isDodging)
                {
                    if (isDodging && onStairs)
                    {
                        playerTargetPosition.y = playerTargetPosition.y + verticalTargetPositionOffset;
                    }


                    transform.position = Vector3.Lerp(transform.position, playerTargetPosition, Time.deltaTime / 0.1f);

                }
                else
                {
                    transform.position = playerTargetPosition;
                }
            }


            //Debug.Log($"<color=cyan>velocity on fall = {playerRigidBody.linearVelocity}</color>");
        }

        private void DoubleCheckFallDamageOnLanding()
        {
            if (!canDoubleCheckFallDamageOnLanding) return;
            canDoubleCheckFallDamageOnLanding = false;
            var abs_FallHeightDiff = Mathf.Abs(transform.position.y - fallDistancerayStart.y);
            if (abs_FallHeightDiff > maxFallHeight)
            {
                Debug.Log($"<color=red>LAND FALL DEATH</color>");
                if (playerHealth.CanTakeFallDamage)
                {
                    playerHealth.TakeDamage(playerHealth.MaxHealth * 5);
                }
                
                shouldDieAtLanding = false;
                canInitiateVoidFallDamageDeathCheck = false;
            }
        }

        private void HandleFallStrafe()
        {
            if (!canFallStrafe) return;

            Vector3 inAirTargetDirection = mainCamera.transform.forward * myInputManager.verticalMovementInput;
            inAirTargetDirection = inAirTargetDirection + mainCamera.transform.right * myInputManager.horizontalMovementInput;
            inAirTargetDirection.Normalize();
            inAirTargetDirection.y = 0;

            //Debug.Log($"<color=yellow> Fall direction = {inAirTargetDirection}</color>");

            if (inAirTargetDirection != Vector3.zero)
            {
                Quaternion turnRotation = Quaternion.LookRotation(inAirTargetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, turnRotation, Time.deltaTime * fallTurnSpeed);

                //Debug.Log($"<color=red> Turn Velocity after SLerp = {playerRigidBody.transform.rotation}</color>");
            }


            //Vector3 inAirControlStrengthVector = transform.TransformDirection(inAirTargetDirection * fallControlStrength);
            Vector3 inAirControlStrengthVector = inAirTargetDirection * fallControlStrength;

            //Debug.Log($"<color=white> InAirControlStrength Vector = {inAirControlStrengthVector}</color>");

            Vector3 currentVelocity = playerRigidBody.linearVelocity;

            // Blend movement input with current velocity
            Vector3 fallControlVelocity = Vector3.Lerp(
                new Vector3(currentVelocity.x, 0, currentVelocity.z),
                new Vector3(inAirControlStrengthVector.x, 0, inAirControlStrengthVector.z),
                fallControlAcceleration * Time.deltaTime
            );

            //Vector3 fallControlVelocity = new Vector3(inAirControlStrengthVector.x,currentVelocity.y,inAirControlStrengthVector.z);




            //playerRigidBody.linearVelocity = Vector3.Lerp(currentVelocity,fallControlVelocity,Time.deltaTime * fallControlAcceleration);
            playerRigidBody.linearVelocity = new Vector3(fallControlVelocity.x, currentVelocity.y, fallControlVelocity.z);

            //Debug.Log($"<color=cyan> target fall velocity b4 collisiom = {fallControlVelocity}</color>");




            //Debug.Log($"<color=green> Fall Strafe Velocity after Lerp = {playerRigidBody.linearVelocity}</color>");
        }

        public bool CanBufferDodge()
        {
            if (isJumping) return true;

            if (playerCombat.isBlocking) return true;
            if (playerAnimationManager.playerAnimator.IsInTransition(1)
                // || playerAnimationManager.playerAnimator.IsInTransition(2)
                ) return true; // checking if block animation to empty state transition is happening
            if (playerAnimationManager.inAnimActionStatus) return true;
            if (canChainDodge) return false;
            if (playerAnimationManager.rootMotionUseStatus) return true;

            return false;

        }

        public void HandleRolling()
        {
            if (isJumping) return;

            if (playerCombat.isBlocking) return;
            if (playerAnimationManager.playerAnimator.IsInTransition(1)
                // || playerAnimationManager.playerAnimator.IsInTransition(2)
                ) return; // checking if block animation to empty state transition is happening
            //if (playerAnimationManager.inAnimActionStatus) return;

            if (staminaSystem_Player.CurrentStamina < dodgeStaminaCost) return;


            playerAnimationManager.playerAnimator.Play("Empty State", 1);
            playerAnimationManager.playerAnimator.SetLayerWeight(1, 0);


            if (canChainDodge)
            {
                canRotate = true;

                playerAnimationManager.playerAnimator.SetBool(dodgeRollChainTrigger, true);
                capsuleCollider.height = 1f;
                capsuleCollider.center = new Vector3(capsuleCollider.center.x, 0.7f, capsuleCollider.center.z);

                staminaSystem_Player.DepleteStamina(dodgeStaminaCost);
                onPlayerDodge?.Invoke();
                //gameObject.GetComponent<AudioSource>().Play(); //debug
                //AudioManager_STN.instance.PlayAudio_SFX(gameObject.GetComponent<AudioSource>().clip, transform.position);//debug
                return;
            }

            if (playerAnimationManager.CanOverrideAnimation)
            {
                isDodging = true;
                //canRotate = true;
                canRotateWhileAction = true;

                //PerformDodge();
                playerAnimationManager.PlayAnyInteractiveAnimation("OS_Roll_F", false, true, false, true);
                //Debug.Log("<color=yellow>In ROll</color>");

                capsuleCollider.height = 1f;
                capsuleCollider.center = new Vector3(capsuleCollider.center.x, 0.7f, capsuleCollider.center.z);

                staminaSystem_Player.DepleteStamina(dodgeStaminaCost);
                onPlayerDodge?.Invoke();
                //gameObject.GetComponent<AudioSource>().Play(); //debug
                //AudioManager_STN.instance.PlayAudio_SFX(gameObject.GetComponent<AudioSource>().clip, transform.position);//debug
                Debug.Log("<color=yellow>overridden Dodge</color>");
                return;
            }

            if (playerAnimationManager.inAnimActionStatus) return;
            if (playerAnimationManager.rootMotionUseStatus) return;
            if (isDodging) return;
            isDodging = true;
            //if(isLockedOnTarget)  DisableLockON();

            PerformDodge();

            // if (!playerAnimationManager.rootMotionUseStatus || playerAnimationManager.CanOverrideAnimation)
            // {
            //     isDodging = true;
            //     //if(isLockedOnTarget)  DisableLockON();

            //     PerformDodge();
            // }

        }

        private void PerformDodge()
        {
            Vector3 rollDirection = mainCamera.transform.forward * myInputManager.verticalMovementInput;
            rollDirection = rollDirection + mainCamera.transform.right * myInputManager.horizontalMovementInput;
            rollDirection.Normalize();
            rollDirection.y = 0;

            if (rollDirection == Vector3.zero)
            {
                rollDirection = transform.forward;
            }

            Quaternion rollRotation = Quaternion.LookRotation(rollDirection);
            transform.rotation = rollRotation;

            playerAnimationManager.PlayAnyInteractiveAnimation("OS_Roll_F", false, true, false, true);
            //Debug.Log("<color=yellow>In ROll</color>");

            capsuleCollider.height = 1f;
            capsuleCollider.center = new Vector3(capsuleCollider.center.x, 0.7f, capsuleCollider.center.z);

            staminaSystem_Player.DepleteStamina(dodgeStaminaCost);

            //gameObject.GetComponent<AudioSource>().Play(); //debug
            //AudioManager_STN.instance.PlayAudio_SFX(gameObject.GetComponent<AudioSource>().clip, transform.position);//debug
            onPlayerDodge?.Invoke();
        }

        public void EnableDodgeRollChain()
        {
            canChainDodge = true;
        }

        public void DisableDodgeRollChain()
        {
            canChainDodge = false;
        }

        public void EnableDodgeAttack()
        {
            canAttackAfterDodge = true;
        }

        public void DisableDodgeAttack()
        {
            canAttackAfterDodge = false;
        }

        public void ResetColliderHeightAndCenter()
        {
            capsuleCollider.height = capsuleHeight_Default;
            capsuleCollider.center = capsuleCenter_Default;
            DisableOnStairsBool();
        }

        private void DisableOnStairsBool()
        {
            onStairs = false;
        }

        public void HandleTargetLockON()
        {
            if (isLockedOnTarget)
            {
                DisableLockON();
                return;
            }
            else
            {
                isLockedOnTarget = true;
            }

            isLockedOnTarget = true;

            if (enemiesWithinFOV.Count > 0)
                enemiesWithinFOV.Clear();

            Vector3 capusleEndPoint = transform.position + transform.forward * maxLockOnDistance;

            Collider[] enemyColliders = Physics.OverlapCapsule(transform.position, capusleEndPoint, lockONDetectionRadius,
                playerCombat.enemyLayerMask);



            if (enemyColliders.Length > 0)
            {

                float dotProductThreshold = Mathf.Cos(playerFOV * 0.5f * Mathf.Deg2Rad);

                foreach (var enemyCollider in enemyColliders)
                {
                    Vector3 enemyDirection = (enemyCollider.transform.position - transform.position).normalized;
                    float dotProduct = Vector3.Dot(transform.forward, enemyDirection);

                    if (dotProduct > dotProductThreshold)
                    {
                        //BaseEnemy enemy = enemyCollider.GetComponent<BaseEnemy>();
                        NPC_Root enemy = enemyCollider.GetComponent<NPC_Root>();
                        if (enemy != null)
                        {
                            if (enemy.healthSystem.IsDead) continue;
                            if (Physics.Linecast(transform.position + Vector3.up * 0.5f, enemy.lockOnTransform_Self.position, obstacleLayerMask))
                                continue;
                            enemiesWithinFOV.Add(enemy);
                        }

                    }

                }
            }
            else
            {
                isLockedOnTarget = false;
                return;
            }

            if (enemiesWithinFOV.Count > 0)
            {
                //BaseEnemy nearestEnemy = null;
                NPC_Root nearestEnemy = null;
                float shortestDistance = Mathf.Infinity;

                foreach (var enemy in enemiesWithinFOV)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        nearestEnemy = enemy;
                    }
                }

                lockOnTarget = nearestEnemy;

            }
            else
            {
                isLockedOnTarget = false;
                return;
            }


            if (lockOnTarget == null)
            {
                isLockedOnTarget = false;
                return;
            }
            else
            {

                lockOnCamera.LookAt = lockOnTarget.lockOnTransform_Self;
                mainCinemachineCamera.gameObject.SetActive(false);
                lockOnCamera.gameObject.SetActive(true);

                //Debug.Log("Locked on");

                lockOnTarget.EnableEnemyCanvas();
                //EnableLockOnImage();
            }

            //clear enemies within fov list after testing implementation
            //enemiesWithinFOV.Clear();
        }

        public void DisableLockON()
        {
            if (!isLockedOnTarget) return;
            isLockedOnTarget = false;
            mainCinemachineCamera.gameObject.SetActive(true);
            lockOnCamera.gameObject.SetActive(false);
            DisableLockOnImage();

            lockOnTarget.DisableEnemyCanvas();
            lockOnTarget = null;

            if (enemiesWithinFOV.Count > 0)
                enemiesWithinFOV.Clear();
        }

        void EnableLockOnImage()
        {
            Vector2 lastScreenPos = lockOnImage.transform.position;
            lockOnImage.gameObject.SetActive(true);
            Vector2 targetPos = mainCamera.WorldToScreenPoint(lockOnTarget.lockOnTransform_Self.transform.position);

            if (Vector3.Distance(targetPos, lastScreenPos) > .25f)
            {
                //lockOnImage.transform.position = targetPos;
                lockOnImage.transform.position =
                                Vector2.Lerp(lockOnImage.transform.position, targetPos, Time.deltaTime * 3f);
                lastScreenPos = targetPos;
            }

            // lockOnImage.transform.position = 
            //         Vector2.Lerp(lockOnImage.transform.position, screenPos, Time.deltaTime * 10);

            //lockOnImage.transform.position = screenPos;
        }

        void DisableLockOnImage()
        {
            lockOnImage.gameObject.SetActive(false);

        }

        public void HandleSwitchLeftTarget()
        {
            if (!isLockedOnTarget) return;

            if (lockOnTarget == null) return;

            if (enemiesWithinFOV.Count > 0)
                enemiesWithinFOV.Clear();

            Vector3 capusleEndPoint = transform.forward * maxLockOnDistance;

            Collider[] enemyColliders = Physics.OverlapCapsule(transform.position, capusleEndPoint, lockONDetectionRadius,
                playerCombat.enemyLayerMask);



            if (enemyColliders.Length > 0)
            {

                float dotProductThreshold = Mathf.Cos(playerFOV * 0.5f * Mathf.Deg2Rad);

                foreach (var enemyCollider in enemyColliders)
                {
                    Vector3 enemyDirection = (enemyCollider.transform.position - transform.position).normalized;
                    float dotProduct = Vector3.Dot(transform.forward, enemyDirection);

                    if (dotProduct > dotProductThreshold)
                    {
                        //BaseEnemy enemy = enemyCollider.GetComponent<BaseEnemy>();
                        NPC_Root enemy = enemyCollider.GetComponent<NPC_Root>();
                        if (enemy != null)
                        {
                            if (enemy.healthSystem.IsDead) continue;
                            if (Physics.Linecast(transform.position + Vector3.up * 0.5f, enemy.lockOnTransform_Self.position, obstacleLayerMask))
                                continue;
                            enemiesWithinFOV.Add(enemy);
                        }

                    }

                }
            }

            if (enemiesWithinFOV.Count == 0) return;


            float bestLeftScore = Mathf.NegativeInfinity;
            float shortestDistanceFromCurrentTarget = Mathf.Infinity;

            foreach (NPC_Root potentialTarget in enemiesWithinFOV)
            {
                if (potentialTarget == lockOnTarget) continue;

                Vector3 directionToEnemy = (potentialTarget.transform.position - transform.position).normalized;


                float leftScore = Vector3.Dot(transform.right, directionToEnemy); // Negative means left
                float distance = Vector3.Distance(lockOnTarget.transform.position, potentialTarget.transform.position);

                // Select the closest left target
                if (leftScore < 0 && (leftScore > bestLeftScore || (leftScore == bestLeftScore && distance < shortestDistanceFromCurrentTarget)))
                {
                    lockOnTarget_Left = potentialTarget;
                    bestLeftScore = leftScore;
                    shortestDistanceFromCurrentTarget = distance;
                }
            }

            if (lockOnTarget_Left != null)
            {
                lockOnTarget.DisableEnemyCanvas();
                lockOnTarget = lockOnTarget_Left;
                //Debug.Log($"<color=green>enter switch</color>");
                lockOnCamera.LookAt = lockOnTarget_Left.lockOnTransform_Self;
                //Debug.Log($"<color=green>Left Look at Target {lockOnCamera.LookAt.parent.name}</color>");

                //EnableLockOnImage();
                lockOnTarget.EnableEnemyCanvas();
            }


        }



        public void HandleSwitchRightTarget()
        {
            if (!isLockedOnTarget) return;

            if (lockOnTarget == null) return;

            if (enemiesWithinFOV.Count > 0)
                enemiesWithinFOV.Clear();

            Vector3 capusleEndPoint = transform.forward * maxLockOnDistance;

            Collider[] enemyColliders = Physics.OverlapCapsule(transform.position, capusleEndPoint, lockONDetectionRadius,
                playerCombat.enemyLayerMask);



            if (enemyColliders.Length > 0)
            {

                float dotProductThreshold = Mathf.Cos(playerFOV * 0.5f * Mathf.Deg2Rad);

                foreach (var enemyCollider in enemyColliders)
                {
                    Vector3 enemyDirection = (enemyCollider.transform.position - transform.position).normalized;
                    float dotProduct = Vector3.Dot(transform.forward, enemyDirection);

                    if (dotProduct > dotProductThreshold)
                    {
                        //BaseEnemy enemy = enemyCollider.GetComponent<BaseEnemy>();
                        NPC_Root enemy = enemyCollider.GetComponent<NPC_Root>();
                        if (enemy != null)
                        {
                            if (enemy.healthSystem.IsDead) continue;
                            if (Physics.Linecast(transform.position + Vector3.up * 0.5f, enemy.lockOnTransform_Self.position, obstacleLayerMask))
                                continue;
                            enemiesWithinFOV.Add(enemy);
                        }

                    }

                }
            }

            if (enemiesWithinFOV.Count == 0) return;


            float bestRightScore = Mathf.Infinity;
            float shortestDistanceFromCurrentTarget = Mathf.Infinity;

            foreach (NPC_Root potentialTarget in enemiesWithinFOV)
            {
                if (potentialTarget == lockOnTarget) continue;

                Vector3 directionToEnemy = (potentialTarget.transform.position - transform.position).normalized;


                float rightScore = Vector3.Dot(transform.right, directionToEnemy); // Positive means right
                float distance = Vector3.Distance(lockOnTarget.transform.position, potentialTarget.transform.position);

                // Select the closest left target
                if (rightScore > 0 && (rightScore < bestRightScore || (rightScore == bestRightScore && distance < shortestDistanceFromCurrentTarget)))
                {
                    lockOnTarget_Right = potentialTarget;
                    bestRightScore = rightScore;
                    shortestDistanceFromCurrentTarget = distance;
                }
            }

            if (lockOnTarget_Right != null)
            {
                lockOnTarget.DisableEnemyCanvas();
                lockOnTarget = lockOnTarget_Right;
                //Debug.Log($"<color=green>enter switch</color>");
                lockOnCamera.LookAt = lockOnTarget_Right.lockOnTransform_Self;
                //Debug.Log($"<color=green>Left Look at Target {lockOnCamera.LookAt.parent.name}</color>");

                //EnableLockOnImage();
                lockOnTarget.EnableEnemyCanvas();
            }


        }

        public void SetMovementAndRotationSpeedToZero()
        {
            movementSpeed = 0f;
            rotationSpeed = 0f;
            //Debug.Log("Im Called");
        }

        public void ResetPlayerLocomotion()
        {
            canMove = true;
            canRotate = true;
            playerRigidBody.constraints = RigidbodyConstraints.None;
            playerRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            isGrounded = true;
            isJumping = false;
            isLockedOnTarget = false;
            isDodging = false;
            canAttackAfterDodge = false;
            canChainDodge = false;
            canRotateWhileAction = false;

            rotationSpeed = defaultRotationSpeed;
            movementSpeed = defaultMovementSpeed;

            lockOnTarget = null;
            lockOnTarget_Left = null;
            lockOnTarget_Right = null;
            enemiesWithinFOV.Clear();

        }

        public void PeformCameraShake(float duration, float magnitude)
        {
            if (isCameraShaking) return;
            if (isLockedOnTarget)
            {
                StartCoroutine(ShakeCameraCoroutine(magnitude, duration, lockOnCameraMultiChannelperlin));
            }
            else
            {
                StartCoroutine(ShakeCameraCoroutine(magnitude, duration, mainCinemachineCameraMultiChannelperlin));
            }
        }

        IEnumerator ShakeCameraCoroutine(float magnitude, float duration, CinemachineBasicMultiChannelPerlin multiChannelperlin)
        {
            isCameraShaking = true;
            float elapsedTime = 0f;
            float durationHalved = duration * 0.5f;
            while (elapsedTime < durationHalved)
            {
                multiChannelperlin.AmplitudeGain = Mathf.Lerp(0f, magnitude, elapsedTime / durationHalved);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            multiChannelperlin.AmplitudeGain = magnitude;
            elapsedTime = 0f;
            while (elapsedTime < durationHalved)
            {
                multiChannelperlin.AmplitudeGain = Mathf.Lerp(magnitude, 0f, elapsedTime / durationHalved);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            multiChannelperlin.AmplitudeGain = 0f;
            isCameraShaking = false;
        }

        public void SetVelocityToZero()
        {
            playerRigidBody.linearVelocity = Vector3.zero;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == stairsTag)
            {
                onStairs = true;
            }
            else
            {
                onStairs = false;
            }


        }

        void OnCollisionExit(Collision collision)
        {

            canFallStrafe = true;

            horizontalJumpForce = defaultHorizontalJumpForce;


        }

        void OnCollisionStay(Collision collision)
        {

            if (!isGrounded)
            {
                canFallStrafe = false;
                //Debug.Log($"<color=green> canFallStrafe status = {canFallStrafe}</color>");
            }
            horizontalJumpForce = 0f;

        }


        #region DEBUG
    #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            //VisualiseGroundCheck();

            // VisualiseFOV();
            // VisualiseLockOnCapsule();

            //if (!Application.isPlaying) return;

            // Vector3 point1 = transform.position + Vector3.up * 0.1f;
            // Vector3 point2 = transform.position + Vector3.up * 1.2f;
            // float radius = 1f;
            // Vector3 direction = moveDirection.normalized;
            // float distance = .25f;

            // Gizmos.color = Color.yellow;
            // Gizmos.DrawWireSphere(point1 + direction * distance, radius);
            // Gizmos.DrawWireSphere(point2 + direction * distance, radius);
            // Gizmos.DrawLine(point1 + direction * distance, point2 + direction * distance);

            //Debug.DrawRay(transform.position, -Vector3.up * 8, Color.green);

        }

        private void VisualiseGroundCheck()
        {
            // Define the start position and direction
            Vector3 start = transform.position;
            start.y = start.y + groundRaycastOffset;
            Vector3 direction = -Vector3.up;
            float radius = 0.2f;
            float maxDistance = maxGroundCheckDistance;

            // Set Gizmo color
            Gizmos.color = Color.cyan;

            // Draw the initial sphere at the raycast start point
            Gizmos.DrawWireSphere(start, radius);

            // If SphereCast hits something, draw the hit point and full cast path
            if (Physics.SphereCast(start, radius, direction, out RaycastHit hit, maxDistance, groundLayer))
            {
                Gizmos.color = Color.blue;
                // Draw a line from start to hit point
                Gizmos.DrawLine(start, hit.point);

                // Draw sphere at hit point
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(hit.point, radius);
            }
            else
            {
                // Draw the full cast length if nothing was hit (limited to avoid infinite line)
                Gizmos.DrawRay(start, direction * 5f); // Adjust 5f as needed
            }
        }

        private void VisualiseLockOnCapsule()
        {
            Vector3 capsuleStart = transform.position;
            Vector3 capsuleEnd = transform.position + transform.forward * maxLockOnDistance;

            Gizmos.color = Color.yellow;

            DrawCapsule(capsuleStart, capsuleEnd, lockONDetectionRadius);
        }

        private void DrawCapsule(Vector3 start, Vector3 end, float radius)
        {
            Gizmos.DrawWireSphere(start, radius);
            Gizmos.DrawWireSphere(end, radius);
            Gizmos.DrawLine(start + Vector3.up * radius, end + Vector3.up * radius);
            Gizmos.DrawLine(start + Vector3.down * radius, end + Vector3.down * radius);
            Gizmos.DrawLine(start + Vector3.right * radius, end + Vector3.right * radius);
            Gizmos.DrawLine(start + Vector3.left * radius, end + Vector3.left * radius);
        }

        private void VisualiseFOV()
        {
            float halfFOV = playerFOV * 0.5f;

            Quaternion leftRayRotation = Quaternion.Euler(0, -halfFOV, 0);
            Quaternion rightRayRotation = Quaternion.Euler(0, halfFOV, 0);

            Vector3 leftRayDirection = leftRayRotation * transform.forward * maxLockOnDistance;
            Vector3 rightRayDirection = rightRayRotation * transform.forward * maxLockOnDistance;
            Vector3 centerRayDirection = transform.forward * maxLockOnDistance;

            Gizmos.color = Color.red;

            Gizmos.DrawRay(transform.position, leftRayDirection);
            Gizmos.DrawRay(transform.position, rightRayDirection);
            Gizmos.DrawRay(transform.position, centerRayDirection);
        }
    #endif

        #endregion DEBUG


    }


}




