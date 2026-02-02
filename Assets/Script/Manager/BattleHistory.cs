using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

/// <summary>
/// จัดการประวัติการเล่น Battle ทั้งหมด - บันทึก/โหลด/วิเคราะห์
/// </summary>
public class BattleHistory : MonoBehaviour
{
    public static BattleHistory Instance { get; private set; }

    [Header("Battle History")]
    public List<BattleStatistics> allBattles = new List<BattleStatistics>();
    
    [Header("Settings")]
    public int maxHistorySize = 100; // เก็บสูงสุด 100 เกม
    public bool autoSaveOnBattleEnd = true; // บันทึกอัตโนมัติหลังจบเกม

    private string savePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSavePath();
            LoadHistory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeSavePath()
    {
        savePath = Path.Combine(Application.persistentDataPath, "battle_history.json");
        Debug.Log($"📁 Battle History Path: {savePath}");
    }

    /// <summary>เพิ่มผลการเล่นใหม่</summary>
    public void AddBattleResult(BattleStatistics stats)
    {
        if (stats == null) return;

        allBattles.Add(stats);

        // จำกัดขนาดประวัติ (เก็บแค่เกมล่าสุด)
        if (allBattles.Count > maxHistorySize)
        {
            allBattles.RemoveAt(0); // ลบเกมเก่าสุด
        }

        Debug.Log($"📊 Battle #{allBattles.Count} added to history");

        if (autoSaveOnBattleEnd)
        {
            SaveHistory();
        }
    }

