using UnityEngine;
using System.Collections.Generic;

// --- Enums ---
public enum CardType { Monster, Spell, EquipSpell, Token }
public enum MainCategory {  A01, A02, A03, General }
public enum SubCategory { General, IDOR, PathTraversal, MFLAC, InsecureTransit, InsecureRest, WeakHash, NoSalt, BadKey, SQLi, XSS, OSCommand, XXE }
public enum Rarity { Common, Rare, Epic, Legendary } // 🔥 ความหายากสำหรับกาชา

public enum EffectTrigger { None, OnDeploy, OnStrike, OnStrikeHit, Continuous, OnIntercept, OnDestroyed }
public enum TargetType { Self, EnemyPlayer, EnemyMonster, EnemyEquip, EnemyHand, EnemyDeck, AllGlobal }
public enum ActionType { None, Destroy, DisableAttack, DisableAbility, RevealHand, RevealHandMultiple, DiscardDeck, SummonToken, ModifyStat, ControlEquip, HealHP, ForceIntercept, BypassIntercept, DisableIntercept, DrawCard, Rush, DoubleStrike, GraveyardATK, ZeroStats, RemoveCategory, ForceChooseDiscard, ReturnEquipFromGraveyard }
public enum DestroyMode { SelectTarget, DestroyAll } // 🔥 โหมดการทำลาย: เลือกเป้าหมาย vs ทำลายทั้งหมด

[System.Serializable]
public struct CardEffect {
    public EffectTrigger trigger;
    public TargetType targetType;
    public ActionType action;
    public MainCategory targetMainCat;
    public SubCategory targetSubCat;
    public int value;
    public int duration; // 🔥 ระยะเวลา (เทิร์น): 0 = ตลอด, >= 1 = จำนวนเทิร์นที่กำหนด (ใช้กับ RemoveCategory, ForceIntercept, DisableIntercept)
    public DestroyMode destroyMode; // 🔥 โหมดการทำลาย (ใช้เมื่อ action = Destroy)
    public string tokenCardId; // 🔥 card_id ของ Token ที่จะ summon (ใช้เมื่อ action = SummonToken)
    public MainCategory bypassAllowedMainCat; // 🔥 MainCategory ที่สามารถ Intercept ได้ (ใช้กับ BypassIntercept, General = ข้าวทั้งหมด)
    public SubCategory bypassAllowedSubCat; // 🔥 SubCategory ที่สามารถ Intercept ได้ (ใช้กับ BypassIntercept, General = ข้ามทั้งหมด)
}

[CreateAssetMenu(fileName = "New Card", menuName = "Game Content/Card")]
public class CardData : ScriptableObject {
    [Header("Identity")]
    public string card_id;
    public string cardName;
    public Sprite artwork;
    public Sprite frame;

    [Header("Classification")]
    public CardType type;
    public MainCategory mainCategory;
    public SubCategory subCategory;
    public Rarity rarity; // 🔥 ระดับความหายาก

    [Header("Stats")]
    public int cost;
    public int atk;
    public int hp; 

    [Header("Texts")]
    [TextArea(3, 6)] public string abilityText; // Skill
    [TextArea(2, 4)] public string flavorText;  // Description

    [Header("Game Logic")]
    public List<CardEffect> effects;
}