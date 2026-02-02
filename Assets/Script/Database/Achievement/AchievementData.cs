using UnityEngine;

public enum AchievementType
{
    StoryMaster,    // ครบทุกบท
    StageMaster,    // ครบทุกด่าน
    GrandMaster     // ครบทั้งหมด (ได้ฉายา)
}

[CreateAssetMenu(fileName = "NewAchievement", menuName = "Game/AchievementData")]
public class AchievementData : ScriptableObject
{
    public string id;                   // เช่น "ach_story_all"
    public string title;                // ชื่อความสำเร็จ
    [TextArea] public string description; // รายละเอียด
    public AchievementType type;
    public Sprite icon;

    // ของรางวัล
    public int rewardGold;
    public string rewardItemName;       // เช่น "Legendary Sword"
    public string rewardTitle;          // ฉายาพิเศษ (เฉพาะ GrandMaster)
}