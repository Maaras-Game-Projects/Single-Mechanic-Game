namespace EternalKeep
{
    public interface IEnemySavable
    {
        void SaveEnemy(ref EnemySaveData enemySaveData);
        void LoadEnemy(EnemySaveData enemySaveData);

        void ResetEnemySave(ref EnemySaveData enemySaveData);
    }
}

