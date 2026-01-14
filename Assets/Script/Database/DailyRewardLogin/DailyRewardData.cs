using UnityEngine;

// 1. ประเภทของรางวัล (มีอะไรบ้างในเกมคุณ)
// public enum RewardType
// {
//     Gold,       // เงิน Bitcoin
//     Card        // การ์ด Deck
// }

// 2. โครงสร้างข้อมูลของรางวัลในแต่ละวัน
[System.Serializable]
public struct DailyRewardItem
{
    public string rewardName;  // ชื่อรางวัล (เช่น "100 Gold")
    public string dayLabel;    // วัน (Day 1, Day 2, ...)
    public RewardType type;    // ประเภท
    public int amount;         // จำนวนที่แจก
    public CardData card;      // รหัสการ์ด (ถ้าเป็นรางวัลการ์ด)
    public Sprite icon;        // รูปไอคอนที่จะโชว์ใน UI
}

// 3. ตัวไฟล์ ScriptableObject ที่จะเก็บรายการทั้งหมด
[CreateAssetMenu(fileName = "DailyRewardList", menuName = "GameData/DailyRewardList")]
public class DailyRewardData : ScriptableObject
{
    public DailyRewardItem[] rewards; // รายการรางวัลทั้ง 7 วัน
}


