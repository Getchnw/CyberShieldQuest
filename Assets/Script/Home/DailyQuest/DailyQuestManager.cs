using UnityEngine;
using System;
using System.Collections.Generic;

public class DailyQuestManager : MonoBehaviour
{
    public static DailyQuestManager Instance;

    [SerializeField] private Transform contentContainer;
    [SerializeField] private GameObject questPrefab;

    private void Awake() { Instance = this; }

    private void Start()
    {
        CheckDailyReset();
    }

    public void CheckDailyReset()
    {
        var data = GameManager.Instance.CurrentGameData.dailyQuestData;
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");

        if (data.lastQuestDate != today)
        {
            // === วันใหม่: สุ่มเควสใหม่ ===
            List<DailyQuestsData> newQuests = GameContentDatabase.Instance.GetRandomQuests(4);

            data.activeQuests.Clear();
            foreach (var q in newQuests)
            {
                Debug.Log(q.questID + "" + q.description);
                data.activeQuests.Add(new PlayerQuestData
                {
                    questID = q.questID,
                    currentAmount = 0,
                    isClaimed = false
                });
            }

            data.lastQuestDate = today;
            GameManager.Instance.SaveCurrentGame();
        }

        CreateUI();
    }

    private void CreateUI()
    {
        // ล้างของเก่า
        foreach (Transform t in contentContainer) Destroy(t.gameObject);

        var savedQuests = GameManager.Instance.CurrentGameData.dailyQuestData.activeQuests;

        foreach (var savedQ in savedQuests)
        {
            // เอา ID จากเซฟ ไปหาข้อมูลจริงใน Database
            DailyQuestsData def = GameContentDatabase.Instance.GetQuestByID(savedQ.questID);

            if (def != null)
            {
                GameObject obj = Instantiate(questPrefab, contentContainer);
                obj.GetComponent<QuestItemUI>().Setup(savedQ, def);
            }
        }
    }

    // ฟังก์ชันอัปเดตภารกิจ (เรียกจากที่อื่น)
    public void UpdateProgress(QuestType type, int amount)
    {
        var activeQuests = GameManager.Instance.CurrentGameData.dailyQuestData.activeQuests;
        bool isChanged = false;

        foreach (var savedQ in activeQuests)
        {
            DailyQuestsData def = GameContentDatabase.Instance.GetQuestByID(savedQ.questID);

            // เช็คว่าประเภทตรงกัน และยังไม่เต็ม
            if (def.type == type && savedQ.currentAmount < def.targetAmount && !savedQ.isClaimed)
            {
                savedQ.currentAmount += amount;
                if (savedQ.currentAmount > def.targetAmount) savedQ.currentAmount = def.targetAmount;
                isChanged = true;
            }
        }

        if (isChanged)
        {
            GameManager.Instance.SaveCurrentGame();
            CreateUI(); // รีเฟรชหน้าจอ
        }
    }

    public void ClaimReward(string questID)
    {
        var data = GameManager.Instance.CurrentGameData;

        // 1. หาเควสใน Save Data
        var savedQ = data.dailyQuestData.activeQuests.Find(q => q.questID == questID);

        // 2. หาข้อมูลรางวัลจาก Database
        var def = GameContentDatabase.Instance.GetQuestByID(questID);

        if (savedQ != null && def != null && !savedQ.isClaimed)
        {
            // 3. แจกรางวัล (สมมติแจก Gold)
            data.profile.gold += def.rewardGold;
            Debug.Log($"ได้รับเงิน {def.rewardGold} Gold!");

            // 4. อัปเดตสถานะว่ารับแล้ว
            savedQ.isClaimed = true;

            // 5. บันทึกและรีเฟรชหน้าจอ
            GameManager.Instance.SaveCurrentGame();
            CreateUI(); // รีเฟรชปุ่มให้เป็นสีเทา/ขึ้นว่ารับแล้ว
        }

    }
}