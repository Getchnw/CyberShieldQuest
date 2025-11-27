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

[CreateAssetMenu(fileName = "New Card", menuName = "Game Content/Card")]
public class CardData : ScriptableObject
{
    // 1. Basic Info (ข้อมูลพื้นฐาน)
    [Header("Basic Info")]
    [Tooltip("Unique ID เช่น card_idor_001")]
    public string cardId; 
    public string cardName;
    public Sprite artwork;

    // 2. Card Type & Category (ประเภทและหมวดหมู่)
    [Header("Classification")]
    public CardType type; // Monster, Spell, Equip
    
    // [สำคัญ] เพิ่มตรงนี้เพื่อรองรับ [A01: IDOR] หรือหมวดหมู่อื่นๆ ในอนาคต
    [Tooltip("เช่น IDOR, SQLi, XSS หรือปล่อยว่างถ้าไม่มี")]
    public CardCategory category; 

    // 3. Stats (ค่าพลัง)
    [Header("Stats")]
    [Min(0)] public int cost;
    
    // ใช้ ShowIf ใน Editor Script หรือปล่อยไว้แบบนี้ก็ได้ แต่ Monster เท่านั้นที่ควรมีค่านี้
    [Tooltip("ใส่ค่าเฉพาะ Monster ถ้าเป็นการ์ดเวทย์ให้ใส่ 0")]
    [Min(0)] public int atk; 
    [Min(0)] public int hp;  // เพิ่ม HP เข้ามาเพราะเห็น Effect "โจมตี HP"

    // 4. Texts (ข้อความ)
    [Header("Texts")]
    [Tooltip("ข้อความ Effect ที่แสดงบนการ์ด (ส่วนล่าง)")]
    [TextArea(3, 5)] 
    public string effectText; 

    [Tooltip("ข้อความบรรยายเนื้อเรื่อง (แถบสีเทาเข้ม)")]
    [TextArea(2, 4)] 
    public string flavorText; 

    // 5. Logic & Keywords (ตรรกะและคีย์เวิร์ด)
    [Header("Logic Configuration")]
    [Tooltip("เช่น Strike-Hit, Continuous, OnPlay")]
    public EffectTrigger triggerType;
    
    // (Optional) ถ้าคุณจะทำระบบ Effect แบบ Advance ในอนาคต
    // public List<CardEffect> effects; 
}

// Enums ควรแยกออกมาเพื่อง่ายต่อการเรียกใช้
public enum CardType {
    Monster,
    Spell,
    EquipSpell
}

public enum CardCategory {
    None,
    IDOR,
    Injection,
    BrokenAccess
    // เพิ่มตามช่องโหว่ Security อื่นๆ
}

public enum EffectTrigger {
    None,
    deploy,         // เกิดผลทันทีเมื่อลง (Spell ปกติ)
    Continuous,     // [Cont.] เกิดผลค้างไว้ (Equip / Field)
    StrikeHit,      // [Strike-Hit] เกิดผลเมื่อโจมตีโดน
    OnDestroyed     // เกิดผลเมื่อถูกทำลาย
}