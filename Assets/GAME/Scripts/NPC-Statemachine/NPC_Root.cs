using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace EternalKeep
{
    // This class contains all common properties and methods for all NPC classes
    public class NPC_Root : MonoBehaviour
    {
        [SerializeField] protected Transform spawnPoint; //

        [SerializeField] bool canEnemyRespawnAfterDeath = true;
        [SerializeField] bool canPatrol = false;
        public bool CanPatrol => canPatrol;
        
        

        
        [SerializeField] public bool CanEnemyRespawnAfterDeath => canEnemyRespawnAfterDeath;

        [SerializeField] public float currentDamageToDeal = 50f; //
        [SerializeField] public bool canAttackKnockback = false; //

        public bool CanAttackKnockback => canAttackKnockback; //
        //[SerializeField] public bool isDead = false; //

        [SerializeField] public Animator animator; // 
        [SerializeField] public AnimationClip idleAnimationClip; // 
        [SerializeField] public Rigidbody rigidBody; //

        [SerializeField] private CapsuleCollider npcCollider;

        [SerializeField] public bool isInteracting = false; //

        [SerializeField] public bool canRotateWhileAttack = false; //

        [SerializeField] public Transform lockOnTransform_Self; //
        [SerializeField] public float lookRotationSpeed; //
        [SerializeField] public Transform targetTransform; //

        [SerializeField] public List<State> states = new List<State>(); //


        [SerializeField] public Statemachine statemachine; //[SerializeField] public State currentState => statemachine.currentState; //

        [SerializeField]protected IdleState idleState;
        public bool canDetectHit = false; ////////
        public bool parryable = false; //////// might create seperate hit detection module with parryable logic

        [SerializeField] public StaminaSystem staminaSystem;
        [SerializeField] public HealthSystem healthSystem;
        [SerializeField] public ShieldSystem shieldSystem;
        [SerializeField] public PoiseSystem poiseSystem;
        [SerializeField] public ProjectileSpawner projectileSpawner;

        [SerializeField] private CanvasGroup enemyCanvasGroup;

        [SerializeField] public LayerMask obstacleLayerMask;

        [SerializeField] public LayerMask enemyLayerMask;

        [Space]
        [Header("Combo Variables")]
        [Space]
        [SerializeField] bool canChainCombo = false;
        public bool CanChainCombo => canChainCombo;

        [SerializeField] bool isPerformingComboAttacks = false;
        public bool IsPerformingComboAttacks => isPerformingComboAttacks;
        [SerializeField] DynamicComboAttackState dynamicComboAttackState;

        [Space]
        [Header("Custom Start Animation Variables")]
        [Space]
        [SerializeField] public AnimationClip customStartAnimationClip;
        [SerializeField] public string preAggression_TransitionBoolString;
        [SerializeField] public bool canGoHostile = true;
        [SerializeField] float targetDetectionRadius_PreAggression = 5f;
        [SerializeField] float preAggression_TransitionAdditionalDelay = 1f;

        [Space]
        [Header("NavMesh and Strafe Variables")]
        [Space]

        [SerializeField] public NavMeshAgent navMeshAgent;
        [SerializeField] public bool isChasingTarget = false; //bb
        [SerializeField] public bool isPatrolling = false; //bb
        [SerializeField] public bool isStrafing = false; //bb

        [Space]
        [Header("Strafe Transition Variables")]
        [Space]

        [SerializeField] float strafeBlendSpeed = 8f;
        float currentX, currentZ;
        float targetX, targetZ;





        [Space]
        [Header("Stun Variables")]
        [Space]
        [SerializeField] protected bool isStunned;

        [SerializeField] float stunDuration = 5f;

        [SerializeField] string stunTransitionBoolString = "isStunned"; //
        [SerializeField] private AnimationClip stunAnimationClip;

        public bool IsStunned => isStunned; //
        [SerializeField] public float chaseSpeed = 1f; //
        [SerializeField] public float strafeSpeed = 1f; //


        [Space]
        [Header("Leap Attack Variables")]
        [Space]

        public bool inLeapAttack = false;
        public bool useModifiedLeapSpeed = false;

        public float mod_verticalLeapingSpeed = 1f;
        public float mod_forwardLeapingSpeed = 1f;

        [SerializeField] private float verticalLeapingSpeed_Default = 1f; //
        [SerializeField] private float forwardLeapingSpeed_Default = 1f; //


        [Space]
        [Header("Player Variables")]
        [Space]
        public PlayerHealth playerHealth;
        public LayerMask playerLayerMask;

        [Space]
        [Header("Turn Animation Variables")]
        [Space]
        [SerializeField] private AnimationClip turnAnimRight_90;
        [SerializeField] private AnimationClip turnAnimLeft_90;
        [SerializeField] private AnimationClip turnAnimRight_180;
        [SerializeField] private AnimationClip turnAnimLeft_180;
        [SerializeField] public bool isTurning = false; //

        private Quaternion targetRotationAtEndOfTurn;


        [Space]
        [Header("Debugging")]
        [Space]
        public bool debug = false;
        [SerializeField] private TextMeshPro debugStateText;

        [Space]
        [Header("Fall Damage Variables")]
        [Space]

        [SerializeField] private float maxFallHeight;
        [SerializeField]private int maxFallHeightCheckDistance;

        [SerializeField] protected UnityEvent onVoidFall;
        [SerializeField] protected UnityEvent onFallDeath;

       
        private bool canInitiateVoidFallDamageDeathCheck;
        private Vector3 voidfallDistancerayEndPoint;
        private Vector3 fallDistancerayStart;
        
        private bool canDoubleCheckFallDamageOnLanding;
        private bool canCheckFallDamageDistance;
        private bool shouldDieAtLanding;




        [Space]
        [Header("Grounding & Falling Variables")]
        [Space]

        [SerializeField] protected bool canFallAndLand = true;

        [SerializeField] private float groundRaycastOffset_Vertical;
        [SerializeField]private float groundRaycastOffset_Horizontal;
        [SerializeField]private float groundRaycastOffset_Forward;
        private Vector3 groundedTargetPosition;

        [SerializeField] private float verticalTargetPositionOffset;
        [SerializeField] private float maxGroundCheckDistance;
        [SerializeField] private LayerMask groundLayer;

        [SerializeField] protected bool isGrounded;
        [SerializeField] protected bool isJumping;

        [SerializeField] private float inAirTimer;
        [SerializeField] private float leapingVelocity;
        [SerializeField] private float fallingVelocity;
        [SerializeField] private AnimationClip landAnimClip;
        [SerializeField] private AnimationClip fallAnimClip;


        protected CapsuleCollider capsuleCollider;
        private Vector3 capsuleColliderCenter_Default;
        private float capsuleHeight_Default;
        [SerializeField] private Vector3 targetPosition_OnStairs;


        [Space]
        [Header("Events")]
        [Space]

        [SerializeField] private UnityEvent onHitDetectionEnd;
        

        void OnEnable()
        {
            if (healthSystem.IsDead) return;
            //Reset animation
            animator.SetBool("isInteracting", false);
            animator.SetBool("isStunned", false);
            animator.Play("Empty State", 1);

            if (customStartAnimationClip != null)
            {
                if (preAggression_TransitionBoolString != "")
                {
                    animator.SetBool(preAggression_TransitionBoolString, false);
                }
                animator.Play(customStartAnimationClip.name, 0);
            }
            else
            {
                animator.Play(idleAnimationClip.name, 0); // Reset to idle animation
            }
            

            capsuleCollider.enabled = true;
        }

        void Start()
        {


        }



        // might need to add collider for hit detection


        public virtual void SetAllStates()
        {
            foreach (State state in states)
            {
                state.AssignRootTOState(this);
            }
        }

        public void InitAllSubStatemachines()
        {
            foreach (State state in states)
            {
                state.InitSubStateMachine();
                state.SetCurrentSubState();
            }
        }

        protected virtual void Awake()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
            capsuleColliderCenter_Default = capsuleCollider.center;
            capsuleHeight_Default = capsuleCollider.height;
            //Debug.Log("awake " + capsuleCollider.name);
            DisableNavMeshMovement();
        }


        protected virtual void FixedUpdate()
        {
            if (healthSystem.IsDead) return;
            if (!canFallAndLand) return;
            HandleFallingAndLanding();
        }

        public void SetPerformingComboAttacksStatus(bool value)
        {
            isPerformingComboAttacks = value;
        }


        protected virtual void LateUpdate()
        {
            if (healthSystem.IsDead) return;

            isInteracting = animator.GetBool("isInteracting");

            if (canFallAndLand)
            {
                animator.SetBool("isGrounded", isGrounded);
            }

            if (navMeshAgent != null && !navMeshAgent.updatePosition)
            {
                navMeshAgent.nextPosition = transform.position;
                navMeshAgent.transform.rotation = transform.rotation;
            }

        }

        public void LookAtPlayer(float rotationSpeed)
        {

            // Get direction to target (ignore Y-axis to prevent tilting)
            Vector3 direction = (targetTransform.position - transform.position).normalized;
            direction.y = 0; // Prevent vertical tilting

            // Smoothly rotate towards the player
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

        }

        public void LookAtTargetPoint(float rotationSpeed, Vector3 targetPointVector)
        {

            // Get direction to target (ignore Y-axis to prevent tilting)
            Vector3 direction = (targetPointVector - transform.position).normalized;
            direction.y = 0; // Prevent vertical tilting

            // Smoothly rotate towards the player
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

        }

        public void EnableComboChaining()
        {
            canChainCombo = true;
            //Debug.Log("<color=yellow>Combo chaining enabled</color>");
        }
    
        public void DisableComboChaining()
        {
            canChainCombo = false;
        }

        public void UpdateFixedComboChainAttacks()
        {
            dynamicComboAttackState.UpdateFixedComboChain();
        }

        public void UpdateFixedComboCanCombo()
        {
            dynamicComboAttackState.UpdateCanComboOnAnimExit();
        }

        public void RotateOnAttack(float rotationSpeed)
        {
            if (!canRotateWhileAttack) return;

            LookAtPlayer(rotationSpeed);
            //Debug.Log("asdadadadad");
        }

        #region NavMesh Utilities

        public void DisableNavMeshMovement()
        {
            if (navMeshAgent == null) return;


            navMeshAgent.isStopped = false;
            navMeshAgent.updateRotation = false;
            navMeshAgent.updatePosition = false;


        }


        public void SetNavMeshAgentVelocityToZero()
        {
            if (navMeshAgent == null) return;
            navMeshAgent.velocity = Vector3.zero;
        }

        public void SetNavMeshAgentDestination(Vector3 target)
        {
            if (navMeshAgent == null) return;
            navMeshAgent.SetDestination(target);
        }

        #endregion


        public void ResetMovementAnimatorValues()
        {
            animator.SetFloat("X_Velocity", 0f);
            animator.SetFloat("Z_Velocity", 0f);
        }

        public void SetMovementAnimatorValues(Vector3 Velocity)
        {
            float x_velocityVal;
            float z_velocityVal;

            // if (!isStrafing)
            // {
            //     x_velocityVal = Mathf.Clamp01(Mathf.Abs(Velocity.x));
            //     z_velocityVal = Mathf.Clamp01(Mathf.Abs(Velocity.z));
            // }
            // else
            // {
            //     x_velocityVal = Velocity.x;
            //     z_velocityVal = Velocity.z;
            // }

            x_velocityVal = Mathf.Clamp01(Mathf.Abs(Velocity.x));
            z_velocityVal = Mathf.Clamp01(Mathf.Abs(Velocity.z));

            animator.SetFloat("X_Velocity", x_velocityVal, 0.1f, Time.deltaTime);
            animator.SetFloat("Z_Velocity", z_velocityVal, 0.1f, Time.deltaTime);
        }

        public void SetStrafeAnimatorValues(direction direction)
        {
            switch (direction)
            {
                case direction.left:
                    SetStrafeAnimatorValues_Left();
                    break;
                case direction.right:
                    SetStrafeAnimatorValues_Right();
                    break;
                case direction.front:
                    SetStrafeAnimatorValues_Front();
                    break;
                case direction.back:
                    SetStrafeAnimatorValues_Back();
                    break;
            }
        }

        //call this after setting target strafe direction
        public void UpdateMoveDirection()
        {
            if (currentX == targetX && currentZ == targetZ) return;
            //currentX = animator.GetFloat("X_Velocity");
            //currentX = animator.GetFloat("Z_Velocity");

            currentX = Mathf.Lerp(currentX, targetX, Time.deltaTime * strafeBlendSpeed);
            currentZ = Mathf.Lerp(currentZ, targetZ, Time.deltaTime * strafeBlendSpeed);

            animator.SetFloat("X_Velocity", currentX);
            animator.SetFloat("Z_Velocity", currentZ);

            //Debug.Log($"<color=green>  Updating npc movement direction c = {currentX}{currentZ}, t = {targetX}{targetZ}");

        }

        public void SetStrafeAnimatorValues_Run()
        {
            // animator.SetFloat("X_Velocity", 0, 0.1f, Time.deltaTime);
            // animator.SetFloat("Z_Velocity", 1, 0.1f, Time.deltaTime);

            targetX = 0;
            targetZ = 1;
        }


        private void SetStrafeAnimatorValues_Left()
        {
            // animator.SetFloat("X_Velocity", -0.5f, 0.25f, Time.deltaTime);
            // animator.SetFloat("Z_Velocity", 0, 0.25f, Time.deltaTime);

            targetX = -0.5f;
            targetZ = 0;
        }

        private void SetStrafeAnimatorValues_Right()
        {
            //animator.SetFloat("X_Velocity", 0.5f, 0.25f, Time.deltaTime);
            //animator.SetFloat("Z_Velocity", 0, 0.25f, Time.deltaTime);

            targetX = 0.5f;
            targetZ = 0;
        }

        private void SetStrafeAnimatorValues_Front()
        {
            //animator.SetFloat("X_Velocity", 0, 0.25f, Time.deltaTime);
            //animator.SetFloat("Z_Velocity", 0.5f, 0.25f, Time.deltaTime);

            targetX = 0f;
            targetZ = 0.5f;
        }

        private void SetStrafeAnimatorValues_Back()
        {
            //animator.SetFloat("X_Velocity", 0, 0.25f, Time.deltaTime);
            //animator.SetFloat("Z_Velocity", -0.5f, 0.25f, Time.deltaTime);

            targetX = 0f;
            targetZ = -0.5f;
        }

        public void EnableEnemyCanvas()
        {
            enemyCanvasGroup.alpha = 1;
        }

        public void DisableEnemyCanvas()
        {
            enemyCanvasGroup.alpha = 0;
            enemyCanvasGroup.blocksRaycasts = false;
            enemyCanvasGroup.interactable = false;
        }

        public void EnableHitDetection()
        {
            canDetectHit = true;
        }

        public void DisableHitDetection()
        {
            canDetectHit = false;
            onHitDetectionEnd?.Invoke();
        }

        public void DisableCanKnockBackOnAttack()
        {
            canAttackKnockback = false;
        }


        public void EnableHAttackRotation()
        {
            canRotateWhileAttack = true;
        }

        public void DisableHAttackRotation()
        {
            canRotateWhileAttack = false;
        }

        public void DisableHitDetectionInDelay(float duration)
        {
            StartCoroutine(DisableHitDetectionAfterDelay(duration));
        }

        IEnumerator DisableHitDetectionAfterDelay(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            canDetectHit = false;
            onHitDetectionEnd?.Invoke();
            //Debug.Log("<color=yellow>hit detection disabled</color>");
        }

        public void EnableParryWindow()
        {
            parryable = true;
        }

        public void DisableParryWindow()
        {
            parryable = false;
        }

        public void SpawnProjectile()
        {
            if (projectileSpawner != null)
            {
                projectileSpawner.SpawnProjectile();
            }
        }

        public void SetDebugStateText(string stateName)
        {
            if (!debug) return; // If debug is not enabled, do not set the text
            if (debugStateText != null)
            {
                debugStateText.text = stateName;
            }
        }

        // public void TakeDamage(float damageAmount) // might need to seperate with health and damage logic
        // {
        //     if (isDead) return;

        //     //EnableDisableAttackBehaviour(false);
        //     DisableHitDetectionInDelay(0.75f);
        //     DisableHitDetectionInDelay(0.15f);
        //     DisableHitDetectionInDelay(0.1f);



        //     if(isStunned)
        //     {
        //         damageAmount *= stunDamageMultiplier;
        //     }

        //     health -= damageAmount;

        //     //animator.Play("Hit_left");
        //     PlayAnyActionAnimation("Hit_left");

        //     float animLength = getHitClip.length;
        //     StartCoroutine(EnableAttackBehaviourAfterDuration(animLength));
        //     KnockBack();

        //     if (health <= 0)
        //     {
        //         Debug.Log("Dead");

        //         PlayAnyActionAnimation("Falling_Back_Death");
        //         isDead = true;
        //     }
        // }

        public void PlayAnyActionAnimation(string animationName, bool isInteracting = false,float transitionDuration = 0.1f)
        {
            animator.SetBool("isInteracting", isInteracting);
            animator.CrossFade(animationName, transitionDuration);

        }

        public void PlayAnyActionAnimation(string animationName,int layer, bool isInteracting = false,float transitionDuration = 0.1f,float normalisedEntryTransitionTime = 0)
        {
            animator.SetBool("isInteracting", isInteracting);
            animator.CrossFadeInFixedTime(animationName,transitionDuration,layer,normalisedEntryTransitionTime);
        }

        void OnAnimatorMove()
        {
            if (animator == null) return;

            //HandleRootMotionUsage();

            //HandleHitDetectionOnTransitions();

            Vector3 animDeltaPosition = animator.deltaPosition;
            Quaternion animDeltaRotation = animator.deltaRotation;
            UpdateTurnRotation();

            if (inLeapAttack)
            {
                if (useModifiedLeapSpeed)
                {
                    animDeltaPosition.y *= mod_verticalLeapingSpeed;
                    animDeltaPosition.z *= mod_forwardLeapingSpeed;

                    transform.position += animDeltaPosition;

                    SetRigidbodyVelocityBasedOnAnimDelta(animDeltaPosition);
                }
                else
                {
                    animDeltaPosition.y *= verticalLeapingSpeed_Default;
                    animDeltaPosition.z *= forwardLeapingSpeed_Default;

                    transform.position += animDeltaPosition;

                    SetRigidbodyVelocityBasedOnAnimDelta(animDeltaPosition);
                }

                return;
            }

            if (!isChasingTarget)
            {

                if (isStrafing)
                {
                    Vector3 finalDeltaPos = animDeltaPosition * strafeSpeed;
                    transform.position += finalDeltaPos;

                    SetRigidbodyVelocityBasedOnAnimDelta(animDeltaPosition);
                }
                else if (isInteracting)
                {
                    transform.position += animDeltaPosition;
                    SetRigidbodyVelocityBasedOnAnimDelta(animDeltaPosition);
                }
                else
                {
                    //animDeltaPosition.y = 0f;
                    transform.position += animDeltaPosition;

                    SetRigidbodyVelocityBasedOnAnimDelta(animDeltaPosition);
                }


            }
            else if (navMeshAgent != null)
            {
                Vector3 navMeshPosition = navMeshAgent.nextPosition;

                //Vector3 chaseDirection = (navMeshPosition - transform.position).normalized;
                Vector3 chaseDirection = navMeshAgent.desiredVelocity.normalized;

                Vector3 moveDelta = chaseDirection * animDeltaPosition.magnitude;
                //Vector3 moveDelta =   animDeltaPosition;

                float moveSpeed = chaseSpeed;



                Vector3 finalDeltaPos = moveDelta * moveSpeed;

                transform.position += moveDelta * moveSpeed;

                SetRigidbodyVelocityBasedOnAnimDelta(animDeltaPosition);

                // Smoothly rotate towards the player
                if (chaseDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(chaseDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookRotationSpeed);

                }

            }




        }

        private void SetRigidbodyVelocityBasedOnAnimDelta(Vector3 animDeltaPosition)
        {
            Vector3 rbVelocity = animDeltaPosition / Time.fixedDeltaTime;// Calculate velocity from delta position
            rigidBody.linearVelocity = rbVelocity; // Set rigidbody velocity directly
        }

        public void SetAnimationSpeed(float speed)
        {
            if (animator == null) return;

            animator.speed = speed;
        }

        public void ResetAnimationSpeed()
        {
            if (animator == null) return;

            animator.speed = 1f;
        }

        public void UpdateTurnRotation()
        {

            if (isInteracting)
            {

                if (isTurning)
                {

                    Vector3 directionToTarget = targetTransform.position - transform.position;
                    directionToTarget.y = 0; // Ignore vertical component

                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookRotationSpeed);
                }

            }
        }


        //Call this to enable modified leap speed in animation event for leap attack Strat
        public void EnableModifiedLeapSpeed()
        {
            useModifiedLeapSpeed = true;
        }

        //Call this to disable modified leap speed in animation event for leap attack strat
        public void DisableModifiedLeapSpeed()
        {
            useModifiedLeapSpeed = false;
        }

        public void HandlePreAggressionDetection()
        {
            if (canGoHostile) return;
            if (customStartAnimationClip == null || preAggression_TransitionBoolString == "") return;
            DetectPlayerBeforeHostile();
        }

        private void DetectPlayerBeforeHostile()
        {
            if (!isPlayerInLineOfSight()) return;
            if (IsPlayerInRange_Sphere(transform.position, targetDetectionRadius_PreAggression))
            {
                animator.SetBool(preAggression_TransitionBoolString, true);
                float delay = customStartAnimationClip.length + preAggression_TransitionAdditionalDelay;
                StartCoroutine(EnableCanGoHostileDelayed(delay));
            }
        }

        IEnumerator EnableCanGoHostileDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            canGoHostile = true;
        }


        private void HandleRootMotionUsage()
        {
            if (isInteracting)
            {
                rigidBody.linearDamping = 0;
                Vector3 animDeltaPosition = animator.deltaPosition;
                animDeltaPosition.y = 0;
                Vector3 animTargetVelocity = animDeltaPosition / Time.deltaTime; // vel = changeinPos/ChangeinTime
                //animTargetVelocity.y = 0;
                rigidBody.linearVelocity = animTargetVelocity;
            }
        }

        public bool IsPlayerInRange_Capsule(Vector3 startPoint, Vector3 endPoint, float playerDetectionRadius)
        {
            Collider[] hitColliders = Physics.OverlapCapsule(startPoint, endPoint, playerDetectionRadius, playerLayerMask);

            if (hitColliders.Length == 0) return false;

            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    return true;
                }
            }

            return false;

        }

        public bool IsPlayerInRange_Sphere(Vector3 startPoint, float playerDetectionRadius)
        {
            Collider[] hitColliders = Physics.OverlapSphere(startPoint, playerDetectionRadius, playerLayerMask);

            if (hitColliders.Length == 0) return false;

            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    return true;
                }
            }

            return false;

        }

        public bool isPlayerInLineOfSight()
        {
            if (Physics.Linecast(lockOnTransform_Self.position, targetTransform.position + Vector3.up * .5f, obstacleLayerMask))
            {
                return false;
            }

            return true;


        }

        public bool isAnotherEnemyCloseToTargetBlockingLOS()
        {
            RaycastHit Hit;
            if (Physics.Linecast(lockOnTransform_Self.position, targetTransform.position + Vector3.up * .5f, out Hit, enemyLayerMask))
            {
                Collider enemyCollider = Hit.collider;

                if (enemyCollider != null)
                {
                    Collider[] enemiesAroundTarget = Physics.OverlapSphere(targetTransform.position, 1.25f, enemyLayerMask);

                    foreach (Collider enemy in enemiesAroundTarget)
                    {
                        if (enemy == enemyCollider)
                        {
                            //the blocking enemy is close to target
                            return true;
                        }
                    }

                    return false; //the blocking enemy is not close to target

                }

            }

            // No enemy blocking the line of sight
            return false;
        }

        // call this in animEvent at the end of all turning anims
        public void DisableTurnBoolean()
        {
            isTurning = false;

            // Vector3 directionToTarget = targetTransform.position - transform.position;
            // directionToTarget.y = 0; // Ignore vertical component

            // Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            // //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookRotationSpeed);
            // transform.rotation = targetRotation;
            SnapRotationAfterTurning();
        }

        public void TurnCharacter()
        {
            if (animator.IsInTransition(1)) return;
            Vector3 directionToTarget = targetTransform.position - transform.position;
            directionToTarget.y = 0; // Ignore vertical component

            float signedAngle = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);

            //Debug.Log("Signed Angle: " + signedAngle);

            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            targetRotationAtEndOfTurn = targetRotation;

            if (signedAngle > -45f && signedAngle < 45f)
            {
                // Do nothing, facing forward
                //Debug.Log("Signed Angle: " + signedAngle);
                return;
            }

            isTurning = true;

            if (signedAngle > 135f || signedAngle < -135f)
            {
                if (turnAnimRight_180 != null)
                {
                    PlayAnyActionAnimation(turnAnimRight_180.name, true);
                }

            }
            else

            if (signedAngle > 45f)
            {
                if (turnAnimRight_90 != null)
                {
                    PlayAnyActionAnimation(turnAnimRight_90.name, true);
                }

            }
            else if (signedAngle > 135f)
            {
                if (turnAnimRight_180 != null)
                {
                    PlayAnyActionAnimation(turnAnimRight_180.name, true);
                }

            }
            else if (signedAngle < -45f)
            {
                if (turnAnimLeft_90 != null)
                {
                    PlayAnyActionAnimation(turnAnimLeft_90.name, true);
                }

            }
            else if (signedAngle < -135f)
            {
                if (turnAnimLeft_180 != null)
                {
                    PlayAnyActionAnimation(turnAnimLeft_180.name, true);
                }

            }

            Debug.Log($"<color=red>TURNING</color>");

        }

        // call this in animEvent at the end of all turning anims
        public void SnapRotationAfterTurning()
        {
            transform.rotation = targetRotationAtEndOfTurn;
        }

        public void OnParried()
        {
            GetStunned();
        }

        public void DisableStun()
        {
            //animator.SetBool(stunTransitionBoolString, false);
            isStunned = false;
        }

        public void DisableStunAndStunAnimParam()
        {
            animator.SetBool(stunTransitionBoolString, false);
            isStunned = false;
        }

        private void GetStunned()
        {
            DisableHitDetection();
            DisableHitDetectionInDelay(0.15f);

            //dependent on string need to refactor
            //animator.Play("Empty State", 1); // to cancel ongoing animations in these two layers
            //animator.Play("Empty State", 2);

            isStunned = true;
            PlayAnyActionAnimation(stunAnimationClip.name, true);
            animator.SetBool(stunTransitionBoolString, true);
            StartCoroutine(DisableStunFlagStringAfterDelay(stunDuration));

        }

        IEnumerator DisableStunFlagStringAfterDelay(float delay)
        {
            DisableHitDetection();
            yield return new WaitForSeconds(delay);
            DisableHitDetection();
            DisableStunAndStunAnimParam();
            //Debug.Log("<color=red>Stun animation ended</color>");
        }

        public void DisableCOllider()
        {
            npcCollider.enabled = false;
            rigidBody.useGravity = false;

        }

        private void HandleFallingAndLanding()
        {
            RaycastHit hit;
            Vector3 raycastOrigin = transform.position;
            raycastOrigin.y = raycastOrigin.y + groundRaycastOffset_Vertical;
            raycastOrigin.x = raycastOrigin.x + groundRaycastOffset_Horizontal;
            raycastOrigin.z = raycastOrigin.z + groundRaycastOffset_Forward;

            groundedTargetPosition = transform.position;

            //Debug.DrawLine(raycastOrigin, raycastOrigin + Vector3.down * groundRaycastOffset, Color.cyan);
            // Debug.DrawRay(raycastOrigin, Vector3.down * groundRaycastOffset, Color.red);

            if (!isGrounded)
            {
                if (!isJumping && !inLeapAttack)
                {
                    if (!isInteracting)
                    {
                        if (fallAnimClip != null)
                        {
                            PlayAnyActionAnimation(fallAnimClip.name, true);
                        }


                    }

                    //animator.SetBool("isUsingRootMotion", false);
                    inAirTimer += Time.deltaTime;

                    rigidBody.AddForce(transform.forward * leapingVelocity);
                    rigidBody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);

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
                                //Debug.Log($"<color=green> ENEMY Fall Height DIFF Val = {fallHeightDifference_ABS}</color>");
                                if (fallHeightDifference_ABS > maxFallHeight)
                                {
                                    shouldDieAtLanding = true;
                                }
                                else
                                {
                                    //Debug.Log($"<color=green>ENEMY within Fall Height</color>");
                                    canInitiateVoidFallDamageDeathCheck = false;
                                }
                                canDoubleCheckFallDamageOnLanding = true;
                            }
                        }
                        else
                        {
                            canInitiateVoidFallDamageDeathCheck = true;
                            voidfallDistancerayEndPoint = fallDistancerayStart + Vector3.down * maxFallHeightCheckDistance;
                        }
                    }

                    if (canInitiateVoidFallDamageDeathCheck)
                    {
                        float heightDifference_ABS = Mathf.Abs(transform.position.y - voidfallDistancerayEndPoint.y);
                        //Debug.Log($"<color=yellow>Fall height diff = {heightDifference_ABS}</color>");
                        if (heightDifference_ABS <= 0.5f)
                        {
                            //kill self and disable
                            //Debug.Log($"<color=blue>ENEMY VOID FALL DEATH</color>");
                            
                            canInitiateVoidFallDamageDeathCheck = false;
                            canCheckFallDamageDistance = true;
                            onVoidFall?.Invoke();
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
            if (inLeapAttack) return;

            //if(inCoyoteTime) return;

            if (Physics.SphereCast(raycastOrigin, 0.3f, -Vector3.up, out hit, maxGroundCheckDistance, groundLayer))
            {
                if (!isGrounded && isInteracting)
                //if (playerAnimationManager.inAnimActionStatus)
                {
                    if (landAnimClip != null)
                    {
                        PlayAnyActionAnimation(landAnimClip.name, true);
                    }


                }

                Vector3 rayHitPoint = hit.point;
                groundedTargetPosition.y = rayHitPoint.y + verticalTargetPositionOffset;
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
                if (isInteracting)
                {
                    // if(isDodging && onStairs)
                    // {
                    //     playerTargetPosition.y = playerTargetPosition.y + verticalTargetPositionOffset;
                    // }


                    transform.position = Vector3.Lerp(transform.position, groundedTargetPosition, Time.deltaTime / 0.1f);

                }
                else
                {
                    transform.position = groundedTargetPosition;

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
               
                shouldDieAtLanding = false;
                canInitiateVoidFallDamageDeathCheck = false;
                onFallDeath?.Invoke();
            }
        }

        // private void HandleStairsManeuver()
        // {
        //     RaycastHit hit;
        //     Vector3 raycastOrigin = transform.position;
        //     raycastOrigin.y = raycastOrigin.y + groundRaycastOffset;

        //     targetPosition_OnStairs = transform.position;


        //     if (Physics.SphereCast(raycastOrigin, 0.2f, -Vector3.up, out hit, maxGroundCheckDistance, groundLayer))
        //     {

        //         Vector3 rayHitPoint = hit.point;
        //         targetPosition_OnStairs.y = rayHitPoint.y;
        //         //y_targetPosition_OnStairs = rayHitPoint.y;
        //         //Debug.Log("Ground hit: " + hit.collider.name);

        //     }
        //     // float smoothSpeed = 10f; // you can tweak this
        //     // Vector3 currentPosition = rigidBody.position;
        //     // Vector3 smoothed = new Vector3(
        //     //     currentPosition.x,
        //     //     Mathf.Lerp(currentPosition.y, targetPosition_OnStairs.y, Time.fixedDeltaTime * smoothSpeed),
        //     //     currentPosition.z
        //     // );

        //     // rigidBody.MovePosition(smoothed);

        //     if (isInteracting)
        //     {

        //         transform.position = Vector3.Lerp(transform.position, targetPosition_OnStairs, Time.deltaTime/0.1f);
        //         //transform.position = targetPosition_OnStairs;


        //     }
        //     else
        //     {
        //         transform.position = targetPosition_OnStairs;
        //     }

        //    //rigidBody.position = targetPosition_OnStairs;

        // }

        void OnCollisionEnter(Collision collision)
        {


            if (collision.gameObject.CompareTag("Player"))
            {
                //Debug.Log("<color=cyan>Collided with Player</color>");
                rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            }

            if (collision.gameObject.CompareTag("Enemy"))
            {
                //rigidBody.linearVelocity = Vector3.zero; // Stop the rigidbody from moving
                //Debug.Log("<color=yellow>Collided with Enemy</color>");
            }

            // if(collision.gameObject.tag == stairsTag)
            // {
            //     onStairs = true;
            //     ShrinkCollider();
            // }
            // else
            // {
            //     onStairs = false;
            //     ResizeCollider();
            // }
        }

        void OnCollisionExit(Collision collision)
        {


            if (collision.gameObject.CompareTag("Player"))
            {
                // Debug.Log("<color=cyan>Collided with exit Player</color>");
                rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            }




        }

        private void ShrinkCollider()
        {
            capsuleCollider.center = new Vector3(capsuleCollider.center.x, capsuleCollider.center.y + .35f, capsuleCollider.center.z);
            capsuleCollider.height = capsuleCollider.height * .75f;
        }

        private void ResizeCollider()
        {
            capsuleCollider.center = capsuleColliderCenter_Default;
            capsuleCollider.height = capsuleHeight_Default;
        }


        // private void OnGUI()
        // {
        //     GUIStyle gUIStyle = new GUIStyle();
        //     gUIStyle.fontSize = 30;
        //     gUIStyle.normal.textColor = Color.white;

        //     if(statemachine!=null && statemachine.currentState != null)
        //     {
        //         GUI.Label(new Rect(1500, 25, 500, 500), "Current State: " + statemachine.currentState.GetType().Name, gUIStyle);
        //     }
        //     else
        //     {
        //         GUI.Label(new Rect(1500, 25, 500, 500), "Current State: None", gUIStyle);
        //     }

        // }

        void OnDrawGizmos()
        {
            VisualiseGroundCheck();
        }
        public void VisualiseGroundCheck()
        {
            // Define the start position and direction
            Vector3 start = transform.position;
            start.y = start.y + groundRaycastOffset_Vertical;
            start.x = start.x + groundRaycastOffset_Horizontal;
            start.z = start.z + groundRaycastOffset_Forward;
            Vector3 direction = -Vector3.up;
            float radius = .3f;
            float maxDistance = maxGroundCheckDistance;

            // Set Gizmo color
            Gizmos.color = Color.cyan;

            // Draw the initial sphere at the raycast start point
            Gizmos.DrawWireSphere(start, radius);

            // If SphereCast hits something, draw the hit point and full cast path
            if (Physics.SphereCast(start, radius, direction, out RaycastHit hit, maxDistance, groundLayer))
            {
                Gizmos.color = Color.yellow;
                // Draw a line from start to hit point
                Gizmos.DrawLine(start, hit.point);

                // Draw sphere at hit point
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(hit.point, radius);
            }
            else
            {
                // Draw the full cast length if nothing was hit (limited to avoid infinite line)
                Gizmos.DrawRay(start, direction * maxDistance); // Adjust 5f as needed
            }
        }

        public void VisualiseDetectionCapsule(float maxDistance, float lockONDetectionRadius)

        {
            Vector3 capsuleStart = transform.position;
            Vector3 capsuleEnd = transform.position + transform.forward * maxDistance;

            Gizmos.color = Color.cyan;

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


    }
    
    [System.Serializable]
    public struct EnemySaveData
    {
        public bool isDead;

    }

}



