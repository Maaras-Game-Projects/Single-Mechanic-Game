using UnityEngine;

namespace EternalKeep
{
    public class EffectsHandler : MonoBehaviour
    {
        [SerializeField] EffectData[] availableEffects;

        [SerializeField] float defaultEffectDuration = 1f;


        public void SpawnEffect(int effectIndex)
        {
            EffectData effectData = availableEffects[effectIndex];
            GameObject effectInstance = Instantiate(effectData.effectPrefab, effectData.effectSpawnPoint.position,
                effectData.effectSpawnPoint.rotation);

            float effectDuration = effectData.GetEffectDuration();
            if (effectDuration == 0f)
            {
                effectDuration = defaultEffectDuration;
            }
            Destroy(effectInstance, effectDuration);
        }


    }

    [System.Serializable] 

    public class EffectData
    {
        public GameObject effectPrefab;
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



