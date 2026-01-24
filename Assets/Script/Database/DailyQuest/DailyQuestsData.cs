using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Daily/QuestData")]
public class DailyQuestsData : ScriptableObject
{
    public string questID;        // รหัสเควส (เช่น "kill_slime_10")

    [TextArea]
    public string description;    // คำอธิบาย (เช่น "กำจัด Slime 10 ตัว")

    public int targetAmount;      // เป้าหมาย (เช่น 10)
    public int rewardGold;        // รางวัล (เช่น 100)
    public Sprite icon;           // รูปไอคอนเควส

    //ประเภทเควส เอาไว้เช็คเงื่อนไข
    public QuestType type;
}

public enum QuestType
{
    Stage,
    Story,
    Gacha,
    Card,
}