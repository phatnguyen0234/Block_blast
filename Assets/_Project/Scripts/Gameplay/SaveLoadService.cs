using UnityEngine;

public static class SaveLoadService
{
    private const string SaveKey = "block_puzzle_save";

    public static void Save(GameSaveData saveData)
    {
        if (saveData == null)
            return;

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static bool TryLoad(out GameSaveData saveData)
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            saveData = null;
            return false;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            saveData = null;
            return false;
        }

        saveData = JsonUtility.FromJson<GameSaveData>(json);
        return saveData != null;
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(SaveKey);
    }
}
