using System.Collections.Generic;
using UnityEngine;

namespace EternalKeep
{
    [CreateAssetMenu(fileName = "ImpactEffectsContainer", menuName = "ScriptableObjects/ImpactEffects")]
    public class ImpactEffect_SO : ScriptableObject
    {
        public List<ImpactEffectData> allImpactEffects = new List<ImpactEffectData>();
    }

    [System.Serializable]

    public class ImpactEffectData
    {
        public int iD = -1;
        public GameObject impactEffectPrefab;
    }

    
}

