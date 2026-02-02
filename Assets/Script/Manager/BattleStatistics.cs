using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// สถิติการเล่นในแต่ละ Battle เก็บทุกรายละเอียดสำหรับประเมิน Quest และ Daily Mission
/// </summary>
[System.Serializable]
public class BattleStatistics
{
    [Header("Battle Result")]
    public bool victory = false; // ชนะ หรือ แพ้
    public int turnsPlayed = 0; // จำนวนเทิร์นที่เล่นไป
    public int finalPlayerHP = 0; // HP ของผู้เล่นเมื่อจบเกม
    public int finalEnemyHP = 0; // HP ของบอทเมื่อจบเกม
    public float battleDuration = 0f; // เวลาที่ใช้ในการต่อสู้ (วินาที)

    [Header("Cards Played - Total")]
    public int totalCardsPlayed = 0; // การ์ดทั้งหมดที่เล่น
    public int monsterCardsPlayed = 0; // Monster ที่เล่น
    public int spellCardsPlayed = 0; // Spell ที่เล่น
    public int equipCardsPlayed = 0; // EquipSpell ที่เล่น

    [Header("Cards Played - By Category")]
    public int cardsPlayedA01 = 0; // [A01] Broken Access Control
    public int cardsPlayedA02 = 0; // [A02] Cryptographic Failures
    public int cardsPlayedA03 = 0; // [A03] Injection
    public int cardsPlayedGeneral = 0; // [General]

    [Header("Cards Played - By SubCategory")]
    public Dictionary<SubCategory, int> cardsPlayedBySubCategory = new Dictionary<SubCategory, int>();

    [Header("Damage & Combat")]
    public int totalDamageDealt = 0; // ดาเมจรวมที่ทำให้ศัตรู
    public int totalDamageTaken = 0; // ดาเมจรวมที่ได้รับ
    public int totalHealingReceived = 0; // การฟื้นฟู HP ทั้งหมด
    public int monstersDefeated = 0; // มอนสเตอร์ของศัตรูที่ทำลาย
    public int playerMonstersLost = 0; // มอนสเตอร์ของผู้เล่นที่ถูกทำลาย

    [Header("Cards Destroyed")]
    public int enemyCardsDestroyed = 0; // การ์ดของศัตรูที่ทำลายทั้งหมด
    public int playerCardsDestroyed = 0; // การ์ดของผู้เล่นที่ถูกทำลาย

    [Header("Special Actions")]
    public int interceptionsSuccessful = 0; // จำนวนครั้งที่กันการโจมตีสำเร็จ
    public int interceptionsBlocked = 0; // จำนวนครั้งที่ถูกข้ามการกัน
    public int spellsCast = 0; // จำนวนครั้งที่ใช้ Spell
    public int cardsDrawn = 0; // จำนวนการ์ดที่จั่ว
    public int cardsSacrificed = 0; // จำนวนการ์ดที่ Sacrifice

    [Header("Deck Info")]
    public List<string> cardsUsedInBattle = new List<string>(); // card_id ของการ์ดที่ใช้ในเกม
    public List<string> uniqueCardsPlayed = new List<string>(); // card_id ของการ์ดที่เล่น (ไม่ซ้ำ)

    [Header("Resource Management")]
    public int totalPPSpent = 0; // PP รวมที่ใช้ไป
    public int cardsRemainingInDeck = 0; // การ์ดที่เหลือในเด็คเมื่อจบ
    public int cardsInHandAtEnd = 0; // การ์ดในมือเมื่อจบเกม

    [Header("Special Achievements")]
    public bool perfectVictory = false; // ชนะโดยไม่เสีย HP
    public bool quickVictory = false; // ชนะภายใน 5 เทิร์น
    public bool noMonstersLost = false; // ชนะโดยไม่เสียมอนสเตอร์เลย
    public bool usedAllCardTypes = false; // ใช้การ์ดทุกประเภท (Monster, Spell, Equip)

    [Header("Time Tracking")]
    public DateTime battleStartTime; // เวลาเริ่มต้น
    public DateTime battleEndTime; // เวลาสิ้นสุด

    /// <summary>เริ่มต้นสถิติใหม่</summary>
    public void Initialize()
    {
        victory = false;
        turnsPlayed = 0;
        finalPlayerHP = 0;
        finalEnemyHP = 0;
        battleDuration = 0f;

        totalCardsPlayed = 0;
        monsterCardsPlayed = 0;
        spellCardsPlayed = 0;
        equipCardsPlayed = 0;

        cardsPlayedA01 = 0;
        cardsPlayedA02 = 0;
        cardsPlayedA03 = 0;
        cardsPlayedGeneral = 0;

        cardsPlayedBySubCategory.Clear();

        totalDamageDealt = 0;
        totalDamageTaken = 0;
        totalHealingReceived = 0;
        monstersDefeated = 0;
        playerMonstersLost = 0;

        enemyCardsDestroyed = 0;
        playerCardsDestroyed = 0;

        interceptionsSuccessful = 0;
        interceptionsBlocked = 0;
        spellsCast = 0;
        cardsDrawn = 0;
        cardsSacrificed = 0;

        cardsUsedInBattle.Clear();
        uniqueCardsPlayed.Clear();

        totalPPSpent = 0;
        cardsRemainingInDeck = 0;
        cardsInHandAtEnd = 0;

        perfectVictory = false;
        quickVictory = false;
        noMonstersLost = false;
        usedAllCardTypes = false;

        battleStartTime = DateTime.Now;
        battleEndTime = DateTime.Now;
    }

