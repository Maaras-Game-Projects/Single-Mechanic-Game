using UnityEngine;

public class Clown_E : NPC_Root, IDamagable
{

    [Header("Clown E Variables")]
    [SerializeField] private string transitionBool;
    [SerializeField] private float transitionWaitTime = 2f;
    [SerializeField] private float transitionChangeChance = 50f;

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
        PlayAnyActionAnimation("Hit_Front");

        if (health <= 0)
        {
           // Debug.Log("Dead");

            PlayAnyActionAnimation("Zombie_Dying");
            isDead = true;
        }
    }
}
