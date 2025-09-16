using UnityEngine;

namespace EternalKeep
{
    public class EffectsHandler : MonoBehaviour
    {
        [SerializeField] EffectData[] availableEffects;

        [SerializeField] float defaultEffectDuration = 1f;

        void Start()
        {
            foreach (EffectData effectData in availableEffects)
            {
                if (effectData.useFromPool)
                {
                    effectData.fxPoolListIndex = ImpactEffectManager_STN.instance.GetFXPoolListIndex(effectData.fxID);
                }
                
            }
        }

        public void SpawnEffect(int effectIndex)
        {
            EffectData effectData = availableEffects[effectIndex];

            float effectDuration = effectData.GetEffectDuration();
            if (effectDuration == 0f)
            {
                effectDuration = defaultEffectDuration;
            }

            if (!effectData.useFromPool)
            {
                GameObject effectInstance = Instantiate(effectData.effectPrefab, effectData.effectSpawnPoint.position,
                effectData.effectSpawnPoint.rotation);

                Destroy(effectInstance, effectDuration);
            }
            else
            {
                ImpactEffectManager_STN.instance.InitialiseFX(effectData.fxPoolListIndex, effectData.effectSpawnPoint.position,
                effectData.effectSpawnPoint.rotation, effectDuration);
            }

        }


    }

    [System.Serializable] 

    public class EffectData
    {
        public GameObject effectPrefab;

        public int fxID = -1;
        public int fxPoolListIndex = -1;

        public bool useFromPool = false;

        float duration;
        public float defaultduration;
        public bool useDefaultDuration = false;

        public Transform effectSpawnPoint;

        public float GetEffectDuration()
        {
            if(useDefaultDuration)
            {
                return defaultduration;
            }

            ParticleSystem particleSystem = effectPrefab.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                return particleSystem.main.startLifetime.constantMax;
            }
            

            return 0f;
        }   
    }


}



