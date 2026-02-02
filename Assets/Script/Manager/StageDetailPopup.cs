using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // ถ้าใช้ TextMeshPro
using System.Collections.Generic;

public class StageDetailPopup : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public Image botImage;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI deckInfoText;
    public TextMeshProUGUI[] starCriteriaTexts; // อาร์เรย์เก็บ Text เงื่อนไขดาว 3 ข้อ
    public TextMeshProUGUI statusText;         // แสดงสถานะ (ชนะแล้ว/ยังไม่เล่น/ดาวที่ได้)
    public Image completedBadge;               // ⭐ Badge สำหรับแสดง "COMPLETED"
    public Button startButton;
    public Button closeButton;
    // ตัวแปรเก็บข้อมูลด่านปัจจุบันที่กำลังดูอยู่
    private StageManager.StageData currentStageData;

    void Awake()
    {
        // ตั้งค่าปุ่มปิดและปุ่มเริ่ม
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
            Debug.Log("✅ Close button listener ตั้งค่าเสร็จ");
        }
        else
        {
            Debug.LogError("❌ closeButton ไม่ได้ reference ใน Inspector!");
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartClick);
            Debug.Log("✅ Start button listener ตั้งค่าเสร็จ");
        }
        else
        {
            Debug.LogError("❌ startButton ไม่ได้ reference ใน Inspector!");
        }
    }

    // ฟังก์ชันเปิด Popup และอัปเดตข้อมูล
    public void Open(StageManager.StageData data)
    {
        Debug.Log($"[POPUP] Open() ถูกเรียก สำหรับ {data.stageName}");
        
        currentStageData = data;

        // 1. อัปเดตข้อความต่างๆ
        titleText.text = data.stageName;
        levelText.text = $"Level: {data.botLevel}";
        deckInfoText.text = $"Enemy Deck:\n{data.deckDescription}";
        
        // 2. อัปเดตรูปบอท (ถ้ามี)
        if (data.botSprite != null)
        {
            botImage.sprite = data.botSprite;
            botImage.gameObject.SetActive(true);
        }
        else
        {
            botImage.gameObject.SetActive(false);
        }

        // 3. อัปเดตเงื่อนไขดาว (Star Criteria)
        // ดึงข้อมูล progress เพื่อเช็คว่าดาวไหนได้แล้ว
        var progress = GameManager.Instance != null ? GameManager.Instance.GetStageProgress(data.stageID) : null;
        int starsEarned = (progress != null && progress.isCompleted) ? progress.starsEarned : 0;
        
        for (int i = 0; i < starCriteriaTexts.Length; i++)
        {
            if (i < data.starConditions.Count)
            {
                // เช็คว่าดาวนี้ได้แล้วหรือยัง (star index 0, 1, 2 = ดาว 1, 2, 3)
                bool starCompleted = (i < starsEarned);
                
                if (starCompleted)
                {
                    // ทำแล้ว = [X] + สีเขียว
                    starCriteriaTexts[i].text = $"[X] {data.starConditions[i].description}";
                    starCriteriaTexts[i].color = new Color(0.2f, 1f, 0.2f); // สีเขียว
                }
                else
                {
                    // ยังไม่ทำ = [ ] + สีขาว
                    starCriteriaTexts[i].text = $"[ ] {data.starConditions[i].description}";
                    starCriteriaTexts[i].color = Color.white;
                }
            }
            else
            {
                starCriteriaTexts[i].text = ""; // เคลียร์ข้อความถ้าไม่มี
            }
        }

        // 4. อัปเดตสถานะ (ชนะแล้ว/ยังไม่เล่น)
        Debug.Log($"[POPUP] statusText = {(statusText != null ? "Found" : "NULL")}");
        Debug.Log($"[POPUP] GameManager.Instance = {(GameManager.Instance != null ? "Found" : "NULL")}");
        
        if (statusText != null)
        {
            Debug.Log($"[POPUP] Stage {data.stageID}: Progress = {(progress != null ? "Found" : "NULL")}");
            
            if (progress != null && progress.isCompleted)
            {
                Debug.Log($"[POPUP] Stage COMPLETED: {progress.starsEarned}/3 Stars");
                statusText.text = $"✅ COMPLETED! {progress.starsEarned}/3 Stars";
                statusText.color = new Color(0.2f, 1f, 0.2f); // สีเขียว
                
                // แสดง badge "COMPLETED"
                if (completedBadge != null)
                {
                    completedBadge.gameObject.SetActive(true);
                    completedBadge.color = new Color(1f, 0.84f, 0f); // สีทอง
                }
                
                // เปลี่ยนสี Start button เป็นน้ำเงิน (Replay)
                if (startButton != null)
                {
                    startButton.image.color = new Color(0.2f, 0.6f, 1f); // สีน้ำเงิน
                    var btnText = startButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null) btnText.text = "REPLAY";
                }
            }
            else
            {
                Debug.Log($"[POPUP] Stage NOT CLEARED: progress={progress}, isCompleted={(progress != null ? progress.isCompleted : false)}");
                statusText.text = "⚪ NOT CLEARED";
                statusText.color = Color.gray;
                
                // ซ่อน badge
                if (completedBadge != null)
                    completedBadge.gameObject.SetActive(false);
                
                // เปลี่ยนสี Start button เป็นเขียว (Start)
                if (startButton != null)
                {
                    startButton.image.color = new Color(0.2f, 1f, 0.2f); // สีเขียว
                    var btnText = startButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null) btnText.text = "START";
                }
            }
        }

        // 5. แสดงหน้าต่าง
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void OnStartClick()
    {
        Debug.Log("🔵 OnStartClick ถูกเรียก!");

        if (currentStageData == null)
        {
            Debug.LogError("❌ currentStageData เป็น null! ตรวจสอบว่า StageManager เรียก Open(data) หรือยัง และ detailPopup ถูก assign ใน Inspector");
            return;
        }

        // บันทึก Stage ID ลงหน่วยความจำ (เผื่อระบบ Battle ต้องอ่าน ID นี้)
        Debug.Log($"✅ กำลังเริ่มด่าน: {currentStageData.stageID}");
        PlayerPrefs.SetString("CurrentStageID", currentStageData.stageID);

        // ตรวจว่า Scene 'Battle' อยู่ใน Build Settings หรือไม่
        bool canLoad = Application.CanStreamedLevelBeLoaded("Battle");
        if (!canLoad)
        {
            Debug.LogError("❌ ไม่พบ Scene 'Battle' ใน Build Settings! ไปที่ File > Build Settings แล้วกด Add Open Scenes หรือเพิ่ม Assets/Scenes/Battle.unity");
            return;
        }

        // โหลดฉากแบบ async เพื่อไม่ให้ค้าง
        Debug.Log("🟡 กำลังโหลด Battle Scene (async)...");
        SceneManager.LoadSceneAsync("Battle");
    }
}