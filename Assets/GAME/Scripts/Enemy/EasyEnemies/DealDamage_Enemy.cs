using UnityEngine;

public class DealDamage_Enemy : MonoBehaviour
{
    [SerializeField] float baseDamageVal = 50f;
    [SerializeField] NPC_Root nPC_Root;

    private void OnTriggerEnter(Collider other)
    {
        if(nPC_Root == null) return;
        if (!nPC_Root.canDetectHit) return;

        if (other == null)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<Collider>().GetComponent<PlayerHealth>();

        if (playerHealth == null) return;

        //Debug.Log("Got IDAmagable");

        playerHealth.TakeDamage(nPC_Root.currentDamageToDeal,nPC_Root.parryable,nPC_Root);
    }
}
