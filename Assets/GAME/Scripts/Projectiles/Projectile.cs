
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

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
        //[SerializeField] GameObject impactEffect;
        [SerializeField]int impactFxID = -1;
        [SerializeField]int impactFxPoolListIndex = -1;

        [SerializeField] Vector3 directionToTarget;
        [SerializeField] Transform targetTransform;

        [SerializeField] Rigidbody rigidBodySelf;

        float elapsedTime = 0f;

        bool canMove = false;

        Transform originTransform;
        private IObjectPool<Projectile> projectileObjectPool;



        private void Start()
        {
            //rb = GetComponent<Rigidbody>();
            //Invoke(nameof(SetFXPoolListIndex), 2f);
            SetFXPoolListIndex();
        }

        public void SetFXPoolListIndex()
        {
            impactFxPoolListIndex = ImpactEffectManager_STN.instance.GetFXPoolListIndex(impactFxID);
            Debug.Log("FX POOL INDEX = " + impactFxPoolListIndex);
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
            //Debug.Log($"<color=yellow>Projectile target set to: {target.name} and target transform = {targetTransform}</color>");
        }

        public void SetProjectileObjectPool(IObjectPool<Projectile> objectPool)
        {
            projectileObjectPool = objectPool;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            rigidBodySelf.position = position;
            //Debug.Log("Projectile POsition SET = " + position);
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
            rigidBodySelf.rotation = rotation;
        }

        public void SetCanMove(bool value)
        {
            canMove = value;
        }
        // public void SetImpactEffectActivation(bool value)
        // {
        //     impactEffect.SetActive(value);
        // }

        public void SetOriginTransform(Transform transform)
        {
            originTransform = transform;
            transform.SetParent(originTransform);
            transform.position = Vector3.zero;

            transform.rotation = Quaternion.identity;
        }



        private void Update()
        {
            #region OLD CODE

            // elapsedTime += Time.deltaTime;
            // if (elapsedTime >= lifetime)
            // {
            //     HandleProjectileDeath();
            //     return;
            // }

            // if (targetTransform != null && elapsedTime <= trackingTime)
            // {
            //     //directionToTarget = (targetTransform.position - transform.position).normalized;
            //     Vector3 targetDirection = (targetTransform.position - transform.position).normalized;
            //     directionToTarget = Vector3.Lerp(directionToTarget, targetDirection, Time.deltaTime * trackSpeed);

            //     //rigidbody.rotation = Quaternion.LookRotation(targetDirection);
            //     //rigidbody.linearVelocity = directionToTarget * speed * Time.deltaTime;

            // }

            // Vector3 velocity = directionToTarget * speed;

            // transform.position += velocity * Time.deltaTime;

            // if(velocity != Vector3.zero)
            // {
            //     // Rotate the projectile to face the direction of movement
            //     Quaternion targetRotation = Quaternion.LookRotation(velocity);
            //     transform.rotation = targetRotation * Quaternion.Euler(90, 0, 0); // Adjust the rotation if needed
            // }

            // //transform.Translate(transform.forward * speed * Time.deltaTime);
            // //transform.position += directionToTarget * speed * Time.deltaTime;
            // //rigidbody.linearVelocity = directionToTarget * speed * Time.deltaTime;

            #endregion

            //if (!canMove) return;

            if (targetTransform != null && elapsedTime <= trackingTime)
            {
                //directionToTarget = (targetTransform.position - transform.position).normalized;
                Vector3 targetDirection = (targetTransform.position - transform.position).normalized;
                directionToTarget = Vector3.Lerp(directionToTarget, targetDirection, Time.deltaTime * trackSpeed);

                //rigidbody.rotation = Quaternion.LookRotation(targetDirection);
                //rigidbody.linearVelocity = directionToTarget * speed * Time.deltaTime;

            }

            if (rigidBodySelf.linearVelocity != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(rigidBodySelf.linearVelocity) * Quaternion.Euler(90f, 0f, 0f);
            }
        }


        private void FixedUpdate()
        {
            //if (!canMove) return;

            elapsedTime += Time.fixedDeltaTime;
            if (elapsedTime >= lifetime)
            {
                HandleProjectileDeath();
                return;
            }

            // if (targetTransform != null && elapsedTime <= trackingTime)
            // {
            //     //directionToTarget = (targetTransform.position - transform.position).normalized;
            //     Vector3 targetDirection = (targetTransform.position - transform.position).normalized;
            //     directionToTarget = Vector3.Lerp(directionToTarget, targetDirection, Time.deltaTime * trackSpeed);

            //     //rigidbody.rotation = Quaternion.LookRotation(targetDirection);
            //     //rigidbody.linearVelocity = directionToTarget * speed * Time.deltaTime;

            // }

            Vector3 velocity = directionToTarget * speed;

            rigidBodySelf.linearVelocity = velocity;

            // if(velocity != Vector3.zero)
            // {
            //     // Rotate the projectile to face the direction of movement
            //     Quaternion targetRotation = Quaternion.LookRotation(velocity);
            //     transform.rotation = targetRotation * Quaternion.Euler(90, 0, 0); // Adjust the rotation if needed
            // }


            // if (rb.linearVelocity != Vector3.zero)
            // {
            //     transform.rotation = Quaternion.LookRotation(rb.linearVelocity) * Quaternion.Euler(90f, 0f, 0f);
            // }
        }

        private void OnTriggerEnter(Collider other)
        {

            if (other.CompareTag("Player"))
            {
                // Apply damage to the player
                bool isBlockable = IsTargetFacingTowardsMe();
                other.GetComponent<PlayerHealth>().TakeDamage(damage,isBlockable);

                HandleProjectileDeath();
            }
            else if (canHitAndDieOtherThanTarget && !other.CompareTag("Player Weapon") && !other.CompareTag("Enemy")
                && !other.CompareTag("EnemyObject"))
            {
                HandleProjectileDeath();
                Debug.Log($"<color=yellow>Projectile collided with: {other.name}</color>");
            }
        }
        
        private bool IsTargetFacingTowardsMe()
        {
            Vector3 targetsForwardVector = targetTransform.forward.normalized;
            Vector3 directionVectorFromTarget = (transform.position - targetTransform.position).normalized;

            float dotProduct = Vector3.Dot(targetsForwardVector, directionVectorFromTarget);

            if (dotProduct >= 0.2f)
            {
                //Debug.Log($"<color=yellow>FACING ME");
                return true;
            }
            else
            {
                //Debug.Log($"<color=red>NOT FACING ME");
                return false;
            }
                
        }

        private void HandleProjectileDeath()
        {
            // Instantiate explosion effect
            //GameObject impactObj = Instantiate(impactEffectPrefab, transform.position, transform.rotation);

            // Destroy the projectile
            //Destroy(gameObject);

            elapsedTime = 0f;
            //Destroy(impactObj, impactEffectLifetime);
            //StartCoroutine(ShowAndHideImpactEffect(impactEffectLifetime));
            ImpactEffectManager_STN.instance.InitialiseFX(impactFxPoolListIndex, transform.position, transform.rotation, impactEffectLifetime);

            //transform.position = Vector3.zero;
            //transform.rotation = Quaternion.identity;
            SetCanMove(false);
            projectileObjectPool.Release(this);
        }

        // IEnumerator ShowAndHideImpactEffect(float lifetime)
        // {
        //     impactEffect.SetActive(true);
        //     Debug.Log("IMPACT HIT ");
        //     yield return new WaitForSeconds(lifetime);
        //     Debug.Log("IMPACT OFF ");
        //     impactEffect.SetActive(false);
        // }
    }

}


