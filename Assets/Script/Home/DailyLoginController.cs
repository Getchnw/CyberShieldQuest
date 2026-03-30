using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;

public class DailyLoginController : MonoBehaviour
{
    //[Header("Data")]
    // public DailyRewardData rewardConfig; // ตรวจสอบว่าชื่อ Class ตรงกับไฟล์ ScriptableObject ของคุณ
    public TextMeshProUGUI goldText;
    [Header("UI References")]

    public GameObject loginPopup;       // ตัว Popup ทั้งหมด
    public Button claimButton;          // ปุ่ม "CLAIM"
    public TextMeshProUGUI nextRewardTimeText;     // ข้อความ "Time unit Next time"

    [Header("Day Slots UI")]
    public Image[] dayIcons;            // ช่องใส่รูปไอคอน Day 1-7
    public TextMeshProUGUI[] dayAmounts;           // ช่องใส่จำนวนรางวัล
    public GameObject[] claimedMarks;   // รูป "ติ๊กถูก" (Checkmark) ของ Day 1-7
    public GameObject[] dayHighlightBGs;


    // ตัวแปรภายใน
    private int streakDayToClaim;
    private bool canClaimToday;
    private DailyRewardData rewardConfig; // โหลดข้อมูลรางวัลรายวัน

    void Start()
    {
        // โหลดข้อมูลรางวัลจาก Database
        rewardConfig = GameContentDatabase.Instance.GetDailyRewardData();

        SetupRewardIcons();

        if (claimButton != null)
            claimButton.onClick.AddListener(OnClaimClicked);

        // แสดงหน้าต่างLogin หลังจากเข้าเกมมา 
        // เงื่อนไขการแสดง:
        // 1. ยังไม่ได้รับรางวัลวันนี้ (lastLogin != today)
        // 2. ไม่ใช่ New Game หรือ tutorial Home จบแล้ว (หลีกเลี่ยงชนกับ GuidedTutorial)
        // 3. ไม่ติด Delay (มีของรางวัลให้รับ = canClaimToday จะเป็น true)
        // if (GameManager.Instance != null && GameManager.Instance.CurrentGameData != null)
        // {
        //     var data = GameManager.Instance.CurrentGameData.dailyLoginData;
        //     var tutorial = GameManager.Instance.CurrentGameData.tutorialData; // เข้าถึงข้อมูล Tutorial

        //     // 1. ถ้ายังดู Tutorial หน้า Home ไม่จบ -> จบการทำงานเลย (ไม่โชว์ Popup)
        //     if (!tutorial.hasSeenTutorial_Home)
        //     {
        //         return;
        //     }

        //     DateTime today = DateTime.Now.Date;
        //     DateTime lastLogin = DateTime.MinValue;

        //     if (!string.IsNullOrEmpty(data.lastClaimedDate))
        //     {
        //         lastLogin = DateTime.Parse(data.lastClaimedDate);
        //     }

        //     if (lastLogin != today)
        //     {
        //         OnOpenPopup();
        //     }

        //     // // แสดง popup เมื่อ: ยังไม่ได้รับวันนี้ AND (ไม่ใช่ new game OR tutorial จบแล้ว)
        //     // bool canShowPopup = (lastLogin != today) &&
        //     //                     (!GameManager.Instance.CurrentGameData.isNewGameStarted ||
        //     //                      GameManager.Instance.CurrentGameData.tutorialData.hasSeenTutorial_Home);

        //     // if (canShowPopup)
        //     // {
        //     //     OnOpenPopup();
        //     // }
        // }
    }

    public void OnOpenPopup()
    {
        if (loginPopup != null) loginPopup.SetActive(true);
        CheckLoginStatus();
    }

    void SetupRewardIcons()
    {
        if (rewardConfig == null || rewardConfig.rewards == null) return;

        for (int i = 0; i < dayIcons.Length && i < rewardConfig.rewards.Length; i++)
        {
            if (dayIcons[i] != null)
                if (rewardConfig.rewards[i].type == RewardType.Gold)
                    dayIcons[i].sprite = rewardConfig.rewards[i].icon; // ใช้ไอคอนจาก Struct
                else if (rewardConfig.rewards[i].type == RewardType.Card)
                    dayIcons[i].sprite = rewardConfig.rewards[i].card.artwork; // ใช้รูปการ์ดจาก CardData

            if (dayAmounts != null && i < dayAmounts.Length && dayAmounts[i] != null)
                // dayAmounts[i].text = "x" + rewardConfig.rewards[i].amount;
                if (rewardConfig.rewards[i].type == RewardType.Gold)
                    dayAmounts[i].text = "x " + rewardConfig.rewards[i].amount.ToString() + " G";
                else if (rewardConfig.rewards[i].type == RewardType.Card)
                    dayAmounts[i].text = rewardConfig.rewards[i].rewardName + " x" + rewardConfig.rewards[i].amount.ToString();
        }
    }

