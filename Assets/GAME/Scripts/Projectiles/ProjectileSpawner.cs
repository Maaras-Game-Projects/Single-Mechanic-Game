using System.Collections;
using UnityEngine;

namespace EternalKeep
{
    public class ProjectileSpawner : MonoBehaviour
    {
        [SerializeField] Transform projectileSpawnPoint;
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] int projectileID;
        [SerializeField] Transform playerTransform;

        [SerializeField] float spawnCount = 1f;
        [SerializeField] float spawnIntervalWhileMultiSpawn = 0.5f;

        int projectilePoolListIndex = -1;

        bool isSpawnTimerOn = false;

        void Start()
        {
            projectilePoolListIndex = ProjectileManager_STN.instance.GetProjectilePoolListIndex(projectileID);
        }

        void Update()
        {
            //Debug
            // if (Input.GetKeyDown(KeyCode.P))
            // {
            //     // For testing purposes, pressing 'P' will spawn the projectile
            //     SpawnProjectile();
            // }
        }

        public void SpawnProjectile()
        {

            if (spawnCount == 1f)
            {
                //SpawnAndSetProjectileDirectionAndTarget();
                ProjectileManager_STN.instance.InitialiseProjectile(projectilePoolListIndex, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

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
            GameObject projectileObject = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetTarget(playerTransform);
                projectile.SetDirectionTowardsTarget(playerTransform.position);
                
            }
        }

        private IEnumerator SpawnProjectilesCoroutine()
        {
            for (int i = 0; i < spawnCount; i++)
            {
                //SpawnAndSetProjectileDirectionAndTarget();
                ProjectileManager_STN.instance.InitialiseProjectile(projectilePoolListIndex, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
                yield return new WaitForSeconds(spawnIntervalWhileMultiSpawn);
            }
            isSpawnTimerOn = false;
        }
    }

}

