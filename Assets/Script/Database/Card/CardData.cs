// using UnityEngine;

// [CreateAssetMenu(fileName = "New Card" ,menuName = "Game Content/Card")]
// public class CardData : ScriptableObject
// {
//     public enum CardType { 
//         Monster,
//         Spell,
//         EquipSpell 
//     }

//     [Header("Card Info")]
//     [Tooltip("Unique ID, e.g. card_01")]
//     public string card_id;
//     [Tooltip("Display name")]
//     public string cardName;
//     public CardType type;
//     [TextArea(3,6)]
//     public string description;
//     public StoryData StroryId;
//     [Tooltip("Sprite artwork (use Sprite, not UI.Image)")]
//     public Sprite artwork;
//     public string info;
//     [Header("Card Stats")]
//     [Min(0)] public int cost;
//     [Min(0)] public int atk;
    
// }
using UnityEngine;
using System.Collections.Generic;

// =========================================================================
// 1. Enums: กำหนดประเภทและหมวดหมู่ (อ้างอิงตามเอกสารทั้งหมด)
// =========================================================================

public enum CardType {
    Monster,
    Spell,
    EquipSpell,
    Token // สำหรับ Rogue Token
}

public enum MainCategory {
    None,
    [InspectorName("A01: Broken Access Control")] A01,
    [InspectorName("A02: Cryptographic Failures")] A02,
    [InspectorName("A03: Injection")] A03,
    General
}

public enum SubCategory {
    None,
    // A01
    IDOR, PathTraversal, MFLAC,
    // A02
    InsecureTransit, InsecureRest, WeakHash, NoSalt, BadKey,
    // A03
    SQLi, XSS, OSCommand, XXE
}

// =========================================================================
// 2. Logic Structs: โครงสร้าง Effect
// =========================================================================

public enum EffectTrigger {
    None,
    OnDeploy,       // [Deploy] ทำงานเมื่อลงสนาม (ทั้งแบบปกติและแบบทับ)
    OnStrike,       // [Strike] เมื่อประกาศโจมตี
    OnStrikeHit,    // [Strike-Hit] เมื่อตีเข้า HP สำเร็จ
    Continuous,     // [Cont.] ผลต่อเนื่อง
    OnIntercept,    // เมื่อใช้ Intercept
    OnDestroyed
}

public enum TargetType {
    Self,
    EnemyPlayer,
    EnemyMonster,
    EnemyEquip,
    EnemyHand,
    EnemyDeck,
    AllGlobal
}

public enum ActionType {
    None,
    Destroy,        // ทำลายการ์ด
    DisableAttack,  // ห้ามโจมตี
    DisableAbility, // ห้ามใช้ Ability
    RevealHand,     // ดูมือ
    DiscardDeck,    // ทิ้งการ์ดจาก Deck
    SummonToken,    // เรียก Token
    ModifyStat,     // ปรับ Cost/Atk
    ControlEquip,   // ยึด Equip
    HealHP,         // ฮีล
    ForceIntercept, // บังคับรับดาเมจ
    BypassIntercept // ทะลุการป้องกัน
}

[System.Serializable]
public struct CardEffect {
    [Tooltip("จังหวะที่ Effect ทำงาน")]
    public EffectTrigger trigger;
    
    [Tooltip("เป้าหมาย")]
    public TargetType targetType;
    
    [Tooltip("ผลลัพธ์")]
    public ActionType action;

    [Header("Conditions & Values")]
    public MainCategory targetMainCat; // เงื่อนไขเป้าหมาย (Main)
    public SubCategory targetSubCat;   // เงื่อนไขเป้าหมาย (Sub)
    public int value;                  // ค่าตัวเลข (Damage, Cost condition ฯลฯ)
}

// =========================================================================
// 3. CardData: ตัวข้อมูลการ์ด (ตัด Def และ SacrificeCount ออก)
// =========================================================================

[CreateAssetMenu(fileName = "New Card", menuName = "Game Content/Card")]
public class CardData : ScriptableObject {
    
    // --- Identity ---
    [Header("Identity")]
    public string card_id;
    public string cardName;    // 
    public Sprite artwork;
    public Sprite frame;

    // --- Classification ---
    [Header("Classification")]
    public CardType type;
    public MainCategory mainCategory; // 
    public SubCategory subCategory;   // 

    // --- Stats & Costs ---
    [Header("Stats")]
    [Tooltip("Cost เต็มของการ์ด (ถ้าลงทับตัวเดิม โปรแกรมจะคำนวณส่วนต่างให้เอง)")]
    [Min(0)] public int cost; // 

    [Min(0)] public int atk; // 
    

    // --- Text Info ---
    [Header("Texts")]
    [TextArea(3, 6)] public string abilityText; // 
    [TextArea(2, 4)] public string flavorText;  // 

    // --- Game Logic ---
    [Header("Game Logic")]
    public List<CardEffect> effects;

    // =====================================================================
    // Helper Function: ตัวอย่างคำนวณ Cost (สำหรับเรียกใช้ใน GameManager)
    // =====================================================================
    /// <summary>
    /// คำนวณ Cost ที่ต้องจ่ายจริง
    /// </summary>
    /// <param name="targetOverlayCard">การ์ดใบเก่าในสนามที่จะลงทับ (ถ้าไม่มีให้ใส่ null)</param>
    /// <returns>Final Cost (PP)</returns>
    public int GetPlayCost(CardData targetOverlayCard = null) {
        if (targetOverlayCard == null) {
            // ลงแบบปกติ จ่ายเต็ม
            return cost;
        } else {
            // ลงแบบทับ (System Sacrifice) จ่ายส่วนต่าง
            int diff = cost - targetOverlayCard.cost;
            return Mathf.Max(0, diff); // ห้ามติดลบ อย่างน้อย 0
        }
    }
}