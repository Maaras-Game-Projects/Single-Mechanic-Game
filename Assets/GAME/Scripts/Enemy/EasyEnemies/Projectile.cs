using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float damage = 10f;
    [SerializeField] float lifetime = 5f;
    [SerializeField] float trackingTime = 2f;
    [SerializeField] float trackSpeed = 5f;

    // remove Rigidbody if u dont want to hit objects other than the target
    [SerializeField] bool canHitAndDieOtherThanTarget = true;

    [SerializeField] GameObject impactEffectPrefab;

    [SerializeField] Vector3 directionToTarget;
    [SerializeField] Transform targetTransform;

    float elapsedTime = 0f;

    private void Start()
    {

    }

    public void SetDirectionTowardsTarget(Vector3 targetPostion)
    {
        directionToTarget = (targetPostion - transform.position).normalized;
        Debug.Log("Direction to target: " + directionToTarget);
    }

    public void SetTarget(Transform target)
    {
        targetTransform = target;
    }

    private void Update()
    {
        
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= lifetime)
        {
            HandleProjectileDeath();
            return;
        }

        if (targetTransform != null && elapsedTime <= trackingTime)
        {
            //directionToTarget = (targetTransform.position - transform.position).normalized;
            Vector3 targetDirection = (targetTransform.position - transform.position).normalized;
            directionToTarget = Vector3.Lerp(directionToTarget, targetDirection, Time.deltaTime * trackSpeed);
        }

        //transform.Translate(transform.forward * speed * Time.deltaTime);
        transform.position += directionToTarget* speed * Time.deltaTime;

        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Apply damage to the player
            other.GetComponent<PlayerHealth>().TakeDamage(damage);

            HandleProjectileDeath();
        }
        else if(canHitAndDieOtherThanTarget && !other.CompareTag("Enemy"))
        {
            HandleProjectileDeath();
        }
    }

    private void HandleProjectileDeath()
    {
        // Instantiate explosion effect
        GameObject impactObj = Instantiate(impactEffectPrefab, transform.position, transform.rotation);

        // Destroy the projectile
        Destroy(gameObject);

        Destroy(impactObj, 1f);
    }
}
