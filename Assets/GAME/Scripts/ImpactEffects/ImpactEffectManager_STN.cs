using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace EternalKeep
{
    //this manager is used when effects are to be used across many enemies and/or player
    public class ImpactEffectManager_STN : MonoBehaviour
    {
        public static ImpactEffectManager_STN instance;

        private List<ImpactEffectsPoolData> allFXPoolData;

        [SerializeField] ImpactEffect_SO impactFX_SO;

        [SerializeField] Transform fxPoolsParent;


        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitialiseAllFXPoolData();
            InitialiseAllFXPools();

        }

        public int GetFXPoolListIndex(int projectileID)
        {
            int index = -1;
            for (int i = 0; i < allFXPoolData.Count; i++)
            {
                if (allFXPoolData[i].iD == projectileID)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private void InitialiseAllFXPools()
        {
            for (int i = 0; i < allFXPoolData.Count; i++)
            {
                GameObject fX_Prefab = allFXPoolData[i].effectPrefab;
                InitialiseProjectileObjectPool(fX_Prefab, i);
            }
        }

        private void InitialiseAllFXPoolData()
        {
            allFXPoolData = new List<ImpactEffectsPoolData>(impactFX_SO.allImpactEffects.Count);

            for (int i = 0; i < impactFX_SO.allImpactEffects.Count; i++)
            {
                ImpactEffectsPoolData fXPoolData = new ImpactEffectsPoolData();
                allFXPoolData.Add(fXPoolData);
            }

            for (int i = 0; i < impactFX_SO.allImpactEffects.Count; i++)
            {
                ImpactEffectsPoolData fXPoolData = allFXPoolData[i];
                ImpactEffectData fXData = impactFX_SO.allImpactEffects[i];
                fXPoolData.iD = fXData.iD;
                fXPoolData.effectPrefab = fXData.impactEffectPrefab;
            }
        }

        private void InitialiseProjectileObjectPool(GameObject fXPrefab, int poolIndex)
        {
            allFXPoolData[poolIndex].fxPool = new ObjectPool<BaseFX>(() => CreateFXObject(fXPrefab, poolIndex),
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

        private BaseFX CreateFXObject(GameObject fxPrefab, int poolIndex)
        {
            GameObject fxObject = Instantiate(fxPrefab);

            BaseFX baseFX = fxObject.GetComponent<BaseFX>();
            // if (projectile != null)
            // {
            //     projectile.SetTarget(playerTransform);
            //     projectile.SetDirectionTowardsTarget(playerTransform.position);

            // }
            baseFX.SetFXObjectPool(allFXPoolData[poolIndex].fxPool);
            baseFX.transform.SetParent(fxPoolsParent);
            return baseFX;
        }

        public void InitialiseFX(int fxPoolListIndex, Vector3 spawnPosition, Quaternion spawnRotation, float fxLifeTime)
        {
            BaseFX baseFXObject = allFXPoolData[fxPoolListIndex].fxPool.Get();
            baseFXObject.SetPosition(spawnPosition);
            baseFXObject.SetRotation(spawnRotation);
            baseFXObject.SetLifetime(fxLifeTime);
            baseFXObject.HideFXAfterLifetimeOver();
           
        }

    }

    [System.Serializable]

    public class ImpactEffectsPoolData
    {
        public int iD;
        public GameObject effectPrefab;
        public IObjectPool<BaseFX> fxPool;
    }


}

