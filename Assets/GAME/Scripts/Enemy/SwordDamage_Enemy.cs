using UnityEngine;

public class SwordDamage_Enemy : MonoBehaviour
{
    [SerializeField] float baseDamageVal = 50f;
    [SerializeField] BaseEnemy baseEnemy;

    private void OnTriggerEnter(Collider other)
    {
        
        if (!baseEnemy.canDetectHit) return;

        if (other == null)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<Collider>().GetComponent<PlayerHealth>();

        if (playerHealth == null) return;

        //Debug.Log("Got IDAmagable");

        playerHealth.TakeDamage(baseDamageVal,baseEnemy.parryable);
    }
}
