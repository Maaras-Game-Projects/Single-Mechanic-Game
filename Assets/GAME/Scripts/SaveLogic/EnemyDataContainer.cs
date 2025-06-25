using UnityEngine;

public class EnemyDataContainer : MonoBehaviour
{

    [SerializeField]  GameObject[] enemies;
    
    public GameObject[] GetEnemies
    {
        get { return enemies; }
    }
}
