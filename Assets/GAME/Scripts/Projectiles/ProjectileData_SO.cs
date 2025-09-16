using System.Collections.Generic;
using UnityEngine;

namespace EternalKeep
{
    [CreateAssetMenu(fileName = "ProjectileDataContainer", menuName = "ScriptableObjects/ProjectileData")]
   // [CreateAssetMenu(fileName = "PlayerOGData", menuName = "ScriptableObjects/PlayerOriginalData")]
    public class ProjectileData_SO : ScriptableObject
    {
        public List<ProjectileData> allProjectiles = new List<ProjectileData>();
    }

    [System.Serializable]

    public class ProjectileData
    {
        public int iD = -1;
        public GameObject projectilePrefab;
    }

}

