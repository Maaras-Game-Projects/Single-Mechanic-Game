using UnityEngine;
using System.IO;

public class SaveSystem
{
    public static SaveData saveData = new SaveData();

    

    public static string GetSaveFilePath()
    {
        return Application.persistentDataPath + "/EkSave.ek";
    }

    public static void SaveGame()
    {
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
    }

    public static void LoadGame()
    {
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

        Debug.Log("allpickups count = "+GameSaveData.Instance.pickUpItemDataContainer.GetItemPickUps.Length);
        for (int i = 0; i < GameSaveData.Instance.pickUpItemDataContainer.GetItemPickUps.Length; i++)
        {
            GameSaveData.Instance.pickUpItemDataContainer.GetItemPickUps[i].LoadItemPickUpData(saveData.itemListData.itemPickUpDataList[i]);
        }
    }

    public static void ResetSave()
    {
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
    }




}

[System.Serializable]
public struct SaveData
{
    public PlayerPositionData playerPositionData;
    public PlayerHealthData playerHealthData;
    public itemListData itemListData;
}

[System.Serializable]
public struct itemListData
{
    public ItemPickUpData[] itemPickUpDataList;
}
