using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Clown_E : NPC_Root, IDamagable, IEnemySavable, IEnemyReset
{

    [Header("Clown E Variables")]
    [SerializeField] private string transitionBool;
    [SerializeField] private float transitionWaitTime = 2f;
    [SerializeField] private float transitionChangeChance = 50f;
    [SerializeField] private AnimationClip damageClip;
    [SerializeField] private AnimationClip deathAnimClip;

    [SerializeField] private UnityEvent onDamageTaken;

    private float elapsedTime = 0f;
    [SerializeField] private int deathAnimationLayer;

    protected override void Awake()
    {
        base.Awake();
        // statemachine = new Statemachine();
        // statemachine.SetCurrentState(states[0]);
        // SetAllStates();
        // InitAllSubStatemachines();

        if (healthSystem.IsDead && CanEnemyRespawnAfterDeath) return;

        PlayAnyActionAnimation(startAnimationClip.name);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    // Update is called once per frame
    void Update()
    {

        if (healthSystem.IsDead) return;
        if (playerHealth.isPlayerDead) return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= transitionWaitTime)
        {
            RollTransitionChance();
            elapsedTime = 0f; // Reset the elapsed time
        }
    }



    protected override void LateUpdate()
    {

        base.LateUpdate();

    }

    void RollTransitionChance()
    {
        float randomValue = Random.Range(0f, 100f);
        if (randomValue <= transitionChangeChance)
        {
            animator.SetTrigger(transitionBool);
        }

    }

    public void TakeDamage(float damageAmount)
    {
        if (healthSystem.IsDead) return;

        healthSystem.DepleteHealth(damageAmount);

        PlayAnyActionAnimation(damageClip.name, true);

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



    public void TakeDamage(float damageAmount, float criticalDamage)
    {


        if (healthSystem.IsDead) return;


        if (IsStunned)
        {
            if (shieldSystem.ActiveShieldCount == 0)
            {
                healthSystem.DepleteHealth(criticalDamage);
                shieldSystem.BreakSheild();
                healthSystem.DisplayDamageTaken(damageAmount);
            }
            else
            {
                shieldSystem.BreakSheild();
            }

        }
        else
        {
            if (shieldSystem.ActiveShieldCount == 0)
            {
                healthSystem.DepleteHealth(damageAmount);
                shieldSystem.BreakSheild();
                healthSystem.DisplayDamageTaken(damageAmount);
            }
            else
            {
                shieldSystem.BreakSheild();
            }
        }


        onDamageTaken?.Invoke();

        //dependent on string need to refactor
        // animator.Play("Empty State",1); // to cancel ongoing animations in these two layers
        // animator.Play("Empty State",2);

        PlayAnyActionAnimation(damageClip.name, true);

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
            animator.Play(deathAnimClip.name, deathAnimationLayer, 1f);
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
        if(healthSystem.IsDead && !CanEnemyRespawnAfterDeath)
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
        canFallAndLand = false;
        isGrounded = true;
        isJumping = false;

        healthSystem.ResetHealthSystem();
        staminaSystem.ResetStamina();
        poiseSystem?.ResetPoise();

        //Reset State

        //Reset animation
        // animator.SetBool("isInteracting", false);
        // animator.SetBool("isStunned", false);
        // animator.Play("Empty State", 1);
        // animator.Play(startAnimationClip.name, 0); // Reset to idle animation

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


        //capsuleCollider.enabled = true;
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
