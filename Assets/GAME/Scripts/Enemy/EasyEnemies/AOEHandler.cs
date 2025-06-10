using UnityEngine;

public class AOEHandler : MonoBehaviour
{
    [SerializeField] Transform originTransform;
    [SerializeField] float radius = 5f;
    [SerializeField] float damage = 10f;
    [SerializeField] float selfDamage = 0f;

    [SerializeField] NPC_Root npcRootOFAttackingNPC;
    [SerializeField] RootEnemy rootEnemyOFAttackingNPC;
    [SerializeField] LayerMask targetLayerMask;

    [SerializeField] GameObject aoeEffectPrefab;

    public void PerformAOEAttack()
    {
        Collider[] hitTargets = Physics.OverlapSphere(originTransform.position, radius, targetLayerMask);
        foreach (Collider target in hitTargets)
        {
            if (target.gameObject == originTransform.gameObject) continue;

            IDamagable damagableTarget = target.GetComponent<IDamagable>();
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            playerHealth?.TakeDamage(damage, false, npcRootOFAttackingNPC);
            damagableTarget?.TakeDamage(damage);
            rootEnemyOFAttackingNPC.TakeDamage(selfDamage);
        }

        
        GameObject aoeEffectInstance = Instantiate(aoeEffectPrefab, originTransform.position, originTransform.rotation);
        ParticleSystem aoeEffect = aoeEffectPrefab.GetComponent<ParticleSystem>();
        if(aoeEffect!= null)
        { 
            float destroyTime = aoeEffect.main.duration;
            Destroy(aoeEffectInstance, destroyTime);
        }
        else
        {
            Destroy(aoeEffectInstance, 2f);
        }
       
    }
    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(originTransform.position, radius);
    // }
}
