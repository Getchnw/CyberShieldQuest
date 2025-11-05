using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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


    // --- ตัวแปรจัดการสถานะ ---
    private int currentChapterID;
    private List<ChapterEventsData> allChapterEvents; // "เพลย์ลิสต์"
    private int currentEventIndex; 
    private List<DialogueLinesData> currentDialogueLines; 
    private int currentLineIndex; 

    void Start()
    {
        // 1. ตั้งค่าปุ่ม
        nextButton.onClick.AddListener(OnNextLineClicked);
        quizController.OnQuizCompleted += OnQuizFinished; // "เงี่ยหูฟัง" Quiz

        // ปิด ฺBackground
        dialoguePanel.SetActive(false);
        quizBackgroundPanel.SetActive(false);

        // 2. ดึงข้อมูล
        currentChapterID = GameManager.Instance.CurrentGameData.selectedStory.lastSelectedchapterId;
        allChapterEvents = GameContentDatabase.Instance.GetChapterEventsByChapterID(currentChapterID);
        
        // 3. ค้นหาว่าเล่นถึง Event ไหนแล้ว
        // PlayerChapterProgress progress = GameManager.Instance.GetChapterProgress(currentChapterID);
        // currentEventIndex = allChapterEvents.FindIndex(e => e.eventOrder > progress.last_completed_event_order);
        // if (currentEventIndex == -1) currentEventIndex = 0; // (ถ้าจบแล้ว หรือเริ่มใหม่)

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
        // ถ้ามีหลายบรรทัดในหนึ่ง DialogueLinesData
        dialogueText.text = string.Join("\n", currentDialogueLines[currentLineIndex].Dialog_Text);
        SetupSenderNow(currentDialogueLines , currentLineIndex);
    }

    void SetupSenderNow(List<DialogueLinesData> currentDialogueLines, int currentLineIndex)
{
    // 1. ดึงข้อมูลตัวละครมาก่อน (ทำแค่ครั้งเดียว)
    var characterData = currentDialogueLines[currentLineIndex].character;

    // 2. ตั้งชื่อ (ทำได้เลย เพราะทำเหมือนกันทั้ง if/else)
    NameDialog.text = characterData.characterName;

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
        // OracleImage.sprite = characterData.characterImage; // แก้ไข: เปลี่ยนที่ OracleImage
        OracleImage.gameObject.SetActive(true); 

        // --- ซ่อน Player ---
        PlayerImage.gameObject.SetActive(false); 
    }
}

    /// <summary>
    /// (ฟังก์ชันนี้จะถูกเรียกโดย nextButton) "คลิกไปเรื่อยๆ"
    /// </summary>
    void OnNextLineClicked()
    {
        currentLineIndex++; // ไปบรรทัดถัดไป

        // เช็คว่าบทพูดใน Scene นี้หมดหรือยัง
        if (currentLineIndex < currentDialogueLines.Count)
        {
            // ถ้ามีหลายบรรทัดในหนึ่ง DialogueLinesData
            dialogueText.text = string.Join("\n", currentDialogueLines[currentLineIndex].Dialog_Text);
            SetupSenderNow(currentDialogueLines , currentLineIndex);
        }
        else
        {
            // ถ้าหมดแล้ว (จบบทพูด Event นี้):

            //เลื่อนไป Event ถัดไป
            currentEventIndex++;

            //โหลด Event ถัดไป (ซึ่งอาจจะเป็น Quiz หรือ Dialogue ต่อ)
            LoadCurrentEvent();
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
}