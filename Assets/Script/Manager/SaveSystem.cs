using UnityEngine;

public static class SaveSystem
{
    private static readonly string SAVE_KEY = "gameData";

    public static void SaveGameData(GameData gameData)
    {
        try
        {
            string jsonData = JsonUtility.ToJson(gameData, true);
            PlayerPrefs.SetString(SAVE_KEY, jsonData);
            PlayerPrefs.Save();
            Debug.Log("Game data saved successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game data: {e.Message}");
        }
    }

    public static GameData LoadGameData()
    {
        try
        {
            if (SaveFileExists())
            {
                string jsonData = PlayerPrefs.GetString(SAVE_KEY);
                GameData gameData = JsonUtility.FromJson<GameData>(jsonData);
                Debug.Log("Game data loaded successfully");
                return gameData;
            }
            else
            {
                Debug.LogWarning("No save data found!");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game data: {e.Message}");
            return null;
        }
    }

    public static bool SaveFileExists()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    public static void DeleteSaveData()
    {
        try
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            Debug.Log("Save data deleted successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to delete save data: {e.Message}");
        }
    }
}