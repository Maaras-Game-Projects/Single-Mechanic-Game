using UnityEngine;

namespace EternalKeep
{
    public class GameSaveData : MonoBehaviour
    {
        public static GameSaveData Instance { get; private set; }

        [SerializeField] float saveInterval = 120f; // Save every 2 minutes
        private float elapsedTime = 0f;

        [Space]
        [Header("DEBUG")]
        [SerializeField] bool canSave = true;
        public bool CanSave => canSave;

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

            SaveSystem.LoadGame();
        }

        void Update()
        {
            // if (Input.GetKeyDown(KeyCode.F5))
            // {
            //    SaveSystem.SaveGame();
            // }

            // if (Input.GetKeyDown(KeyCode.F6))
            // {
            //     SaveSystem.LoadGame();
            // }
            if (!canSave) return;

            elapsedTime += Time.deltaTime;
            if (elapsedTime >= saveInterval)
            {
                SaveSystem.SaveGame();
                elapsedTime = 0f; // Reset the timer after saving
            }
        }

        public PlayerManager playerManager;
        public PlayerHealth playerHealthManager;

        public HandlePlayerCheatMode playerGodModeManager;

        public PickUpItemDataContainer pickUpItemDataContainer;
        public EnemyDataContainer enemyDataContainer;


        void OnApplicationQuit()
        {
            if (!canSave) return;
            SaveSystem.SaveGame();
        }


    }


}





