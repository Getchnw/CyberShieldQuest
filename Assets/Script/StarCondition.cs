using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// ระบบตรวจสอบเงื่อนไขดาว - รองรับการตรวจสอบหลายประเภท
/// </summary>
[System.Serializable]
public class StarCondition
{
    public enum ConditionType
    {
        // === Basic Victory Conditions ===
        WinBattle,           // ชนะ
        MaxTurns,            // ชนะภายใน X เทิร์น
        PerfectVictory,      // ชนะโดยไม่โดนดาเมจเลย
        QuickVictory,        // ชนะภายใน 3 เทิร์น

        // === Damage & Combat ===
        MinDamageDealt,      // ทำดาเมจรวมอย่างน้อย X
        MaxDamageTaken,      // โดนดาเมจไม่เกิน X
        MinMonstersDefeated, // ทำลายมอนสเตอร์ศัตรูอย่างน้อย X ตัว
        MinEnemyCardsDestroyed, // ทำลายการ์ดศัตรูอย่างน้อย X ใบ
        MaxPlayerCardsLost,  // สูญเสียการ์ดไม่เกิน X ใบ

        // === Card Usage ===
        UseCardCategory,     // ใช้การ์ดหมวด X อย่างน้อย Y ใบ
        UseCardType,         // ใช้การ์ดประเภท X อย่างน้อย Y ใบ
        UseCardSubCategory,  // ใช้การ์ด SubCategory X อย่างน้อย Y ใบ
        MinUniqueCardsPlayed, // ใช้การ์ดที่ไม่ซ้ำกันอย่างน้อย X ใบ
        MaxTotalCardsPlayed, // ใช้การ์ดไม่เกิน X ใบรวม
        MinSpellsCast,       // ใช้ Spell อย่างน้อย X ใบ
        MinMonstersSummoned, // เรียกมอนสเตอร์อย่างน้อย X ตัว
        MinEquipSpellsUsed,  // ใช้ EquipSpell อย่างน้อย X ใบ

        // === Monster Protection ===
        NoMonstersLost,      // ไม่มีมอนสเตอร์ของเราตาย
        MaxMonstersLost,     // สูญเสียมอนสเตอร์ไม่เกิน X ตัว

        // === Resource Management ===
        MaxPPSpent,          // ใช้ PP ไม่เกิน X
        MinPPSpent,          // ใช้ PP อย่างน้อย X (เล่นแบบ aggressive)
        MinCardsDrawn,       // จั่วการ์ดอย่างน้อย X ใบ
        MaxCardsInHandAtEnd, // เหลือการ์ดในมือไม่เกิน X ใบ
        MinCardsRemainingInDeck, // เหลือการ์ดในเด็คอย่างน้อย X ใบ
        MinCardsSacrificed,  // Sacrifice การ์ดอย่างน้อย X ใบ

        // === Health & Healing ===
        PlayerHealthRemaining, // เหลือ HP อย่างน้อย X
        PlayerHealthPercentage, // เหลือ HP อย่างน้อย X% (0-100)
        MinHealingReceived,  // ฟื้นฟู HP อย่างน้อย X

        // === Special Mechanics ===
        MinInterceptsSuccessful, // สกัดสำเร็จอย่างน้อย X ครั้ง
        MaxInterceptsBlocked,    // ถูกข้ามการสกัดไม่เกิน X ครั้ง

        // === Time-based ===
        MaxBattleDuration,   // จบการต่อสู้ภายใน X วินาที
    }

    [Header("Condition Settings")]
    public ConditionType type;
    public string description; // คำอธิบายที่แสดงให้ผู้เล่นเห็น
    public string description_th;

    [Header("Parameters")]
    public int intValue;          // ค่าตัวเลข (เช่น จำนวนเทิร์น, HP, ดาเมจ)
    public float floatValue;      // ค่าทศนิยม (เช่น เปอร์เซ็นต์, เวลา)
    public MainCategory category; // สำหรับ UseCardCategory
    public CardType cardType;     // สำหรับ UseCardType
    public SubCategory subCategory; // สำหรับ UseCardSubCategory

