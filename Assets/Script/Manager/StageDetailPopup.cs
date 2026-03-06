using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro; // ถ้าใช้ TextMeshPro
using System.Collections.Generic;
using UnityEngine.Localization.Settings;

public class StageDetailPopup : MonoBehaviour
{
    [System.Serializable]
    private class RuntimeStageConditionPayload
    {
        public string stageID;
        public List<RuntimeStarConditionData> conditions = new List<RuntimeStarConditionData>();
    }

    [System.Serializable]
    private class RuntimeStarConditionData
    {
        public StarCondition.ConditionType type;
        public int intValue;
        public float floatValue;
        public MainCategory category;
        public CardType cardType;
        public SubCategory subCategory;
    }

    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public Image botImage;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI deckInfoText;
    public TextMeshProUGUI[] starCriteriaTexts; // อาร์เรย์เก็บ Text เงื่อนไขดาว 3 ข้อ
    public TextMeshProUGUI statusText;         // แสดงสถานะ (ชนะแล้ว/ยังไม่เล่น/ดาวที่ได้)
    public Image completedBadge;               // ⭐ Badge สำหรับแสดง "COMPLETED"

    public TextMeshProUGUI TypeDeckText_StoryBattle;

    public Button startButton;
    public Button closeButton;
    // ตัวแปรเก็บข้อมูลด่านปัจจุบันที่กำลังดูอยู่
    private StageManager.StageData currentStageData;

    void Awake()
    {
        if (currentStageData == null)
        {
            Debug.LogWarning("⚠️ currentStageData ยังไม่ได้รับค่า! ตรวจสอบว่า StageManager เรียก Open(data) หรือยัง และ detailPopup ถูก assign ใน Inspector");
        }
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

        if (currentStageData.isStoryBattle)
        {
            Debug.Log("🔵 กำลังตั้งค่าเงื่อนไข Deck สำหรับ Story Battle...");
            // เช็คประเภทเด็ค
            // ตรงประเภท เปิดให้กดเริ่มเกมได้
            bool ischeckDeck = CheckDeck(GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId);
            startButton.interactable = ischeckDeck;

            if (ischeckDeck)
            {
                // ตรงประเภท
                if (TypeDeckText_StoryBattle != null)
                {
                    TypeDeckText_StoryBattle.text = LocalizationSettings.SelectedLocale.Identifier.Code == "th"
                    ? $"<color=green>ใช้ทั้งเด็คเป็นประเภท {GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId}</color>"
                    : $"<color=green>ใช้ทั้งเด็คเป็นประเภท {GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId}</color>";
                }
            }
            else
            {
                if (TypeDeckText_StoryBattle != null)
                {
                    TypeDeckText_StoryBattle.text = LocalizationSettings.SelectedLocale.Identifier.Code == "th"
                    ? $"<color=red>ใช้ทั้งเด็คเป็นประเภท {GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId}</color>"
                    : $"<color=red>ใช้ทั้งเด็คเป็นประเภท {GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId}</color>";
                }
            }

        }
    }