    void Update()
    {
        if (!canClaimToday && nextRewardTimeText != null)
        {
            TimeSpan timeLeft = DateTime.Now.Date.AddDays(1) - DateTime.Now;
            nextRewardTimeText.text = string.Format("Reset in: {0:D2}:{1:D2}:{2:D2}",
                timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds);
        }
        else if (canClaimToday && nextRewardTimeText != null)
        {
            nextRewardTimeText.text = LocalizationSettings.SelectedLocale.Identifier.Code == "en"
                                        ? "Ready to Claim!"
                                        : "รับของรางวัล";
        }

        var data = GameManager.Instance.CurrentGameData.dailyLoginData;
        var tutorial = GameManager.Instance.CurrentGameData.tutorialData;
        DateTime today = DateTime.Now.Date;
        DateTime lastLogin = DateTime.MinValue;

        if (!string.IsNullOrEmpty(data.lastClaimedDate))
        {
            lastLogin = DateTime.Parse(data.lastClaimedDate);
        }

        if (lastLogin != today && tutorial.hasSeenTutorial_Home)
        {
            OnOpenPopup();
        }

    }

    // --- 1. เช็คสถานะตอนเปิดหน้าต่าง ---
    public void CheckLoginStatus()
    {
        if (GameManager.Instance == null) return;

        // 🛠️ แก้ไขจุดที่ 1: ใช้ชื่อ .dailyLogin ให้ตรงกันทั้งไฟล์
        var data = GameManager.Instance.CurrentGameData.dailyLoginData;

        DateTime today = DateTime.Now.Date;
        DateTime lastLogin = DateTime.MinValue; // ประกาศตัวแปรชื่อ lastLogin

        if (!string.IsNullOrEmpty(data.lastClaimedDate))
        {
            // 🛠️ แก้ไขจุดที่ 2: ใช้ lastLogin รับค่า (เดิมคุณใช้ lastClaimed ซึ่งไม่มีตัวตน)
            lastLogin = DateTime.Parse(data.lastClaimedDate);
        }

        int currentStreak = data.currentStreak;
        if (currentStreak <= 0) currentStreak = 1;
        // Debug.Log("Last Login: " + lastLogin.ToString("yyyy-MM-dd") + ", Current Streak: " + currentStreak);
        Debug.Log("Current Streak: " + currentStreak);
        // 🛠️ แก้ไข: ใช้ lastLogin ในการเช็ค
        if (lastLogin == today)
        {
            canClaimToday = false;
        }
        else
        {
            canClaimToday = true;

            if ((today - lastLogin).Days > 1 && !string.IsNullOrEmpty(data.lastClaimedDate))
            {
                currentStreak = 1; // รีเซ็ต
            }
            else if (!string.IsNullOrEmpty(data.lastClaimedDate))
            {
                currentStreak++; // ต่อเนื่อง
            }
            else
            {
                currentStreak = 1; // เพิ่งเล่นครั้งแรก
            }

            if (currentStreak > 7) currentStreak = 1;
        }

        streakDayToClaim = currentStreak;
        UpdateUI();
    }

    // --- 2. การกดปุ่ม CLAIM ---
    void OnClaimClicked()
    {
        if (!canClaimToday) return;

        GiveReward(streakDayToClaim);
        AudioManager.Instance.PlaySFX("ButtonClick");

        // บันทึกข้อมูล
        var data = GameManager.Instance.CurrentGameData.dailyLoginData;
        data.lastClaimedDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
        data.currentStreak = streakDayToClaim;

        GameManager.Instance.SaveCurrentGame();

        canClaimToday = false;
        UpdateUI();
    }

    // --- 3. จัดการหน้าตา UI ---
    void UpdateUI()
    {
        if (claimButton != null)
        {
            Debug.Log("Setting claim button interactable in UpdateUI: " + canClaimToday);
            claimButton.interactable = canClaimToday;
        }
        for (int i = 0; i < claimedMarks.Length; i++)
        {
            Debug.Log("Updating claimed mark for day " + (i + 1));
            if (claimedMarks[i] == null) continue;

            int dayIndex = i + 1;

            if (dayIndex < streakDayToClaim)
            {
                claimedMarks[i].SetActive(true); // รับไปแล้ว
            }
            else if (dayIndex == streakDayToClaim)
            {
                claimedMarks[i].SetActive(!canClaimToday); // วันนี้ (โชว์ถ้ากดรับแล้ว)
            }
            else
            {
                claimedMarks[i].SetActive(false); // อนาคต
            }
        }

    }

    void GiveReward(int day)
    {
        int index = day - 1;
        if (rewardConfig == null || index >= rewardConfig.rewards.Length) return;

        var reward = rewardConfig.rewards[index];

        // 🛠️ เช็คประเภทรางวัล (ต้องตรงกับ Enum ใน RewardData)
        if (reward.type == RewardType.Gold)
        {
            GameManager.Instance.AddGold(reward.amount);
            Debug.Log($"Daily Reward: Added {reward.amount} Gold");
            UpdateUIGold();
        }
        else if (reward.type == RewardType.Card)
        {
            // 🛠️ แก้ไขจุดที่ 3: เลือกใช้วิธีใดวิธีหนึ่งให้ตรงกับ Struct ของคุณ

            // กรณีที่ 1: ถ้า Struct เก็บเป็น "Object CardData" (ลากไฟล์ใส่)
            GameManager.Instance.AddCardToInventory(reward.card.card_id, reward.amount);

            // // กรณีที่ 2: ถ้า Struct เก็บเป็น "String ID" (พิมพ์ชื่อ ID)
            // GameManager.Instance.AddCardToInventory(reward.cardID, reward.amount);

            Debug.Log($"Daily Reward: Added Card ID {reward.card.card_id}");
        }
    }

    void UpdateUIGold()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameData != null && goldText != null)
        {
            goldText.text = $"{GameManager.Instance.CurrentGameData.profile.gold}";
        }
    }
}