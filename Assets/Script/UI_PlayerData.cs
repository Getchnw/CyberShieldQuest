using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UI_PlayerData : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI goldText;

    void Start()
    {
        LoadAndDisplayPlayerData();
    }

    private void OnEnable()
    {
        // Refresh UI when this object becomes enabled
        LoadAndDisplayPlayerData();
        // Also listen for scene load so HUD updates after scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadAndDisplayPlayerData()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameData != null)
        {
            UpdateUI(GameManager.Instance.CurrentGameData);
        }
        else
        {
            Debug.LogWarning("GameManager or game data not found!");
        }
    }

    private void UpdateUI(GameData gameData)
    {
        if (playerNameText != null)
            playerNameText.text = $"{gameData.profile.playerName}";
        
        if (levelText != null)
            levelText.text = $"LV.{gameData.profile.level}";
        
        if (goldText != null)
            goldText.text = $"{gameData.profile.gold}";
    }

    // เมธอดสำหรับอัพเดทข้อมูลแบบเรียลไทม์
    public void UpdatePlayerData(int level, string playerName, int gold)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CurrentGameData.profile.level = level;
            GameManager.Instance.CurrentGameData.profile.playerName = playerName;
            GameManager.Instance.CurrentGameData.profile.gold = gold;
            GameManager.Instance.SaveCurrentGame();
            UpdateUI(GameManager.Instance.CurrentGameData);
        }
    }

    // // เมธอดสำหรับเพิ่ม/ลดทอง
    // public void UpdateGold(int amount)
    // {
    //     if (GameManager.Instance != null)
    //     {
    //         GameManager.Instance.CurrentGameData.gold += amount;
    //         GameManager.Instance.SaveCurrentGame();
    //         UpdateUI(GameManager.Instance.CurrentGameData);
    //     }
    // }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When a new scene loads, ensure UI is displaying current data
        LoadAndDisplayPlayerData();
    }
}