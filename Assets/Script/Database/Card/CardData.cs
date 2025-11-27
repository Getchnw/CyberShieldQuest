using UnityEngine;

[CreateAssetMenu(fileName = "New Card" ,menuName = "Game Content/Card")]
public class CardData : ScriptableObject
{
    public enum CardType { 
        Monster,
        Spell,
        EquipSpell 
    }

    [Header("Card Info")]
    [Tooltip("Unique ID, e.g. card_01")]
    public string card_id;
    [Tooltip("Display name")]
    public string cardName;
    public CardType type;
    [TextArea(3,6)]
    public string description;
    public StoryData StroryId;
    [Tooltip("Sprite artwork (use Sprite, not UI.Image)")]
    public Sprite artwork;
    public string info;
    [Header("Card Stats")]
    [Min(0)] public int cost;
    [Min(0)] public int atk;
    
}
