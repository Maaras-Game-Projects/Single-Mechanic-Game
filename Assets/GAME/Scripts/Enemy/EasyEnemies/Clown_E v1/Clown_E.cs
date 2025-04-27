using System.Collections;
using UnityEngine;

public class Clown_E : NPC_Root, IDamagable
{

    [Header("Clown E Variables")]
    [SerializeField] private string transitionBool;
    [SerializeField] private float transitionWaitTime = 2f;
    [SerializeField] private float transitionChangeChance = 50f; 
    [SerializeField] private AnimationClip damageClip;
    [SerializeField] private AnimationClip deathAnimClip; 

    private float elapsedTime = 0f;
    

    protected override void  Awake()
    {
        base.Awake();
        // statemachine = new Statemachine();
        // statemachine.SetCurrentState(states[0]);
        // SetAllStates();
        // InitAllSubStatemachines();
    }

    // Update is called once per frame
    void Update()
    {
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

        PlayAnyActionAnimation(damageClip.name,true);

        if(healthSystem.CheckForDeath())
        {
            PlayAnyActionAnimation(deathAnimClip.name,true);
            float animLength = deathAnimClip.length;
            StartCoroutine(DisableEnemyColliderAFterDelay(animLength));
        }
    }

    IEnumerator DisableEnemyColliderAFterDelay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        DisableCOllider();
    }

    public void TakeDamage(float damageAmount, float criticalDamage)
    {
        throw new System.NotImplementedException();
    }
}
