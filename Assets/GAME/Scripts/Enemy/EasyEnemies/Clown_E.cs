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
    

    void Awake()
    {
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
        if (isDead) return;

        health -= damageAmount;

        //animator.Play("Hit_left");
        PlayAnyActionAnimation(damageClip.name);

        if (health <= 0)
        {
           // Debug.Log("Dead");

            PlayAnyActionAnimation(deathAnimClip.name);
            isDead = true;

            float animLength = deathAnimClip.length;
            StartCoroutine(DisableEnemyColliderAFterDelay(animLength));
        }
    }

    IEnumerator DisableEnemyColliderAFterDelay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        DisableCOllider();
    }


}
