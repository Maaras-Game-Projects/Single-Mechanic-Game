using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace EternalKeep
{
    public class ProjectileManager_STN : MonoBehaviour
    {
        public static ProjectileManager_STN instance;

        private List<ProjectilePoolData> allProjectilePoolData;

        [SerializeField] ProjectileData_SO projectileData_SO;
        [SerializeField]private Transform playerTransform;
        [SerializeField]private Transform projectilePoolParent;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitialiseAllProjectilePoolData();
            InitialiseAllProjectilePools();

        }

        public int GetProjectilePoolListIndex(int projectileID)
        {
            int index = -1;
            for (int i = 0; i < allProjectilePoolData.Count; i++)
            {
                if (allProjectilePoolData[i].iD == projectileID)
                {
                    index = i;
                    break;
                }
            }
            Debug.Log($"<color=green>Projectile List Index = {index}");
            return index;
        }

        private void InitialiseAllProjectilePools()
        {
            for (int i = 0; i < allProjectilePoolData.Count; i++)
            {
                GameObject projectilePrefab = allProjectilePoolData[i].projectilePrefab;
                InitialiseProjectileObjectPool(projectilePrefab, i);
            }
        }

        private void InitialiseAllProjectilePoolData()
        {
            allProjectilePoolData = new List<ProjectilePoolData>(projectileData_SO.allProjectiles.Count);
            
            for (int i = 0; i < projectileData_SO.allProjectiles.Count; i++)
            {
                ProjectilePoolData projectilePoolData = new ProjectilePoolData();
                allProjectilePoolData.Add(projectilePoolData);
            }

            for (int i = 0; i < projectileData_SO.allProjectiles.Count; i++)
            {
                ProjectilePoolData projectilePoolData = allProjectilePoolData[i];
                ProjectileData projectileData = projectileData_SO.allProjectiles[i];
                projectilePoolData.iD = projectileData.iD;
                projectilePoolData.projectilePrefab = projectileData.projectilePrefab;
            }
        }

        private void InitialiseProjectileObjectPool(GameObject projectilePreb,int poolIndex)
        {
            allProjectilePoolData[poolIndex].projectilePool = new ObjectPool<Projectile>(() => CreateProjectile(projectilePreb,poolIndex),
                    actionOnGet: obj => obj.gameObject.SetActive(true),
                    actionOnRelease: obj => obj.gameObject.SetActive(false),
                    actionOnDestroy: obj => Destroy(obj),
                    defaultCapacity: 25,
                    maxSize: 40
                    );
        }

        // private void EnableProjectile(Projectile proj)
        // {
        //     proj.SetOriginTransform
        // }

        private Projectile CreateProjectile(GameObject projectilePrefab, int poolIndex)
        {
            GameObject projectileObject = Instantiate(projectilePrefab);

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            // if (projectile != null)
            // {
            //     projectile.SetTarget(playerTransform);
            //     projectile.SetDirectionTowardsTarget(playerTransform.position);

            // }
            projectile.SetProjectileObjectPool(allProjectilePoolData[poolIndex].projectilePool);
            projectile.transform.SetParent(projectilePoolParent);
            return projectile;
        }

        public void InitialiseProjectile(int projectilePoolListIndex, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            Projectile projectileObject = allProjectilePoolData[projectilePoolListIndex].projectilePool.Get();
            projectileObject.SetPosition(spawnPosition);
            projectileObject.SetRotation(spawnRotation);
            //projectileObject.SetImpactEffectActivation(false);
            //projectileObject.SetCanMove(true);
            projectileObject.SetTarget(playerTransform);
            projectileObject.SetDirectionTowardsTarget(playerTransform.position);
        }

    }

    [Serializable]

    public class ProjectilePoolData
    {
        public int iD;
        public GameObject projectilePrefab;
        public IObjectPool<Projectile> projectilePool;
    }
    

}
