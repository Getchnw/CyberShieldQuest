using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Localization.Settings;

public class StoryEventController : MonoBehaviour
{
    [Header("UI Panels (ลากใส่)")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject quizBackgroundPanel;
    [SerializeField] private UI_QuizController quizController;

    [Header("Dialogue UI (ลากใส่)")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button nextButton; // ปุ่ม "คลิกไปเรื่อยๆ"
    [SerializeField] private Image PlayerImage;
    [SerializeField] private Image OracleImage;
    [SerializeField] private TextMeshProUGUI NameDialog;

    [Header("Log UI (ลากใส่)")]
    [SerializeField] private GameObject logWindowPanel; // ตัวหน้าต่าง Log ทั้งหมด (เอาไว้เปิด/ปิด)
    [SerializeField] private Transform logContainer;    // Content ใน Scroll View
    [SerializeField] private GameObject layoutLogPrefab; // Prefab แถวข้อความ 1 อัน


    // --- ตัวแปรจัดการสถานะ ---
    private int currentChapterID;
    private List<ChapterEventsData> allChapterEvents; // "เพลย์ลิสต์"
    private int currentEventIndex;
    private List<DialogueLinesData> currentDialogueLines;
    private int currentLineIndex;

    // ตัวแปรสำหรับ Typewriter
    private float typingSpeed = 0.05f;
    private Coroutine typingCoroutine;
    private bool isTyping = false;       // เช็คว่ากำลังพิมพ์อยู่ไหม
    private string targetText = "";      // เก็บข้อความเต็มๆ ไว้เผื่อกดข้าม

    void Start()
    {
        // 1. ตั้งค่าปุ่ม
        nextButton.onClick.AddListener(OnNextLineClicked);
        quizController.OnQuizCompleted += OnQuizFinished; // "เงี่ยหูฟัง" Quiz

        // ปิด ฺBackground
        dialoguePanel.SetActive(false);
        // quizBackgroundPanel.SetActive(false);
        quizBackgroundPanel.SetActive(true);
        if (logWindowPanel != null) logWindowPanel.SetActive(false); // ปิดหน้า Log ก่อน

        // 2. ดึงข้อมูล
        currentChapterID = GameManager.Instance.CurrentGameData.selectedStory.lastSelectedchapterId;
        allChapterEvents = GameContentDatabase.Instance.GetChapterEventsByChapterID(currentChapterID);

        //เริ่มที่ Event แรกเสมอ
        currentEventIndex = 0;

        // 4. เริ่ม Event แรก
        LoadCurrentEvent();
    }

    void OnDestroy()
    {
        if (quizController != null)
            quizController.OnQuizCompleted -= OnQuizFinished;
    }

    //โหลด Event ตามลำดับ (Dialogue หรือ Quiz)
    void LoadCurrentEvent()
    {
        // เช็คว่าจบ Chapter หรือยัง
        if (currentEventIndex >= allChapterEvents.Count) // จบแล้ว
        {
            Debug.Log("Chapter Completed!");
            dialoguePanel.SetActive(false);
            quizController.gameObject.SetActive(false);
            UpdateDailyQuest();
            // (คุณอาจจะโหลด Scene กลับหน้า Chapter Select ที่นี่)
            SceneManager.LoadScene("Template_select_chapter_story");
            return;
        }

        // ดึง Event ปัจจุบัน
        ChapterEventsData eventData = allChapterEvents[currentEventIndex];

        // ตรวจสอบประเภท Event
        if (eventData.type == ChapterEventsData.EventType.Dialogue) //
        {
            StartDialogueEvent(eventData);
        }
        else if (eventData.type == ChapterEventsData.EventType.Quiz) //
        {
            StartQuizEvent(eventData);
        }
    }

    /// <summary>
    /// เริ่มฉาก "บทพูดคุย"
    /// </summary>
    void StartDialogueEvent(ChapterEventsData eventData)
    {
        dialoguePanel.SetActive(true);
        quizBackgroundPanel.SetActive(false);

        // โหลดข้อมูล Dialogue (ScriptableObject)
        DialogsceneData sceneData = eventData.dialogueReference;
        backgroundImage.sprite = sceneData.backgroundScene;

        // โหลด "บทพูด" (ScriptableObject)
        currentDialogueLines = GameContentDatabase.Instance.GetDialogueLinesByScene(sceneData.scene_id);

        // เริ่มที่บรรทัดแรก
        currentLineIndex = 0;
        UpdateDialogueUI();
    }

    // ฟังก์ชันรวมสำหรับอัปเดตหน้าจอและเพิ่ม Log
    void UpdateDialogueUI()
    {
        // // 1. ดึงข้อความปัจจุบัน
        // string currentText = string.Join("\n", currentDialogueLines[currentLineIndex].Dialog_Text);
        // // dialogueText.text = currentText;
        // // เช็คว่าต้องแปลภาษาไหม
        // if (GameManager.Instance.CurrentGameData.selectedStory.isTranstale)
        // {
        //     foreach(var line in currentDialogueLines[currentLineIndex].Dialog_Text)
        //     {
        //         currentText = LanguageBridge.Get(line);
        //     }
        // }

        // 1. สร้าง List ชั่วคราวเพื่อเก็บประโยคที่แปลแล้ว
        List<string> translatedLines = new List<string>();

        // 2. วนลูปแปลทีละประโยค แล้ว Add ลงใน List
        if (LocalizationSettings.SelectedLocale.Identifier.Code == "en")
        {
            foreach (var line in currentDialogueLines[currentLineIndex].Dialog_Text)
            {
                translatedLines.Add(LanguageBridge.Get(line));
            }
        }
        else
        {
            // ถ้าไม่ต้องแปล ก็ใส่ข้อความเดิมลงไปเลย
            translatedLines.AddRange(currentDialogueLines[currentLineIndex].Dialog_Text);
        }

        // 3. นำ List ที่แปลครบทุกประโยคแล้ว มาต่อกันด้วย \n (ขึ้นบรรทัดใหม่)
        string currentText = string.Join("\n", translatedLines);

        // 2. ตั้งค่ารูปภาพและชื่อ
        SetupSenderNow(currentDialogueLines, currentLineIndex);

        // 3. เพิ่มลงใน Log ทันทีที่แสดงผล
        string currentName = currentDialogueLines[currentLineIndex].character.characterName;
        AddLogEntry(currentName, currentText);

        // 4. เริ่มให้ค่อยๆ พิมพ์ข้อความ
        targetText = currentText;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeWriterEffect(targetText));
    }

    IEnumerator TypeWriterEffect(string textToType)
    {
        isTyping = true;
        dialogueText.text = ""; // เคลียร์ข้อความเก่า

        foreach (char letter in textToType.ToCharArray())
        {
            dialogueText.text += letter; // เพิ่มทีละตัว
            yield return new WaitForSeconds(typingSpeed); // รอเวลา
        }

        isTyping = false; // พิมพ์เสร็จแล้ว
    }

    void SetupSenderNow(List<DialogueLinesData> currentDialogueLines, int currentLineIndex)
    {
        // 1. ดึงข้อมูลตัวละครมาก่อน (ทำแค่ครั้งเดียว)
        var characterData = currentDialogueLines[currentLineIndex].character;

        // 2. ตั้งชื่อ (ทำได้เลย เพราะทำเหมือนกันทั้ง if/else)
        NameDialog.text = characterData.characterName;

        // 3. ตั้งรูปตัวละคร
        if (characterData.characterName == "Sentinel")
        {
            // --- แสดง Player ---
            PlayerImage.sprite = characterData.characterImage;
            PlayerImage.gameObject.SetActive(true);
            // --- ซ่อน Oracle ---
            OracleImage.gameObject.SetActive(false);
        }
        else
        {
            OracleImage.gameObject.SetActive(true);
            // --- ซ่อน Player ---
            PlayerImage.gameObject.SetActive(false);
        }
    }

    void AddLogEntry(string name, string text)
    {
        if (layoutLogPrefab == null || logContainer == null) return;

        // สร้าง Prefab ใหม่ใส่ใน Container
        GameObject logObj = Instantiate(layoutLogPrefab, logContainer);

        // ค้นหา Text Components แล้วใส่ข้อความ
        TextMeshProUGUI txtName = logObj.transform.Find("NameDialog").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI txtMsg = logObj.transform.Find("DialogText").GetComponent<TextMeshProUGUI>();

        if (txtName != null) txtName.text = name;
        if (txtMsg != null) txtMsg.text = text;

        // บังคับให้ Scroll ลงล่างสุด (Optional)
        Canvas.ForceUpdateCanvases();
        // logContainer.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 0f; 
    }

    public void ToggleLogWindow()
    {
        bool isActive = logWindowPanel.activeSelf;
        AudioManager.Instance.PlaySFX("ButtonClick");
        logWindowPanel.SetActive(!isActive);
    }

    // (ฟังก์ชันนี้จะถูกเรียกโดย nextButton) "คลิกไปเรื่อยๆ"
    void OnNextLineClicked()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        // กรณีที่ 1: กำลังพิมพ์อยู่ -> ให้หยุดพิมพ์แล้วแสดงข้อความทั้งหมดทันที
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogueText.text = targetText; // โชว์ข้อความเต็ม
            isTyping = false;
        }
        // กรณีที่ 2: พิมพ์เสร็จแล้ว -> ไปประโยคถัดไป
        else
        {
            currentLineIndex++;
            if (currentLineIndex < currentDialogueLines.Count)
            {
                UpdateDialogueUI();
            }
            else
            {
                currentEventIndex++;
                LoadCurrentEvent();
            }
        }
    }

    /// <summary>
    /// เริ่มฉาก "Quiz"
    /// </summary>
    void StartQuizEvent(ChapterEventsData eventData)
    {
        dialoguePanel.SetActive(false); // ปิดหน้าต่างบทพูด
        quizBackgroundPanel.SetActive(true);
        // สั่งให้ Quiz Controller เริ่มทำงาน
        quizController.StartQuiz(eventData.quizReference);
    }

    /// <summary>
    /// (ฟังก์ชันนี้จะถูกเรียกโดย "โทรโข่ง" 📢 จาก QuizController)
    /// </summary>
    void OnQuizFinished()
    {
        Debug.Log("Quiz จบแล้ว! กำลังไป Event ถัดไป");
        // 2. เลื่อนไป Event ถัดไป
        currentEventIndex++;
        // 3. โหลด Event ถัดไป (ซึ่งอาจจะเป็น Dialogue ฉากจบบท)
        LoadCurrentEvent();
    }

    void UpdateDailyQuest()
    {
        // 1. แปลง Story ID เป็น Key ที่ Manager เข้าใจ
        var Storyid = GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId;
        string questKey = "";

        // ตรงนี้อาจจะยังต้อง if/switch บ้าง แต่สั้นกว่าเดิมเยอะ
        // หรือถ้าตั้งชื่อ Scene ให้ตรงกับ Key ก็แทบไม่ต้อง if เลย
        if (Storyid == "A01") questKey = "A01";
        else if (Storyid == "A02") questKey = "A02";
        else if (Storyid == "A03") questKey = "A03";

        // 2. ตะโกนบอก Manager ทีเดียวจบ!
        // "เฮ้! มีคนเล่น Story จบ 1 ครั้งนะ รหัสคือ A01"
        DailyQuestManager.Instance.UpdateProgress(QuestType.Story, 1, questKey);

    }


}