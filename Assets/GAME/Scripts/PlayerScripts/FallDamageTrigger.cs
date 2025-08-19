
using UnityEngine;

namespace EternalKeep
{
    public class FallDamageTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();

                if (playerHealth != null)
                {
                    playerHealth.DieByVOIDFallDamage();
                    Debug.Log("DEATH BY FALL DAMAGE TRIGGER");
                }
        }
        else if (other.gameObject.tag == "Enemy")
        {
            RootEnemy rootEnemy = other.gameObject.GetComponent<RootEnemy>();

            if (rootEnemy != null)
            {
                rootEnemy.DeathByVoidFall();
            }
        }
    }
}


}
