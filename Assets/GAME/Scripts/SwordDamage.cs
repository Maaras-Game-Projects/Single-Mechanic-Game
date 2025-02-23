using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    [SerializeField] float baseDamgeVal = 50f;
    [SerializeField] float swordRotVal_X_AtDeath = 50f;
    [SerializeField] PlayerCombat playerCombat;

    Collider swordCollider;
    private void Start()
    {
        swordCollider = GetComponent<Collider>();
    }

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

    public void SetSwordRotationValue(float x,float y, float z)
    {
        transform.localRotation = Quaternion.Euler(x,y,z);
    }
    
    public void SetSwordRotationValueAtPlayerDeath()
    {
        Vector3 currentRotation = transform.localEulerAngles;
        currentRotation.x = swordRotVal_X_AtDeath;
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void EnableDisableSwordCollider(bool value)
    {
        if (swordCollider == null) return;
        swordCollider.enabled = value;
    }

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
