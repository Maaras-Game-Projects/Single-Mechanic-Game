using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveSystem
{
    public static SaveData saveData = new SaveData();

    

    public static string GetSaveFilePath()
    {
        return Application.persistentDataPath + "/EkSave.ek";
    }

    public static void SaveGame()
    {
        if (!GameSaveData.Instance.CanSave) return; //Debug
        HandleSaveData();

        File.WriteAllText(GetSaveFilePath(), JsonUtility.ToJson(saveData, true));
        Debug.Log($"<color=green>Game Saved To </color>" + GetSaveFilePath());
    }

    public static void HandleSaveData()
    {
        GameSaveData.Instance.playerManager.SavePlayerPositionData(ref saveData.playerPositionData);
        GameSaveData.Instance.playerHealthManager.SavePlayerHealthData(ref saveData.playerHealthData);

        saveData.itemListData.itemPickUpDataList = new ItemPickUpData[GameSaveData.Instance.pickUpItemDataContainer.GetItemPickUps.Length];

        for (int i = 0; i < GameSaveData.Instance.pickUpItemDataContainer.GetItemPickUps.Length; i++)
        {
            GameSaveData.Instance.pickUpItemDataContainer.GetItemPickUps[i].SaveItemData(ref saveData.itemListData.itemPickUpDataList[i]);
        }

        saveData.enemyListData.enemySaveDataList = new EnemySaveData[GameSaveData.Instance.enemyDataContainer.GetEnemies.Length];

        for (int i = 0; i < GameSaveData.Instance.enemyDataContainer.GetEnemies.Length; i++)
        {
            IEnemySavable enemySavable = GameSaveData.Instance.enemyDataContainer.GetEnemies[i].
                GetComponent<IEnemySavable>();
            if (enemySavable != null)
            {
                enemySavable.SaveEnemy(ref saveData.enemyListData.enemySaveDataList[i]);
            }
        }
    }

    public static void LoadGame()
    {
        if (!GameSaveData.Instance.CanSave) return; //Debug
        if (!File.Exists(GetSaveFilePath()))
        {
            Debug.LogWarning($"<color=red>Save file not found at </color>{GetSaveFilePath()}");
            return;
        }
        string saveDataString = File.ReadAllText(GetSaveFilePath());
        saveData = JsonUtility.FromJson<SaveData>(saveDataString);
        HandleLoadData();
        Debug.Log($"<color=yellow>Game Loaded From </color>" + GetSaveFilePath());
    }

    public static void HandleLoadData()
    {
        GameSaveData.Instance.playerManager.LoadPlayerPositionData(saveData.playerPositionData);
        GameSaveData.Instance.playerHealthManager.LoadPlayerHealthData(saveData.playerHealthData);

        //saveData.itemListData.itemPickUpDataList = new ItemPickUpData[GameSaveData.Instance.pickUpItemDataContainer.GetItemPickUps.Length];

        if (saveData.itemListData.itemPickUpDataList != null)
        {
            for (int i = 0; i < GameSaveData.Instance.pickUpItemDataContainer.GetItemPickUps.Length; i++)
            {
                GameSaveData.Instance.pickUpItemDataContainer.GetItemPickUps[i].LoadItemPickUpData(saveData.itemListData.itemPickUpDataList[i]);
            }
        }

        if (saveData.enemyListData.enemySaveDataList != null)
        {
            for (int i = 0; i < GameSaveData.Instance.enemyDataContainer.GetEnemies.Length; i++)
            {
                IEnemySavable enemySavable = GameSaveData.Instance.enemyDataContainer.GetEnemies[i].
                    GetComponent<IEnemySavable>();
                if (enemySavable != null)
                {
                    enemySavable.LoadEnemy(saveData.enemyListData.enemySaveDataList[i]);
                }
            }
        }
        
    }

    public static void ResetSave()
    {
        if (!GameSaveData.Instance.CanSave) return; //Debug
        HandleSaveDataReset();

        File.WriteAllText(GetSaveFilePath(), JsonUtility.ToJson(saveData, true));
        Debug.Log($"<color=white>Game Reset And saved to </color>" + GetSaveFilePath());
    }

    public static void HandleSaveDataReset()
    {
        GameSaveData.Instance.playerManager.ResetPlayerPositionSaveData(ref saveData.playerPositionData);
        GameSaveData.Instance.playerHealthManager.ResetPlayerHealthDataSaves(ref saveData.playerHealthData);

        saveData.itemListData.itemPickUpDataList = new ItemPickUpData[GameSaveData.Instance.pickUpItemDataContainer.GetItemPickUps.Length];

        for (int i = 0; i < GameSaveData.Instance.pickUpItemDataContainer.GetItemPickUps.Length; i++)
        {
            GameSaveData.Instance.pickUpItemDataContainer.GetItemPickUps[i].ResetItemPickUpDataSaves(ref saveData.itemListData.itemPickUpDataList[i]);
        }

        saveData.enemyListData.enemySaveDataList = new EnemySaveData[GameSaveData.Instance.enemyDataContainer.GetEnemies.Length];

        for (int i = 0; i < GameSaveData.Instance.enemyDataContainer.GetEnemies.Length; i++)
        {
            IEnemySavable enemySavable = GameSaveData.Instance.enemyDataContainer.GetEnemies[i].
                GetComponent<IEnemySavable>();
            if (enemySavable != null)
            {
                enemySavable.ResetEnemySave(ref saveData.enemyListData.enemySaveDataList[i]);
            }
        }
    }




}

[System.Serializable]
public struct SaveData
{
    public PlayerPositionData playerPositionData;
    public PlayerHealthData playerHealthData;
    public itemListData itemListData;
    public EnemyListData enemyListData;
}

[System.Serializable]
public struct itemListData
{
    public ItemPickUpData[] itemPickUpDataList;
}

[System.Serializable]
public struct EnemyListData
{
    public EnemySaveData[] enemySaveDataList;
}

