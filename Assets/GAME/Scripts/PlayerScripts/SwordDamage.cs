using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    [SerializeField] float baseDamgeVal = 50f;
    [SerializeField] float criticalDamage = 50f;
    [SerializeField] float AttackPower = 50f;

    [SerializeField] float attackScaleFactor = 0.4f;
    [SerializeField] float criticalAttackScaleFactor = 1.5f;
    [SerializeField] float swordRotVal_X_AtDeath = 50f;
    [SerializeField] PlayerCombat playerCombat;

    Collider swordCollider;
    private void Start()
    {
        swordCollider = GetComponent<Collider>();

        AttackPower = baseDamgeVal + Mathf.Round(baseDamgeVal * attackScaleFactor); 
        criticalDamage = baseDamgeVal + Mathf.Round(baseDamgeVal * criticalAttackScaleFactor); 
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
        if (!playerCombat.canDetectHit) return;


        if (other == null)
        {
            

            return;
        }
        
        IDamagable damagable = other.GetComponent<Collider>().GetComponent<IDamagable>();

        if (damagable == null) return;
        
        damagable.TakeDamage(AttackPower,criticalDamage);
        

    }
    
}
