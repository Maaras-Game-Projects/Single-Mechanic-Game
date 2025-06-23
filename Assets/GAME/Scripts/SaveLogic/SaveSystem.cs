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
        Debug.Log("Game saved to " + GetSaveFilePath());
    }

    public static void HandleSaveData()
    {
        GameSaveData.Instance.playerLocomotion.SavePlayerLocomotionData(ref saveData.playerLocomotionData);
    }

    public static void LoadGame()
    {
        string saveDataString = File.ReadAllText(GetSaveFilePath());
        saveData = JsonUtility.FromJson<SaveData>(saveDataString);
        HandleLoadData();
        Debug.Log("Game loaded from " + GetSaveFilePath());
    }

    public static void HandleLoadData()
    {
        GameSaveData.Instance.playerLocomotion.LoadPlayerLocomotionData(saveData.playerLocomotionData);
    }




}

[System.Serializable]
public struct SaveData
{
    public PlayerLocomotionData playerLocomotionData;
}
