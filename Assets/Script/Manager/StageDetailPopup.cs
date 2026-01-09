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
        for (int i = 0; i < starCriteriaTexts.Length; i++)
        {
            if (i < data.starConditions.Count)
                starCriteriaTexts[i].text = $"★ {data.starConditions[i]}";
            else
                starCriteriaTexts[i].text = ""; // เคลียร์ข้อความถ้าไม่มี
        }

        // 4. แสดงหน้าต่าง
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