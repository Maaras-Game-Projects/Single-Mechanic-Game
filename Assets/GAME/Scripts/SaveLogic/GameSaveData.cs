using UnityEngine;

public class GameSaveData : MonoBehaviour
{
    public static GameSaveData Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
           SaveSystem.SaveGame();
        }
        
        if (Input.GetKeyDown(KeyCode.F6))
        {
            SaveSystem.LoadGame();
        }
    }

    public PlayerLocomotion playerLocomotion;


}