    /// <summary>บันทึกการเล่นการ์ด</summary>
    public void RecordCardPlayed(CardData card)
    {
        if (card == null) return;

        totalCardsPlayed++;

        // นับตามประเภท
        switch (card.type)
        {
            case CardType.Monster:
                monsterCardsPlayed++;
                break;
            case CardType.Spell:
                spellCardsPlayed++;
                break;
            case CardType.EquipSpell:
                equipCardsPlayed++;
                break;
        }

        // นับตาม MainCategory
        switch (card.mainCategory)
        {
            case MainCategory.A01:
                cardsPlayedA01++;
                break;
            case MainCategory.A02:
                cardsPlayedA02++;
                break;
            case MainCategory.A03:
                cardsPlayedA03++;
                break;
            case MainCategory.General:
                cardsPlayedGeneral++;
                break;
        }

        // นับตาม SubCategory
        if (!cardsPlayedBySubCategory.ContainsKey(card.subCategory))
        {
            cardsPlayedBySubCategory[card.subCategory] = 0;
        }
        cardsPlayedBySubCategory[card.subCategory]++;

        // เก็บ card_id
        if (!string.IsNullOrEmpty(card.card_id))
        {
            cardsUsedInBattle.Add(card.card_id);
            
            if (!uniqueCardsPlayed.Contains(card.card_id))
            {
                uniqueCardsPlayed.Add(card.card_id);
            }
        }
    }

    /// <summary>จบเกมและคำนวณสถิติสุดท้าย</summary>
    public void Finalize(bool playerWon, int playerHP, int enemyHP, int turns, int deckRemaining, int handSize)
    {
        victory = playerWon;
        finalPlayerHP = playerHP;
        finalEnemyHP = enemyHP;
        turnsPlayed = turns;
        cardsRemainingInDeck = deckRemaining;
        cardsInHandAtEnd = handSize;

        battleEndTime = DateTime.Now;
        battleDuration = (float)(battleEndTime - battleStartTime).TotalSeconds;

        // คำนวณ Achievements
        perfectVictory = playerWon && finalPlayerHP >= 20; // ไม่เสีย HP เลย
        quickVictory = playerWon && turnsPlayed <= 5;
        noMonstersLost = playerWon && playerMonstersLost == 0;
        usedAllCardTypes = monsterCardsPlayed > 0 && spellCardsPlayed > 0 && equipCardsPlayed > 0;
    }

    /// <summary>สร้างสรุปสถิติเป็น string</summary>
    public string GetSummary()
    {
        string result = "=== Battle Statistics ===\n";
        result += $"Result: {(victory ? "VICTORY" : "DEFEAT")}\n";
        result += $"Turns: {turnsPlayed}\n";
        result += $"Final HP: Player {finalPlayerHP} | Enemy {finalEnemyHP}\n";
        result += $"Duration: {battleDuration:F1}s\n";
        result += $"\nCards Played: {totalCardsPlayed} (M:{monsterCardsPlayed} S:{spellCardsPlayed} E:{equipCardsPlayed})\n";
        result += $"Categories: A01:{cardsPlayedA01} A02:{cardsPlayedA02} A03:{cardsPlayedA03} Gen:{cardsPlayedGeneral}\n";
        result += $"\nDamage: Dealt {totalDamageDealt} | Taken {totalDamageTaken}\n";
        result += $"Monsters: Defeated {monstersDefeated} | Lost {playerMonstersLost}\n";
        result += $"Cards Destroyed: Enemy {enemyCardsDestroyed} | Player {playerCardsDestroyed}\n";
        result += $"\nSpecial: Intercepts {interceptionsSuccessful} | Spells {spellsCast} | Drawn {cardsDrawn}\n";
        result += $"PP Spent: {totalPPSpent}\n";
        
        if (perfectVictory) result += "🏆 Perfect Victory!\n";
        if (quickVictory) result += "⚡ Quick Victory!\n";
        if (noMonstersLost) result += "🛡️ No Monsters Lost!\n";
        if (usedAllCardTypes) result += "🎴 Used All Card Types!\n";
        
        return result;
    }

    /// <summary>Export เป็น JSON</summary>
    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }
}