    private bool CheckDeck(string StoryId)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager.Instance เป็น null! ตรวจสอบว่า GameManager มีอยู่ในฉากและถูกตั้งค่าเป็น Singleton อย่างถูกต้อง");
            return false;
        }
        // 1. ดึงข้อมูล GameData จาก GameManager
        var data = GameManager.Instance.CurrentGameData; //
        if (data == null || data.decks == null || data.decks.Count == 0) return false;

        // 2. หาเด็คที่ผู้เล่นเลือกใช้งานอยู่ (อ้างอิงจาก selectedDeckId ที่เราคุยกันก่อนหน้า)
        // var currentDeck = data.decks.FirstOrDefault(d => d.deck_id == data.selectedDeckId);
        // if (currentDeck == null || currentDeck.card_ids_in_deck.Count == 0) return false;

        int selectedIndex = PlayerPrefs.GetInt("SelectedDeckIndex", 0);

        // ป้องกันกรณี Index เกินจำนวนเด็คที่มี
        if (selectedIndex < 0 || selectedIndex >= data.decks.Count) selectedIndex = 0;

        // ดึง DeckData ตามลำดับ Index
        DeckData currentDeck = data.decks[selectedIndex];
        if (currentDeck == null || currentDeck.card_ids_in_deck.Count == 0) return false;

        // 3. วนลูปเช็คการ์ดทุกใบในเด็ค
        foreach (string id in currentDeck.card_ids_in_deck)
        {
            // เรียกใช้ฟังก์ชัน GetCardByID ที่คุณเตรียมไว้
            CardData card = GameContentDatabase.Instance.GetCardByID(id);

            if (card != null)
            {
                MainCategory mainCategory = ChangeStoryBattleToMainCategory(StoryId);
                // ตรวจสอบว่า FromStroryId ของการ์ด ตรงกับ StoryId ของด่านหรือไม่
                // (ใช้ชื่อ FromStroryId ตาม typo ใน CardData ของคุณ)
                if (card.mainCategory != mainCategory)
                {
                    Debug.Log($"[CheckDeck] การ์ด {card.cardName} ไม่ใช่ประเภท {StoryId}");
                    return false; // พบการ์ดที่ไม่เข้าพวก
                }
            }
        }

        return true; // การ์ดทุกใบในเด็คตรงประเภททั้งหมด
    }

    private MainCategory ChangeStoryBattleToMainCategory(string StoryId)
    {
        switch (StoryId)
        {
            case "A01":
                return MainCategory.A01;
            case "A02":
                return MainCategory.A02;
            case "A03":
                return MainCategory.A03;
            default:
                Debug.LogWarning($"[ChangeStoryBattleToMainCategory] StoryId {StoryId} ไม่ตรงกับกรณีที่กำหนด");
                return MainCategory.General; // ค่า default ถ้าไม่ตรงกับกรณีใดๆ
        }
    }

    // ฟังก์ชันเปิด Popup และอัปเดตข้อมูล
    public void Open(StageManager.StageData data)
    {
        Debug.Log($"[POPUP] Open() ถูกเรียก สำหรับ {data.stageName}");

        currentStageData = data;

        if (currentStageData.isStoryBattle)
        {
            Debug.Log("🔵 กำลังตั้งค่าเงื่อนไข Deck สำหรับ Story Battle...");
            // เช็คประเภทเด็ค
            // ตรงประเภท เปิดให้กดเริ่มเกมได้
            bool ischeckDeck = CheckDeck(GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId);
            startButton.interactable = ischeckDeck;

            if (ischeckDeck)
            {
                // ตรงประเภท
                if (TypeDeckText_StoryBattle != null)
                {
                    TypeDeckText_StoryBattle.text = LocalizationSettings.SelectedLocale.Identifier.Code == "th"
                    ? $"<color=green>ใช้ทั้งเด็คเป็นประเภท {GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId}</color>"
                    : $"<color=green>ใช้ทั้งเด็คเป็นประเภท {GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId}</color>";
                }
            }
            else
            {
                if (TypeDeckText_StoryBattle != null)
                {
                    TypeDeckText_StoryBattle.text = LocalizationSettings.SelectedLocale.Identifier.Code == "th"
                    ? $"<color=red>ใช้ทั้งเด็คเป็นประเภท {GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId}</color>"
                    : $"<color=red>ใช้ทั้งเด็คเป็นประเภท {GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId}</color>";
                }
            }

        }

        // 1. อัปเดตข้อความต่างๆ
        if (LocalizationSettings.SelectedLocale.Identifier.Code == "th")
        {
            titleText.text = data.stageName_th;
            levelText.text = $"เลเวล: {data.botLevel}";
            deckInfoText.text = $"เด็คคู่ต่อสู้:\n{data.deckDescription_th}";

        }
        else
        {
            titleText.text = data.stageName;
            levelText.text = $"Level: {data.botLevel}";
            deckInfoText.text = $"Enemy Deck:\n{data.deckDescription}";
        }

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
        List<bool> completedMissions = progress != null ? progress.completedStarMissions : null;
        if (starCriteriaTexts.Length > 0)
            for (int i = 0; i < starCriteriaTexts.Length; i++)
            {
                if (LocalizationSettings.SelectedLocale.Identifier.Code == "th")
                {
                    // Thai
                    if (i < data.starConditions.Count)
                    {
                        bool hasMissionState = completedMissions != null && i < completedMissions.Count;
                        bool starCompleted = hasMissionState ? completedMissions[i] : (i < starsEarned);

                        if (starCompleted)
                        {
                            // ทำแล้ว = [X] + สีเขียว
                            starCriteriaTexts[i].text = $"[X] {data.starConditions[i].description_th}";
                            starCriteriaTexts[i].color = new Color(0.2f, 1f, 0.2f); // สีเขียว
                        }
                        else
                        {
                            // ยังไม่ทำ = [ ] + สีขาว
                            starCriteriaTexts[i].text = $"[ ] {data.starConditions[i].description_th}";
                            starCriteriaTexts[i].color = Color.white;
                        }
                    }
                    else
                    {
                        Debug.Log("I'm Here");
                        starCriteriaTexts[i].text = ""; // เคลียร์ข้อความถ้าไม่มี
                    }
                }
                else
                {
                    // Englih
                    if (i < data.starConditions.Count)
                    {
                        bool hasMissionState = completedMissions != null && i < completedMissions.Count;
                        bool starCompleted = hasMissionState ? completedMissions[i] : (i < starsEarned);

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
                    if (btnText != null) btnText.text = LocalizationSettings.SelectedLocale.Identifier.Code == "th" ? "เล่นอีกครั้ง" : "REPLAY";
                }
            }
            else
            {
                Debug.Log($"[POPUP] Stage NOT CLEARED: progress={progress}, isCompleted={(progress != null ? progress.isCompleted : false)}");
                statusText.text = LocalizationSettings.SelectedLocale.Identifier.Code == "th" ? "ยังไม่เคลียร์" : "⚪ NOT CLEARED";
                statusText.color = Color.gray;

                // ซ่อน badge
                if (completedBadge != null)
                    completedBadge.gameObject.SetActive(false);

                // เปลี่ยนสี Start button เป็นเขียว (Start)
                if (startButton != null)
                {
                    startButton.image.color = new Color(0.2f, 1f, 0.2f); // สีเขียว
                    var btnText = startButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null) btnText.text = LocalizationSettings.SelectedLocale.Identifier.Code == "th" ? "เริ่มเกม" : "START";
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

        // เพิ่มเงื่อนไขเช็ค Deck

        // บันทึก Stage ID ลงหน่วยความจำ (เผื่อระบบ Battle ต้องอ่าน ID นี้)
        Debug.Log($"✅ กำลังเริ่มด่าน: {currentStageData.stageID}");
        PlayerPrefs.SetString("CurrentStageID", currentStageData.stageID);

        // บันทึก mission condition ของด่านนี้ เพื่อให้ Battle scene คำนวณผลรายข้อได้ถูกต้อง
        RuntimeStageConditionPayload payload = new RuntimeStageConditionPayload
        {
            stageID = currentStageData.stageID,
            conditions = new List<RuntimeStarConditionData>()
        };

        if (currentStageData.starConditions != null)
        {
            foreach (var condition in currentStageData.starConditions)
            {
                if (condition == null) continue;

                payload.conditions.Add(new RuntimeStarConditionData
                {
                    type = condition.type,
                    intValue = condition.intValue,
                    floatValue = condition.floatValue,
                    category = condition.category,
                    cardType = condition.cardType,
                    subCategory = condition.subCategory
                });
            }
        }

        PlayerPrefs.SetString("CurrentStageConditionsJson", JsonUtility.ToJson(payload));
        PlayerPrefs.Save();

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