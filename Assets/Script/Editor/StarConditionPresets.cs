#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor Tool สำหรับตั้งค่า Star Conditions แบบ Preset
/// ให้คลิกปุ่มใน Inspector แล้วมันจะสร้างเงื่อนไขให้อัตโนมัติ
/// </summary>
public class StarConditionPresets
{
    // ============================================
    // === EASY STAGES (บทเรียน/ด่านแรก) ===
    // ============================================
    [MenuItem("CyberShield/Star Conditions/Easy - Basic Victory")]
    public static void CreateEasyBasic()
    {
        var conditions = new List<StarCondition>
        {
            StarCondition.CreateWinCondition(),
            StarCondition.CreateDamageTakenCondition(30, "โดนดาเมจไม่เกิน 30"),
            StarCondition.CreateHealthRemainingCondition(50, "เหลือ HP อย่างน้อย 50")
        };
        Debug.Log("✅ Easy Stage - Basic Victory ตั้งค่าเสร็จ");
    }

    [MenuItem("CyberShield/Star Conditions/Easy - Category Focus")]
    public static void CreateEasyCategoryFocus()
    {
        var conditions = new List<StarCondition>
        {
            StarCondition.CreateWinCondition(),
            StarCondition.CreateCategoryCondition(MainCategory.A01, 3, "ใช้การ์ด A01 อย่างน้อย 3 ใบ"),
            StarCondition.CreateDamageDealtCondition(20, "ทำดาเมจอย่างน้อย 20")
        };
        Debug.Log("✅ Easy Stage - Category Focus ตั้งค่าเสร็จ");
    }

    // ============================================
    // === MEDIUM STAGES (ด่านปกติ) ===
    // ============================================
    [MenuItem("CyberShield/Star Conditions/Medium - Turn Limit")]
    public static void CreateMediumTurnLimit()
    {
        var conditions = new List<StarCondition>
        {
            StarCondition.CreateWinCondition(),
            StarCondition.CreateTurnLimitCondition(7, "ชนะภายใน 7 เทิร์น"),
            StarCondition.CreateDamageDealtCondition(50, "ทำดาเมจอย่างน้อย 50")
        };
        Debug.Log("✅ Medium Stage - Turn Limit ตั้งค่าเสร็จ");
    }

    [MenuItem("CyberShield/Star Conditions/Medium - Balanced Challenge")]
    public static void CreateMediumBalanced()
    {
        var conditions = new List<StarCondition>
        {
            StarCondition.CreateTurnLimitCondition(8, "ชนะภายใน 8 เทิร์น"),
            StarCondition.CreateMonstersDefeatedCondition(2, "ทำลายมอนสเตอร์ศัตรู 2 ตัว"),
            StarCondition.CreateNoMonstersLostCondition()
        };
        Debug.Log("✅ Medium Stage - Balanced Challenge ตั้งค่าเสร็จ");
    }

    [MenuItem("CyberShield/Star Conditions/Medium - Damage Dealer")]
    public static void CreateMediumDamageDealer()
    {
        var conditions = new List<StarCondition>
        {
            StarCondition.CreateWinCondition(),
            StarCondition.CreateDamageDealtCondition(80, "ทำดาเมจอย่างน้อย 80"),
            StarCondition.CreateCardsDestroyedCondition(5, "ทำลายการ์ดศัตรู 5 ใบ")
        };
        Debug.Log("✅ Medium Stage - Damage Dealer ตั้งค่าเสร็จ");
    }

    // ============================================
    // === HARD STAGES (ด่านยาก/Challenge) ===
    // ============================================
    [MenuItem("CyberShield/Star Conditions/Hard - Perfect Victory")]
    public static void CreateHardPerfect()
    {
        var conditions = new List<StarCondition>
        {
            StarCondition.CreateWinCondition(),
            StarCondition.CreatePerfectVictoryCondition(),
            StarCondition.CreateQuickVictoryCondition()
        };
        Debug.Log("✅ Hard Stage - Perfect Victory ตั้งค่าเสร็จ");
    }

    [MenuItem("CyberShield/Star Conditions/Hard - Economy (Use Few Cards)")]
    public static void CreateHardEconomy()
    {
        var conditions = new List<StarCondition>
        {
            StarCondition.CreateWinCondition(),
            StarCondition.CreateMaxCardsPlayedCondition(12, "ใช้การ์ดไม่เกิน 12 ใบ"),
            StarCondition.CreateUniqueCardsCondition(10, "ใช้การ์ดไม่ซ้ำกัน 10 ใบ")
        };
        Debug.Log("✅ Hard Stage - Economy Challenge ตั้งค่าเสร็จ");
    }

    [MenuItem("CyberShield/Star Conditions/Hard - Speed Run")]
    public static void CreateHardSpeedRun()
    {
        var conditions = new List<StarCondition>
        {
            StarCondition.CreateWinCondition(),
            StarCondition.CreateTurnLimitCondition(5, "ชนะภายใน 5 เทิร์น"),
            StarCondition.CreateTimeLimitCondition(120f, "จบภายใน 2 นาที")
        };
        Debug.Log("✅ Hard Stage - Speed Run ตั้งค่าเสร็จ");
    }

    [MenuItem("CyberShield/Star Conditions/Hard - Tank (High Damage Taken OK)")]
    public static void CreateHardTank()
    {
        var conditions = new List<StarCondition>
        {
            StarCondition.CreateWinCondition(),
            StarCondition.CreateNoMonstersLostCondition(),
            StarCondition.CreateDamageDealtCondition(100, "ทำดาเมจอย่างน้อย 100")
        };
        Debug.Log("✅ Hard Stage - Tank Challenge ตั้งค่าเสร็จ");
    }

    // ============================================
    // === EXTREME/BONUS STAGES (โหมด Extreme) ===
    // ============================================
    [MenuItem("CyberShield/Star Conditions/Extreme - All Achievements")]
    public static void CreateExtremeAll()
    {
        var conditions = new List<StarCondition>
        {
            StarCondition.CreatePerfectVictoryCondition(),
            StarCondition.CreateQuickVictoryCondition(),
            StarCondition.CreateNoMonstersLostCondition()
        };
        Debug.Log("✅ Extreme Stage - All Achievements ตั้งค่าเสร็จ");
    }

    [MenuItem("CyberShield/Star Conditions/Extreme - Resource Master")]
    public static void CreateExtremeResourceMaster()
    {
        var conditions = new List<StarCondition>
        {
            StarCondition.CreateWinCondition(),
            StarCondition.CreateMaxCardsPlayedCondition(15, "ใช้การ์ดไม่เกิน 15 ใบ"),
            StarCondition.CreateMaxPPSpentCondition(50, "ใช้ PP ไม่เกิน 50")
        };
        Debug.Log("✅ Extreme Stage - Resource Master ตั้งค่าเสร็จ");
    }
}
#endif