    /// <summary>บันทึกประวัติลง JSON</summary>
    public void SaveHistory()
    {
        try
        {
            BattleHistoryData data = new BattleHistoryData { battles = allBattles };
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"💾 Saved {allBattles.Count} battles to: {savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Failed to save battle history: {e.Message}");
        }
    }

    /// <summary>โหลดประวัติจาก JSON</summary>
    public void LoadHistory()
    {
        try
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                BattleHistoryData data = JsonUtility.FromJson<BattleHistoryData>(json);
                
                if (data != null && data.battles != null)
                {
                    allBattles = data.battles;
                    Debug.Log($"📂 Loaded {allBattles.Count} battles from history");
                }
            }
            else
            {
                Debug.Log("📂 No battle history found - starting fresh");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Failed to load battle history: {e.Message}");
        }
    }

    /// <summary>ล้างประวัติทั้งหมด</summary>
    public void ClearHistory()
    {
        allBattles.Clear();
        SaveHistory();
        Debug.Log("🗑️ Battle history cleared");
    }

    // ========================================================
    // 📊 STATISTICS & ANALYSIS
    // ========================================================

    /// <summary>จำนวนเกมที่เล่นทั้งหมด</summary>
    public int GetTotalBattles() => allBattles.Count;

    /// <summary>จำนวนเกมที่ชนะ</summary>
    public int GetTotalVictories() => allBattles.FindAll(b => b.victory).Count;

    /// <summary>จำนวนเกมที่แพ้</summary>
    public int GetTotalDefeats() => allBattles.FindAll(b => !b.victory).Count;

    /// <summary>อัตราชนะ (%)</summary>
    public float GetWinRate()
    {
        if (allBattles.Count == 0) return 0f;
        return (float)GetTotalVictories() / allBattles.Count * 100f;
    }

    /// <summary>ค่าเฉลี่ยจำนวนเทิร์นที่ชนะ</summary>
    public float GetAverageTurnsToWin()
    {
        var victories = allBattles.FindAll(b => b.victory);
        if (victories.Count == 0) return 0f;

        int totalTurns = 0;
        foreach (var v in victories)
        {
            totalTurns += v.turnsPlayed;
        }
        return (float)totalTurns / victories.Count;
    }

    /// <summary>ดาเมจรวมที่ทำได้ทั้งหมด</summary>
    public int GetTotalDamageDealt()
    {
        int total = 0;
        foreach (var b in allBattles)
        {
            total += b.totalDamageDealt;
        }
        return total;
    }

    /// <summary>การ์ดที่เล่นทั้งหมด</summary>
    public int GetTotalCardsPlayed()
    {
        int total = 0;
        foreach (var b in allBattles)
        {
            total += b.totalCardsPlayed;
        }
        return total;
    }

    /// <summary>นับจำนวนเกมที่ชนะแบบ Perfect Victory</summary>
    public int GetPerfectVictories() => allBattles.FindAll(b => b.perfectVictory).Count;

    /// <summary>นับจำนวนเกมที่ชนะแบบ Quick Victory</summary>
    public int GetQuickVictories() => allBattles.FindAll(b => b.quickVictory).Count;

    /// <summary>การ์ดที่เล่นมากที่สุด (Top 10)</summary>
    public Dictionary<string, int> GetMostPlayedCards(int topCount = 10)
    {
        Dictionary<string, int> cardCounts = new Dictionary<string, int>();

        // นับการ์ดทั้งหมด
        foreach (var battle in allBattles)
        {
            foreach (var cardId in battle.cardsUsedInBattle)
            {
                if (string.IsNullOrEmpty(cardId)) continue;

                if (!cardCounts.ContainsKey(cardId))
                    cardCounts[cardId] = 0;

                cardCounts[cardId]++;
            }
        }

        // เรียงจากมากไปน้อย
        var sorted = new List<KeyValuePair<string, int>>(cardCounts);
        sorted.Sort((a, b) => b.Value.CompareTo(a.Value));

        // เอาแค่ Top N
        Dictionary<string, int> topCards = new Dictionary<string, int>();
        for (int i = 0; i < Mathf.Min(topCount, sorted.Count); i++)
        {
            topCards[sorted[i].Key] = sorted[i].Value;
        }

        return topCards;
    }

    /// <summary>สร้างรายงานสรุป</summary>
    public string GetSummaryReport()
    {
        string report = "=== BATTLE HISTORY SUMMARY ===\n";
        report += $"Total Battles: {GetTotalBattles()}\n";
        report += $"Victories: {GetTotalVictories()} ({GetWinRate():F1}%)\n";
        report += $"Defeats: {GetTotalDefeats()}\n";
        report += $"Avg Turns to Win: {GetAverageTurnsToWin():F1}\n";
        report += $"Total Damage Dealt: {GetTotalDamageDealt()}\n";
        report += $"Total Cards Played: {GetTotalCardsPlayed()}\n";
        report += $"Perfect Victories: {GetPerfectVictories()}\n";
        report += $"Quick Victories: {GetQuickVictories()}\n";

        // Top 5 cards
        var topCards = GetMostPlayedCards(5);
        if (topCards.Count > 0)
        {
            report += "\nTop 5 Most Played Cards:\n";
            int rank = 1;
            foreach (var card in topCards)
            {
                report += $"  {rank}. {card.Key} ({card.Value}x)\n";
                rank++;
            }
        }

        return report;
    }

    /// <summary>ผลการเล่นวันนี้</summary>
    public List<BattleStatistics> GetTodaysBattles()
    {
        DateTime today = DateTime.Now.Date;
        return allBattles.FindAll(b => b.battleEndTime.Date == today);
    }

    /// <summary>ดาเมจรวมวันนี้</summary>
    public int GetTodaysTotalDamage()
    {
        var todaysBattles = GetTodaysBattles();
        int total = 0;
        foreach (var b in todaysBattles)
        {
            total += b.totalDamageDealt;
        }
        return total;
    }

    /// <summary>จำนวนชนะวันนี้</summary>
    public int GetTodaysVictories()
    {
        return GetTodaysBattles().FindAll(b => b.victory).Count;
    }
}

/// <summary>Wrapper สำหรับ Serialize List</summary>
[System.Serializable]
public class BattleHistoryData
{
    public List<BattleStatistics> battles = new List<BattleStatistics>();
}
