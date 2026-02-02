using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq; // จำเป็นสำหรับการคำนวณผลรวม (Sum)

public class AchievementItemUI : MonoBehaviour
{
    [Header("UI References (ลากใส่ตามลำดับใน Hierarchy)")]
    [SerializeField] private Image iconImage;          // Box icon > Icon Achievement
    [SerializeField] private TextMeshProUGUI titleText;// QuestName & Progress > Quest Name
    [SerializeField] private TextMeshProUGUI descText; // QuestName & Progress > Quest Name > Description
    [SerializeField] private Slider progressSlider;    // QuestName & Progress > Progress Slider
    [SerializeField] private TextMeshProUGUI progressText; // QuestName & Progress > text progress
    [SerializeField] private Button claimButton;       // Box > Reward (ที่เป็นปุ่ม)
    [SerializeField] private TextMeshProUGUI rewardText; // Text ข้างในปุ่ม Reward

    private AchievementData staticData;
    private PlayerAchievementData saveData;

    public void Setup(AchievementData sData, PlayerAchievementData pData)
    {
        staticData = sData;
        saveData = pData;
        RefreshUI();
    }

    void RefreshUI()
    {
        // 1. ใส่ข้อมูลพื้นฐาน
        titleText.text = staticData.title;
        descText.text = staticData.description;
        iconImage.sprite = staticData.icon;

        // 2. คำนวณความคืบหน้า (เพื่อทำ Slider)
        CalculateProgress();

        // 3. จัดการปุ่มรับรางวัล
        if (saveData.isClaimed)
        {
            claimButton.interactable = false;
            rewardText.text = "Claimed";
        }
        else if (saveData.isUnlocked) // ปลดล็อคแล้วแต่ยังไม่รับ
        {
            claimButton.interactable = true;
            rewardText.text = "CLAIM";
        }
        else // ยังทำไม่เสร็จ
        {
            claimButton.interactable = false;
            // โชว์ของรางวัลที่จะได้ (ถ้ามี)
            rewardText.text = $"{staticData.rewardGold} Gold";
        }
    }

    void CalculateProgress()
    {
        int current = 0;
        int max = 1;

        // ดึงข้อมูลเกมมาคำนวณ
        var gameData = GameManager.Instance.CurrentGameData;
        var db = GameContentDatabase.Instance;
        var stages = StageManager.Instance.allStages;

        switch (staticData.type)
        {
            case AchievementType.StoryMaster:
                // เป้าหมาย: จำนวนบททั้งหมด x 3 (3 ดาวต่อบท)
                max = db.GetAllStoryChapters().Count * 3;
                // ปัจจุบัน: ผลรวมดาวที่ผู้เล่นทำได้ใน Story
                current = gameData.chapterProgress.Sum(x => x.stars_earned);
                break;

            case AchievementType.StageMaster:
                // เป้าหมาย: จำนวนด่านทั้งหมด x 3
                max = stages.Count * 3;
                // ปัจจุบัน: ผลรวมดาวที่ผู้เล่นทำได้ใน Stage
                current = gameData.stageProgress.Sum(x => x.starsEarned);
                break;

            case AchievementType.GrandMaster:
                // เป้าหมาย: 2 อย่าง (Story ครบ + Stage ครบ)
                max = 2;
                // เช็คว่า Unlock ไปกี่อย่างแล้ว
                bool storyDone = GameManager.Instance.CurrentGameData.achievements.Exists(x => x.achievementID == "ach_story_all" && x.isUnlocked); // แก้ ID ให้ตรงกับที่คุณตั้ง
                bool stageDone = GameManager.Instance.CurrentGameData.achievements.Exists(x => x.achievementID == "ach_stage_all" && x.isUnlocked);
                current = (storyDone ? 1 : 0) + (stageDone ? 1 : 0);
                break;
        }

        // อัปเดต Slider และ Text
        progressSlider.maxValue = max;
        progressSlider.value = current;
        progressText.text = $"{current} / {max}";
    }

    public void OnClaimClicked()
    {
        // เรียก Manager ให้แจกของ
        AchievementManager.Instance.ClaimReward(staticData.id);

        // รีเฟรชหน้าตัวเองทันทีเพื่อให้ปุ่มเปลี่ยนเป็น Claimed
        // (ต้องดึงข้อมูลใหม่จาก Save เพราะสถานะเปลี่ยนแล้ว)
        var newSaveData = GameManager.Instance.CurrentGameData.achievements.Find(x => x.achievementID == staticData.id);
        Setup(staticData, newSaveData);
    }
}