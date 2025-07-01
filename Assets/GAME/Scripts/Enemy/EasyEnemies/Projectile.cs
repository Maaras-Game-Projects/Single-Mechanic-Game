using UnityEngine;

namespace EternalKeep
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float speed = 10f;
        [SerializeField] float damage = 10f;
        [SerializeField] float lifetime = 5f;
        [SerializeField] float impactEffectLifetime = 1f;
        [SerializeField] float trackingTime = 2f;
        [SerializeField] float trackSpeed = 5f;

        // remove Rigidbody if u dont want to hit objects other than the target
        [SerializeField] bool canHitAndDieOtherThanTarget = true;

        [SerializeField] GameObject impactEffectPrefab;

        [SerializeField] Vector3 directionToTarget;
        [SerializeField] Transform targetTransform;

        Rigidbody rb;

        float elapsedTime = 0f;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void SetDirectionTowardsTarget(Vector3 targetPostion)
        {
            directionToTarget = (targetPostion - transform.position).normalized;
            //rb.linearVelocity = directionToTarget * speed;
            transform.rotation = Quaternion.LookRotation(directionToTarget) * Quaternion.Euler(90f, 0f, 0f);
        }

        public void SetTarget(Transform target)
        {
            targetTransform = target;
        }

        // private void Update()
        // {

        //     elapsedTime += Time.deltaTime;
        //     if (elapsedTime >= lifetime)
        //     {
        //         HandleProjectileDeath();
        //         return;
        //     }

        //     if (targetTransform != null && elapsedTime <= trackingTime)
        //     {
        //         //directionToTarget = (targetTransform.position - transform.position).normalized;
        //         Vector3 targetDirection = (targetTransform.position - transform.position).normalized;
        //         directionToTarget = Vector3.Lerp(directionToTarget, targetDirection, Time.deltaTime * trackSpeed);

        //         //rigidbody.rotation = Quaternion.LookRotation(targetDirection);
        //         //rigidbody.linearVelocity = directionToTarget * speed * Time.deltaTime;

        //     }

        //     Vector3 velocity = directionToTarget * speed;

        //     transform.position += velocity * Time.deltaTime;

        //     if(velocity != Vector3.zero)
        //     {
        //         // Rotate the projectile to face the direction of movement
        //         Quaternion targetRotation = Quaternion.LookRotation(velocity);
        //         transform.rotation = targetRotation * Quaternion.Euler(90, 0, 0); // Adjust the rotation if needed
        //     }

        //     //transform.Translate(transform.forward * speed * Time.deltaTime);
        //     //transform.position += directionToTarget * speed * Time.deltaTime;
        //     //rigidbody.linearVelocity = directionToTarget * speed * Time.deltaTime;


        // }

        private void FixedUpdate()
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

                //rigidbody.rotation = Quaternion.LookRotation(targetDirection);
                //rigidbody.linearVelocity = directionToTarget * speed * Time.deltaTime;

            }

            Vector3 velocity = directionToTarget * speed;

            rb.linearVelocity = velocity;

            // if(velocity != Vector3.zero)
            // {
            //     // Rotate the projectile to face the direction of movement
            //     Quaternion targetRotation = Quaternion.LookRotation(velocity);
            //     transform.rotation = targetRotation * Quaternion.Euler(90, 0, 0); // Adjust the rotation if needed
            // }


            if (rb.linearVelocity != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(rb.linearVelocity) * Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Debug.Log($"<color=red>Projectile collided with: {other.name}");
            if (other.CompareTag("Player"))
            {
                // Apply damage to the player
                other.GetComponent<PlayerHealth>().TakeDamage(damage);

                HandleProjectileDeath();
            }
            else if (canHitAndDieOtherThanTarget && !other.CompareTag("Player Weapon") && !other.CompareTag("Enemy")
                && !other.CompareTag("EnemyObject"))
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

            Destroy(impactObj, impactEffectLifetime);
        }
    }

}


