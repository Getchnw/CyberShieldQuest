using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    [System.Serializable]
    public class StageData
    {
        [Header("UI Configuration")]
        public string stageName;        // ชื่อที่แสดงบนหัวข้อ
        public string stageID;          // ID ภาษาอังกฤษ (ห้ามซ้ำ) เช่น L1_A01, L2_Mix1
        public Button stageButton;      // ปุ่มกดเลือกด่าน
        public GameObject lockIcon;     // รูปแม่กุญแจ
        
        // ⭐ Prefab ของดาวที่จะ instantiate บนปุ่ม
        public Sprite starSprite;       // รูปดาว
        [Range(0.15f, 1f)]
        public float starSize = 0.4f;   // ขนาดดาว (เทียบกับปุ่ม) 0.15-1.0

        [Header("Popup Details (ข้อมูลแสดงในหน้าต่าง)")]
        public Sprite botSprite;        // รูปบอท
        public int botLevel;            // เลเวลบอท
        [TextArea]
        public string deckDescription;  // คำบรรยายเด็คบอท
        
        // ⭐ เปลี่ยนเป็น StarCondition แทน string
        public List<StarCondition> starConditions; // เงื่อนไขดาว 3 ข้อ

        [Header("Unlock Conditions (เงื่อนไขการปลดล็อค)")]
        // 1. ต้องเรียนจบบทไหนบ้าง (1=A01, 2=A02, 3=A03)
        public List<int> requiredChapters;

        // 2. ต้องชนะด่านไหนมาก่อน (ใส่ StageID ของด่านก่อนหน้า)
        public List<string> requiredPrevStages;

        [Header("Battle Settings (ส่งไปฉากต่อสู้)")]
        public List<MainCategory> botDecks; // บอทจะใช้การ์ดหมวดไหนบ้าง
        
        private Transform starsContainer; // Container สำหรับเก็บดาว
        
        /// <summary>
        /// ตรวจสอบเงื่อนไขดาวจาก BattleStatistics
        /// </summary>
        public int CalculateStarsEarned(BattleStatistics stats)
        {
            if (stats == null || starConditions == null) return 0;
            
            int stars = 0;
            foreach (var condition in starConditions)
            {
                if (condition.CheckCondition(stats))
                    stars++;
            }
            return stars;
        }
        
        /// <summary>
        /// อัปเดตการแสดงดาวบนปุ่ม
        /// </summary>
        public void UpdateStarDisplay(int starsEarned)
        {
            if (stageButton == null || starSprite == null) return;

            RectTransform buttonRect = stageButton.GetComponent<RectTransform>();
            if (buttonRect == null) return;

            // เปลี่ยนสีปุ่มตามสถานะ
            Image buttonImage = stageButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (starsEarned > 0)
                {
                    // ทำไปแล้ว = เหลือง/ทอง
                    buttonImage.color = new Color(1f, 0.84f, 0f); // สีทอง
                }
                else
                {
                    // ยังไม่ทำ = สีปกติ (รักษาสีเดิม)
                    buttonImage.color = Color.white;
                }
            }

            // ลบดาวเก่าทั้งหมด
            foreach (Transform child in buttonRect)
            {
                if (child.name.StartsWith("Star_"))
                    Object.Destroy(child.gameObject);
            }

            // สร้างดาวใหม่ตามจำนวน
            float buttonWidth = buttonRect.rect.width;
            float starWidth = buttonWidth * starSize;
            
            for (int i = 0; i < starsEarned && i < 3; i++)
            {
                GameObject starGO = new GameObject($"Star_{i}");
                starGO.transform.SetParent(buttonRect);
                starGO.transform.localScale = Vector3.one;

                Image starImage = starGO.AddComponent<Image>();
                starImage.sprite = starSprite;
                starImage.raycastTarget = false; // ป้องกันการบล็อก click
                
                RectTransform starRect = starGO.GetComponent<RectTransform>();
                starRect.anchorMin = new Vector2(1, 1);
                starRect.anchorMax = new Vector2(1, 1);
                starRect.pivot = new Vector2(1, 1);
                starRect.sizeDelta = new Vector2(starWidth, starWidth);
                starRect.anchoredPosition = new Vector3(-(i * (starWidth + 2)) - 5, -5, 0);
            }
        }
    }

    [Header("Manager Settings")]
    public List<StageData> allStages;   // ลากปุ่มด่านทั้งหมดมาใส่ตรงนี้
    public StageDetailPopup detailPopup; // ลากหน้าต่าง Popup มาใส่ตรงนี้

    void Start()
    {
        Debug.Log("🟢 StageManager Start() เริ่มทำงาน");

        // ซ่อน Popup ไว้ก่อนเสมอตอนเริ่ม
        if (detailPopup != null) detailPopup.Close();

        // อัปเดตสถานะด่าน (ล็อค/ปลดล็อค)
        UpdateStageStatus();

        Debug.Log($"🟢 มีด่านทั้งหมด: {allStages.Count} ด่าน");
    }

    // ฟังก์ชันหลักสำหรับเช็คและอัปเดตปุ่ม
    public void UpdateStageStatus()
    {
        Debug.Log("🔵 UpdateStageStatus() ถูกเรียก");

        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ ไม่พบ GameManager ใน Scene!");
            return;
        }

        // ดึงข้อมูลการเรียน (Post-Test)
        var status = GameManager.Instance.CurrentGameData.statusPostTest;

        foreach (var stage in allStages)
        {
            // 1. เช็คเงื่อนไขบทเรียน (Chapters)
            bool passChapters = CheckChapterUnlock(stage.requiredChapters, status);

            // 2. เช็คเงื่อนไขด่านก่อนหน้า (Linear Progression)
            bool passPrevStages = CheckPrevStageUnlock(stage.requiredPrevStages);

            // ต้องผ่านทั้ง 2 เงื่อนไขถึงจะเล่นได้
            bool isUnlocked = passChapters && passPrevStages;

            // --- อัปเดตหน้าตาปุ่ม ---
            stage.stageButton.interactable = isUnlocked;

            // เปิด/ปิด ไอคอนกุญแจ
            if (stage.lockIcon != null)
                stage.lockIcon.SetActive(!isUnlocked);

            // เปลี่ยนสีปุ่ม (ขาว=เล่นได้, เทา=ล็อค)
            stage.stageButton.image.color = isUnlocked ? Color.white : Color.gray;

            // ⭐ อัปเดตดาว
            var progress = GameManager.Instance.GetStageProgress(stage.stageID);
            if (progress != null)
            {
                Debug.Log($"Stage {stage.stageID}: {progress.starsEarned}/3 Stars");
                stage.UpdateStarDisplay(progress.starsEarned);
            }
            else
            {
                Debug.Log($"⚪ Stage {stage.stageID}: ยังไม่เล่น");
                stage.UpdateStarDisplay(0); // ยังไม่เคยเล่น = 0 ดาว
            }

            // --- จัดการ Event การกดปุ่ม ---
            stage.stageButton.onClick.RemoveAllListeners(); // ล้างคำสั่งเก่าออกก่อน
            if (isUnlocked)
            {
                // ถ้าปลดล็อค -> กดแล้วเปิด Popup
                stage.stageButton.onClick.AddListener(() => OpenDetail(stage));
                Debug.Log($"✅ เพิ่ม Event ให้ปุ่ม: {stage.stageName}");
            }
            else
            {
                Debug.Log($"🔒 ด่าน {stage.stageName} ยังล็อคอยู่");
            }
        }
    }

    // ฟังก์ชันเปิด Popup
    void OpenDetail(StageData stage)
    {
        Debug.Log($"🎯 กดปุ่มด่าน: {stage.stageName}");

        if (detailPopup != null)
        {
            detailPopup.Open(stage);
        }
        else
        {
            Debug.LogError("⚠️ ลืมลาก StageDetailPopup ใส่ใน Inspector ของ StageManager!");
        }
    }

    // ---------------------------------------------------------
    // Helper Functions (ฟังก์ชันช่วยเช็คเงื่อนไข)
    // ---------------------------------------------------------

    // เช็คว่าเรียนจบครบตามที่กำหนดไหม
    bool CheckChapterUnlock(List<int> reqChapters, PlayerPostTest status)
    {
        // ถ้า List ว่างเปล่า แปลว่าไม่ต้องการบทเรียนไหนเลย -> ให้ผ่าน
        if (reqChapters == null || reqChapters.Count == 0) return true;

        foreach (int chapID in reqChapters)
        {
            if (chapID == 1 && !status.hasSucessPost_A01) return false;
            if (chapID == 2 && !status.hasSucessPost_A02) return false;
            if (chapID == 3 && !status.hasSucessPost_A03) return false;
        }
        return true;
    }

    // เช็คว่าชนะด่านก่อนหน้าครบไหม
    bool CheckPrevStageUnlock(List<string> reqStages)
    {
        // ถ้า List ว่างเปล่า แปลว่าไม่ต้องผ่านด่านไหนมาก่อน (เช่น ด่านแรกสุด) -> ให้ผ่าน
        if (reqStages == null || reqStages.Count == 0) return true;

        foreach (string prevID in reqStages)
        {
            // เรียกใช้ฟังก์ชัน IsStageCleared จาก GameManager (ที่เราเพิ่มไปก่อนหน้านี้)
            if (!GameManager.Instance.IsStageCleared(prevID))
            {
                return false; // มีด่านนึงยังไม่ผ่าน -> ล็อคทันที
            }
        }
        return true;
    }
}