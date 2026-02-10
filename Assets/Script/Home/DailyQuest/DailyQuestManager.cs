using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class DailyQuestManager : MonoBehaviour
{
    public static DailyQuestManager Instance;

    [SerializeField] private Transform contentContainer;
    [SerializeField] private GameObject questPrefab;
    public TextMeshProUGUI goldText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ห้ามทำลายเมื่อเปลี่ยนฉาก

        }
        else
        {
            Destroy(gameObject); // ถ้ามีตัวซ้ำ (เวลากลับมาหน้าเมนู) ให้ลบตัวใหม่ทิ้ง
        }
    }

    private void Start()
    {
        CheckDailyReset();
    }

    public void RegisterUI(Transform container, TextMeshProUGUI goldRef)
    {
        this.contentContainer = container;
        this.goldText = goldRef;
        Debug.Log("✅ UI เชื่อมต่อสำเร็จ! เริ่มวาดรายการเควส...");

        CreateUI(); // วาด UI ทันทีที่หน้าต่างเปิด
    }

    // void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     // พยายามหา Content Container ใหม่ทุกครั้งโดยการ "ค้นหาจากชื่อ"
    //     // (ต้องตั้งชื่อ GameObject ในฉาก Home ให้ตรงกับชื่อนี้เป๊ะๆ)
    //     GameObject foundContainer = GameObject.Find("QuestListContent");

    //     if (foundContainer != null)
    //     {
    //         contentContainer = foundContainer.transform;
    //         Debug.Log("✅ เจอ Quest Container แล้ว! เชื่อมต่อสำเร็จ");

    //         // หา Text เงินด้วย (ถ้ามี)
    //         GameObject foundGold = GameObject.Find("GoldText");
    //         if (foundGold != null) goldText = foundGold.GetComponent<TextMeshProUGUI>();

    //         CreateUI(); // วาด UI ใหม่ทันทีเพราะกลับมาหน้าเมนูแล้ว
    //     }
    //     else
    //     {
    //         contentContainer = null; // ถ้าหาไม่เจอ (เช่น อยู่ฉาก Battle) ให้เป็น null ไว้
    //     }
    // }

    public void CheckDailyReset()
    {
        var data = GameManager.Instance.CurrentGameData.dailyQuestData;
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");

        if (data.lastQuestDate != today)
        {
            // === วันใหม่: สุ่มเควสใหม่ ===
            // ค้องใส่เลขที่หาร 4 ลงตัว
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
        if (contentContainer == null)
        {
            Debug.LogWarning("❌ Quest Container not found in the scene. Cannot create Daily Quest UI.");
            return;
        }
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
    public void UpdateProgress(QuestType type, int amount, string conditionKey = "")
    {
        var activeQuests = GameManager.Instance.CurrentGameData.dailyQuestData.activeQuests;
        bool isChanged = false;

        foreach (var savedQ in activeQuests)
        {
            DailyQuestsData def = GameContentDatabase.Instance.GetQuestByID(savedQ.questID);
            if (def == null)
            {
                Debug.LogWarning($"Quest ID '{savedQ.questID}' not found in DB.");
                continue;
            }
            if (def.type != type) continue;
            bool isMatchkey = (def.conditionKey == conditionKey) || string.IsNullOrEmpty(def.conditionKey) || string.IsNullOrEmpty(def.conditionKey);
            if (!isMatchkey)
            {
                Debug.Log($"Quest ID '{savedQ.questID}' conditionKey '{def.conditionKey}' does not match '{conditionKey}'.");
                continue;
            }
            // เช็คว่าประเภทตรงกัน และยังไม่เต็ม
            if (isMatchkey && savedQ.currentAmount < def.targetAmount && !savedQ.isClaimed)
            {
                Debug.Log($"อัปเดตเควส {def.questID} เพิ่ม {amount} จาก {savedQ.currentAmount} / {def.targetAmount}");
                savedQ.currentAmount += amount;
                if (savedQ.currentAmount > def.targetAmount) savedQ.currentAmount = def.targetAmount;
                isChanged = true;
            }
        }

        if (isChanged)
        {
            GameManager.Instance.SaveCurrentGame();
            if (contentContainer != null)
            {
                CreateUI();
            }
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
            // Update Ui
            UpdateUI();
            // 4. อัปเดตสถานะว่ารับแล้ว
            savedQ.isClaimed = true;

            // 5. บันทึกและรีเฟรชหน้าจอ
            GameManager.Instance.SaveCurrentGame();
            CreateUI(); // รีเฟรชปุ่มให้เป็นสีเทา/ขึ้นว่ารับแล้ว
        }
    }

    void UpdateUI()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameData != null && goldText != null)
        {
            goldText.text = $"{GameManager.Instance.CurrentGameData.profile.gold}";
        }
    }
}