    /// <summary>
    /// ตรวจสอบว่าเงื่อนไขนี้ผ่านหรือไม่จาก BattleStatistics
    /// </summary>
    public bool CheckCondition(BattleStatistics stats)
    {
        if (stats == null)
        {
            Debug.LogWarning("StarCondition.CheckCondition: stats is null");
            return false;
        }

        switch (type)
        {
            case ConditionType.WinBattle:
                return stats.victory;

            case ConditionType.MaxTurns:
                return stats.victory && stats.turnsPlayed <= intValue;

            case ConditionType.MinDamageDealt:
                return stats.totalDamageDealt >= intValue;

            case ConditionType.MaxDamageTaken:
                return stats.totalDamageTaken <= intValue;

            case ConditionType.UseCardCategory:
                int categoryCount = 0;
                switch (category)
                {
                    case MainCategory.A01:
                        categoryCount = stats.cardsPlayedA01;
                        break;
                    case MainCategory.A02:
                        categoryCount = stats.cardsPlayedA02;
                        break;
                    case MainCategory.A03:
                        categoryCount = stats.cardsPlayedA03;
                        break;
                    case MainCategory.General:
                        categoryCount = stats.cardsPlayedGeneral;
                        break;
                }
                return categoryCount >= intValue;

            case ConditionType.UseCardType:
                int typeCount = 0;
                switch (cardType)
                {
                    case CardType.Monster:
                        typeCount = stats.monsterCardsPlayed;
                        break;
                    case CardType.Spell:
                        typeCount = stats.spellCardsPlayed;
                        break;
                    case CardType.EquipSpell:
                        typeCount = stats.equipCardsPlayed;
                        break;
                }
                return typeCount >= intValue;

            case ConditionType.NoMonstersLost:
                return stats.noMonstersLost;

            case ConditionType.PerfectVictory:
                return stats.perfectVictory;

            case ConditionType.QuickVictory:
                return stats.quickVictory;

            case ConditionType.MaxPPSpent:
                return stats.totalPPSpent <= intValue;

            case ConditionType.MinInterceptsSuccessful:
                return stats.interceptionsSuccessful >= intValue;

            case ConditionType.MinCardsDrawn:
                return stats.cardsDrawn >= intValue;

            case ConditionType.MinSpellsCast:
                return stats.spellsCast >= intValue;

            case ConditionType.MinMonstersSummoned:
                return stats.monsterCardsPlayed >= intValue;

            case ConditionType.PlayerHealthRemaining:
            case ConditionType.PlayerHealthPercentage:
                // คำนวณเปอร์เซ็นต์ HP (ต้องรู้ maxHP - สมมติว่ามี 100)
                // หรือเก็บไว้ใน stats.maxPlayerHP ถ้ามี
                float hpPercent = (stats.finalPlayerHP / 100f) * 100f;
                return hpPercent >= floatValue;

            case ConditionType.MinMonstersDefeated:
                return stats.monstersDefeated >= intValue;

            case ConditionType.MinEnemyCardsDestroyed:
                return stats.enemyCardsDestroyed >= intValue;

            case ConditionType.MaxPlayerCardsLost:
                return stats.playerCardsDestroyed <= intValue;

            case ConditionType.UseCardSubCategory:
                if (stats.cardsPlayedBySubCategory.ContainsKey(subCategory))
                    return stats.cardsPlayedBySubCategory[subCategory] >= intValue;
                return false;

            case ConditionType.MinUniqueCardsPlayed:
                return stats.uniqueCardsPlayed.Count >= intValue;

            case ConditionType.MaxTotalCardsPlayed:
                return stats.totalCardsPlayed <= intValue;

            case ConditionType.MinEquipSpellsUsed:
                return stats.equipCardsPlayed >= intValue;

            case ConditionType.MaxMonstersLost:
                return stats.playerMonstersLost <= intValue;

            case ConditionType.MinPPSpent:
                return stats.totalPPSpent >= intValue;

            case ConditionType.MaxCardsInHandAtEnd:
                return stats.cardsInHandAtEnd <= intValue;

            case ConditionType.MinCardsRemainingInDeck:
                return stats.cardsRemainingInDeck >= intValue;

            case ConditionType.MinCardsSacrificed:
                return stats.cardsSacrificed >= intValue;

            case ConditionType.MinHealingReceived:
                return stats.totalHealingReceived >= intValue;

            case ConditionType.MaxInterceptsBlocked:
                return stats.interceptionsBlocked <= intValue;

            case ConditionType.MaxBattleDuration:
                return stats.battleDuration <= floatValue;

            default:
                Debug.LogWarning($"Unknown condition type: {type}");
                return false;
        }
    }

    /// <summary>
    /// สร้าง StarCondition สำเร็จรูปแบบง่าย
    /// </summary>
    public static StarCondition CreateWinCondition(string desc = "ชนะการต่อสู้")
    {
        return new StarCondition { type = ConditionType.WinBattle, description = desc };
    }

    public static StarCondition CreateTurnLimitCondition(int maxTurns, string desc = null)
    {
        return new StarCondition
        {
            type = ConditionType.MaxTurns,
            intValue = maxTurns,
            description = desc ?? $"ชนะภายใน {maxTurns} เทิร์น"
        };
    }

