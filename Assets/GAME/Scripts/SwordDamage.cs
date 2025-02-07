using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    [SerializeField] float baseDamgeVal = 50f;
    [SerializeField] PlayerCombat playerCombat;

   /* private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("col enter chek");
        if (!playerCombat.canDetectHit) return;

        if (collision == null)
        {
            Debug.Log("Sword Not Collided");
            return;
        }

        Debug.Log("Sword Collided");

        IDamagable damagable = collision.collider.GetComponent<IDamagable>();

        if (damagable == null) return;

        Debug.Log("Got IDAmagable");

        damagable.TakeDamage(baseDamgeVal);
    }*/

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("col enter chek");
        if (!playerCombat.canDetectHit) return;

        if (other == null)
        {
            Debug.Log("Sword Not Collided");
            return;
        }

        Debug.Log("Sword Collided");

        IDamagable damagable = other.GetComponent<Collider>().GetComponent<IDamagable>();

        if (damagable == null) return;

        Debug.Log("Got IDAmagable");

        damagable.TakeDamage(baseDamgeVal);
    }
    
}
