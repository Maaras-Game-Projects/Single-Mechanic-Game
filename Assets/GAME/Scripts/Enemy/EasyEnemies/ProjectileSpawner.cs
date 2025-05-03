using System.Collections;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform playerTransform;

    [SerializeField] float spawnCount = 1f;
    [SerializeField] float spawnIntervalWhileMultiSpawn = 0.5f;

    bool isSpawnTimerOn = false;

    

    public void SpawnProjectile()
    {
    
       if(spawnCount == 1f)
        {
            SpawnAndSetProjectileDirectionAndTarget();

            return;
        }
        else
       {
            if (!isSpawnTimerOn)
            {
                isSpawnTimerOn = true;
                StartCoroutine(SpawnProjectilesCoroutine());
            }
       }
    }

    private void SpawnAndSetProjectileDirectionAndTarget()
    {
        Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

        Projectile projectile = projectilePrefab.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.SetDirectionTowardsTarget(playerTransform.position);
            projectile.SetTarget(playerTransform);
        }
    }

    private IEnumerator SpawnProjectilesCoroutine()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnAndSetProjectileDirectionAndTarget();
            yield return new WaitForSeconds(spawnIntervalWhileMultiSpawn);
        }
        isSpawnTimerOn = false;
    }
}
