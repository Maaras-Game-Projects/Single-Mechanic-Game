using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace EternalKeep
{
    public class RootEnemy : NPC_Root, IDamagable, IEnemySavable, IEnemyReset
    {
        [Header("Clown E V2 Variables")]
        [SerializeField] private AnimationClip damageClip;
        [SerializeField] private AnimationClip sheildbreakClip;
        [SerializeField] private AnimationClip deathAnimClip;
        [SerializeField] private UnityEvent onDamageTaken;
        [SerializeField] private UnityEvent onShieldBroken;

        [SerializeField] int deathAAnimationLayer = 3;

        protected override void Awake()
        {
            base.Awake();
            statemachine = new Statemachine();
            statemachine.SetCurrentState(states[0]);
            SetAllStates();
            InitAllSubStatemachines();
        }

        // public override void SetAllStates()
        // {
        //     base.SetAllStates();
        //     foreach (State state in states)
        //     {
        //         state.AssignRootTOState(this);
        //     }
        // }

        void OnEnable()
        {
            onVoidFall.AddListener(() => DeathByVoidFall());
            onFallDeath.AddListener(() => DeathByLandFall());
        }

        void OnDisable()
        {
            onVoidFall.RemoveListener(() => DeathByVoidFall());
            onFallDeath.RemoveListener(() => DeathByLandFall());
        }

        void Start()
        {
            statemachine.currentState?.OnEnter();
        }


        // Update is called once per frame
        void Update()
        {

            if (healthSystem.IsDead) return;
            idleState.FallBackToDefaultStateOnPlayerDeath();
            //if (playerHealth.isPlayerDead) return;


            if (statemachine.currentState != null)
            {
                statemachine.currentState?.TickLogic_All();

            }

        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (healthSystem.IsDead) return;
            if (playerHealth.isPlayerDead) return;

            if (statemachine.currentState != null)
            {
                statemachine.currentState?.TickPhysics_All();

            }

        }

        public void TakeDamage(float damageAmount, float criticalDamage)
        {
            if (healthSystem.IsDead) return;

            DisableHitDetection();


            if (IsStunned)
            {
                // If the enemy is stunned, take critical damage, break all shields, and deplete poise
                
                healthSystem.DepleteHealth(criticalDamage);
                healthSystem.DisplayDamageTaken(criticalDamage);
                shieldSystem.BreakAllShields();
                poiseSystem.DepletePoise(criticalDamage);
                //Debug.Log("Enemy is stunned, taking critical damage: " + criticalDamage);
                CancelOtherLayerAnims();

                PlayAnyActionAnimation(damageClip.name, true);

                DisableStunAndStunAnimParam();
                statemachine.SwitchState(states[0]);
                onDamageTaken?.Invoke();
                onShieldBroken?.Invoke();

            }
            else
            {
                if (shieldSystem.ActiveShieldCount == 0)
                {
                    healthSystem.DepleteHealth(damageAmount);
                    healthSystem.DisplayDamageTaken(damageAmount);
                    shieldSystem.BreakSheild();
                    poiseSystem.DepletePoise(damageAmount);

                    if (poiseSystem.CurrentPoise <= 0)
                    {
                        CancelOtherLayerAnims();
                        PlayAnyActionAnimation(damageClip.name, true);
                        statemachine.SwitchState(states[0]);
                    }

                    onDamageTaken?.Invoke();
                }
                else
                {
                    shieldSystem.BreakSheild();

                    CancelOtherLayerAnims();

                    PlayAnyActionAnimation(sheildbreakClip.name, true);
                    statemachine.SwitchState(states[0]);
                    onShieldBroken?.Invoke();
                }
            }

            if (poiseSystem.CurrentPoise <= 0)
            {
                poiseSystem.ResetPoise();
            }



            if (healthSystem.CheckForDeath())
            {
                DisableEnemyCanvas();
                PlayAnyActionAnimation(deathAnimClip.name, true);
                float animLength = deathAnimClip.length;

                SaveSystem.SaveGame();

                if (CanEnemyRespawnAfterDeath)
                {
                    StartCoroutine(DisableEnemyColliderAFterDelay(animLength));
                }
                else
                {
                    // add sound and death vfx

                    StartCoroutine(DisableEnemyObjectAFterDelay(animLength));

                }
            }

        }



        IEnumerator DisableEnemyColliderAFterDelay(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            DisableCOllider();
        }

        IEnumerator DisableEnemyObjectAFterDelay(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            gameObject.SetActive(false);
        }

        void OnDrawGizmos()
        {

            // stare radius
            //VisualiseDetectionCapsule(6f, 2f);


            // chase radius
            //VisualiseDetectionCapsule(12, 10f);

            // // combat radius
            // Gizmos.color = Color.red;
            // Gizmos.DrawWireSphere(transform.position, 7.5f);

            // // combat offset radius
            // Gizmos.color = Color.green;
            // Gizmos.DrawWireSphere(transform.position, 6.5f);

            // //closeRange radius
            // Gizmos.color = Color.red;
            // Gizmos.DrawWireSphere(transform.position, 1.5f);

            // //BackoffRange radius
            // Gizmos.color = Color.yellow;
            // Gizmos.DrawWireSphere(transform.position, 3f);

            // //MidRange radius
            // Gizmos.color = Color.blue;
            // Gizmos.DrawWireSphere(transform.position, 5.5f);

            // //LongRange radius
            // Gizmos.color = Color.green;
            // Gizmos.DrawWireSphere(transform.position, 7f);

            // if (navMeshAgent != null)
            // {
            //     Gizmos.color = Color.red;
            //     Gizmos.DrawSphere(transform.position, .5f); // GameObject position

            //     Gizmos.color = Color.green;
            //     Gizmos.DrawSphere(navMeshAgent.nextPosition, .5f); // Agent position
            // }

            VisualiseGroundCheck();

        }

        public void DeathByVoidFall()
        {
            if (healthSystem.IsDead) return;

            DisableHitDetection();

            float dmg = healthSystem.MaxHealth * 5f;
            healthSystem.DepleteHealth(dmg);
            healthSystem.DisplayDamageTaken(dmg);
            shieldSystem.BreakAllShields();
            poiseSystem.DepletePoise(dmg);

            CancelOtherLayerAnims();

            DisableStunAndStunAnimParam();
            onDamageTaken?.Invoke();
            onShieldBroken?.Invoke();


            if (poiseSystem.CurrentPoise <= 0)
            {
                poiseSystem.ResetPoise();
            }



            if (healthSystem.CheckForDeath())
            {
                DisableEnemyCanvas();
                PlayAnyActionAnimation(deathAnimClip.name, true);
                float animLength = deathAnimClip.length;

                SaveSystem.SaveGame();

                if (CanEnemyRespawnAfterDeath)
                {
                    StartCoroutine(DisableEnemyColliderAFterDelay(animLength));
                }
                else
                {
                    // add sound and death vfx

                    StartCoroutine(DisableEnemyObjectAFterDelay(animLength));

                }

                StartCoroutine(DisableEnemyObjectAFterDelay(animLength));
            }

        }

        private void DeathByLandFall()
        {
            if (healthSystem.IsDead) return;

            DisableHitDetection();

            float dmg = healthSystem.MaxHealth * 5f;
            healthSystem.DepleteHealth(dmg);
            healthSystem.DisplayDamageTaken(dmg);
            shieldSystem.BreakAllShields();
            poiseSystem.DepletePoise(dmg);

            CancelOtherLayerAnims();

            DisableStunAndStunAnimParam();
            onDamageTaken?.Invoke();
            onShieldBroken?.Invoke();


            if (poiseSystem.CurrentPoise <= 0)
            {
                poiseSystem.ResetPoise();
            }



            if (healthSystem.CheckForDeath())
            {
                DisableEnemyCanvas();
                PlayAnyActionAnimation(deathAnimClip.name, true);
                float animLength = deathAnimClip.length;

                SaveSystem.SaveGame();

                if (CanEnemyRespawnAfterDeath)
                {
                    StartCoroutine(DisableEnemyColliderAFterDelay(animLength));
                }
                else
                {
                    // add sound and death vfx

                    StartCoroutine(DisableEnemyObjectAFterDelay(animLength));

                }

                //StartCoroutine(DisableEnemyObjectAFterDelay(animLength));
            }

        }

        public void TakeDamage(float damageAmount)
        {
            if (healthSystem.IsDead) return;
            if (damageAmount <= 0) return;

            DisableHitDetection();

            if (shieldSystem.ActiveShieldCount == 0)
            {
                healthSystem.DepleteHealth(damageAmount);
                shieldSystem.BreakSheild();
                healthSystem.DisplayDamageTaken(damageAmount);
                CancelOtherLayerAnims();

                PlayAnyActionAnimation(damageClip.name, true);
                statemachine.SwitchState(states[0]);
                onDamageTaken?.Invoke();
            }
            else
            {
                shieldSystem.BreakSheild();
                CancelOtherLayerAnims();

                PlayAnyActionAnimation(sheildbreakClip.name, true);
                statemachine.SwitchState(states[0]);
                onShieldBroken?.Invoke();
            }



            if (healthSystem.CheckForDeath())
            {
                DisableEnemyCanvas();
                PlayAnyActionAnimation(deathAnimClip.name, true);
                float animLength = deathAnimClip.length;
                SaveSystem.SaveGame();
                if (CanEnemyRespawnAfterDeath)
                {
                    StartCoroutine(DisableEnemyColliderAFterDelay(animLength));
                }
                else
                {
                    // add sound and death vfx

                    StartCoroutine(DisableEnemyObjectAFterDelay(animLength));

                }

            }
        }

        private void CancelOtherLayerAnims()
        {
            //dependent on string need to refactor
            animator.Play("Empty State", 1); // to cancel ongoing animations in these two layers
            animator.Play("Empty State", 2);
        }


        #region SAVE/LOAD
        public void SaveEnemy(ref EnemySaveData enemySaveData)
        {
            enemySaveData.isDead = healthSystem.CheckForDeath();
        }

        public void LoadEnemy(EnemySaveData enemySaveData)
        {
            if (enemySaveData.isDead)
            {
                healthSystem.SetEnemyDead();
                animator.Play(deathAnimClip.name, deathAAnimationLayer, 1f);
                DisableEnemyCanvas();
                DisableCOllider();

                if (!CanEnemyRespawnAfterDeath)
                {
                    gameObject.SetActive(false);
                }


            }
        }

        public void ResetEnemySave(ref EnemySaveData enemySaveData)
        {
            if (healthSystem.IsDead && !CanEnemyRespawnAfterDeath)
            {
                enemySaveData.isDead = true; // Keep the enemy as dead in save data if it cannot respawn
                return;
            }

            enemySaveData.isDead = false; // Reset the enemy to not dead in save data

        }

        #endregion

        public void ResetEnemy()
        {
            if (healthSystem.IsDead && !CanEnemyRespawnAfterDeath)
            {
                gameObject.SetActive(false);
                DisableEnemyCanvas();
                return; // Do not reset if the enemy is dead and cannot respawn
            }

            canAttackKnockback = false;
            isInteracting = false;
            canRotateWhileAttack = false;
            canDetectHit = false;
            parryable = false;
            isStunned = false;
            isChasingTarget = false;
            isStrafing = false;
            inLeapAttack = false;
            useModifiedLeapSpeed = false;
            canFallAndLand = true;
            isGrounded = true;
            isJumping = false;

            healthSystem.ResetHealthSystem();
            staminaSystem.ResetStamina();
            poiseSystem?.ResetPoise();
            SetPerformingComboAttacksStatus(false);
            DisableComboChaining();

            //Reset State
            foreach (State state in states)
            {
                IEnemyStateReset resettableState = state.gameObject.GetComponent<IEnemyStateReset>();
                resettableState?.ResetEnemyState();
            }
            statemachine.SwitchState(states[0]); // Switch to the first state in the list

            //Reset animation
            if (gameObject.activeSelf)
            {
                animator.SetBool("isInteracting", false);
                animator.SetBool("isStunned", false);
                animator.Play("Empty State", 3);
                animator.Play(startAnimationClip.name, 0); // Reset to idle animation 
            }


            //Reset Position and Rotation
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
            rigidBody.transform.position = spawnPoint.position;
            rigidBody.transform.rotation = spawnPoint.rotation;

            if (navMeshAgent != null)
            {
                navMeshAgent.nextPosition = spawnPoint.position;
                navMeshAgent.transform.rotation = spawnPoint.rotation;
            }

            if (gameObject.activeSelf)
            {
                capsuleCollider.enabled = true;
            }
        }

        public void ResetEnemyDelayed(float delay)
        {
            StartCoroutine(ResetEnemyDelayedCoroutine(delay));
        }

        IEnumerator ResetEnemyDelayedCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            ResetEnemy();
        }

    }

}