    public static StarCondition CreatePerfectVictoryCondition(string desc = "ชนะโดยไม่โดนดาเมจ")
    {
        return new StarCondition { type = ConditionType.PerfectVictory, description = desc };
    }

    public static StarCondition CreateQuickVictoryCondition(string desc = "ชนะภายใน 3 เทิร์น")
    {
        return new StarCondition { type = ConditionType.QuickVictory, description = desc };
    }

    public static StarCondition CreateDamageDealtCondition(int minDamage, string desc = null)
    {
        return new StarCondition
        {
            type = ConditionType.MinDamageDealt,
            intValue = minDamage,
            description = desc ?? $"ทำดาเมจอย่างน้อย {minDamage}"
        };
    }

    public static StarCondition CreateDamageTakenCondition(int maxDamage, string desc = null)
    {
        return new StarCondition
        {
            type = ConditionType.MaxDamageTaken,
            intValue = maxDamage,
            description = desc ?? $"โดนดาเมจไม่เกิน {maxDamage}"
        };
    }

    public static StarCondition CreateCategoryCondition(MainCategory cat, int count, string desc = null)
    {
        string categoryName = cat.ToString();
        return new StarCondition
        {
            type = ConditionType.UseCardCategory,
            category = cat,
            intValue = count,
            description = desc ?? $"ใช้การ์ด {categoryName} อย่างน้อย {count} ใบ"
        };
    }

    public static StarCondition CreateNoMonstersLostCondition(string desc = "ไม่มีมอนสเตอร์ของเราตาย")
    {
        return new StarCondition { type = ConditionType.NoMonstersLost, description = desc };
    }

    public static StarCondition CreateHealthRemainingCondition(int minHP, string desc = null)
    {
        return new StarCondition
        {
            type = ConditionType.PlayerHealthRemaining,
            intValue = minHP,
            description = desc ?? $"เหลือ HP อย่างน้อย {minHP}"
        };
    }

    public static StarCondition CreateHealthPercentageCondition(float minPercent, string desc = null)
    {
        return new StarCondition
        {
            type = ConditionType.PlayerHealthPercentage,
            floatValue = minPercent,
            description = desc ?? $"เหลือ HP อย่างน้อย {minPercent}%"
        };
    }

    public static StarCondition CreateSubCategoryCondition(SubCategory subCat, int count, string desc = null)
    {
        string subCatName = subCat.ToString();
        return new StarCondition
        {
            type = ConditionType.UseCardSubCategory,
            subCategory = subCat,
            intValue = count,
            description = desc ?? $"ใช้การ์ด {subCatName} อย่างน้อย {count} ใบ"
        };
    }

    public static StarCondition CreateUniqueCardsCondition(int minUnique, string desc = null)
    {
        return new StarCondition
        {
            type = ConditionType.MinUniqueCardsPlayed,
            intValue = minUnique,
            description = desc ?? $"ใช้การ์ดที่ไม่ซ้ำกันอย่างน้อย {minUnique} ใบ"
        };
    }

    public static StarCondition CreateMonstersDefeatedCondition(int minMonsters, string desc = null)
    {
        return new StarCondition
        {
            type = ConditionType.MinMonstersDefeated,
            intValue = minMonsters,
            description = desc ?? $"ทำลายมอนสเตอร์ศัตรูอย่างน้อย {minMonsters} ตัว"
        };
    }

    public static StarCondition CreateCardsDestroyedCondition(int minCards, string desc = null)
    {
        return new StarCondition
        {
            type = ConditionType.MinEnemyCardsDestroyed,
            intValue = minCards,
            description = desc ?? $"ทำลายการ์ดศัตรูอย่างน้อย {minCards} ใบ"
        };
    }

    public static StarCondition CreateMaxCardsPlayedCondition(int maxCards, string desc = null)
    {
        return new StarCondition
        {
            type = ConditionType.MaxTotalCardsPlayed,
            intValue = maxCards,
            description = desc ?? $"ใช้การ์ดไม่เกิน {maxCards} ใบ"
        };
    }

    public static StarCondition CreateTimeLimitCondition(float maxSeconds, string desc = null)
    {
        return new StarCondition
        {
            type = ConditionType.MaxBattleDuration,
            floatValue = maxSeconds,
            description = desc ?? $"จบการต่อสู้ภายใน {maxSeconds} วินาที"
        };
    }

    public static StarCondition CreateMaxPPSpentCondition(int maxPP, string desc = null)
    {
        return new StarCondition
        {
            type = ConditionType.MaxPPSpent,
            intValue = maxPP,
            description = desc ?? $"ใช้ PP ไม่เกิน {maxPP}"
        };
    }
}
