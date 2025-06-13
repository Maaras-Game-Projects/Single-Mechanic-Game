using UnityEngine;

public class PersistentObjectSpawner : MonoBehaviour
{
    static bool hasPersistentObjectSpawned = false;
    public GameObject persistentObjectPrefab;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        if (!hasPersistentObjectSpawned)
        {
            SpawnPersistentObject();
            hasPersistentObjectSpawned = true;
        }
    }

    void SpawnPersistentObject()
    {
       GameObject persistentObject = Instantiate(persistentObjectPrefab);
       DontDestroyOnLoad(persistentObject);
    }


}
