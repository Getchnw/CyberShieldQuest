using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;
    public List<AchievementData> allAchievements;
    public TextMeshProUGUI goldText; // อ้างอิง UI เงินเพื่ออัพเดตเมื่อรับรางวัล
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Debug.Log("🟢 AchievementManager เริ่มทำงาน");
        allAchievements = GameContentDatabase.Instance.GetAllAchievementData();
    }

    // ฟังก์ชันนี้ควรเรียกตอนจบด่าน หรือ จบเนื้อเรื่อง
    public void CheckAchievements()
    {
        var gameData = GameManager.Instance.CurrentGameData;

        // 1. เช็ค Story Master (ดาวเต็มทุกบท)
        bool isStoryComplete = CheckAllStoriesFullStars();
        Debug.Log($"ตรวจสอบ Story Master: {(isStoryComplete ? "ผ่าน" : "ไม่ผ่าน")}");
        if (isStoryComplete) UnlockAchievement(AchievementType.StoryMaster);

        // 2. เช็ค Stage Master (ดาวเต็มทุกด่าน)
        bool isStageComplete = CheckAllStagesFullStars();
        if (isStageComplete) UnlockAchievement(AchievementType.StageMaster);

        // 3. เช็ค Grand Master (ครบทั้งคู่)
        if (isStoryComplete && isStageComplete)
        {
            UnlockAchievement(AchievementType.GrandMaster);
        }

        GameManager.Instance.SaveCurrentGame();
    }

    // --- Logic ตรวจสอบ (ต้องปรับตามตัวแปรจริงของคุณ) ---

    bool CheckAllStoriesFullStars()
    {
        // สมมติว่าคุณมี List ของ Story Data ทั้งหมดใน Database
        var allStories = GameContentDatabase.Instance.GetAllStoryChapters();
        var userProgress = GameManager.Instance.CurrentGameData.chapterProgress; // Save ของผู้เล่น

        foreach (var story in allStories)
        {
            // หาเซฟของบทนั้นๆ
            var progress = userProgress.Find(p => p.chapter_id == story.chapter_id);
            Debug.Log($"ตรวจสอบบท {story.chapter_id}: ได้ดาว {progress?.stars_earned ?? 0} / 3");
            // ถ้ายังไม่เล่น หรือ ได้ดาวไม่เต็ม 3 -> ถือว่ายังไม่ผ่านเงื่อนไข
            if (progress == null || progress.stars_earned < 3)
                return false;
        }
        return true; // ถ้าวนลูปครบแล้วไม่มีอันไหนผิดเงื่อนไขเลย แปลว่าครบ!
    }

    bool CheckAllStagesFullStars()
    {
        var allStages = StageManager.Instance.allStages;
        var userProgress = GameManager.Instance.CurrentGameData.stageProgress;

        foreach (var stage in allStages)
        {
            var progress = userProgress.Find(p => p.stageID == stage.stageID);

            // เช็คคว่าได้ 3 ดาวหรือยัง
            if (progress == null || progress.starsEarned < 3)
                return false;
        }
        return true;
    }

    // --- ระบบปลดล็อค ---

    void UnlockAchievement(AchievementType type)
    {
        var dataObj = allAchievements.Find(x => x.type == type);
        if (dataObj == null) return;

        var saveList = GameManager.Instance.CurrentGameData.achievements;
        var userAchieve = saveList.Find(x => x.achievementID == dataObj.id);

        // ถ้ายังไม่มีในเซฟ ให้สร้างใหม่
        if (userAchieve == null)
        {
            userAchieve = new PlayerAchievementData { achievementID = dataObj.id, isUnlocked = true, isClaimed = false };
            saveList.Add(userAchieve);

            // แจ้งเตือนผู้เล่น (UI Popup)
            Debug.Log($"🏆 Achievement Unlocked: {dataObj.title}");
        }
        else if (!userAchieve.isUnlocked)
        {
            // ถ้ามีอยู่แล้วแต่ยังไม่ปลดล็อค (กรณีสร้างรอไว้)
            userAchieve.isUnlocked = true;
            Debug.Log($"🏆 Achievement Unlocked: {dataObj.title}");
        }
    }

    // --- ระบบรับรางวัล ---
    public void ClaimReward(string achievementID)
    {
        var saveList = GameManager.Instance.CurrentGameData.achievements;
        var userAchieve = saveList.Find(x => x.achievementID == achievementID);
        var dataObj = allAchievements.Find(x => x.id == achievementID);

        if (userAchieve != null && userAchieve.isUnlocked && !userAchieve.isClaimed)
        {
            // 1. ให้ของรางวัล
            if (dataObj.rewardGold > 0) GameManager.Instance.AddGold(dataObj.rewardGold);
            // if (!string.IsNullOrEmpty(dataObj.rewardItemName))
            //     GameManager.Instance.AddItemToInventory(dataObj.rewardItemName, 1);

            // 2. ถ้าเป็น GrandMaster ให้ฉายา
            if (dataObj.type == AchievementType.GrandMaster)
            {
                GameManager.Instance.CurrentGameData.currentTitle = dataObj.rewardTitle;
                Debug.Log($"👑 ได้รับฉายาใหม่: {dataObj.rewardTitle}");
            }

            // 3. บันทึกสถานะ
            userAchieve.isClaimed = true;
            GameManager.Instance.SaveCurrentGame();
        }
    }

    // Update UI Gold
    public void UpdateUIGold()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameData != null && goldText != null)
        {
            goldText.text = $"{GameManager.Instance.CurrentGameData.profile.gold}";
        }
    }
}