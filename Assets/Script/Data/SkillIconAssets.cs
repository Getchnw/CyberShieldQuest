using UnityEngine;
using System.Linq;

// ScriptableObject สำหรับเก็บ Skill Icon Sprites ไว้ที่เดียว
// ทั้ง BattleCardUI และ CardUISlot จะ reference จากที่นี่
[CreateAssetMenu(fileName = "SkillIconAssets", menuName = "Game/Skill Icon Assets")]
public class SkillIconAssets : ScriptableObject
{
    [Header("Trigger Icon Sprites")]
    public Sprite iconOnDeploy;
    public Sprite iconOnStrike;
    public Sprite iconOnStrikeHit;
    public Sprite iconOnIntercept;
    public Sprite iconOnDestroyed;
    public Sprite iconOnTurnEnd;
    public Sprite iconContinuous;
    
    [Header("Action Icon Sprites")]
    public Sprite iconDestroy;
    public Sprite iconDisableAttack;
    public Sprite iconDisableAbility;
    public Sprite iconRevealHand;
    public Sprite iconDiscardDeck;
    public Sprite iconSummonToken;
    public Sprite iconModifyStat;
    public Sprite iconControlEquip;
    public Sprite iconHealHP;
    public Sprite iconForceIntercept;
    public Sprite iconBypassIntercept;
    public Sprite iconDisableIntercept;
    public Sprite iconDrawCard;
    public Sprite iconRush;
    public Sprite iconDoubleStrike;
    public Sprite iconGraveyardATK;
    public Sprite iconZeroStats;
    public Sprite iconRemoveCategory;
    public Sprite iconForceChooseDiscard;
    public Sprite iconReturnEquipFromGraveyard;
    public Sprite iconPeekDiscardTopDeck;
    public Sprite iconMarkInterceptMillDeck;
    public Sprite iconProtection;

    // Singleton instance for easy access
    private static SkillIconAssets _instance;
    public static SkillIconAssets Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<SkillIconAssets>("SkillIconAssets");
                if (_instance == null)
                {
                    // Fallback for editor/runtime cases where asset is loaded but not under Resources.
                    _instance = Resources.FindObjectsOfTypeAll<SkillIconAssets>().FirstOrDefault();
                }
                if (_instance == null)
                {
                    Debug.LogError("SkillIconAssets not found in Resources folder! Create one via Assets > Create > Game > Skill Icon Assets and move to Resources folder.");
                }
            }
            return _instance;
        }
    }

    // Helper methods
    public Sprite GetTriggerSprite(EffectTrigger trigger)
    {
        switch (trigger)
        {
            case EffectTrigger.OnDeploy: return iconOnDeploy;
            case EffectTrigger.OnStrike: return iconOnStrike;
            case EffectTrigger.OnStrikeHit: return iconOnStrikeHit;
            case EffectTrigger.OnIntercept: return iconOnIntercept;
            case EffectTrigger.OnDestroyed: return iconOnDestroyed;
            case EffectTrigger.OnTurnEnd: return iconOnTurnEnd;
            case EffectTrigger.Continuous: return iconContinuous;
            default: return null;
        }
    }

    public Sprite GetActionSprite(ActionType action)
    {
        switch (action)
        {
            case ActionType.Destroy: return iconDestroy;
            case ActionType.DisableAttack: return iconDisableAttack;
            case ActionType.DisableAbility: return iconDisableAbility;
            case ActionType.RevealHand:
            case ActionType.RevealHandMultiple: return iconRevealHand;
            case ActionType.DiscardDeck: return iconDiscardDeck;
            case ActionType.SummonToken: return iconSummonToken;
            case ActionType.ModifyStat: return iconModifyStat;
            case ActionType.ControlEquip: return iconControlEquip;
            case ActionType.HealHP:
            case ActionType.HealOnMonsterSummoned: return iconHealHP;
            case ActionType.ForceIntercept: return iconForceIntercept;
            case ActionType.BypassIntercept: return iconBypassIntercept;
            case ActionType.DisableIntercept: return iconDisableIntercept;
            case ActionType.DrawCard: return iconDrawCard;
            case ActionType.Rush: return iconRush;
            case ActionType.DoubleStrike: return iconDoubleStrike;
            case ActionType.GraveyardATK: return iconGraveyardATK;
            case ActionType.ZeroStats: return iconZeroStats;
            case ActionType.RemoveCategory: return iconRemoveCategory;
            case ActionType.ForceChooseDiscard: return iconForceChooseDiscard;
            case ActionType.ReturnEquipFromGraveyard: return iconReturnEquipFromGraveyard;
            case ActionType.PeekDiscardTopDeck: return iconPeekDiscardTopDeck;
            case ActionType.MarkInterceptMillDeck: return iconMarkInterceptMillDeck;
            case ActionType.InterceptAlwaysTypeMatch: return iconForceIntercept;
            case ActionType.ProtectDrawnCards:
            case ActionType.ProtectRevealHandMultiple:
            case ActionType.ProtectForceInterceptEquip:
            case ActionType.ProtectOtherOwnEquipFromAbilityDestroy: return iconProtection;
            default: return null;
        }
    }
}
